using UnityEngine;

[AddComponentMenu("Colorful/Photo Filter")]
[ExecuteInEditMode]
public class CC_PhotoFilter : CC_Base
{
	public Color color = new Color(1f, 0.5f, 0.2f, 1f);

	public float density = 0.35f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetColor("_rgb", color);
		base.material.SetFloat("_density", density);
		Graphics.Blit(source, destination, base.material);
	}
}
