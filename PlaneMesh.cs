using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class PlaneMesh : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private Mesh cache;

	[SerializeField]
	private Vector2 divisions = new Vector2(2f, 2f);

	private void OnEnable()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (!cache)
		{
			cache = new Mesh();
			cache.name = base.gameObject.name;
		}
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		int num = 0;
		for (int i = 0; (float)i < Mathf.Max(2f, divisions.y); i++)
		{
			float y = (float)i / (divisions.y - 1f);
			for (int j = 0; (float)j < Mathf.Max(2f, divisions.x); j++)
			{
				float x = (float)j / (divisions.x - 1f);
				list.Add(new Vector3(x, y));
				list2.Add(new Vector2(x, y));
				if (j > 0 && i > 0)
				{
					list3.Add(num - (int)divisions.x);
					list3.Add(num - 1);
					list3.Add(num - (int)divisions.x - 1);
					list3.Add(num);
					list3.Add(num - 1);
					list3.Add(num - (int)divisions.x);
				}
				num++;
			}
		}
		cache.Clear();
		cache.vertices = list.ToArray();
		cache.uv = list2.ToArray();
		cache.triangles = list3.ToArray();
		cache.RecalculateNormals();
		Vector4[] array = new Vector4[cache.vertexCount];
		for (int k = 0; k < cache.vertexCount; k++)
		{
			ref Vector4 reference = ref array[k];
			reference = Vector3.right;
		}
		cache.tangents = array;
		Color[] array2 = new Color[cache.vertexCount];
		for (int l = 0; l < cache.vertexCount; l++)
		{
			ref Color reference2 = ref array2[l];
			reference2 = Color.white;
		}
		cache.colors = array2;
		component.sharedMesh = cache;
	}

	private void Update()
	{
	}
}
