using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermaGUI : Singleton<PermaGUI>
{
	public enum GuiItem
	{
		LeftPanel
	}

	public enum FadeState
	{
		fadedIn,
		fadedOut,
		fadingIn,
		fadingOut,
		none
	}

	private struct FadeRoutineArgs
	{
		public float waitTime;
	}

	public TweenAlpha FadeScreen;

	private bool fading;

	public GameObject genericBackgroundPanel;

	public GameObject genericBackgroundPanelLeft;

	public UILabel titleLabel;

	public UILabel subTitleLabel;

	public UILabel subSubTitleLabel;

	public UILabel backButton;

	public UILabel forwardsButton;

	public TweenPosition bgLeftPanelTween;

	private bool togglingBgPanelLeftIn;

	public State_OptionsGame optionsGame;

	public GameObject leftArrow;

	public GameObject rightArrow;

	private bool arrowsVisible;

	private ArrowPositions prefArrowPositions;

	private Vector3 prefArrowScale;

	public GameObject confirmationWindow;

	private string confirmationAction = string.Empty;

	private GameObject confirmationRequester;

	public bool confirmationRequestUp;

	public UILabel confirmationMessage;

	private bool errorPopupVisible;

	public GameObject chatBoxObject;

	public UILabel labelReady;

	public UILabel labelGo;

	public List<UISprite> labelsReadyGo;

	public UILabel notificationLabel;

	private List<string> notificationQueue = new List<string>();

	private bool showingNotification;

	public GameObject comingSoon;

	public GameObject errorPopup;

	public List<Material> medalMaterials;

	public GameObject ingameTutorial;

	public GameObject ingameTutorialHook;

	public UILabel medalCountLabel;

	public UISprite medalCountSprite;

	public UISprite medalCountSpriteRainbow;

	public GameObject medalCountObjects;

	public int inboxOffset;

	private int numItemsInInbox = 7;

	public UILabel inboxNotificationCount;

	public GameObject inboxObject;

	public GameObject inboxButtonParent;

	public List<GameObject> inboxButtons;

	public UILabel inboxCountLabel;

	public GameObject inbox;

	public InputObject firstInboxButton;

	public InputObject prevButton;

	public bool canToggleInbox;

	public GameObject inboxDeleteNotificationButton;

	private InboxMessage selectedInboxMessage;

	private string prevForwardsButtonText = string.Empty;

	public GameObject storyLineImagePlane;

	public UIInput inputField;

	public UILabel inputFieldTitle;

	public GameObject batchLevelUnlock;

	public UILabel batchLevelInfoLabel;

	public GameObject batchLevelImagePlane;

	public GameObject characterUnlock;

	public UILabel characterInfoLabel;

	public GameObject loadingSprite;

	private bool newMessage;

	public GameObject twitchPanel;

	public UILabel twitchStatusLabel;

	public UILabel twitchTextLabel;

	public GameObject continueButton;

	public UILabel continueButtonText;

	public GameObject multiplayerMutatorObject;

	public UILabel multiplayerMutatorLabel;

	private FadeState fadeState = FadeState.none;

	private void Awake()
	{
		FadeScreen.GetComponent<UISprite>().enabled = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (confirmationRequestUp)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel, forceThroughDisabledInput: true))
			{
				Confirm(state: false);
			}
			else if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm, forceThroughDisabledInput: true))
			{
				Confirm(state: true);
			}
		}
		if (storyLineImagePlane.activeInHierarchy && (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm, forceThroughDisabledInput: true) || Singleton<InputManager>.SP.CheckAction(InputAction.Cancel, forceThroughDisabledInput: true)))
		{
			HideStoryPlane();
		}
		int numberOfNotifications = Singleton<InboxManager>.SP.GetNumberOfNotifications();
		if (numberOfNotifications == 0)
		{
			inboxNotificationCount.text = " 0";
		}
		else if (Mathf.Repeat(Time.time, 1f) > 0.6f)
		{
			inboxNotificationCount.text = string.Empty;
		}
		else
		{
			inboxNotificationCount.text = "[FF0000] " + numberOfNotifications + "[-]";
		}
		if (Input.GetKeyDown(KeyCode.LeftAlt))
		{
			newMessage = !newMessage;
		}
		if (inbox.activeInHierarchy)
		{
			if ((bool)Singleton<InputManager>.SP.GetCurrentButton() && (bool)Singleton<InputManager>.SP.GetCurrentButton().GetComponent<InboxItem>())
			{
				selectedInboxMessage = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<InboxItem>().inboxMessage;
			}
			else
			{
				selectedInboxMessage = null;
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_6")
			{
				int num = Singleton<InboxManager>.SP.GetAllMessages().Length - numItemsInInbox;
				if (inboxOffset < num)
				{
					inboxOffset++;
					InitializeInboxMessages();
					AudioController.Play("ui_main_hover");
				}
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Up) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_0" && inboxOffset > 0)
			{
				inboxOffset--;
				InitializeInboxMessages();
				AudioController.Play("ui_main_hover");
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.DeleteInboxMessage))
			{
				DeleteMessage();
			}
		}
		if (notificationQueue.Count > 0 && !showingNotification)
		{
			StartCoroutine(ShowNotification());
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.ToggleInbox))
		{
			ToggleInbox(!inbox.activeInHierarchy);
		}
	}

	public void ToggleMultiplayerMutatorText(bool state, string mutatorText)
	{
		multiplayerMutatorObject.SetActive(state);
		multiplayerMutatorLabel.text = mutatorText;
	}

	public void ToggleLoadingSprite(bool state)
	{
		loadingSprite.GetComponent<TweenPosition>().Play(state);
	}

	private IEnumerator DelayedSelectButton(InputObject buttonToSelect)
	{
		yield return new WaitForEndOfFrame();
		Singleton<InputManager>.SP.Select(buttonToSelect);
	}

	public void ShowInboxInfo(bool toggle = true)
	{
		inboxObject.GetComponent<UITweener>().Play(toggle);
		canToggleInbox = toggle;
	}

	public void ToggleInbox(bool toggle)
	{
		if (canToggleInbox)
		{
			if (toggle)
			{
				inboxOffset = 0;
				prevButton = Singleton<InputManager>.SP.GetCurrentButton();
				inbox.SetActive(value: true);
				InitializeInboxMessages();
				Singleton<InputManager>.SP.Select(firstInboxButton, delayedTillEndOfFrame: false, playSound: false);
				prevForwardsButtonText = forwardsButton.text;
				SetBackForwardButtonTexts(string.Empty, "OPEN");
				AudioController.Play("inbox_open");
			}
			else
			{
				Singleton<InboxManager>.SP.MarkNotificationsAsRead();
				inboxButtonParent.BroadcastMessage("Reset", SendMessageOptions.DontRequireReceiver);
				inbox.SetActive(value: false);
				Singleton<InputManager>.SP.Select(prevButton, delayedTillEndOfFrame: false, playSound: false);
				SetBackForwardButtonTexts(string.Empty, prevForwardsButtonText);
				AudioController.Play("inbox_close");
			}
		}
	}

	public void OpenMessage()
	{
		if (selectedInboxMessage != null && selectedInboxMessage.messageType == InboxMessageType.FriendChallenge)
		{
			Singleton<PermaGUI>.SP.RequestConfirmation("ConfirmedOpenMessage", base.gameObject, Language.Get("STARTLEVEL", "INBOX") + "\n" + selectedInboxMessage.linkedSession.levelName + "?");
		}
		else if (selectedInboxMessage != null && selectedInboxMessage.messageType == InboxMessageType.StoryImage)
		{
			selectedInboxMessage.unread = false;
			storyLineImagePlane.SetActive(value: true);
			storyLineImagePlane.GetComponentInChildren<MeshRenderer>().material = selectedInboxMessage.storyMessageMaterial;
			Singleton<InputManager>.SP.inputEnabled = false;
		}
		else if (selectedInboxMessage != null && selectedInboxMessage.messageType == InboxMessageType.StoryMessage)
		{
			selectedInboxMessage.unread = false;
			storyLineImagePlane.SetActive(value: true);
			storyLineImagePlane.GetComponentInChildren<MeshRenderer>().material = selectedInboxMessage.storyMessageMaterial;
			MonoBehaviour.print(selectedInboxMessage.body);
			Singleton<InputManager>.SP.inputEnabled = false;
		}
		AudioController.Play("inbox_messageopen");
		InitializeInboxMessages();
	}

	public void HideStoryPlane()
	{
		AudioController.Play("inbox_messageclose");
		storyLineImagePlane.SetActive(value: false);
		Singleton<InputManager>.SP.inputEnabled = true;
		Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
	}

	public void DeleteMessage()
	{
		if (selectedInboxMessage != null && selectedInboxMessage.messageType == InboxMessageType.FriendChallenge)
		{
			Singleton<PermaGUI>.SP.RequestConfirmation("ConfirmedDeleteMessage", base.gameObject, Language.Get("DELETEMESSAGE", "INBOX"));
		}
	}

	public void ConfirmedDeleteMessage(bool confirm)
	{
		if (confirm)
		{
			Singleton<HenkSWNotifications>.SP.DeleteSession(selectedInboxMessage.linkedSession.sessionID);
			AudioController.Play("inbox_messageremove");
			InitializeInboxMessages();
		}
	}

	public void ConfirmedOpenMessage(bool confirm)
	{
		if (!confirm)
		{
			AudioController.Play("inbox_messageclose");
			return;
		}
		ToggleInbox(toggle: false);
		StartLevelFromMessage(selectedInboxMessage.linkedSession.level, selectedInboxMessage.linkedSession.theirSteamID);
	}

	public void StartLevelFromMessage(int levelCode, ulong friendID)
	{
		Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode(levelCode);
		Singleton<LevelBatchManager>.SP.lookingAtBatchNum = Singleton<LevelBatchManager>.SP.GetBatchNumFromLevel(levelFromCode);
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.Singleplayer);
		Object.FindObjectOfType<State_PreGame>().friendIDToDownloadInstantly = friendID;
		AudioController.Play("LevelStart");
		if (levelFromCode.isSceneLess)
		{
			Singleton<LevelBatchManager>.SP.LoadLevelSceneless(levelFromCode);
		}
		else
		{
			Singleton<LevelBatchManager>.SP.LoadLevel(levelFromCode.levelCode);
		}
	}

	private void InitializeInboxMessages()
	{
		int num = Singleton<InboxManager>.SP.GetAllMessages().Length;
		int num2 = num - numItemsInInbox;
		if (num2 < 0)
		{
			num2 = 0;
		}
		inboxOffset = Mathf.Clamp(inboxOffset, 0, num2);
		inboxCountLabel.text = Language.Get("NUMMESSAGES", "INBOX").Replace("{X}", num.ToString());
		InboxMessage[] allMessages = Singleton<InboxManager>.SP.GetAllMessages();
		for (int i = 0; i < numItemsInInbox; i++)
		{
			int num3 = i + inboxOffset;
			if (num3 >= allMessages.Length)
			{
				inboxButtons[i].GetComponent<InboxItem>().subjLabel.text = string.Empty;
				inboxButtons[i].GetComponent<InboxItem>().fromLabel.text = string.Empty;
				inboxButtons[i].GetComponent<InboxItem>().unreadSprite.enabled = false;
				inboxButtons[i].GetComponent<InboxItem>().inboxMessage = null;
			}
			else
			{
				InboxMessage inboxMessage = allMessages[num3];
				inboxButtons[i].GetComponent<InboxItem>().inboxMessage = inboxMessage;
				inboxButtons[i].GetComponent<InboxItem>().subjLabel.text = inboxMessage.subject;
				inboxButtons[i].GetComponent<InboxItem>().fromLabel.text = inboxMessage.from;
				inboxButtons[i].GetComponent<InboxItem>().unreadSprite.enabled = inboxMessage.unread;
			}
		}
	}

	public void ShowMedalCount(bool toggle = true)
	{
		medalCountLabel.text = Singleton<LevelBatchManager>.SP.MedalCount().ToString();
		medalCountObjects.GetComponent<UITweener>().Play(toggle);
		if (Singleton<LevelBatchManager>.SP.RainbowMedalCount() > 0)
		{
			medalCountSprite.enabled = false;
			medalCountSpriteRainbow.enabled = true;
		}
		else
		{
			medalCountSprite.enabled = true;
			medalCountSpriteRainbow.enabled = false;
		}
	}

	public void PopUpNotification(string notification)
	{
		notificationQueue.Add(notification);
	}

	private IEnumerator ShowNotification()
	{
		showingNotification = true;
		notificationLabel.text = notificationQueue[0];
		notificationLabel.GetComponent<UITweener>().Play(forward: true);
		yield return new WaitForSeconds(1.5f);
		notificationLabel.GetComponent<UITweener>().Play(forward: false);
		yield return new WaitForSeconds(0.2f);
		notificationLabel.text = string.Empty;
		notificationQueue.RemoveAt(0);
		showingNotification = false;
	}

	public void ToggleChatBox(bool state)
	{
		chatBoxObject.SetActive(state);
	}

	public void ToggleIngameTutorial(bool state)
	{
		if (!optionsGame.showTutorialDuringFirstBatchLevels)
		{
			ingameTutorial.SetActive(value: false);
			ingameTutorialHook.SetActive(value: false);
		}
		else if (state)
		{
			int num = ((!Singleton<LevelBatchManager>.SP.currentLevel) ? Application.loadedLevel : Singleton<LevelBatchManager>.SP.currentLevel.levelCode);
			if (num == 13 || num == 34 || num == 3)
			{
				ingameTutorial.SetActive(value: true);
			}
			else
			{
				ingameTutorial.SetActive(value: false);
			}
			if (num == 8 || num == 15 || num == 48)
			{
				ingameTutorialHook.SetActive(value: true);
			}
			else
			{
				ingameTutorialHook.SetActive(value: false);
			}
		}
		else
		{
			ingameTutorialHook.SetActive(value: false);
			ingameTutorial.SetActive(value: false);
		}
	}

	public void RequestConfirmation(string functionToCall, GameObject requestOwner, string message = "AREYOUSURE")
	{
		if (message == "AREYOUSURE")
		{
			message = Language.Get("AREYOUSURE", "PERMA");
		}
		confirmationAction = functionToCall;
		confirmationRequester = requestOwner;
		confirmationWindow.SetActive(value: true);
		confirmationRequestUp = true;
		confirmationMessage.text = message;
		AudioController.Play("ButtonForwards");
	}

	public void Confirm(bool state)
	{
		if (confirmationRequestUp)
		{
			confirmationWindow.SetActive(value: false);
			confirmationRequester.SendMessage(confirmationAction, state);
			confirmationRequester = null;
			confirmationAction = string.Empty;
			confirmationRequestUp = false;
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			if (!state)
			{
				AudioController.Play("ButtonBackwards");
			}
		}
	}

	public void ToggleArrows(bool state, bool hard = true, ArrowPositions arrowPositions = null)
	{
	}

	public void ToggleCharacterInfo(bool state, string text = "")
	{
	}

	public void ToggleGhostLabels(bool state)
	{
		GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
		foreach (GameObject gameObject in allPlayers)
		{
			if (gameObject.GetComponent<PlayerGraphics>().ghostNameLabel != null)
			{
				gameObject.GetComponent<PlayerGraphics>().ghostNameLabel.ToggleLabel(state);
			}
		}
	}

	public void ToggleBGPanel(bool state, bool removePanel = false)
	{
	}

	public void OnToggleBGPanel()
	{
	}

	public void ToggleBGPanelLeft(bool state, string titleLabelText = "", bool inMainMenu = false)
	{
		if (inMainMenu)
		{
			Singleton<MainMenuHighlighter>.SP.ToggleOnOff(state: false);
		}
		if (state)
		{
			genericBackgroundPanelLeft.SetActive(value: true);
		}
		else
		{
			inMainMenu = false;
		}
		bgLeftPanelTween.Play(state);
		togglingBgPanelLeftIn = state;
	}

	public void ToggleBackForwardsButtons(bool stateBack, bool stateForwards, string backText = "", string forwardsText = "")
	{
		SetBackForwardButtonTexts(backText, forwardsText);
		backButton.transform.parent.gameObject.SetActive(stateBack);
		forwardsButton.transform.parent.gameObject.SetActive(stateForwards);
		if (stateBack || stateForwards)
		{
			bool flag = true;
			if (backButton.transform.parent.parent.gameObject.activeInHierarchy)
			{
				flag = false;
			}
			backButton.transform.parent.parent.gameObject.SetActive(value: true);
			if (flag)
			{
				UITweener component = backButton.transform.parent.parent.GetComponent<UITweener>();
				component.ResetToBeginning();
				component.Play(forward: true);
			}
		}
		else
		{
			backButton.transform.parent.parent.gameObject.SetActive(value: false);
		}
	}

	public void SetBackForwardButtonTexts(string back = "", string forwards = "")
	{
		if (back == string.Empty)
		{
			backButton.text = Language.Get("BACK", "PERMA");
		}
		else
		{
			backButton.text = back;
		}
		if (forwards == string.Empty)
		{
			forwardsButton.text = Language.Get("SELECT", "PERMA");
		}
		else
		{
			forwardsButton.text = forwards;
		}
	}

	public void BGPanelSetTitles(string title, string subtitle)
	{
	}

	public void ToggleSubtitle(bool state)
	{
	}

	public void ToggleSubSubTitle(bool state, string text = "")
	{
	}

	public void ErrorPopup(string text)
	{
		if (!errorPopupVisible)
		{
			StartCoroutine(ErrorPopupRoutine(text));
		}
	}

	private IEnumerator ErrorPopupRoutine(string text)
	{
		errorPopupVisible = true;
		errorPopup.GetComponentInChildren<UILabel>().text = text;
		errorPopup.GetComponent<TweenPosition>().Play(forward: true);
		yield return new WaitForSeconds(3.5f);
		errorPopup.GetComponent<TweenPosition>().Play(forward: false);
		errorPopupVisible = false;
	}

	private void SetFadeState(FadeState state)
	{
		switch (state)
		{
		case FadeState.fadedIn:
			FadeScreen.GetComponent<UISprite>().enabled = false;
			break;
		case FadeState.fadingIn:
			Singleton<InputManager>.SP.inputEnabled = true;
			if (fadeState == FadeState.fadedIn)
			{
				return;
			}
			FadeScreen.GetComponent<UISprite>().enabled = true;
			FadeScreen.Play(forward: true);
			break;
		case FadeState.fadingOut:
			if (fadeState == FadeState.fadedOut)
			{
				return;
			}
			FadeScreen.GetComponent<UISprite>().enabled = true;
			FadeScreen.Play(forward: false);
			break;
		default:
			Debug.LogError("Error: Setting nonexistant fadestate: " + state);
			return;
		case FadeState.fadedOut:
			break;
		}
		fadeState = state;
	}

	public void FadeInOrOut(float seconds, bool fadeIn, float delay = 0f, bool fadeOverGUI = false)
	{
		if (fadeOverGUI)
		{
			FadeScreen.transform.parent.parent.GetComponent<UIPanel>().depth = 13;
		}
		else
		{
			FadeScreen.transform.parent.parent.GetComponent<UIPanel>().depth = -6;
		}
		FadeScreen.duration = seconds;
		if (delay > 0f)
		{
			StartCoroutine(DelayedFadeInOrOut(fadeIn, delay));
		}
		else if (fadeIn)
		{
			SetFadeState(FadeState.fadingIn);
		}
		else
		{
			SetFadeState(FadeState.fadingOut);
		}
	}

	private IEnumerator DelayedFadeInOrOut(bool fadeIn, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (fadeIn)
		{
			SetFadeState(FadeState.fadingIn);
		}
		else
		{
			SetFadeState(FadeState.fadingOut);
		}
	}

	public void OnFadeComplete()
	{
		if (FadeScreen.value == 0f)
		{
			SetFadeState(FadeState.fadedIn);
		}
		else
		{
			SetFadeState(FadeState.fadedOut);
		}
	}

	public void FadeOutAndIn(float fadeDur, float waitDur)
	{
		if (fadeState == FadeState.fadedIn)
		{
			FadeScreen.duration = fadeDur;
			StartCoroutine("FadeRoutine", new FadeRoutineArgs
			{
				waitTime = waitDur + fadeDur
			});
		}
	}

	private IEnumerator FadeRoutine(FadeRoutineArgs args)
	{
		FadeInOrOut(FadeScreen.duration, fadeIn: false);
		yield return new WaitForSeconds(args.waitTime);
		FadeInOrOut(FadeScreen.duration, fadeIn: true);
	}

	public void CancelFade()
	{
		StopCoroutine("FadeRoutine");
		FadeInOrOut(0.25f, fadeIn: true);
	}

	public void TweenComplete()
	{
	}

	public void Button_Quit()
	{
		if (!Object.FindObjectOfType<State_PreGameMultiplayer>().countingDown)
		{
			Singleton<InGameMenu>.SP.ToggleMenu(toggle: false);
			KillCountdownLabels();
			Singleton<MultiManager>.SP.DisconnectFromGame(string.Empty);
			Singleton<Scoreboard>.SP.forceScoreboardVisible = false;
		}
	}

	public void Button_Resume()
	{
		Singleton<InGameMenu>.SP.ToggleMenu(toggle: false);
	}

	public void KillCountdownLabels()
	{
		Object.FindObjectOfType<State_PreGameMultiplayer>().KillCountdownSequence();
		labelReady.text = string.Empty;
		labelGo.text = string.Empty;
		labelsReadyGo[0].enabled = false;
		labelsReadyGo[1].enabled = false;
	}

	public void RedrawAllGUI()
	{
		StartCoroutine(RedrawAllGUIRoutine());
	}

	private IEnumerator RedrawAllGUIRoutine()
	{
		yield return new WaitForEndOfFrame();
		UIPanel[] componentsInChildren = Singleton<GetRoot>.SP.Get().GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetDirty();
		}
	}

	public void ToggleTHPanel(bool state)
	{
		twitchPanel.SetActive(state);
		Singleton<InputManager>.SP.inputEnabled = !state;
	}

	public void SetTHTextLabel(string text)
	{
		twitchTextLabel.text = text;
	}

	public void SetTHStatusLabel(string text)
	{
		if (text == string.Empty)
		{
			twitchStatusLabel.gameObject.SetActive(value: false);
			return;
		}
		twitchStatusLabel.gameObject.SetActive(value: true);
		twitchStatusLabel.text = text;
	}

	public void SetTHButton(string text)
	{
		if (text == string.Empty)
		{
			continueButton.SetActive(value: false);
			return;
		}
		continueButtonText.text = text;
		continueButton.SetActive(value: true);
	}
}
