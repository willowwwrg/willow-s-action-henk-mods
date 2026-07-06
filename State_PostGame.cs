using System.Collections;
using UnityEngine;

public class State_PostGame : GameState
{
	public GameObject GUIObject;

	private GUI_PostGame guiScript;

	private CameraState prevState;

	public override void OnActivate()
	{
		// Force GC collection at natural pause point to prevent mid-run freezes
		System.GC.Collect();
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.None)
		{
			Singleton<LevelBatchManager>.SP.AddCurrentLevelToSessionLevels();
		}
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.Local, state: false);
		SlomoTrigger[] array = Object.FindObjectsOfType<SlomoTrigger>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnReset();
		}
		Time.timeScale = 1f;
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		PlayerPrefs.SetInt("MOSTRECENTLYCOMPLETEDLEVEL", Singleton<LevelBatchManager>.SP.GetCurrentLevel());
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.LevelComplete, Singleton<LevelBatchManager>.SP.GetCurrentLevel().ToString());
		Singleton<ReplayManager>.SP.CopyRecordedDataToController();
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
		{
			Singleton<Workshop>.SP.SetLevelCompleted();
		}
		bool flag = Singleton<CheckpointManager>.SP.GetFinishTime() < Singleton<CheckpointManager>.SP.opponentFinishTime;
		if (flag)
		{
			Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
			if (currentLevelObj.levelType != LevelType.Challenge && currentLevelObj.levelType == LevelType.Standard)
			{
				Singleton<AudioManager>.SP.PlayCharacterVictory(Singleton<PlayerManager>.SP.GetPlayer(), 1.2f);
			}
		}
		else
		{
			Level currentLevelObj2 = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
			if (currentLevelObj2.levelType == LevelType.Challenge)
			{
				Singleton<AudioManager>.SP.PlayCharacterTaunt(currentLevelObj2.challenger, 1.2f);
			}
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelCodeOrID() == 635322454)
		{
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 2);
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 39 && flag)
		{
			Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerPhysics>().input.walkInput = 1f;
		}
		StartCoroutine("StartReplay");
		PostGameGUI();
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.None)
		{
			Singleton<HenkSWUserStats>.SP.WriteCloudSave();
		}
		Singleton<AchievementsManager>.SP.FinishedLevel(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
	}

	private void PostGameGUI()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_PostGame, "None");
		guiScript = GUIObject.GetComponent<GUI_PostGame>();
		guiScript.StartPostGame();
	}

	public override void OnDeactivate()
	{
		StopCoroutine("StartReplay");
		StopCoroutine("KentonyTNTLavaDeath");
		Camera.main.GetComponent<PlatformerCamera>().ToggleExtraControls(enable: false);
		HenkUtils.ToggleGUICameras(state: true);
		ResetCamera();
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerController>().isExternalControlled = false;
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<ReplayController>().Stop();
		Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
		guiScript.ResetGUI();
		Singleton<GUIManager>.SP.LeaveGUIScreen();
	}

	public override void OnUpdate()
	{
		if (!Singleton<InputManager>.SP.inputEnabled)
		{
			return;
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Retry))
		{
			guiScript.Button_Retry();
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.SwitchGhost))
		{
			guiScript.Button_SwitchGhost();
			AudioController.Play("ButtonBackwards");
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm) || Singleton<InputManager>.SP.CheckAction(InputAction.BackToMainMenu))
		{
			guiScript.Button_ToLevelSelect();
			AudioController.Play("ButtonForwards");
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.NextLeaderboard))
		{
			if (guiScript.postGameState != GUI_PostGame.PostGameState.Finish)
			{
				guiScript.NextLeaderboard(forwards: true);
			}
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.PrevLeaderboard) && guiScript.postGameState != GUI_PostGame.PostGameState.Finish)
		{
			guiScript.NextLeaderboard(forwards: false);
		}
	}

	private IEnumerator StartReplay()
	{
		yield return new WaitForSeconds(3f);
		if (!Singleton<RewardManager>.SP.showingReward)
		{
			Singleton<PermaGUI>.SP.FadeOutAndIn(0.5f, 0.2f);
		}
		yield return new WaitForSeconds(0.6f);
		GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
		Singleton<CheckpointManager>.SP.InitializeCheckpoints();
		EffectsTrigger[] array = Object.FindObjectsOfType<EffectsTrigger>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnReset();
		}
		Pickup[] array2 = Object.FindObjectsOfType<Pickup>();
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].OnReset();
		}
		Collectable[] array3 = Object.FindObjectsOfType<Collectable>();
		for (int k = 0; k < array3.Length; k++)
		{
			array3[k].OnReset();
		}
		Singleton<PlayerManager>.SP.ResetAllPlayers();
		player.GetComponent<PlatformerController>().isExternalControlled = true;
		StartPostGameCamera();
		Singleton<CheckpointManager>.SP.GetStartline().GetComponent<StartLine>().DoCountDown();
		yield return new WaitForSeconds(State_PreGame.GetCountDownTime());
		Singleton<PlayerManager>.SP.TogglePlayerControls(player, state: true);
		player.GetComponent<ReplayController>().Stop();
		player.GetComponent<ReplayController>().Play();
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.Ghost, state: true);
	}

	private void StartPostGameCamera()
	{
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.StaticFollow);
		Camera.main.GetComponent<PlatformerCamera>().SnapCamera();
		Camera.main.GetComponent<PlatformerCamera>().ToggleExtraControls(enable: true);
	}

	private void ResetCamera()
	{
		Camera.main.transform.parent = null;
		Camera.main.GetComponent<PlatformerCamera>().enabled = true;
	}

	private IEnumerator KentonyTNTLavaDeath()
	{
		GameObject camParent = Object.FindObjectOfType<TNT_Ending>().cameraParent;
		Camera.main.GetComponent<PlatformerCamera>().enabled = false;
		Camera.main.transform.parent = camParent.transform;
		Camera.main.transform.localPosition = Vector3.zero;
		Camera.main.transform.localEulerAngles = Vector3.zero;
		camParent.animation.Play();
		GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
		GameObject ghost = Singleton<PlayerManager>.SP.GetGhost();
		Singleton<PlayerManager>.SP.TogglePlayerControls(ghost, state: false);
		SplineFollow component = Singleton<CheckpointManager>.SP.Finishline.GetComponent<SplineFollow>();
		Vector2 vector = component.splineOffset;
		Vector2 offset = vector + new Vector2(-2f, 0.88f);
		Vector2 offset2 = vector + new Vector2(-45f, 0.88f);
		player.GetComponent<PlayerWaypointManager>().SetOffset(offset, alsoSetPos: true);
		ghost.GetComponent<PlayerWaypointManager>().SetOffset(offset2, alsoSetPos: true);
		player.GetComponent<RaycastCollider>().velocity = component.transform.right * 30f;
		ghost.GetComponent<RaycastCollider>().velocity = component.transform.right * 1f;
		player.GetComponent<PlatformerPhysics>().input.walkInput = 1f;
		ghost.GetComponent<PlatformerPhysics>().input.walkInput = 0.75f;
		yield return new WaitForSeconds(1.1f);
		player.GetComponent<PlatformerPhysics>().input.jumpInput = true;
		yield return new WaitForSeconds(3f);
		Singleton<AudioManager>.SP.PlayCharacterLavaDeath(Singleton<PlayerManager>.SP.GetGhost());
		yield return new WaitForSeconds(3f);
		Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 0);
		Singleton<InputManager>.SP.inputEnabled = true;
		camParent.animation.Stop();
		HenkUtils.ToggleGUICameras(state: true);
	}
}
