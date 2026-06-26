public class State_LevelSelectMultiplayer : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelSelectMultiplayer, "none");
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
