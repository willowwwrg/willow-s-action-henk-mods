using UnityEngine;

[AddComponentMenu("Colorful/Frost")]
[ExecuteInEditMode]
public class CC_Frost : CC_Base
{
	public float scale = 1.2f;

	public float sharpness = 40f;

	public float darkness = 35f;

	public bool enableVignette = true;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_scale", scale);
		base.material.SetFloat("_enableVignette", (!enableVignette) ? 0f : 1f);
		base.material.SetFloat("_sharpness", sharpness * 0.01f);
		base.material.SetFloat("_darkness", darkness * 0.02f);
		Graphics.Blit(source, destination, base.material);
	}
}
