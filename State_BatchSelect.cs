public class State_BatchSelect : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_BatchSelect, "none");
	}

	public override void OnUpdate()
	{
	}

	public override void OnDeactivate()
	{
	}
}
