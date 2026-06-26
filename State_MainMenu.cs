using System.Collections;
using UnityEngine;

public class State_MainMenu : GameState
{
	public GUI_MainMenu guiScript;

	private bool waitingForSceneLoad;

	[HideInInspector]
	public bool initialized;

	public bool startScreenShown;

	private float inactivityTime;

	private bool mainMenuLoaded;

	public override void OnActivate()
	{
		initialized = false;
		Screen.showCursor = false;
		if (Application.loadedLevelName != "MenuScene" && !mainMenuLoaded)
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_None, "none");
			waitingForSceneLoad = true;
			Application.LoadLevelAsync("MenuScene");
		}
		else
		{
			HenkUtils.BackToMenu();
			InitState();
		}
		Object.FindObjectOfType<State_LevelEditorMain>().cameFromMenu = true;
		if (Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters)
		{
			Camera.main.GetComponent<MenuCamera>().LookAtCharacter((CharacterSelect.Characters)Singleton<PlayerPrefsManager>.SP.GetInt("LASTPLAYEDCHARACTER", 1));
			Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters = false;
		}
	}

	private void OnLevelWasLoaded()
	{
		if (waitingForSceneLoad)
		{
			HenkUtils.BackToMenu();
			InitState();
			waitingForSceneLoad = false;
		}
	}

	private void InitState()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MainMenu, "none");
		Camera.main.enabled = true;
		Camera.main.GetComponent<MenuCamera>().SetMode(MenuCamera.CameraMode.MainMenu);
		if (!mainMenuLoaded)
		{
			StartCoroutine(FirstTimeMenu());
		}
		else if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music");
			AudioController.PlayMusic("MainTheme");
		}
		initialized = true;
		Singleton<MultiManager>.SP.CheckServerStatus();
		Singleton<AchievementsManager>.SP.CheckStatBasedAchievements();
		if (Singleton<MutatorManager>.SP.seedOfToday != 0)
		{
			Singleton<HenkSWLeaderboards>.SP.GetDailyChallengeRank();
		}
		guiScript.loadingLabel.enabled = false;
		mainMenuLoaded = true;
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_MainMenu && Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_SplashStart && initialized)
		{
			Singleton<ActionHenk>.SP.HardResetGame("Main menu - " + Singleton<GUIManager>.SP.GetCurrentScreenName());
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel) && Singleton<InputManager>.SP.GetCurrentButton() != guiScript.exitButton)
		{
			Singleton<InputManager>.SP.Select(guiScript.exitButton);
		}
	}

	private void Update()
	{
		inactivityTime += Time.deltaTime;
		if (Input.anyKey || Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
		{
			inactivityTime = 0f;
		}
		Singleton<GUIManager>.SP.GetCurrentScreenName();
	}

	private void StartReplayMode()
	{
		inactivityTime = 0f;
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Replay);
		Object.FindObjectOfType<State_ReplayMode>().NextLevel(0f);
	}

	private IEnumerator FirstTimeMenu()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music");
			AudioController.PlayMusic("MainTheme");
		}
		Singleton<HenkSWUserStats>.SP.CheckSkinUnlocks();
	}

	public void SetTHStatusBox(string text)
	{
		bool flag = text != string.Empty;
		guiScript.twitchStatusBox.GetComponent<UITweener>().Play(!flag);
		guiScript.twitchStatusBoxLabel.text = text;
	}
}
