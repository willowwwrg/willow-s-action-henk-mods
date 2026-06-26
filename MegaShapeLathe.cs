using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("MegaShapes/Lathe")]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class MegaShapeLathe : MonoBehaviour
{
	public MegaShape shape;

	public int curve;

	public float degrees = 360f;

	public float startang;

	public int segments = 8;

	public MegaAxis direction;

	public Vector3 axis;

	public bool genuvs = true;

	public int steps = 8;

	public bool update = true;

	public bool flip;

	public bool doublesided;

	public bool buildTangents;

	public float twist;

	public AnimationCurve twistcrv = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool useprofilecurve;

	public float profileamt = 1f;

	public AnimationCurve profilecrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float globalscale = 1f;

	public Vector3 scale = Vector3.one;

	public bool usescalecrv;

	public AnimationCurve scalexcrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve scaleycrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve scalezcrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve scaleamthgt = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector2 uvoffset = Vector2.zero;

	public Vector2 uvscale = Vector2.one;

	public float uvrotate;

	public bool pivotbase;

	public Mesh mesh;

	public Vector3 limits = Vector3.zero;

	public Vector3 min = Vector3.zero;

	public Vector3 max = Vector3.zero;

	public MegaPolyShape pshape;

	public Vector3 pivot = Vector3.zero;

	private List<Vector3> verts = new List<Vector3>();

	private List<Vector2> uvs = new List<Vector2>();

	private List<int> tris = new List<int>();

	private List<int> dtris = new List<int>();

	private List<Vector3> normals = new List<Vector3>();

	private void LateUpdate()
	{
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.name = "Lathe";
			GetComponent<MeshFilter>().sharedMesh = mesh;
		}
		if (update)
		{
			BuildMesh(mesh);
		}
	}

	private MegaPolyShape MakePolyShape(MegaShape shape, int steps)
	{
		if (pshape == null)
		{
			pshape = new MegaPolyShape();
		}
		pshape.length.Clear();
		pshape.points.Clear();
		int num = 0;
		float num2 = 0f;
		Vector3 b = shape.splines[curve].knots[0].p + axis;
		Vector3 zero = Vector3.zero;
		min = b;
		max = b;
		pivot = Vector3.zero;
		bool closed = shape.splines[curve].closed;
		int num3 = shape.splines[curve].knots.Count - 1;
		if (closed)
		{
			num3++;
		}
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < num3; i++)
		{
			num4 = i;
			num5 = i + 1;
			if (num5 >= shape.splines[curve].knots.Count)
			{
				num5 = 0;
			}
			for (int j = num; j < steps; j++)
			{
				float t = (float)j / (float)steps;
				zero = shape.splines[curve].knots[num4].InterpolateCS(t, shape.splines[curve].knots[num5]);
				zero += axis;
				pshape.points.Add(zero);
				num2 += Vector3.Distance(zero, b);
				pshape.length.Add(num2);
				if (zero.x < min.x)
				{
					min.x = zero.x;
				}
				if (zero.y < min.y)
				{
					min.y = zero.y;
				}
				if (zero.z < min.z)
				{
					min.z = zero.z;
				}
				if (zero.x > max.x)
				{
					max.x = zero.x;
				}
				if (zero.y > max.y)
				{
					max.y = zero.y;
				}
				if (zero.z > max.z)
				{
					max.z = zero.z;
				}
				b = zero;
			}
			num = 0;
		}
		zero = shape.splines[curve].knots[num5].p;
		zero += axis;
		pshape.points.Add(zero);
		num2 += Vector3.Distance(zero, b);
		pshape.length.Add(num2);
		limits = max - min;
		if (pivotbase)
		{
			pivot.z = max.z;
			max.z -= min.z;
			min.z = 0f;
		}
		if (useprofilecurve)
		{
			float num6 = 0f;
			for (int k = 0; k < pshape.points.Count; k++)
			{
				Vector3 value = pshape.points[k];
				value.z -= pivot.z;
				num6 = (pivotbase ? (0f - (value.z - min.z) / limits.z) : (1f - (value.z - min.z) / limits.z));
				value.x += profilecrv.Evaluate(num6) * profileamt;
				if (value.x > max.x)
				{
					max.x = value.x;
				}
				if (value.x < min.x)
				{
					min.x = value.x;
				}
				pshape.points[k] = value;
			}
		}
		else
		{
			for (int l = 0; l < pshape.points.Count; l++)
			{
				Vector3 value2 = pshape.points[l];
				value2.z -= pivot.z;
				pshape.points[l] = value2;
			}
		}
		if (base.collider != null && base.collider is BoxCollider)
		{
			BoxCollider obj = (BoxCollider)base.collider;
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			if (pivotbase)
			{
				zero3.z = (0f - (max.z - min.z)) * 0.5f;
			}
			obj.center = zero3;
			zero2.x = Mathf.Abs(min.x);
			zero2.y = zero2.x;
			zero2.z = (max.z - min.z) * 0.5f;
			obj.size = zero2;
		}
		return pshape;
	}

	public void BuildMesh(Mesh mesh)
	{
		if (shape == null)
		{
			return;
		}
		verts.Clear();
		uvs.Clear();
		tris.Clear();
		dtris.Clear();
		normals.Clear();
		pshape = MakePolyShape(shape, steps);
		Matrix4x4 identity = Matrix4x4.identity;
		Quaternion q = Quaternion.identity;
		switch (direction)
		{
		case MegaAxis.X:
			q = Quaternion.Euler(90f, 0f, 0f);
			break;
		case MegaAxis.Y:
			q = Quaternion.Euler(0f, 90f, 0f);
			break;
		case MegaAxis.Z:
			q = Quaternion.Euler(0f, 0f, 90f);
			break;
		}
		int num = segments + 1;
		identity.SetTRS(axis, q, Vector3.one);
		Matrix4x4 inverse = identity.inverse;
		Matrix4x4 identity2 = Matrix4x4.identity;
		float num2 = pshape.length[pshape.length.Count - 1];
		Vector2 zero = Vector2.zero;
		Vector2 vector = zero;
		Matrix4x4 identity3 = Matrix4x4.identity;
		identity3.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, uvrotate), Vector3.one);
		Vector3 zero2 = Vector3.zero;
		Vector3 vector2 = scale;
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		int num3 = 0;
		float num4 = 0f;
		if (!pivotbase)
		{
			num4 = 1f;
		}
		for (int i = 0; i < num; i++)
		{
			zero.y = (float)i / (float)segments;
			float num5 = zero.y * degrees + startang;
			identity2 = Matrix4x4.identity;
			MegaMatrix.RotateZ(ref identity2, num5 * ((float)Math.PI / 180f));
			Matrix4x4 matrix4x = inverse * identity2 * identity;
			Vector3 vector3 = Vector3.zero;
			float num6 = 0f;
			int count = normals.Count;
			for (int j = 0; j < pshape.points.Count; j++)
			{
				zero4 = pshape.points[j];
				float num7 = num4 - (zero4.z - min.z) / limits.z;
				if (usescalecrv)
				{
					float num8 = scaleamthgt.Evaluate(num7) * globalscale;
					vector2.x = 1f + scalexcrv.Evaluate(zero.y) * num8;
					vector2.y = 1f + scaleycrv.Evaluate(zero.y) * num8;
					vector2.z = 1f + scalezcrv.Evaluate(zero.y) * num8;
				}
				zero4.x *= vector2.x;
				zero4.y *= vector2.y;
				zero4.z *= vector2.z;
				if (twist != 0f)
				{
					float f = (twist * num7 * twistcrv.Evaluate(num7) + num5) * ((float)Math.PI / 180f);
					float value = Mathf.Cos(f);
					float num9 = Mathf.Sin(f);
					identity2[0, 0] = value;
					identity2[0, 1] = num9;
					identity2[1, 0] = 0f - num9;
					identity2[1, 1] = value;
					matrix4x = inverse * identity2 * identity;
				}
				zero3.x = 0f - (zero4.z - vector3.z);
				zero3.y = 0f;
				zero3.z = zero4.x - vector3.x;
				vector3 = zero4;
				zero2 = matrix4x * zero4;
				verts.Add(zero2);
				if (j != 0)
				{
					if (j == 1)
					{
						if (flip)
						{
							normals.Add(matrix4x.MultiplyVector(zero3).normalized);
						}
						else
						{
							normals.Add(-matrix4x.MultiplyVector(zero3).normalized);
						}
					}
					if (flip)
					{
						normals.Add(matrix4x.MultiplyVector(zero3).normalized);
					}
					else
					{
						normals.Add(-matrix4x.MultiplyVector(zero3).normalized);
					}
				}
				num6 = pshape.length[j];
				zero.x = num6 / num2;
				vector = zero;
				vector.x *= uvscale.x;
				vector.y *= uvscale.y;
				vector += uvoffset;
				vector = identity3.MultiplyPoint(vector);
				uvs.Add(vector);
			}
			if (shape.splines[curve].closed)
			{
				normals[normals.Count - 1] = normals[count];
			}
		}
		int count2 = pshape.points.Count;
		for (int k = 0; k < num - 1; k++)
		{
			int num10 = k * count2;
			for (int l = 0; l < pshape.points.Count - 1; l++)
			{
				int num11 = l + num10;
				int item = num11 + 1;
				int num12 = ((k != num - 1) ? (num11 + count2) : l);
				int item2 = num12 + 1;
				if (flip)
				{
					tris.Add(num11);
					tris.Add(item2);
					tris.Add(item);
					tris.Add(num11);
					tris.Add(num12);
					tris.Add(item2);
				}
				else
				{
					tris.Add(item);
					tris.Add(item2);
					tris.Add(num11);
					tris.Add(item2);
					tris.Add(num12);
					tris.Add(num11);
				}
			}
		}
		if (doublesided)
		{
			int count3 = verts.Count;
			for (int m = 0; m < count3; m++)
			{
				verts.Add(verts[m]);
				uvs.Add(uvs[m]);
				normals.Add(-normals[m]);
				num3++;
			}
			for (int n = 0; n < tris.Count; n += 3)
			{
				int num13 = tris[n];
				int num14 = tris[n + 1];
				int num15 = tris[n + 2];
				dtris.Add(num15 + count3);
				dtris.Add(num14 + count3);
				dtris.Add(num13 + count3);
			}
		}
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		if (doublesided)
		{
			mesh.subMeshCount = 2;
		}
		else
		{
			mesh.subMeshCount = 1;
		}
		mesh.SetTriangles(tris.ToArray(), 0);
		if (doublesided)
		{
			mesh.SetTriangles(dtris.ToArray(), 1);
		}
		mesh.RecalculateBounds();
		mesh.normals = normals.ToArray();
		if (buildTangents)
		{
			MegaUtils.BuildTangents(mesh);
		}
	}
}
