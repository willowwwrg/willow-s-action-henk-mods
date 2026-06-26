using Steamworks;
using UnityEngine;

public class State_EndGameMultiplayer : GameState
{
	public GUI_EndGameMultiplayer guiScript;

	private bool abortDownloadingScores;

	private MultiplayerPodium podium;

	public override void OnActivate()
	{
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.Local, state: false);
		Singleton<Scoreboard>.SP.gameOver.enabled = true;
		abortDownloadingScores = false;
		Singleton<PermaGUI>.SP.KillCountdownLabels();
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_EndGameMultiplayer, "none");
		AudioController.Play("mul_over");
		AudioController.Play("crowd");
		Singleton<Scoreboard>.SP.forceScoreboardVisible = true;
		Singleton<InGameMenu>.SP.ToggleMenu(toggle: false);
		if (PhotonNetwork.player.customProperties["score"] != null && (int)Singleton<Scoreboard>.SP.ranking == 1 && Singleton<PlayerManager>.SP.GetAllPlayers().Length > 1)
		{
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.WinMultiplayer);
		}
		podium = Object.FindObjectOfType<MultiplayerPodium>();
		if (podium == null)
		{
			Debug.LogError("Couldn't find podium in level style " + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle);
			return;
		}
		Camera.main.GetComponent<PlatformerCamera>().enabled = false;
		Camera.main.GetComponent<HandyCam>().SetCameraTarget(podium.camera.transform, snapToTarget: true);
		if ((bool)Camera.main.GetComponent<DepthOfFieldScatter>())
		{
			Camera.main.GetComponent<DepthOfFieldScatter>().enabled = false;
		}
		GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
		foreach (GameObject gameObject in allPlayers)
		{
			gameObject.GetComponent<RaycastCollider>().enabled = false;
			gameObject.GetComponent<PlayerWaypointManager>().enabled = false;
			gameObject.GetComponent<PlatformerPhysics>().enabled = false;
			gameObject.GetComponent<PlayerGraphics>().enabled = false;
			gameObject.GetComponent<PlayerNetworking>().enabled = false;
			Singleton<PlayerManager>.SP.ResetPlayer(gameObject, hard: true);
			PhotonPlayer owner = gameObject.GetComponent<PhotonView>().owner;
			if (owner.customProperties["score"] != null)
			{
				int playerRank = Singleton<Scoreboard>.SP.GetPlayerRank(owner);
				if (playerRank == 1)
				{
					gameObject.GetComponent<PlayerGraphics>().GoToPodium(podium.spot1);
				}
				if (playerRank == 2)
				{
					gameObject.GetComponent<PlayerGraphics>().GoToPodium(podium.spot2);
				}
				if (playerRank == 3)
				{
					gameObject.GetComponent<PlayerGraphics>().GoToPodium(podium.spot3);
				}
				if (playerRank > 0 && playerRank < 4 && gameObject.GetComponent<PhotonView>().isMine)
				{
					gameObject.GetComponent<PlayerNetworking>().enabled = true;
				}
			}
		}
	}

	public override void OnDeactivate()
	{
		Singleton<Scoreboard>.SP.forceScoreboardVisible = false;
		Singleton<Scoreboard>.SP.scoreBoard.SetActive(value: false);
		Singleton<Scoreboard>.SP.gameOver.enabled = false;
		Singleton<InGameMenu>.SP.ToggleMenu(toggle: false);
		Camera.main.GetComponent<HandyCam>().SetCameraTarget(null);
		Camera.main.GetComponent<CameraEffectsManager>().UpdateComponents();
	}

	public override void OnUpdate()
	{
		Singleton<MultiManager>.SP.StartNewLevel();
	}

	public void HenkSWSubmitScoreMultiplayerResult(bool result)
	{
		if (!result)
		{
			abortDownloadingScores = true;
		}
	}

	public void HenkSWDownloadScoresForUsers(LeaderboardEntry_t[] entries)
	{
		if (entries != null)
		{
			Singleton<Scoreboard>.SP.SetEndGameScores(entries);
		}
	}
}
