using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/NGon")]
public class MegaShapeNGon : MegaShape
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	public float radius = 1f;

	public float fillet;

	public int sides = 6;

	public bool circular;

	public bool scribe;

	public override string GetHelpURL()
	{
		return "?page_id=500";
	}

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		radius = Mathf.Clamp(radius, 0f, float.MaxValue);
		sides = Mathf.Clamp(sides, 3, 100);
		float num = radius;
		if (scribe)
		{
			num = radius / Mathf.Cos((float)Math.PI * 2f / ((float)sides * 2f));
		}
		MegaSpline megaSpline = NewSpline();
		float num2 = ((!circular) ? 0f : (MegaShape.veccalc((float)Math.PI * 2f / (float)sides) * num));
		if (fillet == 0f || circular)
		{
			for (int i = 0; i < sides; i++)
			{
				float f = (float)Math.PI * 2f * (float)i / (float)sides;
				float num3 = Mathf.Sin(f);
				float num4 = Mathf.Cos(f);
				Vector3 vector = new Vector3(num4 * num, num3 * num, 0f);
				Vector3 vector2 = new Vector3(num3 * num2, (0f - num4) * num2, 0f);
				megaSpline.AddKnot(vector, vector + vector2, vector - vector2, matrix);
			}
			megaSpline.closed = true;
		}
		else
		{
			for (int j = 0; j < sides; j++)
			{
				float num5 = (float)Math.PI * 2f * (float)j / (float)sides;
				float num6 = (float)Math.PI * (float)(sides - 2) / (float)sides / 2f;
				float f2 = num5 + num6;
				float f3 = num5 - num6;
				float num7 = fillet * Mathf.Tan((float)Math.PI / 2f - num6);
				float num8 = Mathf.Sin(num5);
				float num9 = Mathf.Cos(num5);
				Vector3 vector3 = new Vector3(num9 * num, num8 * num, 0f);
				Vector3 vector4 = new Vector3(0f - Mathf.Cos(f2), 0f - Mathf.Sin(f2), 0f) * num7;
				Vector3 vector5 = new Vector3(0f - Mathf.Cos(f3), 0f - Mathf.Sin(f3), 0f) * num7;
				Vector3 vector6 = vector3 + vector4;
				Vector3 vector7 = vector3 + vector5;
				Vector3 vector8 = vector4 * 0.5517862f;
				Vector3 vector9 = vector5 * 0.5517862f;
				megaSpline.AddKnot(vector6, vector6 + vector8, vector6 - vector8, matrix);
				megaSpline.AddKnot(vector7, vector7 - vector9, vector7 + vector9, matrix);
			}
			megaSpline.closed = true;
		}
		CalcLength();
	}
}
