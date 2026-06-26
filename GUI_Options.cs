public class GUI_Options : GUI_Base
{
	public InputObject FirstButton;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
	}

	private void NextWindow()
	{
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.ClickCurrentButton();
		AudioController.Play("ButtonForwards");
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		AudioController.Play("ButtonBackwards");
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
	}

	public void Button_SettingsAudio()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_OptionsAudio));
	}

	public void Button_SettingsGame()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_OptionsGame));
	}

	public void Button_SettingsGraphics()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_OptionsGraphics));
	}

	public void Button_Resolutions()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_OptionsGraphicsResolutions, "none");
	}
}
