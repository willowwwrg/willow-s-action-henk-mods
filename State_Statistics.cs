public class State_Statistics : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Statistics, "none");
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
