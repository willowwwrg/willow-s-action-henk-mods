using UnityEngine;

public class GUI_Loading : GUI_Base
{
	public UILabel tipLabel;

	public UILabel loadingLabel;

	private int tipCount = 31;

	public UISprite loadingSprite;

	private void TransitionCompleted()
	{
		InitializeScreen();
		loadingSprite.spriteName = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle.ToString();
	}

	public void SetCustomText(string label, string tip)
	{
		loadingLabel.text = label;
		tipLabel.text = tip;
	}

	public void SetDailyChallengeLoadingText()
	{
		tipLabel.text = "Mutator: " + Singleton<MutatorManager>.SP.GetActiveMutatorString();
		loadingLabel.text = "Daily Challenge: " + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelName;
	}

	public void SetLoadingText()
	{
		int num = Random.Range(0, tipCount);
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() >= 64 && Random.value > 0.5f)
		{
			num = Random.Range(21, tipCount);
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 13)
		{
			num = 18;
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 48)
		{
			num = 20;
		}
		else if (num == 20)
		{
			num = Random.Range(0, tipCount);
		}
		tipLabel.text = Language.Get(num.ToString(), "LOADINGMESSAGES");
		loadingLabel.text = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelName;
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 97)
		{
			tipLabel.text = string.Empty;
			loadingLabel.text = string.Empty;
		}
	}
}
