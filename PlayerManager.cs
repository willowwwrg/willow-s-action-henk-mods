using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	private List<GameObject> localPlayers = new List<GameObject>();

	private List<GameObject> ghostPlayers = new List<GameObject>();

	private List<GameObject> networkPlayers = new List<GameObject>();

	private GameObject[] cachedAllPlayers;
	private bool allPlayersDirty = true;

	[NonSerialized]
	public GhostType ghostType;

	[HideInInspector]
	public bool ghostSet;

	private GhostType currentGhost;

	public GameObject[] GetAllPlayers()
	{
		if (allPlayersDirty)
		{
			List<GameObject> list = new List<GameObject>();
			list.AddRange(localPlayers);
			list.AddRange(ghostPlayers);
			list.AddRange(networkPlayers);
			cachedAllPlayers = list.ToArray();
			allPlayersDirty = false;
		}
		return cachedAllPlayers;
	}

	public GameObject[] GetNetworkPlayers()
	{
		return networkPlayers.ToArray();
	}

	public GameObject[] GetAllGhosts()
	{
		return ghostPlayers.ToArray();
	}

	public GameObject[] GetLocalPlayers()
	{
		return localPlayers.ToArray();
	}

	public bool IsGhost(GameObject player)
	{
		return ghostPlayers.Contains(player);
	}

	public bool IsLocalPlayer(GameObject player)
	{
		return localPlayers.Contains(player);
	}

	public bool IsNetworkedPlayer(GameObject player)
	{
		return networkPlayers.Contains(player);
	}

	public GameObject GetPlayer()
	{
		if (localPlayers.Count == 0)
		{
			return null;
		}
		return localPlayers[0];
	}

	public GameObject GetGhost()
	{
		if (ghostPlayers.Count == 0)
		{
			return null;
		}
		return ghostPlayers[0];
	}

	public void RemoveGhosts()
	{
		foreach (GameObject ghostPlayer in ghostPlayers)
		{
			UnityEngine.Object.Destroy(ghostPlayer);
		}
		ghostPlayers.Clear();
		allPlayersDirty = true;
	}

	public void AddNetworkPlayer(GameObject player)
	{
		networkPlayers.Add(player);
		allPlayersDirty = true;
	}

	public void RemoveNetworkPlayer(GameObject player)
	{
		MonoBehaviour.print("removing network player");
		networkPlayers.Remove(player);
		allPlayersDirty = true;
	}

	public void RemoveNetworkPlayers()
	{
		networkPlayers.Clear();
		allPlayersDirty = true;
	}

	public void RemovePlayer(GameObject player)
	{
		MonoBehaviour.print("removing " + player);
		if (localPlayers.Contains(player))
			localPlayers.Remove(player);
		if (networkPlayers.Contains(player))
			networkPlayers.Remove(player);
		if (ghostPlayers.Contains(player))
			ghostPlayers.Remove(player);
		allPlayersDirty = true;
	}

	public void SpawnPlayer(bool multiPlayer = false, int localPlayerNumber = -1)
	{
		GameObject gameObject;
		if (!multiPlayer)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load("Player")) as GameObject;
			gameObject.GetComponent<PlayerNetworking>().enabled = false;
			if (localPlayerNumber != -1)
			{
				gameObject.GetComponent<PlatformerController>().localPlayerNumber = localPlayerNumber;
				UnityEngine.Object.Destroy(gameObject.GetComponent<ReplayRecorder>());
				string text = Singleton<LocalMultiManager>.SP.playerBrightColorCodes[localPlayerNumber];
				SpawnNameLabel(gameObject, text + "P" + (localPlayerNumber + 1) + "[-]");
			}
		}
		else
		{
			gameObject = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
		}
		gameObject.name = "Player";
		localPlayers.Add(gameObject);
		allPlayersDirty = true;
		PlacePlayerAtCheckpoint(Singleton<CheckpointManager>.SP.GetStartline(), gameObject);
		ResetPlayer(gameObject, hard: true);
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelCodeOrID() == 97)
		{
			gameObject.GetComponent<PlatformerPhysics>().jumping.gravity = 25f;
		}
	}

	public GameObject SpawnGhost(GhostType spawnGhostType, ulong playerID = 0uL)
	{
		if (spawnGhostType != GhostType.CustomID)
		{
			ghostSet = true;
			ghostType = spawnGhostType;
		}
		if (spawnGhostType == GhostType.None)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Player")) as GameObject;
		ghostPlayers.Add(gameObject);
		allPlayersDirty = true;
		gameObject.name = "Ghost";
		if (ghostType != GhostType.Challenger)
		{
			UnityEngine.Object.Destroy(gameObject.GetComponent<PlayerAudio>());
		}
		UnityEngine.Object.Destroy(gameObject.GetComponent<ReplayRecorder>());
		gameObject.GetComponent<GrapplingHook>().enabled = false;
		gameObject.GetComponent<PlayerNetworking>().enabled = false;
		gameObject.GetComponent<PlatformerController>().isExternalControlled = true;
		gameObject.GetComponent<PlayerGraphics>().hasGhostGraphics = true;
		SpawnNameLabel(gameObject, string.Empty);
		if (gameObject == null)
		{
			Debug.LogError("Error trying to spawn ghost.");
			return null;
		}
		gameObject.GetComponent<ReplayController>().LoadReplay(spawnGhostType, playerID);
		PlacePlayerAtCheckpoint(Singleton<CheckpointManager>.SP.GetStartline(), gameObject);
		ResetPlayer(gameObject, hard: true);
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelCodeOrID() == 97)
		{
			gameObject.GetComponent<PlatformerPhysics>().jumping.gravity = 25f;
		}
		return gameObject;
	}

	public void SpawnNameLabel(GameObject ownerObject, string name = "")
	{
		GameObject gameObject = GameObject.Find("Anchor_GhostLabels");
		if (gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(Singleton<ReplayManager>.SP.replayNameLabelObject) as GameObject;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject2.GetComponent<ReplayName>().SetTargetObject(ownerObject);
			gameObject2.GetComponent<ReplayName>().SetGhostName(name);
			ownerObject.GetComponent<PlayerGraphics>().ghostNameLabel = gameObject2.GetComponent<ReplayName>();
		}
	}

	public void TogglePlayerControls(GameObject player, bool state)
	{
		if (IsGhost(player))
		{
			if (state)
			{
				player.GetComponent<PlatformerController>().GiveControl();
				player.GetComponent<ReplayController>().Play();
			}
			else
			{
				player.GetComponent<PlatformerController>().RemoveControl();
				player.GetComponent<ReplayController>().Stop();
			}
		}
		else if (IsLocalPlayer(player))
		{
			if (state)
			{
				player.GetComponent<PlatformerController>().GiveControl();
			}
			else
			{
				player.GetComponent<PlatformerController>().RemoveControl();
			}
		}
	}

	public void TogglePlayerControls(PlayerType type, bool state)
	{
		switch (type)
		{
		case PlayerType.Ghost:
		{
			foreach (GameObject ghostPlayer in ghostPlayers)
			{
				TogglePlayerControls(ghostPlayer, state);
			}
			break;
		}
		case PlayerType.Local:
		{
			foreach (GameObject localPlayer in localPlayers)
			{
				TogglePlayerControls(localPlayer, state);
			}
			break;
		}
		case PlayerType.All:
		{
			GameObject[] allPlayers = GetAllPlayers();
			foreach (GameObject player in allPlayers)
			{
				TogglePlayerControls(player, state);
			}
			break;
		}
		}
	}

	public void PlacePlayerAtCheckpoint(Checkpoint checkpoint, GameObject player, [Optional] Vector3 extraOffset)
	{
		if (checkpoint == null)
		{
			checkpoint = Singleton<CheckpointManager>.SP.GetStartline();
		}
		if (player == null)
		{
			Debug.LogError("Trying to place player.");
			return;
		}
		PlayerGraphics pg = player.GetComponent<PlayerGraphics>();
		PlatformerController pc = player.GetComponent<PlatformerController>();
		pg.ToggleParticles(toggle: false);
		Vector3 direction = checkpoint.extraOffsetToSpawnPosition + extraOffset;
		if (pc.localPlayerNumber != -1)
		{
			if (checkpoint == Singleton<CheckpointManager>.SP.Startline)
			{
				int playerRank = Singleton<LocalMultiManager>.SP.GetPlayerRank(pc.localPlayerNumber);
				int num = Singleton<PlayerManager>.SP.GetAllPlayers().Length - playerRank - 1;
				direction = checkpoint.LMPextraOffsetToSpawnPosition + checkpoint.LMPExtraOffsetPerPlayer * num;
			}
			else
			{
				direction += checkpoint.LMPExtraOffsetAtCheckpoint;
			}
		}
		direction.x *= checkpoint.transform.localScale.x;
		player.transform.position = checkpoint.transform.position + checkpoint.transform.TransformDirection(direction);
		SplineFollow component = checkpoint.GetComponent<SplineFollow>();
		PlayerWaypointManager component2 = player.GetComponent<PlayerWaypointManager>();
		if ((bool)component && (bool)component2)
		{
			component2.SetOffset(component.splineOffset.x + direction.x);
			component2.RotateToWaypoint(snap: true);
		}
		pg.SnapPhysicsInterpolator();
		pg.SetDirection(90f * Mathf.Sign(checkpoint.transform.localScale.x));
		pg.ToggleParticles(toggle: true);
		player.GetComponent<PlatformerPhysics>().UpdatePrediction();
	}

	public void ResetAllPlayers()
	{
		GameObject[] allPlayers = GetAllPlayers();
		foreach (GameObject player in allPlayers)
		{
			ResetPlayer(player, hard: true);
		}
	}

	public void ResetPlayer(GameObject player, bool hard)
	{
		PlatformerController pc = player.GetComponent<PlatformerController>();
		if (pc.localPlayerNumber != -1 && !Singleton<LocalMultiManager>.SP.IsPlayerAlive(player))
		{
			Singleton<LocalMultiManager>.SP.SetPlayerAlive(player);
		}
		if (IsGhost(player) && hard)
		{
			player.GetComponent<ReplayController>().CheckForReloadReplay();
		}
		else if (IsLocalPlayer(player) && !hard)
		{
			if (!Singleton<CheckpointManager>.SP.FirstCheckpointPassed() && Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.Replay && Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LocalMultiplayer)
			{
				if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer || Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
				{
					Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
				}
				else
				{
					Singleton<GamestateManager>.SP.SetState(typeof(State_PreGameMultiplayer));
				}
				return;
			}
			ReplayRecorder rr = player.GetComponent<ReplayRecorder>();
			if ((bool)rr)
			{
				rr.insertResetFrame = true;
			}
		}
		if (hard)
		{
			if (!IsNetworkedPlayer(player))
			{
				TogglePlayerControls(player, state: false);
			}
			player.SendMessage("OnReset", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			player.SendMessage("OnRespawn", SendMessageOptions.DontRequireReceiver);
		}
		PlacePlayerAtCheckpoint(player.GetComponent<PlayerWaypointManager>().GetMostRecentCheckpoint(), player);
		if (IsLocalPlayer(player))
		{
			if (!hard)
			{
				Camera.main.GetComponent<PlatformerCamera>().OnRespawn();
			}
			SlomoTrigger[] array = UnityEngine.Object.FindObjectsOfType<SlomoTrigger>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnReset();
			}
			Camera.main.GetComponent<CameraEffectsManager>().SetTargetFogAmount(0f, force: true);
		}
	}
}
