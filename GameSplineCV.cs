using UnityEngine;

[ExecuteInEditMode]
public class GameSplineCV : MonoBehaviour
{
	private Vector3 prevPos = Vector3.zero;

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(base.transform.position, 0.5f);
	}

	private void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		if (localPosition.z != 0f)
		{
			localPosition.z = 0f;
			base.transform.localPosition = localPosition;
		}
		if (base.transform.localPosition != prevPos && (bool)base.transform.parent.GetComponent<GameSpline>())
		{
			base.transform.parent.GetComponent<GameSpline>().RebuildSpline();
		}
		prevPos = base.transform.localPosition;
	}
}
