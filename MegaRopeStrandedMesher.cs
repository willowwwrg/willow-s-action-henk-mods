using System;
using UnityEngine;

[Serializable]
public class MegaRopeStrandedMesher : MegaRopeMesher
{
	private Matrix4x4 tm;

	private Matrix4x4 wtm;

	public int sides = 8;

	public int segments = 20;

	public float uvtwist;

	public float uvtilex = 1f;

	public float uvtiley = 1f;

	public int strands = 1;

	public float offset;

	public float Twist;

	public bool cap = true;

	public float strandRadius;

	public float SegsPerUnit = 4f;

	public float TwistPerUnit;

	private Vector3 ropeup;

	private Vector3[] cross;

	public override void BuildMesh(MegaRope rope)
	{
		float num = uvtiley * rope.RopeLength;
		Twist = TwistPerUnit * rope.RopeLength;
		segments = (int)(rope.RopeLength * SegsPerUnit);
		float num2 = rope.radius * 0.5f + offset;
		float num3 = 0f;
		if (strands == 1)
		{
			num2 = offset;
			num3 = rope.radius;
		}
		else
		{
			num3 = rope.radius * 0.5f + strandRadius;
		}
		BuildCrossSection(num3);
		int num4 = (segments + 1) * (sides + 1) * strands;
		int num5 = sides * 2 * segments * strands;
		if (cap)
		{
			num4 += (sides + 1) * 2 * strands;
			num5 += sides * 2 * strands;
		}
		if (verts == null || verts.Length != num4)
		{
			verts = new Vector3[num4];
		}
		bool flag = false;
		if (uvs == null || uvs.Length != num4)
		{
			uvs = new Vector2[num4];
			tris = new int[num5 * 3];
			flag = true;
		}
		tm = Matrix4x4.identity;
		switch (rope.axis)
		{
		case MegaAxis.X:
			MegaMatrix.RotateY(ref tm, -(float)Math.PI / 2f);
			break;
		case MegaAxis.Y:
			MegaMatrix.RotateX(ref tm, -(float)Math.PI / 2f);
			break;
		}
		int num6 = 0;
		int num7 = 0;
		Vector2 zero = Vector2.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		Vector3 N = Vector3.zero;
		Vector3 B = Vector3.zero;
		for (int i = 0; i < strands; i++)
		{
			float num8 = (float)i / (float)strands * (float)Math.PI * 2f;
			zero2.x = Mathf.Sin(num8) * num2;
			zero2.z = Mathf.Cos(num8) * num2;
			int num9 = num6;
			if (cap)
			{
				float num10 = 0f;
				wtm = rope.GetDeformMat(num10);
				float num11 = num10 * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num8 + num11) * num2;
				zero2.z = Mathf.Cos(num8 + num11) * num2;
				for (int j = 0; j <= cross.Length; j++)
				{
					Vector3 v = tm.MultiplyPoint3x4(cross[j % cross.Length] + zero2);
					ref Vector3 reference = ref verts[num6];
					reference = wtm.MultiplyPoint3x4(v);
					if (flag)
					{
						zero.y = 0f;
						zero.x = 0f;
						uvs[num6++] = zero;
					}
					else
					{
						num6++;
					}
				}
				if (flag)
				{
					for (int k = 1; k < sides; k++)
					{
						tris[num7++] = num9;
						tris[num7++] = num9 + k + 1;
						tris[num7++] = num9 + k;
					}
				}
				num9 = num6;
				num10 = 1f;
				wtm = rope.GetDeformMat(num10);
				num11 = num10 * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num8 + num11) * num2;
				zero2.z = Mathf.Cos(num8 + num11) * num2;
				for (int l = 0; l <= cross.Length; l++)
				{
					Vector3 v2 = tm.MultiplyPoint3x4(cross[l % cross.Length] + zero2);
					ref Vector3 reference2 = ref verts[num6];
					reference2 = wtm.MultiplyPoint3x4(v2);
					if (flag)
					{
						zero.y = 0f;
						zero.x = 0f;
						uvs[num6++] = zero;
					}
					else
					{
						num6++;
					}
				}
				if (flag)
				{
					for (int m = 1; m < sides; m++)
					{
						tris[num7++] = num9;
						tris[num7++] = num9 + m;
						tris[num7++] = num9 + m + 1;
					}
				}
			}
			num9 = num6;
			for (int n = 0; n <= segments; n++)
			{
				float num12 = (float)n / (float)segments;
				float num13 = num12 * uvtwist;
				float num14 = num12 * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num8 + num14) * num2;
				zero2.z = Mathf.Cos(num8 + num14) * num2;
				if (n == 0)
				{
					wtm = rope.GetDeformMat(num12);
					zero3 = wtm.MultiplyPoint3x4(rope.Velocity(0f).normalized);
					N = (wtm.MultiplyPoint3x4(tm.MultiplyPoint3x4(cross[0])) - wtm.MultiplyPoint3x4(rope.Interp(0f))).normalized;
					B = Vector3.Cross(zero3, N);
				}
				else
				{
					Vector3 p = rope.Interp(num12);
					zero3 = rope.Velocity(num12).normalized;
					wtm = rope.CalcFrame(zero3, ref N, ref B);
					MegaMatrix.SetTrans(ref wtm, p);
				}
				for (int num15 = 0; num15 <= cross.Length; num15++)
				{
					Vector3 v3 = tm.MultiplyPoint3x4(cross[num15 % cross.Length] + zero2);
					ref Vector3 reference3 = ref verts[num6];
					reference3 = wtm.MultiplyPoint3x4(v3);
					zero.y = num12 * num;
					zero.x = (float)num15 / (float)cross.Length * uvtilex + num13;
					uvs[num6++] = zero;
				}
			}
			if (!flag)
			{
				continue;
			}
			int num16 = sides + 1;
			for (int num17 = 0; num17 < segments; num17++)
			{
				for (int num18 = 0; num18 < cross.Length; num18++)
				{
					tris[num7++] = num17 * num16 + num18 + num9;
					tris[num7++] = (num17 + 1) * num16 + (num18 + 1) % num16 + num9;
					tris[num7++] = (num17 + 1) * num16 + num18 + num9;
					tris[num7++] = num17 * num16 + num18 + num9;
					tris[num7++] = num17 * num16 + (num18 + 1) % num16 + num9;
					tris[num7++] = (num17 + 1) * num16 + (num18 + 1) % num16 + num9;
				}
			}
		}
		if (flag)
		{
			rope.mesh.Clear();
			rope.mesh.vertices = verts;
			rope.mesh.uv = uvs;
			rope.mesh.triangles = tris;
		}
		else
		{
			rope.mesh.vertices = verts;
			rope.mesh.uv = uvs;
		}
		rope.mesh.RecalculateBounds();
		rope.mesh.RecalculateNormals();
	}

	private void BuildCrossSection(float rad)
	{
		if (cross == null || cross.Length != sides)
		{
			cross = new Vector3[sides];
		}
		for (int i = 0; i < sides; i++)
		{
			float f = (float)i / (float)sides * (float)Math.PI * 2f;
			ref Vector3 reference = ref cross[i];
			reference = new Vector3(Mathf.Sin(f) * rad, 0f, Mathf.Cos(f) * rad);
		}
	}
}
