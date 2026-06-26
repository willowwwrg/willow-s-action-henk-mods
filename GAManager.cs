using UnityEngine;

public class GAManager : Singleton<GAManager>
{
	private float lastTimestamp;

	private int frameCount;

	private float totalMS;

	private int msCount;

	private float genericUpdateInterval = 60f;

	private float lastUpdate;

	public void Update()
	{
		frameCount++;
		totalMS += Time.deltaTime;
		msCount++;
		if (Time.time - lastUpdate > genericUpdateInterval && !Application.isEditor)
		{
			GenericUpdate();
		}
	}

	private void GenericUpdate()
	{
		lastUpdate = Time.time;
		string text = Singleton<GamestateManager>.SP.State.GetType().ToString();
		GA.API.Design.NewEvent("Generic:GameState:" + text);
		if (HenkUtils.IsInALevel())
		{
			Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
			int levelCode = currentLevelObj.levelCode;
			if (levelCode != 0)
			{
				GA.API.Design.NewEvent("Generic:CurrentLevel:" + levelCode);
			}
			string text2 = string.Concat(Singleton<CharacterSelect>.SP.GetSelectedCharacter(), "_", Singleton<CharacterSelect>.SP.GetSelectedSkin());
			string characterName = Singleton<UnlockManager>.SP.GetCharacterName(Singleton<CharacterSelect>.SP.GetSelectedCharacter(), Singleton<CharacterSelect>.SP.GetSelectedSkin());
			GA.API.Design.NewEvent("Generic:CurrentSkin:" + text2 + " (" + characterName + ")");
			LevelType levelType = currentLevelObj.levelType;
			GA.API.Design.NewEvent("Generic:CurrentLevelType:" + levelType);
			LevelStyle levelStyle = currentLevelObj.levelStyle;
			GA.API.Design.NewEvent("Generic:CurrentLevelStyle:" + levelStyle);
		}
	}

	public void LevelStart()
	{
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		string text = "level_" + currentLevelObj.levelCode + "_" + currentLevelObj.levelVersion + ":_Started";
		if (PlayerPrefs.GetInt(text, 0) != 1)
		{
			PlayerPrefs.SetInt(text, 1);
			GA.API.Design.NewEvent(text);
		}
		text = "level_" + currentLevelObj.levelCode + "_" + currentLevelObj.levelVersion + ":_Loaded";
		GA.API.Design.NewEvent(text);
		ResetAverageCounter();
	}

	public void LevelComplete()
	{
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		string text = "level_" + currentLevelObj.levelCode + "_" + currentLevelObj.levelVersion + ":_Completed";
		if (PlayerPrefs.GetInt(text, 0) != 1)
		{
			PlayerPrefs.SetInt(text, 1);
			GA.API.Design.NewEvent(text);
		}
		SubmitAverageFPS();
	}

	public void LevelLoadTime(int levelNum, float loadTime)
	{
		GA.API.Design.NewEvent("loadtimeLevel:" + levelNum, loadTime);
	}

	public void MedalScored(string medal)
	{
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		string text = "level_" + currentLevelObj.levelCode + "_" + currentLevelObj.levelVersion + ":_";
		switch (medal)
		{
		case "Bronze":
			SetMedalStat(text + "Bronze");
			break;
		case "Silver":
			SetMedalStat(text + "Bronze");
			SetMedalStat(text + "Silver");
			break;
		case "Gold":
			SetMedalStat(text + "Bronze");
			SetMedalStat(text + "Silver");
			SetMedalStat(text + "Gold");
			break;
		case "Rainbow":
			SetMedalStat(text + "Bronze");
			SetMedalStat(text + "Silver");
			SetMedalStat(text + "Gold");
			SetMedalStat(text + "Rainbow");
			break;
		}
	}

	private void SetMedalStat(string medalString)
	{
		if (PlayerPrefs.GetInt(medalString, 0) != 1)
		{
			PlayerPrefs.SetInt(medalString, 1);
			GA.API.Design.NewEvent(medalString);
		}
	}

	private void SubmitAverageFPS()
	{
		float num = Time.time - lastTimestamp;
		float eventValue = (float)frameCount / num;
		float num2 = totalMS / (float)msCount * 1000f;
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (currentLevelObj != null)
		{
			MonoBehaviour.print(currentLevelObj.levelName + " ms: " + num2.ToString("N2") + " fps: " + (1000f / num2).ToString("N2"));
		}
		if (!Application.isEditor)
		{
			int currentLevel = Singleton<LevelBatchManager>.SP.GetCurrentLevel();
			GA.API.Design.NewEvent("AverageLevelFPS:" + currentLevel, eventValue);
			GA.API.Design.NewEvent("AverageLevelMS:" + currentLevel, num2);
			string text = Camera.main.GetComponent<CameraEffectsManager>().levelStyle.ToString();
			GA.API.Design.NewEvent("AverageAreaFPS:" + text, eventValue);
			GA.API.Design.NewEvent("AverageAreaMS:" + text, num2);
			ResetAverageCounter();
		}
	}

	public void ResetAverageCounter()
	{
		lastTimestamp = Time.time;
		frameCount = 0;
		totalMS = 0f;
		msCount = 0;
	}

	public void SubmitFriendLeaderboardLength(int level, int length)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("FriendLeaderboardLength:" + level, length);
		}
	}

	public void GetAllSessions(float time)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("Sessions:GetAllSessions", time);
		}
	}

	public void NumberOfSessions(int number)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("Sessions:NumberOfSessions", number);
		}
	}

	public void SentSession(int levelNumber)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("Sessions:SentSession:" + levelNumber);
		}
	}

	public void RequestNotifications(bool showedPopup, bool allowNotifications)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("Sessions:ShowedPopup:" + showedPopup);
			GA.API.Design.NewEvent("Sessions:AllowedNotifications:" + allowNotifications);
		}
	}

	public void MultiplayerEvent(string eventName)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("Multiplayer:Events:" + eventName);
		}
	}

	public void IRCConnect(string channel)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("IRC:Connect:" + channel);
		}
	}

	public void IRCSpawn(string ghostName)
	{
		if (!Application.isEditor)
		{
			GA.API.Design.NewEvent("IRCSpawn:" + ghostName);
		}
	}
}
