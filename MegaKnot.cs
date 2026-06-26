using System;
using UnityEngine;

[Serializable]
public class MegaKnot
{
	public Vector3 p;

	public Vector3 invec;

	public Vector3 outvec;

	public float seglength;

	public float length;

	public bool notlocked;

	public float twist;

	public int id;

	public float[] lengths;

	public Vector3[] points;

	public MegaKnot()
	{
		p = default(Vector3);
		invec = default(Vector3);
		outvec = default(Vector3);
		length = 0f;
		seglength = 0f;
	}

	public Vector3 Interpolate(float t, MegaKnot k)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = num2 * num;
		float num4 = t * t;
		float num5 = num4 * t;
		num2 = 3f * num2 * t;
		num = 3f * num * num4;
		Vector3 zero = Vector3.zero;
		zero.x = num3 * p.x + num2 * outvec.x + num * k.invec.x + num5 * k.p.x;
		zero.y = num3 * p.y + num2 * outvec.y + num * k.invec.y + num5 * k.p.y;
		zero.z = num3 * p.z + num2 * outvec.z + num * k.invec.z + num5 * k.p.z;
		return zero;
	}

	public Vector3 InterpolateCS(float t, MegaKnot k)
	{
		if (lengths == null || lengths.Length == 0)
		{
			return Interpolate(t, k);
		}
		float num = t * seglength + lengths[0];
		int num2 = lengths.Length - 1;
		int num3 = -1;
		int num4 = 0;
		while (num2 - num3 > 1)
		{
			num4 = (num2 + num3) / 2;
			if (num >= lengths[num4])
			{
				if (num < lengths[num4 + 1])
				{
					break;
				}
				num3 = num4;
			}
			else
			{
				num2 = num4;
			}
		}
		float t2 = (num - lengths[num4]) / (lengths[num4 + 1] - lengths[num4]);
		return Vector3.Lerp(points[num4], points[num4 + 1], t2);
	}

	public Vector3 Tangent(float t, MegaKnot k)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = t * t;
		return new Vector3
		{
			x = -3f * p.x * num2 + 3f * outvec.x * num * (num - 2f * t) + 3f * k.invec.x * t * (2f * num - t) + k.p.x * 3f * num3,
			y = -3f * p.y * num2 + 3f * outvec.y * num * (num - 2f * t) + 3f * k.invec.y * t * (2f * num - t) + k.p.y * 3f * num3,
			z = -3f * p.z * num2 + 3f * outvec.z * num * (num - 2f * t) + 3f * k.invec.z * t * (2f * num - t) + k.p.z * 3f * num3
		};
	}
}
