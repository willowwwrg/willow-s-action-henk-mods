public class State_LevelSelectCampaign : GameState
{
	private bool waitingForSceneLoad;

	public GUI_LevelSelectCampaign guiScript;

	public override void OnActivate()
	{
		HenkUtils.BackToMenu();
		InitState();
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Singleplayer);
	}

	private void OnLevelWasLoaded()
	{
		if (waitingForSceneLoad)
		{
			InitState();
			waitingForSceneLoad = false;
		}
	}

	private void InitState()
	{
		if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music");
			AudioController.PlayMusic("MainTheme");
		}
	}

	public override void OnDeactivate()
	{
		Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
		Singleton<GUIManager>.SP.LeaveGUIScreen();
	}

	public override void OnUpdate()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_LevelSelectCampaign && Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_LevelSelectLeaderboards)
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelSelectCampaign, "None");
		}
	}
}
