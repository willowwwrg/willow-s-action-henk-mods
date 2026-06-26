using UnityEngine;

public class SplineFollowNoRotate : SplineObject
{
	private Vector3 prevPos = Vector3.zero;

	public override void Update()
	{
		if ((!Application.isPlaying && !(spline == null)) || createdAtRuntime)
		{
			ApplyHandleOffset();
			SnapCoordinates();
			RecalcHandlePosition();
			if (handlePosition != prevPos)
			{
				base.transform.position = handlePosition;
			}
			prevPos = handlePosition;
			ClearFirstFrame();
			prevSplineOffset = splineOffset;
		}
	}

	private void OnDrawGizmos()
	{
	}
}
