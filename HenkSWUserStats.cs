using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Steamworks;
using UnityEngine;

[RequireComponent(typeof(SteamManager))]
public class HenkSWUserStats : Singleton<HenkSWUserStats>
{
	private bool initialized;

	public int waitForNameCallbacks;

	private bool submittingMedalFile;

	public string achievementStatus;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected CallResult<GlobalStatsReceived_t> m_GlobalStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected CallResult<RemoteStorageFileShareResult_t> m_RemoteStorageFileShareResult;

	protected Callback<PersonaStateChange_t> m_PersonaStateChange;

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	public bool Initialize()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		m_GameID = new CGameID(SteamUtils.GetAppID());
		m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_GlobalStatsReceived = CallResult<GlobalStatsReceived_t>.Create(OnGlobalStatsReceived);
		m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		m_RemoteStorageFileShareResult = CallResult<RemoteStorageFileShareResult_t>.Create(OnCloudFileShareResult);
		m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
		m_bRequestedStats = false;
		m_bStatsValid = false;
		initialized = true;
		ReadCloudSave();
		CheckUnfairUnlocks();
		return true;
	}

	private void Update()
	{
		_ = SteamManager.Initialized;
	}

	private void OnGlobalStatsReceived(GlobalStatsReceived_t pCallback, bool bIOFailure)
	{
		if (SteamManager.Initialized && (ulong)m_GameID == pCallback.m_nGameID && pCallback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError("Failed to download global stats: " + pCallback.m_eResult);
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (SteamManager.Initialized && (ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				m_bStatsValid = true;
				base.transform.root.BroadcastMessage("HenkSWUserStatsReceived", true, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.transform.root.BroadcastMessage("HenkSWUserStatsReceived", false);
				Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Singleton<AchievementsManager>.SP.CheckStatBasedAchievements();
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.Log("StoreStats - some failed to validate");
				OnUserStatsReceived(new UserStatsReceived_t
				{
					m_eResult = EResult.k_EResultOK,
					m_nGameID = (ulong)m_GameID
				});
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
		RefreshUserStats();
	}

	public bool IncrementStat(string statHandle, bool increment = true)
	{
		if (!initialized)
		{
			Debug.LogError("Steamworks not initialized, failed to increment stat.");
			return false;
		}
		int pData = -1;
		if (!SteamUserStats.GetStat(statHandle, out pData))
		{
			Debug.LogError("Failed to read " + statHandle);
			return false;
		}
		pData = ((!increment) ? (pData - 1) : (pData + 1));
		if (!SteamUserStats.SetStat(statHandle, pData))
		{
			Debug.LogError("Failed to write " + statHandle + " to Steam statistics.");
			return false;
		}
		return true;
	}

	public bool IncrementStatBy_f(string statHandle, float amount, bool increment = true)
	{
		if (!initialized)
		{
			Debug.LogError("Steamworks not initialized, failed to increment stat.");
			return false;
		}
		float pData = 0f;
		if (!SteamUserStats.GetStat(statHandle, out pData))
		{
			Debug.LogError("Failed to read " + statHandle);
			return false;
		}
		pData = ((!increment) ? (pData - amount) : (pData + amount));
		if (!SteamUserStats.SetStat(statHandle, pData))
		{
			Debug.LogError("Failed to write " + statHandle + " to Steam statistics.");
			return false;
		}
		return true;
	}

	public bool IncrementStatBy_i(string statHandle, int amount, bool increment = true)
	{
		if (!initialized)
		{
			Debug.LogError("Steamworks not initialized, failed to increment stat.");
			return false;
		}
		if (!SteamUserStats.GetStat(statHandle, out int pData))
		{
			Debug.LogError("Failed to read " + statHandle);
			return false;
		}
		pData = ((!increment) ? (pData - amount) : (pData + amount));
		if (!SteamUserStats.SetStat(statHandle, pData))
		{
			Debug.LogError("Failed to write " + statHandle + " to Steam statistics.");
			return false;
		}
		return true;
	}

	public void ResetAllStatsAndAchievements()
	{
		SteamUserStats.ResetAllStats(bAchievementsToo: true);
	}

	public void StoreStats()
	{
		SteamUserStats.StoreStats();
	}

	public int GetStat_i(string statHandle)
	{
		int pData = -1;
		if (!SteamUserStats.GetStat(statHandle, out pData))
		{
			Debug.LogWarning("Failed to read " + statHandle + ".");
			pData = -1;
		}
		return pData;
	}

	public float GetStat_f(string statHandle)
	{
		float pData = 0f;
		if (!SteamUserStats.GetStat(statHandle, out pData))
		{
			Debug.LogWarning("Failed to read " + statHandle + ".");
			pData = 0f;
		}
		return pData;
	}

	public long GetGlobalStat(string statHandle)
	{
		long pData = -1L;
		if (!SteamUserStats.GetGlobalStat(statHandle, out pData))
		{
			Debug.LogWarning("Failed to read " + statHandle + ".");
			pData = -1L;
		}
		return pData;
	}

	public void RefreshUserStats()
	{
		SteamUserStats.RequestCurrentStats();
		m_GlobalStatsReceived.Set(SteamUserStats.RequestGlobalStats(0));
	}

	public CSteamID GetSteamID()
	{
		if (!SteamManager.Initialized)
		{
			return default(CSteamID);
		}
		return SteamUser.GetSteamID();
	}

	public bool RequestNameBySteamID(CSteamID id)
	{
		if (SteamFriends.RequestUserInformation(id, bRequireNameOnly: true))
		{
			waitForNameCallbacks++;
			return true;
		}
		return false;
	}

	public string GetNameBySteamID(CSteamID id)
	{
		if (!SteamManager.Initialized)
		{
			return "Player";
		}
		return SteamFriends.GetFriendPersonaName(id);
	}

	public string GetNameBySteamID(ulong id)
	{
		return SteamFriends.GetFriendPersonaName(new CSteamID
		{
			m_SteamID = id
		});
	}

	private void OnPersonaStateChanged(PersonaStateChange_t pCallback)
	{
		waitForNameCallbacks--;
	}

	public void CheckUnfairUnlocks()
	{
		if (!HenkUtils.IsDev(Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID) && Singleton<PlayerPrefsManager>.SP.GetInt("UnlockAll", 0) == 1)
		{
			Debug.LogWarning("Clearing unfair unlocks!");
			if (Singleton<PlayerPrefsManager>.SP.GetInt("EA", 0) == 0)
			{
				Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Henk, 3);
			}
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Henk, 4);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Henk, 6);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Henk, 7);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Henk, 11);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Betsy, 5);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Afronaut, 5);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Afronaut, 6);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Kentony, 2);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Kentony, 1);
			Singleton<UnlockManager>.SP.LockCharacter(CharacterSelect.Characters.Cedar, 1);
			Singleton<PlayerPrefsManager>.SP.SetInt("UnlockAll", 0);
			ActionHenk.UNLOCKALL = false;
			WriteCloudSave(forceWrite: true);
		}
	}

	public void ReadCloudSave()
	{
		if (!initialized)
		{
			return;
		}
		string pchFile = "cloudsave.dat";
		int fileSize = SteamRemoteStorage.GetFileSize(pchFile);
		if (fileSize == 0)
		{
			Debug.LogWarning("Couldn't find cloud save file");
			return;
		}
		byte[] array = new byte[fileSize];
		SteamRemoteStorage.FileRead(pchFile, array, fileSize);
		string s = HenkSWLeaderboards.GetString(array);
		int num = 0;
		using (StringReader stringReader = new StringReader(s))
		{
			string empty = string.Empty;
			do
			{
				empty = stringReader.ReadLine();
				if (empty == null || !(empty != string.Empty))
				{
					continue;
				}
				switch (num)
				{
				case 0:
				{
					if (empty == "unlocks")
					{
						num = 1;
						break;
					}
					string[] array4 = empty.Split('_');
					Level levelFromCode2 = Singleton<LevelBatchManager>.SP.GetLevelFromCode(int.Parse(array4[0]));
					int num3 = int.Parse(array4[1]);
					if (num3 > Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(levelFromCode2))
					{
						Singleton<PlayerPrefsManager>.SP.SetMedalsEarned(levelFromCode2, num3);
					}
					break;
				}
				case 1:
				{
					if (empty == "highscores")
					{
						num = 2;
						break;
					}
					string[] array2 = empty.Split('_');
					CharacterSelect.Characters character = (CharacterSelect.Characters)int.Parse(array2[0]);
					int skinNum = int.Parse(array2[1]);
					Singleton<PlayerPrefsManager>.SP.SetCharacterUnlocked(character, skinNum, unlocked: true);
					break;
				}
				case 2:
				{
					if (empty == "misc")
					{
						num = 3;
						break;
					}
					string[] array3 = empty.Split('_');
					Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode(int.Parse(array3[0]));
					float num2 = HenkUtils.FloatParse(array3[1]);
					float highscore = Singleton<PlayerPrefsManager>.SP.GetHighscore(levelFromCode);
					if (highscore == 0f || num2 < highscore)
					{
						Singleton<PlayerPrefsManager>.SP.SetHighscore(levelFromCode, num2);
					}
					break;
				}
				case 3:
					if (empty == "EA")
					{
						Singleton<PlayerPrefsManager>.SP.SetInt("EA", 1);
					}
					break;
				}
			}
			while (empty != null && empty != string.Empty);
		}
		MonoBehaviour.print("Done reading cloud save");
	}

	public void WriteCloudSave(bool forceWrite = false)
	{
		if (!initialized)
		{
			return;
		}
		if (!forceWrite)
		{
			ReadCloudSave();
		}
		else
		{
			MonoBehaviour.print("Forced writing cloud save");
		}
		string text = string.Empty;
		List<Level> campaignLevels = Singleton<LevelBatchManager>.SP.GetCampaignLevels();
		foreach (Level item in campaignLevels)
		{
			int medalsEarned = Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(item);
			if (medalsEarned != 0)
			{
				string text2 = text;
				text = text2 + item.levelCode + "_" + medalsEarned + "\n";
			}
		}
		text += "unlocks\n";
		for (int i = 0; i <= 5; i++)
		{
			CharacterSelect.Characters character = (CharacterSelect.Characters)i;
			foreach (UnlockableCharacter unlockableCharacter in Singleton<UnlockManager>.SP.GetUnlockableCharacterList(character))
			{
				int skinNum = unlockableCharacter.skinNum;
				if (Singleton<PlayerPrefsManager>.SP.GetCharacterUnlocked(character, skinNum))
				{
					string text3 = text;
					text = text3 + i + "_" + skinNum + "\n";
				}
			}
		}
		text += "highscores\n";
		foreach (Level item2 in campaignLevels)
		{
			float highscore = Singleton<PlayerPrefsManager>.SP.GetHighscore(item2);
			if (highscore != 0f)
			{
				string text4 = text;
				text = text4 + item2.levelCode + "_" + highscore.ToString(CultureInfo.InvariantCulture) + "\n";
			}
		}
		text += "misc\n";
		if (Singleton<PlayerPrefsManager>.SP.GetInt("EA", 0) == 1)
		{
			text += "EA\n";
		}
		byte[] bytes = HenkSWLeaderboards.GetBytes(text);
		SteamRemoteStorage.FileWrite("cloudsave.dat", bytes, bytes.Length);
		submittingMedalFile = true;
		MonoBehaviour.print("Done writing cloud save");
	}

	private void OnCloudFileShareResult(RemoteStorageFileShareResult_t pCallback, bool bIOFailure)
	{
		if (SteamManager.Initialized)
		{
			if (submittingMedalFile)
			{
				Debug.Log("File share result: " + pCallback.m_eResult);
			}
			submittingMedalFile = false;
		}
	}

	public void UnlockAchievement(string achievementName)
	{
		if (SteamManager.Initialized && !(achievementName == string.Empty) && !IsAchievementUnlocked(achievementName))
		{
			SteamUserStats.SetAchievement(achievementName);
			SteamUserStats.StoreStats();
		}
	}

	public void LockAchievement(string achievementName)
	{
		if (SteamManager.Initialized && !(achievementName == string.Empty) && IsAchievementUnlocked(achievementName))
		{
			SteamUserStats.ClearAchievement(achievementName);
			SteamUserStats.StoreStats();
		}
	}

	public bool IsAchievementUnlocked(string achievementName)
	{
		if (!SteamManager.Initialized || achievementName == string.Empty)
		{
			return false;
		}
		bool pbAchieved = false;
		if (SteamUserStats.GetAchievement(achievementName, out pbAchieved))
		{
			return pbAchieved;
		}
		return false;
	}

	public void OpenSteamOverlayNotifcationSettings()
	{
		if (SteamManager.Initialized && !Application.isEditor)
		{
			SteamFriends.ActivateGameOverlayToWebPage(string.Concat("http://steamcommunity.com/profiles/", GetSteamID(), "/gamenotificationsettings/"));
		}
	}

	public string GetAchievementName(Achievement achievement)
	{
		string result = string.Empty;
		switch (achievement)
		{
		case Achievement.BetsyChallenge:
			result = "ACH_CHALLENGEBETSY";
			break;
		case Achievement.KentonyChallenge1:
			result = "ACH_CHALLENGEKENTONY1";
			break;
		case Achievement.UploadCustomLevel:
			result = "ACH_HENKTHEBUILDER";
			break;
		case Achievement.NeilChallenge:
			result = "NEILCHALLENGE";
			break;
		case Achievement.CedarChallenge:
			result = "CEDARCHALLENGE";
			break;
		case Achievement.KentonyChallenge2:
			result = "KENTONYCHALLENGE_2";
			break;
		case Achievement.Redemption:
			result = "REDEMPTION";
			break;
		case Achievement.AllBronze:
			result = "ALLBRONZE";
			break;
		case Achievement.AllSilver:
			result = "ALLSILVER";
			break;
		case Achievement.AllGold:
			result = "ALLGOLD";
			break;
		case Achievement.AllRainbow:
			result = "ALLRAINBOW";
			break;
		case Achievement.AllBonus:
			result = "ALLBONUS";
			break;
		case Achievement.FloorIsLava:
			result = "FLOORISLAVA";
			break;
		case Achievement.ChallengeFriend:
			result = "CHALLENGEFRIEND";
			break;
		case Achievement.RageQuit:
			result = "RAGEQUIT";
			break;
		case Achievement.OnePercent:
			result = "ONEPERCENT";
			break;
		case Achievement.Barrier:
			result = "BARRIER";
			break;
		case Achievement.Henkvsgame:
			result = "HENK_VS_GAME";
			break;
		case Achievement.WinMultiplayer:
			result = "WINMULTIPLAYER";
			break;
		case Achievement.ResetXTimes:
			result = "RESET_X_TIMES";
			break;
		case Achievement.DieXTimes:
			result = "DIE_X_TIMES";
			break;
		case Achievement.Airtime:
			result = "X_SECONDS_AIRTIME";
			break;
		case Achievement.Marathon:
			result = "MARATHON";
			break;
		case Achievement.Buttslide:
			result = "BUTTSLIDE";
			break;
		}
		return result;
	}

	public void CheckSkinUnlocks()
	{
		if (SteamManager.Initialized)
		{
			StartCoroutine(SkinUnlocksRoutine());
		}
	}

	private IEnumerator SkinUnlocksRoutine()
	{
		string text = Singleton<PhpMyAdminMan>.SP.SkinUnlocksURL();
		string text2 = "?steamid=" + GetSteamID().m_SteamID;
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error retrieving skin unlocks: " + webCall.error);
		}
		else
		{
			if (!(webCall.text != string.Empty))
			{
				yield break;
			}
			string[] array = webCall.text.Split(',');
			foreach (string text3 in array)
			{
				if (text3.StartsWith("a_"))
				{
					string achievementName = text3.Substring(2);
					UnlockAchievement(achievementName);
					continue;
				}
				string[] array2 = text3.Split('_');
				CharacterSelect.Characters character = (CharacterSelect.Characters)(int)Enum.Parse(typeof(CharacterSelect.Characters), array2[0], ignoreCase: true);
				int skinNum = int.Parse(array2[1]);
				Singleton<UnlockManager>.SP.UnlockCharacter(character, skinNum);
			}
		}
	}
}
