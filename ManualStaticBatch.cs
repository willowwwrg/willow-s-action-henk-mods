using System.Collections.Generic;
using UnityEngine;

public class ManualStaticBatch : MonoBehaviour
{
	[SerializeField]
	private GameObject[] directOwners;

	[SerializeField]
	private bool deleteParentsIfEmpty;

	private List<Transform> postProcessParents = new List<Transform>();

	public void Set(GameObject[] directOwners, bool deleteParentsIfEmpty = false)
	{
		this.directOwners = directOwners;
		this.deleteParentsIfEmpty = deleteParentsIfEmpty;
		Awake();
	}

	private void Awake()
	{
		if (directOwners == null)
		{
			return;
		}
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (GetComponent<MeshRenderer>() == null)
		{
			base.gameObject.AddComponent<MeshRenderer>();
		}
		Mesh mesh = meshFilter.sharedMesh;
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<Vector2> list3 = new List<Vector2>();
		List<Vector3> list4 = new List<Vector3>();
		List<Vector4> list5 = new List<Vector4>();
		List<Color> list6 = new List<Color>();
		List<int> list7 = new List<int>();
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		bool[] array = new bool[5];
		for (int i = 0; i < directOwners.Length; i++)
		{
			if (!directOwners[i])
			{
				continue;
			}
			MeshFilter component = directOwners[i].GetComponent<MeshFilter>();
			MeshRenderer component2 = directOwners[i].GetComponent<MeshRenderer>();
			if (component == null || component2 == null)
			{
				continue;
			}
			Mesh sharedMesh = component.sharedMesh;
			Matrix4x4 localToWorldMatrix2 = directOwners[i].transform.localToWorldMatrix;
			Matrix4x4 worldToLocalMatrix2 = directOwners[i].transform.worldToLocalMatrix;
			int vertexCount = sharedMesh.vertexCount;
			int count = list.Count;
			list.AddRange(sharedMesh.vertices);
			if (sharedMesh.uv != null && sharedMesh.uv.Length == sharedMesh.vertices.Length)
			{
				list2.AddRange(sharedMesh.uv);
				array[0] = true;
			}
			else
			{
				for (int j = 0; j < vertexCount; j++)
				{
					list2.Add(Vector2.zero);
				}
			}
			if (sharedMesh.uv2 != null && sharedMesh.uv2.Length == sharedMesh.vertices.Length)
			{
				list3.AddRange(sharedMesh.uv2);
				array[1] = true;
			}
			else
			{
				for (int k = 0; k < vertexCount; k++)
				{
					list3.Add(Vector2.zero);
				}
			}
			if (sharedMesh.normals != null && sharedMesh.normals.Length == sharedMesh.vertices.Length)
			{
				list4.AddRange(sharedMesh.normals);
				array[2] = true;
			}
			else
			{
				for (int l = 0; l < vertexCount; l++)
				{
					list4.Add(Vector3.up);
				}
			}
			if (sharedMesh.tangents != null && sharedMesh.tangents.Length == sharedMesh.vertices.Length)
			{
				list5.AddRange(sharedMesh.tangents);
				array[3] = true;
			}
			else
			{
				for (int m = 0; m < vertexCount; m++)
				{
					list5.Add(Vector3.right);
				}
			}
			if (sharedMesh.colors != null && sharedMesh.colors.Length == sharedMesh.vertices.Length)
			{
				list6.AddRange(sharedMesh.colors);
				array[4] = true;
			}
			else
			{
				for (int n = 0; n < vertexCount; n++)
				{
					list6.Add(Color.white);
				}
			}
			for (int num = 0; num < vertexCount; num++)
			{
				list[count + num] = worldToLocalMatrix.MultiplyPoint(localToWorldMatrix2.MultiplyPoint(list[count + num]));
				list4[count + num] = localToWorldMatrix.MultiplyVector(worldToLocalMatrix2.MultiplyVector(list4[count + num])).normalized;
				list5[count + num] = worldToLocalMatrix.MultiplyVector(localToWorldMatrix2.MultiplyPoint(list5[count + num])).normalized;
			}
			int num2 = sharedMesh.triangles.Length;
			int count2 = list7.Count;
			list7.AddRange(sharedMesh.triangles);
			for (int num3 = 0; num3 < num2; num3++)
			{
				int index2;
				int index = (index2 = count2 + num3);
				index2 = list7[index2];
				list7[index] = index2 + count;
			}
			int num4 = 2;
			if (directOwners[i].GetComponent<Renderer>() != null)
			{
				num4++;
			}
			if (directOwners[i].GetComponents<Component>().Length == num4)
			{
				if (deleteParentsIfEmpty)
				{
					Transform parent = directOwners[i].transform.parent;
					if (parent != null && !postProcessParents.Contains(parent))
					{
						postProcessParents.Add(parent);
					}
				}
				Object.Destroy(directOwners[i]);
			}
			else
			{
				Object.Destroy(component);
				Object.Destroy(component2);
			}
		}
		mesh.vertices = list.ToArray();
		if (array[0])
		{
			mesh.uv = list2.ToArray();
		}
		if (array[1])
		{
			mesh.uv2 = list3.ToArray();
		}
		if (array[2])
		{
			mesh.normals = list4.ToArray();
		}
		if (array[3])
		{
			mesh.tangents = list5.ToArray();
		}
		if (array[4])
		{
			mesh.colors = list6.ToArray();
		}
		mesh.triangles = list7.ToArray();
		meshFilter.sharedMesh = mesh;
		if (!deleteParentsIfEmpty)
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (postProcessParents.Count == 0)
		{
			Object.Destroy(this);
			return;
		}
		for (int num = postProcessParents.Count - 1; num >= 0; num--)
		{
			Transform transform = postProcessParents[num];
			if (transform.childCount == 0)
			{
				postProcessParents.RemoveAt(num);
				Object.Destroy(transform.gameObject);
			}
			Transform parent = transform.parent;
			if (parent != null && !postProcessParents.Contains(parent) && parent.gameObject.GetComponents<Component>().Length == 1)
			{
				postProcessParents.Add(parent);
			}
		}
	}
}
