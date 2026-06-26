using System;
using UnityEngine;

[Serializable]
public class TOD_StarParameters
{
	public float Tiling = 2f;

	public float Density = 0.5f;

	public void CheckRange()
	{
		Tiling = Mathf.Max(0f, Tiling);
		Density = Mathf.Clamp01(Density);
	}
}
