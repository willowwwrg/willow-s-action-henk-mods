using System.Collections.Generic;
using UnityEngine;

public class GUI_LMP : GUI_Base
{
	public List<TweenAlpha> readyLabelTweens;

	public List<TweenAlpha> joinLabelTweens;

	public List<UILabel> nameLabels;

	public List<UILabel> charNameLabels;

	public TweenPosition startButton;

	public CharacterPreviewerLMP charPreviewer;

	public UILabel levelNames;

	public UILabel setting_CheckpointRespawning;

	public UILabel setting_RetriesPerLevel;

	private int maxLevelsInList = 10;

	private void Start()
	{
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		string text = string.Empty;
		int num = Singleton<LocalMultiManager>.SP.levelRotation.Length;
		int num2 = num;
		if (num > maxLevelsInList)
		{
			num2 = maxLevelsInList - 1;
		}
		for (int i = 0; i < num2; i++)
		{
			text = text + Singleton<LevelBatchManager>.SP.GetLevelFromCode((int)Singleton<LocalMultiManager>.SP.levelRotation[i]).levelName + "\n";
		}
		if (num > maxLevelsInList)
		{
			int num3 = num - maxLevelsInList + 1;
			text += Language.Get("LMPMORELEVELS", "PERMA").Replace("{X}", num3.ToString());
		}
		levelNames.text = text;
		int num4 = num2 * 25;
		if (num > maxLevelsInList)
		{
			num4 += 25;
		}
		levelNames.transform.parent.localPosition = new Vector3(0f, -400 + num4, 0f);
		setting_CheckpointRespawning.text = ((Singleton<PlayerPrefsManager>.SP.GetInt("LMPSettings_CHECKPOINTRESPAWNING", 1) != 0) ? "< Yes >" : "< No >");
		setting_RetriesPerLevel.text = Singleton<PlayerPrefsManager>.SP.GetInt("LMPSettings_NUMRETRIES", 3).ToString();
		Singleton<LocalMultiManager>.SP.allowCheckpointRespawn = Singleton<PlayerPrefsManager>.SP.GetInt("LMPSettings_CHECKPOINTRESPAWNING", 1) != 0;
		Singleton<LocalMultiManager>.SP.attemptsPerLevel = Singleton<PlayerPrefsManager>.SP.GetInt("LMPSettings_NUMRETRIES", 3);
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
	}

	private void Update()
	{
		if (charPreviewer == null)
		{
			charPreviewer = Object.FindObjectOfType<CharacterPreviewerLMP>();
		}
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (charPreviewer.lockedInPlayers[i])
			{
				num++;
				joinLabelTweens[i].Play(forward: false);
				readyLabelTweens[i].Play(forward: true);
				nameLabels[i].transform.parent.GetComponent<TweenAlpha>().Play(forward: false);
			}
			else if (charPreviewer.IsCharacterActive(i))
			{
				joinLabelTweens[i].Play(forward: false);
				readyLabelTweens[i].Play(forward: false);
				nameLabels[i].transform.parent.GetComponent<TweenAlpha>().Play(forward: true);
			}
			else
			{
				joinLabelTweens[i].Play(forward: true);
				readyLabelTweens[i].Play(forward: false);
				nameLabels[i].transform.parent.GetComponent<TweenAlpha>().Play(forward: false);
			}
		}
		int num2 = 0;
		for (int j = 0; j < 4; j++)
		{
			if (charPreviewer.IsCharacterActive(j))
			{
				num2++;
			}
		}
		bool flag = true;
		for (int k = 0; k < 4; k++)
		{
			if (charPreviewer.playerCharacters[k].character == Singleton<LocalMultiManager>.SP.playerInfo[k].selectedCharacter.character)
			{
				if (charPreviewer.playerCharacters[k].skinNum == Singleton<LocalMultiManager>.SP.playerInfo[k].selectedCharacter.skinNum)
				{
				}
			}
			else
			{
				flag = false;
			}
		}
		if (num < 2)
		{
			flag = false;
		}
		if (flag)
		{
			startButton.Play(forward: true);
		}
		else
		{
			startButton.Play(forward: false);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.StartLMP))
		{
			if (flag)
			{
				StartGame();
			}
			else
			{
				AudioController.Play("ButtonClick");
			}
		}
	}

	private void StartGame()
	{
		AudioController.Play("LevelStart");
		Singleton<LocalMultiManager>.SP.StartGame();
	}
}
