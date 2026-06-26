using UnityEngine;

public class PlayerPrefsManager : Singleton<PlayerPrefsManager>
{
	private ulong currentLoginID;

	public void Initialize()
	{
		if (SteamManager.Initialized)
		{
			currentLoginID = Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID;
			MonoBehaviour.print("Login ID is " + currentLoginID);
			if (!PlayerPrefs.HasKey("lastLoginID"))
			{
				Debug.Log("First time with login ID, copying all ID-less data");
				CopyAllDataToLogin();
			}
			PlayerPrefs.SetString("lastLoginID", currentLoginID.ToString());
		}
		else if (PlayerPrefs.HasKey("lastLoginID"))
		{
			ulong.TryParse(PlayerPrefs.GetString("lastLoginID"), out currentLoginID);
			MonoBehaviour.print("Fallback to lastlogin " + currentLoginID);
		}
	}

	private void CopyAllDataToLogin()
	{
		foreach (Level campaignLevel in Singleton<LevelBatchManager>.SP.GetCampaignLevels())
		{
			float highscore = GetHighscore(campaignLevel, useLoginID: false);
			int medalsEarned = GetMedalsEarned(campaignLevel, useLoginID: false);
			if (highscore != 0f)
			{
				SetHighscore(campaignLevel, highscore);
			}
			if (medalsEarned != 0)
			{
				SetMedalsEarned(campaignLevel, medalsEarned);
			}
		}
		for (int i = 0; i <= 5; i++)
		{
			CharacterSelect.Characters character = (CharacterSelect.Characters)i;
			foreach (UnlockableCharacter unlockableCharacter in Singleton<UnlockManager>.SP.GetUnlockableCharacterList(character))
			{
				int skinNum = unlockableCharacter.skinNum;
				if (GetCharacterUnlocked(character, skinNum, useLoginID: false))
				{
					SetCharacterUnlocked(character, skinNum, unlocked: true);
				}
			}
		}
	}

	public bool GetCharacterUnlocked(CharacterSelect.Characters character, int skinNum, bool useLoginID = true)
	{
		string text = string.Concat("unlocked", character, "_", skinNum);
		if (currentLoginID != 0 && useLoginID)
		{
			text = text + "_" + currentLoginID;
		}
		return PlayerPrefs.GetInt(text, 0) == 1;
	}

	public void SetCharacterUnlocked(CharacterSelect.Characters character, int skinNum, bool unlocked)
	{
		string text = string.Concat("unlocked", character, "_", skinNum);
		if (currentLoginID != 0L)
		{
			text = text + "_" + currentLoginID;
		}
		if (unlocked)
		{
			PlayerPrefs.SetInt(text, 1);
		}
		else if (PlayerPrefs.HasKey(text))
		{
			PlayerPrefs.DeleteKey(text);
		}
	}

	public int GetMedalsEarned(Level level, bool useLoginID = true)
	{
		string text = "Medal_Level_" + level.levelCode;
		if (level.levelType == LevelType.Challenge)
		{
			text = "Medal_Level_Challenge_" + level.levelCode;
		}
		if (level.levelType == LevelType.Bonus)
		{
			text = "Medal_Level_Bonus_" + level.levelCode;
		}
		if (currentLoginID != 0 && useLoginID)
		{
			text = text + "_" + currentLoginID;
		}
		return PlayerPrefs.GetInt(text, 0);
	}

	public void SetMedalsEarned(Level level, int medals)
	{
		string text = "Medal_Level_" + level.levelCode;
		if (level.levelType == LevelType.Challenge)
		{
			text = "Medal_Level_Challenge_" + level.levelCode;
		}
		if (level.levelType == LevelType.Bonus)
		{
			text = "Medal_Level_Bonus_" + level.levelCode;
		}
		if (currentLoginID != 0L)
		{
			text = text + "_" + currentLoginID;
		}
		if (medals != 0)
		{
			PlayerPrefs.SetInt(text, medals);
		}
		else if (PlayerPrefs.HasKey(text))
		{
			PlayerPrefs.DeleteKey(text);
		}
	}

	public float GetHighscore(Level level, bool useLoginID = true)
	{
		string text = "HIGHSCORE-" + level.levelCode;
		if (level.levelType == LevelType.Workshop)
		{
			text = "HIGHSCORE-" + level.guid;
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			text = "HIGHSCORE-DAILY" + Singleton<MutatorManager>.SP.seedOfToday;
		}
		if (currentLoginID != 0 && useLoginID)
		{
			text = text + "_" + currentLoginID;
		}
		return PlayerPrefs.GetFloat(text, 0f);
	}

	public void SetHighscore(Level level, float highscore)
	{
		string text = "HIGHSCORE-" + level.levelCode;
		if (level.levelType == LevelType.Workshop)
		{
			text = "HIGHSCORE-" + level.guid;
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			text = "HIGHSCORE-DAILY" + Singleton<MutatorManager>.SP.seedOfToday;
		}
		if (currentLoginID != 0L)
		{
			text = text + "_" + currentLoginID;
		}
		PlayerPrefs.SetFloat(text, highscore);
	}

	public bool HasKey(string key)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		return PlayerPrefs.HasKey(key);
	}

	public void SetString(string key, string value)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		PlayerPrefs.SetString(key, value);
	}

	public string GetString(string key, string defaultValue)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public void SetInt(string key, int value)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		PlayerPrefs.SetInt(key, value);
	}

	public int GetInt(string key, int defaultValue)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public void SetFloat(string key, float value)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		PlayerPrefs.SetFloat(key, value);
	}

	public float GetFloat(string key, float defaultValue)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public void SetLong(string key, long value)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		PlayerPrefs.SetString(key, value.ToString());
	}

	public long GetLong(string key, long defaultValue)
	{
		if (currentLoginID != 0L)
		{
			key = key + "_" + currentLoginID;
		}
		if (long.TryParse(PlayerPrefs.GetString(key, defaultValue.ToString()), out var result))
		{
			return result;
		}
		return defaultValue;
	}
}
