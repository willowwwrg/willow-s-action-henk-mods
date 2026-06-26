using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Ellipse")]
public class MegaShapeEllipse : MegaShape
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	public float length = 1f;

	public float width = 1f;

	public override string GetHelpURL()
	{
		return "?page_id=1178";
	}

	private void MakeCircle(float radius, float xmult, float ymult)
	{
		Matrix4x4 matrix = GetMatrix();
		float num = 0.5517862f * radius;
		MegaSpline megaSpline = NewSpline();
		Vector3 b = new Vector3(xmult, ymult, 1f);
		for (int i = 0; i < 4; i++)
		{
			float f = (float)Math.PI * 2f * (float)i / 4f;
			float num2 = Mathf.Sin(f);
			float num3 = Mathf.Cos(f);
			Vector3 vector = new Vector3(num3 * radius, num2 * radius, 0f);
			Vector3 vector2 = new Vector3(num2 * num, (0f - num3) * num, 0f);
			megaSpline.AddKnot(Vector3.Scale(vector, b), Vector3.Scale(vector + vector2, b), Vector3.Scale(vector - vector2, b), matrix);
		}
		megaSpline.closed = true;
		CalcLength();
	}

	public override void MakeShape()
	{
		length = Mathf.Clamp(length, 0f, float.MaxValue);
		width = Mathf.Clamp(width, 0f, float.MaxValue);
		float num;
		float xmult;
		float ymult;
		if (length < width)
		{
			num = width;
			xmult = 1f;
			ymult = length / width;
		}
		else if (width < length)
		{
			num = length;
			xmult = width / length;
			ymult = 1f;
		}
		else
		{
			num = length;
			xmult = (ymult = 1f);
		}
		MakeCircle(num / 2f, xmult, ymult);
		CalcLength();
	}
}
