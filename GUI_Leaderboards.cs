public class GUI_Leaderboards : GUI_Base
{
	private void TransitionCompleted()
	{
		InitializeScreen();
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		AudioController.Play("ButtonBackwards");
	}
}
