using UnityEngine;

[ExecuteInEditMode]
public class Noir : MonoBehaviour
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

	private void OnDisable()
	{
		if ((bool)material)
		{
			Object.DestroyImmediate(material);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material mat = Material();
		Graphics.Blit(source, destination, mat);
	}
}
