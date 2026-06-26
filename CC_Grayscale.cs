using UnityEngine;

[AddComponentMenu("Colorful/Grayscale")]
[ExecuteInEditMode]
public class CC_Grayscale : CC_Base
{
	public float redLuminance = 0.3f;

	public float greenLuminance = 0.59f;

	public float blueLuminance = 0.11f;

	public float amount = 1f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_rLum", redLuminance);
		base.material.SetFloat("_gLum", greenLuminance);
		base.material.SetFloat("_bLum", blueLuminance);
		base.material.SetFloat("_amount", amount);
		Graphics.Blit(source, destination, base.material);
	}
}
