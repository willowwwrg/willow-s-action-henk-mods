using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
	public List<Vector3> vertices = new List<Vector3>();

	public List<Color> colors = new List<Color>();

	public List<Vector3> normals = new List<Vector3>();

	public List<Vector4> tangents = new List<Vector4>();

	public List<Vector2> uv = new List<Vector2>();

	public List<Vector2> uv2 = new List<Vector2>();

	public List<int> triangles = new List<int>();

	public List<int> triangles2 = new List<int>();

	public bool colorsUsed;

	public bool normalsUsed;

	public bool tangentsUsed;

	public bool uvUsed;

	public bool uv2Used;

	public void Apply(Mesh mesh)
	{
		mesh.vertices = vertices.ToArray();
		if (colorsUsed)
		{
			mesh.colors = colors.ToArray();
		}
		if (normalsUsed)
		{
			mesh.normals = normals.ToArray();
		}
		if (tangentsUsed)
		{
			mesh.tangents = tangents.ToArray();
		}
		if (uvUsed)
		{
			mesh.uv = uv.ToArray();
		}
		if (uv2Used)
		{
			mesh.uv2 = uv2.ToArray();
		}
		if (triangles2.Count == 0)
		{
			mesh.triangles = triangles.ToArray();
			return;
		}
		mesh.subMeshCount = 2;
		mesh.SetTriangles(triangles.ToArray(), 1);
		mesh.SetTriangles(triangles2.ToArray(), 0);
	}

	public void AppendMesh(Mesh mesh, Matrix4x4 localOffset)
	{
		DoAppendMesh(mesh, localOffset, 0);
	}

	public void AppendMesh2(Mesh mesh, Matrix4x4 localOffset)
	{
		DoAppendMesh(mesh, localOffset, 1);
	}

	public void DoAppendMesh(Mesh mesh, Matrix4x4 localOffset, int subMesh)
	{
		int count = vertices.Count;
		Vector3[] array = mesh.vertices;
		Color[] array2 = mesh.colors;
		Vector3[] array3 = mesh.normals;
		Vector4[] array4 = mesh.tangents;
		Vector2[] array5 = mesh.uv;
		Vector2[] array6 = mesh.uv2;
		int[] array7 = mesh.triangles;
		Matrix4x4 transpose = localOffset.inverse.transpose;
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			vertices.Add(localOffset.MultiplyPoint(array[i]));
			if (array2 != null && array2.Length == array.Length)
			{
				colorsUsed = true;
				colors.Add(array2[i]);
			}
			else
			{
				colors.Add(Color.white);
			}
			if (array3 != null && array3.Length == array.Length)
			{
				normalsUsed = true;
				normals.Add(transpose.MultiplyVector(array3[i]).normalized);
			}
			else
			{
				normals.Add(Vector3.up);
			}
			if (array4 != null && array4.Length == array.Length)
			{
				tangentsUsed = true;
				Vector4 item = localOffset.MultiplyVector(array4[i]).normalized;
				item.w = array4[i].w;
				tangents.Add(item);
			}
			else
			{
				tangents.Add(new Vector4(1f, 0f, 0f, 1f));
			}
			if (array4 != null && array5.Length == array.Length)
			{
				uvUsed = true;
				uv.Add(array5[i]);
			}
			else
			{
				uv2.Add(Vector2.zero);
			}
			if (array4 != null && array6.Length == array.Length)
			{
				uv2Used = true;
				uv2.Add(array6[i]);
			}
			else
			{
				uv2.Add(Vector2.zero);
			}
		}
		switch (subMesh)
		{
		case 0:
		{
			for (int k = 0; k < array7.Length; k++)
			{
				triangles.Add(array7[k] + count);
			}
			break;
		}
		case 1:
		{
			for (int j = 0; j < array7.Length; j++)
			{
				triangles2.Add(array7[j] + count);
			}
			break;
		}
		}
	}
}
