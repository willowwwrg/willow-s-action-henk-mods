using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Apply Force")]
[USequencerEvent("Physics/Apply Force")]
[USequencerEventHideDuration]
public class USApplyForceEvent : USEventBase
{
	public Vector3 direction = Vector3.up;

	public float strength = 1f;

	public ForceMode type = ForceMode.Impulse;

	private Transform previousTransform;

	public override void FireEvent()
	{
		Rigidbody component = base.AffectedObject.GetComponent<Rigidbody>();
		if (!component)
		{
			Debug.Log("Attempting to apply an impulse to an object, but it has no rigid body from USequencerApplyImpulseEvent::FireEvent");
			return;
		}
		component.AddForceAtPosition(direction * strength, base.transform.position, type);
		previousTransform = component.transform;
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
		if (!base.AffectedObject)
		{
			return;
		}
		Rigidbody component = base.AffectedObject.GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.Sleep();
			if ((bool)previousTransform)
			{
				base.AffectedObject.transform.position = previousTransform.position;
				base.AffectedObject.transform.rotation = previousTransform.rotation;
			}
		}
	}
}
