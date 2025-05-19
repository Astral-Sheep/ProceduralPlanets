using Godot;

namespace Com.Astral.Stars;

public partial class DebugControllerJoystick : DebugControllerInput<Vector2>
{
	[Export] protected Control stick;
	[Export] protected float maxDistance = 0f;

	protected Vector2 limit;

	public override void _Ready()
	{
		base._Ready();
		stick.Position = Vector2.Zero;
		limit = maxDistance > 0f ? new Vector2(Mathf.Min(Size.X, maxDistance), Mathf.Min(Size.Y, maxDistance)) : Size;
	}

	public override void SetInput(Vector2 pInput)
	{
		stick.Position = pInput * limit * .5f;
	}

	public void SetInputHorizontal(float pInput)
	{
		stick.Position = new Vector2(pInput * limit.X * .5f, stick.Position.Y);
	}

	public void SetInputVertical(float pInput)
	{
		stick.Position = new Vector2(stick.Position.X, pInput * limit.Y * .5f);
	}
}