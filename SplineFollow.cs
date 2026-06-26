using UnityEngine;

public class SplineFollow : SplineObject
{
	private bool snapCoordinates = true;

	private Vector3 prevPos = Vector3.zero;

	public override void Update()
	{
		FindSpline();
		if ((!Application.isPlaying && !(spline == null)) || createdAtRuntime)
		{
			ApplyHandleOffset();
			if (snapCoordinates)
			{
				SnapCoordinates();
			}
			ClampCoordinates();
			RecalcHandlePosition();
			if (handlePosition != prevPos)
			{
				base.transform.position = handlePosition;
				float num = Vector3.Angle(Vector3.right, handleDirection);
				num *= Mathf.Sign(Vector3.Cross(Vector3.right, handleDirection).y);
				Vector3 eulerAngles = base.transform.eulerAngles;
				eulerAngles.y = num;
				base.transform.eulerAngles = eulerAngles;
			}
			prevPos = handlePosition;
			ClearFirstFrame();
			prevSplineOffset = splineOffset;
		}
	}

	public void ForceOneUpdate()
	{
		bool flag = createdAtRuntime;
		createdAtRuntime = true;
		prevPos = Vector3.zero;
		Update();
		createdAtRuntime = flag;
	}

	public void SnapCoordinates(bool toggle)
	{
		snapCoordinates = toggle;
	}

	private void OnDrawGizmos()
	{
	}
}
