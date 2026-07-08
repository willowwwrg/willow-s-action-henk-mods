using System;
using System.Collections.Generic;
using System.IO;
using Henk;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
	private enum ReplayReadState
	{
		header,
		checkpoints,
		walk,
		vertical,
		ability,
		jump,
		slide,
		reset,
		snapshots,
		coindata
	}

	public const bool correctToSnapshotsPrinting = false;

	public const bool correctToSnapshotsPrintingWarnings = false;

	private bool isInitialized;

	private bool isPlaying;

	private int currentReplayFrame;

	private PlatformerPhysics player;

	private RaycastCollider playerCollider;

	private PlayerWaypointManager playerWaypoint;

	public List<ReplayFrame> replay = new List<ReplayFrame>();

	public List<SnapshotFrame> snapshots = new List<SnapshotFrame>();

	private GhostType replayType;

	[NonSerialized]
	public int snapshotRate;

	public CharacterSelect.Characters replayCharacter = CharacterSelect.Characters.Henk;

	public int replaySkin;

	private Vector3 prevPos = Vector3.zero;

	public ulong steamID;

	public string playerName = string.Empty;

	public float finishTime = -1f;

	[HideInInspector]
	public bool reloadReplay;

	private bool replayInvalid;

	private ReplayReadState readState;

	private void Awake()
	{
		player = GetComponent<PlatformerPhysics>();
		playerCollider = GetComponent<RaycastCollider>();
		playerWaypoint = GetComponent<PlayerWaypointManager>();
	}

	public void SetSteamID(ulong id)
	{
		steamID = id;
		playerName = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(steamID);
	}

	public void ClearReplay()
	{
		replay.Clear();
		snapshots.Clear();
		Singleton<CheckpointManager>.SP.ClearOpponentTimes();
	}

	public bool ReadReplay(string data, string medalReplay = "")
	{
		int num = 0;
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
		Dictionary<int, bool> dictionary3 = new Dictionary<int, bool>();
		Dictionary<int, bool> dictionary4 = new Dictionary<int, bool>();
		Dictionary<int, bool> dictionary5 = new Dictionary<int, bool>();
		Dictionary<int, bool> dictionary6 = new Dictionary<int, bool>();
		Dictionary<int, float> coinData = new Dictionary<int, float>();
		snapshots.Clear();
		List<float> list = new List<float>();
		finishTime = 0f;
		Singleton<CheckpointManager>.SP.ClearOpponentTimes();
		using (StringReader stringReader = new StringReader(data))
		{
			readState = ReplayReadState.header;
			string intString = stringReader.ReadLine();
			num = HenkUtils.IntParse(intString);
			intString = stringReader.ReadLine();
			if (HenkUtils.IntParse(intString) != Singleton<ReplayManager>.SP.replayVersion)
			{
				Debug.LogError("Replay not correct version.");
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDLOADINGREPLAY", "PERMA"));
				return false;
			}
			intString = stringReader.ReadLine();
			finishTime = HenkUtils.FloatParse(intString);
			intString = stringReader.ReadLine();
			replayCharacter = (CharacterSelect.Characters)HenkUtils.IntParse(intString);
			intString = stringReader.ReadLine();
			replaySkin = HenkUtils.IntParse(intString);
			intString = stringReader.ReadLine();
			snapshotRate = HenkUtils.IntParse(intString);
			intString = stringReader.ReadLine();
			if (!(intString == "cpdata"))
			{
				Debug.LogError("Error while reading replay data: Invalid replay file format.");
				return false;
			}
			readState = ReplayReadState.checkpoints;
			if (num == 0)
			{
				Debug.LogError("Error decompressing replay data");
				return false;
			}
			do
			{
				intString = stringReader.ReadLine();
				if (intString == null || !(intString != string.Empty))
				{
					continue;
				}
				switch (readState)
				{
				case ReplayReadState.checkpoints:
				{
					if (intString == "walk")
					{
						readState = ReplayReadState.walk;
						break;
					}
					float item = HenkUtils.FloatParse(intString);
					list.Add(item);
					break;
				}
				case ReplayReadState.walk:
				{
					if (intString == "vert")
					{
						readState = ReplayReadState.vertical;
						break;
					}
					string[] array6 = intString.Split(',');
					dictionary.Add(HenkUtils.IntParse(array6[0]), HenkUtils.FloatParse(array6[1]));
					break;
				}
				case ReplayReadState.vertical:
				{
					if (intString == "abil")
					{
						readState = ReplayReadState.ability;
						break;
					}
					string[] array3 = intString.Split(',');
					dictionary2.Add(HenkUtils.IntParse(array3[0]), HenkUtils.FloatParse(array3[1]));
					break;
				}
				case ReplayReadState.ability:
				{
					if (intString == "jump")
					{
						readState = ReplayReadState.jump;
						break;
					}
					string[] array5 = intString.Split(',');
					dictionary3.Add(HenkUtils.IntParse(array5[0]), HenkUtils.BoolParse(array5[1]));
					break;
				}
				case ReplayReadState.jump:
				{
					if (intString == "slide")
					{
						readState = ReplayReadState.slide;
						break;
					}
					string[] array7 = intString.Split(',');
					dictionary4.Add(HenkUtils.IntParse(array7[0]), HenkUtils.BoolParse(array7[1]));
					break;
				}
				case ReplayReadState.slide:
				{
					if (intString == "reset")
					{
						readState = ReplayReadState.reset;
						break;
					}
					string[] array4 = intString.Split(',');
					dictionary5.Add(HenkUtils.IntParse(array4[0]), HenkUtils.BoolParse(array4[1]));
					break;
				}
				case ReplayReadState.reset:
				{
					if (intString == "snaps")
					{
						readState = ReplayReadState.snapshots;
						break;
					}
					string[] array2 = intString.Split(',');
					dictionary6.Add(HenkUtils.IntParse(array2[0]), HenkUtils.BoolParse(array2[1]));
					break;
				}
				case ReplayReadState.snapshots:
				{
					if (intString == "coindata")
					{
						readState = ReplayReadState.coindata;
						break;
					}
					string[] array = intString.Split(',');
					SnapshotFrame snapshotFrame = new SnapshotFrame();
					snapshotFrame.position = new Vector3(HenkUtils.FloatParse(array[0]), HenkUtils.FloatParse(array[1]), HenkUtils.FloatParse(array[2]));
					snapshotFrame.velocity = new Vector3(HenkUtils.FloatParse(array[3]), HenkUtils.FloatParse(array[4]), HenkUtils.FloatParse(array[5]));
					snapshotFrame.waypoint = HenkUtils.IntParse(array[6]);
					snapshots.Add(snapshotFrame);
					break;
				}
				case ReplayReadState.coindata:
				{
					string[] parts = intString.Split(',');
					if (parts.Length == 2)
						coinData[HenkUtils.IntParse(parts[0])] = HenkUtils.FloatParse(parts[1]);
					break;
				}
				default:
					Debug.LogError("Error decompressing replay data.");
					return false;
				}
			}
			while (intString != null && intString != string.Empty);
			replay.Clear();
			if (num > 0 && num < 1000000)
				replay.Capacity = num;
			ReplayFrame replayFrame = new ReplayFrame();
			replayFrame.frameNumber = 0;
			replayFrame.walkInput = 0f;
			replayFrame.verticalInput = 0f;
			replayFrame.abilityInput = false;
			replayFrame.jumpInput = false;
			replayFrame.slideInput = false;
			replayFrame.resetToLastCheckpoint = false;
			for (int i = 0; i < num; i++)
			{
				replayFrame.frameNumber = i;
				if (dictionary.ContainsKey(i))
					replayFrame.walkInput = dictionary[i];
				if (dictionary2.ContainsKey(i))
					replayFrame.verticalInput = dictionary2[i];
				if (dictionary3.ContainsKey(i))
					replayFrame.abilityInput = dictionary3[i];
				if (dictionary4.ContainsKey(i))
					replayFrame.jumpInput = dictionary4[i];
				if (dictionary5.ContainsKey(i))
					replayFrame.slideInput = dictionary5[i];
				if (dictionary6.ContainsKey(i))
					replayFrame.resetToLastCheckpoint = dictionary6[i];
				replay.Add(new ReplayFrame
				{
					frameNumber = replayFrame.frameNumber,
					walkInput = replayFrame.walkInput,
					verticalInput = replayFrame.verticalInput,
					abilityInput = replayFrame.abilityInput,
					jumpInput = replayFrame.jumpInput,
					slideInput = replayFrame.slideInput,
					resetToLastCheckpoint = replayFrame.resetToLastCheckpoint
				});
			}
		}
		Singleton<CheckpointManager>.SP.SetOpponentTimes(list, finishTime);
		// Feed embedded coin times into BonusSplitManager if present
		if (coinData.Count > 0)
			Singleton<BonusSplitManager>.SP.LoadGhostCoinTimes(coinData);
		GetComponent<PlayerGraphics>().SetGhostSkin(medalReplay, replayCharacter, replaySkin);
		UnityEngine.Object.FindObjectOfType<State_ReplayMode>().SetReplayController(this);
		return true;
	}

	public void OnReset()
	{
	}

	public void CheckForReloadReplay()
	{
		if (reloadReplay)
		{
			if (replayType == GhostType.PersonalBest || replayType == GhostType.PersonalBestSteam)
			{
				GetPersonalBestReplay();
			}
			reloadReplay = false;
		}
	}

	private bool LoadMedalReplay(string medalType)
	{
		string text = "MedalReplays/lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_" + medalType;
		TextAsset textAsset = (TextAsset)Resources.Load(text);
		if (textAsset == null)
		{
			Debug.LogError("Medal replay with name: " + text + " doesn't exist.");
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDLOADINGREPLAY", "PERMA"));
			return false;
		}
		return ReadReplay(textAsset.text, medalType);
	}

	private bool GetPersonalBestReplay()
	{
		string data = string.Empty;
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
		if (!File.Exists(path))
		{
			return false;
		}
		using (StreamReader streamReader = new StreamReader(path))
		{
			data = streamReader.ReadToEnd();
		}
		GetComponent<PlayerGraphics>().ghostNameLabel.SetGhostName("Personal Best");
		return ReadReplay(data, string.Empty);
	}

	public void LoadReplay(GhostType ghostType, ulong playerID = 0uL)
	{
		if (ghostType != GhostType.CustomID)
		{
			Singleton<HenkSWLeaderboards>.SP.targetControllerForDownload = this;
		}
		else
		{
			Singleton<HenkSWLeaderboards>.SP.targetControllerForCustomDownload = this;
		}
		replayType = ghostType;
		bool flag = true;
		switch (ghostType)
		{
		case GhostType.PersonalBest:
			flag = GetPersonalBestReplay();
			if (!flag)
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOPERSONALBEST", "PERMA"));
			}
			break;
		case GhostType.PersonalBestSteam:
			flag = Singleton<HenkSWLeaderboards>.SP.GetPersonalBestReplay();
			break;
		case GhostType.Friend:
			flag = Singleton<HenkSWLeaderboards>.SP.GetFriendReplay(playerID);
			break;
		case GhostType.World1:
			flag = Singleton<HenkSWLeaderboards>.SP.GetGlobalNumberOneReplay();
			break;
		case GhostType.SpecificPlayer:
			flag = Singleton<HenkSWLeaderboards>.SP.GetSpecificPlayerReplay();
			break;
		case GhostType.CustomID:
			flag = Singleton<HenkSWLeaderboards>.SP.GetCustomReplay(playerID);
			break;
		case GhostType.MedalBronze:
			flag = LoadMedalReplay("Bronze");
			playerName = "Bronze Medal";
			break;
		case GhostType.MedalSilver:
			flag = LoadMedalReplay("Silver");
			playerName = "Silver Medal";
			break;
		case GhostType.MedalGold:
			flag = LoadMedalReplay("Gold");
			playerName = "Gold Medal";
			break;
		case GhostType.MedalRainbow:
			flag = LoadMedalReplay("Rainbow");
			playerName = "Rainbow Medal";
			break;
		case GhostType.Challenger:
			flag = LoadMedalReplay("Challenge");
			playerName = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().GetChallengerName();
			break;
		default:
			Debug.LogError("Error while loading replay for the ghost for ghosttype: " + ghostType);
			flag = false;
			return;
		}
		if (!flag)
		{
			ClearReplay();
			UnityEngine.Object.Destroy(base.gameObject);
		}
		isInitialized = true;
	}

	public GhostType GetReplayType()
	{
		return replayType;
	}

	public ReplayFrame GetCurrentReplayFrame()
	{
		if (isPlaying)
		{
			if (currentReplayFrame < replay.Count)
			{
				return replay[currentReplayFrame];
			}
			return new ReplayFrame();
		}
		return new ReplayFrame();
	}

	private void Update()
	{
		bool flag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.Replay || !flag || currentReplayFrame > replay.Count - 1)
		{
			return;
		}
		int num = currentReplayFrame;
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			currentReplayFrame -= currentReplayFrame % snapshotRate + snapshotRate;
			currentReplayFrame = Mathf.Clamp(currentReplayFrame, 0, replay.Count - 1);
			FixedUpdate();
			Camera.main.GetComponent<PlatformerCamera>().SnapCamera();
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			currentReplayFrame += snapshotRate - currentReplayFrame % snapshotRate + snapshotRate;
			currentReplayFrame = Mathf.Clamp(currentReplayFrame, 0, replay.Count - 1);
			if (currentReplayFrame == replay.Count - 1)
			{
				Singleton<CheckpointManager>.SP.Finishline.TriggerCheckpoint();
			}
			FixedUpdate();
			Camera.main.GetComponent<PlatformerCamera>().SnapCamera();
		}
		int frames = currentReplayFrame - num;
		Singleton<Stopwatch>.SP.OffsetTime(frames);
	}

	private void FixedUpdate()
	{
		if (!isInitialized || !isPlaying || currentReplayFrame > replay.Count - 1)
		{
			return;
		}
		ReplayFrame frame = replay[currentReplayFrame];
		player.input.walkInput = frame.walkInput;
		player.input.verticalInput = frame.verticalInput;
		player.input.abilityInput = frame.abilityInput;
		player.input.jumpInput = frame.jumpInput;
		player.input.slideInput = frame.slideInput;
		if (frame.resetToLastCheckpoint)
		{
			Singleton<PlayerManager>.SP.ResetPlayer(base.gameObject, hard: false);
		}
		if (currentReplayFrame % snapshotRate == 0 && snapshotRate != 0)
		{
			int index = currentReplayFrame / snapshotRate;
			SnapshotFrame snapshotFrame = snapshots[index];
			Vector3 position = snapshotFrame.position;
			Vector3 velocity = snapshotFrame.velocity;
			if ((base.transform.position - position).magnitude > 1f)
			{
				_ = replayInvalid;
			}
			playerWaypoint.SetOffset(snapshotFrame.waypoint);
			base.transform.position = position;
			playerCollider.velocity = velocity;
		}
		currentReplayFrame++;
	}

	public void Play()
	{
		isPlaying = true;
	}

	public void Stop()
	{
		isPlaying = false;
		currentReplayFrame = 0;
	}

	public void Initialized()
	{
		isInitialized = true;
	}
}
