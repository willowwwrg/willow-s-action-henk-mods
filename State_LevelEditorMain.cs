public class State_LevelEditorMain : GameState
{
	public bool cameFromMenu = true;

	public override void OnActivate()
	{
		if (cameFromMenu)
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorMain, "none");
		}
		else
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorPlayLevel, "none");
		}
		if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music", 0.5f);
			AudioController.PlayMusic("MainTheme");
		}
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
