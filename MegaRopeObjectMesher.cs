using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaRopeObjectMesher : MegaRopeMesher
{
	public GameObject source;

	public MegaAxis meshaxis;

	public Vector3 rot = Vector3.zero;

	public Vector3 offset = Vector3.zero;

	public Vector3 scale = Vector3.one;

	public bool stretchtofit = true;

	public Vector3[] sverts;

	public Vector3[] overts;

	public Bounds bounds;

	private Matrix4x4 wtm = Matrix4x4.identity;

	private Matrix4x4 tm = Matrix4x4.identity;

	private Vector3 T = Vector3.zero;

	private Vector3 N = Vector3.zero;

	private Vector3 B = Vector3.zero;

	private Matrix4x4[] frames;

	public int numframes = 10;

	public MegaRopeNormMap[] mapping;

	public int[] otris;

	public Vector3[] facenorms;

	public Vector3[] norms;

	public void SetSource()
	{
	}

	[ContextMenu("Rebuild")]
	public void Rebuild(MegaRope rope)
	{
		if (!source)
		{
			return;
		}
		Mesh sharedMesh = MegaUtils.GetSharedMesh(source);
		if ((bool)sharedMesh)
		{
			bounds = sharedMesh.bounds;
			sverts = sharedMesh.vertices;
			overts = new Vector3[sharedMesh.vertexCount];
			rope.mesh.Clear();
			rope.mesh.vertices = sharedMesh.vertices;
			rope.mesh.normals = sharedMesh.normals;
			rope.mesh.uv = sharedMesh.uv;
			rope.mesh.uv1 = sharedMesh.uv1;
			rope.mesh.subMeshCount = sharedMesh.subMeshCount;
			for (int i = 0; i < sharedMesh.subMeshCount; i++)
			{
				rope.mesh.SetTriangles(sharedMesh.GetTriangles(i), i);
			}
			MeshRenderer component = source.GetComponent<MeshRenderer>();
			MeshRenderer component2 = rope.GetComponent<MeshRenderer>();
			if (component2 != null && component != null)
			{
				component2.sharedMaterials = component.sharedMaterials;
			}
			BuildNormalMapping(sharedMesh, force: true);
			BuildMesh(rope);
		}
	}

	private Vector3 Deform(Vector3 p, float off, MegaRope rope, float alpha)
	{
		Vector3 p2 = rope.Interp(alpha);
		T = rope.Velocity(alpha).normalized;
		wtm = rope.CalcFrame(T, ref N, ref B);
		MegaMatrix.SetTrans(ref wtm, p2);
		return p;
	}

	public override void BuildMesh(MegaRope rope)
	{
		if (!source)
		{
			return;
		}
		if (overts == null)
		{
			Rebuild(rope);
		}
		if (frames == null || frames.Length != numframes + 1)
		{
			frames = new Matrix4x4[numframes + 1];
		}
		wtm = rope.GetDeformMat(0f);
		T = wtm.MultiplyPoint3x4(rope.Velocity(0f).normalized);
		Vector3 vector = wtm.MultiplyPoint3x4(tm.MultiplyPoint3x4(Vector3.right));
		N = (vector - wtm.MultiplyPoint3x4(rope.Interp(0f))).normalized;
		B = Vector3.Cross(T, N);
		ref Matrix4x4 reference = ref frames[0];
		reference = wtm;
		for (int i = 0; i <= numframes; i++)
		{
			float t = (float)i / (float)numframes;
			if (i == 0)
			{
				t = 0.001f;
			}
			T = rope.Velocity(t).normalized;
			ref Matrix4x4 reference2 = ref frames[i];
			reference2 = rope.CalcFrame(T, ref N, ref B);
		}
		int index = (int)meshaxis;
		Vector3 vector2 = scale;
		tm = Matrix4x4.identity;
		float num = bounds.min[index];
		float num2 = bounds.size[index];
		_ = bounds.min[(int)meshaxis];
		_ = vector2[index];
		if (!stretchtofit)
		{
			num2 = rope.RopeLength;
		}
		for (int j = 0; j < sverts.Length; j++)
		{
			Vector3 v = sverts[j];
			float num3 = Mathf.Clamp01((v[index] - num) / num2);
			MegaMatrix.SetTrans(ref frames[(int)(num3 * (float)numframes)], rope.Interp(num3));
			v[index] = 0f;
			v.x *= scale.x;
			v.y *= scale.y;
			v.z *= scale.z;
			ref Vector3 reference3 = ref overts[j];
			reference3 = frames[(int)(num3 * (float)numframes)].MultiplyPoint3x4(v);
		}
		rope.mesh.vertices = overts;
		rope.mesh.RecalculateBounds();
		RecalcNormals(rope.mesh, overts);
	}

	private int[] FindFacesUsing(Vector3 p, Vector3 n)
	{
		List<int> list = new List<int>();
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < otris.Length; i += 3)
		{
			zero = overts[otris[i]];
			if (zero.x == p.x && zero.y == p.y && zero.z == p.z)
			{
				if (n.Equals(norms[otris[i]]))
				{
					list.Add(i / 3);
				}
				continue;
			}
			zero = overts[otris[i + 1]];
			if (zero.x == p.x && zero.y == p.y && zero.z == p.z)
			{
				if (n.Equals(norms[otris[i + 1]]))
				{
					list.Add(i / 3);
				}
				continue;
			}
			zero = overts[otris[i + 2]];
			if (zero.x == p.x && zero.y == p.y && zero.z == p.z && n.Equals(norms[otris[i + 2]]))
			{
				list.Add(i / 3);
			}
		}
		return list.ToArray();
	}

	public void BuildNormalMapping(Mesh mesh, bool force)
	{
		if (mapping == null || mapping.Length == 0 || force)
		{
			Debug.Log("Build norm data");
			otris = mesh.triangles;
			norms = mesh.normals;
			facenorms = new Vector3[otris.Length / 3];
			mapping = new MegaRopeNormMap[overts.Length];
			for (int i = 0; i < overts.Length; i++)
			{
				mapping[i] = new MegaRopeNormMap();
				mapping[i].faces = FindFacesUsing(overts[i], norms[i]);
			}
		}
	}

	public void RecalcNormals(Mesh ms, Vector3[] _verts)
	{
		if (mapping == null)
		{
			return;
		}
		int num = 0;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		Vector3 zero5 = Vector3.zero;
		for (int i = 0; i < otris.Length; i += 3)
		{
			zero = _verts[otris[i]];
			zero2 = _verts[otris[i + 1]];
			zero3 = _verts[otris[i + 2]];
			zero4.x = zero2.x - zero.x;
			zero4.y = zero2.y - zero.y;
			zero4.z = zero2.z - zero.z;
			zero5.x = zero3.x - zero2.x;
			zero5.y = zero3.y - zero2.y;
			zero5.z = zero3.z - zero2.z;
			zero.x = zero4.y * zero5.z - zero4.z * zero5.y;
			zero.y = zero4.z * zero5.x - zero4.x * zero5.z;
			zero.z = zero4.x * zero5.y - zero4.y * zero5.x;
			facenorms[num++] = zero;
		}
		for (int j = 0; j < norms.Length; j++)
		{
			if (mapping[j].faces.Length != 0)
			{
				Vector3 vector = facenorms[mapping[j].faces[0]];
				for (int k = 1; k < mapping[j].faces.Length; k++)
				{
					zero = facenorms[mapping[j].faces[k]];
					vector.x += zero.x;
					vector.y += zero.y;
					vector.z += zero.z;
				}
				float f = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
				f = 1f / Mathf.Sqrt(f);
				vector.x *= f;
				vector.y *= f;
				vector.z *= f;
				norms[j] = vector;
			}
			else
			{
				ref Vector3 reference = ref norms[j];
				reference = Vector3.up;
			}
		}
		ms.normals = norms;
	}
}
