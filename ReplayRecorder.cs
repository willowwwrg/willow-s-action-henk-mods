using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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
		StringBuilder sb = new StringBuilder();
		sb.Append(replay.Count).Append('\n');
		sb.Append(Singleton<ReplayManager>.SP.replayVersion).Append('\n');
		sb.Append(Singleton<CheckpointManager>.SP.GetFinishTime().ToString(CultureInfo.InvariantCulture)).Append('\n');
		CharacterSelect.Characters replayCharacter = Singleton<CharacterSelect>.SP.GetSelectedCharacter();
		int replaySkin = ReplaySkinRemap.RemapSkin(replayCharacter, Singleton<CharacterSelect>.SP.GetSelectedSkin());
		sb.Append((int)replayCharacter).Append('\n');
		sb.Append(replaySkin).Append('\n');
		sb.Append(snapshotRate).Append('\n');
		sb.Append("cpdata\n");
		foreach (float checkpointTime in waypointManager.GetCheckpointTimes())
			sb.Append(checkpointTime.ToString(CultureInfo.InvariantCulture)).Append('\n');
		sb.Append("walk\n");
		float num = 0f;
		for (int i = 0; i < replay.Count; i++)
		{
			if (i == 0 || replay[i].walkInput != num)
			{
				sb.Append(i).Append(',').Append(replay[i].walkInput.ToString(CultureInfo.InvariantCulture)).Append('\n');
				num = replay[i].walkInput;
			}
		}
		sb.Append("vert\n");
		num = 0f;
		for (int j = 0; j < replay.Count; j++)
		{
			if (j == 0 || replay[j].verticalInput != num)
			{
				sb.Append(j).Append(',').Append(replay[j].verticalInput.ToString(CultureInfo.InvariantCulture)).Append('\n');
				num = replay[j].verticalInput;
			}
		}
		sb.Append("abil\n");
		bool flag = false;
		for (int k = 0; k < replay.Count; k++)
		{
			if (k == 0 || replay[k].abilityInput != flag)
			{
				sb.Append(k).Append(',').Append(replay[k].abilityInput ? '1' : '0').Append('\n');
				flag = replay[k].abilityInput;
			}
		}
		sb.Append("jump\n");
		flag = false;
		for (int l = 0; l < replay.Count; l++)
		{
			if (l == 0 || replay[l].jumpInput != flag)
			{
				sb.Append(l).Append(',').Append(replay[l].jumpInput ? '1' : '0').Append('\n');
				flag = replay[l].jumpInput;
			}
		}
		sb.Append("slide\n");
		flag = false;
		for (int m = 0; m < replay.Count; m++)
		{
			if (m == 0 || replay[m].slideInput != flag)
			{
				sb.Append(m).Append(',').Append(replay[m].slideInput ? '1' : '0').Append('\n');
				flag = replay[m].slideInput;
			}
		}
		sb.Append("reset\n");
		flag = false;
		for (int n = 0; n < replay.Count; n++)
		{
			if (n == 0 || replay[n].resetToLastCheckpoint != flag)
			{
				sb.Append(n).Append(',').Append(replay[n].resetToLastCheckpoint ? '1' : '0').Append('\n');
				flag = replay[n].resetToLastCheckpoint;
			}
		}
		sb.Append("snaps\n");
		for (int num2 = 0; num2 < snapshots.Count; num2++)
		{
			SnapshotFrame snapshotFrame = snapshots[num2];
			sb.Append(snapshotFrame.position.x.ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(snapshotFrame.position.y.ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(snapshotFrame.position.z.ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(snapshotFrame.velocity.x.ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(snapshotFrame.velocity.y.ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(snapshotFrame.velocity.z.ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(snapshotFrame.waypoint).Append('\n');
		}
		// Append coin split data for bonus levels — placed after snaps so vanilla clients ignore it
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj() != null &&
			Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus)
		{
			Dictionary<int, float> coinTimes = Singleton<BonusSplitManager>.SP.GetPBCoinTimesForReplay();
			if (coinTimes.Count > 0)
			{
				sb.Append("coindata\n");
				foreach (KeyValuePair<int, float> kvp in coinTimes)
					sb.Append(kvp.Key).Append(',').Append(kvp.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');
			}
		}
		replayString = sb.ToString();
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
