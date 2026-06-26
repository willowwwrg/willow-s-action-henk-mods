using UnityEngine;

public abstract class SupportBase : MonoBehaviour
{
	protected Mesh _output;

	protected void Awake()
	{
		if (_output == null)
		{
			_output = new Mesh();
		}
		else
		{
			_output.Clear();
		}
		MeshData meshData = new MeshData();
		Create(meshData);
		meshData.Apply(_output);
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null)
		{
			component.sharedMesh = _output;
		}
	}

	protected abstract void Create(MeshData intermediateData);
}
