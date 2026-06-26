using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPreviewer : MonoBehaviour
{
	public List<PlayerGraphics> previewModels;

	public Material[] blackMaterials;

	private int selectedPreviewCharacterSkin;

	private int lookingAtCharacterNum = 1;

	public CharacterSelect.Characters lookingAt = CharacterSelect.Characters.Henk;

	public bool ignoreErrorSound;

	public bool swappingSkin;

	private bool initialized;

	private CharacterSelect.Characters initialCharacter = CharacterSelect.Characters.Henk;

	private int initialSkinNum;

	private void Start()
	{
		Singleton<CharacterSelect>.SP.SetCharacterPreviewer(this);
		Singleton<CharacterSelect>.SP.InitCharacterPreviewer();
	}

	public CharacterPreviewer InitPreviewer()
	{
		initialCharacter = (CharacterSelect.Characters)Singleton<PlayerPrefsManager>.SP.GetInt("LASTPLAYEDCHARACTER", 1);
		initialSkinNum = Singleton<PlayerPrefsManager>.SP.GetInt(initialCharacter.ToString() + "LASTPLAYEDSKIN", 0);
		if (initialized)
		{
			return this;
		}
		InitCharacterPreviewModels();
		SelectCharacter((CharacterSelect.Characters)Singleton<PlayerPrefsManager>.SP.GetInt("LASTPLAYEDCHARACTER", 1));
		SetSkin();
		initialized = true;
		return this;
	}

	public void NextCharacter()
	{
		lookingAtCharacterNum++;
		if (lookingAtCharacterNum > previewModels.Count - 1)
		{
			lookingAtCharacterNum = 0;
		}
		SelectCharacter(previewModels[lookingAtCharacterNum].currentCharacter);
		SetSkin();
	}

	public void PrevCharacter()
	{
		lookingAtCharacterNum--;
		if (lookingAtCharacterNum < 0)
		{
			lookingAtCharacterNum = previewModels.Count - 1;
		}
		SelectCharacter(previewModels[lookingAtCharacterNum].currentCharacter);
		SetSkin();
	}

	public void NextSkin()
	{
		if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(previewModels[lookingAtCharacterNum].currentCharacter, 0) && Singleton<UnlockManager>.SP.GetUnlockedCharacterList(lookingAt).Count >= 2)
		{
			AudioController.Play("NextMenuItem");
			selectedPreviewCharacterSkin++;
			SkinBoundaryCheck(up: true);
			int skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
			while (!Singleton<UnlockManager>.SP.CheckCharacterUnlocked(previewModels[lookingAtCharacterNum].currentCharacter, skinNum))
			{
				selectedPreviewCharacterSkin++;
				SkinBoundaryCheck(up: true);
				skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
			}
			SetSkin();
		}
	}

	public void PrevSkin()
	{
		if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(previewModels[lookingAtCharacterNum].currentCharacter, 0) && Singleton<UnlockManager>.SP.GetUnlockedCharacterList(lookingAt).Count >= 2)
		{
			AudioController.Play("PrevMenuItem");
			selectedPreviewCharacterSkin--;
			SkinBoundaryCheck(up: false);
			int skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
			while (!Singleton<UnlockManager>.SP.CheckCharacterUnlocked(previewModels[lookingAtCharacterNum].currentCharacter, skinNum))
			{
				selectedPreviewCharacterSkin--;
				SkinBoundaryCheck(up: false);
				skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
			}
			SetSkin();
		}
	}

	public void GoToPrevWindow()
	{
		Singleton<PlayerPrefsManager>.SP.SetInt(initialCharacter.ToString() + "LASTPLAYEDSKIN", initialSkinNum);
		SelectCharacter(initialCharacter);
		SetSkin();
	}

	public void SkinBoundaryCheck(bool up)
	{
		if (up)
		{
			if (selectedPreviewCharacterSkin > Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt).Count - 1)
			{
				selectedPreviewCharacterSkin = 0;
			}
			if (!CheckIfSkinExists(lookingAt, Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum))
			{
				selectedPreviewCharacterSkin = 0;
			}
		}
		else
		{
			if (selectedPreviewCharacterSkin < 0)
			{
				selectedPreviewCharacterSkin = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt).Count - 1;
			}
			if (!CheckIfSkinExists(lookingAt, Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum))
			{
				selectedPreviewCharacterSkin = 0;
			}
		}
	}

	public void SetSkin()
	{
		swappingSkin = true;
		int skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
		previewModels[lookingAtCharacterNum].SetModel(lookingAt, skinNum);
		Singleton<CharacterSelect>.SP.SetSelectedSkin(skinNum);
		Singleton<CharacterSelect>.SP.guiObj.SetSkinName(Singleton<UnlockManager>.SP.GetCharacterName(lookingAt, skinNum));
		StartCoroutine(CheckIfLocked(previewModels[lookingAtCharacterNum]));
	}

	public bool CheckIfSkinExists(CharacterSelect.Characters character, int skinNum)
	{
		if (Resources.Load(string.Concat("CharacterModels/", character, "_", skinNum)) as GameObject == null)
		{
			return false;
		}
		return true;
	}

	public void SelectCharacter(CharacterSelect.Characters character)
	{
		lookingAt = character;
		Camera.main.GetComponent<MenuCamera>().LookAtCharacter(character);
		int num = Singleton<PlayerPrefsManager>.SP.GetInt(lookingAt.ToString() + "LASTPLAYEDSKIN", 0);
		List<UnlockableCharacter> unlockableCharacterList = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(character);
		for (int i = 0; i < unlockableCharacterList.Count; i++)
		{
			if (unlockableCharacterList[i].skinNum == num)
			{
				selectedPreviewCharacterSkin = i;
			}
		}
		if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(character, num))
		{
			Singleton<CharacterSelect>.SP.guiObj.SetCharacterName(Singleton<UnlockManager>.SP.GetCharacterName(character, 0));
			Singleton<PermaGUI>.SP.ToggleCharacterInfo(state: false, string.Empty);
			Singleton<CharacterSelect>.SP.SetSelectedCharacter(character);
		}
		else
		{
			Singleton<CharacterSelect>.SP.guiObj.SetCharacterName("??????");
			Singleton<PermaGUI>.SP.ToggleCharacterInfo(state: true, Singleton<UnlockManager>.SP.GetCharacterUnlockCriteria(character, num));
		}
		Singleton<CharacterSelect>.SP.guiObj.UpdateCharacterScrollIndicators(character);
		for (int j = 0; j < previewModels.Count; j++)
		{
			if (previewModels[j].currentCharacter == lookingAt)
			{
				lookingAtCharacterNum = j;
				break;
			}
		}
	}

	public bool ConfirmSkinChoice()
	{
		int skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
		if (!Singleton<UnlockManager>.SP.CheckCharacterUnlocked(lookingAt, skinNum))
		{
			return false;
		}
		Singleton<CharacterSelect>.SP.SetSelectedCharacter(lookingAt);
		Singleton<CharacterSelect>.SP.SetSelectedSkin(skinNum);
		Singleton<AudioManager>.SP.PlayCharacterIntro(lookingAt, skinNum);
		return true;
	}

	public bool ConfirmCharacterChoice()
	{
		if (!Singleton<UnlockManager>.SP.CheckCharacterUnlocked(lookingAt, 0))
		{
			return false;
		}
		previewModels[lookingAtCharacterNum].GetComponentInChildren<PlayerGraphics>().OnCharacterSelected();
		Singleton<AudioManager>.SP.PlayCharacterLaugh(previewModels[lookingAtCharacterNum].gameObject);
		Singleton<CharacterSelect>.SP.SetSelectedCharacter(lookingAt);
		Singleton<CharacterSelect>.SP.guiObj.SetSkinName(Singleton<UnlockManager>.SP.GetCharacterName(lookingAt, Singleton<PlayerPrefsManager>.SP.GetInt(lookingAt.ToString() + "LASTPLAYEDSKIN", 0)));
		return true;
	}

	private void InitCharacterPreviewModels()
	{
		previewModels[0].SetModel(CharacterSelect.Characters.Henk, Singleton<PlayerPrefsManager>.SP.GetInt(CharacterSelect.Characters.Henk.ToString() + "LASTPLAYEDSKIN", 0));
		previewModels[1].SetModel(CharacterSelect.Characters.Betsy, Singleton<PlayerPrefsManager>.SP.GetInt(CharacterSelect.Characters.Betsy.ToString() + "LASTPLAYEDSKIN", 0));
		StartCoroutine(CheckIfLocked(previewModels[1]));
		previewModels[2].SetModel(CharacterSelect.Characters.Afronaut, Singleton<PlayerPrefsManager>.SP.GetInt(CharacterSelect.Characters.Afronaut.ToString() + "LASTPLAYEDSKIN", 0));
		StartCoroutine(CheckIfLocked(previewModels[2]));
		previewModels[3].SetModel(CharacterSelect.Characters.Kentony, Singleton<PlayerPrefsManager>.SP.GetInt(CharacterSelect.Characters.Kentony.ToString() + "LASTPLAYEDSKIN", 0));
		StartCoroutine(CheckIfLocked(previewModels[3]));
		previewModels[4].SetModel(CharacterSelect.Characters.Cedar, Singleton<PlayerPrefsManager>.SP.GetInt(CharacterSelect.Characters.Cedar.ToString() + "LASTPLAYEDSKIN", 0));
		StartCoroutine(CheckIfLocked(previewModels[4]));
	}

	private IEnumerator CheckIfLocked(PlayerGraphics previewModel)
	{
		while (previewModel.spawningModel)
		{
			yield return new WaitForEndOfFrame();
		}
		if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(previewModel.currentCharacter, previewModel.currentSkinNum))
		{
			yield break;
		}
		Singleton<CharacterSelect>.SP.guiObj.SetSkinName(string.Empty, locked: true);
		Renderer[] componentsInChildren = previewModel.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.GetType() == typeof(SkinnedMeshRenderer) || renderer.GetType() == typeof(MeshRenderer))
			{
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					renderer.materials = blackMaterials;
				}
			}
		}
	}

	public void CheckIfBackToDefaultModel()
	{
		int skinNum = Singleton<UnlockManager>.SP.GetUnlockableCharacterList(lookingAt)[selectedPreviewCharacterSkin].skinNum;
		if (!Singleton<UnlockManager>.SP.CheckCharacterUnlocked(lookingAt, skinNum))
		{
			selectedPreviewCharacterSkin = 0;
			Singleton<CharacterSelect>.SP.SetSelectedSkinNum(0);
			previewModels[lookingAtCharacterNum].SetModel(lookingAt);
		}
	}
}
