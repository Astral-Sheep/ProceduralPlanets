using System.Linq;
using Godot;

namespace Com.Astral.Stars;

public partial class PlayerCharacter : GravitationalBody
{
	[Export] protected Camera3D camera;

	[ExportGroup("Movement")]
	[Export] protected float linearForce = 1f;
	[Export(PropertyHint.Range, "0,1,0.001")] protected float linearFriction = 0.1f;
	[Export] protected float angularForce = 360f;
	[Export(PropertyHint.Range, "0,1,0.001")] protected float angularFriction = 0.9f;
	[Export(PropertyHint.Range, "0,1,0.001")] protected float brakePower = 0.9f;

	protected Vector3 linearAcceleration = Vector3.Zero;
	protected Vector3 angularVelocity = Vector3.Zero;
	protected Vector3 angularAcceleration = Vector3.Zero;
	protected bool braking = false;

	public override void _Ready()
	{
		base._Ready();

		if (camera == null)
		{
			camera = GetChildren(true).First((Node node) => node is Camera3D) as Camera3D;

			if (camera == null)
			{
				GD.PrintErr($"{nameof(PlayerCharacter)} is missing a camera. Looking around will generated errors");
			}
		}
	}

	public override void _PhysicsProcess(double pDelta)
	{
		float lDelta = (float)pDelta;

		// Linear forces
		velocity += linearAcceleration * lDelta;
		velocity *= Mathf.Pow(1f - linearFriction, lDelta);

		if (braking)
		{
			velocity *= Mathf.Pow(1f - brakePower, lDelta);
		}

		// Angular forces
		angularVelocity += angularAcceleration * lDelta;
		angularVelocity *= Mathf.Pow(1f - angularFriction, lDelta);

		if (braking)
		{
			angularVelocity *= Mathf.Pow(1f - brakePower, lDelta);
		}

		Quaternion lYawOffset = new Quaternion(
			this.GetUpVector(),
			angularVelocity.Z * lDelta
		);
		Quaternion lPitchOffset = new Quaternion(
			this.GetRightVector(),
			angularVelocity.Y * lDelta
		);
		Quaternion lRollOffset = new Quaternion(
			this.GetForwardVector(),
			angularVelocity.X * lDelta
		);

		// Physics application
		Quaternion = lRollOffset * lPitchOffset * lYawOffset * Quaternion;
		// GlobalPosition += velocity * lDelta;
		base._PhysicsProcess(pDelta);
	}

	public void Move(Vector3 pInput)
	{
		if (Mathf.IsEqualApprox(pInput.LengthSquared(), 0f))
		{
			linearAcceleration = Vector3.Zero;
		}
		else
		{
			linearAcceleration = (Quaternion * pInput) * linearForce;
		}
	}

	public void LookAround(Vector3 pInput)
	{
		angularAcceleration = new Vector3(
			Mathf.DegToRad(pInput.X),
			Mathf.DegToRad(-pInput.Y),
			Mathf.DegToRad(-pInput.Z)
		) * angularForce;
	}

	public void Brake(bool pBraking)
	{
		braking = pBraking;
	}
}