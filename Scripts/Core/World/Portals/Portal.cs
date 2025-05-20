using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Com.Astral.Stars;

[Tool, GlobalClass]
public partial class Portal : Area3D
{
	public Transform3D ToOtherSide => otherSide == null
		? Transform3D.Identity
		: otherSide.GlobalTransform.NoScale() * GlobalTransform.Inverse().NoScale();

	[Export] protected Node3D otherSide;

	[Export]
	protected Viewport.DebugDrawEnum DebugDraw
	{
		get => subviewport?.DebugDraw ?? Viewport.DebugDrawEnum.Disabled;
		set
		{
			if (subviewport == null)
				return;

			subviewport.DebugDraw = value;
		}
	}

	[Export]
	protected Vector2 Size
	{
		get
		{
			if (renderTarget?.Mesh != null)
			{
				return new Vector2(renderTarget.Mesh.GetAabb().Size.X, renderTarget.Mesh.GetAabb().Size.Y);
			}

			if (collider?.Shape is BoxShape3D lBoxShape)
			{
				return new Vector2(lBoxShape.Size.X, lBoxShape.Size.Y);
			}

			return Vector2.Zero;
		}
		set
		{
			if (renderTarget?.Mesh is BoxMesh lBoxMesh)
			{
				lBoxMesh.Size = new Vector3(value.X, value.Y, lBoxMesh.Size.Z);
			}

			if (collider?.Shape is BoxShape3D lBoxShape)
			{
				lBoxShape.Size = new Vector3(value.X, value.Y, lBoxShape.Size.Z);
			}
		}
	}

	protected SubViewport subviewport;
	protected Camera3D renderCamera;
	protected MeshInstance3D renderTarget;
	protected CollisionShape3D collider;

	protected Godot.Collections.Dictionary<Node3D, float> interactingObjects = new Godot.Collections.Dictionary<Node3D, float>();

	public override void _EnterTree()
	{
		base._EnterTree();

		InputRayPickable = false;

		if (subviewport == null)
		{
			subviewport = new SubViewport();
			subviewport.Name = "SubViewport";
			subviewport.Size = DisplayServer.ScreenGetSize();
			subviewport.RenderTargetUpdateMode = SubViewport.UpdateMode.WhenParentVisible;
			subviewport.HandleInputLocally = false;
			subviewport.Msaa3D = (Viewport.Msaa)ProjectSettings.GetSetting("rendering/anti_aliasing/quality/msaa_3d").As<int>();
			subviewport.ScreenSpaceAA = (Viewport.ScreenSpaceAAEnum)ProjectSettings.GetSetting("rendering/anti_aliasing/quality/screen_space_aa").As<int>();
			subviewport.UseTaa = ProjectSettings.GetSetting("rendering/anti_aliasing/quality/use_taa").As<bool>();
			subviewport.UseDebanding = ProjectSettings.GetSetting("rendering/anti_aliasing/quality/use_debanding").As<bool>();
			subviewport.UseOcclusionCulling = ProjectSettings.GetSetting("rendering/occlusion_culling/use_occlusion_culling").As<bool>();
			subviewport.GuiDisableInput = true;

			AddChild(subviewport);
			subviewport.SetOwner(GetTree().EditedSceneRoot);
		}

		if (renderCamera == null)
		{
			renderCamera = new Camera3D();
			renderCamera.Name = "RenderCamera";
			renderCamera.CullMask &= ~(1u << 19);

			subviewport.AddChild(renderCamera);
			renderCamera.SetOwner(GetTree().EditedSceneRoot);
		}

		if (renderTarget == null)
		{
			renderTarget = new MeshInstance3D();
			renderTarget.Name = "RenderTarget";

			BoxMesh lMesh = new BoxMesh();
			lMesh.Size = new Vector3(2f, 2f, .05f);
			lMesh.ResourceLocalToScene = true;

			ShaderMaterial lShaderMaterial = new ShaderMaterial();
			lShaderMaterial.Shader = ResourceLoader.Load<Shader>("res://Resources/Shaders/PortalRenderer.gdshader");
			lShaderMaterial.ResourceLocalToScene = true;

			renderTarget.Mesh = lMesh;
			renderTarget.SetSurfaceOverrideMaterial(0, lShaderMaterial);
			renderTarget.Layers = 1u << 19;
			renderTarget.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
			AddChild(renderTarget);
			renderTarget.SetOwner(GetTree().EditedSceneRoot);
		}

		if (collider == null)
		{
			collider = new CollisionShape3D();
			collider.Name = "Collider";

			BoxShape3D lShape = new BoxShape3D();
			lShape.Size = new Vector3(2f, 2f, 2f);
			lShape.ResourceLocalToScene = true;

			collider.Shape = lShape;
			AddChild(collider);
		}

		SetMeta("_edit_group_", true);
	}

	public override void _Ready()
	{
		base._Ready();
		SetProcess(otherSide != null);

#if DEBUG
		SetPhysicsProcess(!Engine.IsEditorHint() && otherSide != null && interactingObjects.Count > 0);
#else
		SetPhysicsProcess(otherSide != null);
#endif //DEBUG

		if (renderTarget != null && subviewport != null)
		{
			if (
				renderTarget.GetSurfaceOverrideMaterial(0) is ShaderMaterial lMaterial
				&& lMaterial.GetShaderParameter("screen_tex").As<ViewportTexture>() == null
			)
			{
				ViewportTexture lTexture = new ViewportTexture();
				lTexture.ResourceLocalToScene = true;

				string lPath = subviewport.GetPath().ToString();

				if (lPath.LastIndexOf("/@SubViewport@") >= 0)
				{
					lPath = lPath[(lPath.LastIndexOf("/@SubViewport@") + "/@SubViewport@".Length)..];
				}

				if (lPath.IndexOf("/Main/") >= 0)
				{
					lPath = lPath[(lPath.LastIndexOf("/Main/") + "/Main/".Length)..];
				}

				lTexture.ViewportPath = lPath;
				GD.Print(lPath);
				lMaterial.SetShaderParameter("screen_tex", lTexture);
			}
		}

#if DEBUG
		if (Engine.IsEditorHint())
			return;
#endif //DEBUG

		if (otherSide != null)
		{
			AreaEntered += OnAreaEntered;
			AreaExited += OnAreaExited;
			BodyEntered += OnBodyEntered;
			BodyExited += OnBodyExited;
		}
		else
		{
			renderCamera.Current = false;
		}
	}

	public override void _Process(double pDelta)
	{
		base._Process(pDelta);

		if (otherSide == null)
		{
			GD.PrintErr("OtherSide is null. Disabling process");
			SetProcess(false);
			return;
		}

		if (renderCamera == null)
		{
			GD.PrintErr("RenderCamera is null. Can't render other side");
			return;
		}

		if (GetViewport().GetCamera3D() == null)
		{
			GD.PrintErr("No current camera found. Can't render other side");
			return;
		}

		renderCamera.Transform = ToOtherSide * GetViewport().GetCamera3D().GlobalTransform;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (otherSide == null)
		{
			GD.PrintErr("OtherSide is null. Disabling process");
			SetPhysicsProcess(false);
			return;
		}

		Transform3D lTransform = ToOtherSide;

		foreach (KeyValuePair<Node3D, float> lObject in interactingObjects)
		{
			float lDot = (lObject.Key.GlobalPosition - GlobalPosition).Dot(Transform.GetForward());

			if (Mathf.Sign(lDot) != Mathf.Sign(lObject.Value))
			{
				lObject.Key.Transform = lTransform * lObject.Key.Transform;
			}

			interactingObjects[lObject.Key] = lDot;
		}
	}

	public override Array<Dictionary> _GetPropertyList()
	{
		return new Array<Dictionary> {
			new Dictionary {
				{ "name", nameof(subviewport) },
				{ "type", Variant.From(Variant.Type.Object) },
				{ "hint", Variant.From(PropertyHint.NodeType) },
				{ "hint_string", nameof(SubViewport) },
				{ "usage", Variant.From(PropertyUsageFlags.Storage) },
			},
			new Dictionary {
				{ "name", nameof(renderCamera) },
				{ "type", Variant.From(Variant.Type.Object) },
				{ "hint", Variant.From(PropertyHint.NodeType) },
				{ "hint_string", nameof(Camera3D) },
				{ "usage", Variant.From(PropertyUsageFlags.Storage) },
			},
			new Dictionary {
				{ "name", nameof(renderTarget) },
				{ "type", Variant.From(Variant.Type.Object) },
				{ "hint", Variant.From(PropertyHint.NodeType) },
				{ "hint_string", nameof(MeshInstance3D) },
				{ "usage", Variant.From(PropertyUsageFlags.Storage) },
			},
			new Dictionary {
				{ "name", nameof(collider) },
				{ "type", Variant.From(Variant.Type.Object) },
				{ "hint", Variant.From(PropertyHint.NodeType) },
				{ "hint_string", nameof(CollisionShape3D) },
				{ "usage", Variant.From(PropertyUsageFlags.Storage) },
			},
		};
	}

	public override void _Notification(int pWhat)
	{
		base._Notification(pWhat);

		if (pWhat != NotificationWMSizeChanged || subviewport == null)
			return;

		subviewport.Size = DisplayServer.ScreenGetSize();
	}

	protected void OnAreaEntered(Node3D pArea)
	{
		interactingObjects[pArea] = (pArea.GlobalPosition - GlobalPosition).Dot(Transform.GetForward());
		SetPhysicsProcess(interactingObjects.Count > 0);

		GD.Print($"{Name} area entered: {pArea.Name}");
	}

	protected void OnAreaExited(Node3D pArea)
	{
		interactingObjects.Remove(pArea);
		SetPhysicsProcess(interactingObjects.Count > 0);
		GD.Print($"{Name} area exited: {pArea.Name}");
	}

	protected void OnBodyEntered(Node3D pBody)
	{
		interactingObjects[pBody] = (pBody.GlobalPosition - GlobalPosition).Dot(Transform.GetForward());
		SetPhysicsProcess(interactingObjects.Count > 0);
		GD.Print($"{Name} body entered: {pBody.Name}");
	}

	protected void OnBodyExited(Node3D pBody)
	{
		interactingObjects.Remove(pBody);
		SetPhysicsProcess(interactingObjects.Count > 0);
		GD.Print($"{Name} body exited: {pBody.Name}");
	}
}