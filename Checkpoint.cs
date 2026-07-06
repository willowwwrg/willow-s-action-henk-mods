using System;
using System.Collections.Generic;
using Henk;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	private bool isPassed;

	public bool debugCheckpoint;

	private float timeOfPassing = 999f;

	public int checkpointNumber;

	private float mostRecentTimeDifference;

	[NonSerialized]
	public Vector3 extraOffsetToSpawnPosition = new Vector3(2.5f, 2.9f, -1f);

	[NonSerialized]
	public Vector3 LMPExtraOffsetPerPlayer = new Vector3(-1.5f, 0f, 0f);

	[NonSerialized]
	public Vector3 LMPextraOffsetToSpawnPosition = new Vector3(3f, 2.9f, -1f);

	[NonSerialized]
	public Vector3 LMPExtraOffsetAtCheckpoint = new Vector3(-3.5f, 0f, 0f);

	[NonSerialized]
	public Vector3 extraOffsetToRespawnIndicator = new Vector3(1.4f, 9.4f, 0f);

	private CheckpointFX checkpointFX;

	private CheckpointLights lights;

	[HideInInspector]
	public float positionAlongTrackWhenPassed;

	[HideInInspector]
	public LMPCheckpoint LMPCheckpointWhenPassed;

	public GameObject cpNumLabel;

	[HideInInspector]
	public List<GameObject> peopleWaitingAtThisCheckpoint = new List<GameObject>();

	private void Awake()
	{
		checkpointFX = GetComponent<CheckpointFX>();
	}

	private void Start()
	{
		if (IsRegularCheckpoint())
		{
			lights = GetComponentInChildren<CheckpointLights>();
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGameLevelEditor)))
		{
			return;
		}
		GameObject gameObject = col.transform.root.gameObject;
		if (Singleton<PlayerManager>.SP.IsLocalPlayer(gameObject))
		{
			if (gameObject.GetComponent<PlatformerController>().localPlayerNumber != -1)
			{
				if (Singleton<LocalMultiManager>.SP.IsPlayerAlive(gameObject))
				{
					TriggerCheckpointLMP(gameObject);
				}
			}
			else if (!isPassed)
			{
				TriggerCheckpoint();
			}
		}
		else if (Singleton<PlayerManager>.SP.IsGhost(gameObject) || Singleton<PlayerManager>.SP.IsNetworkedPlayer(gameObject))
		{
			TriggerCheckpointGhost(gameObject);
		}
	}

	public void ObjectPlaced()
	{
		if (IsRegularCheckpoint())
		{
			List<Checkpoint> list = new List<Checkpoint>();
			list.AddRange(Singleton<CheckpointManager>.SP.Checkpoints);
			if (!list.Contains(this))
			{
				list.Add(this);
			}
			Singleton<CheckpointManager>.SP.Checkpoints = list.ToArray();
			Singleton<CheckpointManager>.SP.UpdateCheckpointNumbers();
		}
	}

	public void ObjectDestroyed()
	{
		if (IsRegularCheckpoint())
		{
			List<Checkpoint> list = new List<Checkpoint>();
			list.AddRange(Singleton<CheckpointManager>.SP.Checkpoints);
			list.Remove(this);
			Singleton<CheckpointManager>.SP.Checkpoints = list.ToArray();
			Singleton<CheckpointManager>.SP.UpdateCheckpointNumbers();
		}
	}

	private void Update()
	{
		if (!(cpNumLabel != null))
		{
			return;
		}
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGameLevelEditor)))
		{
			if (Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameLevelEditor>().takingScreenshot)
			{
				cpNumLabel.SetActive(value: false);
			}
			else
			{
				cpNumLabel.SetActive(value: true);
			}
		}
		else if (cpNumLabel.activeInHierarchy)
		{
			cpNumLabel.SetActive(value: false);
		}
	}

	public void SetNumber(int number)
	{
		checkpointNumber = number;
		if (cpNumLabel != null)
		{
			cpNumLabel.GetComponent<TextMesh>().text = "Checkpoint: " + number;
		}
	}

	private void TriggerCheckpointGhost(GameObject ghost)
	{
		Checkpoint mostRecentCheckpoint = ghost.GetComponent<PlayerWaypointManager>().GetMostRecentCheckpoint();
		if (!(mostRecentCheckpoint != null) || Singleton<CheckpointManager>.SP.GetCheckpointNumber(this) > Singleton<CheckpointManager>.SP.GetCheckpointNumber(mostRecentCheckpoint))
		{
			if (IsRegularCheckpoint())
			{
				timeOfPassing = Singleton<Stopwatch>.SP.GetCurrentTime();
				ghost.GetComponent<PlayerWaypointManager>().SetMostRecentCheckpoint(this, timeOfPassing);
			}
			else if (IsFinish())
			{
				Singleton<PlayerManager>.SP.TogglePlayerControls(ghost, state: false);
				ghost.GetComponent<ReplayController>().Stop();
			}
		}
	}

	public void TriggerCheckpoint()
	{
		isPassed = true;
		if (IsRegularCheckpoint())
		{
			timeOfPassing = Singleton<Stopwatch>.SP.GetCurrentTime();
			Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerWaypointManager>().SetMostRecentCheckpoint(this, timeOfPassing);
			bool flag = true;
			if (Singleton<CheckpointManager>.SP.HasOpponentCheckpointTimes())
			{
				mostRecentTimeDifference = Singleton<CheckpointManager>.SP.GetBestCheckpointTimeAtNode(checkpointNumber - 1) - timeOfPassing;
				if (mostRecentTimeDifference <= 0f)
				{
					flag = false;
				}
				if (Singleton<GUIManager>.SP.GetCurrentScreenName() == GUIManager.GUIScreens.GUIScreen_InGame)
				{
					Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGame>().ShowCheckpointTime(mostRecentTimeDifference);
				}
				if (Singleton<GUIManager>.SP.GetCurrentScreenName() == GUIManager.GUIScreens.GUIScreen_InGameMultiplayer)
				{
					Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameMultiplayer>().ShowCheckpointTime(mostRecentTimeDifference);
				}
			}
			if (flag)
			{
				AudioController.Play("Checkpoint_improved", base.transform);
				Singleton<AudioManager>.SP.PlayCharacterSuccess(Singleton<PlayerManager>.SP.GetPlayer(), 0.5f);
			}
			else
			{
				AudioController.Play("Checkpoint", base.transform);
				if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
				{
					float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, Singleton<PlayerManager>.SP.GetGhost().transform.position);
					float volume = Mathf.InverseLerp(30f, 10f, value);
					Singleton<AudioManager>.SP.PlayCharacterLaugh(Singleton<PlayerManager>.SP.GetGhost(), volume);
				}
			}
			checkpointFX.PlayFX(flag);
			AudioController.Play("checkpoint_cannonfire", base.transform);
			if ((bool)lights)
			{
				lights.TurnOnLights();
			}
			if (debugCheckpoint)
			{
				MonoBehaviour.print(base.gameObject.name + ": " + timeOfPassing);
			}
		}
		if (IsFinish())
		{
			Singleton<Stopwatch>.SP.StopTimer();
			float currentTime = Singleton<Stopwatch>.SP.GetCurrentTime();
			Singleton<CheckpointManager>.SP.Finish(currentTime);
			bool improved = true;
			if (Singleton<CheckpointManager>.SP.opponentFinishTime != 0f && currentTime >= Singleton<CheckpointManager>.SP.opponentFinishTime)
			{
				improved = false;
			}
			AudioController.Play("FinishFireworks");
			checkpointFX.PlayFX(improved);
		}
	}

	private void TriggerCheckpointLMP(GameObject player)
	{
		PlayerWaypointManager pwm = player.GetComponent<PlayerWaypointManager>();
		Checkpoint mostRecentCheckpoint = pwm.GetMostRecentCheckpoint();
		if (mostRecentCheckpoint != null && Singleton<CheckpointManager>.SP.GetCheckpointNumber(this) <= Singleton<CheckpointManager>.SP.GetCheckpointNumber(mostRecentCheckpoint))
		{
			return;
		}
		pwm.SetMostRecentCheckpoint(this, 0f);
		if (!isPassed)
		{
			positionAlongTrackWhenPassed = pwm.GetPositionAlongTrack();
			LMPCheckpointWhenPassed = pwm.GetCurrentLMPCheckpoint();
		}
		if (IsRegularCheckpoint())
		{
			if (!isPassed)
			{
				checkpointFX.PlayFX(improved: true);
				AudioController.Play("Checkpoint_improved", base.transform);
				AudioController.Play("checkpoint_cannonfire", base.transform);
				Singleton<AudioManager>.SP.PlayCharacterSuccess(player, 0.5f);
				if ((bool)lights)
				{
					lights.TurnOnLights();
				}
				if (Singleton<LocalMultiManager>.SP.allowCheckpointRespawn)
				{
					Singleton<LocalMultiManager>.SP.RespawnAllDeadPlayers(this, player);
				}
			}
		}
		else if (IsFinish())
		{
			if (!isPassed)
			{
				AudioController.Play("FinishFireworks");
				AudioController.Play("Finish_improved");
				checkpointFX.PlayFX(improved: true);
				Singleton<AudioManager>.SP.PlayCharacterVictory(player, 1.2f);
			}
			else
			{
				AudioController.Play("Finish");
			}
			Singleton<PlayerManager>.SP.TogglePlayerControls(player, state: false);
			Singleton<LocalMultiManager>.SP.PlayerFinished(player.GetComponent<PlatformerController>().localPlayerNumber);
		}
		isPassed = true;
	}

	private bool IsRegularCheckpoint()
	{
		if (base.gameObject.name == "StartLine")
		{
			return false;
		}
		if (base.gameObject.name == "Finishline")
		{
			return false;
		}
		if (Singleton<CheckpointManager>.SP.Startline != this)
		{
			return Singleton<CheckpointManager>.SP.Finishline != this;
		}
		return false;
	}

	private bool IsFinish()
	{
		return Singleton<CheckpointManager>.SP.Finishline == this;
	}

	public float GetTimeOfPassing()
	{
		return timeOfPassing;
	}

	public bool Passed()
	{
		return isPassed;
	}

	public void ResetCheckpoint()
	{
		if (checkpointFX != null)
		{
			checkpointFX.ResetFX();
		}
		mostRecentTimeDifference = 0f;
		timeOfPassing = 999f;
		isPassed = false;
		positionAlongTrackWhenPassed = 0f;
		LMPCheckpointWhenPassed = null;
		peopleWaitingAtThisCheckpoint.Clear();
	}
}
