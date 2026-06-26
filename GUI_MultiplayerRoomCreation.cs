using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_MultiplayerRoomCreation : GUI_Base
{
	public InputObject FirstButton;

	public UILabel labelRoomName;

	private string roomName = "My server";

	public UILabel labelRoundDuration;

	private int roundDuration = 3;

	public UILabel labelMaxPlayers;

	private int maxPlayers = 20;

	public GameObject creatingServerLabel;

	private int minRoundDuration = 1;

	private int maxRoundDuration = 10;

	private int minMaxPlayers = 2;

	private int maxMaxPlayers = 20;

	private bool inputFieldUp;

	public GUI_MultiplayerLevelRotation guiLevelRotation;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		roomName = Singleton<PlayerPrefsManager>.SP.GetString("MultiplayerSettings_ROOMNAME", "My Server");
		roundDuration = Singleton<PlayerPrefsManager>.SP.GetInt("MultiplayerSettings_ROUNDDURATION", 3);
		maxPlayers = Singleton<PlayerPrefsManager>.SP.GetInt("MultiplayerSettings_MAXPLAYERS", 15);
		FillInSettingsLabels();
	}

	private void NextWindow()
	{
		if (Singleton<MultiManager>.SP.multiplayerState != MultiplayerState.CreatingServer)
		{
			FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
			Singleton<InputManager>.SP.ClickCurrentButton();
			AudioController.Play("ButtonForwards");
		}
	}

	private void PrevWindow()
	{
		if (Singleton<MultiManager>.SP.multiplayerState != MultiplayerState.CreatingServer && !inputFieldUp)
		{
			FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Multiplayer, "none");
			AudioController.Play("ButtonBackwards");
		}
	}

	private void FillInSettingsLabels()
	{
		labelRoomName.text = roomName;
		labelRoundDuration.text = "< " + roundDuration + " minutes >";
		labelMaxPlayers.text = "< " + maxPlayers + " >";
		Singleton<PlayerPrefsManager>.SP.SetString("MultiplayerSettings_ROOMNAME", roomName);
		Singleton<PlayerPrefsManager>.SP.SetInt("MultiplayerSettings_ROUNDDURATION", roundDuration);
		Singleton<PlayerPrefsManager>.SP.SetInt("MultiplayerSettings_MAXPLAYERS", maxPlayers);
	}

	private void Update()
	{
		if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.CreatingServer)
		{
			creatingServerLabel.SetActive(value: true);
			return;
		}
		creatingServerLabel.SetActive(value: false);
		if (inputFieldUp && Singleton<InputManager>.SP.CheckAction(InputAction.Cancel, forceThroughDisabledInput: true))
		{
			ToggleInputField(state: false, string.Empty);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Left))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "2 Roundduration")
			{
				SetRoundDuration(forwards: false);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "3 Maxplayers")
			{
				SetMaxPlayers(forwards: false);
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Right))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "2 Roundduration")
			{
				SetRoundDuration();
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "3 Maxplayers")
			{
				SetMaxPlayers();
			}
		}
	}

	private void SetRoundDuration(bool forwards = true)
	{
		if (forwards)
		{
			roundDuration++;
		}
		else
		{
			roundDuration--;
		}
		if (roundDuration < minRoundDuration)
		{
			roundDuration = minRoundDuration;
		}
		if (roundDuration > maxRoundDuration)
		{
			roundDuration = maxRoundDuration;
		}
		FillInSettingsLabels();
	}

	private void SetMaxPlayers(bool forwards = true)
	{
		if (forwards)
		{
			maxPlayers++;
		}
		else
		{
			maxPlayers--;
		}
		if (maxPlayers < minMaxPlayers)
		{
			maxPlayers = minMaxPlayers;
		}
		if (maxPlayers > maxMaxPlayers)
		{
			maxPlayers = maxMaxPlayers;
		}
		FillInSettingsLabels();
	}

	private void Button_RoomName()
	{
		ToggleInputField(state: true, roomName);
	}

	private void Button_LevelRotation()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerLevelRotation, "none");
	}

	private void Button_StartServer()
	{
		guiLevelRotation.ConfirmLevelRotation();
		List<Mutator> multiplayerMutatorsFromPlayerPrefs = Singleton<MutatorManager>.SP.GetMultiplayerMutatorsFromPlayerPrefs();
		Singleton<MultiManager>.SP.CreateRoomFromGUI(roomName, roundDuration, maxPlayers, multiplayerMutatorsFromPlayerPrefs);
	}

	private void Button_PickMutators()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerMutators, "none");
	}

	private void ToggleInputField(bool state, string roomName = "")
	{
		Singleton<PermaGUI>.SP.inputField.transform.parent.gameObject.SetActive(state);
		Singleton<PermaGUI>.SP.inputFieldTitle.text = Language.Get("NAMESERVER", "MULTIPLAYER");
		if (state)
		{
			Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = false;
			Singleton<PermaGUI>.SP.inputField.value = roomName;
			StartCoroutine(GiveFocusToInputField());
			inputFieldUp = true;
		}
		else
		{
			inputFieldUp = false;
			Singleton<PermaGUI>.SP.inputField.RemoveFocus();
			Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = true;
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
		}
	}

	private IEnumerator GiveFocusToInputField()
	{
		yield return new WaitForSeconds(0.1f);
		Singleton<PermaGUI>.SP.inputField.isSelected = true;
	}

	public void ConfirmName(UILabel input)
	{
		if (inputFieldUp)
		{
			if (input.text.Length < 4)
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_ROOMNAMETOOSHORT", "PERMA"));
				return;
			}
			roomName = input.text;
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			ToggleInputField(state: false, string.Empty);
			FillInSettingsLabels();
		}
	}

	public void OnHover(GameObject button)
	{
		switch (button.name)
		{
		case "1 Roomname":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, Language.Get("BACK", "PERMA"), Language.Get("SETSERVERNAME", "MULTIPLAYER"));
			break;
		case "2 Roundduration":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: false, string.Empty, string.Empty);
			break;
		case "3 Maxplayers":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: false, string.Empty, string.Empty);
			break;
		case "4 Levelrotation":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, Language.Get("BACK", "PERMA"), Language.Get("SETLEVELROTATIONUPPER", "MULTIPLAYER"));
			break;
		case "5 Mutators":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, Language.Get("BACK", "PERMA"), "SELECT MUTATORS");
			break;
		case "6 StartServer":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, Language.Get("BACK", "PERMA"), Language.Get("STARTSERVERUPPER", "MULTIPLAYER"));
			break;
		}
	}
}
