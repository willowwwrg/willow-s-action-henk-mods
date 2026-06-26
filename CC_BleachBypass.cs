using UnityEngine;

[AddComponentMenu("Colorful/Bleach Bypass")]
[ExecuteInEditMode]
public class CC_BleachBypass : CC_Base
{
	public float amount = 1f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_amount", amount);
		Graphics.Blit(source, destination, base.material);
	}
}
