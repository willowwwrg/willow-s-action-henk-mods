using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;
using yIRC;

public class IRCManager : Singleton<IRCManager>
{
	private string serverName = "irc.twitch.tv";

	private string userName = "ActionHenkBot";

	private string channel = string.Empty;

	private int port = 6667;

	private IrcClient irc;

	public int maxGhosts = 5;

	public IRCConnectionState connectionState;

	private Dictionary<string, string> channelsText = new Dictionary<string, string>();

	public List<ulong> playersToAdd = new List<ulong>();

	public List<string> nicksToAdd = new List<string>();

	public List<GameObject> allSpawnedGhosts = new List<GameObject>();

	public string currentNicknameSpawning = string.Empty;

	public bool updatingNickname;

	private int count;

	private void Update()
	{
		if (irc != null)
		{
			if (irc.IsConnected)
			{
				irc.ListenOnce(blocking: false);
			}
			else if (connectionState == IRCConnectionState.Connected)
			{
				connectionState = IRCConnectionState.Disconnected;
			}
		}
		allSpawnedGhosts.Remove(null);
		bool num = Singleton<HenkSWLeaderboards>.SP.downloadingCustomReplay != 0L || Singleton<HenkSWLeaderboards>.SP.currentlyDownloadingScoresForUserIDs;
		bool flag = Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer && (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PreGame)) || Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGame)) || Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)));
		bool flag2 = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj() != null;
		bool flag3 = Singleton<HenkSWLeaderboards>.SP.GetLevelFromLeaderboardHandle(Singleton<HenkSWLeaderboards>.SP.currentLeaderboardHandle) == Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (!num && flag && flag3 && flag2 && playersToAdd.Count > 0)
		{
			currentNicknameSpawning = nicksToAdd[0];
			GameObject item = Singleton<PlayerManager>.SP.SpawnGhost(GhostType.CustomID, playersToAdd[0]);
			allSpawnedGhosts.Add(item);
			playersToAdd.RemoveAt(0);
			nicksToAdd.RemoveAt(0);
		}
	}

	public bool TrySpawnGhost(ulong ghostID, string nick = "")
	{
		nick = ((!(nick == string.Empty)) ? ("\"" + nick + "\"") : ("ID " + ghostID));
		if (playersToAdd.Contains(ghostID))
		{
			return false;
		}
		GameObject[] allGhosts = Singleton<PlayerManager>.SP.GetAllGhosts();
		for (int i = 0; i < allGhosts.Length; i++)
		{
			if (allGhosts[i].GetComponent<ReplayController>().steamID == ghostID)
			{
				SendIRCMessage("Couldn't spawn ghost for " + nick + ". This ghost is already spawned.");
				return false;
			}
		}
		if (allSpawnedGhosts.Count + playersToAdd.Count >= maxGhosts)
		{
			SendIRCMessage("Couldn't spawn ghost for " + nick + ". There's already a maximum of " + maxGhosts + " spawned right now. Please wait until a level or ghost switch and try again.");
			return false;
		}
		playersToAdd.Add(ghostID);
		nicksToAdd.Add(nick);
		return true;
	}

	private void irc_OnError(object sender, ErrorEventArgs e)
	{
		Debug.LogError(e.ErrorMessage);
	}

	public void Disconnect()
	{
		if (irc != null && irc.IsConnected)
		{
			irc.Disconnect();
		}
	}

	public void Connect(string channelToJoin)
	{
		channel = "#" + channelToJoin.ToLower();
		if (irc != null && irc.IsConnected)
		{
			irc.Disconnect();
		}
		irc = new IrcClient();
		irc.Encoding = Encoding.UTF8;
		irc.SendDelay = 200;
		irc.ActiveChannelSyncing = true;
		irc.OnError += irc_OnError;
		irc.OnRawMessage += irc_OnRawMessage;
		try
		{
			MonoBehaviour.print("Connecting to IRC server");
			irc.Connect(serverName, port);
		}
		catch (ConnectionException ex)
		{
			Debug.LogError("Couldn't connect! Reason: " + ex.Message);
			connectionState = IRCConnectionState.ConnectionFailed;
		}
		if (irc.IsConnected)
		{
			try
			{
				MonoBehaviour.print("Logging in");
				irc.Login(userName, "Action Henk", 0, userName, "oauth:" + Singleton<ActionHenk>.SP.randomString);
				MonoBehaviour.print("Joining channel");
				irc.Join(channel);
				SendIRCMessage("HenkBot is now active! :D Let " + channelToJoin + " compete with your replay ghost by using the following commands: !spawnghost <steam_nickname> or !spawnghost <64bit_steam_id>");
				MonoBehaviour.print("Done!");
				Singleton<GAManager>.SP.IRCConnect(channelToJoin);
				connectionState = IRCConnectionState.Connected;
			}
			catch (ConnectionException ex2)
			{
				Debug.LogError(ex2.Message);
				connectionState = IRCConnectionState.ConnectionFailed;
			}
			catch (Exception ex3)
			{
				Debug.LogError("Error occurred! Message: " + ex3.Message);
				Debug.LogError("Exception: " + ex3.StackTrace);
				connectionState = IRCConnectionState.ConnectionFailed;
			}
		}
	}

	private void irc_OnRawMessage(object sender, IrcEventArgs e)
	{
		string text = ((!string.IsNullOrEmpty(e.Data.Channel) && e.Data.Type == ReceiveType.ChannelMessage) ? e.Data.Channel : "SERVER");
		string text2 = ((!string.IsNullOrEmpty(e.Data.Nick)) ? ("<" + e.Data.Nick + "> " + e.Data.Message) : ("* " + e.Data.Message));
		string key2;
		if (!channelsText.ContainsKey(text))
		{
			channelsText[text] = string.Empty;
		}
		else
		{
			Dictionary<string, string> dictionary = channelsText;
			string key = (key2 = text);
			key2 = dictionary[key2];
			dictionary[key] = key2 + "\r\n";
		}
		Dictionary<string, string> dictionary2 = channelsText;
		string key3 = (key2 = text);
		key2 = dictionary2[key2];
		dictionary2[key3] = key2 + text2;
		if (!(e.Data.Channel == text) || e.Data.Type != ReceiveType.ChannelMessage)
		{
			return;
		}
		string message = e.Data.Message;
		int num = message.IndexOf(' ');
		if (message.Length <= num || num == -1)
		{
			return;
		}
		string text3 = message.Substring(num + 1);
		if (message.StartsWith("!test "))
		{
			irc.SendMessage(SendType.Message, text, "Hey " + e.Data.Nick + "! Thanks for your " + text3 + "!");
		}
		else if (message.StartsWith("!spawnghost "))
		{
			MonoBehaviour.print("Spawning ghost of " + text3);
			ulong result = 0uL;
			if (ulong.TryParse(text3, out result))
			{
				TrySpawnGhost(result, string.Empty);
				MonoBehaviour.print("Attempting ghost download for player ID " + result);
			}
			else
			{
				MonoBehaviour.print("Attempting nickname retrieve for player " + text3);
				SpawnGhostFromNick(text3);
			}
		}
	}

	public void UpdateNickname(string nickname = "", ulong id = 0uL)
	{
		if (nickname == string.Empty)
		{
			nickname = SteamFriends.GetPersonaName();
		}
		if (id == 0L)
		{
			id = SteamUser.GetSteamID().m_SteamID;
		}
		updatingNickname = true;
		StartCoroutine(UpdateNicknameRoutine(nickname, id));
	}

	private IEnumerator UpdateNicknameRoutine(string nickname, ulong id)
	{
		string text = Singleton<PhpMyAdminMan>.SP.Md5Sum(id + nickname + Singleton<PhpMyAdminMan>.SP.GetSecretKey());
		Debug.Log("Posting to database: " + nickname + " with ID " + id);
		string text2 = Singleton<PhpMyAdminMan>.SP.SubmitNickURL();
		string text3 = "?";
		text3 = text3 + "id=" + id + "&";
		text3 = text3 + "name=" + WWW.EscapeURL(nickname) + "&";
		text3 = text3 + "hash=" + text;
		WWW webCall = new WWW(text2 + text3);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error posting nick: " + webCall.error);
		}
		yield return new WaitForSeconds(0.1f);
		updatingNickname = false;
	}

	public void SpawnGhostFromNick(string nickname)
	{
		StartCoroutine(SpawnGhostFromNickRoutine(nickname));
	}

	private IEnumerator SpawnGhostFromNickRoutine(string nickname)
	{
		string text = Singleton<PhpMyAdminMan>.SP.FetchIDFromNickURL();
		string text2 = "?name=" + WWW.EscapeURL(nickname);
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error retrieving ID from nick: " + webCall.error);
			yield break;
		}
		ulong result = 0uL;
		ulong.TryParse(webCall.text, out result);
		switch (result)
		{
		case 0uL:
			MonoBehaviour.print("something went wrong, couldn't retrieve name");
			SendIRCMessage("Sorry, I couldn't find the player ID associated with the nickname \"" + nickname + "\".");
			break;
		case 1uL:
			MonoBehaviour.print("duplicate ID for this name");
			SendIRCMessage("There seem to be multiple players with nickname \"" + nickname + "\". You could try using your 64bit steam ID instead.");
			break;
		default:
			TrySpawnGhost(result, nickname);
			MonoBehaviour.print("Success! Adding ghost ID " + result + " from nick " + nickname);
			break;
		}
	}

	public void SendIRCMessage(string message)
	{
		if (irc != null && irc.IsConnected)
		{
			StartCoroutine(TrySendIRCMessage(message));
		}
	}

	private IEnumerator TrySendIRCMessage(string message)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));
		string text = Singleton<PhpMyAdminMan>.SP.IRCCheckURL();
		string text2 = "?message=" + WWW.EscapeURL(message) + "&channel=" + WWW.EscapeURL(channel);
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error checking IRC message: " + webCall.error);
		}
		else if (webCall.text == "true")
		{
			irc.SendMessage(SendType.Message, channel, message);
		}
		else
		{
			MonoBehaviour.print("Cancelling message send on channel " + channel);
		}
	}

	public void ShowNotification(string notification)
	{
		Singleton<PermaGUI>.SP.PopUpNotification(notification);
	}

	private void OnApplicationQuit()
	{
		StartCoroutine(StopIRC());
	}

	private IEnumerator StopIRC()
	{
		if (irc != null && irc.IsConnected)
		{
			irc.Quit("Application close");
			yield return new WaitForSeconds(1f);
			try
			{
				irc.Disconnect();
			}
			catch (Exception)
			{
			}
		}
	}
}
