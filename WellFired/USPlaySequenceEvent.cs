using UnityEngine;

namespace WellFired;

[USequencerEvent("Sequence/Play uSequence")]
[USequencerFriendlyName("Play uSequence")]
[USequencerEventHideDuration]
public class USPlaySequenceEvent : USEventBase
{
	public USSequencer sequence;

	public bool restartSequencer;

	public override void FireEvent()
	{
		if (!sequence)
		{
			Debug.LogWarning("No sequence for USPlaySequenceEvent : " + base.name, this);
			return;
		}
		if (!Application.isPlaying)
		{
			Debug.LogWarning("Sequence playback controls are not supported in the editor, but will work in game, just fine.");
			return;
		}
		if (!restartSequencer)
		{
			sequence.Play();
			return;
		}
		sequence.RunningTime = 0f;
		sequence.Play();
	}

	public override void ProcessEvent(float deltaTime)
	{
	}
}
