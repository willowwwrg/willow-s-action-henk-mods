using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class VertexColor : MonoBehaviour
{
	[SerializeField]
	private Color color;

	[SerializeField]
	private bool use_normal;

	[SerializeField]
	private float normal_y_weight;

	[SerializeField]
	private float normal_y_wrap_weight = 1f;

	[SerializeField]
	private Color dark_color;

	[HideInInspector]
	[SerializeField]
	private Mesh cache;

	private void Awake()
	{
		if (cache == null)
		{
			cache = GetComponent<MeshFilter>().sharedMesh;
		}
		else
		{
			Object.Destroy(GetComponent<MeshFilter>().sharedMesh);
		}
		Mesh mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = cache.vertices;
		mesh.normals = cache.normals;
		mesh.tangents = cache.tangents;
		mesh.uv = cache.uv;
		mesh.uv2 = cache.uv2;
		mesh.triangles = cache.triangles;
		Color[] array = new Color[cache.vertexCount];
		for (int i = 0; i < array.Length; i++)
		{
			if (use_normal)
			{
				ref Color reference = ref array[i];
				reference = Color.Lerp(color, Color.Lerp(dark_color, color, Mathf.Max(0f, cache.normals[i].y * (1f - normal_y_wrap_weight) + (cache.normals[i].y * 0.5f + 0.5f) * normal_y_wrap_weight)), normal_y_weight);
			}
			else
			{
				ref Color reference2 = ref array[i];
				reference2 = color;
			}
		}
		mesh.colors = array;
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private void Update()
	{
	}
}
