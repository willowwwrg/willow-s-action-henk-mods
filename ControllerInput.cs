using Rewired;

public class ControllerInput : Singleton<ControllerInput>
{
	public bool[] prevState_button_RightTrigger = new bool[4];

	public bool[] prevState_button_LeftTrigger = new bool[4];

	public bool GetKeyDown(int controllerNum, string button)
	{
		return button switch
		{
			"A" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("buttonA"), 
			"X" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("buttonX"), 
			"B" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("buttonB"), 
			"Y" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("buttonY"), 
			"LB" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("buttonShoulderL"), 
			"RB" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("buttonShoulderR"), 
			"RT" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("triggerShoulderR"), 
			"LT" => ReInput.players.GetPlayer(controllerNum).GetButtonDown("triggerShoulderL"), 
			_ => false, 
		};
	}

	public bool GetKey(int controllerNum, string button)
	{
		return button switch
		{
			"A" => ReInput.players.GetPlayer(controllerNum).GetButton("buttonA"), 
			"X" => ReInput.players.GetPlayer(controllerNum).GetButton("buttonX"), 
			"B" => ReInput.players.GetPlayer(controllerNum).GetButton("buttonA"), 
			"Y" => ReInput.players.GetPlayer(controllerNum).GetButton("buttonY"), 
			"LB" => ReInput.players.GetPlayer(controllerNum).GetButton("buttonShoulderL"), 
			"RB" => ReInput.players.GetPlayer(controllerNum).GetButton("buttonShoulderR"), 
			"RT" => ReInput.players.GetPlayer(controllerNum).GetButton("triggerShoulderR"), 
			"LT" => ReInput.players.GetPlayer(controllerNum).GetButton("triggerShoulderL"), 
			_ => false, 
		};
	}
}
