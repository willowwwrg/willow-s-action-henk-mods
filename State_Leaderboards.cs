public class State_Leaderboards : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelSelectLeaderboards, "none");
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
