using Godot;

namespace Com.Astral.Stars;

public abstract partial class DebugControllerInput<T> : TextureRect
{
	public abstract void SetInput(T pInput);
}