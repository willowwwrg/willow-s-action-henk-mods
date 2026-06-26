using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class GUI_MainMenu : GUI_Base
{
	public InputObject FirstButton;

	public InputObject PrevButton;

	public UILabel loadingLabel;

	public UILabel serverStatusLabel;

	public InputObject exitButton;

	public UILabel dcLevel;

	public UILabel dcMutator;

	public UILabel dcTimeleft;

	public UILabel dcRank;

	public GameObject twitchNotification;

	private bool waitingForTwitchHenkUnlock;

	public bool streamcheckNowLive;

	private string streamCheckURL = string.Empty;

	public UILabel streamCheckLabel;

	private bool initialFetchDone;

	public GameObject anchorObject;

	public List<UILabel> marqueeLabels;

	private UILabel currentLastMarqueeLabel;

	private bool marqueeStarted;

	public float marqueeSpeed = 0.008f;

	public GameObject twitchStatusBox;

	public UILabel twitchStatusBoxLabel;

	private void Start()
	{
		Singleton<PermaGUI>.SP.KillCountdownLabels();
	}

	private void Update()
	{
		int timeLeftTillServerOnline = Singleton<MultiManager>.SP.timeLeftTillServerOnline;
		if (timeLeftTillServerOnline == 0 || timeLeftTillServerOnline == 1000)
		{
			serverStatusLabel.text = Singleton<MultiManager>.SP.serverStatusText;
		}
		else
		{
			serverStatusLabel.text = "Fetching server status...";
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.SwitchCharacter) && !Singleton<PermaGUI>.SP.confirmationRequestUp)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_CharacterSelectionCampaign));
			AudioController.Play("ButtonForwards");
		}
		if (Singleton<MutatorManager>.SP.selectedLevel != null)
		{
			dcLevel.text = Singleton<MutatorManager>.SP.selectedLevel.levelName;
			dcMutator.text = Singleton<MutatorManager>.SP.GetActiveMutatorString();
			dcTimeleft.text = HenkUtils.ConvertSecondsToTimeString(Singleton<MutatorManager>.SP.secondsLeft);
			dcRank.text = Singleton<MutatorManager>.SP.currentRank;
		}
		if (marqueeStarted)
		{
			UpdateMarqueeLabels();
		}
		else if (Singleton<ActionHenk>.SP.fetchedGameSettings)
		{
			InitMarqueeLabels();
		}
		if (waitingForTwitchHenkUnlock && !Singleton<RewardManager>.SP.showingReward)
		{
			waitingForTwitchHenkUnlock = false;
			if (streamCheckURL != string.Empty)
			{
				Application.OpenURL(streamCheckURL);
			}
		}
		if (streamcheckNowLive)
		{
			anchorObject.transform.localPosition = new Vector3(355f - (float)streamCheckLabel.text.Length * 11.6f, 0f, 0f);
			twitchNotification.GetComponent<TweenPosition>().PlayForward();
			if (Singleton<InputManager>.SP.CheckAction(InputAction.TuneInOnTwitch))
			{
				if (!Singleton<UnlockManager>.SP.CheckCharacterUnlocked(CharacterSelect.Characters.Henk, 19))
				{
					waitingForTwitchHenkUnlock = true;
					Singleton<UnlockManager>.SP.UnlockCharacter(CharacterSelect.Characters.Henk, 19);
				}
				else if (streamCheckURL != string.Empty)
				{
					Application.OpenURL(streamCheckURL);
				}
			}
		}
		else
		{
			twitchNotification.GetComponent<TweenPosition>().PlayReverse();
		}
	}

	public void CheckIfStreamIsOnline()
	{
		if (!initialFetchDone)
		{
			streamcheckNowLive = false;
			streamCheckURL = string.Empty;
		}
		StartCoroutine(CheckIfStreamIsOnlineRoutine());
	}

	private IEnumerator CheckIfStreamIsOnlineRoutine()
	{
		WWW webCall = new WWW(Singleton<PhpMyAdminMan>.SP.GetStreamCheckURL());
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error retrieving stream status for stream: " + webCall.error);
			yield break;
		}
		JSONNode jSONNode = JSON.Parse(webCall.text);
		streamcheckNowLive = jSONNode["live"].AsBool;
		streamCheckLabel.text = jSONNode["text"];
		streamCheckURL = jSONNode["url"];
		if (!initialFetchDone)
		{
			initialFetchDone = true;
		}
	}

	private void NextWindow()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() == GUIManager.GUIScreens.GUIScreen_MainMenu)
		{
			PrevButton = Singleton<InputManager>.SP.GetCurrentButton();
			Singleton<InputManager>.SP.ClickCurrentButton();
			AudioController.Play("ButtonForwards");
		}
	}

	public void Button_Campaign()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_BatchSelect));
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Singleplayer);
	}

	public void Button_LevelEditor()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_LevelEditorMain));
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.LevelEditor);
	}

	public void Button_Multiplayer()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MultiplayerPicker));
	}

	public void Button_Leaderboard()
	{
		if (!SteamManager.Initialized)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDSTEAMCONNECT", "PERMA"));
		}
		else
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_Leaderboards));
		}
	}

	public void Button_Options()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_Options));
	}

	public void Button_Exit()
	{
		Singleton<PermaGUI>.SP.RequestConfirmation("QuitGame", base.gameObject);
	}

	public void QuitGame(object value)
	{
		if ((bool)value)
		{
			Singleton<ActionHenk>.SP.exitgame = true;
		}
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		CheckIfStreamIsOnline();
		Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
		loadingLabel.enabled = false;
		if (PrevButton != null)
		{
			Singleton<InputManager>.SP.Select(PrevButton, delayedTillEndOfFrame: true, playSound: false);
		}
		else
		{
			Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: true, playSound: false);
		}
		Singleton<MainMenuHighlighter>.SP.ToggleOnOff(state: false);
		Singleton<RewardManager>.SP.disableRewardQueue = false;
		if (Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters)
		{
			Camera.main.GetComponent<MenuCamera>().LookAtCharacter((CharacterSelect.Characters)Singleton<PlayerPrefsManager>.SP.GetInt("LASTPLAYEDCHARACTER", 1));
			Camera.main.GetComponent<MenuCamera>().lookingAtLMPCharacters = false;
		}
		Singleton<PermaGUI>.SP.RedrawAllGUI();
	}

	private void UpdateMarqueeLabels()
	{
		for (int i = 0; i < marqueeLabels.Count; i++)
		{
			marqueeLabels[i].transform.Translate(new Vector3((0f - marqueeSpeed) * Time.deltaTime, 0f, 0f));
			if (marqueeLabels[i].transform.localPosition.x < (float)(-(Screen.currentResolution.width / 2 + marqueeLabels[i].width)))
			{
				marqueeLabels[i].transform.localPosition = new Vector3(currentLastMarqueeLabel.transform.localPosition.x + (float)marqueeLabels[i].width + 80f, marqueeLabels[i].transform.localPosition.y, marqueeLabels[i].transform.localPosition.z);
				currentLastMarqueeLabel = marqueeLabels[i];
			}
		}
	}

	private void InitMarqueeLabels()
	{
		for (int i = 0; i < marqueeLabels.Count; i++)
		{
			marqueeLabels[i].transform.localPosition = new Vector3(0f, marqueeLabels[i].transform.localPosition.y, marqueeLabels[i].transform.localPosition.z);
			marqueeLabels[i].text = Singleton<ActionHenk>.SP.motdString;
		}
		int num = 0;
		for (int j = 0; j < marqueeLabels.Count; j++)
		{
			Vector3 localPosition = marqueeLabels[j].transform.localPosition;
			marqueeLabels[j].transform.localPosition = new Vector3(localPosition.x + (float)num, localPosition.y, localPosition.z);
			num += marqueeLabels[j].width + 80;
			currentLastMarqueeLabel = marqueeLabels[j];
		}
		marqueeStarted = true;
	}
}
