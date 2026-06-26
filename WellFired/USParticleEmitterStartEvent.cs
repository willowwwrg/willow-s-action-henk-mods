using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerEvent("Particle System/Start Emitter")]
[USequencerFriendlyName("Start Emitter (Legacy)")]
public class USParticleEmitterStartEvent : USEventBase
{
	public void Update()
	{
		if ((bool)base.AffectedObject)
		{
			ParticleSystem component = base.AffectedObject.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				base.Duration = component.duration + component.startLifetime;
			}
		}
	}

	public override void FireEvent()
	{
		if ((bool)base.AffectedObject)
		{
			ParticleSystem component = base.AffectedObject.GetComponent<ParticleSystem>();
			if (!component)
			{
				Debug.Log("Attempting to emit particles, but the object has no particleSystem USParticleEmitterStartEvent::FireEvent");
			}
			else if (Application.isPlaying)
			{
				component.Play();
			}
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
		if (!Application.isPlaying)
		{
			base.AffectedObject.GetComponent<ParticleSystem>().Simulate(deltaTime);
		}
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		if ((bool)base.AffectedObject)
		{
			ParticleSystem component = base.AffectedObject.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				component.Stop();
			}
		}
	}
}
