public class State_Credits : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Credits, "none");
		if (!AudioController.IsPlaying("LoadingScreen"))
		{
			AudioController.StopMusic();
			AudioController.PlayMusic("LoadingScreen");
		}
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
