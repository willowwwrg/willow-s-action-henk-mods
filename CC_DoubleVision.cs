using UnityEngine;

[AddComponentMenu("Colorful/Double Vision")]
[ExecuteInEditMode]
public class CC_DoubleVision : CC_Base
{
	public Vector2 displace = new Vector2(0.7f, 0f);

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetVector("_displace", new Vector2(displace.x / (float)Screen.width, displace.y / (float)Screen.height));
		Graphics.Blit(source, destination, base.material);
	}
}
