using System;
using UnityEngine;

[Serializable]
public class FogSettings
{
	public bool enableFog;

	public float startDistance;

	public float density = 0.1f;

	public float heightScale = 100f;

	public float height;

	public Color fogColor = Color.grey;
}
