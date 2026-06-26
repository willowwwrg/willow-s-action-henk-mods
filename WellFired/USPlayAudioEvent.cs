using UnityEngine;

namespace WellFired;

[USequencerEvent("Audio/Play Audio")]
[USequencerFriendlyName("Play Audio")]
[USequencerEventHideDuration]
public class USPlayAudioEvent : USEventBase
{
	public AudioClip audioClip;

	public bool loop;

	private bool wasPlaying;

	public void Update()
	{
		if (!loop && (bool)audioClip)
		{
			base.Duration = audioClip.length;
		}
		else
		{
			base.Duration = -1f;
		}
	}

	public override void FireEvent()
	{
		AudioSource audioSource = base.AffectedObject.GetComponent<AudioSource>();
		if (!audioSource)
		{
			audioSource = base.AffectedObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}
		if (audioSource.clip != audioClip)
		{
			audioSource.clip = audioClip;
		}
		audioSource.time = 0f;
		audioSource.loop = loop;
		if (base.Sequence.IsPlaying)
		{
			audioSource.Play();
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
		AudioSource audioSource = base.AffectedObject.GetComponent<AudioSource>();
		if (!audioSource)
		{
			audioSource = base.AffectedObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}
		if (audioSource.clip != audioClip)
		{
			audioSource.clip = audioClip;
		}
		if (!audioSource.isPlaying)
		{
			audioSource.time = deltaTime;
			if (base.Sequence.IsPlaying && !audioSource.isPlaying)
			{
				audioSource.Play();
			}
		}
	}

	public override void ManuallySetTime(float deltaTime)
	{
		AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
		if ((bool)component)
		{
			component.time = deltaTime;
		}
	}

	public override void ResumeEvent()
	{
		AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
		if ((bool)component)
		{
			component.time = base.Sequence.RunningTime - base.FireTime;
			if (wasPlaying)
			{
				component.Play();
			}
		}
	}

	public override void PauseEvent()
	{
		AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
		wasPlaying = false;
		if ((bool)component && component.isPlaying)
		{
			wasPlaying = true;
		}
		if ((bool)component)
		{
			component.Pause();
		}
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void EndEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		if ((bool)base.AffectedObject)
		{
			AudioSource component = base.AffectedObject.GetComponent<AudioSource>();
			if ((bool)component)
			{
				component.Stop();
			}
		}
	}
}
