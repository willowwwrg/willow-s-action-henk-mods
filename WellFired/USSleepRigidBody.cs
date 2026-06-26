using UnityEngine;

namespace WellFired;

[USequencerEvent("Physics/Sleep Rigid Body")]
[USequencerEventHideDuration]
[USequencerFriendlyName("Sleep Rigid Body")]
public class USSleepRigidBody : USEventBase
{
	public override void FireEvent()
	{
		Rigidbody component = base.AffectedObject.GetComponent<Rigidbody>();
		if (!component)
		{
			Debug.Log("Attempting to Nullify a force on an object, but it has no rigid body from USSleepRigidBody::FireEvent");
		}
		else
		{
			component.Sleep();
		}
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
		if ((bool)base.AffectedObject)
		{
			Rigidbody component = base.AffectedObject.GetComponent<Rigidbody>();
			if ((bool)component)
			{
				component.WakeUp();
			}
		}
	}
}
