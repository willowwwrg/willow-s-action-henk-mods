using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Steamworks;
using UnityEngine;

public class HenkSWNotifications : Singleton<HenkSWNotifications>
{
	private bool initialized;

	protected Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;

	public string serverLocation = "http://www.ragesquid.com/scripts/sessions/";

	public bool requestingNotification;

	public bool retrievingSessions;

	public int numSessionsBeingSent;

	public ulong steamID;

	public string steamName = "OurSteamName";

	public List<HenkSession> allSessions = new List<HenkSession>();

	private long lastUpdateTime;

	public int minUpdateInterval = 300;

	private float sessionUpdateInterval = 60f;

	private float timeLeftTillSessionUpdate = 60f;

	private byte[] m_Ticket;

	private uint m_pcbTicket;

	private HAuthTicket m_HAuthTicket;

	public SendNotificationOptions sendNotificationsOptions;

	private ulong launchParamSession;

	public bool Initialize()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		if (initialized)
		{
			return true;
		}
		initialized = true;
		steamID = SteamUser.GetSteamID().m_SteamID;
		steamName = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(steamID);
		lastUpdateTime = Singleton<PlayerPrefsManager>.SP.GetLong("lastNotificationCheck", 0L);
		ulong.TryParse(SteamApps.GetLaunchQueryParam("_sessionid"), out launchParamSession);
		FetchAllSessions(onlyGetChangesSinceLastCheck: false, forceCheck: true);
		return true;
	}

	private void Update()
	{
		if (!initialized)
		{
			return;
		}
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_MainMenu)) && Singleton<GamestateManager>.SP.CurrentState.GetComponent<State_MainMenu>().initialized && launchParamSession != 0L)
		{
			HenkSession session = GetSession(launchParamSession);
			if (session != null)
			{
				launchParamSession = 0uL;
				session.isUnread = false;
				Singleton<PermaGUI>.SP.StartLevelFromMessage(session.level, session.theirSteamID);
			}
		}
		Input.GetKeyDown(KeyCode.LeftShift);
		Input.GetKeyDown(KeyCode.RightShift);
	}

	public static string ByteArrayToHexString(byte[] ba)
	{
		return BitConverter.ToString(ba).Replace("-", string.Empty);
	}

	public HenkSession GetSession(ulong sessionID)
	{
		foreach (HenkSession allSession in allSessions)
		{
			if (allSession.sessionID == sessionID)
			{
				return allSession;
			}
		}
		return null;
	}

	public HenkSession[] GetAllSessions(bool onlyOpenChallenges = false)
	{
		if (!onlyOpenChallenges)
		{
			return allSessions.ToArray();
		}
		List<HenkSession> list = new List<HenkSession>();
		foreach (HenkSession allSession in allSessions)
		{
			if (allSession.ourState != SessionState.Waiting)
			{
				list.Add(allSession);
			}
		}
		return list.ToArray();
	}

	public int GetNumberOfNotifications()
	{
		int num = 0;
		foreach (HenkSession allSession in allSessions)
		{
			if (allSession.isUnread)
			{
				num++;
			}
		}
		return num;
	}

	public void FetchAllSessions(bool onlyGetChangesSinceLastCheck = true, bool forceCheck = false)
	{
		// ragesquid.com server is no longer available - skip web calls
	}

	private IEnumerator FetchAllSessionsWebcall(bool onlyGetChangesSinceLastCheck)
	{
		while (!Singleton<ActionHenk>.SP.fetchedGameSettings)
		{
			yield return new WaitForEndOfFrame();
		}
		float startTime = Time.time;
		string text = serverLocation + "getsessions.php?";
		string text2 = "steamid=" + steamID;
		if (onlyGetChangesSinceLastCheck)
		{
			text2 = text2 + "&since=" + lastUpdateTime;
		}
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error retrieving sessions: " + webCall.error);
		}
		else
		{
			int num = 0;
			JSONNode jSONNode = JSON.Parse(webCall.text);
			if (!onlyGetChangesSinceLastCheck)
			{
				allSessions.Clear();
			}
			foreach (JSONNode item in jSONNode["response"]["sessions"].AsArray)
			{
				bool flag = false;
				ulong asULong = item["sessionid"].AsULong;
				HenkSession henkSession = GetSession(asULong);
				if (henkSession == null)
				{
					henkSession = new HenkSession();
				}
				else
				{
					flag = true;
				}
				henkSession.level = item["context"].AsInt;
				henkSession.sessionID = item["sessionid"].AsULong;
				henkSession.levelName = Singleton<LevelBatchManager>.SP.GetLevelName(henkSession.level);
				henkSession.timeCreated = item["time_created"].AsLong;
				henkSession.timeUpdated = item["time_updated"].AsLong;
				henkSession.score = -1;
				foreach (JSONNode item2 in item["user_status"].AsArray)
				{
					if (item2["steamid"].AsULong == steamID)
					{
						henkSession.ourState = ParseState(item2["state"]);
						continue;
					}
					henkSession.theirSteamID = item2["steamid"].AsULong;
					henkSession.theirState = ParseState(item2["state"]);
					henkSession.TheirSteamName = Singleton<HenkSWUserStats>.SP.GetNameBySteamID(henkSession.theirSteamID);
				}
				if (henkSession.timeUpdated > lastUpdateTime && henkSession.ourState != SessionState.Waiting)
				{
					henkSession.isUnread = true;
				}
				if (!flag)
				{
					allSessions.Insert(0, henkSession);
				}
				num++;
			}
			float time = Time.time - startTime;
			Singleton<GAManager>.SP.GetAllSessions(time);
			if (!onlyGetChangesSinceLastCheck)
			{
				Singleton<GAManager>.SP.NumberOfSessions(num);
			}
			MonoBehaviour.print("fetched " + num + " sessions in: " + time.ToString("N2") + "s. total sessions: " + allSessions.Count + ". notifications: " + GetNumberOfNotifications());
			lastUpdateTime = HenkUtils.GetUnixTimestamp();
			Singleton<PlayerPrefsManager>.SP.SetLong("lastNotificationCheck", lastUpdateTime);
		}
		retrievingSessions = false;
	}

	public SessionState ParseState(string state)
	{
		switch (state)
		{
		case "ready":
			return SessionState.Ready;
		case "waiting":
			return SessionState.Waiting;
		case "done":
			return SessionState.Ready;
		case "Unknown":
			return SessionState.Ready;
		default:
			Debug.LogWarning("Couldn't parse session state " + state);
			return SessionState.Ready;
		}
	}

	public void UpdateSession(HenkSession session, bool create = false)
	{
		if (!initialized)
		{
			return;
		}
		if (session.ourState != SessionState.Waiting)
		{
			Debug.LogWarning("Cant send session update if we're not the ones that need to go to waiting");
			return;
		}
		RequestNotificationsPopup();
		session.timeUpdated = HenkUtils.GetUnixTimestamp();
		if (create)
		{
			session.timeCreated = HenkUtils.GetUnixTimestamp();
		}
		numSessionsBeingSent++;
		StartCoroutine(UpdateSessionWebcall(session, create));
	}

	private IEnumerator UpdateSessionWebcall(HenkSession session, bool createNew)
	{
		string text = serverLocation + "updatesession.php?";
		string empty = string.Empty;
		empty = empty + "createsession=" + ((!createNew) ? "false" : "true") + "&";
		empty = empty + "challengerid=" + steamID + "&";
		empty = empty + "challengername=" + WWW.EscapeURL(steamName) + "&";
		empty = empty + "challengedid=" + session.theirSteamID + "&";
		empty = empty + "challengedname=" + WWW.EscapeURL(session.TheirSteamName) + "&";
		empty = empty + "scorestring=" + WWW.EscapeURL(Singleton<HighscoreManager>.SP.ConvertTimeIntToString(session.score)) + "&";
		empty = empty + "levelname=" + WWW.EscapeURL(session.levelName) + "&";
		empty = empty + "level=" + session.level + "&";
		empty = empty + "sessionid=" + session.sessionID;
		WWW webCall = new WWW(text + empty);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error sending session: " + webCall.error);
			if (createNew)
			{
				Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.ChallengeFriend);
			}
		}
		else if (createNew)
		{
			ulong asULong = JSON.Parse(webCall.text)["response"]["sessionid"].AsULong;
			Debug.Log("Succesfully sent session: " + asULong);
			session.sessionID = asULong;
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.ChallengeFriend);
			Singleton<GAManager>.SP.SentSession(session.level);
		}
		else
		{
			Debug.Log("Succesfully updated session");
		}
		numSessionsBeingSent--;
		if (numSessionsBeingSent < 0)
		{
			Debug.LogWarning("Number of sessions being sent < 0, something got messed up");
			numSessionsBeingSent = 0;
		}
	}

	public void BeatPlayer(ulong playerID, string playerName, int score, int levelID)
	{
		if (sendNotificationsOptions == SendNotificationOptions.Never || !initialized)
		{
			return;
		}
		foreach (HenkSession allSession in allSessions)
		{
			if (allSession.theirSteamID == playerID && allSession.level == levelID)
			{
				if (allSession.ourState != SessionState.Waiting)
				{
					MonoBehaviour.print("Updating session");
					allSession.score = score;
					allSession.theirState = SessionState.Ready;
					allSession.ourState = SessionState.Waiting;
					allSession.TheirSteamName = playerName;
					UpdateSession(allSession);
				}
				return;
			}
		}
		if (sendNotificationsOptions != SendNotificationOptions.OnlyWhenAlreadyChallenged)
		{
			MonoBehaviour.print("Creating session");
			HenkSession henkSession = new HenkSession();
			henkSession.level = levelID;
			henkSession.levelName = Singleton<LevelBatchManager>.SP.GetLevelName(henkSession.level);
			henkSession.theirState = SessionState.Ready;
			henkSession.ourState = SessionState.Waiting;
			henkSession.theirSteamID = playerID;
			henkSession.TheirSteamName = playerName;
			henkSession.score = score;
			allSessions.Add(henkSession);
			UpdateSession(henkSession, create: true);
		}
	}

	public void RequestNotificationsPopup()
	{
		if (initialized && !requestingNotification && Singleton<PlayerPrefsManager>.SP.GetInt("SentNotificationPopup", 0) != 1)
		{
			Singleton<PlayerPrefsManager>.SP.SetInt("SentNotificationPopup", 1);
			MonoBehaviour.print("Requesting notifications popup");
			requestingNotification = true;
			StartCoroutine(RequestNotificationsWebcall());
		}
	}

	private IEnumerator RequestNotificationsWebcall()
	{
		string text = serverLocation + "registernotifications.php?";
		string text2 = "steamid=" + steamID;
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error sending notification popup: " + webCall.error);
		}
		else
		{
			JSONNode jSONNode = JSON.Parse(webCall.text);
			bool asBool = jSONNode["response"]["was_created"].AsBool;
			bool asBool2 = jSONNode["response"]["allow_notifications"].AsBool;
			Debug.Log("Notifications requested. Showed popup: " + asBool + ". Allowed notifications: " + asBool2);
			Singleton<GAManager>.SP.RequestNotifications(asBool, asBool2);
		}
		requestingNotification = false;
	}

	public void MarkNotificationsAsRead()
	{
		if (!initialized)
		{
			return;
		}
		foreach (HenkSession allSession in allSessions)
		{
			allSession.isUnread = false;
		}
	}

	public void DeleteSession(ulong sessionID)
	{
		if (initialized)
		{
			HenkSession session = GetSession(sessionID);
			if (session != null)
			{
				allSessions.Remove(session);
			}
			StartCoroutine(DeleteSessionWebcall(sessionID));
		}
	}

	private IEnumerator DeleteSessionWebcall(ulong sessionID)
	{
		string text = serverLocation + "deletesession.php?";
		string text2 = "sessionid=" + sessionID;
		MonoBehaviour.print("Sending: " + text + text2);
		WWW webCall = new WWW(text + text2);
		yield return webCall;
		if (webCall.error != null)
		{
			Debug.LogError("Error deleting session: " + webCall.error);
		}
		else
		{
			Debug.Log("Deleted session " + sessionID);
		}
	}
}
