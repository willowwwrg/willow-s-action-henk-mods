using System;
using UnityEngine;

[ExecuteInEditMode]
public class DOFCamera : MonoBehaviour
{
	public Camera srcCamera;

	public float sampleRadius = 4f;

	public float alpha;

	public float focalDistance = 8f;

	private Vector3 tpos;

	private void Update()
	{
		if (srcCamera != null)
		{
			float f = alpha * (float)Math.PI * 2f;
			float num = sampleRadius;
			float x = num * Mathf.Cos(f);
			float y = num * Mathf.Sin(f);
			Vector3 position = new Vector3(x, y, 0f);
			Vector3 vector = srcCamera.transform.forward * focalDistance;
			tpos = vector + srcCamera.transform.position;
			base.transform.position = srcCamera.transform.TransformPoint(position);
			base.transform.LookAt(tpos);
		}
	}

	public void OnDrawGizmos()
	{
		if (srcCamera != null)
		{
			Gizmos.DrawLine(base.transform.position, tpos);
		}
	}
}
