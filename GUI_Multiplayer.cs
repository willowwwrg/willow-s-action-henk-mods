using System.Collections.Generic;
using UnityEngine;

public class GUI_Multiplayer : GUI_Base
{
	public InputObject firstButton;

	private InputObject currentButton;

	public List<GameObject> roomList = new List<GameObject>();

	public UILabel roundDuration;

	public UILabel playerCount;

	public UILabel levelRotation;

	public UILabel players;

	public UILabel serverTitle;

	public UILabel mutators;

	public List<GameObject> buttons;

	public GameObject imagePlane;

	public UILabel statusLabel;

	public UILabel statusLabel2;

	public UITweener bgpanel;

	public UITweener bgpanel2;

	private int scrollOffset;

	private int maxOffset;

	private int numRooms;

	public UISprite topArrow;

	public UISprite botArrow;

	private void TransitionCompleted()
	{
		InitializeScreen();
		scrollOffset = 0;
		numRooms = 0;
		for (int i = 0; i < buttons.Count; i++)
		{
			GameObject obj = buttons[i];
			obj.SetActive(value: true);
			obj.GetComponentInChildren<UILabel>().text = string.Empty;
			obj.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = string.Empty;
		}
		roomList.Clear();
		bgpanel2.ResetToBeginning();
		bgpanel2.enabled = false;
		bgpanel.ResetToBeginning();
		bgpanel.enabled = false;
		Singleton<ChatBox>.SP.Reset();
		if (!Singleton<LevelBatchManager>.SP.workshopLevelsUpToDate)
		{
			Singleton<LevelBatchManager>.SP.RefreshWorkshopLevels();
		}
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MultiplayerPicker));
		AudioController.Play("ButtonBackwards");
		Singleton<MultiManager>.SP.Disconnect();
	}

	private void Update()
	{
		UpdateStatusLabel();
		if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.ConnectingToServer || Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.DownloadingLevels)
		{
			return;
		}
		if (numRooms != PhotonNetwork.GetRoomList().Length)
		{
			numRooms = PhotonNetwork.GetRoomList().Length;
			Button_Refresh();
		}
		if (Singleton<InputManager>.SP.GetCurrentButton() != null)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_6")
			{
				maxOffset = numRooms - 7;
				if (scrollOffset < maxOffset)
				{
					scrollOffset++;
					BuildLobbyList();
				}
				Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true);
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Up) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_0")
			{
				if (scrollOffset > 0)
				{
					scrollOffset--;
					BuildLobbyList();
				}
				if (numRooms > 7)
				{
					Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true);
				}
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm))
			{
				Singleton<InputManager>.SP.ClickCurrentButton();
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.CreateServer))
		{
			if (!PhotonNetwork.insideLobby)
			{
				AudioController.Play("ButtonClick");
				return;
			}
			Button_CreateRoom();
			AudioController.Play("ButtonForwards");
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.InstantAction))
		{
			if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.ConnectedToLobby)
			{
				Singleton<MultiManager>.SP.JoinRandomRoom();
				AudioController.Play("LevelStart");
			}
			else
			{
				AudioController.Play("ButtonClick");
			}
		}
	}

	private void Button_Refresh()
	{
		numRooms = PhotonNetwork.GetRoomList().Length;
		BuildLobbyList();
	}

	private void UpdateStatusLabel()
	{
		statusLabel.text = string.Empty;
		statusLabel2.text = string.Empty;
		if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.ConnectingToLobby)
		{
			statusLabel.text = "Status: " + Language.Get("STATUSCONNECTINGTOMASTER", "MULTIPLAYER");
		}
		else if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.ConnectedToLobby)
		{
			if (PhotonNetwork.GetRoomList().Length == 0)
			{
				statusLabel2.text = Language.Get("STATUSNOSERVERS", "MULTIPLAYER");
			}
		}
		else if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.ConnectingToServer)
		{
			statusLabel.text = "Status: " + Language.Get("STATUSCONNECTINGTOSERVER", "MULTIPLAYER");
		}
		else if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.ConnectedToServer)
		{
			statusLabel.text = "Status: " + Language.Get("STATUSCONNECTEDTOSERVER", "MULTIPLAYER");
		}
		else if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.DownloadingLevels)
		{
			statusLabel.text = "Status: " + string.Format(Language.Get("STATUSDOWNLOADINGLEVELS", "MULTIPLAYER") + " {0}/{1}.", Singleton<Workshop>.SP.GetTotalDownloads() - Singleton<Workshop>.SP.GetDownloadsLeft(), Singleton<Workshop>.SP.GetTotalDownloads());
		}
		else if (Singleton<MultiManager>.SP.multiplayerState == MultiplayerState.Disconnected)
		{
			statusLabel2.text = Language.Get("STATUSDISCONNECTED", "MULTIPLAYER");
		}
		if (statusLabel.text == string.Empty)
		{
			statusLabel.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			statusLabel.transform.parent.gameObject.SetActive(value: true);
		}
	}

	public void Button_CreateRoom()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerRoomCreation, "none");
	}

	public void OnLobbyJoined()
	{
	}

	private void BuildLobbyList()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		for (int i = 0; i < buttons.Count; i++)
		{
			GameObject obj = buttons[i];
			obj.SetActive(value: true);
			obj.GetComponentInChildren<UILabel>().text = string.Empty;
			obj.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = string.Empty;
		}
		roomList.Clear();
		if (!PhotonNetwork.connectedAndReady)
		{
			return;
		}
		numRooms = PhotonNetwork.GetRoomList().Length;
		if (numRooms > 0)
		{
			bgpanel2.Play(forward: true);
			bgpanel.Play(forward: true);
		}
		else
		{
			bgpanel2.Play(forward: false);
			bgpanel.Play(forward: false);
		}
		for (int j = 0; j < buttons.Count; j++)
		{
			GameObject gameObject = buttons[j];
			if (j + scrollOffset <= numRooms - 1)
			{
				gameObject.SetActive(value: true);
				string text = PhotonNetwork.GetRoomList()[j + scrollOffset].name;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = text;
				gameObject.GetComponentInChildren<UILabel>().text = text;
			}
		}
		SetInputObjects();
		if (scrollOffset > 0)
		{
			topArrow.enabled = true;
		}
		else
		{
			topArrow.enabled = false;
		}
		if (scrollOffset > numRooms - 8)
		{
			botArrow.enabled = false;
		}
		else
		{
			botArrow.enabled = true;
		}
	}

	private void SetInputObjects()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			InputObject component = buttons[i].GetComponent<InputObject>();
			component.selectOnDown = (component.selectOnLeft = (component.selectOnRight = (component.selectOnUp = null)));
		}
		for (int j = 0; j < buttons.Count; j++)
		{
			if (buttons[j].GetComponent<UIButtonMessageArguments>().GetStrings()[0] != string.Empty)
			{
				InputObject component2 = buttons[j].GetComponent<InputObject>();
				if (j < buttons.Count - 1 && buttons[j + 1].GetComponent<UIButtonMessageArguments>().GetStrings()[0] != string.Empty)
				{
					component2.selectOnDown = buttons[j + 1].GetComponent<InputObject>();
				}
				if (j > 0)
				{
					component2.selectOnUp = buttons[j - 1].GetComponent<InputObject>();
				}
			}
		}
		if (Singleton<InputManager>.SP.GetCurrentButton() != null)
		{
			if (buttons.Contains(Singleton<InputManager>.SP.GetCurrentButton().gameObject))
			{
				if (Singleton<InputManager>.SP.GetCurrentButton().GetComponentInChildren<UILabel>().text == string.Empty)
				{
					Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
				}
			}
			else
			{
				Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
			}
		}
		else
		{
			Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		}
	}

	public void LobbyItemSelected()
	{
	}

	public string PrintPlayerList(string rawPlayerList)
	{
		string text = string.Empty;
		string[] array = rawPlayerList.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			text = text + (i + 1) + ". " + array[i];
			if (i != array.Length - 1)
			{
				text += "\n";
			}
		}
		return text;
	}

	public void HoverOverLevelSelectButton(GameObject button)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = button.GetComponent<UIButtonMessageArguments>().GetStrings()[0];
		for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
		{
			RoomInfo roomInfo = PhotonNetwork.GetRoomList()[i];
			if (!(text4 == roomInfo.name))
			{
				continue;
			}
			num = roomInfo.playerCount;
			num2 = roomInfo.maxPlayers;
			num3 = ((roomInfo.customProperties["roundduration"] != null) ? ((int)roomInfo.customProperties["roundduration"]) : 0);
			string[] array = ((string)roomInfo.customProperties["mutators"]).Split(',');
			bool flag = false;
			for (int j = 0; j < array.Length; j++)
			{
				if (j > 5)
				{
					if (!flag)
					{
						text3 += ", ...";
						flag = true;
					}
				}
				else
				{
					if (j != 0)
					{
						text3 += ", ";
					}
					text3 += array[j];
				}
			}
			serverTitle.text = roomInfo.name;
			ulong[] array2 = MultiManager.ReadLevelRotationFromString((string)roomInfo.customProperties["levelrotation"]);
			bool flag2 = false;
			for (int k = 0; k < array2.Length; k++)
			{
				if (k > 4)
				{
					if (!flag2)
					{
						text += ", ...";
						flag2 = true;
					}
				}
				else
				{
					if (k != 0)
					{
						text += ", ";
					}
					text += Singleton<LevelBatchManager>.SP.GetLevelNameFromID(array2[k]);
				}
			}
			string[] array3 = ((string)roomInfo.customProperties["playerlist"]).Split('\n');
			bool flag3 = false;
			for (int l = 0; l < array3.Length; l++)
			{
				if (l > 5)
				{
					if (!flag3)
					{
						text2 += ", ...";
						flag3 = true;
					}
				}
				else
				{
					if (l != 0)
					{
						text2 += ", ";
					}
					text2 += array3[l];
				}
			}
		}
		playerCount.text = num + "/" + num2;
		roundDuration.text = num3 / 60 + " min.";
		levelRotation.text = text;
		players.text = text2;
		mutators.text = text3;
	}

	public void LevelButton(object value)
	{
		string text = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetStrings()[0];
		RoomInfo[] array = PhotonNetwork.GetRoomList();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == text)
			{
				Singleton<MultiManager>.SP.JoinNamedRoom(text);
				AudioController.Play("LevelStart");
			}
		}
	}
}
