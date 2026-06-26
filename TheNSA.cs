using UnityEngine;

public class TheNSA : Singleton<TheNSA>
{
	private int numTriesThisLevel;

	private int numDeathsThisLevel;

	private bool levelCompleted;

	private void Update()
	{
	}

	public void PokeTheNSA(NotificationType type, string args)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		switch (type)
		{
		case NotificationType.LevelStart:
			levelCompleted = false;
			Singleton<GAManager>.SP.LevelStart();
			break;
		case NotificationType.RageQuit:
			if (!levelCompleted)
			{
				Singleton<HenkSWUserStats>.SP.IncrementStat("numRageQuits");
				Singleton<HenkSWUserStats>.SP.StoreStats();
			}
			levelCompleted = false;
			break;
		case NotificationType.MedalScored:
			Singleton<GAManager>.SP.MedalScored(args);
			break;
		case NotificationType.LevelComplete:
		{
			levelCompleted = true;
			Singleton<GAManager>.SP.LevelComplete();
			Singleton<HenkSWUserStats>.SP.IncrementStat("numLevelsCompleted");
			PlayerStats component = Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerStats>();
			Singleton<HenkSWUserStats>.SP.IncrementStatBy_i("cmTraveledSliding", (int)(component.unitsSliding * 1.7f));
			Singleton<HenkSWUserStats>.SP.IncrementStatBy_i("cmTraveledRunning", (int)(component.unitsRunning * 1.7f));
			Singleton<HenkSWUserStats>.SP.IncrementStatBy_f("kmTraveledSliding", component.unitsSliding * 1.7f * 1E-05f);
			Singleton<HenkSWUserStats>.SP.IncrementStatBy_f("kmTraveledRunning", component.unitsRunning * 1.7f * 1E-05f);
			Singleton<HenkSWUserStats>.SP.IncrementStatBy_f("secondsAirTime", component.secondsOfAirtime);
			Singleton<HenkSWUserStats>.SP.IncrementStatBy_f("minutesAirTime", component.secondsOfAirtime / 60f);
			Singleton<HenkSWUserStats>.SP.StoreStats();
			component.ResetStats();
			break;
		}
		case NotificationType.LevelReset:
			Singleton<HenkSWUserStats>.SP.IncrementStat("numResets");
			Singleton<HenkSWUserStats>.SP.StoreStats();
			break;
		case NotificationType.PlayerDeath:
			Singleton<HenkSWUserStats>.SP.IncrementStat("numDeaths");
			Singleton<HenkSWUserStats>.SP.StoreStats();
			break;
		default:
			Debug.LogError("Invalid NSA notificationtype: " + type.ToString() + "-" + args);
			break;
		}
	}
}
