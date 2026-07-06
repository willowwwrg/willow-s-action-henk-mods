using System;
using System.Collections;
using Steamworks;
using UnityEngine;

public class TwitchManager : Singleton<TwitchManager>
{
	private string scripthost = "http://ragesquid.com/freetostream/";

	private bool hasBeenConnected;

	private bool openPopup;

	private bool fetchingState;

	public TwitchState connectionState;

	private float webCheckInterval = 3f;

	private float webCheckTimeLeft;

	private string twitchAccount = string.Empty;

	private byte[] authTicket;

	private uint ticketLen;

	private int testState;

	public bool justHadPopup;

	public void Initialize()
	{
		Singleton<PermaGUI>.SP.SetTHTextLabel("Loading...");
		Singleton<PermaGUI>.SP.SetTHStatusLabel(string.Empty);
		Singleton<PermaGUI>.SP.SetTHButton(string.Empty);
		authTicket = new byte[256];
		SteamUser.GetAuthSessionTicket(authTicket, 256, out ticketLen);
	}

	private void LateUpdate()
	{
		// ragesquid.com Free To Stream server is no longer available
		// Skip all stream state polling to prevent constant failed web requests
	}

	public void QuitGame(object value)
	{
		if ((bool)value)
		{
			MonoBehaviour.print("Quitting game");
			Singleton<ActionHenk>.SP.exitgame = true;
		}
	}

	public void GetStreamState()
	{
		if (!fetchingState)
		{
			ulong steamID = Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID;
			fetchingState = true;
			StartCoroutine(StreamStateRoutine(steamID));
		}
	}

	private IEnumerator StreamStateRoutine(ulong steamid)
	{
		string text = scripthost + "checkstream.php";
		string text2 = "?steamid=" + steamid;
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error checking stream:" + webCall.error);
			ConnectionFail();
			Singleton<PermaGUI>.SP.SetTHTextLabel("Couldn't connect to the server!\n\nPlease check your internet connection.");
			Singleton<PermaGUI>.SP.SetTHStatusLabel(string.Empty);
			Singleton<PermaGUI>.SP.SetTHButton(string.Empty);
		}
		else
		{
			string text3 = webCall.text;
			Debug.Log("Stream status: " + text3);
			if (text3 == "missing steam id")
			{
				ConnectionFail();
				Singleton<PermaGUI>.SP.SetTHTextLabel("We couldn't find your Steam ID, please make sure you are logged into steam and restart the game.");
				Singleton<PermaGUI>.SP.SetTHStatusLabel(string.Empty);
				Singleton<PermaGUI>.SP.SetTHButton(string.Empty);
			}
			if (text3 == "missing account")
			{
				ConnectionFail();
				openPopup = true;
				Singleton<PermaGUI>.SP.SetTHTextLabel("\nWelcome to Action Henk Free To Stream!\n\nFirst let's verify your Twitch account.");
				Singleton<PermaGUI>.SP.SetTHStatusLabel(string.Empty);
				Singleton<PermaGUI>.SP.SetTHButton("LOGIN");
			}
			else if (text3.StartsWith("not live"))
			{
				ConnectionFail();
				string text4 = string.Empty;
				if (hasBeenConnected)
				{
					text4 = "[FF0000]We lost connection to your stream![-]\n";
				}
				openPopup = false;
				twitchAccount = text3.Substring(9);
				Singleton<PermaGUI>.SP.SetTHTextLabel(text4 + "Please make sure you are live on Twitch and set your game to 'Action Henk'.\n\nIt can take about 30 seconds before we pick it up.");
				Singleton<PermaGUI>.SP.SetTHStatusLabel("Waiting for '" + twitchAccount + "' to go live...");
				Singleton<PermaGUI>.SP.SetTHButton(string.Empty);
				UnityEngine.Object.FindObjectOfType<State_MainMenu>().SetTHStatusBox(string.Empty);
			}
			else if (text3.StartsWith("wrong game"))
			{
				ConnectionFail();
				openPopup = false;
				twitchAccount = text3.Substring(11);
				Singleton<PermaGUI>.SP.SetTHTextLabel("Your stream is live! Please set your game to\n'Action Henk' in your dashboard.");
				Singleton<PermaGUI>.SP.SetTHStatusLabel("Waiting for '" + twitchAccount + "' to have 'Action Henk' selected...");
				Singleton<PermaGUI>.SP.SetTHButton(string.Empty);
				UnityEngine.Object.FindObjectOfType<State_MainMenu>().SetTHStatusBox(string.Empty);
			}
			else if (text3.StartsWith("unlock"))
			{
				openPopup = false;
				twitchAccount = text3.Substring(7);
				if (connectionState != TwitchState.Connected)
				{
					AudioController.Play("Toeter");
					connectionState = TwitchState.Connected;
					hasBeenConnected = true;
					Singleton<PermaGUI>.SP.SetTHTextLabel("You're good to go! Enjoy Action Henk!");
					Singleton<PermaGUI>.SP.SetTHStatusLabel(string.Empty);
					Singleton<PermaGUI>.SP.SetTHButton("Let's go!");
					UnityEngine.Object.FindObjectOfType<State_MainMenu>().SetTHStatusBox("Free to Stream connected to '" + twitchAccount + "'");
				}
			}
		}
		fetchingState = false;
	}

	private string ForceWebText(string inText)
	{
		if (testState == 1)
		{
			inText = "missing steam id";
		}
		else if (testState == 2)
		{
			inText = "missing account";
		}
		else if (testState == 3)
		{
			inText = "not live editortest";
		}
		else if (testState == 4)
		{
			inText = "wrong game editortest";
		}
		else if (testState == 5)
		{
			inText = "unlock editortest";
		}
		return inText;
	}

	private void ConnectionFail()
	{
		if (connectionState == TwitchState.Connected)
		{
			connectionState = TwitchState.ConnectionFailedOnce;
		}
		else if (connectionState == TwitchState.ConnectionFailedOnce)
		{
			connectionState = TwitchState.Disconnected;
		}
	}
}
