using UnityEngine;

public class CharacterSelect : Singleton<CharacterSelect>
{
	public enum Characters
	{
		None,
		Henk,
		Afronaut,
		Betsy,
		Kentony,
		Cedar,
		Selfie
	}

	public GUI_CharacterSelectionCampaign guiObj;

	private Characters selectedCharacter = Characters.Henk;

	private int selectedSkin;

	private CharacterPreviewer charPreview;

	private void Awake()
	{
		selectedCharacter = (Characters)PlayerPrefs.GetInt("LASTPLAYEDCHARACTER", 1);
	}

	private void Update()
	{
	}

	public CharacterPreviewer GetCharacterPreviewer()
	{
		return charPreview;
	}

	public void SetCharacterPreviewer(CharacterPreviewer charPreviewer)
	{
		charPreview = charPreviewer;
	}

	public CharacterPreviewer InitCharacterPreviewer()
	{
		return charPreview.InitPreviewer();
	}

	public void SetSelectedCharacter(Characters character)
	{
		selectedCharacter = character;
		if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(character, 0))
		{
			Singleton<PlayerPrefsManager>.SP.SetInt("LASTPLAYEDCHARACTER", (int)selectedCharacter);
		}
	}

	public Characters GetSelectedCharacter()
	{
		return selectedCharacter;
	}

	public Characters GetLookingAtCharacter()
	{
		return charPreview.lookingAt;
	}

	public void SetSelectedSkin(int skinNum, GameObject obj = null)
	{
		selectedSkin = skinNum;
		if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(charPreview.lookingAt, selectedSkin))
		{
			Singleton<PlayerPrefsManager>.SP.SetInt(charPreview.lookingAt.ToString() + "LASTPLAYEDSKIN", selectedSkin);
		}
	}

	public void SetSelectedSkinNum(int skinNum)
	{
		selectedSkin = skinNum;
	}

	public int GetSelectedSkin()
	{
		return selectedSkin;
	}
}
