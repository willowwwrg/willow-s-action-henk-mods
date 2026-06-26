using System;
using System.Collections.Generic;
using System.IO;
using Henk;
using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
	public Checkpoint[] Checkpoints;

	public Checkpoint Startline;

	public Checkpoint Finishline;

	private bool FinishlineCrossed;

	private float FinishTime;

	private List<float> opponentCheckpointTimes = new List<float>();

	[NonSerialized]
	public float opponentFinishTime;

	[HideInInspector]
	public bool FirstTimeLevelRuns = true;

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!(Startline == null))
		{
			Startline.SetNumber(0);
			for (int i = 0; i < Checkpoints.Length; i++)
			{
				Checkpoints[i].SetNumber(i + 1);
			}
			if (Finishline != null)
			{
				Finishline.SetNumber(Checkpoints.Length + 1);
			}
		}
	}

	private void Update()
	{
		if (!FinishlineCrossed)
		{
			return;
		}
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer || Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
		{
			if (!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)))
			{
				Singleton<ReplayRecorder>.SP.StopRecord();
				Singleton<HenkSWLeaderboards>.SP.SetLevelCompletedLeaderboardHandle();
				Singleton<HighscoreManager>.SP.SubmitScore(FinishTime);
				Singleton<GamestateManager>.SP.SetState(typeof(State_PostGame));
			}
			else
			{
				((State_PostGame)Singleton<GamestateManager>.SP.State).StartCoroutine("StartReplay");
			}
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_PostGameMultiplayer));
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Replay)
		{
			if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_ReplayMode)))
			{
				((State_ReplayMode)Singleton<GamestateManager>.SP.State).Finished();
			}
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_PostGameEditor));
		}
		FinishlineCrossed = false;
	}

	public void SetLevelEditorCPLabels()
	{
		for (int i = 0; i < Checkpoints.Length; i++)
		{
			if (Checkpoints[i].cpNumLabel != null)
			{
				Checkpoints[i].cpNumLabel.GetComponent<TextMesh>().text = "Checkpoint: " + i;
			}
		}
	}

	public void InitializeCheckpoints()
	{
		Checkpoint[] checkpoints = Checkpoints;
		foreach (Checkpoint obj in checkpoints)
		{
			obj.ResetCheckpoint();
			obj.GetComponentInChildren<CheckpointLights>().ResetLights();
		}
		Startline.ResetCheckpoint();
		if ((bool)Finishline)
		{
			Finishline.ResetCheckpoint();
		}
		FinishlineCrossed = false;
		Startline.GetComponent<StartLine>().ResetLights();
	}

	public void LoadPBCheckpointTimes()
	{
		ClearOpponentTimes();
		float num = 0f;
		List<float> list = new List<float>();
		string currentLevelNumAndName = Singleton<LevelBatchManager>.SP.GetCurrentLevelNumAndName();
		string path = Application.dataPath + "/Resources/../../PBreplays/replay" + currentLevelNumAndName + ".txt";
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop)
		{
			path = Application.dataPath + "/Resources/../../PBreplays/replayworkshop" + Singleton<LevelBatchManager>.SP.GetGUID() + ".txt";
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			path = Application.dataPath + "/Resources/../../PBreplays/replay_daily" + Singleton<MutatorManager>.SP.seedOfToday + ".txt";
		}
		if (File.Exists(path))
		{
			using StreamReader streamReader = new StreamReader(path);
			int num2 = 0;
			string text = streamReader.ReadLine();
			text = streamReader.ReadLine();
			if (HenkUtils.IntParse(text) == Singleton<ReplayManager>.SP.replayVersion)
			{
				text = streamReader.ReadLine();
				num = HenkUtils.FloatParse(text);
				while (num2 < 2)
				{
					text = streamReader.ReadLine();
					if (text == null || text == "walk")
					{
						num2 = 2;
					}
					if (num2 == 1)
					{
						list.Add(HenkUtils.FloatParse(text));
					}
					if (num2 == 0 && text == "cpdata")
					{
						num2 = 1;
					}
				}
				SetOpponentTimes(list, num);
				return;
			}
		}
		opponentFinishTime = Singleton<PlayerPrefsManager>.SP.GetHighscore(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
	}

	public void LevelEditorUpdate()
	{
		GameObject gameObject = GameObject.Find("Finishline");
		if (gameObject != null)
		{
			Finishline = gameObject.GetComponent<Checkpoint>();
			gameObject.transform.parent = base.gameObject.transform;
		}
		GameObject gameObject2 = GameObject.Find("StartLine");
		if (gameObject2 != null)
		{
			Startline = gameObject2.GetComponent<Checkpoint>();
			gameObject2.transform.parent = base.gameObject.transform;
		}
		Checkpoint[] array = UnityEngine.Object.FindObjectsOfType<Checkpoint>();
		foreach (Checkpoint checkpoint in array)
		{
			if (checkpoint.name == "Checkpoint")
			{
				checkpoint.transform.parent = base.gameObject.transform;
			}
		}
	}

	public void UpdateCheckpointNumbers()
	{
		for (int i = 0; i < Checkpoints.Length; i++)
		{
			Checkpoints[i].SetNumber(i + 1);
		}
	}

	public void SetOpponentTimes(List<float> opponentTimes, float finishTime)
	{
		if (opponentTimes.Count != Checkpoints.Length)
		{
			Debug.LogError("loaded CP times do not match number of checkpoints, removing opponent times");
			ClearOpponentTimes();
		}
		else
		{
			opponentCheckpointTimes.Clear();
			opponentCheckpointTimes.AddRange(opponentTimes);
			opponentFinishTime = finishTime;
		}
	}

	public void ClearOpponentTimes()
	{
		opponentCheckpointTimes.Clear();
		opponentFinishTime = 0f;
	}

	public float GetBestCheckpointTimeAtNode(int checkpointNumber)
	{
		return opponentCheckpointTimes[checkpointNumber];
	}

	public Checkpoint GetStartline()
	{
		return Startline;
	}

	public void Finish(float finishTime)
	{
		Singleton<Stopwatch>.SP.StopTimer();
		FinishlineCrossed = true;
		FinishTime = finishTime;
		bool flag = true;
		if (opponentFinishTime != 0f && finishTime >= opponentFinishTime)
		{
			flag = false;
		}
		if (flag)
		{
			AudioController.Play("Finish_improved");
		}
		else
		{
			AudioController.Play("Finish");
		}
		if (!Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerController>().isExternalControlled)
		{
			Singleton<AudioManager>.SP.PlayPostgame();
		}
	}

	public float GetFinishTime()
	{
		return FinishTime;
	}

	public Checkpoint[] GetAllCheckpoints()
	{
		List<Checkpoint> list = new List<Checkpoint>();
		list.Add(Startline);
		list.AddRange(Checkpoints);
		if ((bool)Finishline)
		{
			list.Add(Finishline);
		}
		return list.ToArray();
	}

	public bool HasOpponentCheckpointTimes()
	{
		return opponentCheckpointTimes.Count != 0;
	}

	public bool FirstCheckpointPassed()
	{
		if (Checkpoints.Length == 0)
		{
			return false;
		}
		return Checkpoints[0].Passed();
	}

	public Checkpoint GetNextCheckpoint(Checkpoint cp)
	{
		Checkpoint[] allCheckpoints = GetAllCheckpoints();
		for (int i = 0; i < allCheckpoints.Length - 1; i++)
		{
			if (cp == allCheckpoints[i])
			{
				return allCheckpoints[i + 1];
			}
		}
		return null;
	}

	public int GetCheckpointNumber(Checkpoint cp)
	{
		Checkpoint[] allCheckpoints = GetAllCheckpoints();
		for (int i = 0; i < allCheckpoints.Length; i++)
		{
			if (cp == allCheckpoints[i])
			{
				return i;
			}
		}
		Debug.LogWarning("Asking for the number of a checkpoint that's not in this checkpointmanager's list of checkpoints!");
		return -1;
	}
}
