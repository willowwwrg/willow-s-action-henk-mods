public class GUI_LevelEditorMain : GUI_Base
{
	public InputObject FirstButton;

	public State_LevelEditorMain stateObj;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		Singleton<LevelBatchManager>.SP.RefreshWorkshopLevels();
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		AudioController.Play("ButtonBackwards");
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
	}

	private void NextWindow()
	{
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.ClickCurrentButton();
		AudioController.Play("ButtonForwards");
	}

	public void Button_LevelEditorPlay()
	{
		if (!SteamManager.Initialized)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDSTEAMCONNECT", "PERMA"));
			return;
		}
		stateObj.cameFromMenu = true;
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorPlayLevel, "none");
	}

	public void Button_LevelEditorCreate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorCreateLevel, "none");
	}

	public void Button_LevelEditorBrowse()
	{
		if (!SteamManager.Initialized)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDSTEAMCONNECT", "PERMA"));
		}
		else
		{
			Singleton<Workshop>.SP.OpenSteamOverlayWorkshop();
		}
	}
}
