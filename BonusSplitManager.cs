using System.Collections.Generic;
using System.Globalization;
using Henk;
using UnityEngine;

public class BonusSplitManager : Singleton<BonusSplitManager>
{
	// Stable index -> best time this session
	private Dictionary<int, float> pbCoinTimes = new Dictionary<int, float>();

	// Stable index -> ghost time this session
	private Dictionary<int, float> ghostCoinTimes = new Dictionary<int, float>();

	// Coins player collected before ghost reached them (stable index -> local time)
	private Dictionary<int, float> pendingLocalTimes = new Dictionary<int, float>();

	// Instance ID -> stable index, built at level load
	private Dictionary<int, int> instanceToIndex = new Dictionary<int, int>();

	private int currentLevelCode = -1;
	private bool hasGhost = false;
	private int coinsCollectedThisRun = 0;

	public int SplitFrequency { get; private set; }

	private const string FreqKey = "BonusSplit_Frequency";

	private void Awake()
	{
		SplitFrequency = PlayerPrefs.GetInt(FreqKey, 1);
		CleanupLegacyPlayerPrefs();
	}

	private void CleanupLegacyPlayerPrefs()
	{
		for (int levelCode = 0; levelCode <= 200; levelCode++)
		{
			string prefix = "BonusSplit_PB_" + levelCode;
			string countKey = prefix + "_count";
			if (!PlayerPrefs.HasKey(countKey))
				continue;
			int count = PlayerPrefs.GetInt(countKey);
			for (int i = 0; i < count; i++)
			{
				PlayerPrefs.DeleteKey(prefix + "_id_" + i);
				PlayerPrefs.DeleteKey(prefix + "_t_" + i);
			}
			PlayerPrefs.DeleteKey(countKey);
		}
		PlayerPrefs.Save();
	}

	public void CycleSplitFrequency()
	{
		SplitFrequency = (SplitFrequency + 1) % 6;
		PlayerPrefs.SetInt(FreqKey, SplitFrequency);
		PlayerPrefs.Save();
	}

	public void OnLevelLoad()
	{
		Level level = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (level == null || level.levelType != LevelType.Bonus)
		{
			currentLevelCode = -1;
			return;
		}
		// If switching to a different level, clear PB too
		if (level.levelCode != currentLevelCode)
		{
			pbCoinTimes.Clear();
		}
		// Always rebuild the index — instance IDs change on every scene load
		instanceToIndex.Clear();
		BuildCoinIndex();
		currentLevelCode = level.levelCode;
		hasGhost = Singleton<PlayerManager>.SP.ghostSet && Singleton<PlayerManager>.SP.ghostType != GhostType.None;
		// Reset per-attempt state but keep session PB and ghost times
		coinsCollectedThisRun = 0;
		pendingLocalTimes.Clear();
	}

	public void OnLevelUnload()
	{
		pbCoinTimes.Clear();
		ghostCoinTimes.Clear();
		pendingLocalTimes.Clear();
		instanceToIndex.Clear();
		currentLevelCode = -1;
	}

	public void ClearGhostTimes()
	{
		ghostCoinTimes.Clear();
		pendingLocalTimes.Clear();
		pbCoinTimes.Clear();
		coinsCollectedThisRun = 0;
		hasGhost = false;
	}

	private void BuildCoinIndex()
	{
		Collectable[] coins = Object.FindObjectsOfType<Collectable>();
		System.Array.Sort(coins, (a, b) =>
		{
			Vector3 pa = a.transform.position;
			Vector3 pb = b.transform.position;
			int cx = pa.x.CompareTo(pb.x);
			if (cx != 0) return cx;
			int cy = pa.y.CompareTo(pb.y);
			if (cy != 0) return cy;
			return pa.z.CompareTo(pb.z);
		});
		for (int i = 0; i < coins.Length; i++)
			instanceToIndex[coins[i].gameObject.GetInstanceID()] = i;
	}

	public void LoadGhostCoinTimes(Dictionary<int, float> indexedTimes)
	{
		ghostCoinTimes.Clear();
		foreach (KeyValuePair<int, float> kvp in indexedTimes)
			ghostCoinTimes[kvp.Key] = kvp.Value;
		hasGhost = true;
	}

	public Dictionary<int, float> GetPBCoinTimesForReplay()
	{
		return pbCoinTimes;
	}

	public void RecordLocalCoin(int coinInstanceID, float currentTime)
	{
		if (currentLevelCode == -1 || SplitFrequency == 0)
			return;
		if (!instanceToIndex.ContainsKey(coinInstanceID))
			return;

		int stableIndex = instanceToIndex[coinInstanceID];
		coinsCollectedThisRun++;
		bool showThisCoin = (coinsCollectedThisRun % SplitFrequency == 0);

		float comparisonTime = -1f;
		if (ghostCoinTimes.ContainsKey(stableIndex))
			comparisonTime = ghostCoinTimes[stableIndex];
		else if (pbCoinTimes.ContainsKey(stableIndex))
			comparisonTime = pbCoinTimes[stableIndex];

		if (comparisonTime >= 0f)
		{
			if (showThisCoin)
				ShowSplit(comparisonTime - currentTime);
		}

		// Update session PB
		if (!pbCoinTimes.ContainsKey(stableIndex) || currentTime < pbCoinTimes[stableIndex])
			pbCoinTimes[stableIndex] = currentTime;
	}

	public void RecordGhostCoin(int coinInstanceID, float ghostTime)
	{
		if (currentLevelCode == -1)
			return;
		if (!instanceToIndex.ContainsKey(coinInstanceID))
			return;

		int stableIndex = instanceToIndex[coinInstanceID];
		if (ghostCoinTimes.ContainsKey(stableIndex))
			return;

		ghostCoinTimes[stableIndex] = ghostTime;

		if (pendingLocalTimes.ContainsKey(stableIndex))
		{
			float localTime = pendingLocalTimes[stableIndex];
			ShowSplit(ghostTime - localTime);
			pendingLocalTimes.Remove(stableIndex);
		}
	}

	private void ShowSplit(float delta)
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() == GUIManager.GUIScreens.GUIScreen_InGame)
		{
			Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGame>().ShowCheckpointTime(delta);
		}
	}
}
