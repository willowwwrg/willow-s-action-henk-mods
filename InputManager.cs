using System.Collections;
using Rewired;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
	public enum InputType
	{
		Keyboard,
		Joystick
	}

	public enum JoystickDirection
	{
		Left,
		Right,
		Up,
		Down,
		None,
		LeftTrigger,
		RightTrigger
	}

	private InputObject currentObject;

	public bool inputEnabled = true;

	private float joystickThreshold = 0.6f;

	private InputType inputType;

	private InputAxisState[] axisState = new InputAxisState[4];

	private InputAxisState[] prevAxisState = new InputAxisState[4];

	private bool newAxisUpdate = true;

	private float holdTime;

	private JoystickDirection holdDirection = JoystickDirection.None;

	private float holdTimeBeforeScrolling = 0.4f;

	private float holdTimeDuringScrolling = 0.125f;

	private JoystickDirection simulateInputAction = JoystickDirection.None;

	private bool blockNextInputFrame;

	public InputType GetInputType()
	{
		return inputType;
	}

	public void DisableInputTillEndOfFrame()
	{
		blockNextInputFrame = true;
	}

	private IEnumerator FlickerInputRoutine()
	{
		inputEnabled = false;
		yield return new WaitForEndOfFrame();
		inputEnabled = true;
	}

	private void Awake()
	{
		if (Input.GetJoystickNames().Length != 0)
		{
			inputType = InputType.Joystick;
		}
	}

	private void Update()
	{
		if (blockNextInputFrame)
		{
			simulateInputAction = JoystickDirection.None;
			blockNextInputFrame = false;
		}
		else
		{
			if (!inputEnabled || Singleton<PermaGUI>.SP.confirmationRequestUp || (Singleton<GamestateManager>.SP.CurrentState != null && !Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled))
			{
				return;
			}
			if (CheckAction(InputAction.Confirm))
			{
				GoToNextWindow();
			}
			if (CheckAction(InputAction.Cancel))
			{
				GoToPrevWindow();
			}
			if (currentObject == null)
			{
				return;
			}
			if (!Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().useCustomHorizontalInput)
			{
				if (CheckAction(InputAction.Right))
				{
					Select(currentObject.selectOnRight);
				}
				if (CheckAction(InputAction.Left))
				{
					Select(currentObject.selectOnLeft);
				}
			}
			if (CheckAction(InputAction.Up))
			{
				Select(currentObject.selectOnUp);
			}
			if (CheckAction(InputAction.Down))
			{
				Select(currentObject.selectOnDown);
			}
			simulateInputAction = JoystickDirection.None;
			for (int i = 0; i < 4; i++)
			{
				JoystickDirection joystickDirection = (JoystickDirection)i;
				KeyCode key = KeyCode.UpArrow;
				if (joystickDirection == JoystickDirection.Down)
				{
					key = KeyCode.DownArrow;
				}
				if (joystickDirection == JoystickDirection.Left)
				{
					key = KeyCode.LeftArrow;
				}
				if (joystickDirection == JoystickDirection.Right)
				{
					key = KeyCode.RightArrow;
				}
				bool num = GetJoystickAxis(joystickDirection) || Input.GetKeyDown(key);
				bool flag = GetJoystickAxis(joystickDirection, onlyDown: false) || Input.GetKey(key);
				if (num)
				{
					holdDirection = joystickDirection;
					holdTime = 0f;
				}
				if (!flag || holdDirection != joystickDirection)
				{
					continue;
				}
				float num2 = holdTime;
				holdTime += Time.deltaTime;
				if (holdTime >= holdTimeBeforeScrolling)
				{
					int num3 = Mathf.FloorToInt((num2 - holdTimeBeforeScrolling) / holdTimeDuringScrolling);
					if (Mathf.FloorToInt((holdTime - holdTimeBeforeScrolling) / holdTimeDuringScrolling) > num3)
					{
						simulateInputAction = holdDirection;
					}
				}
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < axisState.Length; i++)
		{
			prevAxisState[i] = axisState[i];
		}
		newAxisUpdate = true;
	}

	public bool CheckAction(InputAction action, bool forceThroughDisabledInput = false)
	{
		if (!inputEnabled && !forceThroughDisabledInput)
		{
			return false;
		}
		if (blockNextInputFrame)
		{
			return false;
		}
		UpdateInputAxes();
		bool flag = CheckActionJoystick(action);
		if (CheckActionKeyboard(action) || flag)
		{
			return true;
		}
		return false;
	}

	public bool CheckActionKeyboard(InputAction action)
	{
		if (Singleton<ChatBox>.SP.IsChatBoxOpen())
		{
			return false;
		}
		switch (action)
		{
		case InputAction.LevelSelectLeaderboard:
			if (Input.GetKeyDown(KeyCode.L))
			{
				return true;
			}
			break;
		case InputAction.Confirm:
			if (Input.GetKeyDown(KeyCode.Return))
			{
				return true;
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				return true;
			}
			break;
		case InputAction.Cancel:
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				return true;
			}
			break;
		case InputAction.Left:
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				return false;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				return true;
			}
			if (simulateInputAction == JoystickDirection.Left)
			{
				return true;
			}
			break;
		case InputAction.Right:
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				return false;
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				return true;
			}
			if (simulateInputAction == JoystickDirection.Right)
			{
				return true;
			}
			break;
		case InputAction.Up:
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				return false;
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				return true;
			}
			if (simulateInputAction == JoystickDirection.Up)
			{
				return true;
			}
			break;
		case InputAction.Down:
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				return false;
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				return true;
			}
			if (simulateInputAction == JoystickDirection.Down)
			{
				return true;
			}
			break;
		case InputAction.Start:
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				return true;
			}
			break;
		case InputAction.Retry:
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				return true;
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				return true;
			}
			break;
		case InputAction.ResetCheckpoint:
		case InputAction.StartLMP:
			if (Input.GetKeyDown(KeyCode.Return))
			{
				return true;
			}
			break;
		case InputAction.Taunt:
			if (Input.GetKeyDown(KeyCode.H))
			{
				return true;
			}
			break;
		case InputAction.ToggleInbox:
			return Input.GetKeyDown(KeyCode.M);
		case InputAction.SwitchCharacter:
			return Input.GetKeyDown(KeyCode.H);
		case InputAction.NextCharacter:
			return Input.GetKeyDown(KeyCode.RightArrow);
		case InputAction.PreviousCharacter:
			return Input.GetKeyDown(KeyCode.LeftArrow);
		case InputAction.NextSkin:
			return Input.GetKeyDown(KeyCode.DownArrow);
		case InputAction.PreviousSkin:
			return Input.GetKeyDown(KeyCode.UpArrow);
		case InputAction.ToggleChallengeCheckbox:
			return Input.GetKeyDown(KeyCode.Z);
		case InputAction.SendChallenges:
		case InputAction.CreateServer:
			return Input.GetKeyDown(KeyCode.C);
		case InputAction.SwitchGhost:
			return Input.GetKeyDown(KeyCode.G);
		case InputAction.VoteLevelUp:
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (!Input.GetKey(KeyCode.LeftShift))
				{
					return Input.GetKey(KeyCode.RightShift);
				}
				return true;
			}
			return false;
		case InputAction.VoteLevelDown:
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (!Input.GetKey(KeyCode.LeftShift))
				{
					return Input.GetKey(KeyCode.RightShift);
				}
				return true;
			}
			return false;
		case InputAction.BackToMainMenu:
			return Input.GetKeyDown(KeyCode.Escape);
		case InputAction.NextLeaderboard:
			return CheckAction(InputAction.Right);
		case InputAction.PrevLeaderboard:
			return CheckAction(InputAction.Left);
		case InputAction.InstantAction:
			return Input.GetKeyDown(KeyCode.I);
		case InputAction.RenameLevel:
		case InputAction.RefreshRoomList:
		case InputAction.Randomize:
			return Input.GetKeyDown(KeyCode.R);
		case InputAction.DeleteInboxMessage:
		case InputAction.StartDailyChallenge:
		case InputAction.SelectAllMutators:
			return Input.GetKeyDown(KeyCode.D);
		case InputAction.ExtraLevels:
			return Input.GetKeyDown(KeyCode.E);
		case InputAction.TuneInOnTwitch:
			return Input.GetKeyDown(KeyCode.T);
		}
		return false;
	}

	public bool CheckActionJoystick(InputAction action)
	{
		bool flag = false;
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			switch (action)
			{
			case InputAction.Left:
				flag = GetJoystickAxis(JoystickDirection.Left) || simulateInputAction == JoystickDirection.Left;
				break;
			case InputAction.Right:
				flag = GetJoystickAxis(JoystickDirection.Right) || simulateInputAction == JoystickDirection.Right;
				break;
			case InputAction.Up:
				flag = GetJoystickAxis(JoystickDirection.Up) || simulateInputAction == JoystickDirection.Up;
				break;
			case InputAction.Down:
				flag = GetJoystickAxis(JoystickDirection.Down) || simulateInputAction == JoystickDirection.Down;
				break;
			case InputAction.Confirm:
				flag = GetJoystickButtonDown(InputButton.A, i);
				break;
			case InputAction.Cancel:
			case InputAction.Retry:
				flag = GetJoystickButtonDown(InputButton.B, i);
				break;
			case InputAction.Start:
			case InputAction.LevelSelectLeaderboard:
			case InputAction.BackToMainMenu:
			case InputAction.StartLMP:
			case InputAction.TuneInOnTwitch:
				flag = GetJoystickButtonDown(InputButton.Start, i);
				break;
			case InputAction.NextCharacter:
				flag = GetJoystickButtonDown(InputButton.RightTrigger, i) || CheckAction(InputAction.Right);
				break;
			case InputAction.PreviousCharacter:
				flag = GetJoystickButtonDown(InputButton.LeftTrigger, i) || CheckAction(InputAction.Left);
				break;
			case InputAction.Taunt:
			case InputAction.NextSkin:
				flag = GetJoystickButtonDown(InputButton.RightBumper, i);
				break;
			case InputAction.PreviousSkin:
				flag = GetJoystickButtonDown(InputButton.LeftBumper, i);
				break;
			case InputAction.ToggleInbox:
			case InputAction.RenameLevel:
			case InputAction.ToggleChallengeCheckbox:
			case InputAction.VoteLevelDown:
			case InputAction.InstantAction:
			case InputAction.Randomize:
				flag = GetJoystickButtonDown(InputButton.X, i);
				break;
			case InputAction.ResetCheckpoint:
			case InputAction.DeleteInboxMessage:
			case InputAction.SwitchCharacter:
			case InputAction.SendChallenges:
			case InputAction.VoteLevelUp:
			case InputAction.CreateServer:
			case InputAction.SelectAllMutators:
				flag = GetJoystickButtonDown(InputButton.Y, i);
				break;
			case InputAction.NextLeaderboard:
				flag = GetJoystickButtonDown(InputButton.RightBumper, i) || CheckAction(InputAction.Right);
				break;
			case InputAction.PrevLeaderboard:
				flag = GetJoystickButtonDown(InputButton.LeftBumper, i) || CheckAction(InputAction.Left);
				break;
			case InputAction.Select:
			case InputAction.SwitchGhost:
			case InputAction.StartDailyChallenge:
			case InputAction.ExtraLevels:
				flag = GetJoystickButtonDown(InputButton.Back, i);
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckActionContinuous(InputAction action, int playerNum = 0)
	{
		bool flag = false;
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			switch (action)
			{
			case InputAction.Ability:
				flag = ReInput.players.GetPlayer(i).GetButton("buttonX");
				break;
			case InputAction.Jump:
				flag = ReInput.players.GetPlayer(i).GetButton("buttonA");
				break;
			}
			if (flag)
			{
				break;
			}
		}
		if (CheckActionContinuousKeyboard(action) || flag)
		{
			return true;
		}
		return false;
	}

	public bool CheckActionContinuousKeyboard(InputAction action)
	{
		switch (action)
		{
		case InputAction.Ability:
			if (Input.GetKey(KeyCode.X))
			{
				return true;
			}
			break;
		case InputAction.Jump:
			if (Input.GetKey(KeyCode.Space))
			{
				return true;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				return true;
			}
			break;
		case InputAction.Slide:
			if (Input.GetKey(KeyCode.DownArrow))
			{
				return true;
			}
			break;
		case InputAction.ShowMultiplayerLeaderboard:
			if (Input.GetKey(KeyCode.Tab))
			{
				return true;
			}
			break;
		}
		return false;
	}

	public float CheckTriggerContinuous(InputAction action)
	{
		float num = 0f;
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			num = ReInput.players.GetPlayer(i).GetAxisRaw("triggerShoulderL");
			if (num != 0f)
			{
				return num;
			}
		}
		return num;
	}

	public void GoToNextWindow()
	{
		if (Singleton<PermaGUI>.SP.inbox.activeInHierarchy)
		{
			ClickCurrentButton();
		}
		else
		{
			Singleton<GUIManager>.SP.NextWindow();
		}
	}

	public void GoToPrevWindow()
	{
		if (Singleton<PermaGUI>.SP.inbox.activeInHierarchy)
		{
			Singleton<PermaGUI>.SP.ToggleInbox(toggle: false);
		}
		else
		{
			Singleton<GUIManager>.SP.PrevWindow();
		}
	}

	public void ClickCurrentButton()
	{
		currentObject.ClickMe();
	}

	public InputObject GetCurrentButton()
	{
		return currentObject;
	}

	private void OnGUI()
	{
		if (ReInput.controllers.GetLastActiveController() == null)
		{
			inputType = InputType.Keyboard;
			return;
		}
		switch (inputType)
		{
		case InputType.Keyboard:
			if (ReInput.controllers.GetLastActiveController().type == ControllerType.Joystick)
			{
				inputType = InputType.Joystick;
			}
			break;
		case InputType.Joystick:
			if (ReInput.controllers.GetLastActiveController().type == ControllerType.Keyboard)
			{
				inputType = InputType.Keyboard;
			}
			break;
		}
	}

	public void Select(InputObject obj, bool delayedTillEndOfFrame = false, bool playSound = true, bool isLevelSelect = false)
	{
		if (obj == null || !Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled || !obj.gameObject.activeInHierarchy)
		{
			return;
		}
		if (delayedTillEndOfFrame && !isLevelSelect)
		{
			StartCoroutine(DelayedSelect(obj, playSound));
			return;
		}
		if (currentObject != null)
		{
			UICamera.Notify(currentObject.gameObject, "OnHover", false);
			currentObject.gameObject.SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
		}
		currentObject = obj;
		currentObject.gameObject.SendMessage("Select", SendMessageOptions.DontRequireReceiver);
		UICamera.Notify(currentObject.gameObject, "OnHover", true);
		if (playSound)
		{
			AudioController.Play("ui_main_hover");
		}
		if (Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MessageOnSelect)
		{
			if (delayedTillEndOfFrame)
			{
				currentObject.gameObject.SendMessage("OnSelectItem", true, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				currentObject.gameObject.SendMessage("OnSelectItem", false, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void Deselect()
	{
		if (currentObject != null)
		{
			UICamera.Notify(currentObject.gameObject, "OnHover", false);
			currentObject.gameObject.SendMessage("Deselect", SendMessageOptions.DontRequireReceiver);
			currentObject = null;
		}
	}

	public void UpdateInputAxes()
	{
		for (int i = 0; i < 4; i++)
		{
			axisState[i] = new InputAxisState();
			axisState[i].horizontal = ReInput.players.GetPlayer(i).GetAxisRaw("triggerHorizontal");
			axisState[i].vertical = ReInput.players.GetPlayer(i).GetAxisRaw("triggerVertical");
			if (ReInput.players.GetPlayer(i).GetButton("dpadLeft"))
			{
				axisState[i].dpadHorizontal = -1f;
			}
			if (ReInput.players.GetPlayer(i).GetButton("dpadRight"))
			{
				axisState[i].dpadHorizontal = 1f;
			}
			if (ReInput.players.GetPlayer(i).GetButton("dpadUp"))
			{
				axisState[i].dpadVertical = 1f;
			}
			if (ReInput.players.GetPlayer(i).GetButton("dpadDown"))
			{
				axisState[i].dpadVertical = -1f;
			}
			axisState[i].leftTrigger = ReInput.players.GetPlayer(i).GetAxisRaw("triggerShoulderL");
			axisState[i].rightTrigger = ReInput.players.GetPlayer(i).GetAxisRaw("triggerShoulderR");
		}
		newAxisUpdate = false;
	}

	public bool GetJoystickAxis(JoystickDirection JoyDir, bool onlyDown = true)
	{
		if (newAxisUpdate)
		{
			UpdateInputAxes();
		}
		bool flag = false;
		for (int i = 0; i < 4; i++)
		{
			switch (JoyDir)
			{
			case JoystickDirection.Right:
				if (onlyDown)
				{
					if (axisState[i].horizontal > joystickThreshold && prevAxisState[i].horizontal <= joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadHorizontal > joystickThreshold && prevAxisState[i].dpadHorizontal <= joystickThreshold)
					{
						flag = true;
					}
				}
				else
				{
					if (axisState[i].horizontal > joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadHorizontal > joystickThreshold)
					{
						flag = true;
					}
				}
				break;
			case JoystickDirection.Left:
				if (onlyDown)
				{
					if (axisState[i].horizontal < 0f - joystickThreshold && prevAxisState[i].horizontal >= 0f - joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadHorizontal < 0f - joystickThreshold && prevAxisState[i].dpadHorizontal >= 0f - joystickThreshold)
					{
						flag = true;
					}
				}
				else
				{
					if (axisState[i].horizontal < 0f - joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadHorizontal < 0f - joystickThreshold)
					{
						flag = true;
					}
				}
				break;
			case JoystickDirection.Up:
				if (onlyDown)
				{
					if (axisState[i].vertical > joystickThreshold && prevAxisState[i].vertical <= joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadVertical > joystickThreshold && prevAxisState[i].dpadVertical <= joystickThreshold)
					{
						flag = true;
					}
				}
				else
				{
					if (axisState[i].vertical > joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadVertical > joystickThreshold)
					{
						flag = true;
					}
				}
				break;
			case JoystickDirection.Down:
				if (onlyDown)
				{
					if (axisState[i].vertical < 0f - joystickThreshold && prevAxisState[i].vertical >= 0f - joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadVertical < 0f - joystickThreshold && prevAxisState[i].dpadVertical >= 0f - joystickThreshold)
					{
						flag = true;
					}
				}
				else
				{
					if (axisState[i].vertical < 0f - joystickThreshold)
					{
						flag = true;
					}
					if (axisState[i].dpadVertical < 0f - joystickThreshold)
					{
						flag = true;
					}
				}
				break;
			case JoystickDirection.LeftTrigger:
				if (onlyDown)
				{
					if (axisState[i].leftTrigger > joystickThreshold && prevAxisState[i].leftTrigger <= joystickThreshold)
					{
						flag = true;
					}
				}
				else if (axisState[i].leftTrigger > joystickThreshold)
				{
					flag = true;
				}
				break;
			case JoystickDirection.RightTrigger:
				if (onlyDown)
				{
					if (axisState[i].rightTrigger > joystickThreshold && prevAxisState[i].rightTrigger <= joystickThreshold)
					{
						flag = true;
					}
				}
				else if (axisState[i].rightTrigger > joystickThreshold)
				{
					flag = true;
				}
				break;
			default:
				Debug.LogError("Invalid joystick direction.");
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator DelayedSelect(InputObject obj, bool playsound)
	{
		yield return new WaitForEndOfFrame();
		Select(obj, delayedTillEndOfFrame: false, playsound);
	}

	public bool GetJoystickButtonDown(InputButton button, int playerNum = 0)
	{
		return button switch
		{
			InputButton.A => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonA"), 
			InputButton.B => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonB"), 
			InputButton.X => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonX"), 
			InputButton.Y => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonY"), 
			InputButton.Start => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonStart"), 
			InputButton.Back => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonSelect"), 
			InputButton.RightBumper => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonShoulderR"), 
			InputButton.LeftBumper => ReInput.players.GetPlayer(playerNum).GetButtonDown("buttonShoulderL"), 
			InputButton.RightTrigger => ReInput.players.GetPlayer(playerNum).GetButtonDown("triggerShoulderR"), 
			InputButton.LeftTrigger => ReInput.players.GetPlayer(playerNum).GetButtonDown("triggerShoulderL"), 
			_ => false, 
		};
	}

	public float GetJoystickHorizontalTriggerClamped()
	{
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			num += Mathf.Clamp(ReInput.players.GetPlayer(i).GetAxisRaw("triggerHorizontal") * 2.5f, -1f, 1f);
		}
		if (num == 0f)
		{
			for (int j = 0; j < 4; j++)
			{
				if (ReInput.players.GetPlayer(j).GetButton("dpadLeft"))
				{
					num = -1f;
				}
				if (ReInput.players.GetPlayer(j).GetButton("dpadRight"))
				{
					num = 1f;
				}
			}
		}
		return num;
	}
}
