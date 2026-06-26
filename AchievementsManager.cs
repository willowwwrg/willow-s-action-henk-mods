using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsManager : Singleton<AchievementsManager>
{
	public void Start()
	{
		if (Singleton<PlayerPrefsManager>.SP.GetInt("ragequit", 0) == 1)
		{
			UnlockAchievement(Achievement.RageQuit);
		}
	}

	public void FinishedLevel(Level level)
	{
		if (level == null)
		{
			return;
		}
		if (level.levelType == LevelType.Challenge && level.bestMedal != Medal.None)
		{
			if (level.challenger == CharacterSelect.Characters.Betsy && level.challengerSkinNum == 0)
			{
				UnlockAchievement(Achievement.BetsyChallenge);
			}
			else if (level.challenger == CharacterSelect.Characters.Afronaut && level.challengerSkinNum == 0)
			{
				UnlockAchievement(Achievement.NeilChallenge);
			}
			else if (level.challenger == CharacterSelect.Characters.Afronaut && level.challengerSkinNum == 0)
			{
				UnlockAchievement(Achievement.NeilChallenge);
			}
			else if (level.challenger == CharacterSelect.Characters.Kentony && level.challengerSkinNum == 0)
			{
				UnlockAchievement(Achievement.KentonyChallenge1);
			}
			else if (level.challenger == CharacterSelect.Characters.Cedar && level.challengerSkinNum == 0)
			{
				UnlockAchievement(Achievement.CedarChallenge);
			}
			else if (level.challenger == CharacterSelect.Characters.Kentony && level.challengerSkinNum == 6)
			{
				UnlockAchievement(Achievement.KentonyChallenge2);
			}
			else if (level.challenger == CharacterSelect.Characters.Henk && level.challengerSkinNum == 5)
			{
				UnlockAchievement(Achievement.Redemption);
			}
		}
		List<Level> campaignLevels = Singleton<LevelBatchManager>.SP.GetCampaignLevels();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		foreach (Level item in campaignLevels)
		{
			Medal bestMedal = item.bestMedal;
			if (item.levelType == LevelType.Bonus)
			{
				num++;
				if (bestMedal == Medal.Bronze)
				{
					num2++;
				}
			}
			if (item.levelType == LevelType.Standard)
			{
				num3++;
				if (bestMedal >= Medal.Bronze)
				{
					num4++;
				}
				if (bestMedal >= Medal.Silver)
				{
					num5++;
				}
				if (bestMedal >= Medal.Gold)
				{
					num6++;
				}
				if (bestMedal >= Medal.Rainbow)
				{
					num7++;
				}
			}
		}
		MonoBehaviour.print($"Bonus levels beaten: {num2}/{num}. Standard levels: {num3}. Bronze/Silver/Gold/Rainbow: {num4}/{num5}/{num6}/{num7}");
		if (num2 == num)
		{
			UnlockAchievement(Achievement.AllBonus);
		}
		if (num4 == num3)
		{
			UnlockAchievement(Achievement.AllBronze);
		}
		if (num5 == num3)
		{
			UnlockAchievement(Achievement.AllSilver);
		}
		if (num6 == num3)
		{
			UnlockAchievement(Achievement.AllGold);
		}
		if (num7 == num3)
		{
			UnlockAchievement(Achievement.AllRainbow);
		}
	}

	public void CheckStatBasedAchievements()
	{
		for (int i = 18; i <= 22; i++)
		{
			Achievement achievement = (Achievement)i;
			if (Singleton<HenkSWUserStats>.SP.IsAchievementUnlocked(Singleton<HenkSWUserStats>.SP.GetAchievementName(achievement)))
			{
				UnlockAchievementSkin(achievement);
			}
		}
	}

	public void UnlockAchievement(Achievement achievement)
	{
		UnlockAchievementSkin(achievement);
		Singleton<HenkSWUserStats>.SP.UnlockAchievement(Singleton<HenkSWUserStats>.SP.GetAchievementName(achievement));
	}

	private void UnlockAchievementSkin(Achievement achievement)
	{
		switch (achievement)
		{
		case Achievement.AllBronze:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Betsy, 9);
			break;
		case Achievement.AllSilver:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Afronaut, 9);
			break;
		case Achievement.AllGold:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 9);
			break;
		case Achievement.AllRainbow:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Henk, 8);
			break;
		case Achievement.ChallengeFriend:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 5);
			break;
		case Achievement.RageQuit:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Betsy, 1);
			break;
		case Achievement.OnePercent:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 3);
			break;
		case Achievement.Barrier:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Cedar, 2);
			break;
		case Achievement.WinMultiplayer:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Cedar, 6);
			break;
		case Achievement.ResetXTimes:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Cedar, 3);
			break;
		case Achievement.DieXTimes:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 4);
			break;
		case Achievement.Airtime:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Kentony, 1);
			break;
		case Achievement.Marathon:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Cedar, 1);
			break;
		case Achievement.Buttslide:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Henk, 11);
			break;
		case Achievement.Henkvsgame:
			Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Henk, 13);
			break;
		case Achievement.FloorIsLava:
			break;
		}
	}

	public void OnApplicationQuit()
	{
		if (HenkUtils.IsInALevel() && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGame)) && Singleton<PlayerPrefsManager>.SP.GetInt("ragequit", 0) == 0)
		{
			Singleton<PlayerPrefsManager>.SP.SetInt("ragequit", 1);
			PlayerPrefs.Save();
			Application.CancelQuit();
			StartCoroutine(RageQuit());
		}
	}

	private IEnumerator RageQuit()
	{
		UnlockAchievement(Achievement.RageQuit);
		yield return new WaitForSeconds(1.5f);
		Application.Quit();
	}
}
