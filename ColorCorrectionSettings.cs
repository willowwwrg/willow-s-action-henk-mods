using System;
using UnityEngine;

[Serializable]
public class ColorCorrectionSettings
{
	public Texture2D LutTexture;

	public Texture2D LutBlendTexture;

	public float BlendAmount;
}
