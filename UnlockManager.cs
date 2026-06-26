using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : Singleton<UnlockManager>
{
	public List<UnlockableCharacter> henk;

	public List<UnlockableCharacter> betsy;

	public List<UnlockableCharacter> neil;

	public List<UnlockableCharacter> cedar;

	public List<UnlockableCharacter> stacy;

	public List<UnlockableCharacter> kentony;

	private int prevMedals;

	private int prevRainbowMedals;

	public void Initialize()
	{
		InitUnlocksFromPlayerPrefs();
		UpdateMedals(silent: true);
	}

	private void InitUnlocksFromPlayerPrefs()
	{
		UnlockCharacter(CharacterSelect.Characters.Henk, 0, silent: true);
	}

	public void UpdateMedals(bool silent = false)
	{
		Singleton<LevelBatchManager>.SP.MedalCount();
	}

	public void PlayerPrefsWipe()
	{
		InitUnlocksFromPlayerPrefs();
	}

	public void UnlockEverything()
	{
		foreach (UnlockableCharacter item in henk)
		{
			UnlockCharacter(item.character, item.skinNum, silent: true);
		}
		foreach (UnlockableCharacter item2 in betsy)
		{
			UnlockCharacter(item2.character, item2.skinNum, silent: true);
		}
		foreach (UnlockableCharacter item3 in cedar)
		{
			UnlockCharacter(item3.character, item3.skinNum, silent: true);
		}
		foreach (UnlockableCharacter item4 in stacy)
		{
			UnlockCharacter(item4.character, item4.skinNum, silent: true);
		}
		foreach (UnlockableCharacter item5 in kentony)
		{
			UnlockCharacter(item5.character, item5.skinNum, silent: true);
		}
		foreach (UnlockableCharacter item6 in neil)
		{
			UnlockCharacter(item6.character, item6.skinNum, silent: true);
		}
	}

	public void LockEverything()
	{
		foreach (UnlockableCharacter item in henk)
		{
			LockCharacter(item.character, item.skinNum);
		}
		foreach (UnlockableCharacter item2 in betsy)
		{
			LockCharacter(item2.character, item2.skinNum);
		}
		foreach (UnlockableCharacter item3 in cedar)
		{
			LockCharacter(item3.character, item3.skinNum);
		}
		foreach (UnlockableCharacter item4 in stacy)
		{
			LockCharacter(item4.character, item4.skinNum);
		}
		foreach (UnlockableCharacter item5 in kentony)
		{
			LockCharacter(item5.character, item5.skinNum);
		}
		foreach (UnlockableCharacter item6 in neil)
		{
			LockCharacter(item6.character, item6.skinNum);
		}
	}

	public void UnlockStandardCharacters()
	{
		Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Henk, 0, silent: true);
	}

	public List<UnlockableCharacter> GetUnlockableCharacterList(CharacterSelect.Characters character)
	{
		return character switch
		{
			CharacterSelect.Characters.Henk => henk, 
			CharacterSelect.Characters.Betsy => betsy, 
			CharacterSelect.Characters.Afronaut => neil, 
			CharacterSelect.Characters.Selfie => stacy, 
			CharacterSelect.Characters.Kentony => kentony, 
			CharacterSelect.Characters.Cedar => cedar, 
			_ => new List<UnlockableCharacter>(), 
		};
	}

	public List<UnlockableCharacter> GetUnlockedCharacters()
	{
		List<UnlockableCharacter> list = new List<UnlockableCharacter>();
		if (CheckCharacterUnlocked(CharacterSelect.Characters.Henk, 0))
		{
			list.Add(henk[0]);
		}
		if (CheckCharacterUnlocked(CharacterSelect.Characters.Betsy, 0))
		{
			list.Add(betsy[0]);
		}
		if (CheckCharacterUnlocked(CharacterSelect.Characters.Afronaut, 0))
		{
			list.Add(neil[0]);
		}
		if (CheckCharacterUnlocked(CharacterSelect.Characters.Kentony, 0))
		{
			list.Add(kentony[0]);
		}
		if (CheckCharacterUnlocked(CharacterSelect.Characters.Cedar, 0))
		{
			list.Add(cedar[0]);
		}
		return list;
	}

	public List<UnlockableCharacter> GetUnlockedCharacterList(CharacterSelect.Characters character)
	{
		List<UnlockableCharacter> list = new List<UnlockableCharacter>();
		switch (character)
		{
		case CharacterSelect.Characters.Henk:
		{
			for (int m = 0; m < henk.Count; m++)
			{
				if (CheckCharacterUnlocked(henk[m].character, henk[m].skinNum))
				{
					list.Add(henk[m]);
				}
			}
			return list;
		}
		case CharacterSelect.Characters.Betsy:
		{
			for (int k = 0; k < betsy.Count; k++)
			{
				if (CheckCharacterUnlocked(betsy[k].character, betsy[k].skinNum))
				{
					list.Add(betsy[k]);
				}
			}
			return list;
		}
		case CharacterSelect.Characters.Afronaut:
		{
			for (int n = 0; n < neil.Count; n++)
			{
				if (CheckCharacterUnlocked(neil[n].character, neil[n].skinNum))
				{
					list.Add(neil[n]);
				}
			}
			return list;
		}
		case CharacterSelect.Characters.Selfie:
		{
			for (int j = 0; j < stacy.Count; j++)
			{
				if (CheckCharacterUnlocked(stacy[j].character, stacy[j].skinNum))
				{
					list.Add(stacy[j]);
				}
			}
			return list;
		}
		case CharacterSelect.Characters.Kentony:
		{
			for (int l = 0; l < kentony.Count; l++)
			{
				if (CheckCharacterUnlocked(kentony[l].character, kentony[l].skinNum))
				{
					list.Add(kentony[l]);
				}
			}
			return list;
		}
		case CharacterSelect.Characters.Cedar:
		{
			for (int i = 0; i < cedar.Count; i++)
			{
				if (CheckCharacterUnlocked(cedar[i].character, cedar[i].skinNum))
				{
					list.Add(cedar[i]);
				}
			}
			return list;
		}
		default:
			return new List<UnlockableCharacter>();
		}
	}

	public bool CheckCharacterUnlocked(CharacterSelect.Characters character, int skinNum)
	{
		return Singleton<PlayerPrefsManager>.SP.GetCharacterUnlocked(character, skinNum);
	}

	public string GetCharacterName(CharacterSelect.Characters character, int skinNum)
	{
		foreach (UnlockableCharacter unlockableCharacter in GetUnlockableCharacterList(character))
		{
			if (unlockableCharacter.skinNum == skinNum)
			{
				return unlockableCharacter.name;
			}
		}
		Debug.LogError("Couldn't find character name: " + character.ToString() + "_" + skinNum);
		return "Unavailable";
	}

	public string GetCharacterUnlockCriteria(CharacterSelect.Characters character, int skinNum)
	{
		foreach (UnlockableCharacter unlockableCharacter in GetUnlockableCharacterList(character))
		{
			if (unlockableCharacter.skinNum == skinNum)
			{
				return unlockableCharacter.unlockCriteria;
			}
		}
		Debug.LogError("Couldn't find character unlock criteria: " + character.ToString() + "_" + skinNum);
		return "Unavailable";
	}

	public void UnlockCharacter(CharacterSelect.Characters character, int skinNum, bool silent = false)
	{
		MonoBehaviour.print(string.Concat("Unlocking char ", character, "_", skinNum));
		if (!silent && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_Splash)))
		{
			silent = true;
		}
		if (!CheckCharacterUnlocked(character, skinNum) && !silent)
		{
			Singleton<RewardManager>.SP.PopUpReward(RewardManager.UnlockType.Character, character, skinNum);
		}
		Singleton<PlayerPrefsManager>.SP.SetCharacterUnlocked(character, skinNum, unlocked: true);
	}

	public void LockCharacter(CharacterSelect.Characters character, int skinNum)
	{
		Singleton<PlayerPrefsManager>.SP.SetCharacterUnlocked(character, skinNum, unlocked: false);
	}
}
