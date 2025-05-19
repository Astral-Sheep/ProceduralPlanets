using Godot;

namespace Stars.Scripts.Debug;

public partial class Profiler : Label
{
	public override void _Process(double delta)
	{
		base._Process(delta);
		Text = $"FPS: {Engine.GetFramesPerSecond()}\n" +
		       $"ms: {1.0 / Engine.GetFramesPerSecond():F4}";
	}
}