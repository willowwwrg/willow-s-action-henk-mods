using UnityEngine;

namespace WellFired;

public class AlterPlaybackForASequence : MonoBehaviour
{
	public USSequencer sequence;

	private float runningTime;

	private void Update()
	{
		runningTime += Time.deltaTime;
		if ((bool)sequence && !(runningTime > 5f))
		{
			sequence.PlaybackRate -= Time.deltaTime * 1f;
		}
	}
}
