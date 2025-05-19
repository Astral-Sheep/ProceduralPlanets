using Godot;

namespace Com.Astral.Stars;

public partial class DebugController : Node
{
	// Buttons
	[Export] protected DebugControllerButton buttonDown;
	[Export] protected DebugControllerButton buttonRight;
	[Export] protected DebugControllerButton buttonLeft;
	[Export] protected DebugControllerButton buttonUp;
	[Export] protected DebugControllerButton buttonLeftStick;
	[Export] protected DebugControllerButton buttonRightStick;
	[Export] protected DebugControllerButton buttonLeftShoulder;
	[Export] protected DebugControllerButton buttonRightShoulder;

	// Axis
	[Export] protected DebugControllerJoystick leftStick;
	[Export] protected DebugControllerJoystick rightStick;
	[Export] protected DebugControllerTrigger leftTrigger;
	[Export] protected DebugControllerTrigger rightTrigger;

	public override void _Input(InputEvent pEvent)
	{
		base._Input(pEvent);

		if (pEvent is InputEventJoypadButton lEventJoypadButton)
		{
			switch (lEventJoypadButton.ButtonIndex)
			{
				case JoyButton.A:
					buttonDown.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.B:
					buttonRight.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.X:
					buttonLeft.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.Y:
					buttonUp.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.LeftStick:
					buttonLeftStick.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.RightStick:
					buttonRightStick.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.LeftShoulder:
					buttonLeftShoulder.SetInput(lEventJoypadButton.Pressed);
					break;
				case JoyButton.RightShoulder:
					buttonRightShoulder.SetInput(lEventJoypadButton.Pressed);
					break;
				default:
					break;
			}
		}
		else if (pEvent is InputEventJoypadMotion lEventJoypadMotion)
		{
			switch (lEventJoypadMotion.Axis)
			{
				case JoyAxis.LeftX:
					leftStick.SetInputHorizontal(lEventJoypadMotion.AxisValue);
					break;
				case JoyAxis.LeftY:
					leftStick.SetInputVertical(lEventJoypadMotion.AxisValue);
					break;
				case JoyAxis.RightX:
					rightStick.SetInputHorizontal(lEventJoypadMotion.AxisValue);
					break;
				case JoyAxis.RightY:
					rightStick.SetInputVertical(lEventJoypadMotion.AxisValue);
					break;
				case JoyAxis.TriggerLeft:
					leftTrigger.SetInput(lEventJoypadMotion.AxisValue);
					break;
				case JoyAxis.TriggerRight:
					rightTrigger.SetInput(lEventJoypadMotion.AxisValue);
					break;
				default:
					break;
			}
		}
	}
}