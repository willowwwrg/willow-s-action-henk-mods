public class State_BatchSelectExtraLevels : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_BatchSelectExtraLevels, "none");
	}

	public override void OnUpdate()
	{
	}

	public override void OnDeactivate()
	{
	}
}
