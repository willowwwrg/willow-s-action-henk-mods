using System.Collections;
using Henk;
using UnityEngine;

public class State_InGame : GameState
{
	public Collectable[] allCollectibles;

	private int prevCollectibles;

	public GUI_InGame guiScript;

	public bool challengerIntroSoundPlayed;

	public override void OnActivate()
	{
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerController>().GiveControl();
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_InGame, "None");
		guiScript.HideMenu();
		Singleton<Stopwatch>.SP.ResetTimer();
		Singleton<Stopwatch>.SP.StartTimer();
		Time.timeScale = 1f;
		Singleton<ReplayRecorder>.SP.StartRecord();
		guiScript.ShowStartLabel();
		Singleton<PermaGUI>.SP.ToggleGhostLabels(state: true);
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.All, state: true);
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerGraphics>().ParticleEvent(SfxEvents.ControlEnabled);
		UpdateCollectibles();
		// Initialise bonus split tracking
		Singleton<BonusSplitManager>.SP.OnLevelLoad();
		if (Singleton<Workshop>.SP.validating && !Singleton<Workshop>.SP.validationMsgShown)
		{
			StartCoroutine(DelayPopup());
			Singleton<Workshop>.SP.validationMsgShown = true;
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
		}
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (currentLevelObj.levelType == LevelType.Challenge && !challengerIntroSoundPlayed)
		{
			Singleton<AudioManager>.SP.PlayCharacterTaunt(currentLevelObj.challenger, 0.5f);
			challengerIntroSoundPlayed = true;
		}
	}

	private IEnumerator DelayPopup()
	{
		yield return new WaitForSeconds(0.5f);
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("VALIDATE", "LEVELEDITOR"));
	}

	public override void OnDeactivate()
	{
		Singleton<Stopwatch>.SP.StopTimer();
		Singleton<PermaGUI>.SP.ToggleGhostLabels(state: false);
		Singleton<PermaGUI>.SP.ToggleIngameTutorial(state: false);
	}

	public override void OnUpdate()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_InGame)
		{
			Singleton<ActionHenk>.SP.HardResetGame("Ingame - " + Singleton<GUIManager>.SP.GetCurrentScreenName());
		}
		if ((Input.GetKeyDown(KeyCode.F2) || Singleton<InputManager>.SP.CheckAction(InputAction.Start)) && Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor && !guiScript.menuActive && Camera.main.GetComponent<PlatformerCamera>().GetCurrentCameraState() != CameraState.Lava)
		{
			GoToInGameLevelEditor();
		}
	}

	private void GoToInGameLevelEditor()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
	}

	public void UpdateCollectibles()
	{
		allCollectibles = Object.FindObjectsOfType<Collectable>();
		prevCollectibles = 0;
		guiScript.bonusLabel.SetActive(allCollectibles.Length != 0);
	}

	public void FixedUpdate()
	{
		if ((!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGame)) && !Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)) && !Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_ReplayMode))) || (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType != LevelType.Bonus && Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType != LevelType.Workshop && Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LevelEditor))
		{
			return;
		}
		int num = 0;
		Collectable[] array = allCollectibles;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].isPickedUp)
			{
				num++;
			}
		}
		if (guiScript.bonusLabel.activeInHierarchy)
		{
			guiScript.bonusLabel.GetComponentInChildren<UILabel>().text = num + " left!";
		}
		if (prevCollectibles != num && num == 0)
		{
			Singleton<CheckpointManager>.SP.Finish(Singleton<Stopwatch>.SP.GetCurrentTime());
		}
		prevCollectibles = num;
	}
}
