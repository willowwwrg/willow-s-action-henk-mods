using UnityEngine;

[AddComponentMenu("Colorful/Hue, Saturation, Value")]
[ExecuteInEditMode]
public class CC_HueSaturationValue : CC_Base
{
	public float hue;

	public float saturation;

	public float value;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_hue", hue / 360f);
		base.material.SetFloat("_saturation", saturation * 0.01f);
		base.material.SetFloat("_value", value * 0.01f);
		Graphics.Blit(source, destination, base.material);
	}
}
