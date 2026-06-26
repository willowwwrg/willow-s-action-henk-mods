public class State_OptionsAudio : GameState
{
	public GUI_OptionsAudio guiScript;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_OptionsAudio, "none");
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
