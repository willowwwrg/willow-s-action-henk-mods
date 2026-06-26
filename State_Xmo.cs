public class State_Xmo : GameState
{
	public GUI_Xmo guiScript;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Xmo, "None");
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.LevelComplete, Singleton<LevelBatchManager>.SP.GetCurrentLevel().ToString());
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
	}
}
