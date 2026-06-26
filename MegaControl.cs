using System;
using UnityEngine;

[Serializable]
public class MegaControl
{
	public float[] Times;

	[HideInInspector]
	public int lastkey;

	[HideInInspector]
	public float lasttime;

	public virtual float GetFloat(float time)
	{
		return 0f;
	}

	public virtual Vector3 GetVector3(float time)
	{
		return Vector3.zero;
	}

	private int BinSearch(float t, int low, int high)
	{
		int num = 0;
		while (high - low > 1)
		{
			num = (high + low) / 2;
			if (t < Times[num])
			{
				high = num;
				continue;
			}
			if (!(t > Times[num + 1]))
			{
				break;
			}
			low = num;
		}
		return num;
	}

	public int GetKey(float t)
	{
		if (t <= Times[1])
		{
			return 0;
		}
		if (t >= Times[Times.Length - 1])
		{
			return Times.Length - 2;
		}
		int num = lastkey;
		if (t >= Times[num] && t < Times[num + 1])
		{
			return num;
		}
		return BinSearch(t, -1, Times.Length - 1);
	}
}
