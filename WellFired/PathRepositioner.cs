using UnityEngine;

namespace WellFired;

[ExecuteInEditMode]
public class PathRepositioner : MonoBehaviour
{
	private USTimelineObjectPath path;

	private Vector3 lastPosition;

	private Quaternion lastRotation;

	private void Start()
	{
	}

	private void Update()
	{
		if (path == null)
		{
			path = GetComponent<USTimelineObjectPath>();
			lastPosition = base.transform.position;
		}
		if (!(path != null))
		{
			return;
		}
		if (base.transform.position != lastPosition)
		{
			Vector3 vector = base.transform.position - lastPosition;
			foreach (SplineKeyframe keyframe in path.Keyframes)
			{
				keyframe.Position += vector;
			}
		}
		if (base.transform.rotation != lastRotation)
		{
			Quaternion quaternion = base.transform.rotation * Quaternion.Inverse(lastRotation);
			foreach (SplineKeyframe keyframe2 in path.Keyframes)
			{
				Vector3 vector2 = keyframe2.Position - base.transform.position;
				Vector3 vector3 = quaternion * vector2;
				keyframe2.Position = base.transform.position + vector3;
			}
		}
		Vector3 vector4 = Vector3.up * 5f;
		int num = 0;
		foreach (SplineKeyframe keyframe3 in path.Keyframes)
		{
			vector4 += keyframe3.Position;
			num++;
		}
		base.transform.position = vector4 / num;
		lastPosition = base.transform.position;
		lastRotation = base.transform.rotation;
	}
}
