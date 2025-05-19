using Godot;

namespace Com.Astral.Stars;

[GlobalClass]
public partial class GravitationalBody : RigidBody3D
{
	public Vector3 velocity = Vector3.Zero;

	public override void _Ready()
	{
		base._Ready();

		if (Engine.IsEditorHint())
			return;

		GravitationManager.Singleton?.Register(this);
		SetPhysicsProcess(!(Freeze && FreezeMode == FreezeModeEnum.Static));
	}

	public override void _PhysicsProcess(double pDelta)
	{
		base._PhysicsProcess(pDelta);
		MoveAndSlide((float)pDelta);
	}

	protected override void Dispose(bool pDisposing)
	{
		base.Dispose(pDisposing);

		if (!pDisposing)
			return;

		GravitationManager.Singleton?.Unregister(this);
	}

	public void MoveAndSlide(float pDelta)
	{
		if (!Mathf.IsEqualApprox(velocity.LengthSquared(), 0f))
		{
			const float SAFE_MARGIN = 1e-3f;
			KinematicCollision3D lCollision = MoveAndCollide(velocity * pDelta, safeMargin: SAFE_MARGIN);

			if (lCollision?.GetCollider() != null && lCollision.GetRemainder() != Vector3.Zero)
			{
				Vector3 lRemainder = lCollision.GetRemainder();
				Vector3 lNormal = lCollision.GetNormal();

				Vector3 lRatio = new Vector3(
					Mathf.IsEqualApprox(lNormal.X, 0f) ? 0f : lRemainder.X / lNormal.X,
					Mathf.IsEqualApprox(lNormal.Y, 0f) ? 0f : lRemainder.Y / lNormal.Y,
					Mathf.IsEqualApprox(lNormal.Z, 0f) ? 0f : lRemainder.Z / lNormal.Z
				);
				GlobalPosition += lNormal * SAFE_MARGIN;
				lRemainder -= lNormal * lRatio;
				velocity = lRemainder.Normalized() * velocity.Length();
				MoveAndCollide(lRemainder, safeMargin: SAFE_MARGIN);
			}
		}
	}
}