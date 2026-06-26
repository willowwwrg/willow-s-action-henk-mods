public class State_GameModeSelection : GameState
{
	public override void OnActivate()
	{
		Singleton<GUIManager>.SP.SetScreen(GUIManager.GUIScreens.GUIScreen_GameModeSelection);
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
