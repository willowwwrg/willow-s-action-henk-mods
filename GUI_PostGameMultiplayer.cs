using UnityEngine;

public class GUI_PostGameMultiplayer : GUI_Base
{
	public GameObject scoreBoardGUI;

	public GUI_InGameMultiplayer inGameGUI;

	public UILabel timeLabel;

	public UILabel newPBLabel;

	private void TransitionCompleted()
	{
		InitializeScreen();
		scoreBoardGUI.SetActive(value: true);
		timeLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Singleton<CheckpointManager>.SP.GetFinishTime());
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
	}

	private void Update()
	{
	}
}
