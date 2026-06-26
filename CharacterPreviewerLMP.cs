using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class CharacterPreviewerLMP : MonoBehaviour
{
	public List<PlayerGraphics> previewModels;

	public int[] skinsIterator = new int[4];

	public int[] charIterator = new int[4];

	private CharacterSelect.Characters[] characters = new CharacterSelect.Characters[4]
	{
		CharacterSelect.Characters.Henk,
		CharacterSelect.Characters.Henk,
		CharacterSelect.Characters.Henk,
		CharacterSelect.Characters.Henk
	};

	public CharSkin[] playerCharacters = new CharSkin[4];

	public bool[] lockedInPlayers = new bool[4];

	public GUI_LMP lmpGui;

	public void SetCharacter(bool state, int controllerNum, int charNum = 0, int skinNum = 0)
	{
		if (state)
		{
			List<UnlockableCharacter> unlockedCharacters = Singleton<UnlockManager>.SP.GetUnlockedCharacters();
			previewModels[controllerNum].gameObject.SetActive(value: true);
			skinsIterator[controllerNum] = skinNum;
			characters[controllerNum] = unlockedCharacters[charIterator[controllerNum]].character;
			List<UnlockableCharacter> unlockedCharacterList = Singleton<UnlockManager>.SP.GetUnlockedCharacterList(characters[controllerNum]);
			previewModels[controllerNum].GetComponent<PlatformerController>().localPlayerNumber = controllerNum;
			previewModels[controllerNum].SetModel(unlockedCharacters[charIterator[controllerNum]].character, unlockedCharacterList[skinNum].skinNum);
			playerCharacters[controllerNum].character = unlockedCharacters[charIterator[controllerNum]].character;
			playerCharacters[controllerNum].skinNum = unlockedCharacterList[skinNum].skinNum;
			lmpGui.nameLabels[controllerNum].text = Singleton<LocalMultiManager>.SP.playerColorCodes[controllerNum] + "< " + Singleton<UnlockManager>.SP.GetCharacterName(playerCharacters[controllerNum].character, playerCharacters[controllerNum].skinNum) + " >[-]";
			lmpGui.charNameLabels[controllerNum].text = Singleton<LocalMultiManager>.SP.playerColorCodes[controllerNum] + "< " + Singleton<UnlockManager>.SP.GetCharacterName(playerCharacters[controllerNum].character, 0) + " >[-]";
		}
		else
		{
			previewModels[controllerNum].gameObject.SetActive(value: false);
			playerCharacters[controllerNum] = default(CharSkin);
		}
	}

	public bool IsCharacterActive(int controllerNum)
	{
		return previewModels[controllerNum].gameObject.activeInHierarchy;
	}

	public void SetNextSkin(bool forwards, int controllerNum)
	{
		List<UnlockableCharacter> unlockedCharacterList = Singleton<UnlockManager>.SP.GetUnlockedCharacterList(characters[controllerNum]);
		if (unlockedCharacterList.Count < 2)
		{
			return;
		}
		if (forwards)
		{
			skinsIterator[controllerNum]++;
			if (skinsIterator[controllerNum] > unlockedCharacterList.Count - 1)
			{
				skinsIterator[controllerNum] = 0;
			}
			AudioController.Play("NextMenuItem");
			SetCharacter(state: true, controllerNum, charIterator[controllerNum], skinsIterator[controllerNum]);
		}
		else
		{
			skinsIterator[controllerNum]--;
			if (skinsIterator[controllerNum] < 0)
			{
				skinsIterator[controllerNum] = unlockedCharacterList.Count - 1;
			}
			AudioController.Play("PrevMenuItem");
			SetCharacter(state: true, controllerNum, charIterator[controllerNum], skinsIterator[controllerNum]);
		}
	}

	public void SetNextCharacter(bool forwards, int controllerNum)
	{
		List<UnlockableCharacter> unlockedCharacters = Singleton<UnlockManager>.SP.GetUnlockedCharacters();
		if (unlockedCharacters.Count < 2)
		{
			return;
		}
		if (forwards)
		{
			charIterator[controllerNum]++;
			if (charIterator[controllerNum] > unlockedCharacters.Count - 1)
			{
				charIterator[controllerNum] = 0;
			}
			AudioController.Play("NextMenuItem");
			SetCharacter(state: true, controllerNum, charIterator[controllerNum]);
		}
		else
		{
			charIterator[controllerNum]--;
			if (charIterator[controllerNum] < 0)
			{
				charIterator[controllerNum] = unlockedCharacters.Count - 1;
			}
			AudioController.Play("PrevMenuItem");
			SetCharacter(state: true, controllerNum, charIterator[controllerNum]);
		}
	}

	private void Update()
	{
		if (lmpGui == null)
		{
			lmpGui = Singleton<GUIManager>.SP.GetGameObjectFromScreenName(GUIManager.GUIScreens.GUIScreen_LMP).GetComponent<GUI_LMP>();
		}
		if (!Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters)
		{
			SetCharacter(state: false, 0);
			SetCharacter(state: false, 1);
			SetCharacter(state: false, 2);
			SetCharacter(state: false, 3);
			for (int i = 0; i < lockedInPlayers.Length; i++)
			{
				lockedInPlayers[i] = false;
			}
		}
		if (!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_LMP)) || Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_LMP)
		{
			return;
		}
		for (int num = ReInput.players.playerCount - 1; num >= 0; num--)
		{
			int controllerNum = num;
			if (Singleton<ControllerInput>.SP.GetKeyDown(controllerNum, "A"))
			{
				if (lockedInPlayers[num])
				{
					break;
				}
				if (!IsCharacterActive(num))
				{
					SetCharacter(state: true, num, charIterator[num], skinsIterator[num]);
					lockedInPlayers[num] = false;
					Singleton<AudioManager>.SP.PlayCheckboxToggleSound(onOff: true);
				}
				else
				{
					Singleton<LocalMultiManager>.SP.playerInfo[num].selectedCharacter.character = playerCharacters[num].character;
					Singleton<LocalMultiManager>.SP.playerInfo[num].selectedCharacter.skinNum = playerCharacters[num].skinNum;
					lockedInPlayers[num] = true;
					Singleton<AudioManager>.SP.PlayCharacterIntro(playerCharacters[num].character, playerCharacters[num].skinNum);
					AudioController.Play("SelectCharacterConfirm");
				}
			}
			if (Singleton<ControllerInput>.SP.GetKeyDown(controllerNum, "B") && IsCharacterActive(num))
			{
				Singleton<AudioManager>.SP.PlayCheckboxToggleSound(onOff: false);
				if (!lockedInPlayers[num])
				{
					SetCharacter(state: false, num);
				}
				else
				{
					lockedInPlayers[num] = false;
					Singleton<LocalMultiManager>.SP.playerInfo[num].selectedCharacter = default(CharSkin);
				}
			}
			if (Singleton<ControllerInput>.SP.GetKeyDown(controllerNum, "LB"))
			{
				if (lockedInPlayers[num])
				{
					break;
				}
				if (IsCharacterActive(num))
				{
					SetNextSkin(forwards: false, num);
				}
			}
			if (Singleton<ControllerInput>.SP.GetKeyDown(controllerNum, "RB"))
			{
				if (lockedInPlayers[num])
				{
					break;
				}
				if (IsCharacterActive(num))
				{
					SetNextSkin(forwards: true, num);
				}
			}
			if (Singleton<ControllerInput>.SP.GetKeyDown(controllerNum, "RT"))
			{
				if (lockedInPlayers[num])
				{
					break;
				}
				if (IsCharacterActive(num))
				{
					SetNextCharacter(forwards: true, num);
				}
			}
			if (Singleton<ControllerInput>.SP.GetKeyDown(controllerNum, "LT"))
			{
				if (lockedInPlayers[num])
				{
					break;
				}
				if (IsCharacterActive(num))
				{
					SetNextCharacter(forwards: false, num);
				}
			}
		}
	}
}
