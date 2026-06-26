using System.Collections;
using UnityEngine;

public class ChatBox : Singleton<ChatBox>
{
	private bool chatBoxOpen;

	public UIInput inputField;

	public UITextList textList;

	public UISprite chatBoxBG;

	public string textColor = "[DDDDDD]";

	private string infoString = "[888888]Press [T] to chat[-]";

	private void Start()
	{
		Reset();
	}

	private void Update()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.Multiplayer)
		{
			return;
		}
		if (chatBoxOpen)
		{
			chatBoxBG.GetComponent<TweenAlpha>().Play(forward: true);
		}
		else
		{
			chatBoxBG.GetComponent<TweenAlpha>().Play(forward: false);
		}
		if (!HenkUtils.IsInALevel())
		{
			return;
		}
		if (Input.GetKeyUp(KeyCode.T))
		{
			if (chatBoxOpen)
			{
				return;
			}
			chatBoxOpen = true;
			inputField.value = string.Empty;
			inputField.isSelected = true;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			StartCoroutine(ChatBoxCloseDelay());
			inputField.isSelected = false;
			inputField.value = infoString;
		}
	}

	public void Reset()
	{
		textList.Clear();
		inputField.value = infoString;
		inputField.GetComponent<UILabel>().supportEncoding = true;
		AddSystemMessageToChatbox("Hold [TAB] to show leaderboard.");
	}

	public bool IsChatBoxOpen()
	{
		return chatBoxOpen;
	}

	public void OnSubmit()
	{
		StartCoroutine(ChatBoxCloseDelay());
		inputField.isSelected = false;
		if (inputField.value == string.Empty)
		{
			inputField.value = infoString;
			return;
		}
		string empty = string.Empty;
		empty += inputField.value;
		Singleton<MultiManager>.SP.photonView.RPC("SendChatMessage", PhotonTargets.All, empty);
		inputField.value = infoString;
	}

	private IEnumerator ChatBoxCloseDelay()
	{
		yield return new WaitForEndOfFrame();
		chatBoxOpen = false;
	}

	public void AddSystemMessageToChatbox(string message)
	{
		textList.Add("[fdff35]" + message + "[-]");
	}

	public void AddMessageToChatbox(string message, PhotonMessageInfo messageInfo)
	{
		textList.Add("[01bae5]" + messageInfo.sender.name + ":[-] " + textColor + message + "[-]");
	}
}
