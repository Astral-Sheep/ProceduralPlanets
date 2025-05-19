using Godot;

namespace Com.Astral.Stars;

public partial class DebugControllerTrigger : DebugControllerInput<float>
{
	[Export] protected Control mask;
	[Export] protected Control fill;

	protected Vector2 defaultMaskPos;
	protected Vector2 defaultFillPos;

	public override void _Ready()
	{
		base._Ready();
		defaultMaskPos = mask.Position;
		defaultFillPos = fill.Position;
		SetInput(0f);
	}

	public override void SetInput(float pInput)
	{
		mask.Position = new Vector2(mask.Position.X, defaultMaskPos.Y + mask.Size.Y * (1f - pInput));
		fill.Position = new Vector2(fill.Position.X, defaultFillPos.Y - (mask.Position.Y - defaultMaskPos.Y));
	}
}