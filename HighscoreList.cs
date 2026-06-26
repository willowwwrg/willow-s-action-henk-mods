using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class HighscoreList : MonoBehaviour
{
	public UILabel scoreLabel;

	public UILabel nameLabel;

	public UILabel rankLabel;

	public UILabel levelsBeatenLabel;

	public UILabel levelsBeatenTitleLabel;

	public UILabel statusLabel;

	public bool ShowLocalHighscore;

	public void ClearList()
	{
		scoreLabel.text = string.Empty;
		nameLabel.text = string.Empty;
		rankLabel.text = string.Empty;
		SetStatusLabel(state: false, string.Empty);
		if ((bool)levelsBeatenLabel)
		{
			levelsBeatenLabel.text = string.Empty;
		}
		if ((bool)levelsBeatenTitleLabel)
		{
			if (!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_Leaderboards)))
			{
				levelsBeatenTitleLabel.enabled = false;
			}
			else
			{
				levelsBeatenTitleLabel.enabled = true;
			}
		}
	}

	public void StartLoadingScores()
	{
		ClearList();
		if (!ShowLocalHighscore)
		{
			Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
			SetStatusLabel(state: true, Singleton<HenkSWLeaderboards>.SP.highscoreSubmitStatus);
		}
		else
		{
			nameLabel.text = "Local Best: ";
			scoreLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Singleton<PlayerPrefsManager>.SP.GetHighscore(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj()));
		}
	}

	public void OnFinishedLoading()
	{
		statusLabel.text = string.Empty;
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
		FillInScoreList();
	}

	private void SetStatusLabel(bool state, string text = "")
	{
		statusLabel.text = text;
		statusLabel.gameObject.SetActive(state);
	}

	public void FillInScoreList(int scoresOffset = 0)
	{
		List<HighscoreEntry> currentLeaderboardEntries = Singleton<HighscoreManager>.SP.currentLeaderboardEntries;
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
		ClearList();
		SetStatusLabel(state: false, string.Empty);
		string empty = string.Empty;
		string text = string.Empty;
		for (int i = scoresOffset; i < scoresOffset + 10 && i < currentLeaderboardEntries.Count; i++)
		{
			if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_Leaderboards)))
			{
				empty = Singleton<HighscoreManager>.SP.ConvertTimeToString(HenkSWLeaderboards.ScoreIntToFloat(currentLeaderboardEntries[i].totalTime));
				text = currentLeaderboardEntries[i].levelsBeaten.ToString();
			}
			else
			{
				empty = Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLeaderboardEntries[i].score);
			}
			int rank = currentLeaderboardEntries[i].rank;
			for (int j = 1; i - j >= 0 && currentLeaderboardEntries[i].score == currentLeaderboardEntries[i - j].score; j++)
			{
				rank = currentLeaderboardEntries[i - j].rank;
			}
			string text2;
			if (currentLeaderboardEntries[i].IsMe())
			{
				UILabel uILabel = nameLabel;
				uILabel.text = uILabel.text + "[00FF00]" + currentLeaderboardEntries[i].name + "[-] \n";
				UILabel uILabel2 = scoreLabel;
				uILabel2.text = uILabel2.text + "[00FF00]" + empty + "[-] \n";
				UILabel uILabel3 = rankLabel;
				text2 = uILabel3.text;
				uILabel3.text = text2 + "[00FF00]" + rank + "[-] \n";
				if ((bool)levelsBeatenLabel)
				{
					UILabel uILabel4 = levelsBeatenLabel;
					uILabel4.text = uILabel4.text + "[00FF00]" + text + "[-] \n";
				}
				continue;
			}
			CSteamID id = new CSteamID
			{
				m_SteamID = ulong.Parse(currentLeaderboardEntries[i].userID)
			};
			UILabel uILabel5 = nameLabel;
			uILabel5.text = uILabel5.text + "[000000]" + Singleton<HenkSWUserStats>.SP.GetNameBySteamID(id) + "[-] \n";
			UILabel uILabel6 = scoreLabel;
			uILabel6.text = uILabel6.text + "[000000]" + empty + "[-] \n";
			UILabel uILabel7 = rankLabel;
			text2 = uILabel7.text;
			uILabel7.text = text2 + "[000000]" + rank + "[-] \n";
			if ((bool)levelsBeatenLabel)
			{
				UILabel uILabel8 = levelsBeatenLabel;
				uILabel8.text = uILabel8.text + "[000000]" + text + "[-] \n";
			}
		}
	}

	public void FillInScoreListNotConnected()
	{
		ShowLocalHighscore = true;
		SetStatusLabel(state: true, Language.Get("ERROR_DOWNLOADINGLEADERBOARDDATA", "PERMA"));
		nameLabel.text = string.Empty;
		scoreLabel.text = string.Empty;
		rankLabel.text = string.Empty;
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
	}
}
