using System;
using System.IO;
using Steamworks;
using UnityEngine;

public class Level : MonoBehaviour
{
	public Difficulty difficulty;

	public Medal bestMedal;

	public WIPState wipState;

	public bool isNewLevel;

	public bool isSceneLess;

	public float bronzeTime;

	public float silverTime;

	public float goldTime;

	public float rainbowTime;

	public float bonusTime;

	[NonSerialized]
	public int steamScore = -1;

	public string levelCreator = string.Empty;

	public ulong uLevelCreator;

	public bool isEditorLevel;

	public SteamLeaderboard_t leaderboardHandle;

	public int levelVersion;

	public string levelName;

	public int levelCode;

	public string guid;

	public LevelType levelType;

	public CharacterSelect.Characters challenger = CharacterSelect.Characters.Henk;

	public int challengerSkinNum;

	public bool dontUnlockSkinOnChallengeComplete;

	public LevelStyle levelStyle = LevelStyle.KidsRoom_Day;

	public string workshopFolderName = string.Empty;

	public string workshopLevelDescription = string.Empty;

	public EWorkshopVote workshopVote;

	protected Callback<PersonaStateChange_t> m_PersonaStateChange;

	private bool waitingForUserName;

	public string preLevelCutscene = string.Empty;

	public string postLevelCutscene = string.Empty;

	public ulong publishedFiledID;

	public int newMedals;

	private void Start()
	{
		InitLevel();
	}

	public void InitLevel()
	{
		m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonalDetailsFetched);
		bestMedal = (Medal)Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(this);
		bronzeTime = HenkUtils.GetHighscoreFromFile(levelCode, "Bronze");
		silverTime = HenkUtils.GetHighscoreFromFile(levelCode, "Silver");
		goldTime = HenkUtils.GetHighscoreFromFile(levelCode, "Gold");
		rainbowTime = HenkUtils.GetHighscoreFromFile(levelCode, "Rainbow");
		if (levelType == LevelType.Challenge)
		{
			bronzeTime = HenkUtils.GetHighscoreFromFile(levelCode, "Challenge");
		}
	}

	public void SetLeaderboardHandle(SteamLeaderboard_t handle)
	{
		leaderboardHandle = handle;
	}

	public void SetPublishedFileID(string line)
	{
		string value = line.Substring(4);
		publishedFiledID = Convert.ToUInt64(value);
	}

	public string GetBestMedalString()
	{
		return bestMedal switch
		{
			Medal.Bronze => "bronze", 
			Medal.Silver => "silver", 
			Medal.Gold => "gold", 
			Medal.Rainbow => "rainbow", 
			_ => "None", 
		};
	}

	public void ChallengeCompleted()
	{
		if (bestMedal == Medal.None)
		{
			newMedals = 1;
		}
		Singleton<PlayerPrefsManager>.SP.SetMedalsEarned(this, 1);
		bestMedal = Medal.Bronze;
		if (!dontUnlockSkinOnChallengeComplete)
		{
			Singleton<UnlockManager>.SP.UnlockCharacter(challenger, challengerSkinNum);
		}
		if (challenger == CharacterSelect.Characters.Betsy && challengerSkinNum == 0)
		{
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.BetsyChallenge);
		}
		Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
	}

	public bool IsChallengeCompleted()
	{
		return Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(this) == 1;
	}

	public void BonusCompleted()
	{
		ChallengeCompleted();
	}

	public int ScoreMedal(Medal medal)
	{
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.MedalScored, medal.ToString());
		int result = 0;
		if (medal > bestMedal)
		{
			result = medal - bestMedal;
			bestMedal = medal;
			Singleton<PlayerPrefsManager>.SP.SetMedalsEarned(this, (int)bestMedal);
			Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
			Singleton<PermaGUI>.SP.ShowMedalCount();
			return result;
		}
		return result;
	}

	public string GetChallengerName()
	{
		return Singleton<UnlockManager>.SP.GetCharacterName(challenger, challengerSkinNum);
	}

	public void SetItemCreator(RemoteStorageGetPublishedFileDetailsResult_t pCallback)
	{
		if (pCallback.m_eResult != EResult.k_EResultFileNotFound && publishedFiledID == pCallback.m_nPublishedFileId.m_PublishedFileId)
		{
			uLevelCreator = pCallback.m_ulSteamIDOwner;
			CSteamID cSteamID = new CSteamID(uLevelCreator);
			if (SteamFriends.RequestUserInformation(cSteamID, bRequireNameOnly: true))
			{
				waitingForUserName = true;
			}
			else
			{
				levelCreator = SteamFriends.GetFriendPersonaName(cSteamID);
			}
		}
	}

	private void OnPersonalDetailsFetched(PersonaStateChange_t pCallback)
	{
		if (waitingForUserName && pCallback.m_ulSteamID == uLevelCreator)
		{
			levelCreator = SteamFriends.GetFriendPersonaName(new CSteamID(pCallback.m_ulSteamID));
			waitingForUserName = false;
		}
	}

	public ulong GetWorkshopID()
	{
		if (levelType == LevelType.Workshop)
		{
			return Convert.ToUInt64(new DirectoryInfo(workshopFolderName).Name);
		}
		return 0uL;
	}

	public ulong GetLevelID()
	{
		if (levelType == LevelType.Workshop)
		{
			return GetWorkshopID();
		}
		return (ulong)levelCode;
	}

	public float MedalTime(Medal medal)
	{
		return medal switch
		{
			Medal.Bronze => bronzeTime, 
			Medal.Silver => silverTime, 
			Medal.Gold => goldTime, 
			Medal.Rainbow => rainbowTime, 
			_ => 0f, 
		};
	}
}
