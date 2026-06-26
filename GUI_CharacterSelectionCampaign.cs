using System.Collections.Generic;
using UnityEngine;

public class GUI_CharacterSelectionCampaign : GUI_Base
{
	public State_CharacterSelectionCampaign stateObj;

	public UILabel charLabel;

	public UILabel skinLabel;

	public UILabel unlockCriteriaLabel;

	public UILabel infoLabel;

	public List<GameObject> skinArrows;

	private string characterText = string.Empty;

	private string skinText = string.Empty;

	public List<UISprite> characterScrollIndicators;

	private void TransitionCompleted()
	{
		InitializeScreen();
		string characterName = Singleton<UnlockManager>.SP.GetCharacterName(Singleton<CharacterSelect>.SP.GetSelectedCharacter(), Singleton<CharacterSelect>.SP.GetSelectedSkin());
		SetSkinName(characterName);
	}

	private void LeaveGUIScreen()
	{
		Singleton<PermaGUI>.SP.ShowMedalCount(toggle: false);
	}

	private void Update()
	{
		if (Singleton<UnlockManager>.SP.GetUnlockedCharacterList(Singleton<CharacterSelect>.SP.GetCharacterPreviewer().lookingAt).Count < 2)
		{
			ToggleSkinArrows(state: false);
		}
		else
		{
			ToggleSkinArrows(state: true);
		}
	}

	public void ToggleSkinArrows(bool state)
	{
		foreach (GameObject skinArrow in skinArrows)
		{
			skinArrow.SetActive(state);
		}
	}

	public void SetStateCharSelect()
	{
		skinText = string.Empty;
	}

	public void ToggleUnlockCriteria(bool state, string text = "")
	{
		unlockCriteriaLabel.transform.parent.GetComponent<TweenPosition>().Play(state);
		if (state)
		{
			unlockCriteriaLabel.text = text;
		}
	}

	public void SetSkinName(string skinName, bool locked = false)
	{
		if (locked)
		{
			skinText = "???? ????";
			ToggleUnlockCriteria(state: true, Singleton<UnlockManager>.SP.GetCharacterUnlockCriteria(Singleton<CharacterSelect>.SP.GetLookingAtCharacter(), Singleton<CharacterSelect>.SP.GetSelectedSkin()));
		}
		else
		{
			skinText = skinName;
			skinLabel.text = string.Empty;
			ToggleUnlockCriteria(state: false, string.Empty);
		}
		skinLabel.text = skinText;
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_CharacterSelectionCampaign)
		{
			return;
		}
		if (locked)
		{
			infoLabel.transform.parent.GetComponent<UITweener>().Play(forward: false);
			infoLabel.text = string.Empty;
			return;
		}
		string text = Language.Get("SKIN_" + (int)Singleton<CharacterSelect>.SP.GetLookingAtCharacter() + "_" + Singleton<CharacterSelect>.SP.GetSelectedSkin(), "PERMA");
		if (!text.StartsWith("#!#"))
		{
			infoLabel.transform.parent.GetComponent<UITweener>().Play(forward: true);
			infoLabel.text = text;
		}
		else
		{
			infoLabel.transform.parent.GetComponent<UITweener>().Play(forward: false);
			infoLabel.text = string.Empty;
		}
	}

	public void SetCharacterName(string characterName)
	{
		charLabel.text = characterName;
	}

	public void UpdateCharacterScrollIndicators(CharacterSelect.Characters character)
	{
		for (int i = 0; i < characterScrollIndicators.Count; i++)
		{
			characterScrollIndicators[i].alpha = 0.2f;
		}
		switch (character)
		{
		case CharacterSelect.Characters.Henk:
			characterScrollIndicators[0].alpha = 1f;
			break;
		case CharacterSelect.Characters.Betsy:
			characterScrollIndicators[1].alpha = 1f;
			break;
		case CharacterSelect.Characters.Afronaut:
			characterScrollIndicators[2].alpha = 1f;
			break;
		case CharacterSelect.Characters.Kentony:
			characterScrollIndicators[3].alpha = 1f;
			break;
		case CharacterSelect.Characters.Cedar:
			characterScrollIndicators[4].alpha = 1f;
			break;
		}
	}

	private void NextWindow()
	{
		if (stateObj.Confirm())
		{
			AudioController.Play("SelectCharacterConfirm");
		}
	}

	private void PrevWindow()
	{
		AudioController.Play("ButtonBackwards");
		if (stateObj.Confirm())
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		}
	}
}
