using System.Collections;
using System.Collections.Generic;
using Henk;
using UnityEngine;

public class State_ReplayMode : GameState
{
	public GUI_ReplayMode GUIScript;

	public bool demoMode;

	private int replayPlayerRank;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_ReplayMode, "none");
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
		if (HenkUtils.IsInALevel())
		{
			Singleton<PermaGUI>.SP.FadeInOrOut(1.5f, fadeIn: true);
			Singleton<AudioManager>.SP.PlayIngameTheme();
			GUIScript.demoMode = demoMode;
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
			Camera.main.GetComponent<PlatformerCamera>().ToggleExtraControls(enable: true);
			StartCoroutine("StartReplay", 0);
		}
	}

	public override void OnDeactivate()
	{
		StopCoroutine("StartReplay");
		StopCoroutine("NextLevelRoutine");
	}

	public override void OnUpdate()
	{
		if (demoMode && Input.anyKeyDown)
		{
			Object.FindObjectOfType<State_MainMenu>().startScreenShown = false;
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		}
	}

	public void SetReplayController(ReplayController controller)
	{
		GUIScript.replayController = controller;
	}

	private IEnumerator StartReplay(float delay)
	{
		yield return new WaitForSeconds(delay);
		Singleton<AudioManager>.SP.PlayIngameTheme();
		GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
		player.GetComponent<ReplayController>().Stop();
		Singleton<PlayerManager>.SP.ResetAllPlayers();
		Singleton<Stopwatch>.SP.ResetTimer();
		Singleton<CheckpointManager>.SP.InitializeCheckpoints();
		HenkUtils.ResetLevel();
		Camera.main.GetComponent<PlatformerCamera>().FindTarget();
		Camera.main.GetComponent<PlatformerCamera>().OnReset();
		AudioController.Play("Ready");
		Singleton<CheckpointManager>.SP.GetStartline().GetComponent<StartLine>().DoCountDown();
		yield return new WaitForSeconds(State_PreGame.GetCountDownTime());
		AudioController.Play("Start");
		Singleton<PlayerManager>.SP.TogglePlayerControls(player, state: true);
		player.GetComponent<ReplayController>().Play();
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus)
		{
			Object.FindObjectOfType<State_InGame>().UpdateCollectibles();
		}
		Singleton<Stopwatch>.SP.StartTimer();
	}

	public void LevelFileLoadedReplay()
	{
		OnLevelWasLoadedCampaign();
	}

	public void OnLevelWasLoadedCampaign()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.Replay || !HenkUtils.IsInALevel())
		{
			return;
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj() != null)
		{
			Singleton<LevelBatchManager>.SP.SetCurrentLevel(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelCode);
		}
		else
		{
			Singleton<LevelBatchManager>.SP.SetCurrentLevel(Application.loadedLevel);
		}
		if (demoMode)
		{
			Singleton<CharacterSelect>.SP.SetSelectedCharacter(CharacterSelect.Characters.Henk);
			Singleton<CharacterSelect>.SP.SetSelectedSkin(0);
			Singleton<PlayerManager>.SP.SpawnPlayer();
			Singleton<PlayerManager>.SP.GetPlayer().GetComponent<ReplayController>().LoadReplay(GhostType.MedalRainbow, 0uL);
			Singleton<GamestateManager>.SP.SetState(typeof(State_ReplayMode));
			return;
		}
		Singleton<PlayerManager>.SP.SpawnPlayer();
		GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
		if (Input.GetKey(KeyCode.LeftControl) && Application.isEditor)
		{
			player.GetComponent<ReplayController>().LoadReplay(GhostType.MedalRainbow, 0uL);
			player.GetComponent<PlayerGraphics>().SetModel(Singleton<CharacterSelect>.SP.GetSelectedCharacter(), Singleton<CharacterSelect>.SP.GetSelectedSkin());
		}
		else
		{
			player.GetComponent<ReplayController>().LoadReplay(GhostType.SpecificPlayer, 0uL);
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_ReplayMode));
	}

	public void StartReplay(int levelCode)
	{
		Singleton<LevelBatchManager>.SP.LoadLevelSceneless(Singleton<LevelBatchManager>.SP.GetLevelFromCode(levelCode));
	}

	public void NextLevel(float delay)
	{
		StartCoroutine("NextLevelRoutine", delay);
	}

	private IEnumerator NextLevelRoutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		List<Level> allLevels = Singleton<LevelBatchManager>.SP.GetAllLevels();
		int index = Random.Range(0, allLevels.Count);
		while (!LevelCanHaveReplayMode(allLevels[index]))
		{
			index = Random.Range(0, allLevels.Count);
		}
		Singleton<LevelBatchManager>.SP.LoadLevelSceneless(allLevels[index]);
	}

	private bool LevelCanHaveReplayMode(Level level)
	{
		if (level.levelCode == 0 || level.levelType != LevelType.Standard)
		{
			return false;
		}
		return true;
	}

	public void Finished()
	{
		if (demoMode)
		{
			NextLevel(5f);
		}
		else
		{
			StartCoroutine("StartReplay", 5f);
		}
	}
}
