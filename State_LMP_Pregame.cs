using System.Collections;
using UnityEngine;

public class State_LMP_Pregame : GameState
{
	public bool countingDown;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP_Pregame, "none");
		Initialize();
	}

	public override void OnDeactivate()
	{
		StopCoroutine("StartGameAfterDelay");
	}

	public override void OnUpdate()
	{
	}

	public void LevelFileLoadedLocalMultiplayer()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LocalMultiplayer || !HenkUtils.IsInALevel())
		{
			return;
		}
		Object.FindObjectOfType<PreGameCamera>().Initialize();
		for (int i = 0; i < 4; i++)
		{
			if (Singleton<LocalMultiManager>.SP.IsPlayerActive(i))
			{
				Singleton<PlayerManager>.SP.SpawnPlayer(multiPlayer: false, i);
			}
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_LMP_Pregame));
	}

	public void Initialize()
	{
		if (!HenkUtils.IsInALevel())
		{
			return;
		}
		Singleton<PlayerManager>.SP.ResetAllPlayers();
		HenkUtils.ResetLevel();
		Camera.main.SendMessage("OnReset", SendMessageOptions.DontRequireReceiver);
		foreach (PlatformerCamera playerCamera in Singleton<LocalMultiManager>.SP.playerCameras)
		{
			playerCamera.OnReset();
		}
		Singleton<PermaGUI>.SP.FadeInOrOut(1.5f, fadeIn: true);
		Singleton<AudioManager>.SP.PlayIngameTheme();
		Singleton<CheckpointManager>.SP.InitializeCheckpoints();
		if (Singleton<CheckpointManager>.SP.FirstTimeLevelRuns)
		{
			HenkUtils.EnablePreGameCamera();
			StartCoroutine("StartGameAfterDelay");
		}
		else
		{
			StartCoroutine("CountdownSequence");
		}
	}

	private IEnumerator StartGameAfterDelay()
	{
		Singleton<PermaGUI>.SP.ToggleGhostLabels(state: false);
		yield return new WaitForSeconds(2.5f);
		StartGame();
	}

	public void StartGame()
	{
		if (!Camera.main.GetComponent<PlatformerCameraMultiplayer>())
		{
			Camera.main.gameObject.AddComponent<PlatformerCameraMultiplayer>();
		}
		HenkUtils.DisablePreGameCamera();
		Camera.main.GetComponent<PlatformerCamera>().enabled = false;
		if ((bool)Camera.main.GetComponent<DepthOfFieldScatter>())
		{
			Camera.main.GetComponent<DepthOfFieldScatter>().enabled = false;
		}
		Singleton<AudioManager>.SP.PlayEnvironmentAudio(pregame: false);
		StartCoroutine("CountdownSequence");
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
		Singleton<PermaGUI>.SP.ToggleGhostLabels(state: true);
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
		Singleton<GamestateManager>.SP.SetState(typeof(State_LMP_Ingame));
		countingDown = false;
		Singleton<CheckpointManager>.SP.FirstTimeLevelRuns = false;
		yield return new WaitForSeconds(0.7f);
		Singleton<PermaGUI>.SP.labelGo.text = string.Empty;
	}
}
