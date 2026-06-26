using UnityEngine;

namespace WellFired;

[USequencerEvent("Animation (Mecanim)/Animator/Toggle Stabalize Feet")]
[USequencerFriendlyName("Toggle Stabalize Feet")]
[USequencerEventHideDuration]
public class USToggleAnimatorStabalizeFeet : USEventBase
{
	public bool stabalizeFeet = true;

	private bool prevStabalizeFeet;

	public override void FireEvent()
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if (!component)
		{
			Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", this);
			return;
		}
		prevStabalizeFeet = component.stabilizeFeet;
		component.stabilizeFeet = stabalizeFeet;
	}

	public override void ProcessEvent(float runningTime)
	{
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if ((bool)component)
		{
			component.stabilizeFeet = prevStabalizeFeet;
		}
	}
}
