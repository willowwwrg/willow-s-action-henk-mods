using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaSplineAnim
{
	public bool Enabled;

	public List<MegaKnotAnimCurve> knots = new List<MegaKnotAnimCurve>();

	public void SetState(MegaSpline spline, float t)
	{
	}

	public void GetState1(MegaSpline spline, float t)
	{
		for (int i = 0; i < knots.Count; i++)
		{
			knots[i].GetState(spline.knots[i], t);
		}
	}

	private int FindKey(float t)
	{
		if (knots.Count > 0)
		{
			Keyframe[] keys = knots[0].px.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				if (keys[i].time == t)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void AddState(MegaSpline spline, float t)
	{
		if (knots.Count == 0)
		{
			Init(spline);
		}
		int num = FindKey(t);
		if (num == -1)
		{
			for (int i = 0; i < spline.knots.Count; i++)
			{
				knots[i].AddKey(spline.knots[i], t);
			}
		}
		else
		{
			for (int j = 0; j < spline.knots.Count; j++)
			{
				knots[j].MoveKey(spline.knots[j], t, num);
			}
		}
	}

	public void Remove(float t)
	{
		int num = FindKey(t);
		if (num != -1)
		{
			for (int i = 0; i < knots.Count; i++)
			{
				knots[i].RemoveKey(num);
			}
		}
	}

	public void RemoveKey(int k)
	{
		if (k < NumKeys())
		{
			for (int i = 0; i < knots.Count; i++)
			{
				knots[i].RemoveKey(k);
			}
		}
	}

	public void Init(MegaSpline spline)
	{
		knots.Clear();
		for (int i = 0; i < spline.knots.Count; i++)
		{
			MegaKnotAnimCurve megaKnotAnimCurve = new MegaKnotAnimCurve();
			megaKnotAnimCurve.MoveKey(spline.knots[i], 0f, 0);
			knots.Add(megaKnotAnimCurve);
		}
	}

	public int NumKeys()
	{
		if (knots == null || knots.Count == 0)
		{
			return 0;
		}
		return knots[0].px.keys.Length;
	}

	public float GetKeyTime(int k)
	{
		if (knots == null || knots.Count == 0)
		{
			return 0f;
		}
		Keyframe[] keys = knots[0].px.keys;
		if (k < keys.Length)
		{
			return keys[k].time;
		}
		return 0f;
	}

	public void SetKeyTime(MegaSpline spline, int k, float t)
	{
		if (knots != null && knots.Count != 0)
		{
			for (int i = 0; i < spline.knots.Count; i++)
			{
				knots[i].MoveKey(spline.knots[i], t, k);
			}
		}
	}

	public void GetKey(MegaSpline spline, int k)
	{
		float keyTime = GetKeyTime(k);
		GetState1(spline, keyTime);
		spline.CalcLength();
	}

	public void UpdateKey(MegaSpline spline, int k)
	{
		float keyTime = GetKeyTime(k);
		for (int i = 0; i < spline.knots.Count; i++)
		{
			knots[i].MoveKey(spline.knots[i], keyTime, k);
		}
	}
}
