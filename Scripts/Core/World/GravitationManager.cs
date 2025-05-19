using System.Collections.Generic;
using Godot;

namespace Com.Astral.Stars;

public partial class GravitationManager : Node
{
	public static GravitationManager Singleton { get; private set; }

	[Export] protected float gravityConstant = 9.81f;
	protected List<GravitationalBody> bodies = new List<GravitationalBody>();

	public GravitationManager() : base()
	{
		if (Singleton != null)
		{
			GD.PushWarning($"{nameof(GravitationManager)} instance already exists, destroying the last added");
			QueueFree();
			return;
		}

		Singleton = this;
	}

	public override void _Ready()
	{
		base._Ready();
		gravityConstant = ProjectSettings.GetSetting("physics/3d/default_gravity", 9.81f).As<float>();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (bodies.Count < 2)
			return;

		for (int i = 0; i < bodies.Count; i++)
		{
			for (int j = 0; j < bodies.Count; j++)
			{
				if (i == j)
					continue;

				bodies[i].velocity += gravityConstant * (bodies[j].Mass / Mathf.Pow((bodies[j].GlobalPosition - bodies[i].GlobalPosition).LengthSquared(), 1.5f)) * (bodies[j].GlobalPosition - bodies[i].GlobalPosition);
			}
		}
	}

	protected override void Dispose(bool pDisposing)
	{
		base.Dispose(pDisposing);

		if (!pDisposing)
			return;

		if (Singleton == this)
		{
			Singleton = null;
		}
	}

	public void Register(GravitationalBody pBody)
	{
		if (bodies.Contains(pBody))
			return;

		bodies.Add(pBody);
	}

	public void Unregister(GravitationalBody pBody)
	{
		bodies.Remove(pBody);
	}
}