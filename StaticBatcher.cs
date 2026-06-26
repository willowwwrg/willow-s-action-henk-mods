using System.Collections.Generic;
using UnityEngine;

public class StaticBatcher : MonoBehaviour
{
	private const int MAX_VERTEX_COUNT = 65536;

	public static bool init;

	public bool deleteEmptiedTransforms = true;

	private void Awake()
	{
		if (init)
		{
			return;
		}
		init = true;
		List<GameObject> list = new List<GameObject>(Object.FindObjectsOfType<GameObject>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!list[num].isStatic)
			{
				list.RemoveAt(num);
			}
			else if (list[num].GetComponent<MeshRenderer>() == null)
			{
				list.RemoveAt(num);
			}
			else if (list[num].GetComponent<MeshFilter>() == null)
			{
				list.RemoveAt(num);
			}
		}
		Dictionary<Material, List<GameObject>> dictionary = new Dictionary<Material, List<GameObject>>();
		for (int i = 0; i < list.Count; i++)
		{
			MeshRenderer component = list[i].GetComponent<MeshRenderer>();
			Material[] sharedMaterials = component.sharedMaterials;
			foreach (Material key in sharedMaterials)
			{
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, new List<GameObject> { list[i] });
				}
				else
				{
					dictionary[key].Add(list[i]);
				}
			}
			Object.Destroy(component);
		}
		foreach (Material key2 in dictionary.Keys)
		{
			while (dictionary[key2].Count > 0)
			{
				GameObject obj = new GameObject();
				obj.AddComponent<MeshRenderer>().sharedMaterial = key2;
				ManualStaticBatch manualStaticBatch = obj.AddComponent<ManualStaticBatch>();
				int num2 = 65536;
				List<GameObject> list2 = new List<GameObject>();
				for (int num3 = dictionary[key2].Count - 1; num3 >= 0; num3--)
				{
					int vertexCount = dictionary[key2][num3].GetComponent<MeshFilter>().sharedMesh.vertexCount;
					if (vertexCount <= num2)
					{
						num2 -= vertexCount;
						list2.Add(dictionary[key2][num3]);
						dictionary[key2].RemoveAt(num3);
					}
				}
				manualStaticBatch.Set(list2.ToArray(), deleteEmptiedTransforms);
			}
		}
	}
}
