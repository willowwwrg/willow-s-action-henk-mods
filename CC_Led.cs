using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/LED")]
public class CC_Led : CC_Base
{
	public float scale = 80f;

	public float brightness = 1f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_scale", scale);
		base.material.SetFloat("_brightness", brightness);
		Graphics.Blit(source, destination, base.material);
	}
}
