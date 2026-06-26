using System.Collections;
using UnityEngine;

public class State_PreGameMultiplayer : GameState
{
	public bool countingDown;

	public override void OnActivate()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			Singleton<PlayerManager>.SP.ResetAllPlayers();
			HenkUtils.ResetLevel();
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
			Camera.main.GetComponent<PlatformerCamera>().OnReset();
			Singleton<GamestateManager>.SP.SetState(typeof(State_InGame));
		}
		else
		{
			Initialize();
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_PreGameMultiplayer, "none");
			Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
			Singleton<PermaGUI>.SP.ToggleChatBox(state: true);
		}
	}

	public override void OnDeactivate()
	{
		StopCoroutine("SetupConnection");
		if ((bool)Camera.main && (bool)Camera.main.GetComponent<PreGameCamera>() && Camera.main.GetComponent<PreGameCamera>().enabled)
		{
			HenkUtils.DisablePreGameCamera();
		}
	}

	public override void OnUpdate()
	{
		if (!Singleton<AudioManager>.SP.IsIngameThemePlaying())
		{
			Singleton<AudioManager>.SP.PlayIngameTheme();
		}
	}

	public void LevelFileLoadedMultiplayer()
	{
		OnLevelWasLoadedCampaign();
	}

	public void OnLevelWasLoadedCampaign()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer && HenkUtils.IsInALevel())
		{
			Object.FindObjectOfType<PreGameCamera>().Initialize();
			HenkUtils.EnablePreGameCamera();
			StartCoroutine("SetupConnection");
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGameMultiplayer));
		}
	}

	private IEnumerator SetupConnection()
	{
		if (!PhotonNetwork.inRoom)
		{
			Singleton<MultiManager>.SP.DisconnectFromGame("In level but not in room");
			yield break;
		}
		Debug.Log("level loaded");
		while (!PhotonNetwork.inRoom || !Singleton<MultiManager>.SP.IsReadyToReceiveCharacters())
		{
			yield return new WaitForEndOfFrame();
		}
		Debug.Log("connected & spawning player");
		Singleton<MultiManager>.SP.DoneLoading();
		yield return new WaitForSeconds(0.1f);
		if (Singleton<MultiManager>.SP.RoundTimeLeft() > 0f && !Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_EndGameMultiplayer)))
		{
			Singleton<PlayerManager>.SP.SpawnPlayer(multiPlayer: true);
			Debug.Log("spawned player");
			while (Singleton<MultiManager>.SP.WarmupTimeLeft() > 0f)
			{
				yield return new WaitForEndOfFrame();
			}
			Debug.Log("level start");
			HenkUtils.DisablePreGameCamera();
			Singleton<AudioManager>.SP.PlayEnvironmentAudio(pregame: false);
			StopCoroutine("CountdownSequence");
			StartCoroutine(CountdownSequence());
		}
		else
		{
			Debug.Log("too late for level start");
			HenkUtils.DisablePreGameCamera();
		}
	}

	public void Initialize()
	{
		if (HenkUtils.IsInALevel())
		{
			if ((bool)Singleton<PlayerManager>.SP.GetPlayer())
			{
				Singleton<PlayerManager>.SP.ResetPlayer(Singleton<PlayerManager>.SP.GetPlayer(), hard: true);
			}
			HenkUtils.ResetLevel();
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
			Camera.main.GetComponent<PlatformerCamera>().OnReset();
			Singleton<PermaGUI>.SP.FadeInOrOut(1.5f, fadeIn: true);
			Singleton<AudioManager>.SP.PlayIngameTheme();
			Singleton<CheckpointManager>.SP.InitializeCheckpoints();
			Singleton<Scoreboard>.SP.UpdateLevelDetails();
			if (!Singleton<CheckpointManager>.SP.FirstTimeLevelRuns && Singleton<MultiManager>.SP.WarmupTimeLeft() <= 0f)
			{
				StartCoroutine("CountdownSequence");
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

	public void KillCountdownSequence()
	{
		StopCoroutine("CountdownSequence");
	}

	private IEnumerator CountdownSequence()
	{
		Singleton<CheckpointManager>.SP.GetStartline().GetComponent<StartLine>().DoCountDown();
		countingDown = true;
		float countDownTime = GetCountDownTime();
		AudioController.Play("Ready");
		Singleton<PermaGUI>.SP.labelGo.text = string.Empty;
		Singleton<PermaGUI>.SP.labelReady.text = "Ready...";
		yield return new WaitForSeconds(countDownTime);
		AudioController.Play("Start");
		Singleton<PermaGUI>.SP.labelGo.text = "Go!";
		Singleton<PermaGUI>.SP.labelReady.text = string.Empty;
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGameMultiplayer));
		countingDown = false;
		Singleton<CheckpointManager>.SP.FirstTimeLevelRuns = false;
		yield return new WaitForSeconds(0.7f);
		Singleton<PermaGUI>.SP.labelGo.text = string.Empty;
	}
}
