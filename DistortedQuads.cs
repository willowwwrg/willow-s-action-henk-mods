using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class DistortedQuads : MonoBehaviour
{
	[SerializeField]
	private float numQuads = 10f;

	[SerializeField]
	private float distortionWeight = 0.4f;

	[SerializeField]
	private int seed;

	[SerializeField]
	private Vector2 spawnSpread = new Vector2(5f, 2f);

	[SerializeField]
	private Vector2 heightSpread = new Vector2(-7f, 7f);

	[SerializeField]
	private Transform intermediateCamera;

	private List<Vector3> originalVertices;

	private List<Vector3> deformedVertices;

	private List<Vector2> uvs;

	private List<int> triangles;

	private List<Vector3> origins;

	private List<float> temperatures;

	private List<float> scales;

	private List<Color> colors;

	private MeshFilter f;

	[SerializeField]
	[HideInInspector]
	private Mesh m;

	private void OnEnable()
	{
		Random.seed = seed;
		originalVertices = new List<Vector3>();
		uvs = new List<Vector2>();
		triangles = new List<int>();
		origins = new List<Vector3>();
		temperatures = new List<float>();
		scales = new List<float>();
		colors = new List<Color>();
		for (int i = 0; (float)i < numQuads; i++)
		{
			originalVertices.AddRange(new Vector3[4]
			{
				Vector3.Lerp(new Vector3(-1f, -1f, 0f), new Vector3(0f - Random.value, 0f - Random.value, 0f), distortionWeight),
				Vector3.Lerp(new Vector3(1f, -1f, 0f), new Vector3(Random.value, 0f - Random.value, 0f), distortionWeight),
				Vector3.Lerp(new Vector3(1f, 1f, 0f), new Vector3(Random.value, Random.value, 0f), distortionWeight),
				Vector3.Lerp(new Vector3(-1f, 1f, 0f), new Vector3(0f - Random.value, Random.value, 0f), distortionWeight)
			});
			uvs.AddRange(new Vector2[4]
			{
				Vector2.zero,
				Vector2.up,
				Vector2.one,
				Vector2.right
			});
			int num = i * 4;
			triangles.AddRange(new int[6]
			{
				num,
				num + 2,
				num + 1,
				num + 3,
				num + 2,
				num
			});
			origins.Add(new Vector3(spawnSpread.x * (Random.value - 0.5f), spawnSpread.y * (Random.value - 0.5f), spawnSpread.x * (Random.value - 0.5f)));
			temperatures.Add(Random.value);
			float num2 = Random.value * 1.5f + 0.5f;
			scales.Add(num2);
			Color color = new Color(num2 * 0.5f, 0f, 0f, 0f);
			colors.AddRange(new Color[4] { color, color, color, color });
		}
		if (m == null)
		{
			m = new Mesh();
		}
		m.vertices = originalVertices.ToArray();
		m.uv = uvs.ToArray();
		m.triangles = triangles.ToArray();
		m.colors = colors.ToArray();
		f = GetComponent<MeshFilter>();
		f.sharedMesh = m;
	}

	private void Update()
	{
		deformedVertices = new List<Vector3>();
		float num = heightSpread.y - heightSpread.x;
		Vector3 normalized = (Camera.main.transform.position - base.transform.position).normalized;
		Quaternion quaternion = Quaternion.LookRotation(-normalized, Vector3.up);
		intermediateCamera.position = base.transform.position + normalized;
		intermediateCamera.rotation = quaternion;
		for (int i = 0; (float)i < numQuads; i++)
		{
			float num2 = (origins[i].y - heightSpread.x) / num;
			List<float> list = temperatures;
			int index2;
			int index = (index2 = i);
			float num3 = list[index2];
			list[index] = num3 - Time.deltaTime * ((num2 - 0.5f) / scales[i]);
			List<Vector3> list2 = origins;
			int index3 = (index2 = i);
			Vector3 vector = list2[index2];
			list2[index3] = vector + Time.deltaTime * (Vector3.up * temperatures[i]);
			for (int j = 0; j < 4; j++)
			{
				deformedVertices.Add(quaternion * originalVertices[i * 4 + j] * scales[i] + origins[i]);
			}
		}
		m.vertices = deformedVertices.ToArray();
		f.sharedMesh = m;
	}
}
