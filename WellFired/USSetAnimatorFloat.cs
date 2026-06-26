using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerEvent("Animation (Mecanim)/Animator/Set Value/Float")]
[USequencerFriendlyName("Set Mecanim Float")]
public class USSetAnimatorFloat : USEventBase
{
	public string valueName = string.Empty;

	public float Value;

	private float prevValue;

	private int hash;

	public override void FireEvent()
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if (!component)
		{
			Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", this);
			return;
		}
		if (valueName.Length == 0)
		{
			Debug.LogWarning("Invalid name passed to the uSequencer Event Set Float", this);
			return;
		}
		hash = Animator.StringToHash(valueName);
		prevValue = component.GetFloat(hash);
		component.SetFloat(hash, Value);
	}

	public override void ProcessEvent(float runningTime)
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if (!component)
		{
			Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", this);
			return;
		}
		if (valueName.Length == 0)
		{
			Debug.LogWarning("Invalid name passed to the uSequencer Event Set Float", this);
			return;
		}
		hash = Animator.StringToHash(valueName);
		prevValue = component.GetFloat(hash);
		component.SetFloat(hash, Value);
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if ((bool)component && valueName.Length != 0)
		{
			component.SetFloat(hash, prevValue);
		}
	}
}
