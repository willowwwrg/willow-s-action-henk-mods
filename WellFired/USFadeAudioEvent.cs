using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerFriendlyName("Fade Audio")]
[USequencerEvent("Audio/Fade Audio")]
public class USFadeAudioEvent : USEventBase
{
	private float previousVolume = 1f;

	public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	public void Update()
	{
		base.Duration = fadeCurve.length;
	}

	public override void FireEvent()
	{
		AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
		if (!component)
		{
			Debug.LogWarning("Trying to fade audio on an object without an AudioSource");
		}
		else
		{
			previousVolume = component.volume;
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
		AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
		if (!component)
		{
			Debug.LogWarning("Trying to fade audio on an object without an AudioSource");
		}
		else
		{
			component.volume = fadeCurve.Evaluate(deltaTime);
		}
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
		if (!component)
		{
			Debug.LogWarning("Trying to fade audio on an object without an AudioSource");
		}
		else
		{
			component.volume = previousVolume;
		}
	}
}
