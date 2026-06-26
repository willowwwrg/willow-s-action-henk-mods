using UnityEngine;

public class GUI_SplashStart : GUI_Base
{
	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: false, stateForwards: false, string.Empty, string.Empty);
		Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
		Singleton<PermaGUI>.SP.ToggleArrows(state: false);
		Singleton<InputManager>.SP.Deselect();
		AudioController.Play("ActionHenk");
	}

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MainMenu, "none");
			AudioController.Play("LevelStart");
		}
	}
}
