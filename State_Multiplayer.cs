public class State_Multiplayer : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Multiplayer, "none");
		Singleton<MultiManager>.SP.ResetConnectTimer();
		if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music", 0.5f);
			AudioController.PlayMusic("MainTheme");
		}
	}

	public override void OnDeactivate()
	{
		Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
	}

	public override void OnUpdate()
	{
		if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.Disconnected)
		{
			Singleton<MultiManager>.SP.Connect();
		}
	}
}
