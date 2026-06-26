using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Helix")]
public class MegaShapeHelix : MegaShape
{
	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	public float radius1 = 1f;

	public float radius2 = 0.75f;

	public float height;

	public float turns = 1f;

	public float bias;

	public float adjust = 0.4f;

	public bool clockwise = true;

	public int PointsPerTurn = 8;

	public override void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		PointsPerTurn = Mathf.Clamp(PointsPerTurn, 3, 100);
		MegaSpline megaSpline = NewSpline();
		float num = 0f;
		float num2 = turns * (float)Math.PI * 2f;
		if (num > num2)
		{
			num2 += (float)Math.PI * 2f;
		}
		float num3 = (float)Math.PI * 2f * turns;
		if (clockwise)
		{
			num3 *= -1f;
		}
		int num4 = (int)(turns * (float)PointsPerTurn);
		if (num4 == 0)
		{
			num4 = 1;
		}
		float num5 = num4;
		float num6 = MegaShape.veccalc((num2 - num) / num5);
		float num7 = radius2 - radius1;
		float p = 1f;
		if (bias > 0f)
		{
			p = bias * 9f + 1f;
		}
		else if (bias < 0f)
		{
			p = (0f - bias) * 9f + 1f;
		}
		for (int i = 0; i <= num4; i++)
		{
			float num8 = (float)i / num5;
			float num9 = radius1 + num7 * num8;
			float num10 = num8;
			if (bias > 0f)
			{
				num10 = 1f - Mathf.Pow(1f - num8, p);
			}
			else if (bias < 0f)
			{
				num10 = Mathf.Pow(num8, p);
			}
			float f = num3 * num8;
			float num11 = Mathf.Sin(f);
			float num12 = Mathf.Cos(f);
			float num13 = num6 * num9;
			Vector3 vector = new Vector3(num12 * num9, num11 * num9, height * num10);
			Vector3 vector2 = new Vector3(num11 * num13, (0f - num12) * num13, 0f);
			Vector3 vector3 = vector + vector2;
			Vector3 vector4 = vector - vector2;
			if (!clockwise)
			{
				megaSpline.AddKnot(vector, vector3, vector4, matrix);
			}
			else
			{
				megaSpline.AddKnot(vector, vector4, vector3, matrix);
			}
		}
		CalcLength();
	}
}
