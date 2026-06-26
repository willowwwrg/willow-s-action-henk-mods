using System.Collections;
using Henk;
using UnityEngine;

public class GUI_InGame : GUI_Base
{
	public GameObject[] MenuObjects;

	public State_InGame stateObj;

	public UILabel checkpointLabel;

	public GameObject legendObj;

	public UILabel levelNameLabel;

	public GameObject validationAnchor;

	public GameObject bonusLabel;

	public UILabel bonusTimeLabel;

	public bool menuActive;

	private float timeScaleBeforePause = 1f;

	private int resetsThisLevel;

	private int playResetSoundAt = 3;

	private bool bonusDefeatSoundPlayed;

	private bool bonusCountDownPlayed;

	private void TransitionCompleted()
	{
		InitializeScreen();
		checkpointLabel.text = string.Empty;
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			ToggleLegend(state: true);
		}
		else
		{
			ToggleLegend(state: false);
		}
		bonusLabel.SetActive(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus || Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop);
		bonusTimeLabel.transform.parent.gameObject.SetActive(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus);
		bonusDefeatSoundPlayed = false;
		bonusCountDownPlayed = false;
	}

	private void Update()
	{
		if (!Singleton<InputManager>.SP.inputEnabled)
		{
			return;
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel) && menuActive)
		{
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			Button_Resume();
			AudioController.Play("ButtonForwards");
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Start))
		{
			if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
			{
				return;
			}
			if (menuActive)
			{
				Button_Resume();
				AudioController.Play("ButtonForwards");
			}
			else
			{
				ShowMenu();
				AudioController.Play("ButtonBackwards");
			}
		}
		if (!menuActive)
		{
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus)
			{
				UpdateBonusTimeLabel();
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Retry))
			{
				AudioController.Play("Reset");
				Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
				Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.LevelReset, string.Empty);
				resetsThisLevel++;
				if (resetsThisLevel >= playResetSoundAt)
				{
					Singleton<AudioManager>.SP.PlayCharacterResetSound(Singleton<PlayerManager>.SP.GetPlayer());
					playResetSoundAt += 3;
				}
			}
			else if (Singleton<InputManager>.SP.CheckAction(InputAction.ResetCheckpoint))
			{
				AudioController.Play("Reset");
				Singleton<PlayerManager>.SP.ResetPlayer(Singleton<PlayerManager>.SP.GetPlayer(), hard: false);
			}
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm))
		{
			Singleton<InputManager>.SP.ClickCurrentButton();
			AudioController.Play("ButtonForwards");
		}
	}

	private void UpdateBonusTimeLabel()
	{
		float num = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().bonusTime - Singleton<Stopwatch>.SP.GetCurrentTime();
		string text = "[FFFFFF]";
		if (num < 5f)
		{
			text = "[FF0000]";
		}
		if (num < 3.1f && !bonusCountDownPlayed)
		{
			bonusCountDownPlayed = true;
			AudioController.Play("bonus_countdown");
		}
		if (num < 0f)
		{
			num = 0f;
			if (!bonusDefeatSoundPlayed)
			{
				Singleton<AudioManager>.SP.PlayCharacterDefeat(Singleton<PlayerManager>.SP.GetPlayer());
				bonusDefeatSoundPlayed = true;
			}
		}
		bonusTimeLabel.text = text + Singleton<HighscoreManager>.SP.ConvertTimeToString(num) + "[-]";
	}

	private void ShowMenu()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			ToggleLegend(state: false);
		}
		GameObject[] menuObjects = MenuObjects;
		for (int i = 0; i < menuObjects.Length; i++)
		{
			menuObjects[i].SetActive(value: true);
			for (int j = 0; j < MenuObjects.Length; j++)
			{
				MenuObjects[j].SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
			}
		}
		Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: true, string.Empty);
		Singleton<MainMenuHighlighter>.SP.SetAsTitle(Language.Get("PAUSE", "GAME"));
		menuActive = true;
		timeScaleBeforePause = Time.timeScale;
		Time.timeScale = 0f;
		stateObj.MyInputEnabled = true;
		Singleton<InputManager>.SP.Select(MenuObjects[0].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
		Singleton<PermaGUI>.SP.ToggleIngameTutorial(state: false);
		Singleton<PermaGUI>.SP.KillCountdownLabels();
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerController>().RemoveControl();
		PlatformerController.RemoveRumble();
		if ((bool)AudioController.GetCurrentMusic())
		{
			Camera.main.gameObject.AddComponent<AudioLowPassFilter>();
		}
	}

	public void HideMenu()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			ToggleLegend(state: true);
		}
		GameObject[] menuObjects = MenuObjects;
		for (int i = 0; i < menuObjects.Length; i++)
		{
			menuObjects[i].SetActive(value: false);
		}
		Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
		menuActive = false;
		Time.timeScale = timeScaleBeforePause;
		stateObj.MyInputEnabled = false;
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.None)
		{
			Singleton<PermaGUI>.SP.ToggleIngameTutorial(state: true);
		}
		if ((bool)AudioController.GetCurrentMusic() && (bool)Camera.main.GetComponent<AudioLowPassFilter>())
		{
			Object.Destroy(Camera.main.GetComponent<AudioLowPassFilter>());
		}
	}

	public void ShowStartLabel()
	{
		StartCoroutine(ToggleStart());
	}

	private IEnumerator ToggleStart()
	{
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle == LevelStyle.KidsRoom_Halloween)
		{
			Singleton<PermaGUI>.SP.labelsReadyGo[0].enabled = true;
			yield return new WaitForSeconds(0.8f);
			Singleton<PermaGUI>.SP.labelsReadyGo[0].enabled = false;
		}
		else
		{
			Singleton<PermaGUI>.SP.labelGo.text = "Go!";
			yield return new WaitForSeconds(0.8f);
			Singleton<PermaGUI>.SP.labelGo.text = string.Empty;
		}
	}

	public string GoText()
	{
		return string.Empty;
	}

	public void Button_Quit()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			Object.FindObjectOfType<State_InGameLevelEditor>().CloseLevelEditor();
		}
		HideMenu();
		SlomoTrigger[] array = Object.FindObjectsOfType<SlomoTrigger>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnReset();
		}
		Time.timeScale = 1f;
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.RageQuit, string.Empty);
	}

	public void Button_SwitchCharacter()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			Object.FindObjectOfType<State_InGameLevelEditor>().CloseLevelEditor();
		}
		HideMenu();
		HenkUtils.BackToMenu();
		Camera.main.enabled = true;
		Camera.main.GetComponent<MenuCamera>().SetMode(MenuCamera.CameraMode.MainMenu);
		Singleton<GamestateManager>.SP.SetState(typeof(State_CharacterSelectionCampaign));
	}

	public void Button_LevelSelect()
	{
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 97)
		{
			Time.timeScale = 1f;
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
			HideMenu();
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
			return;
		}
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			Object.FindObjectOfType<State_InGameLevelEditor>().CloseLevelEditor();
		}
		HideMenu();
		SlomoTrigger[] array = Object.FindObjectsOfType<SlomoTrigger>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnReset();
		}
		Time.timeScale = 1f;
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_LevelEditorMain));
			HenkUtils.BackToMenu();
		}
		else
		{
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
			Singleton<GamestateManager>.SP.SetState(typeof(State_LevelSelectCampaign));
		}
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.RageQuit, string.Empty);
	}

	public void Button_SwitchGhost()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LevelEditor)
		{
			HideMenu();
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 97)
			{
				StartCoroutine(RetryNextFrame());
				return;
			}
			Singleton<PlayerManager>.SP.RemoveGhosts();
			Singleton<PlayerManager>.SP.ghostSet = false;
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
		}
	}

	private IEnumerator RetryNextFrame()
	{
		Time.timeScale = 1f;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		AudioController.Play("Reset");
		Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
	}

	public void Button_Resume()
	{
		if (menuActive)
		{
			HideMenu();
			StartCoroutine("GiveControlUntilButtonRelease");
		}
	}

	private IEnumerator GiveControlUntilButtonRelease()
	{
		while (Singleton<InputManager>.SP.CheckActionContinuous(InputAction.Jump))
		{
			yield return new WaitForEndOfFrame();
		}
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerController>().GiveControl();
	}

	public void ShowCheckpointTime(float timeDiff)
	{
		StopCoroutine("ShowCPTime");
		StartCoroutine("ShowCPTime", timeDiff);
	}

	public void ToggleLegend(bool state)
	{
		legendObj.SetActive(state);
		if (state)
		{
			levelNameLabel.text = Singleton<LevelEditorFileWriter>.SP.levelName;
		}
	}

	private IEnumerator ShowCPTime(float timeDiff)
	{
		bool flag = false;
		if (timeDiff > 0f)
		{
			flag = true;
		}
		string text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Mathf.Abs(timeDiff));
		if (!flag)
		{
			checkpointLabel.color = Color.red;
			checkpointLabel.text = "+ " + text.ToString();
		}
		else
		{
			checkpointLabel.color = Color.green;
			checkpointLabel.text = "- " + text.ToString();
		}
		yield return new WaitForSeconds(1.5f);
		checkpointLabel.text = string.Empty;
	}
}
