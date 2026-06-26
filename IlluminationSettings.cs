using System;
using UnityEngine;

[Serializable]
public class IlluminationSettings
{
	public bool illuminateGameplayAssets;

	public float specularIntensity;

	public float glowStrength;

	public float diffuseEmission;

	public Color specularColorMultiply = Color.white;

	public Material characterMaterialOverride;

	public Material hueCycleMaterial;

	public float blackOverride;
}
