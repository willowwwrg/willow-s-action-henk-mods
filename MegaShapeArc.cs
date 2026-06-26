using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Arc")]
public class MegaShapeArc : MegaShape
{
	public float from;

	public float to = 270f;

	public float radius = 1f;

	public bool pie;

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		MegaSpline megaSpline = NewSpline();
		Vector3 zero = Vector3.zero;
		float num = from * ((float)Math.PI / 180f);
		float num2 = to * ((float)Math.PI / 180f);
		if (num > num2)
		{
			num2 += (float)Math.PI * 2f;
		}
		float num3 = num2 - num;
		float num4 = MegaShape.veccalc(num3 / 3f) * radius;
		float num5 = num3 / 3f;
		for (int i = 0; i < 4; i++)
		{
			float f = num + (float)i * num5;
			float num6 = Mathf.Sin(f);
			float num7 = Mathf.Cos(f);
			Vector3 vector = new Vector3(num7 * radius, num6 * radius, 0f);
			Vector3 vector2 = new Vector3(num6 * num4, (0f - num7) * num4, 0f);
			Vector3 invec = ((i != 0) ? (vector + vector2) : vector);
			Vector3 outvec = ((i != 3) ? (vector - vector2) : vector);
			megaSpline.AddKnot(vector, invec, outvec, matrix);
		}
		if (pie)
		{
			megaSpline.AddKnot(zero, zero, zero);
			megaSpline.closed = true;
		}
		CalcLength();
	}
}
