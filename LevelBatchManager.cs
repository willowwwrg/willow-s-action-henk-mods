using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelBatchManager : Singleton<LevelBatchManager>
{
	public List<LevelBatch> batches = new List<LevelBatch>();

	private List<Level> playableLevels = new List<Level>();

	private List<Level> allLevels = new List<Level>();

	private List<Level> campaignLevels = new List<Level>();

	private List<Level> standardLevels = new List<Level>();

	public List<FileInfo> allCustomLevelFiles = new List<FileInfo>();

	private int currentSceneNumber;

	private int numMedals;

	public Level defaultLevel;

	public bool showingCutscene;

	public Level currentLevel;

	public bool refreshingWorkshopLevels;

	public int lookingAtBatchNum;

	public int workshopLevelDetailQueryCount;

	public bool workshopLevelsUpToDate;

	public List<Level> levelsCompletedThisSession = new List<Level>();

	private void OnLevelWasLoaded()
	{
	}

	private void Awake()
	{
		if (batches.Count == 0)
		{
			return;
		}
		if (!batches[0].IsUnlocked())
		{
			batches[0].UnlockBatch();
		}
		allLevels.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (base.transform.GetChild(i).name != "BATCHES" && base.transform.GetChild(i).GetComponent<Level>().wipState != WIPState.None)
			{
				allLevels.Add(base.transform.GetChild(i).GetComponent<Level>());
			}
		}
	}

	private void Start()
	{
		BatchUnlockCheck();
	}

	public void AddCurrentLevelToSessionLevels()
	{
		if (currentLevel != null && currentLevel.levelStyle != LevelStyle.City && currentLevel.levelStyle != LevelStyle.Credits_Space && (currentLevel.levelType == LevelType.Standard || currentLevel.levelType == LevelType.Challenge) && !levelsCompletedThisSession.Contains(currentLevel))
		{
			levelsCompletedThisSession.Add(currentLevel);
		}
		if (levelsCompletedThisSession.Count > 53)
		{
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.Henkvsgame);
		}
		if (HenkUtils.IsHalloween())
		{
			if (levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(43)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(44)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(45)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(46)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(47)))
			{
				Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Henk, 4);
			}
			if (levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(79)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(21)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(19)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(80)) && levelsCompletedThisSession.Contains(Singleton<LevelBatchManager>.SP.GetLevelFromCode(31)))
			{
				Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Betsy, 5);
			}
		}
	}

	public Level GetLevelFromGuid(string guid)
	{
		foreach (Level allLevel in allLevels)
		{
			if (guid == allLevel.guid)
			{
				return allLevel;
			}
		}
		return null;
	}

	public void RemoveLevel(Level levelObj)
	{
		if (allLevels.Contains(levelObj))
		{
			allLevels.Remove(levelObj);
		}
		if (playableLevels.Contains(levelObj))
		{
			playableLevels.Remove(levelObj);
		}
		Object.Destroy(levelObj.gameObject);
	}

	public ulong GetPublishedFileIDFromGuid(string guid)
	{
		if (guid == string.Empty)
		{
			return 0uL;
		}
		foreach (Level workshopLevel in GetWorkshopLevels())
		{
			if (workshopLevel.guid == guid)
			{
				return workshopLevel.publishedFiledID;
			}
		}
		return 0uL;
	}

	public string GetLevelNameFromID(ulong ID)
	{
		string result = string.Empty;
		for (int i = 0; i < GetAllLevels().Count; i++)
		{
			if (ID == GetAllLevels()[i].GetLevelID())
			{
				result = GetAllLevels()[i].levelName;
			}
		}
		return result;
	}

	public string GetLevelCreator(string guid)
	{
		Level levelFromGuid = GetLevelFromGuid(guid);
		if (levelFromGuid != null)
		{
			if (levelFromGuid.levelCreator == string.Empty)
			{
				return "unknown";
			}
			return levelFromGuid.levelCreator;
		}
		return "unknown";
	}

	public List<Level> GetWorkshopLevels()
	{
		List<Level> list = new List<Level>();
		for (int i = 0; i < allLevels.Count; i++)
		{
			if (allLevels[i].levelType == LevelType.Workshop)
			{
				list.Add(allLevels[i]);
			}
		}
		return list;
	}

	public List<Level> GetAllLevels()
	{
		return allLevels;
	}

	public void ResetUnlocks()
	{
		foreach (LevelBatch batch in batches)
		{
			batch.batchUnlocked = false;
			batch.challengeLevelsUnlocked = false;
			batch.bonusLevelUnlocked = false;
		}
		batches[0].UnlockBatch();
	}

	private void Update()
	{
	}

	public int MedalCount()
	{
		numMedals = 0;
		foreach (Level campaignLevel in GetCampaignLevels())
		{
			if (campaignLevel.levelType == LevelType.Standard)
			{
				numMedals += Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(campaignLevel);
			}
		}
		return numMedals;
	}

	public int RainbowMedalCount()
	{
		numMedals = 0;
		foreach (Level campaignLevel in GetCampaignLevels())
		{
			if (campaignLevel.levelType == LevelType.Standard && Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(campaignLevel) == 4)
			{
				numMedals++;
			}
		}
		return numMedals;
	}

	public void RefreshLevels()
	{
		campaignLevels.Clear();
		playableLevels.Clear();
		standardLevels.Clear();
		for (int i = 0; i < batches.Count; i++)
		{
			LevelBatch levelBatch = batches[i];
			if (levelBatch.IsUnlocked())
			{
				playableLevels.AddRange(levelBatch.GetStandardLevels());
			}
			if (levelBatch.CheckChallengeLevelUnlocked())
			{
				playableLevels.AddRange(levelBatch.GetChallengeLevels());
			}
			if (levelBatch.CheckBonusLevelUnlocked())
			{
				playableLevels.Add(levelBatch.GetBonusLevel());
			}
			if (i < 10)
			{
				standardLevels.AddRange(levelBatch.GetStandardLevels());
			}
			campaignLevels.AddRange(levelBatch.GetLevels());
		}
	}

	public void DoneRefreshingWorkshopLevels()
	{
		workshopLevelsUpToDate = true;
		refreshingWorkshopLevels = false;
		Singleton<GetRoot>.SP.Get().BroadcastMessage("RefreshingWorkshopLevelsCompleted", SendMessageOptions.DontRequireReceiver);
	}

	public void RefreshWorkshopLevels()
	{
		workshopLevelsUpToDate = false;
		StartCoroutine(WorkshopRefreshRoutine());
	}

	public void WorkshopLevelDetails(Level levelObj)
	{
		if (DoesLevelExist(levelObj.levelName, levelObj.guid))
		{
			Object.Destroy(levelObj.gameObject);
		}
		else
		{
			Singleton<LevelBatchManager>.SP.AddEditorLevelToAllLevels(levelObj);
		}
	}

	private IEnumerator WorkshopRefreshRoutine()
	{
		refreshingWorkshopLevels = true;
		Singleton<Workshop>.SP.GetSubscribedItemDetails();
		while (workshopLevelDetailQueryCount > 0)
		{
			yield return new WaitForEndOfFrame();
		}
		GetAllItemVotes();
		GetAllItemCreators();
		DoneRefreshingWorkshopLevels();
	}

	public void CheckChallengeLevelUnlock()
	{
		foreach (LevelBatch batch in batches)
		{
			for (int i = 0; i < batch.levels.Count; i++)
			{
				if (batch.levels[i].levelCode == GetCurrentLevel())
				{
					batch.CheckChallengeLevelUnlocked();
				}
			}
		}
	}

	public void CheckBonusLevelUnlock()
	{
		foreach (LevelBatch batch in batches)
		{
			for (int i = 0; i < batch.levels.Count; i++)
			{
				if (batch.levels[i].levelCode == GetCurrentLevel())
				{
					batch.CheckBonusLevelUnlocked();
				}
			}
		}
	}

	public string GetChallengerName()
	{
		if (GetCurrentLevelObj().levelType == LevelType.Challenge || GetCurrentLevelObj().levelType == LevelType.Bonus)
		{
			return GetCurrentLevelObj().GetChallengerName();
		}
		return "Challenger";
	}

	public Level GetCurrentLevelObj()
	{
		if (currentLevel != null)
		{
			return currentLevel;
		}
		foreach (Level allLevel in allLevels)
		{
			if (allLevel.levelCode == currentSceneNumber)
			{
				return allLevel;
			}
		}
		return null;
	}

	public int GetCurrentLevel()
	{
		if (currentLevel != null && currentLevel.isSceneLess)
		{
			return currentLevel.levelCode;
		}
		return currentSceneNumber;
	}

	public ulong GetCurrentLevelCodeOrID()
	{
		if (currentLevel != null && currentLevel.isSceneLess)
		{
			if (currentLevel.levelType == LevelType.Workshop)
			{
				return currentLevel.publishedFiledID;
			}
			return (ulong)currentLevel.levelCode;
		}
		return (ulong)currentSceneNumber;
	}

	public List<Level> GetPlayableLevels()
	{
		if (playableLevels.Count == 0)
		{
			RefreshLevels();
		}
		return playableLevels;
	}

	public List<Level> GetCampaignLevels(bool excludeChallengeBonusAndExtraLevels = false)
	{
		if (campaignLevels.Count == 0)
		{
			RefreshLevels();
		}
		if (excludeChallengeBonusAndExtraLevels)
		{
			return standardLevels;
		}
		return campaignLevels;
	}

	public int GetNumUnlockedBatches()
	{
		int num = 0;
		foreach (LevelBatch batch in batches)
		{
			if (batch.IsUnlocked())
			{
				num++;
			}
		}
		return num;
	}

	public LevelBatch GetBatchFromNum(int batchNum)
	{
		if (batchNum > batches.Count - 1 || batchNum < 0)
		{
			return null;
		}
		return batches[batchNum];
	}

	public int GetBatchNum(LevelBatch batch)
	{
		for (int i = 0; i < batches.Count; i++)
		{
			if (batch == batches[i])
			{
				return i;
			}
		}
		return 0;
	}

	public LevelBatch GetBatchFromLevel(Level levelObj)
	{
		for (int i = 0; i < batches.Count; i++)
		{
			for (int j = 0; j < batches[i].GetLevels().Count; j++)
			{
				if (batches[i].GetLevels()[j] == levelObj)
				{
					return batches[i];
				}
			}
		}
		return null;
	}

	public int GetBatchNumFromLevel(Level levelObj)
	{
		for (int i = 0; i < batches.Count; i++)
		{
			for (int j = 0; j < batches[i].GetLevels().Count; j++)
			{
				if (batches[i].GetLevels()[j].levelCode == levelObj.levelCode)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool DoesLevelExist(string name, string guid)
	{
		foreach (Level allLevel in allLevels)
		{
			if (allLevel.levelName == name && allLevel.guid == guid)
			{
				return true;
			}
		}
		return false;
	}

	public void AddEditorLevelToAllLevels(Level level)
	{
		allLevels.Add(level);
	}

	public void LoadLevelScenelessFromID(ulong level)
	{
		Level levelFromCodeOrID = GetLevelFromCodeOrID(level);
		if (levelFromCodeOrID != null)
		{
			LoadLevelSceneless(levelFromCodeOrID);
		}
		else
		{
			Debug.LogError("Couldn't load level with ID " + level);
		}
	}

	public void LoadLevelSceneless(Level level)
	{
		if (showingCutscene)
		{
			Debug.LogError("Tried to start another cutscene, cancelled.");
			return;
		}
		currentLevel = level;
		string preLevelCutscene = level.preLevelCutscene;
		bool flag = Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer;
		if (preLevelCutscene != string.Empty && flag)
		{
			showingCutscene = true;
			Object.FindObjectOfType<State_Cutscene>().LoadCutscene(preLevelCutscene, isPostGame: false);
		}
		else
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_BuildLevel));
		}
	}

	public void LoadLevel(int levelCode)
	{
		currentSceneNumber = levelCode;
		Singleton<GamestateManager>.SP.SetState(typeof(State_Loading));
	}

	public void LoadNextLevel()
	{
		if (IsCurrentLevelLastLevel())
		{
			MonoBehaviour.print("We're at the last available level");
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
		}
		else
		{
			LoadLevel(playableLevels[GetIteratorPositionOfCurrentSceneNumber() + 1].levelCode);
		}
	}

	public string GetLevelName(int levelCode)
	{
		if (levelCode == 0)
		{
			levelCode = currentSceneNumber;
		}
		foreach (Level allLevel in allLevels)
		{
			if (allLevel.levelCode == levelCode)
			{
				return allLevel.levelName;
			}
		}
		return "LevelNameNotFound";
	}

	public Level GetLevelFromCode(int levelCode)
	{
		foreach (Level allLevel in allLevels)
		{
			if (allLevel.levelCode == levelCode)
			{
				return allLevel;
			}
		}
		return null;
	}

	public Level GetLevelFromCodeOrID(ulong levelCode)
	{
		foreach (Level allLevel in allLevels)
		{
			if ((ulong)allLevel.levelCode == levelCode)
			{
				return allLevel;
			}
			if (allLevel.publishedFiledID == levelCode)
			{
				return allLevel;
			}
		}
		return null;
	}

	public Level GetLevelFromLevelNameAndGUID(string levelName, string guid)
	{
		foreach (Level allLevel in allLevels)
		{
			if (allLevel.levelName == levelName && allLevel.guid == guid)
			{
				return allLevel;
			}
		}
		return null;
	}

	public bool IsBatchUnlocked(int batchNum)
	{
		return batches[batchNum].batchUnlocked;
	}

	public void BatchUnlockCheck()
	{
		int num = MedalCount();
		for (int i = 0; i < batches.Count; i++)
		{
			if (num < batches[i].numMedalsToUnlock)
			{
				continue;
			}
			if (batches[i].unlockDependancy != null)
			{
				if (batches[i].unlockDependancy.bestMedal == Medal.Bronze)
				{
					batches[i].UnlockBatch();
				}
			}
			else
			{
				batches[i].UnlockBatch();
			}
		}
		RefreshLevels();
	}

	public void SetCurrentLevel(int overridelevel = 0)
	{
		if (overridelevel != 0)
		{
			currentSceneNumber = overridelevel;
		}
		else
		{
			OnLevelWasLoaded();
		}
	}

	public bool IsCurrentLevelLastLevel()
	{
		return playableLevels[playableLevels.Count - 1].levelCode == currentSceneNumber;
	}

	public string GetCurrentLevelNumAndName()
	{
		if (currentLevel == null)
		{
			return currentSceneNumber + "-" + GetLevelName(currentSceneNumber);
		}
		return currentLevel.levelCode + "-" + currentLevel.levelName;
	}

	public string GetGUID()
	{
		return currentLevel.guid;
	}

	private int GetIteratorPositionOfCurrentSceneNumber()
	{
		for (int i = 0; i < playableLevels.Count; i++)
		{
			if (playableLevels[i].levelCode == currentSceneNumber)
			{
				return i;
			}
		}
		Debug.LogError("Couldn't find iterator in playablelevels list.");
		return 0;
	}

	public void GetAllItemVotes()
	{
		List<Level> workshopLevels = GetWorkshopLevels();
		List<ulong> list = new List<ulong>();
		for (int i = 0; i < workshopLevels.Count; i++)
		{
			list.Add(workshopLevels[i].publishedFiledID);
		}
		Singleton<Workshop>.SP.GetItemVotes(list);
	}

	public void GetAllItemCreators()
	{
		List<Level> workshopLevels = GetWorkshopLevels();
		List<ulong> list = new List<ulong>();
		for (int i = 0; i < workshopLevels.Count; i++)
		{
			list.Add(workshopLevels[i].publishedFiledID);
		}
		Singleton<Workshop>.SP.GetItemCreators(list);
	}
}
