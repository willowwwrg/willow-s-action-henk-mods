using System;
using UnityEngine;

[ExecuteInEditMode]
public class WoodSupport : SplineFollow
{
	[Serializable]
	private class OrientedBlock
	{
		public Mesh data;

		public Vector3 angles;
	}

	[SerializeField]
	private OrientedBlock[] availableBlocks;

	protected Mesh _output;

	public void RegenMesh()
	{
		MeshData meshData = new MeshData();
		Create(meshData);
		MeshFilter component = GetComponent<MeshFilter>();
		if (!(component != null))
		{
			return;
		}
		if (_output == null)
		{
			_output = new Mesh();
			if (component.sharedMesh != null)
			{
				UnityEngine.Object.DestroyImmediate(component.sharedMesh);
			}
			meshData.Apply(_output);
			component.sharedMesh = _output;
		}
		else
		{
			_output.Clear();
			meshData.Apply(_output);
		}
	}

	public void UpdateSupport()
	{
		RegenMesh();
		Update();
	}

	protected void Create(MeshData intermediateData)
	{
		if (availableBlocks == null || availableBlocks.Length == 0)
		{
			return;
		}
		Vector3 raycastPosition = GetRaycastPosition();
		Debug.DrawRay(raycastPosition, Vector3.up * 20f, Color.red);
		if (Physics.Raycast(raycastPosition, Vector3.up, out var hitInfo, 1000f) && !PlasticSupport.IsPlastic(hitInfo.transform.gameObject))
		{
			Debug.DrawRay(raycastPosition, Vector3.up * hitInfo.distance, Color.green);
			UnityEngine.Random.seed = base.gameObject.GetInstanceID();
			float num = Mathf.Round(hitInfo.distance);
			int num2 = 100;
			while (num > 0f && num2 > 0)
			{
				num2--;
				OrientedBlock orientedBlock = availableBlocks[UnityEngine.Random.Range(0, availableBlocks.Length)];
				Matrix4x4 localOffset = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(orientedBlock.angles), Vector3.one);
				float y = localOffset.MultiplyVector(orientedBlock.data.bounds.size).y;
				float y2 = orientedBlock.data.bounds.min.y;
				num -= y;
				localOffset.SetColumn(3, new Vector4(0f, num - y2, 0f, 1f));
				intermediateData.AppendMesh(orientedBlock.data, localOffset);
			}
		}
	}
}
