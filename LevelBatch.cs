using System.Collections.Generic;
using UnityEngine;

public class LevelBatch : MonoBehaviour
{
	public List<Level> levels = new List<Level>();

	public string batchName = string.Empty;

	public bool batchUnlocked;

	public bool challengeLevelsUnlocked;

	public bool bonusLevelUnlocked;

	public bool playUnlockAnim;

	public int numMedalsToUnlock;

	public Level unlockDependancy;

	public IngameMusic audioTheme;

	private void Awake()
	{
	}

	public bool IsUnlocked()
	{
		if (ActionHenk.UNLOCKALL)
		{
			return true;
		}
		if (this == Singleton<LevelBatchManager>.SP.batches[0])
		{
			return true;
		}
		return batchUnlocked;
	}

	public bool CheckChallengeLevelUnlocked()
	{
		if (ActionHenk.UNLOCKALL)
		{
			challengeLevelsUnlocked = true;
			return true;
		}
		if (challengeLevelsUnlocked)
		{
			return true;
		}
		foreach (Level standardLevel in GetStandardLevels())
		{
			if (Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(standardLevel) < 1)
			{
				challengeLevelsUnlocked = false;
				return false;
			}
		}
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)) && GetChallengeLevel() != null)
		{
			Singleton<RewardManager>.SP.PopUpReward(RewardManager.UnlockType.ChallengeLevel, GetChallengeLevel());
		}
		challengeLevelsUnlocked = true;
		return true;
	}

	public bool CheckBonusLevelUnlocked()
	{
		if (ActionHenk.UNLOCKALL)
		{
			bonusLevelUnlocked = true;
			return true;
		}
		if (bonusLevelUnlocked)
		{
			return true;
		}
		foreach (Level standardLevel in GetStandardLevels())
		{
			if (Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(standardLevel) < 3)
			{
				bonusLevelUnlocked = false;
				return false;
			}
		}
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)) && GetBonusLevel() != null)
		{
			Singleton<RewardManager>.SP.PopUpReward(RewardManager.UnlockType.BonusLevel, GetBonusLevel());
		}
		bonusLevelUnlocked = true;
		return true;
	}

	public Level GetChallengeLevel()
	{
		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelType == LevelType.Challenge)
			{
				return levels[i];
			}
		}
		return null;
	}

	public Level GetBonusLevel()
	{
		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelType == LevelType.Bonus)
			{
				return levels[i];
			}
		}
		return null;
	}

	public void UnlockBatch()
	{
		if (!IsUnlocked())
		{
			batchUnlocked = true;
			if (this != Singleton<LevelBatchManager>.SP.batches[0] && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)))
			{
				Singleton<RewardManager>.SP.PopUpReward(RewardManager.UnlockType.LevelBundle, this);
				playUnlockAnim = true;
			}
		}
	}

	public List<Level> GetLevels()
	{
		return levels;
	}

	public List<Level> GetStandardLevels()
	{
		List<Level> list = new List<Level>();
		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelType == LevelType.Standard)
			{
				list.Add(levels[i]);
			}
		}
		return list;
	}

	public List<Level> GetChallengeLevels()
	{
		List<Level> list = new List<Level>();
		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i].levelType == LevelType.Challenge)
			{
				list.Add(levels[i]);
			}
		}
		return list;
	}

	public bool Contains(Level levelObj)
	{
		for (int i = 0; i < levels.Count; i++)
		{
			if (levels[i] == levelObj)
			{
				return true;
			}
		}
		return false;
	}
}
