using System;
using UnityEngine;

[Serializable]
public class EditorObjBaseValues
{
	public Color color = Color.white;

	public Color specColor = Color.white;

	public float specInt;

	public float glowStrength;

	public string originalShader = string.Empty;

	public string transparentShader = string.Empty;
}
