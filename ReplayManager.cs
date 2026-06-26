using System;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : Singleton<ReplayManager>
{
	[NonSerialized]
	public int replayVersion = 6;

	public GameObject replayNameLabelObject;

	public void RefreshCurrentLevelLocalReplay()
	{
		GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
		for (int i = 0; i < allPlayers.Length; i++)
		{
			allPlayers[i].GetComponent<ReplayController>().reloadReplay = true;
		}
	}

	public void CopyRecordedDataToController()
	{
		ReplayController component = Singleton<PlayerManager>.SP.GetPlayer().GetComponent<ReplayController>();
		component.Stop();
		component.replay = new List<ReplayFrame>(Singleton<ReplayRecorder>.SP.replay);
		component.snapshots = new List<SnapshotFrame>(Singleton<ReplayRecorder>.SP.snapshots);
		component.snapshotRate = Singleton<ReplayRecorder>.SP.snapshotRate;
		component.Initialized();
	}
}
