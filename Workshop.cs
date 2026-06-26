using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

public class Workshop : Singleton<Workshop>
{
	private List<CallResult<SteamUGCRequestUGCDetailsResult_t>> m_UGCRequestDetails;

	private Callback<RemoteStoragePublishedFileSubscribed_t> m_UGCSubscribed;

	private Callback<RemoteStoragePublishedFileUnsubscribed_t> m_UGCUnsubscribed;

	private CallResult<CreateItemResult_t> m_CreateItemResult;

	private CallResult<SubmitItemUpdateResult_t> m_SubmitItemUpdateResult;

	private CallResult<SteamUGCQueryCompleted_t> m_SteamUGCQueryCompleted;

	private List<CallResult<RemoteStorageUserVoteDetails_t>> m_GetPublishedItemVoteDetails;

	private List<CallResult<RemoteStorageGetPublishedFileDetailsResult_t>> m_GetPublishedFileDetails;

	private CallResult<RemoteStorageUpdateUserPublishedItemVoteResult_t> m_UpdateUserPublishedItemVote;

	private List<CallResult<SteamUGCRequestUGCDetailsResult_t>> m_UGCFileDetails;

	protected Callback<ItemInstalled_t> m_ItemInstalled;

	private bool initialized;

	public bool submitting;

	private ulong submittedItemID;

	private List<string> workshopTags = new List<string>();

	public List<PublishedFileId_t> publishedFileIDs = new List<PublishedFileId_t>();

	private string fileDirectory;

	private string levelFileName;

	public bool validating;

	public bool validationMsgShown;

	private PublishedFileId_t currentLevelPublishedFileID;

	private int numQueries;

	private int numVoteQueries;

	private int numItemCreatorQueries;

	private uint queryPageNum = 1u;

	private ulong[] levelIDsToDownload;

	private int downloadsLeft;

	private void Start()
	{
		Initialize();
	}

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
		m_UGCFileDetails = new List<CallResult<SteamUGCRequestUGCDetailsResult_t>>();
		m_UGCRequestDetails = new List<CallResult<SteamUGCRequestUGCDetailsResult_t>>();
		m_UGCSubscribed = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(OnUGCSubscribed);
		m_UGCUnsubscribed = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnUGCUnsubscribed);
		m_CreateItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
		m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
		m_SteamUGCQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);
		m_GetPublishedItemVoteDetails = new List<CallResult<RemoteStorageUserVoteDetails_t>>();
		m_GetPublishedFileDetails = new List<CallResult<RemoteStorageGetPublishedFileDetailsResult_t>>();
		m_UpdateUserPublishedItemVote = CallResult<RemoteStorageUpdateUserPublishedItemVoteResult_t>.Create(OnUpdatePublishedItemVote);
		m_ItemInstalled = Callback<ItemInstalled_t>.Create(OnItemInstalled);
		workshopTags.Add("Action Henk");
		workshopTags.Add("Level");
		initialized = true;
		return true;
	}

	private void Update()
	{
		_ = SteamManager.Initialized;
	}

	public void SubmitLevelToWorkshop(string directory, string levelName)
	{
		submitting = true;
		submittedItemID = 0uL;
		fileDirectory = directory;
		levelFileName = levelName;
		if (!Directory.Exists(directory))
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_SAVINGLEVEL", "PERMA"));
			Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
		}
		else if (SteamManager.Initialized)
		{
			ulong pFIDFromWorkshopFile = HenkUtils.GetPFIDFromWorkshopFile(Singleton<LevelBatchManager>.SP.currentLevel.guid.ToString());
			if (pFIDFromWorkshopFile != 0L)
			{
				currentLevelPublishedFileID.m_PublishedFileId = pFIDFromWorkshopFile;
			}
			if (currentLevelPublishedFileID.m_PublishedFileId.ToString() != "0")
			{
				Debug.Log("Update workshop file.");
				UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), currentLevelPublishedFileID);
				submittedItemID = currentLevelPublishedFileID.m_PublishedFileId;
				FillInUpdateHandle(handle, isUpdate: true);
			}
			else
			{
				Debug.Log("Create workshop file.");
				SteamAPICall_t hAPICall = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst);
				m_CreateItemResult.Set(hAPICall);
			}
		}
	}

	public void SetLevelPlayed(PublishedFileId_t fileID)
	{
		SteamRemoteStorage.SetUserPublishedFileAction(fileID, EWorkshopFileAction.k_EWorkshopFileActionPlayed);
	}

	public void SetLevelCompleted()
	{
	}

	public void SubmitCurrentLevelToWorkshop()
	{
		string levelName = Singleton<LevelEditorFileWriter>.SP.levelName;
		string directory = Application.dataPath + "/../CustomLevels/" + Singleton<LevelBatchManager>.SP.currentLevel.guid.ToString() + "/";
		SubmitLevelToWorkshop(directory, levelName);
	}

	public void OnCreateItemResult(CreateItemResult_t pCallback, bool bIOFailure)
	{
		MonoBehaviour.print("Create item result: " + pCallback.m_eResult);
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDWORKSHOPCONNECT", "PERMA"));
			submitting = false;
			return;
		}
		submittedItemID = pCallback.m_nPublishedFileId.m_PublishedFileId;
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj() != null)
		{
			HenkUtils.WritePFIDInWorkshopFile(submittedItemID, Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().guid);
		}
		UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), pCallback.m_nPublishedFileId);
		FillInUpdateHandle(handle, isUpdate: false);
	}

	private void FillInUpdateHandle(UGCUpdateHandle_t handle, bool isUpdate)
	{
		SteamUGC.SetItemTitle(handle, levelFileName);
		if (File.Exists(fileDirectory + "preview.png"))
		{
			SteamUGC.SetItemPreview(handle, fileDirectory + "preview.png");
		}
		SteamUGC.SetItemDescription(handle, Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().workshopLevelDescription);
		SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
		SteamUGC.SetItemContent(handle, fileDirectory);
		SteamAPICall_t hAPICall;
		if (isUpdate)
		{
			hAPICall = SteamUGC.SubmitItemUpdate(handle, "Update.");
		}
		else
		{
			SteamUGC.SetItemDescription(handle, "Item description.");
			hAPICall = SteamUGC.SubmitItemUpdate(handle, "Initial commit.");
		}
		m_SubmitItemUpdateResult.Set(hAPICall);
	}

	public void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
	{
		Debug.Log("Workshop item submit result: " + pCallback.m_eResult);
		submitting = false;
		string path = Application.dataPath + "/Resources/../../CustomLevels/" + Singleton<LevelBatchManager>.SP.currentLevel.guid.ToString() + "/preview.png";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDWORKSHOPCONNECT", "PERMA"));
			Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
			return;
		}
		if (pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			if (!Application.isEditor)
			{
				SteamFriends.ActivateGameOverlayToWebPage("http://steamcommunity.com/sharedfiles/workshoplegalagreement");
			}
		}
		else
		{
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.UploadCustomLevel);
			OpenSteamOverlayWorkshopItem(submittedItemID);
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("LEVELSUBMITTED", "LEVELEDITOR"));
		Singleton<Workshop>.SP.CheckIfCurrentLevelExistsInWorkshop();
	}

	public void OnUGCSubscribed(RemoteStoragePublishedFileSubscribed_t pCallback)
	{
		if (!(pCallback.m_nAppID != SteamUtils.GetAppID()))
		{
			MonoBehaviour.print("subbed to: " + pCallback.m_nPublishedFileId.ToString());
		}
	}

	private void OnItemInstalled(ItemInstalled_t pCallback)
	{
		Debug.Log(string.Concat("[", 3405, " - ItemInstalled_t] - ", pCallback.m_unAppID, " -- ", pCallback.m_nPublishedFileId));
		Singleton<LevelBatchManager>.SP.RefreshWorkshopLevels();
	}

	public void OnUGCUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t pCallback)
	{
		_ = pCallback.m_nAppID != SteamUtils.GetAppID();
	}

	public void UnsubscribeFrom(Level level)
	{
		if (!SteamManager.Initialized)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDSTEAMCONNECT", "PERMA"));
			return;
		}
		SteamUGC.UnsubscribeItem(new PublishedFileId_t
		{
			m_PublishedFileId = level.publishedFiledID
		});
	}

	public void SubscribeTo(ulong levelId)
	{
		if (!SteamManager.Initialized)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDSTEAMCONNECT", "PERMA"));
		}
		else
		{
			SteamUGC.SubscribeItem((PublishedFileId_t)levelId);
		}
	}

	public void OpenSteamOverlayWorkshop()
	{
		if (SteamManager.Initialized && !Application.isEditor)
		{
			SteamFriends.ActivateGameOverlayToWebPage("http://steamcommunity.com/app/285820/workshop/");
		}
	}

	public void OpenSteamOverlayWorkshopItem(ulong itemNum)
	{
		if (SteamManager.Initialized && !Application.isEditor)
		{
			SteamFriends.ActivateGameOverlayToWebPage("http://steamcommunity.com/sharedfiles/filedetails/?id=" + itemNum);
		}
	}

	private void OnUGCRequestDetails(SteamUGCRequestUGCDetailsResult_t pCallback, bool bIOFailure)
	{
		if (numQueries > 0)
		{
			if (pCallback.m_details.m_rgchTitle == Singleton<LevelEditorFileWriter>.SP.levelName)
			{
				currentLevelPublishedFileID = pCallback.m_details.m_nPublishedFileId;
				numQueries = 0;
			}
			else
			{
				numQueries--;
			}
		}
	}

	public void OnQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			if (pCallback.m_unNumResultsReturned == 50)
			{
				queryPageNum++;
				UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, SteamUtils.GetAppID(), SteamUtils.GetAppID(), queryPageNum);
				m_SteamUGCQueryCompleted.Set(SteamUGC.SendQueryUGCRequest(handle));
			}
			m_UGCRequestDetails.Clear();
			for (uint num = 0u; num < pCallback.m_unNumResultsReturned; num++)
			{
				SteamUGCDetails_t pDetails = default(SteamUGCDetails_t);
				SteamUGC.GetQueryUGCResult(pCallback.m_handle, num, out pDetails);
				m_UGCRequestDetails.Add(CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(OnUGCRequestDetails));
				m_UGCRequestDetails[m_UGCRequestDetails.Count - 1].Set(SteamUGC.RequestUGCDetails(pDetails.m_nPublishedFileId, 0u));
				numQueries++;
			}
		}
	}

	public void GetSubscribedItemDetails()
	{
		ulong[] allWorkshopItems = GetAllWorkshopItems();
		Singleton<LevelBatchManager>.SP.workshopLevelDetailQueryCount = allWorkshopItems.Length;
		m_UGCFileDetails.Clear();
		for (int i = 0; i < allWorkshopItems.Length; i++)
		{
			m_UGCFileDetails.Add(CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(OnSubscribedItemDetailsRequest));
			PublishedFileId_t nPublishedFileID = new PublishedFileId_t
			{
				m_PublishedFileId = allWorkshopItems[i]
			};
			m_UGCFileDetails[m_UGCFileDetails.Count - 1].Set(SteamUGC.RequestUGCDetails(nPublishedFileID, 0u));
		}
	}

	private void OnSubscribedItemDetailsRequest(SteamUGCRequestUGCDetailsResult_t pCallback, bool bIOFailure)
	{
		SteamUGCDetails_t details = pCallback.m_details;
		ulong publishedFileId = details.m_nPublishedFileId.m_PublishedFileId;
		if (!GetItemInstallInfo(publishedFileId, out var folder))
		{
			MonoBehaviour.print("Level " + publishedFileId + " subscribed but not downloaded yet, skipping");
			Singleton<LevelBatchManager>.SP.workshopLevelDetailQueryCount--;
			return;
		}
		if (!Directory.Exists(folder))
		{
			MonoBehaviour.print("Level " + publishedFileId + " folder doesn't exist");
			Singleton<LevelBatchManager>.SP.workshopLevelDetailQueryCount--;
			return;
		}
		string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
		if (files.Length != 1)
		{
			Singleton<LevelBatchManager>.SP.workshopLevelDetailQueryCount--;
			return;
		}
		string fileName = files[0];
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = Singleton<ActionHenk>.SP.levels.transform;
		Level level = gameObject.AddComponent<Level>();
		FileInfo fileInfo = new FileInfo(fileName);
		string a_levelName = string.Empty;
		string a_guid = string.Empty;
		int a_versionNum = -1;
		HenkUtils.GetNameAndGUIDFromLevelFile(out a_levelName, out a_guid, fileInfo);
		a_levelName = details.m_rgchTitle;
		HenkUtils.GetLevelVersionFromLevelFile(out a_versionNum, fileInfo);
		level.levelName = a_levelName;
		level.guid = a_guid;
		gameObject.name = a_levelName;
		level.levelCode = -1;
		level.isSceneLess = true;
		level.isEditorLevel = true;
		level.levelType = LevelType.Workshop;
		level.workshopFolderName = fileInfo.DirectoryName;
		level.publishedFiledID = publishedFileId;
		level.levelVersion = -1;
		level.workshopLevelDescription = details.m_rgchDescription;
		Singleton<LevelBatchManager>.SP.WorkshopLevelDetails(level);
		Singleton<LevelBatchManager>.SP.workshopLevelDetailQueryCount--;
	}

	public void CheckIfCurrentLevelExistsInWorkshop()
	{
		numQueries = 0;
		queryPageNum = 1u;
		currentLevelPublishedFileID = default(PublishedFileId_t);
		if (SteamManager.Initialized)
		{
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, SteamUtils.GetAppID(), SteamUtils.GetAppID(), queryPageNum);
			m_SteamUGCQueryCompleted.Set(SteamUGC.SendQueryUGCRequest(handle));
		}
	}

	public void SetItemVote(ulong fileID, bool upvote)
	{
		SteamAPICall_t hAPICall = SteamRemoteStorage.UpdateUserPublishedItemVote(new PublishedFileId_t
		{
			m_PublishedFileId = fileID
		}, upvote);
		m_UpdateUserPublishedItemVote.Set(hAPICall);
	}

	public void GetItemVotes(List<ulong> fileIDs)
	{
		if (numVoteQueries <= 0 && SteamManager.Initialized)
		{
			m_GetPublishedItemVoteDetails.Clear();
			numVoteQueries = 0;
			for (int i = 0; i < fileIDs.Count; i++)
			{
				SteamAPICall_t userPublishedItemVoteDetails = SteamRemoteStorage.GetUserPublishedItemVoteDetails(new PublishedFileId_t
				{
					m_PublishedFileId = fileIDs[i]
				});
				m_GetPublishedItemVoteDetails.Add(CallResult<RemoteStorageUserVoteDetails_t>.Create(OnGetPublishedItemVoteDetails));
				m_GetPublishedItemVoteDetails[m_GetPublishedItemVoteDetails.Count - 1].Set(userPublishedItemVoteDetails);
				numVoteQueries++;
			}
		}
	}

	public void GetItemCreators(List<ulong> fileIDs)
	{
		if (numItemCreatorQueries <= 0 && SteamManager.Initialized)
		{
			m_GetPublishedFileDetails.Clear();
			numItemCreatorQueries = 0;
			for (int i = 0; i < fileIDs.Count; i++)
			{
				SteamAPICall_t publishedFileDetails = SteamRemoteStorage.GetPublishedFileDetails(new PublishedFileId_t
				{
					m_PublishedFileId = fileIDs[i]
				}, 0u);
				m_GetPublishedFileDetails.Add(CallResult<RemoteStorageGetPublishedFileDetailsResult_t>.Create(OnGetPublishedFileDetails));
				m_GetPublishedFileDetails[m_GetPublishedFileDetails.Count - 1].Set(publishedFileDetails);
				numItemCreatorQueries++;
			}
		}
	}

	public void OnUpdatePublishedItemVote(RemoteStorageUpdateUserPublishedItemVoteResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_SETVOTE", "PERMA"));
		}
		else
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("ItemVoteOnObjectUpdated", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnGetPublishedFileDetails(RemoteStorageGetPublishedFileDetailsResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult != EResult.k_EResultOK && pCallback.m_eResult != EResult.k_EResultFileNotFound)
		{
			Debug.LogWarning("Error while trying to get item owner details. " + pCallback.m_eResult);
			Singleton<ActionHenk>.SP.levels.BroadcastMessage("SetItemCreator", pCallback, SendMessageOptions.DontRequireReceiver);
			numItemCreatorQueries--;
		}
		else if (numItemCreatorQueries > 0)
		{
			Singleton<ActionHenk>.SP.levels.BroadcastMessage("SetItemCreator", pCallback, SendMessageOptions.DontRequireReceiver);
			numItemCreatorQueries--;
		}
	}

	public void OnGetPublishedItemVoteDetails(RemoteStorageUserVoteDetails_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_GETVOTE", "PERMA"));
			numVoteQueries--;
		}
		else
		{
			if (numVoteQueries <= 0)
			{
				return;
			}
			for (int i = 0; i < Singleton<LevelBatchManager>.SP.GetWorkshopLevels().Count; i++)
			{
				if (pCallback.m_nPublishedFileId.m_PublishedFileId == Singleton<LevelBatchManager>.SP.GetPublishedFileIDFromGuid(Singleton<LevelBatchManager>.SP.GetWorkshopLevels()[i].guid))
				{
					Singleton<LevelBatchManager>.SP.GetLevelFromGuid(Singleton<LevelBatchManager>.SP.GetWorkshopLevels()[i].guid).workshopVote = pCallback.m_eVote;
				}
			}
			Singleton<GetRoot>.SP.Get().BroadcastMessage("SetItemVoteOnObject", pCallback, SendMessageOptions.DontRequireReceiver);
			numVoteQueries--;
		}
	}

	public void SubscribeToList(ulong[] levelIDs)
	{
		MonoBehaviour.print("subscribing to " + levelIDs.Length + " levels");
		levelIDsToDownload = levelIDs;
		ulong[] array = levelIDsToDownload;
		foreach (ulong levelId in array)
		{
			SubscribeTo(levelId);
		}
	}

	public int GetTotalDownloads()
	{
		if (levelIDsToDownload == null)
		{
			return 0;
		}
		return levelIDsToDownload.Length;
	}

	public int GetDownloadsLeft()
	{
		int num = 0;
		int num2 = 0;
		ulong num3 = 0uL;
		ulong num4 = 0uL;
		int num5 = 0;
		int num6 = 0;
		ulong[] array = levelIDsToDownload;
		foreach (ulong itemID in array)
		{
			if (GetUpdateInfo(itemID, out var isDownloading, out var bytesDownloaded, out var bytesTotal))
			{
				if (isDownloading)
				{
					num2++;
				}
				num3 += bytesTotal;
				num4 += bytesDownloaded;
			}
			else
			{
				num++;
			}
			if (GetItemInstallInfo(itemID, out var _))
			{
				num5++;
			}
			else
			{
				num6++;
			}
		}
		return num6;
	}

	public ulong[] GetAllWorkshopItems()
	{
		if (!SteamManager.Initialized)
		{
			return new ulong[0];
		}
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		ulong[] array2 = new ulong[numSubscribedItems];
		for (int i = 0; i < numSubscribedItems; i++)
		{
			array2[i] = array[i].m_PublishedFileId;
		}
		return array2;
	}

	public bool GetItemInstallInfo(ulong itemID, out string folder)
	{
		if (!SteamManager.Initialized)
		{
			folder = string.Empty;
			return false;
		}
		ulong punSizeOnDisk;
		bool pbLegacyItem;
		return SteamUGC.GetItemInstallInfo((PublishedFileId_t)itemID, out punSizeOnDisk, out folder, 1024u, out pbLegacyItem);
	}

	public bool GetUpdateInfo(ulong itemID, out bool isDownloading, out ulong bytesDownloaded, out ulong bytesTotal)
	{
		if (!SteamManager.Initialized)
		{
			isDownloading = false;
			bytesDownloaded = 0uL;
			bytesTotal = 0uL;
			return false;
		}
		bool pbNeedsUpdate;
		return SteamUGC.GetItemUpdateInfo((PublishedFileId_t)itemID, out pbNeedsUpdate, out isDownloading, out bytesDownloaded, out bytesTotal);
	}
}
