using System;
using UnityEngine;

[Serializable]
public class TOD_CloudParameters
{
	public float Density = 3f;

	public float Sharpness = 3f;

	public float Brightness = 1f;

	public float Scale1 = 3f;

	public float Scale2 = 7f;

	public float ShadowStrength;

	public void CheckRange()
	{
		Scale1 = Mathf.Max(1f, Scale1);
		Scale2 = Mathf.Max(1f, Scale2);
		Density = Mathf.Max(0f, Density);
		Sharpness = Mathf.Max(0f, Sharpness);
		Brightness = Mathf.Max(0f, Brightness);
		ShadowStrength = Mathf.Clamp01(ShadowStrength);
	}
}
