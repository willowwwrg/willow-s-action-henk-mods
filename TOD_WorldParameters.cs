using System;
using UnityEngine;

[Serializable]
public class TOD_WorldParameters
{
	public bool SetAmbientLight;

	public bool SetFogColor;

	public float FogColorBias;

	public float ViewerHeight;

	public float HorizonOffset;

	public void CheckRange()
	{
		FogColorBias = Mathf.Clamp01(FogColorBias);
		ViewerHeight = Mathf.Clamp01(ViewerHeight);
		HorizonOffset = Mathf.Clamp01(HorizonOffset);
	}
}
