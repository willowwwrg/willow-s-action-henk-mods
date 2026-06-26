using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaSpline
{
	public float length;

	public bool closed;

	public List<MegaKnot> knots = new List<MegaKnot>();

	public List<MegaKnotAnim> animations;

	public Vector3 offset = Vector3.zero;

	public Vector3 rotate = Vector3.zero;

	public Vector3 scale = Vector3.one;

	public bool reverse;

	public int outlineSpline = -1;

	public float outline;

	public bool constantSpeed;

	public int subdivs = 10;

	public MegaShapeEase twistmode;

	public MegaSplineAnim splineanim = new MegaSplineAnim();

	public static MegaSpline Copy(MegaSpline src)
	{
		MegaSpline megaSpline = new MegaSpline();
		megaSpline.closed = src.closed;
		megaSpline.offset = src.offset;
		megaSpline.rotate = src.rotate;
		megaSpline.scale = src.scale;
		megaSpline.length = src.length;
		megaSpline.knots = new List<MegaKnot>();
		megaSpline.constantSpeed = src.constantSpeed;
		megaSpline.subdivs = src.subdivs;
		for (int i = 0; i < src.knots.Count; i++)
		{
			MegaKnot megaKnot = new MegaKnot();
			megaKnot.p = src.knots[i].p;
			megaKnot.invec = src.knots[i].invec;
			megaKnot.outvec = src.knots[i].outvec;
			megaKnot.seglength = src.knots[i].seglength;
			megaKnot.length = src.knots[i].length;
			megaKnot.notlocked = src.knots[i].notlocked;
			megaSpline.knots.Add(megaKnot);
		}
		megaSpline.animations = new List<MegaKnotAnim>(src.animations);
		return megaSpline;
	}

	public void AddKnot(Vector3 p, Vector3 invec, Vector3 outvec)
	{
		MegaKnot megaKnot = new MegaKnot();
		megaKnot.p = p;
		megaKnot.invec = invec;
		megaKnot.outvec = outvec;
		knots.Add(megaKnot);
	}

	public void AddKnot(Vector3 p, Vector3 invec, Vector3 outvec, Matrix4x4 tm)
	{
		MegaKnot megaKnot = new MegaKnot();
		megaKnot.p = tm.MultiplyPoint3x4(p);
		megaKnot.invec = tm.MultiplyPoint3x4(invec);
		megaKnot.outvec = tm.MultiplyPoint3x4(outvec);
		knots.Add(megaKnot);
	}

	public bool Contains(Vector3 p)
	{
		if (!closed)
		{
			return false;
		}
		int index = knots.Count - 1;
		bool flag = false;
		for (int i = 0; i < knots.Count; i++)
		{
			if (((knots[i].p.z < p.z && knots[index].p.z >= p.z) || (knots[index].p.z < p.z && knots[i].p.z >= p.z)) && knots[i].p.x + (p.z - knots[i].p.z) / (knots[index].p.z - knots[i].p.z) * (knots[index].p.x - knots[i].p.x) < p.x)
			{
				flag = !flag;
			}
			index = i;
		}
		return flag;
	}

	public float Area()
	{
		float num = 0f;
		if (closed)
		{
			for (int i = 0; i < knots.Count; i++)
			{
				int index = (i + 1) % knots.Count;
				num += (knots[i].p.z + knots[index].p.z) * (knots[index].p.x - knots[i].p.x);
			}
		}
		return num * 0.5f;
	}

	public float CalcLength(int steps)
	{
		if (steps < 1)
		{
			steps = 1;
		}
		subdivs = steps;
		return CalcLength();
	}

	public float CalcLength()
	{
		length = 0f;
		int num = knots.Count - 1;
		if (closed)
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			int index = (i + 1) % knots.Count;
			Vector3 vector = knots[i].p;
			float num2 = 1f / (float)subdivs;
			float num3 = num2;
			knots[i].seglength = 0f;
			if (knots[i].lengths == null || knots[i].lengths.Length != subdivs + 1)
			{
				knots[i].lengths = new float[subdivs + 1];
				knots[i].points = new Vector3[subdivs + 1];
			}
			knots[i].lengths[0] = length;
			ref Vector3 reference = ref knots[i].points[0];
			reference = knots[i].p;
			float num4 = 0f;
			for (int j = 1; j < subdivs; j++)
			{
				Vector3 vector2 = knots[i].Interpolate(num3, knots[index]);
				knots[i].points[j] = vector2;
				num4 = Vector3.Magnitude(vector2 - vector);
				knots[i].seglength += num4;
				vector = vector2;
				num3 += num2;
				length += num4;
				knots[i].lengths[j] = length;
			}
			num4 = Vector3.Magnitude(knots[index].p - vector);
			knots[i].seglength += num4;
			length += num4;
			knots[i].lengths[subdivs] = length;
			ref Vector3 reference2 = ref knots[i].points[subdivs];
			reference2 = knots[index].p;
			knots[i].length = length;
			length = knots[i].length;
		}
		return length;
	}

	public float GetTwist(float alpha)
	{
		int num = 0;
		if (closed)
		{
			alpha = Mathf.Repeat(alpha, 1f);
			float num2 = alpha * length;
			if (num2 > knots[knots.Count - 1].length)
			{
				alpha = 1f - (length - num2) / knots[knots.Count - 1].seglength;
				return TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
			}
			for (num = 0; num < knots.Count && !(num2 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num2) / knots[num].seglength;
			if (num < knots.Count - 1)
			{
				return TwistVal(knots[num].twist, knots[num + 1].twist, alpha);
			}
			return TwistVal(knots[num].twist, knots[0].twist, alpha);
		}
		float num3 = alpha * length;
		for (num = 0; num < knots.Count && !(num3 <= knots[num].length); num++)
		{
		}
		alpha = 1f - (knots[num].length - num3) / knots[num].seglength;
		if (num < knots.Count - 1)
		{
			return TwistVal(knots[num].twist, knots[num + 1].twist, alpha);
		}
		return knots[num].twist;
	}

	public Vector3 Interpolate(float alpha, bool type, ref int k)
	{
		int num = 0;
		if (constantSpeed)
		{
			return InterpolateCS(alpha, type, ref k);
		}
		if (closed)
		{
			if (type)
			{
				float num2 = alpha * length;
				if (num2 > knots[knots.Count - 1].length)
				{
					k = knots.Count - 1;
					alpha = 1f - (length - num2) / knots[knots.Count - 1].seglength;
					return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
				}
				for (num = 0; num < knots.Count && !(num2 <= knots[num].length); num++)
				{
				}
				alpha = 1f - (knots[num].length - num2) / knots[num].seglength;
			}
			else
			{
				float num3 = alpha * (float)knots.Count;
				num = (int)num3;
				if (num == knots.Count)
				{
					num--;
					alpha = 1f;
				}
				else
				{
					alpha = num3 - (float)num;
				}
			}
			if (num < knots.Count - 1)
			{
				k = num;
				return knots[num].Interpolate(alpha, knots[num + 1]);
			}
			k = num;
			return knots[num].Interpolate(alpha, knots[0]);
		}
		if (type)
		{
			float num4 = alpha * length;
			for (num = 0; num < knots.Count && !(num4 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num4) / knots[num].seglength;
		}
		else
		{
			float num5 = alpha * (float)knots.Count;
			num = (int)num5;
			if (num == knots.Count)
			{
				num--;
				alpha = 1f;
			}
			else
			{
				alpha = num5 - (float)num;
			}
		}
		if (num < knots.Count - 1)
		{
			k = num;
			return knots[num].Interpolate(alpha, knots[num + 1]);
		}
		k = num;
		return knots[num].p;
	}

	public Vector3 InterpolateCS(float alpha, bool type, ref int k)
	{
		int num = 0;
		if (closed)
		{
			float num2 = alpha * length;
			if (num2 > knots[knots.Count - 1].length)
			{
				k = knots.Count - 1;
				alpha = 1f - (length - num2) / knots[knots.Count - 1].seglength;
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			for (num = 0; num < knots.Count && !(num2 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num2) / knots[num].seglength;
			if (num < knots.Count - 1)
			{
				k = num;
				return knots[num].InterpolateCS(alpha, knots[num + 1]);
			}
			k = num;
			return knots[num].InterpolateCS(alpha, knots[0]);
		}
		if (type)
		{
			float num3 = alpha * length;
			for (num = 0; num < knots.Count && !(num3 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num3) / knots[num].seglength;
		}
		else
		{
			float num4 = alpha * (float)knots.Count;
			num = (int)num4;
			if (num == knots.Count)
			{
				num--;
				alpha = 1f;
			}
			else
			{
				alpha = num4 - (float)num;
			}
		}
		if (num < knots.Count - 1)
		{
			k = num;
			return knots[num].InterpolateCS(alpha, knots[num + 1]);
		}
		k = num;
		return knots[num].p;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * value / 1f) - 1f) + start;
	}

	private float TwistVal(float v1, float v2, float alpha)
	{
		if (twistmode == MegaShapeEase.Linear)
		{
			return Mathf.Lerp(v1, v2, alpha);
		}
		return easeInOutSine(v1, v2, alpha);
	}

	public Vector3 Interpolate(float alpha, bool type, ref int k, ref float twist)
	{
		int num = 0;
		if (constantSpeed)
		{
			return InterpolateCS(alpha, type, ref k, ref twist);
		}
		if (closed)
		{
			if (type)
			{
				float num2 = alpha * length;
				if (num2 > knots[knots.Count - 1].length)
				{
					k = knots.Count - 1;
					alpha = 1f - (length - num2) / knots[knots.Count - 1].seglength;
					twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
				}
				for (num = 0; num < knots.Count && !(num2 <= knots[num].length); num++)
				{
				}
				alpha = 1f - (knots[num].length - num2) / knots[num].seglength;
			}
			else
			{
				float num3 = alpha * (float)knots.Count;
				num = (int)num3;
				if (num == knots.Count)
				{
					num--;
					alpha = 1f;
				}
				else
				{
					alpha = num3 - (float)num;
				}
			}
			if (num < knots.Count - 1)
			{
				k = num;
				twist = TwistVal(knots[num].twist, knots[num + 1].twist, alpha);
				return knots[num].Interpolate(alpha, knots[num + 1]);
			}
			k = num;
			twist = TwistVal(knots[num].twist, knots[0].twist, alpha);
			return knots[num].Interpolate(alpha, knots[0]);
		}
		if (type)
		{
			float num4 = alpha * length;
			for (num = 0; num < knots.Count && !(num4 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num4) / knots[num].seglength;
		}
		else
		{
			float num5 = alpha * (float)knots.Count;
			num = (int)num5;
			if (num == knots.Count)
			{
				num--;
				alpha = 1f;
			}
			else
			{
				alpha = num5 - (float)num;
			}
		}
		if (num < knots.Count - 1)
		{
			k = num;
			twist = TwistVal(knots[num].twist, knots[num + 1].twist, alpha);
			return knots[num].Interpolate(alpha, knots[num + 1]);
		}
		k = num;
		twist = knots[num].twist;
		return knots[num].p;
	}

	public Vector3 InterpolateCS(float alpha, bool type, ref int k, ref float twist)
	{
		int num = 0;
		if (closed)
		{
			float num2 = alpha * length;
			if (num2 > knots[knots.Count - 1].length)
			{
				k = knots.Count - 1;
				alpha = 1f - (length - num2) / knots[knots.Count - 1].seglength;
				twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			for (num = 0; num < knots.Count && !(num2 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num2) / knots[num].seglength;
			if (num < knots.Count - 1)
			{
				k = num;
				twist = TwistVal(knots[num].twist, knots[num + 1].twist, alpha);
				return knots[num].InterpolateCS(alpha, knots[num + 1]);
			}
			k = num;
			twist = TwistVal(knots[num].twist, knots[0].twist, alpha);
			return knots[num].InterpolateCS(alpha, knots[0]);
		}
		if (type)
		{
			float num3 = alpha * length;
			for (num = 0; num < knots.Count && !(num3 <= knots[num].length); num++)
			{
			}
			alpha = 1f - (knots[num].length - num3) / knots[num].seglength;
		}
		else
		{
			float num4 = alpha * (float)knots.Count;
			num = (int)num4;
			if (num == knots.Count)
			{
				num--;
				alpha = 1f;
			}
			else
			{
				alpha = num4 - (float)num;
			}
		}
		if (num < knots.Count - 1)
		{
			k = num;
			twist = TwistVal(knots[num].twist, knots[num + 1].twist, alpha);
			return knots[num].InterpolateCS(alpha, knots[num + 1]);
		}
		k = num;
		twist = knots[num].twist;
		return knots[num].p;
	}

	public Vector3 InterpCurve3D(float alpha, bool type, ref int k)
	{
		k = 0;
		if (alpha < 0f)
		{
			if (!closed)
			{
				Vector3 vector = Interpolate(0f, type, ref k);
				Vector3 vector2 = Interpolate(0.01f, type, ref k) - vector;
				vector2.Normalize();
				return vector + length * alpha * vector2;
			}
			alpha = Mathf.Repeat(alpha, 1f);
		}
		else if (alpha > 1f)
		{
			if (!closed)
			{
				Vector3 vector3 = Interpolate(1f, type, ref k);
				Vector3 vector4 = Interpolate(0.99f, type, ref k) - vector3;
				vector4.Normalize();
				return vector3 + length * (1f - alpha) * vector4;
			}
			alpha %= 1f;
		}
		return Interpolate(alpha, type, ref k);
	}

	public Vector3 InterpBezier3D(int knot, float a)
	{
		if (knot < knots.Count)
		{
			int num = knot + 1;
			if (num == knots.Count && closed)
			{
				num = 0;
			}
			return knots[knot].Interpolate(a, knots[num]);
		}
		return Vector3.zero;
	}

	public void Centre(float scale)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < knots.Count; i++)
		{
			zero += knots[i].p;
		}
		zero /= (float)knots.Count;
		for (int j = 0; j < knots.Count; j++)
		{
			knots[j].p -= zero;
			knots[j].invec -= zero;
			knots[j].outvec -= zero;
			knots[j].p *= scale;
			knots[j].invec *= scale;
			knots[j].outvec *= scale;
		}
	}

	public void Reverse()
	{
		List<MegaKnot> list = new List<MegaKnot>();
		for (int num = knots.Count - 1; num >= 0; num--)
		{
			MegaKnot megaKnot = new MegaKnot();
			megaKnot.p = knots[num].p;
			megaKnot.invec = knots[num].outvec;
			megaKnot.outvec = knots[num].invec;
			list.Add(megaKnot);
		}
		knots = list;
		CalcLength();
	}

	public void SetHeight(float y)
	{
		for (int i = 0; i < knots.Count; i++)
		{
			knots[i].p.y = y;
			knots[i].outvec.y = y;
			knots[i].invec.y = y;
		}
	}

	public void SetTwist(float twist)
	{
		for (int i = 0; i < knots.Count; i++)
		{
			knots[i].twist = twist;
		}
	}
}
