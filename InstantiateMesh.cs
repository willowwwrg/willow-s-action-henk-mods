using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class InstantiateMesh : MonoBehaviour
{
	[SerializeField]
	private MeshFilter other;

	private void Awake()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		if (sharedMesh != other.sharedMesh)
		{
			Object.Destroy(sharedMesh);
			GetComponent<MeshFilter>().sharedMesh = other.sharedMesh;
		}
	}

	private void Update()
	{
	}
}
