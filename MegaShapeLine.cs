using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Line")]
public class MegaShapeLine : MegaShape
{
	public int points = 2;

	public float length = 1f;

	public float dir;

	public Transform end;

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		MegaSpline megaSpline = NewSpline();
		float magnitude = length;
		Vector3 to = Vector3.zero;
		if ((bool)end)
		{
			to = base.transform.worldToLocalMatrix.MultiplyPoint(end.position);
			magnitude = to.magnitude;
		}
		else
		{
			to.x = Mathf.Sin((float)Math.PI / 180f * dir) * magnitude;
			to.z = Mathf.Cos((float)Math.PI / 180f * dir) * magnitude;
		}
		Vector3 normalized = to.normalized;
		if (points < 2)
		{
			points = 2;
		}
		float num = magnitude / (float)points / 2f;
		for (int i = 0; i < points; i++)
		{
			float num2 = (float)i / (float)(points - 1);
			Vector3 vector = Vector3.Lerp(Vector3.zero, to, num2);
			Vector3 vector2 = new Vector3(normalized.x * num, normalized.y * num, normalized.z * num);
			Vector3 invec = vector - vector2;
			Vector3 outvec = vector + vector2;
			megaSpline.AddKnot(vector, invec, outvec, matrix);
		}
		CalcLength();
	}
}
