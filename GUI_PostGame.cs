using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

public class GUI_PostGame : GUI_Base
{
	public enum PostGameState
	{
		Finish,
		Results
	}

	private struct PlayerResult
	{
		public float finishTime;

		public string name;

		public GameObject playerObj;

		public bool isPlayer;

		public bool isHuman;
	}

	public HighscoreList highscoreList;

	public UILabel rank;

	public UILabel steamLeaderboardTypeLabel;

	public UILabel playerBest;

	public UILabel timeThisRun;

	public UILabel nextMedalAt;

	public UILabel bonusLevelLabel;

	public UILabel challengeLabel;

	public UILabel timeLabel;

	public UILabel timeDifLabel;

	public UILabel PersonalBestLabel;

	public UILabel topTitleLabel;

	public List<GameObject> finishStateObjects;

	public List<GameObject> resultsStateObjects;

	public GameObject medalPrefab;

	public GameObject medalParent;

	public List<Material> medalMaterials;

	private int friendScoresOffset;

	private int globalPageNum;

	private int globalNearUserPageNum;

	public PostGameState postGameState;

	public HighscoreState highscoreListState;

	private int newMedalsThisRun;

	private bool receivedLeaderboardEntries;

	private bool leaderboardsGlobalReceived;

	private bool leaderboardsGlobalNearUserReceived;

	private bool leaderboardsFriendsReceived;

	public bool submittingReplayDone;

	private bool allowUserVote;

	private bool challengesSent;

	public GameObject sendChallengeButton;

	public UISprite topArrow;

	public UISprite botArrow;

	public UISprite voteSprite;

	public List<GameObject> screen2Medals;

	public List<GameObject> screen2MedalsCopy;

	public GameObject medalSlots;

	public GameObject medalSlotsCopy;

	public GameObject workshopVote;

	public GameObject askForWorkshopVote;

	public GUIHighscoreEntry[] highscoreListButtons;

	public InputObject firstButton;

	public UILabel resultsNameLabel;

	public UILabel resultsTimeLabel;

	public UILabel resultsTimeDiffLabel;

	public UILabel creditsLabel;

	public bool autoSwitched;

	private int prevMedalCount;

	private bool leavingPostgame;

	private void TransitionCompleted()
	{
		autoSwitched = false;
		challengesSent = false;
		friendScoresOffset = 0;
		globalPageNum = 0;
		globalNearUserPageNum = 0;
		newMedalsThisRun = 0;
		receivedLeaderboardEntries = false;
		leaderboardsGlobalReceived = false;
		leaderboardsGlobalNearUserReceived = false;
		leaderboardsFriendsReceived = false;
		submittingReplayDone = false;
		allowUserVote = false;
		bonusLevelLabel.text = string.Empty;
		leavingPostgame = false;
		workshopVote.SetActive(value: false);
		askForWorkshopVote.SetActive(value: false);
		prevMedalCount = Singleton<LevelBatchManager>.SP.MedalCount();
		InitializeScreen(0.5f);
		GUIHighscoreEntry[] array = highscoreListButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset();
		}
		ShowChallengeCheckboxes(state: false);
		FillInResultsLabel();
		AudioController.GetAudioItem("PickupCoin").subItems[0].PitchShift = 0f;
		UITweener[] components = medalSlotsCopy.GetComponents<UITweener>();
		for (int j = 0; j < components.Length; j++)
		{
			components[j].ResetToBeginning();
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().bonusTime > 0f)
		{
			AudioController.Stop("mul_countdown", 0.2f);
		}
	}

	private void LeaveGUIScreen()
	{
		Singleton<PermaGUI>.SP.ShowMedalCount(toggle: false);
	}

	private void FillInResultsLabel()
	{
		List<GameObject> list = Singleton<PlayerManager>.SP.GetAllGhosts().ToList();
		List<PlayerResult> list2 = new List<PlayerResult>();
		for (int i = 0; i < list.Count; i++)
		{
			ReplayController component = list[i].GetComponent<ReplayController>();
			PlayerResult item = new PlayerResult
			{
				finishTime = component.finishTime,
				name = component.playerName,
				playerObj = list[i],
				isPlayer = false
			};
			if (component.steamID == 0L)
			{
				item.isHuman = false;
			}
			else
			{
				item.isHuman = true;
			}
			list2.Add(item);
		}
		list2.Add(new PlayerResult
		{
			finishTime = Singleton<CheckpointManager>.SP.GetFinishTime(),
			name = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(Singleton<HenkSWUserStats>.SP.GetSteamID()),
			playerObj = Singleton<PlayerManager>.SP.GetPlayer(),
			isPlayer = true,
			isHuman = true
		});
		float highscore = Singleton<PlayerPrefsManager>.SP.GetHighscore(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
		if (highscore != Singleton<CheckpointManager>.SP.GetFinishTime())
		{
			list2.Add(new PlayerResult
			{
				finishTime = highscore,
				name = Language.Get("PERSONALBEST", "LEADERBOARD"),
				isPlayer = false,
				isHuman = false
			});
		}
		list2 = list2.OrderBy((PlayerResult x) => x.finishTime).ToList();
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		bool flag = false;
		for (int num = 0; num < list2.Count; num++)
		{
			if (list2.Count <= 21)
			{
				if (list2[num].isPlayer)
				{
					flag = true;
					text = text + Language.Get("THISRUN", "GAME") + "\n";
					text2 = text2 + Singleton<HighscoreManager>.SP.ConvertTimeToString(list2[num].finishTime) + "\n";
					text3 += "\n";
				}
				else
				{
					float time = Math.Abs(Singleton<CheckpointManager>.SP.GetFinishTime() - list2[num].finishTime);
					string empty = string.Empty;
					empty = ((!flag) ? (empty + "[FF0000](+" + Singleton<HighscoreManager>.SP.ConvertTimeToString(time) + ")[-]\n") : (empty + "[00FF00](-" + Singleton<HighscoreManager>.SP.ConvertTimeToString(time) + ")[-]\n"));
					text = ((!list2[num].isHuman) ? (text + "[888888]" + list2[num].name + "[-]\n") : (text + list2[num].name + "\n"));
					text2 = text2 + Singleton<HighscoreManager>.SP.ConvertTimeToString(list2[num].finishTime) + "\n";
					text3 += empty;
				}
			}
		}
		resultsNameLabel.text = text;
		resultsTimeLabel.text = text2;
		resultsTimeDiffLabel.text = text3;
		int num2 = list2.Count * 25;
		resultsNameLabel.transform.parent.localPosition = new Vector3(0f, -550 + num2, 0f);
	}

	private void ToggleCredits(bool state)
	{
		creditsLabel.transform.parent.gameObject.SetActive(state);
		if (state)
		{
			creditsLabel.transform.localPosition = new Vector3(creditsLabel.transform.localPosition.x, -425f, 0f);
			creditsLabel.text = HenkUtils.GetCredits();
		}
	}

	public void Button_Retry()
	{
		if (acceptInput)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
			AudioController.Play("Reset");
		}
	}

	public void Button_MainMenu()
	{
	}

	public void Button_SwitchGhost()
	{
		if (acceptInput)
		{
			Singleton<PlayerManager>.SP.RemoveGhosts();
			Singleton<PlayerManager>.SP.ghostSet = false;
			Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
		}
	}

	public void SetWorkshopLabels()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
		{
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().workshopVote != EWorkshopVote.k_EWorkshopVoteUnvoted)
			{
				workshopVote.SetActive(value: true);
				askForWorkshopVote.SetActive(value: false);
				SetWorkshopVoteIcon(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().workshopVote);
			}
			else
			{
				workshopVote.SetActive(value: false);
				askForWorkshopVote.SetActive(value: true);
				allowUserVote = true;
			}
		}
	}

	public void SetWorkshopVoteIcon(EWorkshopVote vote)
	{
		switch (vote)
		{
		case EWorkshopVote.k_EWorkshopVoteFor:
			voteSprite.spriteName = "up_vote_icon";
			break;
		case EWorkshopVote.k_EWorkshopVoteAgainst:
			voteSprite.spriteName = "down_vote_icon";
			break;
		default:
			voteSprite.spriteName = string.Empty;
			break;
		}
	}

	public void Button_ToLevelSelect()
	{
		if (!acceptInput)
		{
			return;
		}
		if (postGameState == PostGameState.Finish)
		{
			SetPostGameState(PostGameState.Results);
			return;
		}
		leavingPostgame = true;
		if (!submittingReplayDone && Singleton<HighscoreManager>.SP.submittingScore)
		{
			return;
		}
		Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
		{
			UnityEngine.Object.FindObjectOfType<State_LevelEditorMain>().cameFromMenu = false;
			Singleton<GamestateManager>.SP.SetState(typeof(State_LevelEditorMain));
			HenkUtils.BackToMenu();
			return;
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevel() == 97 || Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
			return;
		}
		string postLevelCutscene = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().postLevelCutscene;
		bool flag = Singleton<CheckpointManager>.SP.GetFinishTime() < Singleton<CheckpointManager>.SP.opponentFinishTime;
		bool flag2 = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType != LevelType.Challenge || flag;
		if (postLevelCutscene != string.Empty && flag2)
		{
			UnityEngine.Object.FindObjectOfType<State_Cutscene>().LoadCutscene(postLevelCutscene, isPostGame: true);
		}
		else
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_LevelSelectCampaign));
		}
	}

	public void DoneUploadingHighscores(bool success)
	{
		submittingReplayDone = true;
	}

	public void Button_NextLevel()
	{
		Singleton<LevelBatchManager>.SP.LoadNextLevel();
	}

	public void FillInScoreLabelsNotConnected()
	{
		highscoreList.FillInScoreListNotConnected();
	}

	public void GetNextTenLeaderboardItems(ELeaderboardDataRequest requestType, int pageNum = 0)
	{
		Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(requestType, onlyGetWorldChamp: false, onlyGetSelf: false, pageNum);
		receivedLeaderboardEntries = false;
		highscoreList.StartLoadingScores();
		ShowChallengeCheckboxes(state: false);
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
		SetBGPanelSubTitle(highscoreListState);
		ELeaderboardDataRequest requestType;
		if (highscoreListState == HighscoreState.Global)
		{
			if (leaderboardsGlobalReceived)
			{
				Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(HighscoreState.Global), Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
				StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.Global));
				return;
			}
			requestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
		}
		else if (highscoreListState == HighscoreState.GlobalNearUser)
		{
			if (leaderboardsGlobalNearUserReceived)
			{
				Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(HighscoreState.GlobalNearUser), Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
				StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.GlobalNearUser));
				return;
			}
			requestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
		}
		else
		{
			if (leaderboardsFriendsReceived)
			{
				Singleton<HighscoreManager>.SP.SetCurrentLeaderboardEntries(Singleton<HenkSWLeaderboards>.SP.GetScoreListFromType(HighscoreState.Friends), Singleton<LevelBatchManager>.SP.GetCurrentLevelObj());
				StartCoroutine(FillInScoreLabelsSteamworks(HighscoreState.Friends));
				return;
			}
			requestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
		}
		Singleton<HenkSWLeaderboards>.SP.DownloadEntriesForCurrentLeaderboard(requestType);
		receivedLeaderboardEntries = false;
		challengeLabel.enabled = false;
		highscoreList.StartLoadingScores();
		ShowChallengeCheckboxes(state: false);
	}

	private void SetBGPanelSubTitle(HighscoreState state)
	{
		_ = string.Empty;
		if (Singleton<LevelBatchManager>.SP.currentLevel != null)
		{
			_ = Singleton<LevelBatchManager>.SP.currentLevel.levelName;
		}
		switch (state)
		{
		case HighscoreState.Friends:
			steamLeaderboardTypeLabel.text = Language.Get("FRIENDS", "LEADERBOARD");
			break;
		case HighscoreState.Global:
			steamLeaderboardTypeLabel.text = Language.Get("TOPTEN", "LEADERBOARD");
			break;
		case HighscoreState.GlobalNearUser:
			steamLeaderboardTypeLabel.text = Language.Get("GLOBAL", "LEADERBOARD");
			break;
		}
	}

	public void FillInScoreLabelsRageSquid()
	{
		highscoreList.FillInScoreList();
	}

	public IEnumerator FillInScoreLabelsSteamworks(HighscoreState highscoreState)
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
			if (scoreList.Length <= 1 && !autoSwitched)
			{
				autoSwitched = true;
				NextLeaderboard(forwards: true);
				break;
			}
			challengeLabel.enabled = true;
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType != LevelType.Workshop && Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.None)
			{
				for (int j = 0; j < highscoreListButtons.Length; j++)
				{
					GUIHighscoreEntry gUIHighscoreEntry = highscoreListButtons[j];
					int num = j + friendScoresOffset;
					if (num < Singleton<HighscoreManager>.SP.currentLeaderboardEntries.Count)
					{
						gUIHighscoreEntry.highscoreEntry = Singleton<HighscoreManager>.SP.currentLeaderboardEntries[num];
					}
					else
					{
						gUIHighscoreEntry.highscoreEntry = null;
					}
				}
				ShowChallengeCheckboxes(state: true);
			}
			else
			{
				ShowChallengeCheckboxes(state: false);
				challengeLabel.enabled = false;
			}
			highscoreList.FillInScoreList(friendScoresOffset);
			break;
		case HighscoreState.Global:
			challengeLabel.enabled = false;
			highscoreList.FillInScoreList();
			ShowChallengeCheckboxes(state: false);
			break;
		case HighscoreState.GlobalNearUser:
			challengeLabel.enabled = false;
			highscoreList.FillInScoreList();
			ShowChallengeCheckboxes(state: false);
			break;
		}
	}

	private void ShowMedal()
	{
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			return;
		}
		float finishTime = Singleton<CheckpointManager>.SP.GetFinishTime();
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (currentLevelObj.levelType == LevelType.Challenge || currentLevelObj.levelType == LevelType.Bonus)
		{
			Singleton<LevelBatchManager>.SP.CheckChallengeLevelUnlock();
			Singleton<LevelBatchManager>.SP.CheckBonusLevelUnlock();
			return;
		}
		Medal medal = Medal.None;
		if (finishTime < currentLevelObj.rainbowTime)
		{
			medal = Medal.Rainbow;
		}
		else if (finishTime < currentLevelObj.goldTime)
		{
			medal = Medal.Gold;
		}
		else if (finishTime < currentLevelObj.silverTime)
		{
			medal = Medal.Silver;
		}
		else if (finishTime < currentLevelObj.bronzeTime)
		{
			medal = Medal.Bronze;
		}
		newMedalsThisRun = currentLevelObj.ScoreMedal(medal);
		if (newMedalsThisRun > 0)
		{
			GameObject obj = UnityEngine.Object.Instantiate(medalPrefab) as GameObject;
			obj.renderer.material = medalMaterials[(int)(medal - 1)];
			obj.transform.parent = medalParent.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			Singleton<PermaGUI>.SP.medalCountLabel.text = prevMedalCount.ToString();
			currentLevelObj.newMedals = newMedalsThisRun;
		}
		Medal bestMedal = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().bestMedal;
		if (bestMedal == Medal.Rainbow)
		{
			nextMedalAt.text = "-";
			return;
		}
		float time = currentLevelObj.MedalTime(bestMedal + 1);
		nextMedalAt.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(time);
	}

	public void SetPostGameState(PostGameState state)
	{
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		postGameState = state;
		switch (state)
		{
		case PostGameState.Finish:
		{
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelCodeOrID() == 97)
			{
				ToggleCredits(state: true);
			}
			else
			{
				ToggleCredits(state: false);
			}
			for (int num2 = medalParent.transform.childCount - 1; num2 >= 0; num2--)
			{
				UnityEngine.Object.Destroy(medalParent.transform.GetChild(num2).gameObject);
			}
			foreach (GameObject finishStateObject in finishStateObjects)
			{
				finishStateObject.SetActive(value: true);
			}
			{
				foreach (GameObject resultsStateObject in resultsStateObjects)
				{
					resultsStateObject.SetActive(value: false);
				}
				break;
			}
		}
		case PostGameState.Results:
		{
			ToggleCredits(state: false);
			SetWorkshopLabels();
			foreach (GameObject finishStateObject2 in finishStateObjects)
			{
				finishStateObject2.SetActive(value: false);
			}
			foreach (GameObject resultsStateObject2 in resultsStateObjects)
			{
				resultsStateObject2.SetActive(value: true);
				firstButton = highscoreListButtons[0].GetComponent<InputObject>();
				Singleton<InputManager>.SP.Select(firstButton);
			}
			if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
			{
				bonusLevelLabel.text = ("Active mutator: " + Singleton<MutatorManager>.SP.GetActiveMutatorString()).ToUpper();
				Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
				medalSlots.SetActive(value: false);
				medalSlotsCopy.SetActive(value: false);
				break;
			}
			if (currentLevelObj.levelType == LevelType.Challenge || currentLevelObj.levelType == LevelType.Bonus || Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().wipState == WIPState.Draft || Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
			{
				medalSlots.SetActive(value: false);
				medalSlotsCopy.SetActive(value: false);
			}
			else
			{
				medalSlots.SetActive(value: true);
				medalSlotsCopy.SetActive(value: true);
				int bestMedal = (int)currentLevelObj.bestMedal;
				foreach (GameObject screen2Medal in screen2Medals)
				{
					screen2Medal.SetActive(value: false);
				}
				for (int i = 0; i < screen2MedalsCopy.Count; i++)
				{
					screen2MedalsCopy[i].GetComponent<UISprite>().enabled = false;
				}
				float num = 0f;
				for (int j = 0; j < bestMedal; j++)
				{
					if (j >= bestMedal - newMedalsThisRun)
					{
						screen2Medals[j].SetActive(value: true);
						UITweener[] components = screen2Medals[j].GetComponents<UITweener>();
						foreach (UITweener obj in components)
						{
							obj.ResetToBeginning();
							obj.delay = num;
							num += 0.15f;
							obj.Play(forward: true);
						}
					}
					else
					{
						screen2Medals[j].SetActive(value: true);
					}
				}
				if (newMedalsThisRun > 0)
				{
					float delay = (float)newMedalsThisRun * 0.4f + 0.5f;
					UITweener[] components = medalSlotsCopy.GetComponents<UITweener>();
					foreach (UITweener obj2 in components)
					{
						obj2.delay = delay;
						obj2.Play(forward: true);
					}
					StartCoroutine(ShowMedalCopies(delay, bestMedal));
				}
			}
			string text = string.Empty;
			if (currentLevelObj.levelType == LevelType.Standard)
			{
				switch (currentLevelObj.bestMedal)
				{
				case Medal.None:
					text = Language.Get("BRONZE", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.bronzeTime));
					break;
				case Medal.Bronze:
					text = Language.Get("SILVER", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.silverTime));
					break;
				case Medal.Silver:
					text = Language.Get("GOLD", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.goldTime));
					break;
				case Medal.Gold:
					text = Language.Get("RAINBOW", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.rainbowTime));
					break;
				case Medal.Rainbow:
					text = string.Empty;
					break;
				}
			}
			else if (currentLevelObj.levelType == LevelType.Challenge && !currentLevelObj.IsChallengeCompleted())
			{
				text = "Score: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.bronzeTime) + " to beat " + currentLevelObj.GetChallengerName() + "!";
			}
			else if (currentLevelObj.levelType == LevelType.Bonus && !currentLevelObj.IsChallengeCompleted())
			{
				text = "Score: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.bonusTime) + " to unlock " + currentLevelObj.GetChallengerName() + "!";
			}
			if (text == string.Empty)
			{
				Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
			}
			else
			{
				Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: true, text);
			}
			if (currentLevelObj.levelType == LevelType.Bonus)
			{
				if (Singleton<UnlockManager>.SP.CheckCharacterUnlocked(currentLevelObj.challenger, currentLevelObj.challengerSkinNum))
				{
					bonusLevelLabel.text = Language.Get("YOUVEUNLOCKED", "GAME").Replace("{X}", Singleton<UnlockManager>.SP.GetCharacterName(currentLevelObj.challenger, currentLevelObj.challengerSkinNum));
				}
				else
				{
					bonusLevelLabel.text = Language.Get("SCORETOUNLOCK", "GAME").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(currentLevelObj.bonusTime)).Replace("{Y}", Singleton<UnlockManager>.SP.GetCharacterName(currentLevelObj.challenger, currentLevelObj.challengerSkinNum));
				}
			}
			break;
		}
		}
	}

	private IEnumerator ShowMedalCopies(float delay, int bestMedal)
	{
		yield return new WaitForSeconds(delay - 0.2f);
		for (int i = 0; i < bestMedal; i++)
		{
			screen2MedalsCopy[i].GetComponent<UISprite>().enabled = true;
		}
	}

	public void PlayMedalScoredSound()
	{
		if (!leavingPostgame)
		{
			AudioController.GetAudioItem("PickupCoin").subItems[0].PitchShift = 0f;
			Singleton<PermaGUI>.SP.ShowMedalCount(showMedals);
		}
	}

	public void PlayMedalSound()
	{
		AudioController.Play("PickupCoin");
		AudioController.GetAudioItem("PickupCoin").subItems[0].PitchShift += 1f;
	}

	public void ResetGUI()
	{
		rank.text = string.Empty;
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
		int num = 0;
		for (int i = 0; i < Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard.Length; i++)
		{
			if (Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_steamIDUser == Singleton<HenkSWUserStats>.SP.GetSteamID())
			{
				num = Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_nGlobalRank;
				playerBest.text = Singleton<HighscoreManager>.SP.ConvertTimeToString((float)Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[i].m_nScore / 1000f);
			}
		}
		int numberOfLeaderboardEntriesForCurrentLeaderboard = Singleton<HenkSWLeaderboards>.SP.GetNumberOfLeaderboardEntriesForCurrentLeaderboard();
		if (numberOfLeaderboardEntriesForCurrentLeaderboard > 1)
		{
			float num2 = ((float)num - 1f) / ((float)numberOfLeaderboardEntriesForCurrentLeaderboard - 1f);
			int num3 = Mathf.FloorToInt(100f - num2 * 100f);
			rank.text = Language.Get("PERCENTAGEBEATEN", "LEADERBOARD").Replace("{X}", num3.ToString()) + " [" + Language.Get("RANK", "LEADERBOARD") + " " + num + "/" + Singleton<HenkSWLeaderboards>.SP.GetNumberOfLeaderboardEntriesForCurrentLeaderboard() + "]";
			if (num3 >= 99)
			{
				Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.OnePercent);
			}
		}
		StartCoroutine(FillInScoreLabelsSteamworks(highscoreState));
	}

	private void Update()
	{
		if (!challengesSent && highscoreListState == HighscoreState.Friends && postGameState == PostGameState.Results && Singleton<HighscoreManager>.SP.NumberOfChallengesSelected() != 0)
		{
			sendChallengeButton.GetComponent<UITweener>().PlayForward();
		}
		else
		{
			sendChallengeButton.GetComponent<UITweener>().PlayReverse();
		}
		if (postGameState != PostGameState.Results)
		{
			return;
		}
		UpdateScrollingPages();
		if (highscoreListState == HighscoreState.Friends)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.ToggleChallengeCheckbox))
			{
				AudioController.Play("CheckboxToggleOn");
				GUIHighscoreEntry component = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<GUIHighscoreEntry>();
				if (component.highscoreEntry != null && component.highscoreEntry.localPlayerHasBeatenThisScore)
				{
					ToggleChallengeCheckbox(component, !component.highscoreEntry.sendChallenge);
				}
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.SendChallenges))
			{
				if (Singleton<HenkSWLeaderboards>.SP.currentlyRefreshingScores || challengesSent || Singleton<HighscoreManager>.SP.NumberOfChallengesSelected() == 0)
				{
					return;
				}
				Singleton<HighscoreManager>.SP.SendChallenge();
				challengesSent = true;
				ShowChallengeCheckboxes(state: true);
				AudioController.Play("inbox_challengesent");
			}
		}
		if (allowUserVote)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.VoteLevelUp))
			{
				VoteWorkshopItem(state: true);
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.VoteLevelDown))
			{
				VoteWorkshopItem(state: false);
			}
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
				Singleton<InputManager>.SP.Select(highscoreListButtons[9].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
			}
			else if (highscoreListState == HighscoreState.Global)
			{
				globalPageNum++;
				GetNextTenLeaderboardItems(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, globalPageNum);
				Singleton<InputManager>.SP.Select(highscoreListButtons[9].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
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
				Singleton<InputManager>.SP.Select(highscoreListButtons[0].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
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
				Singleton<InputManager>.SP.Select(highscoreListButtons[0].GetComponent<InputObject>(), delayedTillEndOfFrame: false, playSound: false);
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

	public void VoteWorkshopItem(bool state)
	{
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (currentLevelObj.workshopVote == EWorkshopVote.k_EWorkshopVoteUnvoted)
		{
			Singleton<Workshop>.SP.SetItemVote(currentLevelObj.publishedFiledID, state);
			Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().workshopVote = (state ? EWorkshopVote.k_EWorkshopVoteFor : EWorkshopVote.k_EWorkshopVoteAgainst);
			SetWorkshopLabels();
		}
	}

	public void StartPostGame()
	{
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().postLevelCutscene != string.Empty)
		{
			Singleton<RewardManager>.SP.disableRewardQueue = true;
		}
		SetPostGameState(PostGameState.Finish);
		ResetGUI();
		highscoreList.StartLoadingScores();
		ShowMedal();
		PersonalBestLabel.enabled = Singleton<HighscoreManager>.SP.newLocalBest;
		float finishTime = Singleton<CheckpointManager>.SP.GetFinishTime();
		timeLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(finishTime);
		timeThisRun.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(finishTime);
		float opponentFinishTime = Singleton<CheckpointManager>.SP.opponentFinishTime;
		float time = Mathf.Abs(finishTime - opponentFinishTime);
		if (finishTime < opponentFinishTime || opponentFinishTime == 0f)
		{
			UILabel uILabel = timeLabel;
			Color green = Color.green;
			timeDifLabel.color = green;
			uILabel.color = green;
			timeDifLabel.text = "(-";
		}
		else
		{
			UILabel uILabel2 = timeLabel;
			Color red = Color.red;
			timeDifLabel.color = red;
			uILabel2.color = red;
			timeDifLabel.text = "(+";
		}
		UILabel uILabel3 = timeDifLabel;
		uILabel3.text = uILabel3.text + Singleton<HighscoreManager>.SP.ConvertTimeToString(time) + ")";
		if (opponentFinishTime == 0f)
		{
			timeDifLabel.text = string.Empty;
		}
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (currentLevelObj.levelType == LevelType.Challenge && finishTime < opponentFinishTime)
		{
			currentLevelObj.ChallengeCompleted();
		}
		else if (currentLevelObj.levelType == LevelType.Bonus && finishTime < currentLevelObj.bonusTime)
		{
			currentLevelObj.BonusCompleted();
		}
		topTitleLabel.text = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelName.ToUpper();
	}

	public void ToggleChallengeCheckbox(GUIHighscoreEntry entryButton, bool state)
	{
		if (entryButton.checkbox.gameObject.activeInHierarchy)
		{
			entryButton.highscoreEntry.sendChallenge = state;
			entryButton.checkbox.value = state;
		}
	}

	private void EnableEnvelopes(List<string> userIDs)
	{
		GUIHighscoreEntry[] array = highscoreListButtons;
		foreach (GUIHighscoreEntry gUIHighscoreEntry in array)
		{
			if (gUIHighscoreEntry.highscoreEntry != null && userIDs.Contains(gUIHighscoreEntry.highscoreEntry.userID))
			{
				gUIHighscoreEntry.envelope.enabled = true;
				ToggleChallengeCheckbox(gUIHighscoreEntry, state: false);
				gUIHighscoreEntry.checkbox.gameObject.SetActive(value: false);
			}
		}
	}

	private void EnableExclamation(List<string> userIDs)
	{
		GUIHighscoreEntry[] array = highscoreListButtons;
		foreach (GUIHighscoreEntry gUIHighscoreEntry in array)
		{
			if (gUIHighscoreEntry.highscoreEntry != null && userIDs.Contains(gUIHighscoreEntry.highscoreEntry.userID))
			{
				gUIHighscoreEntry.exclamation.enabled = true;
			}
		}
	}

	private void ShowChallengeCheckboxes(bool state)
	{
		if (state)
		{
			GUIHighscoreEntry[] array = highscoreListButtons;
			foreach (GUIHighscoreEntry gUIHighscoreEntry in array)
			{
				if (!challengesSent && gUIHighscoreEntry.highscoreEntry != null && gUIHighscoreEntry.highscoreEntry.localPlayerHasBeatenThisScore)
				{
					gUIHighscoreEntry.checkbox.gameObject.SetActive(value: true);
					ToggleChallengeCheckbox(gUIHighscoreEntry, gUIHighscoreEntry.highscoreEntry.sendChallenge);
				}
				else
				{
					gUIHighscoreEntry.checkbox.gameObject.SetActive(value: false);
				}
				gUIHighscoreEntry.envelope.enabled = false;
				gUIHighscoreEntry.exclamation.enabled = false;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			HenkSession[] allSessions = Singleton<HenkSWNotifications>.SP.GetAllSessions();
			foreach (HenkSession henkSession in allSessions)
			{
				if (henkSession.level == Singleton<LevelBatchManager>.SP.GetCurrentLevel())
				{
					if (henkSession.ourState == SessionState.Waiting)
					{
						list.Add(henkSession.theirSteamID.ToString());
					}
					else
					{
						list2.Add(henkSession.theirSteamID.ToString());
					}
				}
			}
			EnableExclamation(list2);
			EnableEnvelopes(list);
		}
		else
		{
			GUIHighscoreEntry[] array = highscoreListButtons;
			foreach (GUIHighscoreEntry obj in array)
			{
				obj.checkbox.value = true;
				obj.checkbox.gameObject.SetActive(value: false);
				obj.exclamation.enabled = false;
				obj.envelope.enabled = false;
			}
		}
	}
}
