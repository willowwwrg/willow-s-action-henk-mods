using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Attach Object To Parent")]
[USequencerEvent("Attach/Attach To Parent")]
[USequencerEventHideDuration]
public class USAttachToParentEvent : USEventBase
{
	public Transform parentObject;

	private Transform originalParent;

	public bool resetTransform;

	private Vector3 previousPosition;

	private Quaternion previousRotation;

	public override void FireEvent()
	{
		if (!parentObject)
		{
			Debug.Log("USAttachEvent has been asked to attach an object, but it hasn't been given a parent from USAttachEvent::FireEvent");
			return;
		}
		originalParent = base.AffectedObject.transform.parent;
		if (resetTransform)
		{
			previousPosition = base.AffectedObject.transform.localPosition;
			previousRotation = base.AffectedObject.transform.localRotation;
		}
		base.AffectedObject.transform.parent = parentObject;
		if (resetTransform)
		{
			base.AffectedObject.transform.localPosition = Vector3.zero;
			base.AffectedObject.transform.localRotation = Quaternion.identity;
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
			base.AffectedObject.transform.parent = originalParent;
			if (resetTransform)
			{
				base.AffectedObject.transform.localPosition = previousPosition;
				base.AffectedObject.transform.localRotation = previousRotation;
			}
		}
	}
}
