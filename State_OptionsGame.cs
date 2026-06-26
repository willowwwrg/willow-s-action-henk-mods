using UnityEngine;

public class State_OptionsGame : GameState
{
	public GUI_OptionsGame guiScript;

	public bool showTutorialDuringFirstBatchLevels = true;

	private void Awake()
	{
		if (PlayerPrefs.GetInt("Options_HideTutorialIngame", 0) == 1)
		{
			showTutorialDuringFirstBatchLevels = false;
		}
		else
		{
			showTutorialDuringFirstBatchLevels = true;
		}
	}

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_OptionsGame, "none");
		guiScript.StartOptions();
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		bool num = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		bool flag = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool flag2 = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (num && flag && flag2 && Input.GetKeyDown(KeyCode.Q))
		{
			Singleton<PermaGUI>.SP.RequestConfirmation("DeleteAll", base.gameObject, "DELETE *EVERYTHING*?");
		}
	}

	public void DeleteAll(bool confirm)
	{
		if (confirm)
		{
			Singleton<LevelBatchManager>.SP.ResetUnlocks();
			PlayerPrefs.DeleteAll();
			Singleton<HenkSWUserStats>.SP.WriteCloudSave(forceWrite: true);
			Singleton<HenkSWUserStats>.SP.ResetAllStatsAndAchievements();
			AudioController.Play("Toeter");
		}
	}
}
