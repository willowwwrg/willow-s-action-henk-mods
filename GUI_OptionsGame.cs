using UnityEngine;

public class GUI_OptionsGame : GUI_Base
{
	public InputObject FirstButton;

	public State_OptionsGame stateObj;

	public UIToggle toggleIngameTutorial;

	public UIToggle toggleRumble;

	private void Awake()
	{
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		toggleIngameTutorial.value = PlayerPrefs.GetInt("Options_HideTutorialIngame", 0) == 1;
		toggleRumble.value = PlayerPrefs.GetInt("Options_EnableRumble", 1) == 1;
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
	}

	private void NextWindow()
	{
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_Options));
		AudioController.Play("ButtonBackwards");
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Left))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_SwitchRegion")
			{
				SetPendingRegion(forwards: false);
			}
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.Right) && Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_SwitchRegion")
		{
			SetPendingRegion(forwards: true);
		}
	}

	private void SetPendingRegion(bool forwards)
	{
		if (forwards)
		{
			Singleton<MultiManager>.SP.serverRegion++;
			if (Singleton<MultiManager>.SP.serverRegion > 5)
			{
				Singleton<MultiManager>.SP.serverRegion = 1;
			}
			PlayerPrefs.SetInt("PhotonRegion", Singleton<MultiManager>.SP.serverRegion);
		}
		else
		{
			Singleton<MultiManager>.SP.serverRegion--;
			if (Singleton<MultiManager>.SP.serverRegion < 1)
			{
				Singleton<MultiManager>.SP.serverRegion = 5;
			}
			PlayerPrefs.SetInt("PhotonRegion", Singleton<MultiManager>.SP.serverRegion);
		}
		SetRegionLabel(Singleton<MultiManager>.SP.serverRegion);
	}

	private void SetRegionLabel(int regionNum)
	{
	}

	public void StartOptions()
	{
	}

	public void Button_toggleIngameTutorial()
	{
		toggleIngameTutorial.value = !toggleIngameTutorial.value;
		stateObj.showTutorialDuringFirstBatchLevels = !toggleIngameTutorial.value;
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleIngameTutorial.value);
		PlayerPrefs.SetInt("Options_HideTutorialIngame", toggleIngameTutorial.value ? 1 : 0);
	}

	public void Button_ToggleRumble()
	{
		toggleRumble.value = !toggleRumble.value;
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleRumble.value);
		PlayerPrefs.SetInt("Options_EnableRumble", toggleRumble.value ? 1 : 0);
	}

	public void Button_ClearLocalHighscores()
	{
		Singleton<PermaGUI>.SP.RequestConfirmation("ClearLocalHighscores", base.gameObject);
	}

	public void Button_SteamNotificationSettings()
	{
		Singleton<HenkSWUserStats>.SP.OpenSteamOverlayNotifcationSettings();
	}

	public void Button_Twitch()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_OptionsTwitch, "none");
	}

	public void Button_Language()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_OptionsLanguage, "none");
	}

	public void Button_Credits()
	{
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Singleplayer);
		Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode(97);
		AudioController.Play("LevelStart");
		if (levelFromCode.isSceneLess)
		{
			Singleton<LevelBatchManager>.SP.LoadLevelSceneless(levelFromCode);
		}
		else
		{
			Singleton<LevelBatchManager>.SP.LoadLevel(levelFromCode.levelCode);
		}
	}

	public void ClearLocalHighscores(bool value)
	{
		if (!value)
		{
			return;
		}
		AudioController.Play("Toeter");
		foreach (Level campaignLevel in Singleton<LevelBatchManager>.SP.GetCampaignLevels())
		{
			campaignLevel.bestMedal = Medal.None;
			Singleton<PlayerPrefsManager>.SP.SetMedalsEarned(campaignLevel, 0);
		}
		Singleton<LevelBatchManager>.SP.ResetUnlocks();
		Singleton<HenkSWUserStats>.SP.WriteCloudSave(forceWrite: true);
		AudioController.Play("Horn");
	}
}
