using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class ReplayRecorder : Singleton<ReplayRecorder>
{
	public bool writeDefault;

	private PlatformerPhysics player;

	private RaycastCollider playerCollider;

	private PlayerWaypointManager waypointManager;

	private bool isRecording;

	public List<ReplayFrame> replay = new List<ReplayFrame>();

	public List<SnapshotFrame> snapshots = new List<SnapshotFrame>();

	private ReplayFrame currentFrame = new ReplayFrame();

	private int currentFrameNumber;

	[NonSerialized]
	public int snapshotRate = 100;

	public string replayString;

	public bool insertResetFrame;

	private StreamWriter replayWriter;

	private void Awake()
	{
		player = GetComponent<PlatformerPhysics>();
		playerCollider = GetComponent<RaycastCollider>();
		waypointManager = GetComponent<PlayerWaypointManager>();
	}

	private void FixedUpdate()
	{
		if ((bool)player && isRecording)
		{
			currentFrame = new ReplayFrame();
			currentFrame.frameNumber = currentFrameNumber;
			currentFrame.walkInput = player.input.walkInput;
			currentFrame.verticalInput = player.input.verticalInput;
			currentFrame.jumpInput = player.input.jumpInput;
			currentFrame.slideInput = player.input.slideInput;
			currentFrame.abilityInput = player.input.abilityInput;
			if (insertResetFrame)
			{
				insertResetFrame = false;
				currentFrame.resetToLastCheckpoint = true;
			}
			else
			{
				currentFrame.resetToLastCheckpoint = false;
			}
			replay.Add(currentFrame);
			if (currentFrameNumber % snapshotRate == 0)
			{
				SnapshotFrame snapshotFrame = new SnapshotFrame();
				snapshotFrame.waypoint = GetComponent<PlayerWaypointManager>().GetWaypoint();
				snapshotFrame.position = base.transform.position;
				snapshotFrame.velocity = playerCollider.velocity;
				snapshots.Add(snapshotFrame);
			}
			currentFrameNumber++;
		}
	}

	public void StartRecord()
	{
		if (base.enabled)
		{
			currentFrameNumber = 0;
			replay.Clear();
			snapshots.Clear();
			isRecording = true;
			insertResetFrame = false;
		}
	}

	public void StopRecord()
	{
		if (isRecording)
		{
			isRecording = false;
			SaveReplayToString();
		}
	}

	public void SaveReplayToString()
	{
		replayString = string.Empty;
		replayString = replayString + replay.Count + "\n";
		replayString = replayString + Singleton<ReplayManager>.SP.replayVersion + "\n";
		replayString = replayString + Singleton<CheckpointManager>.SP.GetFinishTime().ToString(CultureInfo.InvariantCulture) + "\n";
		CharacterSelect.Characters replayCharacter = Singleton<CharacterSelect>.SP.GetSelectedCharacter();
		int replaySkin = ReplaySkinRemap.RemapSkin(replayCharacter, Singleton<CharacterSelect>.SP.GetSelectedSkin());
		replayString = replayString + (int)replayCharacter + "\n";
		replayString = replayString + replaySkin + "\n";
		replayString = replayString + snapshotRate + "\n";
		replayString += "cpdata\n";
		foreach (float checkpointTime in waypointManager.GetCheckpointTimes())
		{
			replayString = replayString + checkpointTime.ToString(CultureInfo.InvariantCulture) + "\n";
		}
		replayString += "walk\n";
		float num = 0f;
		for (int i = 0; i < replay.Count; i++)
		{
			if (i == 0)
			{
				string text = replayString;
				replayString = text + i + "," + replay[i].walkInput.ToString(CultureInfo.InvariantCulture) + "\n";
				num = replay[i].walkInput;
			}
			else if (replay[i].walkInput != num)
			{
				string text2 = replayString;
				replayString = text2 + i + "," + replay[i].walkInput.ToString(CultureInfo.InvariantCulture) + "\n";
				num = replay[i].walkInput;
			}
		}
		replayString += "vert\n";
		num = 0f;
		for (int j = 0; j < replay.Count; j++)
		{
			if (j == 0)
			{
				string text3 = replayString;
				replayString = text3 + j + "," + replay[j].verticalInput.ToString(CultureInfo.InvariantCulture) + "\n";
				num = replay[j].verticalInput;
			}
			else if (replay[j].verticalInput != num)
			{
				string text4 = replayString;
				replayString = text4 + j + "," + replay[j].verticalInput.ToString(CultureInfo.InvariantCulture) + "\n";
				num = replay[j].verticalInput;
			}
		}
		replayString += "abil\n";
		bool flag = false;
		for (int k = 0; k < replay.Count; k++)
		{
			string text5 = ((!replay[k].abilityInput) ? "0" : "1");
			if (k == 0)
			{
				string text6 = replayString;
				replayString = text6 + k + "," + text5 + "\n";
				flag = replay[k].abilityInput;
			}
			else if (replay[k].abilityInput != flag)
			{
				string text7 = replayString;
				replayString = text7 + k + "," + text5 + "\n";
				flag = replay[k].abilityInput;
			}
		}
		replayString += "jump\n";
		flag = false;
		for (int l = 0; l < replay.Count; l++)
		{
			string text8 = ((!replay[l].jumpInput) ? "0" : "1");
			if (l == 0)
			{
				string text9 = replayString;
				replayString = text9 + l + "," + text8 + "\n";
				flag = replay[l].jumpInput;
			}
			else if (replay[l].jumpInput != flag)
			{
				string text10 = replayString;
				replayString = text10 + l + "," + text8 + "\n";
				flag = replay[l].jumpInput;
			}
		}
		replayString += "slide\n";
		flag = false;
		for (int m = 0; m < replay.Count; m++)
		{
			string text11 = ((!replay[m].slideInput) ? "0" : "1");
			if (m == 0)
			{
				string text12 = replayString;
				replayString = text12 + m + "," + text11 + "\n";
				flag = replay[m].slideInput;
			}
			else if (replay[m].slideInput != flag)
			{
				string text13 = replayString;
				replayString = text13 + m + "," + text11 + "\n";
				flag = replay[m].slideInput;
			}
		}
		replayString += "reset\n";
		flag = false;
		for (int n = 0; n < replay.Count; n++)
		{
			string text14 = ((!replay[n].resetToLastCheckpoint) ? "0" : "1");
			if (n == 0)
			{
				string text15 = replayString;
				replayString = text15 + n + "," + text14 + "\n";
				flag = replay[n].resetToLastCheckpoint;
			}
			else if (replay[n].resetToLastCheckpoint != flag)
			{
				string text16 = replayString;
				replayString = text16 + n + "," + text14 + "\n";
				flag = replay[n].resetToLastCheckpoint;
			}
		}
		replayString += "snaps\n";
		for (int num2 = 0; num2 < snapshots.Count; num2++)
		{
			SnapshotFrame snapshotFrame = snapshots[num2];
			string text17 = snapshotFrame.position.x.ToString(CultureInfo.InvariantCulture) + "," + snapshotFrame.position.y.ToString(CultureInfo.InvariantCulture) + "," + snapshotFrame.position.z.ToString(CultureInfo.InvariantCulture) + ",";
			string text18 = text17;
			text17 = text18 + snapshotFrame.velocity.x.ToString(CultureInfo.InvariantCulture) + "," + snapshotFrame.velocity.y.ToString(CultureInfo.InvariantCulture) + "," + snapshotFrame.velocity.z.ToString(CultureInfo.InvariantCulture) + ",";
			text17 = text17 + snapshotFrame.waypoint + "\n";
			replayString += text17;
		}
	}

	public void SaveReplayLocal(SaveReplayType replayType = SaveReplayType.PersonalBest)
	{
		string text = Application.dataPath + "/Resources/../../PBreplays/";
		string text2 = text + "replay" + Singleton<LevelBatchManager>.SP.GetCurrentLevelNumAndName() + ".txt";
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop)
		{
			text2 = text + "replayworkshop" + Singleton<LevelBatchManager>.SP.GetGUID() + ".txt";
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			text2 = text + "replay_daily" + Singleton<MutatorManager>.SP.seedOfToday + ".txt";
		}
		switch (replayType)
		{
		case SaveReplayType.ReplayDump:
			text = Application.dataPath + "/Resources/../../ReplayDump/";
			text2 = text + "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_" + Singleton<CheckpointManager>.SP.GetFinishTime() + ".txt";
			break;
		case SaveReplayType.Bronze:
			text = Application.dataPath + "/Resources/MedalReplays/";
			text2 = text + "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_Bronze.txt";
			break;
		case SaveReplayType.Silver:
			text = Application.dataPath + "/Resources/MedalReplays/";
			text2 = text + "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_Silver.txt";
			break;
		case SaveReplayType.Gold:
			text = Application.dataPath + "/Resources/MedalReplays/";
			text2 = text + "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_Gold.txt";
			break;
		case SaveReplayType.Rainbow:
			text = Application.dataPath + "/Resources/MedalReplays/";
			text2 = text + "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_Rainbow.txt";
			break;
		case SaveReplayType.Challenge:
			text = Application.dataPath + "/Resources/MedalReplays/";
			text2 = text + "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_Challenge.txt";
			break;
		}
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		replayWriter = new StreamWriter(text2);
		replayWriter.Write(replayString);
		replayWriter.Close();
		Debug.Log("Replay saved locally: " + text2);
		if (replayType == SaveReplayType.PersonalBest)
		{
			Singleton<ReplayManager>.SP.RefreshCurrentLevelLocalReplay();
		}
	}

	public void SaveReplayToSteam()
	{
		Debug.Log("Saving replay to steam");
		if (replayString == string.Empty)
		{
			Debug.LogError("Error: Could not convert replay to string.");
		}
		else if (Singleton<HenkSWLeaderboards>.SP.Initialize())
		{
			Singleton<HenkSWLeaderboards>.SP.SaveReplayOnSteam(replayString);
		}
	}

	private string ReplayToString(string filePath)
	{
		if (!File.Exists(filePath))
		{
			MonoBehaviour.print("file doesn't exist.");
			return string.Empty;
		}
		_ = string.Empty;
		using StreamReader streamReader = new StreamReader(filePath);
		return streamReader.ReadToEnd();
	}
}
