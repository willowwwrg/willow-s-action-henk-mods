using UnityEngine;

public class MB_Example : MonoBehaviour
{
	public MB2_MeshBaker meshbaker;

	public GameObject[] objsToCombine;

	private void Start()
	{
		meshbaker.AddDeleteGameObjects(objsToCombine, null);
		meshbaker.Apply();
	}

	private void LateUpdate()
	{
		meshbaker.UpdateGameObjects(objsToCombine);
		meshbaker.Apply(triangles: false, vertices: true, normals: true, tangents: true, uvs: false, colors: false, uv1: false, uv2: false);
	}
}
