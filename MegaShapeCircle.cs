using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Circle")]
public class MegaShapeCircle : MegaShape
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	public float Radius = 1f;

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		float num = 0.5517862f * Radius;
		MegaSpline megaSpline = NewSpline();
		for (int i = 0; i < 4; i++)
		{
			float f = (float)Math.PI * 2f * (float)i / 4f;
			float num2 = Mathf.Sin(f);
			float num3 = Mathf.Cos(f);
			Vector3 vector = new Vector3(num3 * Radius, num2 * Radius, 0f);
			Vector3 vector2 = new Vector3(num2 * num, (0f - num3) * num, 0f);
			megaSpline.AddKnot(vector, vector + vector2, vector - vector2, matrix);
		}
		megaSpline.closed = true;
		CalcLength();
	}
}
