using System;
using UnityEngine;

[Serializable]
public class TOD_NightParameters
{
	public Color AdditiveColor = Color.black;

	public Color MoonMeshColor = new Color32(byte.MaxValue, 233, 200, byte.MaxValue);

	public Color MoonLightColor = new Color32(181, 204, byte.MaxValue, byte.MaxValue);

	public Color MoonHaloColor = new Color32(81, 104, 155, byte.MaxValue);

	public float MoonMeshSize = 1f;

	public float MoonLightIntensity = 0.1f;

	public float AmbientIntensity = 0.2f;

	public float ShadowStrength = 1f;

	public float SkyMultiplier = 0.1f;

	public float CloudMultiplier = 0.2f;

	public void CheckRange()
	{
		MoonLightIntensity = Mathf.Max(0f, MoonLightIntensity);
		MoonMeshSize = Mathf.Max(0f, MoonMeshSize);
		AmbientIntensity = Mathf.Clamp01(AmbientIntensity);
		ShadowStrength = Mathf.Clamp01(ShadowStrength);
		SkyMultiplier = Mathf.Clamp01(SkyMultiplier);
		CloudMultiplier = Mathf.Clamp01(CloudMultiplier);
	}
}
