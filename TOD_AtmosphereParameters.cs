using System;
using UnityEngine;

[Serializable]
public class TOD_AtmosphereParameters
{
	public Color ScatteringColor = Color.white;

	public float RayleighMultiplier = 1f;

	public float MieMultiplier = 1f;

	public float Brightness = 1f;

	public float Contrast = 1f;

	public float Directionality = 0.5f;

	public float Haziness = 0.5f;

	public float Fogginess;

	public void CheckRange()
	{
		MieMultiplier = Mathf.Max(0f, MieMultiplier);
		RayleighMultiplier = Mathf.Max(0f, RayleighMultiplier);
		Brightness = Mathf.Max(0f, Brightness);
		Contrast = Mathf.Max(0f, Contrast);
		Directionality = Mathf.Clamp01(Directionality);
		Haziness = Mathf.Clamp01(Haziness);
		Fogginess = Mathf.Clamp01(Fogginess);
	}
}
