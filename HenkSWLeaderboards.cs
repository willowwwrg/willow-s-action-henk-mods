using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class HenkSWLeaderboards : Singleton<HenkSWLeaderboards>
{
	public bool operationTimedOut;

	private bool initialized;

	public bool currentlyRefreshingScores;

	public bool downloadingReplay;

	private string mostRecentlyDownloadedfile = string.Empty;

	private bool isSharingToCloud;

	public int numDownloadRetries = 5;

	private UGCHandle_t mostRecentlyDownloadedUGCHandle;

	public string highscoreSubmitStatus = string.Empty;

	private int playerNumberInGAUList = -1;

	public bool fetchedFirstPlayer;

	public SteamLeaderboard_t currentLeaderboardHandle;

	public SteamLeaderboard_t mostRecentlyCompletedLevelLeaderboardHandle;

	private Dictionary<string, SteamLeaderboard_t> allFetchedLeaderboards = new Dictionary<string, SteamLeaderboard_t>();

	public LeaderboardEntry_t[] globalLeaderboard;

	public LeaderboardEntry_t[] friendsLeaderboard;

	public LeaderboardEntry_t[] globalAroundUserLeaderboard;

	public LeaderboardEntry_t[] specificPlayerLeaderboardEntry;

	private bool fetchingSpecificPlayer;

	private string currentlyFetchingLeaderboardHandle = string.Empty;

	private ELeaderboardDataRequest currentlyFetchingLeaderboardType;

	private CallResult<LeaderboardFindResult_t> m_LeaderboardFindResult;

	private CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloaded;

	private CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoresUploaded;

	private CallResult<LeaderboardUGCSet_t> m_LeaderboardUGCSet;

	private CallResult<RemoteStorageFileShareResult_t> m_CloudFileShareResult;

	private CallResult<RemoteStorageDownloadUGCResult_t> m_CloudDownloadUGCResult;

	public ReplayController targetControllerForDownload;

	public bool currentlyDownloadingScoresForUserIDs;

	private CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloadedForUsers;

	private CallResult<RemoteStorageDownloadUGCResult_t> m_CloudDownloadUGCResultCustom;

	public ulong downloadingCustomReplay;

	public ReplayController targetControllerForCustomDownload;

	public Level retrievingScoreForLevel;

	private CallResult<LeaderboardFindResult_t> onLeaderboardFound;

	private CallResult<LeaderboardScoresDownloaded_t> onScoreDownloaded;

	private CallResult<LeaderboardFindResult_t> onLeaderboardFoundDaily;

	private CallResult<LeaderboardScoresDownloaded_t> onScoreDownloadedDaily;

	public bool Initialize()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		if (initialized)
		{
			return true;
		}
		initialized = true;
		m_LeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
		m_LeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
		m_LeaderboardScoresUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
		m_LeaderboardUGCSet = CallResult<LeaderboardUGCSet_t>.Create(OnLeaderboardUGCSet);
		m_CloudFileShareResult = CallResult<RemoteStorageFileShareResult_t>.Create(OnCloudFileShareResult);
		m_CloudDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnCloudDownloadUGCResult);
		m_LeaderboardScoresDownloadedForUsers = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloadedForUsers);
		m_CloudDownloadUGCResultCustom = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnCloudDownloadUGCResultCustom);
		onLeaderboardFound = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
		onScoreDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create(OnScoreDownloaded);
		onLeaderboardFoundDaily = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFoundDaily);
		onScoreDownloadedDaily = CallResult<LeaderboardScoresDownloaded_t>.Create(OnScoreDownloadedDaily);
		FetchCustomLeaderboardHandle("global_overall_2");
		return true;
	}

	public void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
	{
		string text = 1105.ToString();
		SteamLeaderboard_t hSteamLeaderboard = pCallback.m_hSteamLeaderboard;
		Debug.Log("[" + text + " - OnLeaderboardFindResult] - " + hSteamLeaderboard.ToString());
		if (pCallback.m_bLeaderboardFound != 0)
		{
			currentLeaderboardHandle = pCallback.m_hSteamLeaderboard;
			if (currentlyFetchingLeaderboardHandle != string.Empty && !allFetchedLeaderboards.ContainsKey(currentlyFetchingLeaderboardHandle))
			{
				allFetchedLeaderboards.Add(currentlyFetchingLeaderboardHandle, pCallback.m_hSteamLeaderboard);
			}
			Singleton<GetRoot>.SP.Get().BroadcastMessage("OnLeaderboardHandleFound", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			currentLeaderboardHandle = default(SteamLeaderboard_t);
			Debug.LogError("Leaderboard could not be found. Unable to submit a score for this level.");
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_DOWNLOADINGLEADERBOARDDATA", "PERMA"));
		}
		currentlyFetchingLeaderboardHandle = string.Empty;
	}

	public int GetNumberOfLeaderboardEntriesForCurrentLeaderboard()
	{
		if (currentLeaderboardHandle.m_SteamLeaderboard == 0L)
		{
			return 0;
		}
		return SteamUserStats.GetLeaderboardEntryCount(currentLeaderboardHandle);
	}

	private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_bSuccess == 0)
		{
			base.transform.root.BroadcastMessage("HenkSWSubmitScoreFailed");
			Debug.LogError("Score could not be uploaded to Steam.");
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_SUBMITTINGSCORE", "PERMA"));
			initialized = false;
		}
		else
		{
			Debug.Log("Score uploaded to steam, new highscore: " + pCallback.m_bScoreChanged);
			base.transform.root.BroadcastMessage("HenkSWScoreUploaded", pCallback.m_bScoreChanged == 1);
		}
	}

	private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		fetchedFirstPlayer = false;
		currentlyRefreshingScores = false;
		if (pCallback.m_cEntryCount == 0)
		{
			Singleton<HighscoreManager>.SP.FailedToConnect();
		}
		int num = 2;
		LeaderboardEntry_t[] array = new LeaderboardEntry_t[pCallback.m_cEntryCount];
		int[][] array2 = new int[pCallback.m_cEntryCount][];
		for (int i = 0; i < pCallback.m_cEntryCount; i++)
		{
			array2[i] = new int[num];
			SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out array[i], array2[i], num);
		}
		if (array.Length != 0 && array[0].m_nGlobalRank == 1)
		{
			fetchedFirstPlayer = true;
		}
		if (fetchingSpecificPlayer)
		{
			specificPlayerLeaderboardEntry = array;
			Singleton<GetRoot>.SP.Get().BroadcastMessage("SpecificLeaderboardEntriesDownloaded", currentlyFetchingLeaderboardType, SendMessageOptions.DontRequireReceiver);
			Debug.Log("Found specific leaderboard entry.");
			fetchingSpecificPlayer = false;
			return;
		}
		Level levelFromLeaderboardHandle = GetLevelFromLeaderboardHandle(pCallback.m_hSteamLeaderboard);
		Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(array, levelFromLeaderboardHandle, array2);
		if (currentlyFetchingLeaderboardType == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal)
		{
			globalLeaderboard = array;
		}
		else if (currentlyFetchingLeaderboardType == ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends)
		{
			friendsLeaderboard = array;
		}
		else if (currentlyFetchingLeaderboardType == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser)
		{
			globalAroundUserLeaderboard = array;
		}
		Debug.Log(string.Concat("refreshed", currentlyFetchingLeaderboardType, ", numEntries: ", pCallback.m_cEntryCount.ToString()));
		Singleton<GetRoot>.SP.Get().BroadcastMessage("LeaderboardEntriesDownloaded", currentlyFetchingLeaderboardType, SendMessageOptions.DontRequireReceiver);
	}

	private void OnLeaderboardUGCSet(LeaderboardUGCSet_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError("Error while attaching UGC to leaderboard: " + pCallback.m_eResult);
		}
		Singleton<GetRoot>.SP.Get().BroadcastMessage("DoneUploadingHighscores", true, SendMessageOptions.DontRequireReceiver);
	}

	private void OnCloudFileShareResult(RemoteStorageFileShareResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK && isSharingToCloud)
		{
			Debug.Log("Share result: " + pCallback.m_eResult);
			Debug.Log("Attach to leaderboard");
			SteamAPICall_t hAPICall = SteamUserStats.AttachLeaderboardUGC(mostRecentlyCompletedLevelLeaderboardHandle, pCallback.m_hFile);
			m_LeaderboardUGCSet.Set(hAPICall);
		}
		isSharingToCloud = false;
	}

	private void OnCloudDownloadUGCResult(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
	{
		ulong uGCHandle = pCallback.m_hFile.m_UGCHandle;
		Debug.Log("Downloaded file: " + pCallback.m_pchFileName + " - " + uGCHandle + " - " + pCallback.m_eResult);
		downloadingReplay = false;
		if (pCallback.m_pchFileName == string.Empty)
		{
			if (numDownloadRetries > 0)
			{
				MonoBehaviour.print("Download file failed, trying again.");
				DownloadUGC(mostRecentlyDownloadedUGCHandle);
			}
			else
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
			}
		}
		else if (!(pCallback.m_pchFileName == mostRecentlyDownloadedfile))
		{
			ReadGhostFromResult(pCallback, targetControllerForDownload);
			mostRecentlyDownloadedfile = pCallback.m_pchFileName;
		}
	}

	private void ReadGhostFromResult(RemoteStorageDownloadUGCResult_t pCallback, ReplayController controllerToFillIn)
	{
		if (controllerToFillIn == null)
		{
			return;
		}
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError("Error while downloading replay: " + pCallback.m_eResult);
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
			controllerToFillIn.ClearReplay();
			return;
		}
		controllerToFillIn.SetSteamID(pCallback.m_ulSteamIDOwner);
		byte[] array = new byte[pCallback.m_nSizeInBytes];
		SteamRemoteStorage.UGCRead(pCallback.m_hFile, array, pCallback.m_nSizeInBytes, 0u, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
		string data = GetString(array);
		if (!controllerToFillIn.ReadReplay(data, string.Empty))
		{
			controllerToFillIn.ClearReplay();
		}
	}

	public void SaveReplayOnSteam(string dataString)
	{
		_ = string.Empty;
		string pchFile = "lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "-" + Singleton<HenkSWUserStats>.SP.GetSteamID().ToString() + ".dat";
		byte[] bytes = GetBytes(dataString);
		Debug.Log("write");
		SteamRemoteStorage.FileWrite(pchFile, bytes, bytes.Length);
		isSharingToCloud = true;
		Debug.Log("share");
		highscoreSubmitStatus = Language.Get("SUBMITTINGDATA", "GAME");
		SteamAPICall_t hAPICall = SteamRemoteStorage.FileShare(pchFile);
		m_CloudFileShareResult.Set(hAPICall);
	}

	public static byte[] GetBytes(string str)
	{
		byte[] array = new byte[str.Length * 2];
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	public static string GetString(byte[] bytes)
	{
		char[] array = new char[bytes.Length / 2];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return new string(array);
	}

	public bool RefreshCurrentLeaderboard()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		if (!initialized)
		{
			Initialize();
		}
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer)
		{
			return true;
		}
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		string empty = string.Empty;
		empty = ((!(currentLevelObj.workshopFolderName != string.Empty)) ? ("Lvl_" + currentLevelObj.levelCode + "_" + currentLevelObj.levelVersion) : ((currentLevelObj.levelVersion == 0) ? ("WORKSHOP_" + currentLevelObj.guid) : ("WORKSHOP_" + currentLevelObj.guid + "_" + currentLevelObj.levelVersion)));
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			empty = "Daily_" + Singleton<MutatorManager>.SP.seedOfToday;
		}
		currentLeaderboardHandle = default(SteamLeaderboard_t);
		currentlyFetchingLeaderboardHandle = empty;
		SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(empty, ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds);
		m_LeaderboardFindResult.Set(hAPICall);
		return true;
	}

	public bool SetLeaderboardForLevel(Level level)
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		currentLeaderboardHandle = default(SteamLeaderboard_t);
		currentlyFetchingLeaderboardHandle = "Lvl_" + level.levelCode + "_" + level.levelVersion;
		SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(currentlyFetchingLeaderboardHandle, ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds);
		m_LeaderboardFindResult.Set(hAPICall);
		return true;
	}

	public bool FetchCustomLeaderboardHandle(string leaderboardName, ELeaderboardSortMethod sortMethodIfCreating = ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending)
	{
		if (!SteamManager.Initialized || leaderboardName == string.Empty)
		{
			return false;
		}
		MonoBehaviour.print("fetching leaderboard " + leaderboardName);
		currentLeaderboardHandle = default(SteamLeaderboard_t);
		currentlyFetchingLeaderboardHandle = leaderboardName;
		SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, sortMethodIfCreating, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
		m_LeaderboardFindResult.Set(hAPICall);
		return true;
	}

	public bool SetCurrentLeaderboardHandle(string leaderboardName)
	{
		if (allFetchedLeaderboards.TryGetValue(leaderboardName, out var value))
		{
			currentLeaderboardHandle = value;
			return true;
		}
		return false;
	}

	public void SetLevelCompletedLeaderboardHandle()
	{
		if (currentLeaderboardHandle.m_SteamLeaderboard == 0L)
		{
			mostRecentlyCompletedLevelLeaderboardHandle = default(SteamLeaderboard_t);
		}
		else
		{
			mostRecentlyCompletedLevelLeaderboardHandle = currentLeaderboardHandle;
		}
	}

	public bool SubmitScore()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		Debug.Log("submitting score");
		if (!initialized || mostRecentlyCompletedLevelLeaderboardHandle.m_SteamLeaderboard == 0L)
		{
			base.transform.root.BroadcastMessage("HenkSWSubmitScoreFailed");
			Debug.LogError("failed submitting score");
			return false;
		}
		SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(mostRecentlyCompletedLevelLeaderboardHandle, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (int)(Singleton<CheckpointManager>.SP.GetFinishTime() * 1000f), null, 0);
		m_LeaderboardScoresUploaded.Set(hAPICall);
		return true;
	}

	public bool SubmitCustomScore(int newScore, int[] scoreCustomDetails = null, bool keepBest = true)
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		Debug.Log("submitting custom score " + newScore);
		if (!initialized || currentLeaderboardHandle.m_SteamLeaderboard == 0L)
		{
			base.transform.root.BroadcastMessage("HenkSWSubmitScoreMultiplayerResult", false, SendMessageOptions.DontRequireReceiver);
			Debug.LogError("failed submitting multiplayer score");
			return false;
		}
		int cScoreDetailsCount = 0;
		if (scoreCustomDetails != null)
		{
			cScoreDetailsCount = scoreCustomDetails.Length;
		}
		ELeaderboardUploadScoreMethod eLeaderboardUploadScoreMethod = (keepBest ? ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest : ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);
		SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(currentLeaderboardHandle, eLeaderboardUploadScoreMethod, newScore, scoreCustomDetails, cScoreDetailsCount);
		m_LeaderboardScoresUploaded.Set(hAPICall);
		return true;
	}

	public bool DownloadSpecificLeaderboardEntry(int rank)
	{
		fetchingSpecificPlayer = true;
		SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboardHandle, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, rank, rank + 1);
		m_LeaderboardScoresDownloaded.Set(hAPICall);
		return true;
	}

	public bool DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest requestType, bool onlyGetWorldChamp = false, bool onlyGetSelf = false, int pageNum = 0)
	{
		if (currentlyRefreshingScores)
		{
			return false;
		}
		highscoreSubmitStatus = Language.Get("FETCHING", "LEADERBOARD");
		currentlyRefreshingScores = true;
		currentlyFetchingLeaderboardType = requestType;
		Debug.Log("Fetching entries for: " + requestType);
		SteamAPICall_t hAPICall;
		if (requestType == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser)
		{
			hAPICall = ((!onlyGetSelf) ? SteamUserStats.DownloadLeaderboardEntries(currentLeaderboardHandle, requestType, -5 + pageNum * 10, 4 + pageNum * 10) : SteamUserStats.DownloadLeaderboardEntries(currentLeaderboardHandle, requestType, 0, 0));
		}
		else if (onlyGetWorldChamp)
		{
			hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboardHandle, requestType, 1, 1);
		}
		else
		{
			int num = 10;
			hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboardHandle, requestType, 1 + pageNum * num, num + pageNum * num);
		}
		m_LeaderboardScoresDownloaded.Set(hAPICall);
		return true;
	}

	public bool GetGlobalNumberOneReplay()
	{
		if (globalLeaderboard.Length != 0)
		{
			mostRecentlyDownloadedUGCHandle = globalLeaderboard[0].m_hUGC;
			if (globalLeaderboard[0].m_steamIDUser.m_SteamID == 76561197990024845L)
			{
				AudioController.Play("kakujo");
			}
			numDownloadRetries = 5;
			DownloadUGC(mostRecentlyDownloadedUGCHandle);
			GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
			foreach (GameObject gameObject in allPlayers)
			{
				if (gameObject.GetComponent<PlatformerController>().isExternalControlled)
				{
					gameObject.GetComponent<PlayerGraphics>().ghostNameLabel.SetGhostName(Singleton<HenkSWUserStats>.SP.GetNameBySteamID(globalLeaderboard[0].m_steamIDUser));
				}
			}
			return true;
		}
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
		Debug.LogError("Error while trying to download replay, no leaderboard entries.");
		downloadingReplay = false;
		return false;
	}

	public bool GetSpecificPlayerReplay()
	{
		if (specificPlayerLeaderboardEntry.Length != 0)
		{
			mostRecentlyDownloadedUGCHandle = specificPlayerLeaderboardEntry[0].m_hUGC;
			numDownloadRetries = 5;
			DownloadUGC(mostRecentlyDownloadedUGCHandle);
			return true;
		}
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
		Debug.LogError("Error while trying to download replay, no leaderboard entries.");
		downloadingReplay = false;
		return false;
	}

	public bool GetPersonalBestReplay()
	{
		if (globalAroundUserLeaderboard.Length != 0)
		{
			playerNumberInGAUList = -1;
			for (int i = 0; i < globalAroundUserLeaderboard.Length; i++)
			{
				if (globalAroundUserLeaderboard[i].m_steamIDUser == Singleton<HenkSWUserStats>.SP.GetSteamID())
				{
					playerNumberInGAUList = i;
				}
			}
			if (playerNumberInGAUList != -1)
			{
				mostRecentlyDownloadedUGCHandle = globalAroundUserLeaderboard[playerNumberInGAUList].m_hUGC;
				numDownloadRetries = 5;
				DownloadUGC(mostRecentlyDownloadedUGCHandle);
				return true;
			}
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
			Debug.LogError("Error while trying to download own replay, no leaderboard entries.");
			downloadingReplay = false;
			return false;
		}
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
		Debug.LogError("Error while trying to download own replay, no leaderboard entries.");
		downloadingReplay = false;
		return false;
	}

	public bool GetFriendReplay(ulong playerID)
	{
		if (playerID == 0L)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
			Debug.LogError("Error while trying to download replay, not a valid friend ID");
			downloadingReplay = false;
			return false;
		}
		if (friendsLeaderboard.Length < 1)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
			Debug.LogError("Error while trying to download replay, no friend entries in leaderboard.");
			downloadingReplay = false;
			return false;
		}
		for (int i = 0; i < friendsLeaderboard.Length; i++)
		{
			if (friendsLeaderboard[i].m_steamIDUser.m_SteamID == playerID)
			{
				mostRecentlyDownloadedUGCHandle = friendsLeaderboard[i].m_hUGC;
				numDownloadRetries = 5;
				DownloadUGC(mostRecentlyDownloadedUGCHandle);
				return true;
			}
		}
		Debug.LogError("Error while trying to download replay, not a valid friend ID.");
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDDOWNLOADREPLAY", "PERMA"));
		downloadingReplay = false;
		return false;
	}

	public void DownloadUGC(UGCHandle_t fileHandle)
	{
		downloadingReplay = true;
		numDownloadRetries--;
		mostRecentlyDownloadedfile = string.Empty;
		Debug.Log("Start downloading replay. Tries remaining: " + numDownloadRetries);
		SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(mostRecentlyDownloadedUGCHandle, 0u);
		m_CloudDownloadUGCResult.Set(hAPICall);
		TimeOutCheck();
	}

	private IEnumerator TimeOutCheck()
	{
		operationTimedOut = false;
		yield return new WaitForSeconds(6f);
		if (!operationTimedOut)
		{
			base.transform.root.BroadcastMessage("SteamworksTimeOut", SendMessageOptions.DontRequireReceiver);
		}
		operationTimedOut = true;
	}

	public static float ScoreIntToFloat(int score)
	{
		return (float)score / 1000f;
	}

	public LeaderboardEntry_t[] GetScoreListFromType(HighscoreState scoreState)
	{
		switch (scoreState)
		{
		case HighscoreState.Friends:
			return friendsLeaderboard;
		case HighscoreState.Global:
			return globalLeaderboard;
		case HighscoreState.GlobalNearUser:
			return globalAroundUserLeaderboard;
		default:
			Debug.LogError("Trying to fill in list with inexisting highscorestate: " + scoreState);
			return new LeaderboardEntry_t[0];
		}
	}

	public Level GetLevelFromLeaderboardHandle(SteamLeaderboard_t handle)
	{
		if (!allFetchedLeaderboards.ContainsValue(handle))
		{
			return null;
		}
		foreach (KeyValuePair<string, SteamLeaderboard_t> allFetchedLeaderboard in allFetchedLeaderboards)
		{
			if (allFetchedLeaderboard.Value == handle)
			{
				string key = allFetchedLeaderboard.Key;
				string[] array = key.Split('_');
				if (key.StartsWith("Lvl_") && array.Length > 1)
				{
					int levelCode = int.Parse(array[1]);
					return Singleton<LevelBatchManager>.SP.GetLevelFromCode(levelCode);
				}
				if (key.StartsWith("WORKSHOP_") && array.Length > 1)
				{
					return Singleton<LevelBatchManager>.SP.GetLevelFromGuid(array[1]);
				}
				return null;
			}
		}
		return null;
	}

	public bool GetCustomReplay(ulong playerID)
	{
		MonoBehaviour.print("Downloading custom replay " + playerID);
		if (playerID == 0L || !SteamManager.Initialized)
		{
			Debug.LogError("Error while trying to download replay, not a valid ID");
			return false;
		}
		if (playerID == 76561197990024845L)
		{
			AudioController.Play("kakujo");
		}
		downloadingReplay = true;
		currentlyDownloadingScoresForUserIDs = true;
		CSteamID[] prgUsers = new CSteamID[1]
		{
			new CSteamID(playerID)
		};
		SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntriesForUsers(currentLeaderboardHandle, prgUsers, 1);
		m_LeaderboardScoresDownloadedForUsers.Set(hAPICall);
		StartCoroutine("CustomIDTimeout");
		return true;
	}

	private IEnumerator CustomIDTimeout()
	{
		yield return new WaitForSeconds(5f);
		currentlyDownloadingScoresForUserIDs = false;
		Debug.LogWarning("Leaderboard Timeout for custom id");
		if ((bool)targetControllerForCustomDownload)
		{
			UnityEngine.Object.Destroy(targetControllerForCustomDownload.gameObject);
		}
		m_LeaderboardScoresDownloadedForUsers.Cancel();
		Singleton<IRCManager>.SP.SendIRCMessage("Couldn't find a ghost on this level for " + Singleton<IRCManager>.SP.currentNicknameSpawning);
	}

	private void OnLeaderboardScoresDownloadedForUsers(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		MonoBehaviour.print("Downloaded entries: " + pCallback.m_cEntryCount);
		currentlyDownloadingScoresForUserIDs = false;
		StopCoroutine("CustomIDTimeout");
		if (pCallback.m_cEntryCount == 1)
		{
			LeaderboardEntry_t pLeaderboardEntry = default(LeaderboardEntry_t);
			SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, 0, out pLeaderboardEntry, null, 0);
			downloadingCustomReplay = pLeaderboardEntry.m_hUGC.m_UGCHandle;
			SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(pLeaderboardEntry.m_hUGC, 0u);
			m_CloudDownloadUGCResultCustom.Set(hAPICall);
		}
		else
		{
			Debug.LogWarning("Couldn't find leaderboard entry for custom ID");
			if ((bool)targetControllerForCustomDownload)
			{
				UnityEngine.Object.Destroy(targetControllerForCustomDownload.gameObject);
			}
			Singleton<IRCManager>.SP.SendIRCMessage("Couldn't find a ghost on this level for " + Singleton<IRCManager>.SP.currentNicknameSpawning);
		}
	}

	private void OnCloudDownloadUGCResultCustom(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
	{
		ulong uGCHandle = pCallback.m_hFile.m_UGCHandle;
		downloadingCustomReplay = 0uL;
		downloadingReplay = false;
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogWarning("Couldn't download replay file");
			Singleton<IRCManager>.SP.SendIRCMessage("Couldn't download replay file for " + Singleton<IRCManager>.SP.currentNicknameSpawning);
			if ((bool)targetControllerForCustomDownload)
			{
				UnityEngine.Object.Destroy(targetControllerForCustomDownload.gameObject);
			}
			return;
		}
		Debug.Log("Downloaded custom file: " + pCallback.m_pchFileName + " - " + uGCHandle + " - " + pCallback.m_eResult);
		Singleton<IRCManager>.SP.SendIRCMessage("Success! The ghost for " + Singleton<IRCManager>.SP.currentNicknameSpawning + " should appear shortly.");
		Singleton<IRCManager>.SP.ShowNotification("Spawning ghost from " + Singleton<IRCManager>.SP.currentNicknameSpawning);
		Singleton<GAManager>.SP.IRCSpawn(Singleton<IRCManager>.SP.currentNicknameSpawning);
		ReadGhostFromResult(pCallback, targetControllerForCustomDownload);
	}

	public bool RetrieveScoreForLevel(Level level)
	{
		if (!SteamManager.Initialized || retrievingScoreForLevel != null)
		{
			return false;
		}
		string pchLeaderboardName = "Lvl_" + level.levelCode + "_" + level.levelVersion;
		retrievingScoreForLevel = level;
		SteamAPICall_t hAPICall = SteamUserStats.FindLeaderboard(pchLeaderboardName);
		onLeaderboardFound.Set(hAPICall);
		return true;
	}

	public void OnLeaderboardFound(LeaderboardFindResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_bLeaderboardFound != 0)
		{
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(pCallback.m_hSteamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, 0, 0);
			onScoreDownloaded.Set(hAPICall);
		}
		else
		{
			retrievingScoreForLevel = null;
		}
	}

	private void OnScoreDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_cEntryCount == 0)
		{
			retrievingScoreForLevel.steamScore = 0;
		}
		else if (pCallback.m_cEntryCount == 1)
		{
			LeaderboardEntry_t pLeaderboardEntry = default(LeaderboardEntry_t);
			SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, 0, out pLeaderboardEntry, null, 0);
			retrievingScoreForLevel.steamScore = pLeaderboardEntry.m_nScore;
		}
		retrievingScoreForLevel = null;
	}

	public bool GetDailyChallengeRank()
	{
		if (!SteamManager.Initialized)
		{
			Singleton<MutatorManager>.SP.currentRank = "N/A";
			return false;
		}
		if (!initialized)
		{
			Initialize();
		}
		Singleton<MutatorManager>.SP.currentRank = "...";
		SteamAPICall_t hAPICall = SteamUserStats.FindLeaderboard("Daily_" + Singleton<MutatorManager>.SP.seedOfToday);
		onLeaderboardFoundDaily.Set(hAPICall);
		return true;
	}

	public void OnLeaderboardFoundDaily(LeaderboardFindResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_bLeaderboardFound != 0)
		{
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(pCallback.m_hSteamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, 0, 0);
			onScoreDownloadedDaily.Set(hAPICall);
		}
		else
		{
			Singleton<MutatorManager>.SP.currentRank = "N/A";
		}
	}

	private void OnScoreDownloadedDaily(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_cEntryCount == 0)
		{
			Singleton<MutatorManager>.SP.currentRank = "N/A";
		}
		else if (pCallback.m_cEntryCount == 1)
		{
			LeaderboardEntry_t pLeaderboardEntry = default(LeaderboardEntry_t);
			SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, 0, out pLeaderboardEntry, null, 0);
			Singleton<MutatorManager>.SP.currentRank = pLeaderboardEntry.m_nGlobalRank + "/" + SteamUserStats.GetLeaderboardEntryCount(pCallback.m_hSteamLeaderboard);
		}
	}
}
