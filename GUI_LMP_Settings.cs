public class GUI_LMP_Settings : GUI_Base
{
	public InputObject firstButton;

	private int numRetries = 3;

	private string gamemode = string.Empty;

	private int checkpointRespawning;

	private int maxNumRetries = 10;

	private int minNumRetries = 1;

	public UILabel setting_CheckpointRespawning;

	public UILabel setting_RetriesPerLevel;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		numRetries = Singleton<PlayerPrefsManager>.SP.GetInt("LMPSettings_NUMRETRIES", 3);
		checkpointRespawning = Singleton<PlayerPrefsManager>.SP.GetInt("LMPSettings_CHECKPOINTRESPAWNING", 1);
		FillInSettingsLabels();
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP, "none");
		AudioController.Play("ButtonBackwards");
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Left))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "1 numretries")
			{
				SetNumRetries(forwards: false);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "2 checkpointspawning")
			{
				SetCheckpointSpawning(forwards: false);
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Right))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "1 numretries")
			{
				SetNumRetries();
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "2 checkpointspawning")
			{
				SetCheckpointSpawning();
			}
		}
	}

	private void SetNumRetries(bool forwards = true)
	{
		if (forwards)
		{
			AudioController.Play("NextMenuItem");
			numRetries++;
		}
		else
		{
			AudioController.Play("PrevMenuItem");
			numRetries--;
		}
		if (numRetries < minNumRetries)
		{
			numRetries = minNumRetries;
		}
		if (numRetries > maxNumRetries)
		{
			numRetries = maxNumRetries;
		}
		FillInSettingsLabels();
	}

	private void SetCheckpointSpawning(bool forwards = true)
	{
		if (checkpointRespawning == 0)
		{
			checkpointRespawning = 1;
		}
		else
		{
			checkpointRespawning = 0;
		}
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(checkpointRespawning == 1);
		FillInSettingsLabels();
	}

	private void FillInSettingsLabels()
	{
		setting_CheckpointRespawning.text = ((checkpointRespawning != 0) ? "< Yes >" : "< No >");
		setting_RetriesPerLevel.text = "< " + numRetries + " >";
		Singleton<PlayerPrefsManager>.SP.SetInt("LMPSettings_NUMRETRIES", numRetries);
		Singleton<PlayerPrefsManager>.SP.SetInt("LMPSettings_CHECKPOINTRESPAWNING", checkpointRespawning);
	}
}
