public class State_MultiplayerPicker : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerPicker, "none");
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
