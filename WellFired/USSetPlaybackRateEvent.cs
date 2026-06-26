using UnityEngine;

namespace WellFired;

[USequencerEvent("Sequence/Set Playback Rate")]
[USequencerEventHideDuration]
[USequencerFriendlyName("Set uSequence Playback Rate")]
public class USSetPlaybackRateEvent : USEventBase
{
	public USSequencer sequence;

	public float playbackRate = 1f;

	private float prevPlaybackRate = 1f;

	public override void FireEvent()
	{
		if (!sequence)
		{
			Debug.LogWarning("No sequence for USSetPlaybackRate : " + base.name, this);
		}
		if ((bool)sequence)
		{
			prevPlaybackRate = sequence.PlaybackRate;
			sequence.PlaybackRate = playbackRate;
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		if ((bool)sequence)
		{
			sequence.PlaybackRate = prevPlaybackRate;
		}
	}
}
