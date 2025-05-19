using Godot;

namespace Com.Astral.Stars;

public partial class DebugControllerButton : DebugControllerInput<bool>
{
	[Export] protected Texture2D pressedTexture;
	[Export] protected Texture2D releasedTexture;

	public override void _Ready()
	{
		base._Ready();
		Texture = releasedTexture;
	}

	public override void SetInput(bool pInput)
	{
		Texture = pInput ? pressedTexture : releasedTexture;
	}
}