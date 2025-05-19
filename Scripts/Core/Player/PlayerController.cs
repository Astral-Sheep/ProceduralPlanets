using Godot;

namespace Com.Astral.Stars;

public partial class PlayerController : Node
{
	[Export] protected PlayerCharacter character;

	[ExportGroup("Input")]
	[Export] protected string brakeInput = "game_brake";
	[ExportSubgroup("MovementInput")]
	[Export] protected string forwardMovInput = "game_forward";
	[Export] protected string backwardMovInput = "game_backward";
	[Export] protected string leftMovInput = "game_left";
	[Export] protected string rightMovInput = "game_right";
	[Export] protected string upMovInput = "game_up";
	[Export] protected string downMovInput = "game_down";
	[ExportSubgroup("CameraInput")]
	[Export] protected string upCamInput = "look_up";
	[Export] protected string downCamInput = "look_down";
	[Export] protected string leftCamInput = "look_left";
	[Export] protected string rightCamInput = "look_right";
	[Export] protected string tiltLeftCamInput = "look_tilt_left";
	[Export] protected string tiltRightCamInput = "look_tilt_right";

	protected Vector3 lookInput = Vector3.Zero;
	protected Vector2 joypadInput = Vector2.Zero;

	public override void _Ready()
	{
		base._Ready();

		if (character == null)
		{
			character = GetParent<PlayerCharacter>();

			if (character == null)
			{
				GD.PrintErr($"No character for the {nameof(PlayerController)} to controller");
				return;
			}
		}

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Process(double pDelta)
	{
		ApplyInput();
		character.Brake(Input.GetActionStrength(brakeInput) > 0f);
	}

	public override void _UnhandledInput(InputEvent pEvent)
	{
		base._UnhandledInput(pEvent);

		if (pEvent is InputEventMouseMotion lEventMouseMotion)
		{
			lookInput.Y += lEventMouseMotion.Relative.Y * .5f;
			lookInput.Z += lEventMouseMotion.Relative.X * .5f;
		}
		else if (pEvent is InputEventAction lEventAction)
		{
			GD.Print($"Brake: {lEventAction.GetStrength()}");

			if (lEventAction.Action == brakeInput)
			{
				character.Brake(lEventAction.GetStrength() > 0f);
			}
		}
	}

	private void ApplyInput()
	{
		// Look input
		lookInput += new Vector3(
			Input.GetAxis(tiltLeftCamInput, tiltRightCamInput),
			Input.GetAxis(upCamInput, downCamInput),
			Input.GetAxis(leftCamInput, rightCamInput)
		);
		character.LookAround(lookInput);
		lookInput = Vector3.Zero;

		// Movement input
		character.Move(new Vector3(
			Input.GetAxis(leftMovInput, rightMovInput),
			Input.GetAxis(downMovInput, upMovInput),
			Input.GetAxis(forwardMovInput, backwardMovInput)
		));
	}
}
