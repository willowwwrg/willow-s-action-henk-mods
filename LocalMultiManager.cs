using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalMultiManager : Singleton<LocalMultiManager>
{
	public const int MAXPLAYERS = 4;

	public string[] playerColorCodes = new string[4] { "[f14e17]", "[15d0d1]", "[85fa66]", "[ffcf0d]" };

	public string[] playerBrightColorCodes = new string[4] { "[FF0000]", "[00FFFF]", "[00FF00]", "[FFFF00]" };

	public List<PlatformerCamera> playerCameras = new List<PlatformerCamera>();

	private ulong[] defautlLevelRotation = new ulong[9] { 34uL, 3uL, 2uL, 14uL, 4uL, 16uL, 32uL, 27uL, 7uL };

	public ulong[] levelRotation;

	private int currentLevelInRotation;

	private int attemptsLeft;

	public int attemptsPerLevel = 3;

	public bool allowCheckpointRespawn = true;

	public LMPPlayerInfo[] playerInfo = new LMPPlayerInfo[4];

	public bool gameOver;

	public void Awake()
	{
		ClearPlayers();
	}

	public void ClearPlayers()
	{
		for (int i = 0; i < 4; i++)
		{
			playerInfo[i] = new LMPPlayerInfo();
		}
	}

	public void Start()
	{
		levelRotation = defautlLevelRotation;
		string text = Singleton<PlayerPrefsManager>.SP.GetString("LMPSettings_LEVELROTATION", string.Empty);
		if (!(text == string.Empty))
		{
			List<string> list = new List<string>(text.Split(','));
			List<ulong> list2 = new List<ulong>();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(Convert.ToUInt64(list[i]));
			}
			levelRotation = list2.ToArray();
		}
	}

	public void SetLevelRotation(List<Level> levels)
	{
		if (levels.Count == 0)
		{
			levelRotation = defautlLevelRotation;
			return;
		}
		List<ulong> list = new List<ulong>();
		foreach (Level level in levels)
		{
			list.Add(level.GetLevelID());
		}
		levelRotation = list.ToArray();
	}

	public void StartGame()
	{
		ClearScores();
		gameOver = false;
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.LocalMultiplayer);
		currentLevelInRotation = 0;
		LoadCurrentLevelInRotation();
	}

	public void ClearScores()
	{
		for (int i = 0; i < 4; i++)
		{
			playerInfo[i].playerScore = 0;
		}
	}

	public void ClearLevelData()
	{
		for (int i = 0; i < 4; i++)
		{
			playerInfo[i].finishedNumber = 0;
			playerInfo[i].numResets = 0;
		}
	}

	public bool IsPlayerActive(int player)
	{
		return playerInfo[player].selectedCharacter.character != CharacterSelect.Characters.None;
	}

	public ulong GetLevelInRotation(int offsetFromCurrent = 0)
	{
		int num = currentLevelInRotation + offsetFromCurrent;
		num %= levelRotation.Length;
		return levelRotation[num];
	}

	public int GetAttemptsLeft()
	{
		return attemptsLeft;
	}

	public void RetryOrNextLevel()
	{
		attemptsLeft--;
		if (attemptsLeft <= 0)
		{
			currentLevelInRotation++;
			if (currentLevelInRotation != levelRotation.Length)
			{
				LoadCurrentLevelInRotation();
			}
		}
		else
		{
			ClearLevelData();
			AudioController.Play("Reset");
			Singleton<GamestateManager>.SP.SetState(typeof(State_LMP_Pregame));
		}
	}

	public bool IsGameOver()
	{
		if (attemptsLeft - 1 <= 0 && currentLevelInRotation + 1 == levelRotation.Length)
		{
			gameOver = true;
		}
		return gameOver;
	}

	public void LoadCurrentLevelInRotation()
	{
		ClearLevelData();
		attemptsLeft = attemptsPerLevel;
		ulong levelInRotation = GetLevelInRotation();
		MonoBehaviour.print("loading level with ID " + levelInRotation);
		Singleton<LevelBatchManager>.SP.LoadLevelScenelessFromID(levelInRotation);
	}

	public bool IsPlayerAlive(GameObject player)
	{
		return player.GetComponent<PlayerGraphics>().IsAlive();
	}

	public void KillPlayer(GameObject player, bool addParticleEffect = false)
	{
		if ((bool)player.GetComponent<GrapplingHook>() && player.GetComponent<GrapplingHook>().IsEnabled())
		{
			player.GetComponent<GrapplingHook>().DisableAbility(forced: true);
		}
		player.GetComponent<PlayerAudio>().enabled = false;
		player.GetComponent<RaycastCollider>().enabled = false;
		player.GetComponent<PlayerWaypointManager>().enabled = false;
		player.GetComponent<PlatformerPhysics>().enabled = false;
		player.GetComponent<PlayerGraphics>().enabled = false;
		player.GetComponent<LMPTaunter>().enabled = false;
		player.GetComponent<PlayerAudio>().Mute();
		if (addParticleEffect)
		{
			(UnityEngine.Object.Instantiate(Resources.Load("Particles/PickUpEffect")) as GameObject).transform.position = player.transform.position;
		}
		Checkpoint nextCheckpoint = Singleton<CheckpointManager>.SP.GetNextCheckpoint(GetFurthestCheckpointReached());
		if (nextCheckpoint != null && nextCheckpoint != Singleton<CheckpointManager>.SP.Finishline && allowCheckpointRespawn)
		{
			Vector3 extraOffset = new Vector3(2.35f, 0f, 0f);
			extraOffset.x -= 1.5f * (float)nextCheckpoint.peopleWaitingAtThisCheckpoint.Count;
			Singleton<PlayerManager>.SP.PlacePlayerAtCheckpoint(nextCheckpoint, player, extraOffset);
			nextCheckpoint.peopleWaitingAtThisCheckpoint.Add(player);
		}
		else
		{
			player.SetActive(value: false);
		}
		player.GetComponent<PlayerGraphics>().KillPlayer();
		CheckIfAllPlayersFinishedOrDead();
	}

	public void SetPlayerAlive(GameObject player)
	{
		if (!player.activeSelf)
		{
			player.SetActive(value: true);
		}
		player.GetComponent<PlayerAudio>().enabled = true;
		player.GetComponent<RaycastCollider>().enabled = true;
		player.GetComponent<PlayerWaypointManager>().enabled = true;
		player.GetComponent<PlatformerPhysics>().enabled = true;
		player.GetComponent<PlayerGraphics>().enabled = true;
		player.GetComponent<LMPTaunter>().enabled = true;
		player.GetComponent<PlayerGraphics>().SetPlayerAlive();
	}

	public GameObject[] GetAlivePlayers()
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject in localPlayers)
		{
			if (IsPlayerAlive(gameObject))
			{
				list.Add(gameObject);
			}
		}
		return list.ToArray();
	}

	public void PlayerFinished(int player)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (playerInfo[i].finishedNumber > num)
			{
				num = playerInfo[i].finishedNumber;
			}
		}
		playerInfo[player].finishedNumber = num + 1;
		MonoBehaviour.print("player " + player + " finished with rank " + playerInfo[player].finishedNumber);
		CheckIfAllPlayersFinishedOrDead();
	}

	public GameObject GetLocalPlayer(int playerNum)
	{
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject in localPlayers)
		{
			if (gameObject.GetComponent<PlatformerController>().localPlayerNumber == playerNum)
			{
				return gameObject;
			}
		}
		return null;
	}

	public void RespawnAllDeadPlayers(Checkpoint checkpoint, GameObject playerToFollow)
	{
		bool flag = false;
		Vector3 position = Vector3.zero;
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject in localPlayers)
		{
			if (!IsPlayerAlive(gameObject))
			{
				SetPlayerAlive(gameObject);
				gameObject.GetComponent<PlayerWaypointManager>().SetMostRecentCheckpoint(checkpoint, 0f);
				Singleton<PlayerManager>.SP.ResetPlayer(gameObject, hard: false);
				playerInfo[gameObject.GetComponent<PlatformerController>().localPlayerNumber].numResets++;
				gameObject.GetComponent<PlayerWaypointManager>().CopyLMPData(playerToFollow);
				if (checkpoint.peopleWaitingAtThisCheckpoint.IndexOf(gameObject) != -1)
				{
					Vector3 extraOffset = new Vector3(2.35f, 0f, 0f);
					extraOffset.x -= 1.5f * (float)checkpoint.peopleWaitingAtThisCheckpoint.IndexOf(gameObject);
					Singleton<PlayerManager>.SP.PlacePlayerAtCheckpoint(checkpoint, gameObject, extraOffset);
				}
				Vector3 velocity = playerToFollow.GetComponent<RaycastCollider>().velocity;
				float num = 0.75f;
				float num2 = 10f;
				if (velocity.magnitude > num2 / (1f - num))
				{
					gameObject.GetComponent<RaycastCollider>().velocity = velocity * num;
				}
				else if (velocity.magnitude > num2)
				{
					gameObject.GetComponent<RaycastCollider>().velocity = velocity.normalized * (velocity.magnitude - num2);
				}
				gameObject.GetComponent<RaycastCollider>().velocity.y = 0f;
				if (playerToFollow.GetComponent<GrapplingHook>().enabled)
				{
					gameObject.GetComponent<GrapplingHook>().enabled = true;
				}
				flag = true;
				position = gameObject.transform.position;
			}
		}
		checkpoint.peopleWaitingAtThisCheckpoint.Clear();
		if (flag)
		{
			(UnityEngine.Object.Instantiate(Resources.Load("Particles/PickUpEffect")) as GameObject).transform.position = position;
			AudioController.Play("StartlineGunshot", Camera.main.transform);
		}
	}

	public void CheckIfAllPlayersFinishedOrDead()
	{
		bool flag = true;
		bool flag2 = true;
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject in localPlayers)
		{
			int localPlayerNumber = gameObject.GetComponent<PlatformerController>().localPlayerNumber;
			if (IsPlayerAlive(gameObject))
			{
				flag = false;
				if (playerInfo[localPlayerNumber].finishedNumber == 0)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			AudioController.Play("Reset");
			if (allowCheckpointRespawn)
			{
				Checkpoint furthestCheckpointReached = GetFurthestCheckpointReached();
				if (furthestCheckpointReached == Singleton<CheckpointManager>.SP.Startline)
				{
					Singleton<GamestateManager>.SP.SetState(typeof(State_LMP_Pregame));
					return;
				}
				localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
				foreach (GameObject gameObject2 in localPlayers)
				{
					SetPlayerAlive(gameObject2);
					gameObject2.GetComponent<PlayerWaypointManager>().SetMostRecentCheckpoint(furthestCheckpointReached, 0f);
					Singleton<PlayerManager>.SP.ResetPlayer(gameObject2, hard: false);
					playerInfo[gameObject2.GetComponent<PlatformerController>().localPlayerNumber].numResets++;
					gameObject2.GetComponent<PlayerWaypointManager>().CopyLMPData(furthestCheckpointReached);
				}
				Checkpoint nextCheckpoint = Singleton<CheckpointManager>.SP.GetNextCheckpoint(furthestCheckpointReached);
				if ((bool)nextCheckpoint)
				{
					nextCheckpoint.peopleWaitingAtThisCheckpoint.Clear();
				}
				Camera.main.GetComponent<PlatformerCameraMultiplayer>().OnRespawn();
			}
			else
			{
				Singleton<GamestateManager>.SP.SetState(typeof(State_LMP_Pregame));
			}
		}
		else if (flag2)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_LMP_Postgame));
		}
	}

	public Checkpoint GetFurthestCheckpointReached()
	{
		Checkpoint result = null;
		int num = -1;
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Length; i++)
		{
			Checkpoint mostRecentCheckpoint = localPlayers[i].GetComponent<PlayerWaypointManager>().GetMostRecentCheckpoint();
			int checkpointNumber = Singleton<CheckpointManager>.SP.GetCheckpointNumber(mostRecentCheckpoint);
			if (checkpointNumber > num)
			{
				num = checkpointNumber;
				result = mostRecentCheckpoint;
			}
		}
		return result;
	}

	public Checkpoint GetLeastFurthestCheckpointReached(GameObject exclude = null)
	{
		Checkpoint result = null;
		int num = 10000;
		GameObject[] alivePlayers = GetAlivePlayers();
		foreach (GameObject gameObject in alivePlayers)
		{
			if (!(gameObject == exclude))
			{
				Checkpoint mostRecentCheckpoint = gameObject.GetComponent<PlayerWaypointManager>().GetMostRecentCheckpoint();
				int checkpointNumber = Singleton<CheckpointManager>.SP.GetCheckpointNumber(mostRecentCheckpoint);
				if (checkpointNumber < num)
				{
					num = checkpointNumber;
					result = mostRecentCheckpoint;
				}
			}
		}
		return result;
	}

	public int GetPlayerRank(int player)
	{
		int[] playersSortedByScore = GetPlayersSortedByScore();
		for (int i = 0; i < playersSortedByScore.Length; i++)
		{
			if (player == playersSortedByScore[i])
			{
				return i;
			}
		}
		return -1;
	}

	public int[] GetPlayersSortedByScore()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			int num = -1;
			int num2 = -1;
			for (int j = 0; j < 4; j++)
			{
				if (IsPlayerActive(j) && !list.Contains(j) && playerInfo[j].playerScore > num)
				{
					num2 = j;
					num = playerInfo[j].playerScore;
				}
			}
			if (num2 != -1)
			{
				list.Add(num2);
			}
		}
		return list.ToArray();
	}
}
