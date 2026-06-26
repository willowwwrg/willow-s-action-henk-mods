public class GUI_EndGameMultiplayer : GUI_Base
{
	public UILabel nextLevelLabel;

	private void TransitionCompleted()
	{
		InitializeScreen();
		ulong levelInRotation = Singleton<MultiManager>.SP.GetLevelInRotation(1);
		nextLevelLabel.text = Singleton<LevelBatchManager>.SP.GetLevelFromCodeOrID(levelInRotation).levelName;
		if (Singleton<LevelBatchManager>.SP.GetLevelFromCodeOrID(levelInRotation).levelType == LevelType.Workshop)
		{
			UILabel uILabel = nextLevelLabel;
			uILabel.text = uILabel.text + " by " + Singleton<LevelBatchManager>.SP.GetLevelFromCodeOrID(levelInRotation).levelCreator;
		}
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Start))
		{
			Singleton<InGameMenu>.SP.ToggleMenu(!Singleton<InGameMenu>.SP.menuEnabled);
		}
	}
}
