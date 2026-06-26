using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class MultiManager : Singleton<MultiManager>
{
	private int warmupDuration = 20;

	private int endgameDuration = 15;

	private bool quickStart = true;

	private string version = "0.4";

	public int serverRegion = 1;

	private string roomName = string.Empty;

	private int roundDuration = 140;

	private int maxPlayers = 20;

	private string roomMutators = string.Empty;

	private List<Mutator> mutatorList = new List<Mutator>();

	public Mutator currentActiveMutator;

	private ulong[] defautlLevelRotation = new ulong[9] { 34uL, 3uL, 2uL, 14uL, 4uL, 16uL, 32uL, 27uL, 7uL };

	private ulong[] levelRotation;

	private int currentLevelInRotation;

	public PhotonView photonView;

	public MultiplayerState multiplayerState;

	public bool isAboutToLoadLevel;

	public bool isDownloading;

	public string serverStatusText = string.Empty;

	public int timeLeftTillServerOnline = -1;

	private float lastTimeConnected;

	private float minTimeForReconnect = 3f;

	public void Start()
	{
		levelRotation = defautlLevelRotation;
		photonView = PhotonView.Get(this);
		if (photonView == null)
		{
			photonView = base.gameObject.AddComponent<PhotonView>();
		}
		photonView.viewID = 88;
		if (!Application.isEditor)
		{
			quickStart = false;
		}
		multiplayerState = MultiplayerState.Disconnected;
		CheckServerStatus();
	}

	public void CheckServerStatus()
	{
		serverRegion = 3;
	}

	public void ResetConnectTimer()
	{
		lastTimeConnected = 0f;
	}

	public void Connect()
	{
		if (Time.time - lastTimeConnected < minTimeForReconnect)
		{
			return;
		}
		lastTimeConnected = Time.time;
		if (SteamManager.Initialized)
		{
			PhotonNetwork.playerName = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(Singleton<HenkSWUserStats>.SP.GetSteamID());
			if (PhotonNetwork.playerName == string.Empty)
			{
				PhotonNetwork.playerName = "Action Henk";
			}
			Singleton<GAManager>.SP.MultiplayerEvent("Connect");
			multiplayerState = MultiplayerState.ConnectingToLobby;
			if (GetServerIP() != "best")
			{
				PhotonNetwork.ConnectToMaster(GetServerIP(), 5055, "11ca6d8d-09b2-4178-aa4a-b7c3f1acb62c", version);
				Debug.Log("Connecting to " + GetServerIP() + ".");
			}
			else
			{
				PhotonNetwork.ConnectUsingSettings(version);
				Debug.Log("Connecting to best region.");
			}
			Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Multiplayer);
		}
		else
		{
			DisconnectFromGame("Steam needs to be connected to play multiplayer", completelyBackToMainMenu: true);
		}
	}

	public void Disconnect()
	{
		StopCoroutine("DownloadMissingLevels");
		PhotonNetwork.SetPlayerCustomProperties(null);
		PhotonNetwork.Disconnect();
		multiplayerState = MultiplayerState.Disconnected;
		Singleton<GAManager>.SP.MultiplayerEvent("Disconnect");
	}

	private void OnJoinedLobby()
	{
		Singleton<GAManager>.SP.MultiplayerEvent("JoinedLobby");
		MonoBehaviour.print("Photon master server time: " + PhotonNetwork.time);
		multiplayerState = MultiplayerState.ConnectedToLobby;
		Singleton<GetRoot>.SP.Get().BroadcastMessage("OnLobbyJoined", SendMessageOptions.DontRequireReceiver);
	}

	public void JoinRandomRoom()
	{
		multiplayerState = MultiplayerState.ConnectingToServer;
		Singleton<GAManager>.SP.MultiplayerEvent("JoinRandomRoom");
		Debug.Log("Joining random room");
		if (!PhotonNetwork.JoinRandomRoom())
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_JOINROOMFAILED", "PERMA"));
		}
	}

	public void JoinNamedRoom(string roomName)
	{
		multiplayerState = MultiplayerState.ConnectingToServer;
		Singleton<GAManager>.SP.MultiplayerEvent("JoinNamedRoom");
		Debug.Log("joining room " + roomName);
		if (!PhotonNetwork.JoinRoom(roomName))
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_JOINROOMFAILED", "PERMA"));
		}
	}

	public void OnPhotonRandomJoinFailed()
	{
		Singleton<GAManager>.SP.MultiplayerEvent("RandomJoinFailed");
		Debug.Log("JoinRandom Failed, creating room");
		CreateARoom();
	}

	public void CreateARoom()
	{
		multiplayerState = MultiplayerState.CreatingServer;
		Singleton<GAManager>.SP.MultiplayerEvent("CreateRoom");
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.isVisible = true;
		roomOptions.isOpen = true;
		roomOptions.maxPlayers = maxPlayers;
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable.Add("mutators", roomMutators);
		roomOptions.customRoomProperties = hashtable;
		if (roomName == string.Empty)
		{
			roomName = PhotonNetwork.playerName + "'s game";
		}
		PhotonNetwork.CreateRoom(roomName, roomOptions, null);
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
		if (list.Count == 1)
		{
			list.Add(list[0]);
		}
		levelRotation = list.ToArray();
	}

	public void CreateRoomFromGUI(string a_roomName, int a_roundDuration, int a_maxPlayers, List<Mutator> mutators)
	{
		roomName = a_roomName;
		roundDuration = a_roundDuration * 60;
		maxPlayers = a_maxPlayers;
		mutatorList = mutators;
		roomMutators = string.Empty;
		if (mutators.Count != 0)
		{
			for (int i = 0; i < mutators.Count; i++)
			{
				if (i != 0)
				{
					roomMutators += ",";
				}
				roomMutators += Singleton<MutatorManager>.SP.GetStringFromMutator(mutators[i]);
			}
		}
		CreateARoom();
	}

	private void OnJoinedRoom()
	{
		Singleton<GAManager>.SP.MultiplayerEvent("JoinedRoom");
		Debug.Log("Joined room");
		if (PhotonNetwork.time == 0.0)
		{
			Singleton<GAManager>.SP.MultiplayerEvent("PhotonTime_0");
			Singleton<MultiManager>.SP.DisconnectFromGame("Couldn't sync server time, please reconnect", completelyBackToMainMenu: true);
			return;
		}
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		if (SteamManager.Initialized)
		{
			hashtable["steamID"] = (long)Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID;
		}
		PhotonNetwork.SetPlayerCustomProperties(hashtable);
		ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.room.customProperties;
		if (PhotonNetwork.isMasterClient)
		{
			if (customProperties["currentlevel"] == null || customProperties["levelstarttime"] == null || customProperties["levelrotation"] == null || customProperties["mutators"] == null || customProperties["currentmutator"] == null)
			{
				Debug.Log("Setting level properties");
				double num = PhotonNetwork.time - 1.0;
				if (quickStart)
				{
					num -= (double)warmupDuration;
				}
				customProperties["levelstarttime"] = num;
				customProperties["roundduration"] = roundDuration;
				currentLevelInRotation = 0;
				customProperties["currentlevel"] = 0;
				customProperties["levelrotation"] = LevelRotationToString();
				customProperties["mutators"] = roomMutators;
				currentActiveMutator = ((mutatorList.Count != 0) ? mutatorList[Random.Range(0, mutatorList.Count)] : Mutator.None);
				customProperties["currentmutator"] = Singleton<MutatorManager>.SP.GetStringFromMutator(currentActiveMutator);
				PhotonNetwork.room.SetCustomProperties(customProperties);
				UpdatePlayerList();
				string[] propertiesListedInLobby = new string[7] { "currentlevel", "levelstarttime", "playerlist", "levelrotation", "roundduration", "mutators", "currentmutator" };
				PhotonNetwork.room.SetPropertiesListedInLobby(propertiesListedInLobby);
			}
		}
		else
		{
			if (customProperties["currentlevel"] == null || customProperties["levelstarttime"] == null || customProperties["levelrotation"] == null || customProperties["mutators"] == null || customProperties["currentmutator"] == null)
			{
				Singleton<GAManager>.SP.MultiplayerEvent("ServerMissingProperties");
				DisconnectFromGame("Server is missing critical info.");
			}
			levelRotation = ReadLevelRotationFromString(customProperties["levelrotation"].ToString());
			currentLevelInRotation = (int)customProperties["currentlevel"];
			roomMutators = customProperties["mutators"].ToString();
			mutatorList.Clear();
			List<string> list = new List<string>(roomMutators.Split(','));
			for (int i = 0; i < list.Count; i++)
			{
				Mutator mutatorFromString = Singleton<MutatorManager>.SP.GetMutatorFromString(list[i]);
				if (mutatorFromString != Mutator.None)
				{
					mutatorList.Add(mutatorFromString);
				}
			}
			currentActiveMutator = Singleton<MutatorManager>.SP.GetMutatorFromString(customProperties["currentmutator"].ToString());
		}
		if (CheckIfAllLevelsDownloaded())
		{
			multiplayerState = MultiplayerState.ConnectedToServer;
		}
		isAboutToLoadLevel = true;
		Debug.Log("server level rotation: " + customProperties["levelrotation"]);
		Debug.Log("server current level: " + customProperties["currentlevel"]);
		Debug.Log("server level start time: " + customProperties["levelstarttime"]);
		Debug.Log("server playerlist: " + customProperties["playerlist"]);
		Debug.Log("server mutator list: " + customProperties["mutators"]);
		Debug.Log("server active mutator: " + customProperties["currentmutator"]);
	}

	private void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		MonoBehaviour.print("Master client switched");
	}

	public void StartNewLevel()
	{
		if (PhotonNetwork.isMasterClient && RoundTimeLeft() <= 0f)
		{
			int num = currentLevelInRotation + 1;
			num %= levelRotation.Length;
			if (mutatorList.Count != 0)
			{
				currentActiveMutator = mutatorList[Random.Range(0, mutatorList.Count)];
			}
			Debug.Log("starting level " + num + " soon");
			ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.room.customProperties;
			customProperties["currentlevel"] = num;
			customProperties["levelstarttime"] = PhotonNetwork.time + (double)endgameDuration;
			customProperties["currentmutator"] = Singleton<MutatorManager>.SP.GetStringFromMutator(currentActiveMutator);
			PhotonNetwork.room.SetCustomProperties(customProperties);
		}
	}

	public ulong GetLevelInRotation(int offsetFromCurrent = 0)
	{
		int num = currentLevelInRotation + offsetFromCurrent;
		num %= levelRotation.Length;
		return levelRotation[num];
	}

	public void LoadCurrentLevelInRotation()
	{
		ulong levelInRotation = GetLevelInRotation();
		MonoBehaviour.print("loading level with ID " + levelInRotation);
		PhotonNetwork.isMessageQueueRunning = false;
		Singleton<LevelBatchManager>.SP.LoadLevelScenelessFromID(levelInRotation);
		Singleton<MutatorManager>.SP.SetAndEnableMutator(currentActiveMutator);
	}

	public void DoneLoading()
	{
		isAboutToLoadLevel = false;
		PhotonNetwork.isMessageQueueRunning = true;
	}

	private void Update()
	{
		if (!PhotonNetwork.inRoom || PhotonNetwork.time == 0.0 || !PhotonNetwork.isMessageQueueRunning || isDownloading)
		{
			return;
		}
		ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.room.customProperties;
		if (TimeLeftTillSwitch() <= 0f)
		{
			if (customProperties["currentlevel"] != null && (int)customProperties["currentlevel"] != currentLevelInRotation)
			{
				isAboutToLoadLevel = true;
				currentLevelInRotation = (int)customProperties["currentlevel"];
				MonoBehaviour.print("getting ready to load level nr " + currentLevelInRotation);
				roomMutators = customProperties["mutators"].ToString();
				currentActiveMutator = Singleton<MutatorManager>.SP.GetMutatorFromString(customProperties["currentmutator"].ToString());
				MonoBehaviour.print("setting new mutator to: " + currentActiveMutator);
				PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
				PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable { ["score"] = null });
			}
			else if (isAboutToLoadLevel)
			{
				LoadCurrentLevelInRotation();
			}
		}
		if ((Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PreGameMultiplayer)) || Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGameMultiplayer)) || Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGameMultiplayer))) && (RoundTimeLeft() <= 0f || TimeLeftTillSwitch() > 1f))
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_EndGameMultiplayer));
			Singleton<MutatorManager>.SP.mutatorActive = false;
			Debug.Log("mutator disabled");
		}
	}

	public bool IsReadyToReceiveCharacters(ExitGames.Client.Photon.Hashtable properties = null)
	{
		return GetTimeLeft(1f, properties) < 0f;
	}

	public float WarmupTimeLeft(ExitGames.Client.Photon.Hashtable properties = null)
	{
		return GetTimeLeft(warmupDuration, properties);
	}

	public float RoundTimeLeft(ExitGames.Client.Photon.Hashtable properties = null)
	{
		int num = 1000;
		ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.room.customProperties;
		if (customProperties["roundduration"] == null)
		{
			Debug.LogError("Round duration not synced");
		}
		else
		{
			num = (int)customProperties["roundduration"];
		}
		return GetTimeLeft(warmupDuration + num, properties);
	}

	public float TimeLeftTillSwitch(ExitGames.Client.Photon.Hashtable properties = null)
	{
		return GetTimeLeft(0f, properties);
	}

	public float GetTimeLeft(float addedTime, ExitGames.Client.Photon.Hashtable properties = null)
	{
		if (!PhotonNetwork.inRoom || PhotonNetwork.time == 0.0)
		{
			return float.PositiveInfinity;
		}
		ExitGames.Client.Photon.Hashtable hashtable = properties;
		if (hashtable == null)
		{
			hashtable = PhotonNetwork.room.customProperties;
		}
		if (hashtable["levelstarttime"] != null)
		{
			return (float)((double)hashtable["levelstarttime"] - PhotonNetwork.time + (double)addedTime);
		}
		Debug.LogWarning("missing levelstarttime property");
		return float.PositiveInfinity;
	}

	private bool CheckIfAllLevelsDownloaded()
	{
		List<ulong> list = new List<ulong>();
		ulong[] array = levelRotation;
		foreach (ulong num in array)
		{
			if (Singleton<LevelBatchManager>.SP.GetLevelFromCodeOrID(num) == null)
			{
				list.Add(num);
			}
		}
		if (list.Count != 0)
		{
			StartCoroutine("DownloadMissingLevels", list.ToArray());
			return false;
		}
		return true;
	}

	private IEnumerator DownloadMissingLevels(ulong[] levelsToDownload)
	{
		multiplayerState = MultiplayerState.DownloadingLevels;
		isDownloading = true;
		Singleton<Workshop>.SP.SubscribeToList(levelsToDownload);
		while (Singleton<Workshop>.SP.GetDownloadsLeft() != 0)
		{
			yield return null;
		}
		multiplayerState = MultiplayerState.ConnectedToServer;
		isDownloading = false;
	}

	private void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		if (!(Singleton<PlayerManager>.SP.GetPlayer() == null))
		{
			_ = playerAndUpdatedProps[0];
			if ((playerAndUpdatedProps[1] as ExitGames.Client.Photon.Hashtable)["score"] != null)
			{
				Singleton<Scoreboard>.SP.ScorePropertyChanged(playerAndUpdatedProps);
			}
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log("Player " + player.name + " disconnect");
		Singleton<ChatBox>.SP.AddSystemMessageToChatbox(player.name + " disconnected.");
		UpdatePlayerList();
	}

	private void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Singleton<ChatBox>.SP.AddSystemMessageToChatbox(player.name + " joined the game.");
		UpdatePlayerList();
	}

	public void UpdatePlayerList()
	{
		if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom)
		{
			Singleton<Scoreboard>.SP.UpdateRanking();
			string text = string.Empty;
			PhotonPlayer[] rankedPlayers = Singleton<Scoreboard>.SP.GetRankedPlayers();
			foreach (PhotonPlayer photonPlayer in rankedPlayers)
			{
				text = text + photonPlayer.name + "\n";
			}
			if (text != string.Empty)
			{
				text = text.Substring(0, text.Length - 1);
				ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.room.customProperties;
				customProperties["playerlist"] = text;
				PhotonNetwork.room.SetCustomProperties(customProperties);
			}
		}
	}

	[RPC]
	public void SendChatMessage(string message, PhotonMessageInfo messageInfo)
	{
		Singleton<ChatBox>.SP.AddMessageToChatbox(message, messageInfo);
	}

	public void DisconnectFromGame(string disconnectMessage = "", bool completelyBackToMainMenu = false)
	{
		AudioController.Play("ButtonBackwards");
		if ((bool)Camera.main.GetComponent<PlatformerCamera>())
		{
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		}
		Singleton<Scoreboard>.SP.scoreBoard.transform.parent.parent.gameObject.SetActive(value: false);
		Singleton<PermaGUI>.SP.ToggleChatBox(state: false);
		Singleton<MultiManager>.SP.Disconnect();
		if (!completelyBackToMainMenu)
		{
			MonoBehaviour.print("Disconnecting to multiplayer menu");
			if (HenkUtils.IsInALevel())
			{
				HenkUtils.BackToMenu();
			}
			if (!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_Multiplayer)))
			{
				Singleton<GamestateManager>.SP.SetState(typeof(State_Multiplayer));
			}
		}
		else
		{
			MonoBehaviour.print("Disconnecting to main menu");
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		}
		if (disconnectMessage != string.Empty)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(disconnectMessage);
		}
	}

	public void ForceAllToDisconnect()
	{
		if (Application.isEditor)
		{
			Debug.Log("Forcing disconnect");
			photonView.RPC("ForceDisconnect", PhotonTargets.Others);
		}
	}

	[RPC]
	private void ForceDisconnect()
	{
		Singleton<GAManager>.SP.MultiplayerEvent("ForcedDisconnect");
		Debug.LogWarning("something forced us to disconnect");
		DisconnectFromGame("Lost connection to server");
	}

	private void OnPhotonJoinRoomFailed()
	{
		Singleton<GAManager>.SP.MultiplayerEvent("FailedToJoinRoom");
		DisconnectFromGame("Failed to join room");
	}

	private void OnPhotonCreateRoomFailed()
	{
		Singleton<GAManager>.SP.MultiplayerEvent("FailedToCreateRoom");
		DisconnectFromGame("Failed to create room");
	}

	private void OnConnectionFail(DisconnectCause cause)
	{
		Debug.LogWarning("Lost connection: " + cause);
		Singleton<GAManager>.SP.MultiplayerEvent("LostConnection-" + cause);
		DisconnectFromGame("Lost connection to server :(");
	}

	private void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.LogWarning("Failed to connect to master: " + cause);
		Singleton<GAManager>.SP.MultiplayerEvent("FailedToConnectToMaster-" + cause);
		if (cause == DisconnectCause.MaxCcuReached)
		{
			DisconnectFromGame("Server is at max amount of concurrent players :(");
		}
		else
		{
			DisconnectFromGame("Failed to connect to the master server");
		}
	}

	public string GetServerIP()
	{
		return serverRegion switch
		{
			1 => "app-eu.exitgamescloud.com", 
			2 => "app-asia.exitgamescloud.com", 
			3 => "app-us.exitgamescloud.com", 
			4 => "app-jp.exitgamescloud.com", 
			5 => "best", 
			_ => "best", 
		};
	}

	private IEnumerator ServerStatusRoutine()
	{
		WWW serverStatusCall = new WWW("http://www.ragesquid.com/actionhenkdatabase/serversettings.txt");
		yield return serverStatusCall;
		if (serverStatusCall.error != null)
		{
			Debug.LogError("Error retrieving server status: " + serverStatusCall.error);
			yield break;
		}
		string text = serverStatusCall.text;
		Debug.Log("Server status text: " + text);
		string[] array = text.Split('\n');
		if (array[0] == "offline")
		{
			timeLeftTillServerOnline = 1000;
		}
		else if (array[0] == "online")
		{
			timeLeftTillServerOnline = 0;
		}
		serverStatusText = array[1];
		if (array[2] == "EU")
		{
			serverRegion = 1;
		}
		else if (array[2] == "US")
		{
			serverRegion = 3;
		}
		else if (array[2] == "JP")
		{
			serverRegion = 4;
		}
		else
		{
			serverRegion = 5;
		}
	}

	private string LevelRotationToString()
	{
		string text = string.Empty;
		ulong[] array = levelRotation;
		foreach (ulong num in array)
		{
			text = text + num + ",";
		}
		if (text.Length != 0)
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}

	public static ulong[] ReadLevelRotationFromString(string rotation)
	{
		string[] array = rotation.Split(',');
		ulong[] array2 = new ulong[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = ulong.Parse(array[i]);
		}
		return array2;
	}

	public void SetLevelRotation(ulong[] newLevelRotation)
	{
		levelRotation = newLevelRotation;
	}

	public string GetMultiplayerStatusMessage()
	{
		if (multiplayerState == MultiplayerState.Disconnected)
		{
			return "Disconnected.";
		}
		if (multiplayerState == MultiplayerState.ConnectingToLobby)
		{
			return "Connecting to lobby.";
		}
		if (multiplayerState == MultiplayerState.ConnectedToLobby)
		{
			return "Connected to lobby.";
		}
		if (multiplayerState == MultiplayerState.ConnectingToServer)
		{
			return "Connecting to server.";
		}
		if (multiplayerState == MultiplayerState.ConnectedToServer)
		{
			return "Connected to server.";
		}
		if (multiplayerState == MultiplayerState.CreatingServer)
		{
			return "Creating server.";
		}
		if (multiplayerState == MultiplayerState.DownloadingLevels)
		{
			return $"Downloading levels: {Singleton<Workshop>.SP.GetTotalDownloads() - Singleton<Workshop>.SP.GetDownloadsLeft()}/{Singleton<Workshop>.SP.GetTotalDownloads()}.";
		}
		return "Unknown state.";
	}
}
