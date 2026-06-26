using UnityEngine;

[ExecuteInEditMode]
public class Pixelate : MonoBehaviour
{
	public Shader shader;

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

	private void Start()
	{
		shader = Shader.Find("Hidden/Pixelate");
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
		Graphics.Blit(source, destination, Material());
	}
}
