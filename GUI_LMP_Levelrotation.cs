public class GUI_LMP_Levelrotation : GUI_Base
{
	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
	}

	private void Update()
	{
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP, "none");
		AudioController.Play("ButtonBackwards");
	}
}
