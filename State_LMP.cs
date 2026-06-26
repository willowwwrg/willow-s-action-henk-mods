using UnityEngine;

public class State_LMP : GameState
{
	public CharacterPreviewerLMP charPreviewer;

	public Texture[] henkOverrideTextures;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP, "none");
		Camera.main.GetComponent<MenuCamera>().LookAtLMPcharacters();
		if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music");
			AudioController.PlayMusic("MainTheme");
		}
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() == GUIManager.GUIScreens.GUIScreen_LMP)
		{
			if (Singleton<InputManager>.SP.CheckActionJoystick(InputAction.ResetCheckpoint))
			{
				Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerLevelRotation, "none");
				AudioController.Play("ButtonForwards");
			}
			if (Singleton<InputManager>.SP.CheckActionJoystick(InputAction.InstantAction))
			{
				Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP_Settings, "none");
				AudioController.Play("ButtonForwards");
			}
			if (Singleton<InputManager>.SP.CheckActionJoystick(InputAction.Select) || Singleton<InputManager>.SP.CheckActionKeyboard(InputAction.Cancel))
			{
				Singleton<LocalMultiManager>.SP.ClearPlayers();
				Singleton<GamestateManager>.SP.SetState(typeof(State_MultiplayerPicker));
				AudioController.Play("ButtonBackwards");
				Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			}
		}
	}
}
