using UnityEngine;

public class GUI_MultiplayerPicker : GUI_Base
{
	public InputObject firstButton;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
		if (Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters)
		{
			Camera.main.GetComponent<MenuCamera>().LookAtCharacter((CharacterSelect.Characters)Singleton<PlayerPrefsManager>.SP.GetInt("LASTPLAYEDCHARACTER", 1));
			Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters = false;
		}
	}

	private void Update()
	{
	}

	private void NextWindow()
	{
		firstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.ClickCurrentButton();
		AudioController.Play("ButtonForwards");
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		AudioController.Play("ButtonBackwards");
	}

	public void Button_OnlineMP()
	{
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Multiplayer);
		Singleton<GamestateManager>.SP.SetState(typeof(State_Multiplayer));
	}

	public void Button_LocalMP()
	{
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.LocalMultiplayer);
		Singleton<GamestateManager>.SP.SetState(typeof(State_LMP));
	}
}
