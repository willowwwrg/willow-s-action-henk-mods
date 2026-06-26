using System;

[Serializable]
public class HenkSession
{
	public ulong sessionID;

	public int level;

	public string levelName;

	public long timeCreated;

	public long timeUpdated;

	public SessionState ourState;

	public SessionState theirState;

	public ulong theirSteamID;

	public string TheirSteamName;

	public bool isUnread;

	public int score;

	public override string ToString()
	{
		return string.Format("[Session] ID:{0} -- Level:{1} -- Name:{4}\nLevelName:{2}\nSteamID:{3}\nTheirState:{5}\nOurState:{6}\nScore:{7}\nTimeCreated:{8}\nTimeUpdated:{9}\nIsNewNotification:{10}", sessionID, level, levelName, theirSteamID, TheirSteamName, theirState, ourState, score, timeCreated, timeUpdated, isUnread);
	}
}
