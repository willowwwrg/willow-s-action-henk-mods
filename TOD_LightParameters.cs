using System;
using UnityEngine;

[Serializable]
public class TOD_LightParameters
{
	public float Falloff = 0.7f;

	public float Coloring = 0.5f;

	public float SkyColoring = 0.5f;

	public float CloudColoring = 0.9f;

	public void CheckRange()
	{
		Falloff = Mathf.Clamp01(Falloff);
		Coloring = Mathf.Clamp01(Coloring);
		SkyColoring = Mathf.Clamp01(SkyColoring);
		CloudColoring = Mathf.Clamp01(CloudColoring);
	}
}
