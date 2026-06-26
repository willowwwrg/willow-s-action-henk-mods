using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class GUI_LevelSelectLeaderboards : GUI_Base
{
	public HighscoreList highscoreList;

	public Level selectedLevel;

	private bool receivedLeaderboardEntries;

	private HighscoreState highscoreListState;

	private bool leaderboardsGlobalReceived;

	private bool leaderboardsGlobalNearUserReceived;

	private bool leaderboardsFriendsReceived;

	public UILabel levelNameLabel;

	public UILabel leaderboardTypeLabel;

	public UILabel playersBeatenLabel;

	public InputObject firstButton;

	private int friendScoresOffset;

	private int globalPageNum;

	private int globalNearUserPageNum;

	public List<GameObject> scoreButtons;

	public UISprite topArrow;

	public UISprite botArrow;

	private bool waitingForReplayDownload;

	public GameObject continueButton;

	private void PrevWindow()
	{
		AudioController.Play("ButtonBackwards");
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_LevelSelectCampaign)))
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelSelectCampaign, "none");
		}
		else
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		}
	}

	private void NextWindow()
	{
		if (receivedLeaderboardEntries)
		{
			Singleton<InputManager>.SP.ClickCurrentButton();
		}
	}

	private void UpdateScrollingPages()
	{
		if (!receivedLeaderboardEntries)
		{
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			return;
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "bg10_l")
		{
			if (highscoreListState == HighscoreState.Friends)
			{
				int num = Singleton<HighscoreManager>.SP.currentLeaderboardEntries.Count - 10;
				if (friendScoresOffset < num)
				{
					friendScoresOffset++;
					StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.Friends));
					Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true);
				}
			}
			else if (highscoreListState == HighscoreState.GlobalNearUser)
			{
				globalNearUserPageNum++;
				GetNextTenLeaderboardItems(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, globalNearUserPageNum);
				Singleton<InputManager>.SP.Select(scoreButtons[9].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
			}
			else if (highscoreListState == HighscoreState.Global)
			{
				globalPageNum++;
				GetNextTenLeaderboardItems(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, globalPageNum);
				Singleton<InputManager>.SP.Select(scoreButtons[9].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
			}
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Up) && Singleton<InputManager>.SP.GetCurrentButton().name == "bg1_d")
		{
			if (highscoreListState == HighscoreState.Friends)
			{
				if (friendScoresOffset > 0)
				{
					friendScoresOffset--;
					StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.Friends));
					Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true);
				}
			}
			else if (highscoreListState == HighscoreState.GlobalNearUser)
			{
				if (Singleton<HenkSWLeaderboards>.SP.fetchedFirstPlayer)
				{
					Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
					return;
				}
				globalNearUserPageNum--;
				GetNextTenLeaderboardItems(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, globalNearUserPageNum);
				Singleton<InputManager>.SP.Select(scoreButtons[0].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
			}
			else if (highscoreListState == HighscoreState.Global)
			{
				globalPageNum--;
				if (globalPageNum < 0)
				{
					globalPageNum = 0;
					return;
				}
				GetNextTenLeaderboardItems(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, globalPageNum);
				Singleton<InputManager>.SP.Select(scoreButtons[0].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
			}
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
		}
		if (highscoreListState == HighscoreState.Friends)
		{
			if (friendScoresOffset > 0)
			{
				topArrow.enabled = true;
			}
			else
			{
				topArrow.enabled = false;
			}
			if (friendScoresOffset > Singleton<HighscoreManager>.SP.currentLeaderboardEntries.Count - 11)
			{
				botArrow.enabled = false;
			}
			else
			{
				botArrow.enabled = true;
			}
		}
		else if (highscoreListState == HighscoreState.GlobalNearUser)
		{
			if (Singleton<HenkSWLeaderboards>.SP.fetchedFirstPlayer)
			{
				topArrow.enabled = false;
			}
			else
			{
				topArrow.enabled = true;
			}
			botArrow.enabled = true;
		}
		else if (highscoreListState == HighscoreState.Global)
		{
			if (globalPageNum < 1)
			{
				topArrow.enabled = false;
			}
			else
			{
				topArrow.enabled = true;
			}
			botArrow.enabled = true;
		}
	}

	public void GetNextTenLeaderboardItems(ELeaderboardDataRequest requestType, int pageNum = 0)
	{
		Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(requestType, onlyGetWorldChamp: false, onlyGetSelf: false, pageNum);
		receivedLeaderboardEntries = false;
		highscoreList.StartLoadingScores();
	}

	public void Button_HighscoreEntry(object value)
	{
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_LevelSelectCampaign)))
		{
			bool flag = false;
			if (HenkUtils.IsDev(Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID))
			{
				flag = true;
			}
			HighscoreEntry scoreEntryForButton = GetScoreEntryForButton(value);
			if (scoreEntryForButton != null && scoreEntryForButton.userID == Singleton<HenkSWUserStats>.SP.GetSteamID().ToString())
			{
				flag = true;
			}
			if (!flag && (selectedLevel.levelType == LevelType.Challenge || selectedLevel.levelType == LevelType.Bonus))
			{
				AudioController.Play("ButtonClick");
			}
			else
			{
				StartCoroutine(StartReplaymodeDownload(value));
			}
		}
	}

	public void SpecificLeaderboardEntriesDownloaded()
	{
		waitingForReplayDownload = false;
	}

	private IEnumerator StartReplaymodeDownload(object value)
	{
		int rank = 0;
		int levelCode = selectedLevel.levelCode;
		for (int i = 0; i < scoreButtons.Count; i++)
		{
			if (scoreButtons[i] == value as GameObject)
			{
				rank = ((highscoreListState != HighscoreState.Friends) ? Singleton<HighscoreManager>.SP.currentLeaderboardEntries[i].rank : Singleton<HighscoreManager>.SP.currentLeaderboardEntries[i + friendScoresOffset].rank);
			}
		}
		AudioController.Play("LevelStart");
		MonoBehaviour.print("downloading replay");
		waitingForReplayDownload = true;
		Singleton<HenkSWLeaderboards>.SP.DownloadSpecificLeaderboardEntry(rank);
		while (waitingForReplayDownload)
		{
			yield return new WaitForEndOfFrame();
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Replay);
		Object.FindObjectOfType<State_ReplayMode>().StartReplay(levelCode);
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		playersBeatenLabel.text = string.Empty;
		BroadcastMessage("Reset", SendMessageOptions.DontRequireReceiver);
		Singleton<HenkSWLeaderboards>.SP.highscoreSubmitStatus = Language.Get("FETCHING", "LEADERBOARD");
		highscoreList.StartLoadingScores();
		if (!SteamManager.Initialized)
		{
			highscoreList.FillInScoreListNotConnected();
			levelNameLabel.text = Language.Get("LEADERBOARD", "LEADERBOARD");
			receivedLeaderboardEntries = true;
			return;
		}
		friendScoresOffset = 0;
		globalPageNum = 0;
		globalNearUserPageNum = 0;
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		highscoreListState = HighscoreState.Friends;
		leaderboardsGlobalReceived = false;
		leaderboardsGlobalNearUserReceived = false;
		leaderboardsFriendsReceived = false;
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_LevelSelectCampaign)))
		{
			if (selectedLevel == null)
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_DOWNLOADINGLEADERBOARDDATA", "PERMA"));
				return;
			}
			Singleton<HenkSWLeaderboards>.SP.SetLeaderboardForLevel(selectedLevel);
			levelNameLabel.text = selectedLevel.levelName;
			continueButton.SetActive(value: true);
		}
		else if (Singleton<HenkSWLeaderboards>.SP.SetCurrentLeaderboardHandle("global_overall_2"))
		{
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
			levelNameLabel.text = Language.Get("OVERALLLEADERBOARD", "LEADERBOARD");
			continueButton.SetActive(value: false);
		}
		else
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_DOWNLOADINGLEADERBOARDDATA", "PERMA"));
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.NextLeaderboard))
		{
			NextLeaderboard(forwards: true);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.PrevLeaderboard))
		{
			NextLeaderboard(forwards: false);
		}
		UpdateScrollingPages();
	}

	public void OnLeaderboardHandleFound()
	{
		receivedLeaderboardEntries = false;
		Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
	}

	public void LeaderboardEntriesDownloaded(ELeaderboardDataRequest requestType)
	{
		receivedLeaderboardEntries = true;
		HighscoreState highscoreState;
		switch (requestType)
		{
		case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser:
			leaderboardsGlobalNearUserReceived = true;
			highscoreState = HighscoreState.GlobalNearUser;
			break;
		case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal:
			leaderboardsGlobalReceived = true;
			highscoreState = HighscoreState.Global;
			break;
		default:
			leaderboardsFriendsReceived = true;
			highscoreState = HighscoreState.Friends;
			break;
		}
		int num = -1;
		for (int i = 0; i < Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard.Length; i++)
		{
			if (Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_steamIDUser == Singleton<HenkSWUserStats>.SP.GetSteamID())
			{
				num = Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_nGlobalRank;
			}
		}
		int numberOfLeaderboardEntriesForCurrentLeaderboard = Singleton<HenkSWLeaderboards>.SP.GetNumberOfLeaderboardEntriesForCurrentLeaderboard();
		if (numberOfLeaderboardEntriesForCurrentLeaderboard > 1 && num != -1)
		{
			float num2 = ((float)num - 1f) / ((float)numberOfLeaderboardEntriesForCurrentLeaderboard - 1f);
			int num3 = Mathf.FloorToInt(100f - num2 * 100f);
			playersBeatenLabel.text = Language.Get("PERCENTAGEBEATEN", "LEADERBOARD").Replace("{X}", num3.ToString()) + " [" + Language.Get("RANK", "LEADERBOARD") + " " + num + "/" + Singleton<HenkSWLeaderboards>.SP.GetNumberOfLeaderboardEntriesForCurrentLeaderboard() + "]";
		}
		StartCoroutine(FillInScoreLabelsSteamworks(highscoreState));
	}

	public void NextLeaderboard(bool forwards)
	{
		if (!receivedLeaderboardEntries)
		{
			return;
		}
		if (forwards)
		{
			highscoreListState = (HighscoreState)((int)(highscoreListState + 1) % 3);
		}
		else
		{
			highscoreListState--;
			if (highscoreListState < HighscoreState.Friends)
			{
				highscoreListState = HighscoreState.Global;
			}
		}
		highscoreList.StartLoadingScores();
		bool flag = Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_Leaderboards));
		if (highscoreListState == HighscoreState.Global)
		{
			if (leaderboardsGlobalReceived && !flag)
			{
				Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(HighscoreState.Global), Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
				StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.Global));
				return;
			}
			ELeaderboardDataRequest requestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(requestType, onlyGetWorldChamp: false, onlyGetSelf: false, globalPageNum);
		}
		else if (highscoreListState == HighscoreState.GlobalNearUser)
		{
			if (leaderboardsGlobalNearUserReceived && !flag)
			{
				Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(HighscoreState.GlobalNearUser), Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
				StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.GlobalNearUser));
				return;
			}
			ELeaderboardDataRequest requestType2 = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(requestType2, onlyGetWorldChamp: false, onlyGetSelf: false, globalNearUserPageNum);
		}
		else
		{
			if (leaderboardsFriendsReceived && !flag)
			{
				Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(HighscoreState.Friends), Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
				StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.Friends));
				return;
			}
			ELeaderboardDataRequest requestType3 = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(requestType3);
		}
		receivedLeaderboardEntries = false;
	}

	private IEnumerator FillInScoreLabelsSteamworks(HighscoreState highscoreState)
	{
		highscoreListState = highscoreState;
		SetBGPanelSubTitle(highscoreListState);
		LeaderboardEntry_t[] scoreList = Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(highscoreState);
		Singleton<HenkSWUserStats>.SP.waitForNameCallbacks = 0;
		LeaderboardEntry_t[] array = scoreList;
		for (int i = 0; i < array.Length; i++)
		{
			LeaderboardEntry_t leaderboardEntry_t = array[i];
			Singleton<HenkSWUserStats>.SP.RequestNameBySteamID(leaderboardEntry_t.m_steamIDUser);
		}
		while (Singleton<HenkSWUserStats>.SP.waitForNameCallbacks > 0)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForEndOfFrame();
		switch (highscoreState)
		{
		case HighscoreState.Friends:
		{
			if (scoreList.Length == 0)
			{
				NextLeaderboard(forwards: false);
				break;
			}
			for (int j = 0; j < scoreButtons.Count; j++)
			{
				GUIHighscoreEntry component = scoreButtons[j].GetComponent<GUIHighscoreEntry>();
				int num = j + friendScoresOffset;
				if (num < Singleton<HighscoreManager>.SP.currentLeaderboardEntries.Count)
				{
					component.highscoreEntry = Singleton<HighscoreManager>.SP.currentLeaderboardEntries[num];
				}
				else
				{
					component.highscoreEntry = null;
				}
			}
			highscoreList.FillInScoreList(friendScoresOffset);
			break;
		}
		case HighscoreState.Global:
			highscoreList.FillInScoreList();
			break;
		case HighscoreState.GlobalNearUser:
			if (scoreList.Length == 0)
			{
				NextLeaderboard(forwards: true);
			}
			else
			{
				highscoreList.FillInScoreList();
			}
			break;
		default:
			Debug.LogError("Trying to fill in list with nonexisting highscorestate: " + highscoreState);
			break;
		}
	}

	private void SetBGPanelSubTitle(HighscoreState state)
	{
		switch (state)
		{
		case HighscoreState.Friends:
			leaderboardTypeLabel.text = Language.Get("FRIENDS", "LEADERBOARD");
			break;
		case HighscoreState.Global:
			leaderboardTypeLabel.text = Language.Get("TOPTEN", "LEADERBOARD");
			break;
		case HighscoreState.GlobalNearUser:
			leaderboardTypeLabel.text = Language.Get("GLOBAL", "LEADERBOARD");
			break;
		default:
			Debug.LogError("Trying to set bg panel subtitle in list with nonexisting highscorestate: " + state);
			break;
		}
	}

	public HighscoreEntry GetScoreEntryForButton(object button)
	{
		for (int i = 0; i < scoreButtons.Count; i++)
		{
			if (scoreButtons[i] == button as GameObject)
			{
				if (highscoreListState == HighscoreState.Friends)
				{
					return Singleton<HighscoreManager>.SP.currentLeaderboardEntries[i + friendScoresOffset];
				}
				return Singleton<HighscoreManager>.SP.currentLeaderboardEntries[i];
			}
		}
		return null;
	}
}
