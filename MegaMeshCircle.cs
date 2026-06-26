using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MegaMeshCircle : MonoBehaviour
{
	public float radius = 1f;

	public int segments = 8;

	public float start;

	public float angle = 360f;

	public bool GenUV = true;

	public bool PhysUV = true;

	public MegaAxis axis = MegaAxis.Y;

	public Vector2 uvoffset = Vector2.zero;

	public Vector2 uvscale = Vector2.one;

	public float uvrotate;

	public bool calcNormals = true;

	public bool optimize;

	public bool calcBounds = true;

	public bool flip;

	private Vector3[] verts;

	private Vector2[] uvs;

	private int[] tris;

	private void Reset()
	{
		Rebuild();
	}

	[ContextMenu("Rebuild")]
	public void Rebuild()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null)
		{
			Mesh mesh = component.sharedMesh;
			if (mesh == null)
			{
				Mesh mesh2 = (component.sharedMesh = new Mesh());
				mesh = mesh2;
			}
			if (mesh != null)
			{
				BuildMesh(mesh);
			}
		}
	}

	public void BuildMesh(Mesh mesh)
	{
		int num = segments + 2;
		int num2 = segments * 3;
		if (verts == null || verts.Length != num)
		{
			verts = new Vector3[num];
			uvs = new Vector2[num];
			tris = new int[num2];
		}
		float num3 = (float)Math.PI / 180f * start - angle * 0.5f * ((float)Math.PI / 180f);
		float num4 = (float)Math.PI / 180f * angle / (float)segments;
		ref Vector3 reference = ref verts[0];
		reference = Vector3.zero;
		Vector3 zero = Vector3.zero;
		int index = 0;
		int index2 = 2;
		switch (axis)
		{
		case MegaAxis.X:
			index = 1;
			index2 = 2;
			break;
		case MegaAxis.Y:
			index = 0;
			index2 = 2;
			break;
		case MegaAxis.Z:
			index = 0;
			index2 = 1;
			break;
		}
		for (int i = 0; i <= segments; i++)
		{
			float f = num3 + (float)i * num4;
			zero[index] = radius * Mathf.Cos(f);
			zero[index2] = radius * Mathf.Sin(f);
			verts[i + 1] = zero;
		}
		if (flip)
		{
			for (int j = 0; j < segments; j++)
			{
				tris[j * 3] = 0;
				tris[j * 3 + 2] = j + 1;
				tris[j * 3 + 1] = j + 2;
			}
		}
		else
		{
			for (int k = 0; k < segments; k++)
			{
				tris[k * 3] = 0;
				tris[k * 3 + 1] = k + 1;
				tris[k * 3 + 2] = k + 2;
			}
		}
		if (GenUV)
		{
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, uvrotate, 0f), Vector3.one);
			if (PhysUV)
			{
				for (int l = 0; l < verts.Length; l++)
				{
					Vector3 v = verts[l];
					v = matrix4x.MultiplyPoint(v);
					uvs[l].x = v[index] * uvscale.x + uvoffset.x;
					uvs[l].y = v[index2] * uvscale.y + uvoffset.y;
				}
			}
			else
			{
				ref Vector2 reference2 = ref uvs[0];
				reference2 = new Vector2(0.5f, 0.5f);
				uvs[0] += uvoffset;
				Vector2 zero2 = Vector2.zero;
				for (int m = 0; m <= segments; m++)
				{
					float f2 = num3 + (float)m * num4 + uvrotate * ((float)Math.PI / 180f);
					zero2.x = 0.5f + (Mathf.Cos(f2) * uvscale.x + uvoffset.x);
					zero2.y = 0.5f + (Mathf.Sin(f2) * uvscale.y + uvoffset.y);
					uvs[m + 1] = zero2;
				}
			}
		}
		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		if (GenUV)
		{
			mesh.uv = uvs;
		}
		mesh.triangles = tris;
		if (calcNormals)
		{
			mesh.RecalculateNormals();
		}
		if (optimize)
		{
			mesh.Optimize();
		}
		if (calcBounds)
		{
			mesh.RecalculateBounds();
		}
	}
}
