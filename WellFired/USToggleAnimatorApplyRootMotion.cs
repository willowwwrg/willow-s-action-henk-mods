using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerFriendlyName("Toggle Apply Root Motion")]
[USequencerEvent("Animation (Mecanim)/Animator/Toggle Apply Root Motion")]
public class USToggleAnimatorApplyRootMotion : USEventBase
{
	public bool applyRootMotion = true;

	private bool prevApplyRootMotion;

	public override void FireEvent()
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if (!component)
		{
			Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", this);
			return;
		}
		prevApplyRootMotion = component.applyRootMotion;
		component.applyRootMotion = applyRootMotion;
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
			component.applyRootMotion = prevApplyRootMotion;
		}
	}
}
