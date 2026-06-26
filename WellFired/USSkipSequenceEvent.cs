using UnityEngine;

namespace WellFired;

[USequencerEvent("Sequence/Skip uSequence")]
[USequencerFriendlyName("Skip uSequence")]
[USequencerEventHideDuration]
public class USSkipSequenceEvent : USEventBase
{
	public USSequencer sequence;

	public bool skipToEnd = true;

	public float skipToTime = -1f;

	public override void FireEvent()
	{
		if (!sequence)
		{
			Debug.LogWarning("No sequence for USSkipSequenceEvent : " + base.name, this);
		}
		else if (!skipToEnd && skipToTime < 0f && skipToTime > sequence.Duration)
		{
			Debug.LogWarning("You haven't set the properties correctly on the Sequence for this USSkipSequenceEvent, either the skipToTime is invalid, or you haven't flagged it to skip to the end", this);
		}
		else if (skipToEnd)
		{
			sequence.SkipTimelineTo(sequence.Duration);
		}
		else
		{
			sequence.SkipTimelineTo(skipToTime);
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
	}
}
