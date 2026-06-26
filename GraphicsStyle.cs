using System;
using UnityEngine;

[Serializable]
public class GraphicsStyle
{
	public string name;

	public LevelStyle style;

	public ColorCorrectionSettings colorCorrectionSettings;

	public BloomSettings bloomSettings;

	public IlluminationSettings illuminationSettings;

	public MaterialOverride[] materialOverrides;

	public FogSettings fogSettings;

	public Material skybox;
}
