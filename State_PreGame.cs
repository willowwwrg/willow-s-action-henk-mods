using System.Collections;
using UnityEngine;

public class State_PreGame : GameState
{
	public GUI_PreGame guiScript;

	public GUI_OptionsGraphics guiGraphicsOptions;

	public bool countingDown;

	private bool initialized;

	public bool cameFromLoadingScreen;

	public GhostType lastSelectedGhostType;

	public ulong friendIDToDownloadInstantly;

	public override void OnActivate()
	{
		Initialize();
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
		AudioController.GetAudioItem("PickupCoin").subItems[0].PitchShift = 0f;
	}

	public void LevelFileLoaded()
	{
		Singleton<PlayerManager>.SP.ghostSet = false;
		OnLevelWasLoadedCampaign();
	}

	public void LevelFileLoadedWorkshop()
	{
		Singleton<PlayerManager>.SP.ghostSet = false;
		OnLevelWasLoadedWorkshop();
	}

	public void Initialize()
	{
		initialized = true;
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_PreGame, "None");
		Singleton<InternetManager>.SP.CheckConnection();
		Singleton<GAManager>.SP.ResetAverageCounter();
		if (!cameFromLoadingScreen)
		{
			guiScript.InitializeScreen();
		}
		if (!HenkUtils.IsInALevel())
		{
			return;
		}
		Singleton<PlayerManager>.SP.ResetAllPlayers();
		HenkUtils.ResetLevel();
		if (Singleton<PlayerManager>.SP.ghostSet)
		{
			if (Singleton<PlayerManager>.SP.ghostType == GhostType.None)
			{
				Singleton<CheckpointManager>.SP.LoadPBCheckpointTimes();
			}
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
			Camera.main.GetComponent<PlatformerCamera>().OnReset();
			StartCoroutine("CountdownSequence");
		}
		else
		{
			HenkUtils.EnablePreGameCamera();
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
			{
				Singleton<PlayerManager>.SP.SpawnGhost(GhostType.Challenger, 0uL);
			}
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Standard && friendIDToDownloadInstantly != 0L)
			{
				guiScript.ForceFriendDownload(friendIDToDownloadInstantly);
				friendIDToDownloadInstantly = 0uL;
			}
		}
		Singleton<PermaGUI>.SP.FadeInOrOut(1.5f, fadeIn: true);
		if (HenkUtils.IsInALevel())
		{
			Singleton<LevelBatchManager>.SP.SetCurrentLevel(Application.loadedLevel);
		}
		Singleton<AudioManager>.SP.PlayIngameTheme();
		Singleton<CheckpointManager>.SP.InitializeCheckpoints();
		Singleton<PermaGUI>.SP.ToggleIngameTutorial(state: false);
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_PreGame)
		{
			Singleton<ActionHenk>.SP.HardResetGame("Pregame - " + Singleton<GUIManager>.SP.GetCurrentScreenName());
		}
		if (!Singleton<AudioManager>.SP.IsIngameThemePlaying())
		{
			Singleton<AudioManager>.SP.PlayIngameTheme();
		}
	}

	public void SelectGhost(GhostType ghostType, ulong playerID = 0uL)
	{
		if (!Singleton<PlayerManager>.SP.ghostSet)
		{
			lastSelectedGhostType = ghostType;
			StartCoroutine(DownloadReplay(ghostType, playerID));
		}
	}

	private IEnumerator DownloadReplay(GhostType ghostType, ulong playerID = 0uL)
	{
		Singleton<PlayerManager>.SP.SpawnGhost(ghostType, playerID);
		if (playerID == 76561197990024845L)
		{
			AudioController.Play("kakujo");
		}
		while (Singleton<HenkSWLeaderboards>.SP.downloadingReplay)
		{
			yield return new WaitForEndOfFrame();
		}
		if (!Singleton<CheckpointManager>.SP.HasOpponentCheckpointTimes() || Singleton<PlayerManager>.SP.ghostType == GhostType.None)
		{
			Singleton<CheckpointManager>.SP.LoadPBCheckpointTimes();
		}
		guiScript.downloadingLabel.transform.parent.gameObject.SetActive(value: false);
		StartGame();
	}

	public void StartGame()
	{
		Singleton<PlayerManager>.SP.ghostSet = true;
		HenkUtils.DisablePreGameCamera();
		Singleton<AudioManager>.SP.PlayEnvironmentAudio(pregame: false);
		StartCoroutine("CountdownSequence");
	}

	public bool CanRestartGame()
	{
		return !countingDown;
	}

	public void OnLevelWasLoadedWorkshop()
	{
		Object.FindObjectOfType<PreGameCamera>().Initialize();
		Singleton<PlayerManager>.SP.SpawnPlayer();
		initialized = false;
		Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
		if (cameFromLoadingScreen)
		{
			guiScript.InitializeScreen();
			cameFromLoadingScreen = false;
		}
	}

	public void OnLevelWasLoadedCampaign()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer && HenkUtils.IsInALevel())
		{
			Object.FindObjectOfType<PreGameCamera>().Initialize();
			Singleton<PlayerManager>.SP.SpawnPlayer();
			initialized = false;
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
			if (cameFromLoadingScreen)
			{
				guiScript.InitializeScreen();
				cameFromLoadingScreen = false;
			}
		}
	}

	public static float GetCountDownTime()
	{
		if (Singleton<CheckpointManager>.SP.FirstTimeLevelRuns)
		{
			return 1.6f;
		}
		return 0.8f;
	}

	private IEnumerator CountdownSequence()
	{
		Singleton<CheckpointManager>.SP.GetStartline().GetComponent<StartLine>().DoCountDown();
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
		guiScript.downloadingLabel.transform.parent.gameObject.SetActive(value: false);
		countingDown = true;
		float countDownTime = GetCountDownTime();
		AudioController.Play("Ready");
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle == LevelStyle.KidsRoom_Halloween)
		{
			Singleton<PermaGUI>.SP.labelsReadyGo[1].enabled = true;
			Singleton<PermaGUI>.SP.labelsReadyGo[0].enabled = false;
		}
		else
		{
			Singleton<PermaGUI>.SP.labelGo.text = string.Empty;
			Singleton<PermaGUI>.SP.labelReady.text = "Ready...";
		}
		yield return new WaitForSeconds(countDownTime);
		AudioController.Play("Start");
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle == LevelStyle.KidsRoom_Halloween)
		{
			Singleton<PermaGUI>.SP.labelsReadyGo[1].enabled = false;
		}
		else
		{
			Singleton<PermaGUI>.SP.labelReady.text = string.Empty;
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGame));
		countingDown = false;
		Singleton<CheckpointManager>.SP.FirstTimeLevelRuns = false;
	}
}
