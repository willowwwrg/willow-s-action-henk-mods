using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Steamworks;
using UnityEngine;

public class HighscoreManager : Singleton<HighscoreManager>
{
	private bool initialized;

	private bool connectedToInternet = true;

	private bool currentFetchDone;

	[HideInInspector]
	public bool submittingScore;

	private bool timedOut;

	public List<HighscoreEntry> RageSquidLeaderboard = new List<HighscoreEntry>();

	public List<HighscoreEntry> currentLeaderboardEntries = new List<HighscoreEntry>();

	public bool scoresUpToDate;

	public GUI_PostGame PostGameLeaderboard;

	public bool newLocalBest;

	public ObscuredInt multiplayerScoreNumber = -1;

	public int totalScore;

	private int currentLevelSubmitting;

	public bool gotNewHighscore;

	private bool totalScorePosted;

	public void ScoresUpToDate(bool toggle)
	{
		if (toggle)
		{
			scoresUpToDate = true;
			switch (Singleton<ActionHenk>.SP.database)
			{
			case ActionHenk.Database.RageSquid:
				PostGameLeaderboard.FillInScoreLabelsRageSquid();
				break;
			case ActionHenk.Database.Steamworks:
				if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)))
				{
					StartCoroutine(PostGameLeaderboard.FillInScoreLabelsSteamworks(HighscoreState.Friends));
				}
				break;
			case ActionHenk.Database.None:
			case ActionHenk.Database.Playstation:
				break;
			}
		}
		else
		{
			scoresUpToDate = false;
		}
	}

	public void SetCurrentLeaderboardEntries(LeaderboardEntry_t[] entries, Level level, int[][] extraData = null)
	{
		currentLeaderboardEntries.Clear();
		int num = int.MaxValue;
		if ((bool)level)
		{
			for (int i = 0; i < entries.Length; i++)
			{
				LeaderboardEntry_t leaderboardEntry_t = entries[i];
				if (!(leaderboardEntry_t.m_steamIDUser == Singleton<HenkSWUserStats>.SP.GetSteamID()))
				{
					continue;
				}
				num = leaderboardEntry_t.m_nScore;
				float num2 = HenkSWLeaderboards.ScoreIntToFloat(num);
				if (!(level != null))
				{
					continue;
				}
				float highscore = Singleton<PlayerPrefsManager>.SP.GetHighscore(level);
				if (highscore != 0f)
				{
					if (num2 > highscore)
					{
						Debug.LogWarning("Overwriting local score by steam score. local best was better than steam score. Level: " + level.levelName);
						Debug.LogWarning("Local score was: " + highscore + ". Steam score is: " + num2);
						Singleton<PlayerPrefsManager>.SP.SetHighscore(level, num2);
					}
					else if (num2 < highscore)
					{
						Debug.LogWarning("Overwriting local score by steam score. local best was slower than steam score. Level: " + level.levelName);
						Debug.LogWarning("Local score was: " + highscore + ". Steam score is: " + num2);
						Singleton<PlayerPrefsManager>.SP.SetHighscore(level, num2);
					}
				}
			}
		}
		for (int j = 0; j < entries.Length; j++)
		{
			LeaderboardEntry_t leaderboardEntry_t2 = entries[j];
			HighscoreEntry highscoreEntry = new HighscoreEntry();
			highscoreEntry.score = HenkSWLeaderboards.ScoreIntToFloat(leaderboardEntry_t2.m_nScore);
			highscoreEntry.iScore = leaderboardEntry_t2.m_nScore;
			highscoreEntry.name = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(leaderboardEntry_t2.m_steamIDUser);
			highscoreEntry.userID = leaderboardEntry_t2.m_steamIDUser.ToString();
			highscoreEntry.rank = leaderboardEntry_t2.m_nGlobalRank;
			highscoreEntry.sendChallenge = false;
			highscoreEntry.localPlayerHasBeatenThisScore = false;
			if ((bool)level && num < leaderboardEntry_t2.m_nScore)
			{
				highscoreEntry.localPlayerHasBeatenThisScore = true;
				if (level != null && level.levelType != LevelType.Workshop && HenkUtils.IsInALevel())
				{
					highscoreEntry.sendChallenge = true;
				}
			}
			if (extraData != null && extraData[j] != null)
			{
				if (extraData[j].Length != 0)
				{
					highscoreEntry.levelsBeaten = extraData[j][0];
				}
				if (extraData[j].Length > 1)
				{
					highscoreEntry.totalTime = extraData[j][1];
				}
			}
			currentLeaderboardEntries.Add(highscoreEntry);
		}
	}

	public void InitHighscoreManager()
	{
		if (initialized)
		{
			return;
		}
		Singleton<InternetManager>.SP.RegisterObject(base.gameObject);
		switch (Singleton<ActionHenk>.SP.database)
		{
		case ActionHenk.Database.RageSquid:
			Singleton<RageSquidServer>.SP.InitDatabase();
			break;
		case ActionHenk.Database.Steamworks:
			Singleton<HenkSWNotifications>.SP.Initialize();
			if (Singleton<HenkSWLeaderboards>.SP.Initialize() && Singleton<HenkSWUserStats>.SP.Initialize())
			{
				Singleton<HenkSWUserStats>.SP.RefreshUserStats();
				Singleton<IRCManager>.SP.UpdateNickname(string.Empty, 0uL);
			}
			break;
		}
		initialized = true;
	}

	private void OnInternetChecked(bool internet)
	{
	}

	public void SubmitScore(float finishTime)
	{
		SubmitScoreLocal();
		currentLevelSubmitting = Singleton<LevelBatchManager>.SP.GetCurrentLevel();
		Singleton<HenkSWLeaderboards>.SP.highscoreSubmitStatus = Language.Get("SUBMITTINGDATA", "GAME");
		switch (Singleton<ActionHenk>.SP.database)
		{
		case ActionHenk.Database.RageSquid:
			if (!Singleton<RageSquidServer>.SP.SubmitScore())
			{
				FailedToConnect();
			}
			break;
		case ActionHenk.Database.Steamworks:
			submittingScore = true;
			if (!Singleton<HenkSWLeaderboards>.SP.SubmitScore())
			{
				MonoBehaviour.print("failed submitting score");
				FailedToConnect();
			}
			break;
		case ActionHenk.Database.None:
		case ActionHenk.Database.Playstation:
			break;
		}
	}

	public void SubmitScoreLocal()
	{
		newLocalBest = false;
		if (IsBetterThanLocalHighscore(Singleton<CheckpointManager>.SP.GetFinishTime()))
		{
			Singleton<ReplayRecorder>.SP.SaveReplayLocal();
			Singleton<PlayerPrefsManager>.SP.SetHighscore(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj(), Singleton<CheckpointManager>.SP.GetFinishTime());
			newLocalBest = true;
		}
	}

	public void HenkSWScoreUploaded(bool newHighScore)
	{
		gotNewHighscore = newHighScore;
		if (submittingScore && newHighScore)
		{
			Debug.Log("Score uploaded, new highscore, save replay to steam.");
			Singleton<ReplayRecorder>.SP.SaveReplayToSteam();
			Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().steamScore = -1;
			totalScorePosted = false;
		}
		else if (submittingScore && !newHighScore)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("DoneUploadingHighscores", true, SendMessageOptions.DontRequireReceiver);
		}
		submittingScore = false;
	}

	public void DoneUploadingHighscores(bool success)
	{
		if (success)
		{
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
		}
	}

	public void LeaderboardEntriesDownloaded(ELeaderboardDataRequest requestType)
	{
		if (currentLevelSubmitting > 1 && requestType == ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends)
		{
			Singleton<GAManager>.SP.SubmitFriendLeaderboardLength(Singleton<LevelBatchManager>.SP.GetCurrentLevel(), Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard.Length);
		}
	}

	public void SendChallenge()
	{
		List<string> list = new List<string>();
		int num = 1000000000;
		for (int i = 0; i < Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard.Length; i++)
		{
			if (Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_steamIDUser == Singleton<HenkSWUserStats>.SP.GetSteamID())
			{
				num = Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_nScore;
				break;
			}
		}
		for (int j = 0; j < currentLeaderboardEntries.Count; j++)
		{
			HighscoreEntry highscoreEntry = currentLeaderboardEntries[j];
			if (highscoreEntry.userID != Singleton<HenkSWUserStats>.SP.GetSteamID().ToString())
			{
				int iScore = highscoreEntry.iScore;
				if (num < iScore && highscoreEntry.sendChallenge)
				{
					ulong num2 = ulong.Parse(highscoreEntry.userID);
					string nameBySteamID = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(num2);
					list.Add(highscoreEntry.userID);
					MonoBehaviour.print("Sending level " + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + " challenge to " + nameBySteamID);
					Singleton<HenkSWNotifications>.SP.BeatPlayer(num2, nameBySteamID, num, Singleton<LevelBatchManager>.SP.GetCurrentLevel());
				}
			}
		}
	}

	public void FailedToConnect()
	{
		submittingScore = false;
		PostGameLeaderboard.FillInScoreLabelsNotConnected();
		Singleton<GetRoot>.SP.Get().BroadcastMessage("DoneUploadingHighscores", false, SendMessageOptions.DontRequireReceiver);
	}

	public void HenkSWSubmitScoreFailed()
	{
		FailedToConnect();
	}

	public bool IsBetterThanLocalHighscore(float score, Level level = null)
	{
		if (level == null)
		{
			level = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		}
		float highscore = Singleton<PlayerPrefsManager>.SP.GetHighscore(level);
		if (score < highscore || highscore == 0f)
		{
			return true;
		}
		return false;
	}

	private IEnumerator TimeOutCheck()
	{
		timedOut = false;
		yield return new WaitForSeconds(8f);
		FailedToConnect();
		timedOut = true;
	}

	public string ConvertTimeIntToString(int timeInt, bool showHundreds = true)
	{
		float time = HenkSWLeaderboards.ScoreIntToFloat(timeInt);
		return ConvertTimeToString(time, showHundreds);
	}

	public string ConvertTimeToString(float time, bool showHundreds = true)
	{
		string empty = string.Empty;
		float num = Mathf.FloorToInt(time / 60f);
		float num2 = (float)Mathf.FloorToInt(time) - num * 60f;
		float num3 = Mathf.RoundToInt((time - Mathf.Floor(time)) * 100f);
		if (showHundreds)
			return string.Format("{0:00}:{1:00}.{2:00}", num, num2, num3);
		return string.Format("{0:00}:{1:00}", num, num2);
	}

	private List<Level> cachedCampaignLevels;

	public void Update()
	{
		if (!SteamManager.Initialized || totalScorePosted || !(Singleton<HenkSWLeaderboards>.SP.retrievingScoreForLevel == null) || HenkUtils.IsInALevel())
		{
			return;
		}
		if (cachedCampaignLevels == null)
			cachedCampaignLevels = Singleton<LevelBatchManager>.SP.GetCampaignLevels();
		bool flag = true;
		int num = 0;
		int num2 = 0;
		foreach (Level item in cachedCampaignLevels)
		{
			if (item.steamScore == -1)
			{
				flag = false;
				Singleton<HenkSWLeaderboards>.SP.RetrieveScoreForLevel(item);
				break;
			}
			if (item.steamScore > 0)
			{
				num++;
				num2 += item.steamScore;
			}
		}
		if (flag)
		{
			MonoBehaviour.print("Total levels beaten: " + num + " Total time (ms): " + num2);
			int newScore = num * 1000000 - num2;
			int[] scoreCustomDetails = new int[2] { num, num2 };
			if (Singleton<HenkSWLeaderboards>.SP.SetCurrentLeaderboardHandle("global_overall_2"))
			{
				Singleton<HenkSWLeaderboards>.SP.SubmitCustomScore(newScore, scoreCustomDetails, keepBest: false);
			}
			totalScorePosted = true;
		}
	}

	public void UpdateTotalScore()
	{
		List<Level> campaignLevels = Singleton<LevelBatchManager>.SP.GetCampaignLevels();
		int num = 0;
		float num2 = 0f;
		foreach (Level item in campaignLevels)
		{
			float highscore = Singleton<PlayerPrefsManager>.SP.GetHighscore(item);
			if (highscore != 0f)
			{
				num++;
				num2 += highscore;
			}
		}
		int num3 = Mathf.FloorToInt(num2 * 1000f);
		int num4 = num * 1000000 - num3;
		if (num4 > totalScore)
		{
			totalScore = num4;
			int[] scoreCustomDetails = new int[2] { num, num3 };
			if (Singleton<HenkSWLeaderboards>.SP.SetCurrentLeaderboardHandle("global_overall_2"))
			{
				Singleton<HenkSWLeaderboards>.SP.SubmitCustomScore(num4, scoreCustomDetails);
			}
		}
		MonoBehaviour.print("Levels beaten: " + num + " Total time: " + num2 + " Total score: " + totalScore);
	}

	public int NumberOfChallengesSelected()
	{
		int num = 0;
		foreach (HighscoreEntry currentLeaderboardEntry in currentLeaderboardEntries)
		{
			if (currentLeaderboardEntry.sendChallenge)
			{
				num++;
			}
		}
		return num;
	}
}
