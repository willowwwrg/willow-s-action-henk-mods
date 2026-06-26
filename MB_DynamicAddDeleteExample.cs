using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MB_DynamicAddDeleteExample : MonoBehaviour
{
	public GameObject prefab;

	private List<GameObject> objsInCombined = new List<GameObject>();

	private MB2_MeshBaker mbd;

	private GameObject[] objs;

	private void Start()
	{
		mbd = GetComponent<MB2_MeshBaker>();
		int num = 25;
		GameObject[] array = new GameObject[num * num];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(prefab);
				array[i * num + j] = gameObject.GetComponentInChildren<MeshRenderer>().gameObject;
				gameObject.transform.position = new Vector3(9f * (float)i, 0f, 9f * (float)j);
				if ((i * num + j) % 3 == 0)
				{
					objsInCombined.Add(array[i * num + j]);
				}
			}
		}
		mbd.AddDeleteGameObjects(array, null);
		mbd.Apply();
		objs = objsInCombined.ToArray();
		StartCoroutine(largeNumber());
	}

	private IEnumerator largeNumber()
	{
		while (true)
		{
			yield return new WaitForSeconds(1.5f);
			mbd.AddDeleteGameObjects(null, objs);
			mbd.Apply(triangles: true, vertices: true, normals: true, tangents: true, uvs: true, colors: false, uv1: false, uv2: false);
			yield return new WaitForSeconds(1.5f);
			mbd.AddDeleteGameObjects(objs, null);
			mbd.Apply(triangles: true, vertices: true, normals: true, tangents: true, uvs: true, colors: false, uv1: false, uv2: false);
		}
	}
}
