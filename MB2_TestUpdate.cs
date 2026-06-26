using UnityEngine;

public class MB2_TestUpdate : MonoBehaviour
{
	public MB2_MeshBaker meshbaker;

	public GameObject[] objsToMove;

	public GameObject objWithChangingUVs;

	private Vector2[] uvs;

	private Mesh m;

	private void Start()
	{
		meshbaker.AddDeleteGameObjects(objsToMove, null);
		meshbaker.AddDeleteGameObjects(new GameObject[1] { objWithChangingUVs }, null);
		MeshFilter component = objWithChangingUVs.GetComponent<MeshFilter>();
		m = component.sharedMesh;
		uvs = m.uv;
		meshbaker.Apply();
	}

	private void LateUpdate()
	{
		meshbaker.UpdateGameObjects(objsToMove, recalcBounds: false);
		Vector2[] uv = m.uv;
		for (int i = 0; i < uv.Length; i++)
		{
			ref Vector2 reference = ref uv[i];
			reference = Mathf.Sin(Time.time) * uvs[i];
		}
		m.uv = uv;
		meshbaker.UpdateGameObjects(new GameObject[1] { objWithChangingUVs }, recalcBounds: true, updateVertices: true, updateNormals: true, updateTangents: true, updateUV: true);
		meshbaker.Apply(triangles: false, vertices: true, normals: true, tangents: true, uvs: true, colors: false, uv1: false, uv2: false);
	}
}
