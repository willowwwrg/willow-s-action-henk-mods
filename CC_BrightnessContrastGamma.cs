using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Brightness, Contrast, Gamma")]
public class CC_BrightnessContrastGamma : CC_Base
{
	public float redCoeff = 0.5f;

	public float greenCoeff = 0.5f;

	public float blueCoeff = 0.5f;

	public float brightness;

	public float contrast;

	public float gamma = 1f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_rCoeff", redCoeff);
		base.material.SetFloat("_gCoeff", greenCoeff);
		base.material.SetFloat("_bCoeff", blueCoeff);
		base.material.SetFloat("_brightness", (brightness + 100f) * 0.01f);
		base.material.SetFloat("_contrast", (contrast + 100f) * 0.01f);
		base.material.SetFloat("_gamma", gamma);
		Graphics.Blit(source, destination, base.material);
	}
}
