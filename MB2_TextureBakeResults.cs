using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MB2_TextureBakeResults : ScriptableObject
{
	public MB_AtlasesAndRects[] combinedMaterialInfo;

	public Material[] materials;

	public Rect[] prefabUVRects;

	public Material resultMaterial;

	public MB_MultiMaterial[] resultMaterials;

	public bool doMultiMaterial;

	public bool fixOutOfBoundsUVs;

	public Dictionary<Material, Rect> GetMat2RectMap()
	{
		Dictionary<Material, Rect> dictionary = new Dictionary<Material, Rect>();
		if (materials == null || prefabUVRects == null || materials.Length != prefabUVRects.Length)
		{
			Debug.LogWarning("Bad TextureBakeResults could not build mat2UVRect map");
		}
		else
		{
			for (int i = 0; i < materials.Length; i++)
			{
				dictionary.Add(materials[i], prefabUVRects[i]);
			}
		}
		return dictionary;
	}

	public string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Shaders:\n");
		HashSet<Shader> hashSet = new HashSet<Shader>();
		if (materials != null)
		{
			for (int i = 0; i < materials.Length; i++)
			{
				hashSet.Add(materials[i].shader);
			}
		}
		foreach (Shader item in hashSet)
		{
			stringBuilder.Append("  ").Append(item.name).AppendLine();
		}
		stringBuilder.Append("Materials:\n");
		if (materials != null)
		{
			for (int j = 0; j < materials.Length; j++)
			{
				stringBuilder.Append("  ").Append(materials[j].name).AppendLine();
			}
		}
		return stringBuilder.ToString();
	}
}
