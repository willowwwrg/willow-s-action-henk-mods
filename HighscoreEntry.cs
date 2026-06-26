using System;
using System.Text.RegularExpressions;

[Serializable]
public class HighscoreEntry
{
	public string name;

	public string userID;

	public string level;

	public int rank;

	public float score;

	public int iScore;

	public bool sendChallenge;

	public bool localPlayerHasBeatenThisScore;

	public int levelsBeaten;

	public int totalTime;

	public HighscoreEntry()
	{
		name = string.Empty;
		score = 0f;
		userID = string.Empty;
		level = string.Empty;
		rank = 0;
		sendChallenge = false;
	}

	public HighscoreEntry(string row)
	{
		name = string.Empty;
		score = 0f;
		userID = string.Empty;
		level = string.Empty;
		rank = 0;
		sendChallenge = false;
		foreach (Match item in Regex.Matches(row, "\"(.*?)\":\"(.*?)\""))
		{
			switch (item.Groups[1].Value)
			{
			case "guid":
				userID = Convert.ToString(item.Groups[2].Value);
				break;
			case "name":
				name = Convert.ToString(item.Groups[2].Value);
				break;
			case "score":
			{
				int num = Convert.ToInt32(item.Groups[2].Value);
				score = (float)num / 1000f;
				break;
			}
			case "level":
				level = Convert.ToString(item.Groups[2].Value);
				break;
			}
		}
	}

	public bool IsMe()
	{
		if (Singleton<ActionHenk>.SP.database == ActionHenk.Database.RageSquid && userID == Singleton<ActionHenk>.SP.GetComputerID())
		{
			return true;
		}
		if (Singleton<ActionHenk>.SP.database == ActionHenk.Database.Steamworks && userID == Singleton<HenkSWUserStats>.SP.GetSteamID().ToString())
		{
			return true;
		}
		return false;
	}
}
