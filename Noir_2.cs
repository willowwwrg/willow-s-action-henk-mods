using UnityEngine;

[ExecuteInEditMode]
public class Noir_2 : MonoBehaviour
{
	public Shader shader;

	public float sensitivityDepth = 1f;

	public float sensitivityNormals = 1f;

	public float sampledistance = 0.5f;

	public Color edgeColor = new Color(0f, 0f, 0f, 0.7f);

	public Color rimColor = new Color(0.9f, 0.975f, 1f);

	public float rimPower = 4f;

	public float rimOffset = -0.25f;

	public float rimMultiplier = 4f;

	public Color fogColor = new Color(0.9f, 0.975f, 1f);

	public float fogPower = 2f;

	public float fogOffset;

	public float fogMultiplier = 2f;

	private Material material;

	private Material Material()
	{
		if (material == null)
		{
			material = new Material(shader);
			material.hideFlags = HideFlags.HideAndDontSave;
		}
		return material;
	}

	private void OnDisable()
	{
		if ((bool)material)
		{
			Object.DestroyImmediate(material);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.camera.depthTextureMode = DepthTextureMode.DepthNormals;
		Material material = Material();
		material.SetVector("_Sensitivity", new Vector2(sensitivityDepth, sensitivityNormals));
		material.SetFloat("_SampleDistance", sampledistance);
		material.SetVector("_EdgeColor", edgeColor);
		material.SetVector("_RimColor", rimColor);
		material.SetVector("_RimOptions", new Vector3(rimPower, rimOffset, rimMultiplier));
		material.SetVector("_FogColor", fogColor);
		material.SetVector("_FogOptions", new Vector3(fogPower, fogOffset, fogMultiplier));
		Graphics.Blit(source, destination, material);
	}
}
