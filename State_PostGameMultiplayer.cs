using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class State_PostGameMultiplayer : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_PostGameMultiplayer, "none");
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.Local, state: false);
		Hashtable customProperties = PhotonNetwork.player.customProperties;
		float finishTime = Singleton<CheckpointManager>.SP.GetFinishTime();
		if (customProperties["score"] == null || finishTime < (float)customProperties["score"])
		{
			customProperties["score"] = finishTime;
			PhotonNetwork.SetPlayerCustomProperties(customProperties);
			List<float> checkpointTimes = Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerWaypointManager>().GetCheckpointTimes();
			Singleton<CheckpointManager>.SP.SetOpponentTimes(checkpointTimes, finishTime);
		}
		Singleton<Scoreboard>.SP.forceScoreboardVisible = true;
	}

	public override void OnDeactivate()
	{
		Singleton<Scoreboard>.SP.forceScoreboardVisible = false;
	}

	public override void OnUpdate()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Retry))
		{
			AudioController.Play("ButtonBackwards");
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGameMultiplayer));
		}
	}
}
