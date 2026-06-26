using System;
using UnityEngine;

[Serializable]
public class MegaRopeSpring
{
	public int p1;

	public int p2;

	public float restlen;

	public float ks;

	public float kd;

	public float len;

	public MegaRopeSpring(int _p1, int _p2, float _ks, float _kd, MegaRope hose)
	{
		p1 = _p1;
		p2 = _p2;
		ks = _ks;
		kd = _kd;
		restlen = (hose.masses[p1].pos - hose.masses[p2].pos).magnitude;
	}

	public void doCalculateSpringForceOld(MegaRope hose)
	{
		Vector3 vector = hose.masses[p1].pos - hose.masses[p2].pos;
		float magnitude = vector.magnitude;
		if (magnitude != 0f)
		{
			float num = 1f / magnitude;
			Vector3 lhs = hose.masses[p1].vel - hose.masses[p2].vel;
			float num2 = (magnitude - restlen) * ks + Vector3.Dot(lhs, vector) * kd * num;
			Vector3 vector2 = vector * num * num2;
			hose.masses[p1].force -= vector2;
			hose.masses[p2].force += vector2;
		}
	}

	public void doCalculateSpringForce(MegaRope hose)
	{
		Vector3 vector = hose.masses[p1].pos - hose.masses[p2].pos;
		float magnitude = vector.magnitude;
		float num = (magnitude - restlen) * ks;
		float num2 = Vector3.Dot(hose.masses[p1].vel - hose.masses[p2].vel, vector) * kd / magnitude;
		Vector3 vector2 = vector * (1f / magnitude);
		vector2 *= 0f - (num + num2);
		hose.masses[p1].force += vector2;
		hose.masses[p2].force -= vector2;
	}

	public void doCalculateSpringForce1(MegaRope mod)
	{
		Vector3 vector = mod.masses[p1].pos - mod.masses[p2].pos;
		if (vector != Vector3.zero)
		{
			float magnitude = vector.magnitude;
			vector = vector.normalized;
			Vector3 vector2 = (0f - ks) * ((magnitude - restlen) * vector);
			mod.masses[p1].force += vector2;
			mod.masses[p2].force -= vector2;
			len = magnitude;
		}
	}

	public static float GetDist(MegaRope rope, MegaRopeSpring S1, MegaRopeSpring S2)
	{
		Vector3 vector = rope.masses[S1.p2].pos - rope.masses[S1.p1].pos;
		Vector3 vector2 = rope.masses[S2.p2].pos - rope.masses[S2.p1].pos;
		Vector3 vector3 = rope.masses[S1.p1].pos - rope.masses[S2.p1].pos;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(vector2, vector2);
		float num4 = Vector3.Dot(vector, vector3);
		float num5 = Vector3.Dot(vector2, vector3);
		float num6;
		float num7;
		float num8;
		float num9;
		if ((num6 = (num7 = num * num3 - num2 * num2)) < 1E-07f)
		{
			num8 = 0f;
			num7 = 1f;
			num9 = num5;
			num6 = num3;
		}
		else
		{
			num8 = num2 * num5 - num3 * num4;
			num9 = num * num5 - num2 * num4;
			if ((double)num8 < 0.0)
			{
				num8 = 0f;
				num9 = num5;
				num6 = num3;
			}
			else if (num8 > num7)
			{
				num8 = num7;
				num9 = num5 + num2;
				num6 = num3;
			}
		}
		if (num9 < 0f)
		{
			num9 = 0f;
			if (0f - num4 < 0f)
			{
				num8 = 0f;
			}
			else if (0f - num4 > num)
			{
				num8 = num7;
			}
			else
			{
				num8 = 0f - num4;
				num7 = num;
			}
		}
		else if (num9 > num6)
		{
			num9 = num6;
			if ((double)(0f - num4 + num2) < 0.0)
			{
				num8 = 0f;
			}
			else if (0f - num4 + num2 > num)
			{
				num8 = num7;
			}
			else
			{
				num8 = 0f - num4 + num2;
				num7 = num;
			}
		}
		float num10 = ((!(Mathf.Abs(num8) < 1E-07f)) ? (num8 / num7) : 0f);
		float num11 = ((!(Mathf.Abs(num9) < 1E-07f)) ? (num9 / num6) : 0f);
		return (vector3 + num10 * vector - num11 * vector2).magnitude;
	}
}
