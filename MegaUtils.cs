using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaUtils
{
	public static void Bez3D(out Vector3 b, ref Vector3[] p, float u)
	{
		Vector3 vector = p[0];
		vector.x += (p[1].x - p[0].x) * u;
		vector.y += (p[1].y - p[0].y) * u;
		vector.z += (p[1].z - p[0].z) * u;
		Vector3 vector2 = p[1];
		vector2.x += (p[2].x - p[1].x) * u;
		vector2.y += (p[2].y - p[1].y) * u;
		vector2.z += (p[2].z - p[1].z) * u;
		Vector3 vector3 = vector + (vector2 - vector) * u;
		vector.x = p[2].x + (p[3].x - p[2].x) * u;
		vector.y = p[2].y + (p[3].y - p[2].y) * u;
		vector.z = p[2].z + (p[3].z - p[2].z) * u;
		vector.x = vector2.x + (vector.x - vector2.x) * u;
		vector.y = vector2.y + (vector.y - vector2.y) * u;
		vector.z = vector2.z + (vector.z - vector2.z) * u;
		b.x = vector3.x + (vector.x - vector3.x) * u;
		b.y = vector3.y + (vector.y - vector3.y) * u;
		b.z = vector3.z + (vector.z - vector3.z) * u;
	}

	public static float WaveFunc(float radius, float t, float amp, float waveLen, float phase, float decay)
	{
		if (waveLen == 0f)
		{
			waveLen = 1E-07f;
		}
		float f = (float)Math.PI * 2f * (radius / waveLen + phase);
		return amp * Mathf.Sin(f) * Mathf.Exp((0f - decay) * Mathf.Abs(radius));
	}

	public static Mesh GetMesh(GameObject go)
	{
		if (!Application.isPlaying)
		{
			return GetSharedMesh(go);
		}
		MeshFilter meshFilter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
		if (meshFilter != null)
		{
			return meshFilter.mesh;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)go.GetComponent(typeof(SkinnedMeshRenderer));
		if (skinnedMeshRenderer != null)
		{
			return skinnedMeshRenderer.sharedMesh;
		}
		return null;
	}

	public static Mesh GetSharedMesh(GameObject go)
	{
		MeshFilter meshFilter = (MeshFilter)go.GetComponent(typeof(MeshFilter));
		if (meshFilter != null)
		{
			return meshFilter.sharedMesh;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)go.GetComponent(typeof(SkinnedMeshRenderer));
		if (skinnedMeshRenderer != null)
		{
			return skinnedMeshRenderer.sharedMesh;
		}
		return null;
	}

	public static int LargestComponent(Vector3 p)
	{
		if (p.x > p.y)
		{
			if (p.x > p.z)
			{
				return 0;
			}
			return 2;
		}
		if (!(p.y > p.z))
		{
			return 2;
		}
		return 1;
	}

	public static float LargestValue(Vector3 p)
	{
		if (p.x > p.y)
		{
			if (p.x > p.z)
			{
				return p.x;
			}
			return p.z;
		}
		if (p.y > p.z)
		{
			return p.y;
		}
		return p.z;
	}

	public static float LargestValue1(Vector3 p)
	{
		if (Mathf.Abs(p.x) > Mathf.Abs(p.y))
		{
			if (Mathf.Abs(p.x) > Mathf.Abs(p.z))
			{
				return p.x;
			}
			return p.z;
		}
		if (Mathf.Abs(p.y) > Mathf.Abs(p.z))
		{
			return p.y;
		}
		return p.z;
	}

	public static Vector3 Extents(Vector3[] verts, out Vector3 min, out Vector3 max)
	{
		Vector3 result = Vector3.zero;
		min = Vector3.zero;
		max = Vector3.zero;
		if (verts != null && verts.Length != 0)
		{
			min = verts[0];
			max = verts[0];
			for (int i = 1; i < verts.Length; i++)
			{
				if (verts[i].x < min.x)
				{
					min.x = verts[i].x;
				}
				if (verts[i].y < min.y)
				{
					min.y = verts[i].y;
				}
				if (verts[i].z < min.z)
				{
					min.z = verts[i].z;
				}
				if (verts[i].x > max.x)
				{
					max.x = verts[i].x;
				}
				if (verts[i].y > max.y)
				{
					max.y = verts[i].y;
				}
				if (verts[i].z > max.z)
				{
					max.z = verts[i].z;
				}
			}
			result = max - min;
		}
		return result;
	}

	public static Vector3 Extents(List<Vector3> verts, out Vector3 min, out Vector3 max)
	{
		Vector3 result = Vector3.zero;
		min = Vector3.zero;
		max = Vector3.zero;
		if (verts != null && verts.Count > 0)
		{
			min = verts[0];
			max = verts[0];
			for (int i = 1; i < verts.Count; i++)
			{
				if (verts[i].x < min.x)
				{
					min.x = verts[i].x;
				}
				if (verts[i].y < min.y)
				{
					min.y = verts[i].y;
				}
				if (verts[i].z < min.z)
				{
					min.z = verts[i].z;
				}
				if (verts[i].x > max.x)
				{
					max.x = verts[i].x;
				}
				if (verts[i].y > max.y)
				{
					max.y = verts[i].y;
				}
				if (verts[i].z > max.z)
				{
					max.z = verts[i].z;
				}
			}
			result = max - min;
		}
		return result;
	}

	public static int FindVert(Vector3 vert, List<Vector3> verts, float tolerance, float scl, bool flipyz, bool negx, int vn)
	{
		int result = 0;
		if (negx)
		{
			vert.x = 0f - vert.x;
		}
		if (flipyz)
		{
			float z = vert.z;
			vert.z = vert.y;
			vert.y = z;
		}
		vert /= scl;
		float num = Vector3.SqrMagnitude(verts[0] - vert);
		for (int i = 0; i < verts.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(verts[i] - vert);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		if (num > tolerance)
		{
			return -1;
		}
		return result;
	}

	public static void BuildTangents(Mesh mesh)
	{
		int num = mesh.triangles.Length;
		int num2 = mesh.vertices.Length;
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector4[] array3 = new Vector4[num2];
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		int[] triangles = mesh.triangles;
		for (int i = 0; i < num; i += 3)
		{
			long num3 = triangles[i];
			long num4 = triangles[i + 1];
			long num5 = triangles[i + 2];
			Vector3 vector = vertices[num3];
			Vector3 vector2 = vertices[num4];
			Vector3 vector3 = vertices[num5];
			Vector2 vector4 = uv[num3];
			Vector2 vector5 = uv[num4];
			Vector2 vector6 = uv[num5];
			float num6 = vector2.x - vector.x;
			float num7 = vector3.x - vector.x;
			float num8 = vector2.y - vector.y;
			float num9 = vector3.y - vector.y;
			float num10 = vector2.z - vector.z;
			float num11 = vector3.z - vector.z;
			float num12 = vector5.x - vector4.x;
			float num13 = vector6.x - vector4.x;
			float num14 = vector5.y - vector4.y;
			float num15 = vector6.y - vector4.y;
			float num16 = 1f / (num12 * num15 - num13 * num14);
			Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num16, (num15 * num8 - num14 * num9) * num16, (num15 * num10 - num14 * num11) * num16);
			Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num16, (num12 * num9 - num13 * num8) * num16, (num12 * num11 - num13 * num10) * num16);
			array[num3] += vector7;
			array[num4] += vector7;
			array[num5] += vector7;
			array2[num3] += vector8;
			array2[num4] += vector8;
			array2[num5] += vector8;
		}
		for (int j = 0; j < num2; j++)
		{
			Vector3 normal = normals[j];
			Vector3 tangent = array[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array3[j].x = tangent.x;
			array3[j].y = tangent.y;
			array3[j].z = tangent.z;
			array3[j].w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), array2[j]) < 0f)) ? 1f : (-1f));
		}
		mesh.tangents = array3;
	}
}
