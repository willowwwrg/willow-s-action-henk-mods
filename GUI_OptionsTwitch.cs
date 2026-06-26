using System.Collections;
using UnityEngine;

public class GUI_OptionsTwitch : GUI_Base
{
	public InputObject firstButton;

	public UILabel labelMaxGhosts;

	public UILabel labelChannelName;

	public UILabel labelStatus;

	public GameObject connectButton;

	private string channelName;

	private int maxGhosts;

	private int minmaxGhosts = 1;

	private int maxmaxGhosts = 20;

	private bool inputFieldUp;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		channelName = Singleton<PlayerPrefsManager>.SP.GetString("TWITCH_SERVERNAME", "ChannelName");
		maxGhosts = Singleton<PlayerPrefsManager>.SP.GetInt("TWITCH_MAXGHOSTS", 6);
		FillInSettingsLabels();
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		if (!inputFieldUp)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_OptionsGame));
			AudioController.Play("ButtonBackwards");
		}
	}

	private void Update()
	{
		if (inputFieldUp && Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
		{
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			ToggleInputField(state: false, string.Empty);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Left) && Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "2 maxghosts")
		{
			SetMaxGhosts(forwards: false);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Right) && Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "2 maxghosts")
		{
			SetMaxGhosts();
		}
		if (Singleton<IRCManager>.SP.connectionState == IRCConnectionState.Connected)
		{
			labelStatus.color = Color.green;
		}
		else
		{
			labelStatus.color = Color.red;
		}
		labelStatus.text = Language.Get(Singleton<IRCManager>.SP.connectionState.ToString().ToUpper(), "SETTINGS");
	}

	private void Button_Connect()
	{
		Singleton<IRCManager>.SP.Connect(channelName);
		connectButton.GetComponent<UIButtonMessage>().functionName = "Button_Disconnect";
		connectButton.GetComponentInChildren<UILabel>().text = "Disconnect!";
	}

	private void Button_Disconnect()
	{
		Singleton<IRCManager>.SP.Disconnect();
		connectButton.GetComponent<UIButtonMessage>().functionName = "Button_Connect";
		connectButton.GetComponentInChildren<UILabel>().text = "Connect!";
	}

	private void SetMaxGhosts(bool forwards = true)
	{
		if (forwards)
		{
			maxGhosts++;
		}
		else
		{
			maxGhosts--;
		}
		if (maxGhosts < minmaxGhosts)
		{
			maxGhosts = minmaxGhosts;
		}
		if (maxGhosts > maxmaxGhosts)
		{
			maxGhosts = maxmaxGhosts;
		}
		FillInSettingsLabels();
	}

	private void Button_ChannelName()
	{
		ToggleInputField(state: true, channelName);
	}

	private void FillInSettingsLabels()
	{
		labelChannelName.text = channelName;
		labelMaxGhosts.text = "< " + maxGhosts + " >";
		Singleton<PlayerPrefsManager>.SP.SetString("TWITCH_SERVERNAME", channelName);
		Singleton<PlayerPrefsManager>.SP.SetInt("TWITCH_MAXGHOSTS", maxGhosts);
		Singleton<IRCManager>.SP.maxGhosts = maxGhosts;
	}

	private void ToggleInputField(bool state, string ChannelName = "")
	{
		Singleton<PermaGUI>.SP.inputField.transform.parent.gameObject.SetActive(state);
		Singleton<PermaGUI>.SP.inputFieldTitle.text = Language.Get("NAMECHANNEL", "SETTINGS");
		if (state)
		{
			Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = false;
			Singleton<PermaGUI>.SP.inputField.value = ChannelName;
			StartCoroutine(GiveFocusToInputField());
			inputFieldUp = true;
		}
		else
		{
			inputFieldUp = false;
			Singleton<PermaGUI>.SP.inputField.RemoveFocus();
			Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = true;
		}
	}

	private IEnumerator GiveFocusToInputField()
	{
		yield return new WaitForSeconds(0.1f);
		Singleton<PermaGUI>.SP.inputField.isSelected = true;
	}

	public void ConfirmName(UILabel input)
	{
		if (inputFieldUp)
		{
			channelName = input.text;
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			ToggleInputField(state: false, string.Empty);
			FillInSettingsLabels();
		}
	}
}
