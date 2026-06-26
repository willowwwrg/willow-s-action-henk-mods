using System;
using UnityEngine;

[Serializable]
public class TOD_DayParameters
{
	public Color AdditiveColor = Color.black;

	public Color SunMeshColor = new Color32(byte.MaxValue, 233, 180, byte.MaxValue);

	public Color SunLightColor = new Color32(byte.MaxValue, 243, 234, byte.MaxValue);

	public float SunMeshSize = 1f;

	public float SunLightIntensity = 0.75f;

	public float AmbientIntensity = 0.75f;

	public float ShadowStrength = 1f;

	public float SkyMultiplier = 1f;

	public float CloudMultiplier = 1f;

	public void CheckRange()
	{
		SunLightIntensity = Mathf.Max(0f, SunLightIntensity);
		SunMeshSize = Mathf.Max(0f, SunMeshSize);
		AmbientIntensity = Mathf.Clamp01(AmbientIntensity);
		ShadowStrength = Mathf.Clamp01(ShadowStrength);
		SkyMultiplier = Mathf.Clamp01(SkyMultiplier);
		CloudMultiplier = Mathf.Clamp01(CloudMultiplier);
	}
}
