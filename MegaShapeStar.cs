using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Star")]
public class MegaShapeStar : MegaShape
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	private const int MIN_POINTS = 3;

	private const int MAX_POINTS = 100;

	private const float MIN_RADIUS = 0f;

	private const float MAX_RADIUS = float.MaxValue;

	private const float MIN_DIST = -180f;

	private const float MAX_DIST = 180f;

	private const int DEF_POINTS = 6;

	private const float DEF_DIST = 0f;

	private const float PI180 = 0.0174532f;

	public float radius1 = 2f;

	public float radius2 = 1f;

	public int points = 6;

	public float distortion;

	public float fillet1;

	public float fillet2;

	public override string GetHelpURL()
	{
		return "?page_id=396";
	}

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		Vector3 zero = Vector3.zero;
		radius1 = Mathf.Clamp(radius1, 0f, float.MaxValue);
		radius2 = Mathf.Clamp(radius2, 0f, float.MaxValue);
		distortion = Mathf.Clamp(distortion, -180f, 180f);
		points = Mathf.Clamp(points, 3, 100);
		fillet1 = Mathf.Clamp(fillet1, 0f, float.MaxValue);
		fillet2 = Mathf.Clamp(fillet2, 0f, float.MaxValue);
		if (splines.Count == 0)
		{
			MegaSpline item = new MegaSpline();
			splines.Add(item);
		}
		MegaSpline megaSpline = splines[0];
		megaSpline.knots.Clear();
		float num = 0.0174532f * distortion;
		float num2 = (float)Math.PI / (float)points;
		for (int i = 0; i < 2 * points; i++)
		{
			if (i % 2 != 0)
			{
				float num3 = (float)Math.PI * (float)i / (float)points;
				zero.x = Mathf.Cos(num3) * radius1;
				zero.y = Mathf.Sin(num3) * radius1;
				zero.z = 0f;
				if (fillet1 > 0f)
				{
					float f = num3 - num2;
					float f2 = num3 + num2;
					float num4 = Mathf.Sin(f);
					float num5 = Mathf.Sin(f2);
					float num6 = Mathf.Cos(f);
					float num7 = Mathf.Cos(f2);
					Vector3 vector = new Vector3(radius2 * num6, radius2 * num4, 0f);
					Vector3 vector2 = new Vector3(radius2 * num7, radius2 * num5, 0f);
					Vector3 vector3 = Vector3.Normalize(vector - zero) * fillet1;
					Vector3 vector4 = Vector3.Normalize(vector2 - zero) * fillet1;
					Vector3 vector5 = vector3 * 0.5517862f;
					Vector3 vector6 = vector4 * 0.5517862f;
					Vector3 vector7 = zero + vector3;
					Vector3 vector8 = zero + vector4;
					megaSpline.AddKnot(vector7, vector7 + vector5, vector7 - vector5, matrix);
					megaSpline.AddKnot(vector8, vector8 - vector6, vector8 + vector6, matrix);
				}
				else
				{
					megaSpline.AddKnot(zero, zero, zero, matrix);
				}
			}
			else
			{
				float num8 = num2 * (float)i + num;
				zero.x = Mathf.Cos(num8) * radius2;
				zero.y = Mathf.Sin(num8) * radius2;
				zero.z = 0f;
				if (fillet2 > 0f)
				{
					float f3 = num8 - num2 - num;
					float f4 = num8 + num2 + num;
					float num9 = Mathf.Sin(f3);
					float num10 = Mathf.Sin(f4);
					float num11 = Mathf.Cos(f3);
					float num12 = Mathf.Cos(f4);
					Vector3 vector9 = new Vector3(radius1 * num11, radius1 * num9, 0f);
					Vector3 vector10 = new Vector3(radius1 * num12, radius1 * num10, 0f);
					Vector3 vector11 = Vector3.Normalize(vector9 - zero) * fillet2;
					Vector3 vector12 = Vector3.Normalize(vector10 - zero) * fillet2;
					Vector3 vector13 = vector11 * 0.5517862f;
					Vector3 vector14 = vector12 * 0.5517862f;
					Vector3 vector15 = zero + vector11;
					Vector3 vector16 = zero + vector12;
					megaSpline.AddKnot(vector15, vector15 + vector13, vector15 - vector13, matrix);
					megaSpline.AddKnot(vector16, vector16 - vector14, vector16 + vector14, matrix);
				}
				else
				{
					megaSpline.AddKnot(zero, zero, zero, matrix);
				}
			}
		}
		megaSpline.closed = true;
		CalcLength();
	}
}
