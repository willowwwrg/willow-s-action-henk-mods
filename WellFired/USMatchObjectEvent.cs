using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Match Objects Orientation")]
[USequencerEvent("Transform/Match Objects Orientation")]
public class USMatchObjectEvent : USEventBase
{
	public GameObject objectToMatch;

	public AnimationCurve inCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private Quaternion sourceRotation = Quaternion.identity;

	private Vector3 sourcePosition = Vector3.zero;

	public override void FireEvent()
	{
		if (!objectToMatch)
		{
			Debug.LogWarning("The USMatchObjectEvent event does not provice a object to match", this);
			return;
		}
		sourceRotation = base.AffectedObject.transform.rotation;
		sourcePosition = base.AffectedObject.transform.position;
	}

	public override void ProcessEvent(float deltaTime)
	{
		if (!objectToMatch)
		{
			Debug.LogWarning("The USMatchObjectEvent event does not provice a object to look at", this);
			return;
		}
		float num = 1f;
		num = Mathf.Clamp(inCurve.Evaluate(deltaTime), 0f, 1f);
		Vector3 position = objectToMatch.transform.position;
		Quaternion rotation = objectToMatch.transform.rotation;
		base.AffectedObject.transform.rotation = Quaternion.Slerp(sourceRotation, rotation, num);
		base.AffectedObject.transform.position = Vector3.Slerp(sourcePosition, position, num);
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		if ((bool)base.AffectedObject)
		{
			base.AffectedObject.transform.rotation = sourceRotation;
			base.AffectedObject.transform.position = sourcePosition;
		}
	}
}
