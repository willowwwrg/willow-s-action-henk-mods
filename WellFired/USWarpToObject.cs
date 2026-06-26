using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerEvent("Transform/Warp To Object")]
[USequencerFriendlyName("Warp To Object")]
public class USWarpToObject : USEventBase
{
	public GameObject objectToWarpTo;

	public bool useObjectRotation;

	private Transform previousTransform;

	public override void FireEvent()
	{
		if ((bool)objectToWarpTo)
		{
			base.AffectedObject.transform.position = objectToWarpTo.transform.position;
			if (useObjectRotation)
			{
				base.AffectedObject.transform.rotation = objectToWarpTo.transform.rotation;
			}
		}
		else
		{
			Debug.LogError(base.AffectedObject.name + ": No Object attached to WarpToObjectSequencer Script");
		}
		previousTransform = base.AffectedObject.transform;
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
		if ((bool)previousTransform)
		{
			base.AffectedObject.transform.position = previousTransform.position;
			base.AffectedObject.transform.rotation = previousTransform.rotation;
		}
	}
}
