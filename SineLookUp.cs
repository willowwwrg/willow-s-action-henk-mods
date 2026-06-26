using System;
using UnityEngine;

public class SineLookUp : Singleton<SineLookUp>
{
	private int lookupTableSize = 1024;

	private float[] sinLUT;

	private float[] cosLUT;

	private float stepSize;

	private float maxRad = (float)Math.PI * 2f;

	private void Awake()
	{
		cosLUT = new float[lookupTableSize];
		sinLUT = new float[lookupTableSize];
		stepSize = (float)lookupTableSize / ((float)Math.PI * 2f);
		float num = 0f;
		for (int i = 0; i < lookupTableSize; i++)
		{
			sinLUT[i] = Mathf.Sin(num);
			cosLUT[i] = Mathf.Cos(num);
			num += (float)Math.PI * 2f / (float)lookupTableSize;
		}
	}

	public float GetSin(float radians)
	{
		int num = Mathf.FloorToInt(radians * stepSize) % lookupTableSize;
		return sinLUT[num];
	}

	public float GetCos(float radians)
	{
		int num = Mathf.FloorToInt(radians * stepSize) % lookupTableSize;
		return cosLUT[num];
	}
}
