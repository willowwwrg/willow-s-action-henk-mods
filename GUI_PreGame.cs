using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class GUI_PreGame : GUI_Base
{
	public enum GhostSelectionState
	{
		None,
		Main,
		Medal,
		FetchFriends,
		Friend,
		Challenge,
		WIP,
		Workshop,
		Bonus,
		ChallengeOrBonusGhostSelect
	}

	public State_PreGame stateObject;

	public InputObject firstButton;

	public InputObject mainStateButton;

	public List<InputObject> ghostButtons;

	public GameObject ghostSelectionMenu;

	private bool initializingGhost;

	private bool waitingForFriendsScores;

	private bool friendsFetched;

	private bool waitingForWorld1;

	private bool world1Fetched;

	private bool waitingForPersonalBest;

	private bool personalBestFetched;

	private List<LeaderboardEntry_t> friendsScores = new List<LeaderboardEntry_t>();

	public UILabel downloadingLabel;

	public UILabel charNameLabel;

	public UILabel timeToBeatLabel;

	public List<UITweener> challengeBonusTweens;

	private int friendListOffset;

	private ulong forceFriendDownloadID;

	public GameObject arrows;

	private static bool IsFullGameMode()
	{
		return DevCommands.IsFullGameMode();
	}

	private GhostSelectionState selectionState = GhostSelectionState.Main;

	private void TransitionCompleted()
	{
		InitializeScreen(0f);
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.None)
		{
			Singleton<PermaGUI>.SP.ToggleIngameTutorial(state: true);
		}
		friendsFetched = false;
		personalBestFetched = false;
		world1Fetched = false;
		downloadingLabel.transform.parent.gameObject.SetActive(value: false);
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
	}

	private void Update()
	{
		if (selectionState == GhostSelectionState.Friend)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_Ghost_8")
			{
				int num = friendsScores.Count - 8;
				if (friendListOffset < num)
				{
					friendListOffset++;
					SetFriendLabels();
				}
				Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true, playSound: false);
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Up) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_Ghost_1")
			{
				if (friendListOffset > 0)
				{
					friendListOffset--;
					SetFriendLabels();
				}
				Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true, playSound: false);
			}
		}
		if (!Singleton<InputManager>.SP.CheckAction(InputAction.Cancel) || initializingGhost)
		{
			return;
		}
		if (selectionState == GhostSelectionState.Main || selectionState == GhostSelectionState.WIP)
		{
			Button_NoReplay();
		}
		else if (selectionState == GhostSelectionState.Challenge)
		{
			NextWindow();
		}
		else if (selectionState == GhostSelectionState.FetchFriends)
		{
			NextWindow();
		}
		else if (selectionState == GhostSelectionState.Friend)
		{
			if (!stateObject.countingDown)
			{
				if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().wipState == WIPState.Draft)
				{
					SetGhostSelectionState(GhostSelectionState.WIP);
				}
				else
				{
					SetGhostSelectionState(GhostSelectionState.Main);
				}
				AudioController.Play("ButtonBackwards");
			}
		}
		else if (selectionState == GhostSelectionState.Workshop)
		{
			Button_NoReplay();
		}
		else if (selectionState == GhostSelectionState.Bonus)
		{
			NextWindow();
		}
		else if (selectionState == GhostSelectionState.ChallengeOrBonusGhostSelect)
		{
			if (!stateObject.countingDown)
			{
				if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
				{
					SetGhostSelectionState(GhostSelectionState.Challenge);
				}
				else
				{
					SetGhostSelectionState(GhostSelectionState.Bonus);
				}
				AudioController.Play("ButtonBackwards");
			}
		}
		else if (!stateObject.countingDown)
		{
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelCode == 97)
			{
				if (IsFullGameMode())
					Button_NoReplayChallengeOrBonus();
				else
					SetGhostSelectionState(GhostSelectionState.ChallengeOrBonusGhostSelect);
			}
			else
				SetGhostSelectionState(GhostSelectionState.Main);
			AudioController.Play("ButtonBackwards");
		}
	}

	public void InitializeScreen()
	{
		initializingGhost = false;
		if (Singleton<PlayerManager>.SP.ghostSet)
		{
			SetGhostSelectionState(GhostSelectionState.None);
		}
		else if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelCode == 97)
		{
			if (IsFullGameMode())
				Button_NoReplayChallengeOrBonus();
			else
				SetGhostSelectionState(GhostSelectionState.ChallengeOrBonusGhostSelect);
		}
		else if (!Singleton<PlayerManager>.SP.ghostSet)
		{
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
			{
				SetGhostSelectionState(GhostSelectionState.Challenge);
			}
			else if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().wipState == WIPState.Draft)
			{
				SetGhostSelectionState(GhostSelectionState.WIP);
			}
			else if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop || Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
			{
				SetGhostSelectionState(GhostSelectionState.Workshop);
			}
			else if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus)
			{
				SetGhostSelectionState(GhostSelectionState.Bonus);
			}
			else
			{
				SetGhostSelectionState(GhostSelectionState.Main);
			}
		}
		else
		{
			SetGhostSelectionState(GhostSelectionState.None);
		}
	}

	public void ToggleGhostSelectionMenu(bool toggle)
	{
		ghostSelectionMenu.SetActive(toggle);
		if (toggle)
		{
			Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
		}
	}

	private void ToggleGhostSelectionButton(int buttonNum, bool toggle, bool toggleAll = false)
	{
		if (toggleAll)
		{
			foreach (InputObject ghostButton in ghostButtons)
			{
				ghostButton.gameObject.SetActive(toggle);
			}
			return;
		}
		if (buttonNum < ghostButtons.Count)
		{
			ghostButtons[buttonNum].gameObject.SetActive(toggle);
		}
	}

	private void SetGhostButtonInfo(int buttonNum, string text, string functionName, float timeLabelText, bool showMedal = true, bool toggleButtonOn = true)
	{
		if (buttonNum < ghostButtons.Count)
		{
			ToggleGhostSelectionButton(buttonNum, toggleButtonOn);
			ghostButtons[buttonNum].GetComponentInChildren<UILabel>().text = text;
			ghostButtons[buttonNum].GetComponent<UIButtonMessage>().functionName = functionName;
			HenkUtils.FindTransformInHierarchy(ghostButtons[buttonNum].transform, "timeLabel").GetComponent<UILabel>().text = ((timeLabelText != 0f) ? Singleton<HighscoreManager>.SP.ConvertTimeToString(timeLabelText) : string.Empty);
			Transform transform = HenkUtils.FindTransformInHierarchy(ghostButtons[buttonNum].transform, "medal");
			if ((bool)transform)
			{
				transform.GetComponent<UISprite>().enabled = showMedal;
			}
		}
	}

	public void SetGhostSelectionState(GhostSelectionState state)
	{
		arrows.SetActive(value: false);
		selectionState = state;
		switch (state)
		{
		case GhostSelectionState.None:
			ToggleGhostSelectionMenu(toggle: false);
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
			challengeBonusTweens[2].gameObject.SetActive(value: false);
			break;
		case GhostSelectionState.Main:
			ToggleGhostSelectionButton(0, toggle: false, toggleAll: true);
			ToggleGhostSelectionMenu(toggle: true);
			SetGhostButtonInfo(0, Language.Get("BRONZE", "GENERIC"), "Button_Medal", Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().bronzeTime);
			SetGhostButtonInfo(1, Language.Get("SILVER", "GENERIC"), "Button_Medal", Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().silverTime);
			SetGhostButtonInfo(2, Language.Get("GOLD", "GENERIC"), "Button_Medal", Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().goldTime);
			downloadingLabel.transform.parent.gameObject.SetActive(value: false);
			Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().bestMedal > Medal.Silver)
			{
				SetGhostButtonInfo(3, Language.Get("RAINBOW", "GENERIC"), "Button_Medal", Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().rainbowTime);
				SetGhostButtonInfo(4, Language.Get("FRIENDS", "LEADERBOARD"), "Button_Friends", 0f, showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[4].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEFRIENDS", "GAME");
				SetGhostButtonInfo(5, Language.Get("WORLDCHAMP", "LEADERBOARD"), "Button_World1Replay", 0f, showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[5].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEWORLDCHAMPION", "GAME");
				SetGhostButtonInfo(6, Language.Get("PERSONALBEST", "LEADERBOARD"), "Button_PersonalBestReplay", GetCurrentLevelHighscore(), showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[6].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEPERSONALBEST", "GAME");
				SetGhostButtonInfo(7, Language.Get("NOOPPONENT", "LEADERBOARD"), "Button_NoReplay", 0f, showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[7].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACENOOPPONENT", "GAME");
			}
			else
			{
				SetGhostButtonInfo(3, Language.Get("FRIENDS", "LEADERBOARD"), "Button_Friends", 0f, showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[3].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEFRIENDS", "GAME");
				SetGhostButtonInfo(4, Language.Get("PERSONALBEST", "LEADERBOARD"), "Button_PersonalBestReplay", GetCurrentLevelHighscore(), showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[4].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEPERSONALBEST", "GAME");
				SetGhostButtonInfo(5, Language.Get("NOOPPONENT", "LEADERBOARD"), "Button_NoReplay", 0f, showMedal: false);
				HenkUtils.FindTransformInHierarchy(ghostButtons[5].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACENOOPPONENT", "GAME");
			}
			FixInputObjectsForGhostButtons();
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: true, string.Empty);
			Singleton<MainMenuHighlighter>.SP.SetAsTitle(Language.Get("PICKOPPONENT", "LEADERBOARD"));
			if (mainStateButton != null)
			{
				if (mainStateButton.gameObject.activeSelf)
				{
					Singleton<InputManager>.SP.Select(mainStateButton, delayedTillEndOfFrame: true);
				}
				else
				{
					Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
				}
			}
			else
			{
				Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
			}
			break;
		case GhostSelectionState.FetchFriends:
			ToggleGhostSelectionButton(0, toggle: false, toggleAll: true);
			SetGhostButtonInfo(0, Language.Get("FETCHING", "LEADERBOARD"), "Button_Empty", 0f, showMedal: false);
			break;
		case GhostSelectionState.Friend:
		{
			ToggleGhostSelectionButton(0, toggle: false, toggleAll: true);
			ToggleGhostSelectionMenu(toggle: true);
			friendListOffset = 0;
			friendsScores.Clear();
			for (int i = 0; i < Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard.Length; i++)
			{
				if (Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_steamIDUser != Singleton<HenkSWUserStats>.SP.GetSteamID())
				{
					friendsScores.Add(Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i]);
				}
			}
			if (friendsScores.Count == 0)
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOFRIENDS", "PERMA"));
				SetGhostSelectionState(GhostSelectionState.Main);
			}
			else
			{
				SetFriendLabels();
			}
			FixInputObjectsForGhostButtons();
			Singleton<InputManager>.SP.Select(firstButton);
			break;
		}
		case GhostSelectionState.WIP:
			ToggleGhostSelectionButton(0, toggle: false, toggleAll: true);
			ToggleGhostSelectionMenu(toggle: true);
			SetGhostButtonInfo(0, "Personal best", "Button_PersonalBestReplay", GetCurrentLevelHighscore(), showMedal: false);
			SetGhostButtonInfo(1, "Friends", "Button_Friends", 0f, showMedal: false);
			SetGhostButtonInfo(2, "World Champion", "Button_World1Replay", 0f, showMedal: false);
			SetGhostButtonInfo(3, "No opponent", "Button_NoReplay", 0f, showMedal: false);
			FixInputObjectsForGhostButtons();
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: true, string.Empty);
			Singleton<MainMenuHighlighter>.SP.SetAsTitle("Pick opponent!");
			if (mainStateButton != null)
			{
				if (mainStateButton.gameObject.activeSelf)
				{
					Singleton<InputManager>.SP.Select(mainStateButton, delayedTillEndOfFrame: true);
				}
				else
				{
					Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
				}
			}
			else
			{
				Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
			}
			break;
		case GhostSelectionState.Challenge:
		{
			foreach (UITweener challengeBonusTween in challengeBonusTweens)
			{
				challengeBonusTween.gameObject.SetActive(value: true);
				challengeBonusTween.ResetToBeginning();
				challengeBonusTween.Play(forward: true);
			}
			Level currentLevelObj2 = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
			charNameLabel.text = Singleton<UnlockManager>.SP.GetCharacterName(currentLevelObj2.challenger, currentLevelObj2.challengerSkinNum);
			timeToBeatLabel.text = Language.Get("TIMETOBEAT", "LEADERBOARD") + " " + Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj2.bronzeTime);
			ToggleGhostSelectionMenu(toggle: false);
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
			break;
		}
		case GhostSelectionState.Bonus:
		{
			foreach (UITweener challengeBonusTween2 in challengeBonusTweens)
			{
				challengeBonusTween2.gameObject.SetActive(value: true);
				challengeBonusTween2.ResetToBeginning();
				challengeBonusTween2.Play(forward: true);
			}
			Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
			charNameLabel.text = Singleton<UnlockManager>.SP.GetCharacterName(currentLevelObj.challenger, currentLevelObj.challengerSkinNum);
			timeToBeatLabel.text = "TIME TO BEAT: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.bonusTime);
			ToggleGhostSelectionMenu(toggle: false);
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
			break;
		}
		case GhostSelectionState.ChallengeOrBonusGhostSelect:
			ToggleGhostSelectionButton(0, toggle: false, toggleAll: true);
			ToggleGhostSelectionMenu(toggle: true);
			SetGhostButtonInfo(0, Language.Get("PERSONALBEST", "LEADERBOARD"), "Button_PersonalBestReplay", GetCurrentLevelHighscore(), showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[0].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEPERSONALBEST", "GAME");
			SetGhostButtonInfo(1, Language.Get("WORLDCHAMP", "LEADERBOARD"), "Button_World1Replay", 0f, showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[1].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEWORLDCHAMPION", "GAME");
			SetGhostButtonInfo(2, Language.Get("FRIENDS", "LEADERBOARD"), "Button_Friends", 0f, showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[2].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEFRIENDS", "GAME");
			SetGhostButtonInfo(3, Language.Get("NOOPPONENT", "LEADERBOARD"), "Button_NoReplayChallengeOrBonus", 0f, showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[3].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACENOOPPONENT", "GAME");
			FixInputObjectsForGhostButtons();
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: true, string.Empty);
			Singleton<MainMenuHighlighter>.SP.SetAsTitle(Language.Get("PICKOPPONENT", "LEADERBOARD"));
			Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
			break;
		case GhostSelectionState.Workshop:
			ToggleGhostSelectionButton(0, toggle: false, toggleAll: true);
			ToggleGhostSelectionMenu(toggle: true);
			SetGhostButtonInfo(0, Language.Get("PERSONALBEST", "LEADERBOARD"), "Button_PersonalBestReplay", GetCurrentLevelHighscore(), showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[0].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEPERSONALBEST", "GAME");
			SetGhostButtonInfo(1, Language.Get("WORLDCHAMP", "LEADERBOARD"), "Button_World1Replay", 0f, showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[1].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACEWORLDCHAMPION", "GAME");
			SetGhostButtonInfo(2, Language.Get("NOOPPONENT", "LEADERBOARD"), "Button_NoReplay", 0f, showMedal: false);
			HenkUtils.FindTransformInHierarchy(ghostButtons[2].transform, "timeLabel").GetComponent<UILabel>().text = Language.Get("RACENOOPPONENT", "GAME");
			Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: true, string.Empty);
			Singleton<MainMenuHighlighter>.SP.SetAsTitle(Language.Get("PICKOPPONENT", "LEADERBOARD"));
			FixInputObjectsForGhostButtons();
			Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: true);
			break;
		default:
			SetGhostSelectionState(GhostSelectionState.None);
			break;
		}
		if (selectionState != GhostSelectionState.Bonus && selectionState != GhostSelectionState.Challenge && selectionState != GhostSelectionState.None && selectionState != GhostSelectionState.ChallengeOrBonusGhostSelect)
		{
			charNameLabel.transform.parent.gameObject.SetActive(value: false);
			challengeBonusTweens[2].gameObject.SetActive(value: true);
			challengeBonusTweens[2].ResetToBeginning();
			challengeBonusTweens[2].Play(forward: true);
		}
	}

	private void SetFriendLabels()
	{
		for (int i = 0; i < 8; i++)
		{
			int num = i + friendListOffset;
			if (num < friendsScores.Count)
			{
				SetGhostButtonInfo(i, Singleton<HenkSWUserStats>.SP.GetNameBySteamID(friendsScores[num].m_steamIDUser), "Button_Friend", HenkSWLeaderboards.ScoreIntToFloat(friendsScores[num].m_nScore), showMedal: false);
			}
			else
			{
				ToggleGhostSelectionButton(i, toggle: false);
			}
		}
		if (friendsScores.Count > 8)
		{
			arrows.SetActive(value: true);
		}
	}

	private void NextWindow()
	{
		if (!acceptInput || initializingGhost)
		{
			return;
		}
		if (selectionState == GhostSelectionState.Challenge || selectionState == GhostSelectionState.Bonus)
		{
			foreach (UITweener challengeBonusTween in challengeBonusTweens)
			{
				challengeBonusTween.gameObject.SetActive(value: false);
			}
			Singleton<PlayerManager>.SP.RemoveGhosts();
			Singleton<PlayerManager>.SP.ghostSet = false;
			if (IsFullGameMode())
			{
				Button_NoReplayChallengeOrBonus();
			}
			else
			{
				SetGhostSelectionState(GhostSelectionState.ChallengeOrBonusGhostSelect);
			}
		}
		else
		{
			if (!(Singleton<InputManager>.SP.GetCurrentButton() != null))
			{
				return;
			}
			Singleton<InputManager>.SP.ClickCurrentButton();
		}
		AudioController.Play("ButtonForwards");
	}

	private void PrevWindow()
	{
	}

	public void Button_NoReplay()
	{
		initializingGhost = true;
		Singleton<InputManager>.SP.Deselect();
		mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
		SetGhostSelectionState(GhostSelectionState.None);
		stateObject.SelectGhost(GhostType.None, 0uL);
	}

	public void Button_NoReplayChallengeOrBonus()
	{
		initializingGhost = true;
		Singleton<InputManager>.SP.Deselect();
		mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
		SetGhostSelectionState(GhostSelectionState.None);
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
		{
			stateObject.SelectGhost(GhostType.Challenger, 0uL);
		}
		else
		{
			stateObject.SelectGhost(GhostType.None, 0uL);
		}
	}

	public void Button_PersonalBestReplay()
	{
		if (!SteamManager.Initialized)
		{
			initializingGhost = true;
			Singleton<InputManager>.SP.Deselect();
			mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
			SetGhostSelectionState(GhostSelectionState.None);
			stateObject.SelectGhost(GhostType.PersonalBest, 0uL);
		}
		else
		{
			Button_PersonalBestReplaySteam();
		}
	}

	public void Button_PersonalBestReplaySteam()
	{
		initializingGhost = true;
		mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
		if (!Singleton<HenkSWLeaderboards>.SP.currentlyRefreshingScores)
		{
			if (personalBestFetched)
			{
				SetGhostSelectionState(GhostSelectionState.None);
				LoadPersonalBestAfterDownload();
				return;
			}
			waitingForPersonalBest = true;
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, onlyGetWorldChamp: false, onlyGetSelf: true);
			SetGhostSelectionState(GhostSelectionState.None);
			downloadingLabel.transform.parent.gameObject.SetActive(value: true);
			Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
		}
	}

	private void LoadPersonalBestAfterDownload()
	{
		if (Singleton<HenkSWLeaderboards>.SP.globalAroundUserLeaderboard.Length < 1)
		{
			initializingGhost = true;
			SetGhostSelectionState(GhostSelectionState.None);
			stateObject.SelectGhost(GhostType.PersonalBest, 0uL);
			Debug.LogWarning("No online personal best, trying local.");
		}
		else
		{
			stateObject.SelectGhost(GhostType.PersonalBestSteam, 0uL);
			Singleton<InputManager>.SP.Deselect();
		}
	}

	public void LoadReplayNum(int num)
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Can't select replay because steam is null!");
			return;
		}
		SetGhostSelectionState(GhostSelectionState.None);
		Singleton<InputManager>.SP.Deselect();
		Singleton<HenkSWLeaderboards>.SP.DownloadSpecificLeaderboardEntry(num);
	}

	public void Button_World1Replay()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Can't select friend because steam is null!");
			return;
		}
		initializingGhost = true;
		mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
		if (!Singleton<HenkSWLeaderboards>.SP.currentlyRefreshingScores)
		{
			if (world1Fetched)
			{
				SetGhostSelectionState(GhostSelectionState.None);
				LoadWorld1AfterDownload();
				return;
			}
			waitingForWorld1 = true;
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, onlyGetWorldChamp: true);
			SetGhostSelectionState(GhostSelectionState.None);
			downloadingLabel.transform.parent.gameObject.SetActive(value: true);
			Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
		}
	}

	private void LoadReplayNumAfterDownload()
	{
		stateObject.SelectGhost(GhostType.SpecificPlayer, 0uL);
		Singleton<InputManager>.SP.Deselect();
	}

	private void LoadWorld1AfterDownload()
	{
		if (Singleton<HenkSWLeaderboards>.SP.globalLeaderboard.Length < 1)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOWORLDONE", "PERMA"));
			Debug.LogError("No world #1 replay for this level.");
			SetGhostSelectionState(GhostSelectionState.None);
			stateObject.SelectGhost(GhostType.None, 0uL);
		}
		else
		{
			stateObject.SelectGhost(GhostType.World1, 0uL);
			Singleton<InputManager>.SP.Deselect();
		}
	}

	public void Button_Friends()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Can't select friend because steam is null!");
			return;
		}
		mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
		if (!Singleton<HenkSWLeaderboards>.SP.currentlyRefreshingScores)
		{
			if (friendsFetched)
			{
				SetGhostSelectionState(GhostSelectionState.Friend);
				return;
			}
			waitingForFriendsScores = true;
			Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
			SetGhostSelectionState(GhostSelectionState.FetchFriends);
		}
	}

	public void LeaderboardEntriesDownloaded()
	{
		if (waitingForFriendsScores)
		{
			waitingForFriendsScores = false;
			friendsFetched = true;
			if (forceFriendDownloadID != 0L)
			{
				stateObject.SelectGhost(GhostType.Friend, forceFriendDownloadID);
				forceFriendDownloadID = 0uL;
			}
			else
			{
				SetGhostSelectionState(GhostSelectionState.Friend);
			}
		}
		else if (waitingForPersonalBest)
		{
			waitingForPersonalBest = false;
			SetGhostSelectionState(GhostSelectionState.None);
			personalBestFetched = true;
			LoadPersonalBestAfterDownload();
		}
		else if (waitingForWorld1)
		{
			waitingForWorld1 = false;
			SetGhostSelectionState(GhostSelectionState.None);
			world1Fetched = true;
			LoadWorld1AfterDownload();
		}
	}

	public void SpecificLeaderboardEntriesDownloaded()
	{
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PreGame)))
		{
			LoadReplayNumAfterDownload();
		}
	}

	public void Button_Empty()
	{
	}

	public void Button_BestFriendReplay()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		initializingGhost = true;
		if (!Singleton<HenkSWLeaderboards>.SP.currentlyRefreshingScores)
		{
			if (Singleton<HenkSWLeaderboards>.SP.globalLeaderboard.Length < 1)
			{
				Debug.LogError("No highscore entries for this level.");
				SetGhostSelectionState(GhostSelectionState.None);
				stateObject.SelectGhost(GhostType.None, 0uL);
			}
			else
			{
				Singleton<InputManager>.SP.Deselect();
				SetGhostSelectionState(GhostSelectionState.None);
				stateObject.SelectGhost(GhostType.Friend, 0uL);
			}
		}
	}

	public void Button_FriendReplay()
	{
		if (SteamManager.Initialized)
		{
			Singleton<InputManager>.SP.Deselect();
			mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
			SetGhostSelectionState(GhostSelectionState.Friend);
		}
	}

	public void Button_Friend(object value)
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Can't select friend because steam is null!");
			SetGhostSelectionState(GhostSelectionState.None);
			stateObject.SelectGhost(GhostType.None, 0uL);
			return;
		}
		Singleton<InputManager>.SP.Deselect();
		initializingGhost = true;
		if (friendsScores.Count < 1)
		{
			Debug.LogError("No highscore entries for this level.");
			SetGhostSelectionState(GhostSelectionState.None);
			stateObject.SelectGhost(GhostType.None, 0uL);
			return;
		}
		SetGhostSelectionState(GhostSelectionState.None);
		int num = (value as GameObject).GetComponent<UIButtonMessageArguments>().GetInts()[0];
		num += friendListOffset;
		stateObject.SelectGhost(GhostType.Friend, friendsScores[num].m_steamIDUser.m_SteamID);
		downloadingLabel.transform.parent.gameObject.SetActive(value: true);
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
	}

	public void Button_Medal(object value)
	{
		initializingGhost = true;
		mainStateButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.Deselect();
		SetGhostSelectionState(GhostSelectionState.None);
		string text = (value as GameObject).GetComponent<UIButtonMessageArguments>().GetStrings()[0];
		switch (text)
		{
		case "Bronze":
			stateObject.SelectGhost(GhostType.MedalBronze, 0uL);
			break;
		case "Silver":
			stateObject.SelectGhost(GhostType.MedalSilver, 0uL);
			break;
		case "Gold":
			stateObject.SelectGhost(GhostType.MedalGold, 0uL);
			break;
		case "Rainbow":
			stateObject.SelectGhost(GhostType.MedalRainbow, 0uL);
			break;
		case "Challenge":
			stateObject.StartGame();
			break;
		case "Bonus":
			stateObject.SelectGhost(GhostType.None, 0uL);
			break;
		default:
			Debug.LogError("Error: Medal type: " + text + " doesn't exist.");
			stateObject.SelectGhost(GhostType.None, 0uL);
			break;
		}
	}

	private void FixInputObjectsForGhostButtons()
	{
		int num = 7;
		for (int i = 0; i < ghostButtons.Count; i++)
		{
			if (!ghostButtons[i].gameObject.activeSelf)
			{
				num = i - 1;
				break;
			}
		}
		for (int j = 0; j < num + 1; j++)
		{
			if (j == 0)
			{
				ghostButtons[j].selectOnUp = ghostButtons[num];
			}
			else
			{
				ghostButtons[j].selectOnUp = ghostButtons[j - 1];
			}
			if (j == num)
			{
				ghostButtons[j].selectOnDown = ghostButtons[0];
			}
			else
			{
				ghostButtons[j].selectOnDown = ghostButtons[j + 1];
			}
		}
	}

	public float GetCurrentLevelHighscore()
	{
		return Singleton<PlayerPrefsManager>.SP.GetHighscore(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
	}

	public void ForceFriendDownload(ulong friendID)
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Can't select friend because steam is null!");
			return;
		}
		forceFriendDownloadID = friendID;
		waitingForFriendsScores = true;
		Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
		SetGhostSelectionState(GhostSelectionState.None);
		downloadingLabel.transform.parent.gameObject.SetActive(value: true);
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
	}
}
