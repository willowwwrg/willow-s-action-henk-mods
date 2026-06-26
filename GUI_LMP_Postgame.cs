using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_LMP_Postgame : GUI_Base
{
	public UILabel nameLabel;

	public UILabel scoreLabel;

	public UILabel nameLabel2;

	public UILabel scoreLabel2;

	public UILabel subtitleLabel;

	public UILabel nextLevelLabel;

	public GameObject postgame1;

	public GameObject postgame2;

	public State_LMP_Postgame stateObj;

	public List<GameObject> scoreBackgroundBars;

	private void TransitionCompleted()
	{
		InitializeScreen();
		for (int i = 0; i < scoreBackgroundBars.Count; i++)
		{
			scoreBackgroundBars[i].SetActive(value: false);
		}
		postgame1.SetActive(value: true);
		postgame2.SetActive(value: false);
		List<LMPPlayerScoreEntry> list = new List<LMPPlayerScoreEntry>();
		int[] array = new int[4];
		for (int j = 0; j < 4; j++)
		{
			array[j] = Singleton<LocalMultiManager>.SP.playerInfo[j].playerScore;
		}
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject in localPlayers)
		{
			LMPPlayerScoreEntry item = default(LMPPlayerScoreEntry);
			item.playerNum = gameObject.GetComponent<PlatformerController>().localPlayerNumber;
			item.finishRank = Singleton<LocalMultiManager>.SP.playerInfo[item.playerNum].finishedNumber;
			item.dnf = false;
			item.scoreChange = 0;
			item.numResets = Singleton<LocalMultiManager>.SP.playerInfo[item.playerNum].numResets;
			if (item.finishRank != 0)
			{
				int num = 5 - item.finishRank;
				int num2 = num - item.numResets;
				if (num2 < 0)
				{
					num2 = 0;
				}
				Singleton<LocalMultiManager>.SP.playerInfo[item.playerNum].playerScore += num2;
				item.scoreChange = num;
				list.Add(item);
			}
			else
			{
				item.finishRank = 99;
				item.dnf = true;
				list.Add(item);
			}
			list.Sort((LMPPlayerScoreEntry x, LMPPlayerScoreEntry y) => x.finishRank.CompareTo(y.finishRank));
		}
		nameLabel.text = string.Empty;
		scoreLabel.text = string.Empty;
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			string text = Singleton<LocalMultiManager>.SP.playerColorCodes[list[num3].playerNum] + Singleton<UnlockManager>.SP.GetCharacterName(Singleton<LocalMultiManager>.SP.playerInfo[list[num3].playerNum].selectedCharacter.character, Singleton<LocalMultiManager>.SP.playerInfo[list[num3].playerNum].selectedCharacter.skinNum) + "[-]";
			text += "\n";
			string empty = string.Empty;
			if (list[num3].dnf)
			{
				empty = Language.Get("LMPDNF", "PERMA");
			}
			else
			{
				empty = "+" + list[num3].scoreChange;
				if (list[num3].numResets != 0)
				{
					empty = empty + " [ff0000]-" + list[num3].numResets + "[-]";
				}
			}
			empty += "\n";
			nameLabel.text += text;
			scoreLabel.text += empty;
			scoreBackgroundBars[num3].SetActive(value: true);
		}
		nameLabel2.text = string.Empty;
		scoreLabel2.text = string.Empty;
		int[] playersSortedByScore = Singleton<LocalMultiManager>.SP.GetPlayersSortedByScore();
		for (int num4 = 0; num4 < playersSortedByScore.Length; num4++)
		{
			string text2 = Singleton<LocalMultiManager>.SP.playerColorCodes[playersSortedByScore[num4]] + Singleton<UnlockManager>.SP.GetCharacterName(Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[num4]].selectedCharacter.character, Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[num4]].selectedCharacter.skinNum) + "[-]";
			text2 += "\n";
			int playerScore = Singleton<LocalMultiManager>.SP.playerInfo[playersSortedByScore[num4]].playerScore;
			int num5 = playerScore - array[playersSortedByScore[num4]];
			string text3 = playerScore + " (+" + num5 + ")";
			text3 += "\n";
			nameLabel2.text += text2;
			scoreLabel2.text += text3;
		}
		int attemptsLeft = Singleton<LocalMultiManager>.SP.GetAttemptsLeft();
		int attemptsPerLevel = Singleton<LocalMultiManager>.SP.attemptsPerLevel;
		int num6 = attemptsPerLevel - attemptsLeft + 1;
		subtitleLabel.text = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelName + " (" + num6 + "/" + attemptsPerLevel + ")";
		StartCoroutine(Stage2Postgame());
	}

	private IEnumerator Stage2Postgame()
	{
		yield return new WaitForSeconds(4f);
		postgame1.SetActive(value: false);
		postgame2.SetActive(value: true);
	}

	private void Update()
	{
		string text = "[FFFFFF]";
		int num = Mathf.CeilToInt(stateObj.countdown);
		if (num <= 3)
		{
			text = "[FF0000]";
		}
		string newValue = text + num + "[-]";
		if (Singleton<LocalMultiManager>.SP.GetAttemptsLeft() == 1)
		{
			if (Singleton<LocalMultiManager>.SP.IsGameOver())
			{
				nextLevelLabel.text = Language.Get("LMPGAMEOVER", "PERMA");
			}
			else
			{
				nextLevelLabel.text = Language.Get("LMPNEXTLEVEL", "PERMA").Replace("{X}", newValue);
			}
		}
		else
		{
			nextLevelLabel.text = Language.Get("LMPNEXTATTEMPT", "PERMA").Replace("{X}", newValue);
		}
	}
}
