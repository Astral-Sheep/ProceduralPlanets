using Godot;

namespace Com.Astral.Stars;

[Tool, GlobalClass, Icon("res://Resources/Textures/Editor/planet_node.png")]
public partial class ProceduralPlanet : GravitationalBody
{
	public float Volume { get; private set; }

	[Export]
	private bool Generate
	{
		get => false;
		set
		{
			_Generate();
		}
	}

	[Export] protected PlanetData data;

	[Export(PropertyHint.ResourceType, $"{nameof(StandardMaterial3D)},{nameof(ShaderMaterial)},ORMMaterial3D")]
	public Material Material
	{
		get => _material;
		set
		{
			_material = value;
			mesh.MaterialOverride = _material;
		}
	}
	private Material _material;

	protected MeshInstance3D mesh = new MeshInstance3D();
	protected CollisionShape3D collider = new CollisionShape3D();

	public ProceduralPlanet() : base()
	{
		GravityScale = 0f;
		InputRayPickable = false;
	}

	public override void _EnterTree()
	{
		base._EnterTree();

		mesh ??= new MeshInstance3D();
		collider ??= new CollisionShape3D();

		if (collider.GetParent() == null)
		{
			AddChild(collider);
			collider.Owner = this;
		}

		if (mesh.GetParent() == null)
		{
			AddChild(mesh);
			mesh.Owner = this;
			_Generate();
		}
	}

	public override void _Ready()
	{
		base._Ready();

		if (mesh.Mesh == null)
		{
			_Generate();
		}
	}

	protected void _Generate()
	{
		if (data == null)
			return;

		data.RecomputeMeshData();

		ArrayMesh lMesh = data.GenerateMesh();
		mesh.Mesh = lMesh;

		collider.Shape = new ConvexPolygonShape3D() {
			Points = lMesh.SurfaceGetArrays(0)[0].AsVector3Array(),
		};

		Mass = data.Mass;
	}
}