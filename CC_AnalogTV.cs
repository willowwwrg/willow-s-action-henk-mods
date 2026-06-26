using UnityEngine;

[AddComponentMenu("Colorful/Analog TV")]
[ExecuteInEditMode]
public class CC_AnalogTV : CC_Base
{
	public float phase = 0.01f;

	public bool grayscale;

	public float noiseIntensity = 0.5f;

	public float scanlinesIntensity = 2f;

	public float scanlinesCount = 1024f;

	public float distortion = 0.2f;

	public float cubicDistortion = 0.6f;

	public float scale = 0.8f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_phase", phase);
		base.material.SetFloat("_grayscale", (!grayscale) ? 0f : 1f);
		base.material.SetFloat("_noiseIntensity", noiseIntensity);
		base.material.SetFloat("_scanlinesIntensity", scanlinesIntensity);
		base.material.SetFloat("_scanlinesCount", (int)scanlinesCount);
		base.material.SetFloat("_distortion", distortion);
		base.material.SetFloat("_cubicDistortion", cubicDistortion);
		base.material.SetFloat("_scale", scale);
		Graphics.Blit(source, destination, base.material);
	}
}
