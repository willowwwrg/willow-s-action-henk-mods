using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Set Layer Weight")]
[USequencerEvent("Animation (Mecanim)/Animator/Set Layer Weight")]
[USequencerEventHideDuration]
public class USSetAnimatorLayerWeight : USEventBase
{
	public float layerWeight = 1f;

	public int layerIndex = -1;

	private float prevLayerWeight;

	public override void FireEvent()
	{
		Animator component = base.AffectedObject.GetComponent<Animator>();
		if (!component)
		{
			Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", this);
			return;
		}
		if (layerIndex < 0)
		{
			Debug.LogWarning("Set Animator Layer weight, incorrect index : " + layerIndex);
			return;
		}
		prevLayerWeight = component.GetLayerWeight(layerIndex);
		component.SetLayerWeight(layerIndex, layerWeight);
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
		if ((bool)component && layerIndex >= 0)
		{
			component.SetLayerWeight(layerIndex, prevLayerWeight);
		}
	}
}
