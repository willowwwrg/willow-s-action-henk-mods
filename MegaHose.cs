using System;
using UnityEngine;

[AddComponentMenu("MegaShapes/Hose")]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class MegaHose : MonoBehaviour
{
	private const float HalfIntMax = 16383.5f;

	private const float PIover2 = (float)Math.PI / 2f;

	private const float EPSILON = 0.0001f;

	public bool freecreate = true;

	public bool updatemesh = true;

	private Matrix4x4 S = Matrix4x4.identity;

	public MegaSpline hosespline = new MegaSpline();

	private Mesh mesh;

	public Vector3[] verts = new Vector3[1];

	public Vector2[] uvs = new Vector2[1];

	public int[] faces = new int[1];

	public Vector3[] normals;

	public bool optimize;

	public bool calctangents;

	public bool recalcCollider;

	public bool calcnormals;

	public bool capends = true;

	public GameObject custnode2;

	public GameObject custnode;

	public Vector3 offset = Vector3.zero;

	public Vector3 offset1 = Vector3.zero;

	public Vector3 rotate = Vector3.zero;

	public Vector3 rotate1 = Vector3.zero;

	public Vector3 scale = Vector3.one;

	public Vector3 scale1 = Vector3.one;

	public int endsmethod;

	public float noreflength = 1f;

	public int segments = 45;

	public MegaHoseSmooth smooth;

	public MegaHoseType wiretype;

	public float rnddia = 0.2f;

	public int rndsides = 8;

	public float rndrot;

	public float rectwidth = 0.2f;

	public float rectdepth = 0.2f;

	public float rectfillet;

	public int rectfilletsides;

	public float rectrotangle;

	public float dsecwidth = 0.2f;

	public float dsecdepth = 0.2f;

	public float dsecfillet;

	public int dsecfilletsides;

	public int dsecrndsides = 4;

	public float dsecrotangle;

	public bool mapmemapme = true;

	public bool flexon;

	public float flexstart = 0.1f;

	public float flexstop = 0.9f;

	public int flexcycles = 5;

	public float flexdiameter = -0.2f;

	public float tension1 = 10f;

	public float tension2 = 10f;

	public bool usebulgecurve;

	public AnimationCurve bulge = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float bulgeamount = 1f;

	public float bulgeoffset;

	public Vector2 uvscale = Vector2.one;

	public bool animatebulge;

	public float bulgespeed;

	public float minbulge = -1f;

	public float maxbulge = 2f;

	public bool displayspline = true;

	private bool visible = true;

	public bool InvisibleUpdate;

	public bool dolateupdate;

	private Vector3 endp1 = Vector3.zero;

	private Vector3 endp2 = Vector3.zero;

	private Vector3 endr1 = Vector3.zero;

	private Vector3 endr2 = Vector3.zero;

	public Vector3[] SaveVertex;

	public Vector2[] SaveUV;

	public Vector3[] SaveNormals;

	public bool rebuildcross = true;

	public int NvertsPerRing;

	public int Nverts;

	public Vector3 up = Vector3.up;

	private Vector3 starty = Vector3.zero;

	private Vector3 roty = Vector3.zero;

	private float yangle;

	public Matrix4x4 Tlocal = Matrix4x4.identity;

	private MeshCollider meshCol;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=3436");
	}

	private void Awake()
	{
		updatemesh = true;
		rebuildcross = true;
		Rebuild();
	}

	private void Reset()
	{
		Rebuild();
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}

	public void SetEndTarget(int end, GameObject target)
	{
		if (end == 0)
		{
			custnode = target;
		}
		else
		{
			custnode2 = target;
		}
		updatemesh = true;
	}

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
				BuildMesh();
			}
		}
	}

	private void MakeSaveVertex(int NvertsPerRing, int nfillets, int nsides, MegaHoseType wtype)
	{
		switch (wtype)
		{
		case MegaHoseType.Round:
		{
			float num31 = (float)Math.PI * 2f / (float)(NvertsPerRing - 1);
			float num32 = rnddia;
			num32 *= 0.5f;
			for (int m = 0; m < NvertsPerRing; m++)
			{
				float f5 = (float)(m + 1) * num31;
				ref Vector3 reference42 = ref SaveVertex[m];
				reference42 = new Vector3(num32 * Mathf.Cos(f5), num32 * Mathf.Sin(f5), 0f);
			}
			break;
		}
		case MegaHoseType.Rectangle:
		{
			int num17 = 0;
			int num18 = 1 + nfillets;
			int num19 = 2 * num18;
			int num20 = num18 + num19;
			float num21 = rectwidth * 0.5f;
			float num22 = rectdepth * 0.5f;
			float num23 = rectfillet;
			if (num23 < 0f)
			{
				num23 = 0f;
			}
			if (nfillets > 0)
			{
				float num24 = num21 - num23;
				float num25 = num22 - num23;
				float num26 = 0f - num24;
				float num27 = 0f - num25;
				ref Vector3 reference16 = ref SaveVertex[0];
				reference16 = new Vector3(num21, num25, 0f);
				ref Vector3 reference17 = ref SaveVertex[nfillets];
				reference17 = new Vector3(num24, num22, 0f);
				ref Vector3 reference18 = ref SaveVertex[num18];
				reference18 = new Vector3(num26, num22, 0f);
				ref Vector3 reference19 = ref SaveVertex[num18 + nfillets];
				reference19 = new Vector3(0f - num21, num25, 0f);
				ref Vector3 reference20 = ref SaveVertex[num19];
				reference20 = new Vector3(0f - num21, num27, 0f);
				ref Vector3 reference21 = ref SaveVertex[num19 + nfillets];
				reference21 = new Vector3(num26, 0f - num22, 0f);
				ref Vector3 reference22 = ref SaveVertex[num20];
				reference22 = new Vector3(num24, 0f - num22, 0f);
				ref Vector3 reference23 = ref SaveVertex[num20 + nfillets];
				reference23 = new Vector3(num21, num27, 0f);
				if (nfillets > 1)
				{
					float num28 = (float)Math.PI / 2f / (float)nfillets;
					num17 = 1;
					for (int l = 0; l < nfillets - 1; l++)
					{
						float f4 = (float)(l + 1) * num28;
						float num29 = num23 * Mathf.Cos(f4);
						float num30 = num23 * Mathf.Sin(f4);
						ref Vector3 reference24 = ref SaveVertex[num17];
						reference24 = new Vector3(num24 + num29, num25 + num30, 0f);
						ref Vector3 reference25 = ref SaveVertex[num17 + num18];
						reference25 = new Vector3(num26 - num30, num25 + num29, 0f);
						ref Vector3 reference26 = ref SaveVertex[num17 + num19];
						reference26 = new Vector3(num26 - num29, num27 - num30, 0f);
						ref Vector3 reference27 = ref SaveVertex[num17 + num20];
						reference27 = new Vector3(num24 + num30, num27 - num29, 0f);
						num17++;
					}
				}
				ref Vector3 reference28 = ref SaveVertex[SaveVertex.Length - 1];
				reference28 = SaveVertex[0];
			}
			else if (smooth == MegaHoseSmooth.SMOOTHNONE)
			{
				ref Vector3 reference29 = ref SaveVertex[num17++];
				reference29 = new Vector3(num21, num22, 0f);
				ref Vector3 reference30 = ref SaveVertex[num17++];
				reference30 = new Vector3(0f - num21, num22, 0f);
				ref Vector3 reference31 = ref SaveVertex[num17++];
				reference31 = new Vector3(0f - num21, num22, 0f);
				ref Vector3 reference32 = ref SaveVertex[num17++];
				reference32 = new Vector3(0f - num21, 0f - num22, 0f);
				ref Vector3 reference33 = ref SaveVertex[num17++];
				reference33 = new Vector3(0f - num21, 0f - num22, 0f);
				ref Vector3 reference34 = ref SaveVertex[num17++];
				reference34 = new Vector3(num21, 0f - num22, 0f);
				ref Vector3 reference35 = ref SaveVertex[num17++];
				reference35 = new Vector3(num21, 0f - num22, 0f);
				ref Vector3 reference36 = ref SaveVertex[num17++];
				reference36 = new Vector3(num21, num22, 0f);
			}
			else
			{
				ref Vector3 reference37 = ref SaveVertex[num17++];
				reference37 = new Vector3(num21, num22, 0f);
				ref Vector3 reference38 = ref SaveVertex[num17++];
				reference38 = new Vector3(0f - num21, num22, 0f);
				ref Vector3 reference39 = ref SaveVertex[num17++];
				reference39 = new Vector3(0f - num21, 0f - num22, 0f);
				ref Vector3 reference40 = ref SaveVertex[num17++];
				reference40 = new Vector3(num21, 0f - num22, 0f);
				ref Vector3 reference41 = ref SaveVertex[num17++];
				reference41 = new Vector3(num21, num22, 0f);
			}
			break;
		}
		default:
		{
			int num = 0;
			float num2 = (float)Math.PI / (float)nsides;
			float num3 = dsecwidth * 0.5f;
			float num4 = dsecdepth * 0.5f;
			float num5 = dsecfillet;
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			float num6 = num4 - num3;
			if (nfillets > 0)
			{
				float num7 = num4 - num5;
				float num8 = 0f - num7;
				float num9 = num3 - num5;
				int num10 = 1 + nfillets;
				int num11 = num10 + 1 + nsides;
				ref Vector3 reference = ref SaveVertex[0];
				reference = new Vector3(num3, num7, 0f);
				ref Vector3 reference2 = ref SaveVertex[nfillets];
				reference2 = new Vector3(num9, num4, 0f);
				ref Vector3 reference3 = ref SaveVertex[num10];
				reference3 = new Vector3(num6, num4, 0f);
				ref Vector3 reference4 = ref SaveVertex[num10 + nsides];
				reference4 = new Vector3(num6, 0f - num4, 0f);
				ref Vector3 reference5 = ref SaveVertex[num11];
				reference5 = new Vector3(num9, 0f - num4, 0f);
				ref Vector3 reference6 = ref SaveVertex[num11 + nfillets];
				reference6 = new Vector3(num3, num8, 0f);
				if (nfillets > 1)
				{
					float num12 = (float)Math.PI / 2f / (float)nfillets;
					num = 1;
					for (int i = 0; i < nfillets - 1; i++)
					{
						float f = (float)(i + 1) * num12;
						float num13 = num5 * Mathf.Cos(f);
						float num14 = num5 * Mathf.Sin(f);
						ref Vector3 reference7 = ref SaveVertex[num];
						reference7 = new Vector3(num9 + num13, num7 + num14, 0f);
						ref Vector3 reference8 = ref SaveVertex[num + num11];
						reference8 = new Vector3(num9 + num14, num8 - num13, 0f);
						num++;
					}
				}
				num = 1 + num10;
				for (int j = 0; j < nsides - 1; j++)
				{
					float f2 = (float)(j + 1) * num2;
					float y = num4 * Mathf.Cos(f2);
					float num15 = num4 * Mathf.Sin(f2);
					ref Vector3 reference9 = ref SaveVertex[num];
					reference9 = new Vector3(num6 - num15, y, 0f);
					num++;
				}
			}
			else
			{
				ref Vector3 reference10 = ref SaveVertex[num];
				reference10 = new Vector3(num3, num4, 0f);
				num++;
				ref Vector3 reference11 = ref SaveVertex[num];
				reference11 = new Vector3(num6, num4, 0f);
				num++;
				for (int k = 0; k < nsides - 1; k++)
				{
					float f3 = (float)(k + 1) * num2;
					float y2 = num4 * Mathf.Cos(f3);
					float num16 = num4 * Mathf.Sin(f3);
					ref Vector3 reference12 = ref SaveVertex[num];
					reference12 = new Vector3(num6 - num16, y2, 0f);
					num++;
				}
				ref Vector3 reference13 = ref SaveVertex[num];
				reference13 = new Vector3(num6, 0f - num4, 0f);
				num++;
				ref Vector3 reference14 = ref SaveVertex[num];
				reference14 = new Vector3(num3, 0f - num4, 0f);
				num++;
			}
			ref Vector3 reference15 = ref SaveVertex[SaveVertex.Length - 1];
			reference15 = SaveVertex[0];
			break;
		}
		}
		float num33 = 0f;
		Vector3 vector = Vector3.zero;
		for (int n = 0; n < NvertsPerRing; n++)
		{
			if (n > 0)
			{
				num33 += (SaveVertex[n] - vector).magnitude;
			}
			ref Vector2 reference43 = ref SaveUV[n];
			reference43 = new Vector2(0f, num33 * uvscale.y);
			vector = SaveVertex[n];
		}
		for (int num34 = 0; num34 < NvertsPerRing; num34++)
		{
			SaveUV[num34].y /= num33;
		}
		float num35 = 0f;
		switch (wtype)
		{
		case MegaHoseType.Round:
			num35 = rndrot;
			break;
		case MegaHoseType.Rectangle:
			num35 = rectrotangle;
			break;
		case MegaHoseType.DSection:
			num35 = dsecrotangle;
			break;
		}
		if (num35 != 0f)
		{
			num35 *= (float)Math.PI / 180f;
			float num36 = Mathf.Cos(num35);
			float num37 = Mathf.Sin(num35);
			for (int num38 = 0; num38 < NvertsPerRing; num38++)
			{
				float x = SaveVertex[num38].x * num36 - SaveVertex[num38].y * num37;
				float y3 = SaveVertex[num38].x * num37 + SaveVertex[num38].y * num36;
				SaveVertex[num38].x = x;
				SaveVertex[num38].y = y3;
			}
		}
		if (calcnormals)
		{
			for (int num39 = 0; num39 < NvertsPerRing; num39++)
			{
				int num40 = (num39 + 1) % NvertsPerRing;
				Vector3 normalized = (SaveVertex[num40] - SaveVertex[num39]).normalized;
				ref Vector3 reference44 = ref SaveNormals[num39];
				reference44 = new Vector3(normalized.y, 0f - normalized.x, 0f);
			}
		}
	}

	private void FixHoseFillet()
	{
		float f = rectwidth;
		float f2 = rectdepth;
		float num = rectfillet;
		float num2 = 0.5f * Mathf.Abs(f2);
		float num3 = 0.5f * Mathf.Abs(f);
		float num4 = ((!(num2 > num3)) ? num2 : num3);
		if (num > num4)
		{
			rectfillet = num4;
		}
	}

	private float RND11()
	{
		return (UnityEngine.Random.Range(0f, 32768f) - 16383.5f) / 16383.5f;
	}

	private void Mult1X3(Vector3 A, Matrix4x4 B, ref Vector3 C)
	{
		C[0] = A[0] * B[0, 0] + A[1] * B[0, 1] + A[2] * B[0, 2];
		C[1] = A[0] * B[1, 0] + A[1] * B[1, 1] + A[2] * B[1, 2];
		C[2] = A[0] * B[2, 0] + A[1] * B[2, 1] + A[2] * B[2, 2];
	}

	private void Mult1X4(Vector4 A, Matrix4x4 B, ref Vector4 C)
	{
		C[0] = A[0] * B[0, 0] + A[1] * B[0, 1] + A[2] * B[0, 2] + A[3] * B[0, 3];
		C[1] = A[0] * B[1, 0] + A[1] * B[1, 1] + A[2] * B[1, 2] + A[3] * B[1, 3];
		C[2] = A[0] * B[2, 0] + A[1] * B[2, 1] + A[2] * B[2, 2] + A[3] * B[2, 3];
		C[3] = A[0] * B[3, 0] + A[1] * B[3, 1] + A[2] * B[3, 2] + A[3] * B[3, 3];
	}

	private void SetUpRotation(Vector3 Q, Vector3 W, float Theta, ref Matrix4x4 Rq)
	{
		Vector3 C = Vector3.zero;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = W[0] * W[0];
		float num2 = W[1] * W[1];
		float num3 = W[2] * W[2];
		float num4 = W[0] * W[1];
		float num5 = W[0] * W[2];
		float num6 = W[1] * W[2];
		float num7 = Mathf.Cos(Theta);
		float num8 = 1f - num7;
		float num9 = Mathf.Sin(Theta);
		identity[0, 0] = num + (1f - num) * num7;
		identity[1, 0] = num4 * num8 + W[2] * num9;
		identity[2, 0] = num5 * num8 - W[1] * num9;
		identity[0, 1] = num4 * num8 - W[2] * num9;
		identity[1, 1] = num2 + (1f - num2) * num7;
		identity[2, 1] = num6 * num8 + W[0] * num9;
		identity[0, 2] = num5 * num8 + W[1] * num9;
		identity[1, 2] = num6 * num8 - W[0] * num9;
		identity[2, 2] = num3 + (1f - num3) * num7;
		Mult1X3(Q, identity, ref C);
		Rq.SetColumn(0, identity.GetColumn(0));
		Rq.SetColumn(1, identity.GetColumn(1));
		Rq.SetColumn(2, identity.GetColumn(2));
		Rq[0, 3] = Q[0] - C.x;
		Rq[1, 3] = Q[1] - C.y;
		Rq[2, 3] = Q[2] - C.z;
		float num10 = (Rq[3, 2] = 0f);
		float num12 = num10;
		num10 = (Rq[3, 1] = num12);
		num12 = num10;
		Rq[3, 0] = num12;
		Rq[3, 3] = 1f;
	}

	private void RotateOnePoint(ref Vector3 Pin, Vector3 Q, Vector3 W, float Theta)
	{
		Matrix4x4 Rq = Matrix4x4.identity;
		Vector4 C = Vector3.zero;
		SetUpRotation(Q, W, Theta, ref Rq);
		Vector4 a = Pin;
		a[3] = 1f;
		Mult1X4(a, Rq, ref C);
		Pin = C;
	}

	private void Update()
	{
		if (animatebulge)
		{
			bulgeoffset += bulgespeed * Time.deltaTime;
			if (bulgeoffset > maxbulge)
			{
				bulgeoffset -= maxbulge - minbulge;
			}
			if (bulgeoffset < minbulge)
			{
				bulgeoffset += maxbulge - minbulge;
			}
			updatemesh = true;
		}
		if ((bool)custnode)
		{
			if (custnode.transform.position != endp1)
			{
				endp1 = custnode.transform.position;
				updatemesh = true;
			}
			if (custnode.transform.eulerAngles != endr1)
			{
				endr1 = custnode.transform.eulerAngles;
				updatemesh = true;
			}
		}
		if ((bool)custnode2)
		{
			if (custnode2.transform.position != endp2)
			{
				endp1 = custnode2.transform.position;
				updatemesh = true;
			}
			if (custnode2.transform.eulerAngles != endr2)
			{
				endr2 = custnode2.transform.eulerAngles;
				updatemesh = true;
			}
		}
		if (!dolateupdate && (visible || InvisibleUpdate) && updatemesh)
		{
			updatemesh = false;
			BuildMesh();
		}
	}

	private void LateUpdate()
	{
		if (dolateupdate && (visible || InvisibleUpdate) && updatemesh)
		{
			updatemesh = false;
			BuildMesh();
		}
	}

	private void BuildMesh()
	{
		if (!mesh)
		{
			mesh = GetComponent<MeshFilter>().sharedMesh;
			if (mesh == null)
			{
				updatemesh = true;
				return;
			}
		}
		if (hosespline.knots.Count == 0)
		{
			hosespline.AddKnot(Vector3.zero, Vector3.zero, Vector3.zero);
			hosespline.AddKnot(Vector3.zero, Vector3.zero, Vector3.zero);
		}
		FixHoseFillet();
		bool flag = freecreate;
		if (!flag && (!custnode || !custnode2))
		{
			flag = true;
		}
		if ((bool)custnode && (bool)custnode2)
		{
			flag = false;
		}
		float num = 0f;
		Tlocal = Matrix4x4.identity;
		starty = Vector3.zero;
		roty = Vector3.zero;
		yangle = 0f;
		Vector3 zero = Vector3.zero;
		if (flag)
		{
			num = noreflength;
		}
		else
		{
			zero = up;
			Matrix4x4 localToWorldMatrix = custnode.transform.localToWorldMatrix;
			Matrix4x4 localToWorldMatrix2 = custnode2.transform.localToWorldMatrix;
			Matrix4x4 identity = Matrix4x4.identity;
			Matrix4x4 identity2 = Matrix4x4.identity;
			identity = Matrix4x4.TRS(offset, Quaternion.Euler(rotate), scale).inverse;
			identity2 = Matrix4x4.TRS(offset1, Quaternion.Euler(rotate1), scale1).inverse;
			S = base.transform.localToWorldMatrix;
			Matrix4x4 mat = localToWorldMatrix;
			Matrix4x4 mat2 = localToWorldMatrix2;
			MegaMatrix.NoTrans(ref mat);
			MegaMatrix.NoTrans(ref mat2);
			Vector3 vector = localToWorldMatrix.MultiplyPoint(identity.GetColumn(3));
			Vector3 vector2 = localToWorldMatrix2.MultiplyPoint(identity2.GetColumn(3));
			Vector3 v = mat.MultiplyPoint(identity.GetColumn(2));
			Vector3 v2 = mat2.MultiplyPoint(identity2.GetColumn(2));
			starty = mat.MultiplyPoint(identity.GetColumn(1));
			Vector3 v3 = mat2.MultiplyPoint(identity2.GetColumn(1));
			Matrix4x4 mat3 = S.inverse;
			Vector3 p = mat3.MultiplyPoint(vector);
			Matrix4x4 mat4 = localToWorldMatrix;
			MegaMatrix.NoTrans(ref mat4);
			Vector3 lhs = mat4.MultiplyPoint(zero);
			num = (vector2 - vector).magnitude;
			Vector3 vector3 = ((!(num < 0.01f)) ? (vector2 - vector).normalized : vector.normalized);
			Vector3 normalized = Vector3.Cross(lhs, vector3).normalized;
			Vector3 normalized2 = Vector3.Cross(vector3, normalized).normalized;
			MegaMatrix.NoTrans(ref mat3);
			Vector3 vector4 = mat3.MultiplyPoint(normalized);
			Vector3 vector5 = mat3.MultiplyPoint(normalized2);
			Vector3 vector6 = mat3.MultiplyPoint(vector3);
			Tlocal.SetColumn(0, vector4);
			Tlocal.SetColumn(1, vector5);
			Tlocal.SetColumn(2, vector6);
			MegaMatrix.SetTrans(ref Tlocal, p);
			Matrix4x4 mat5 = Tlocal;
			MegaMatrix.NoTrans(ref mat5);
			mat5 = mat5.inverse;
			float num2 = tension1;
			v = tension2 * mat5.MultiplyPoint(v);
			v2 = num2 * mat5.MultiplyPoint(v2);
			starty = mat5.MultiplyPoint(starty);
			v3 = mat5.MultiplyPoint(v3);
			yangle = Mathf.Acos(Vector3.Dot(starty, v3));
			if (yangle > 0.0001f)
			{
				roty = Vector3.Cross(starty, v3).normalized;
			}
			else
			{
				roty = Vector3.zero;
			}
			Vector3 zero2 = Vector3.zero;
			Vector3 vector7 = new Vector3(0f, 0f, num);
			hosespline.knots[0].p = zero2;
			hosespline.knots[0].invec = zero2 - v;
			hosespline.knots[0].outvec = zero2 + v;
			hosespline.knots[1].p = vector7;
			hosespline.knots[1].invec = vector7 + v2;
			hosespline.knots[1].outvec = vector7 - v2;
			hosespline.CalcLength();
		}
		MegaHoseType megaHoseType = wiretype;
		int num3 = segments;
		if (num3 < 3)
		{
			num3 = 3;
		}
		if (rebuildcross)
		{
			rebuildcross = false;
			int num4 = 0;
			int num5 = 0;
			switch (megaHoseType)
			{
			case MegaHoseType.Round:
				NvertsPerRing = rndsides;
				if (NvertsPerRing < 3)
				{
					NvertsPerRing = 3;
				}
				break;
			case MegaHoseType.Rectangle:
				num4 = rectfilletsides;
				if (num4 < 0)
				{
					num4 = 0;
				}
				if (smooth == MegaHoseSmooth.SMOOTHNONE)
				{
					NvertsPerRing = ((num4 <= 0) ? 8 : (8 + 4 * (num4 - 1)));
				}
				else
				{
					NvertsPerRing = ((num4 <= 0) ? 4 : (8 + 4 * (num4 - 1)));
				}
				break;
			default:
			{
				num4 = dsecfilletsides;
				if (num4 < 0)
				{
					num4 = 0;
				}
				num5 = dsecrndsides;
				if (num5 < 2)
				{
					num5 = 2;
				}
				int num6 = num5 - 1;
				NvertsPerRing = ((num4 <= 0) ? (4 + num6) : (6 + num6 + 2 * (num4 - 1)));
				break;
			}
			}
			NvertsPerRing++;
			int num7 = 0;
			Nverts = (num3 + 1) * (NvertsPerRing + 1);
			if (capends)
			{
				Nverts += 2;
			}
			int nvertsPerRing = NvertsPerRing;
			int num8 = 6 * NvertsPerRing;
			num7 = num3 * num8;
			if (capends)
			{
				num7 += 2 * nvertsPerRing;
			}
			if (SaveVertex == null || SaveVertex.Length != NvertsPerRing)
			{
				SaveVertex = new Vector3[NvertsPerRing];
				SaveUV = new Vector2[NvertsPerRing];
			}
			if (calcnormals && (SaveNormals == null || SaveNormals.Length != NvertsPerRing))
			{
				SaveNormals = new Vector3[NvertsPerRing];
			}
			MakeSaveVertex(NvertsPerRing, num4, num5, megaHoseType);
			if (verts == null || verts.Length != Nverts)
			{
				verts = new Vector3[Nverts];
				uvs = new Vector2[Nverts];
				faces = new int[num7 * 3];
			}
			if (calcnormals && (normals == null || normals.Length != Nverts))
			{
				normals = new Vector3[Nverts];
			}
		}
		if (Nverts == 0)
		{
			return;
		}
		bool flag2 = mapmemapme;
		int num9 = 0;
		int num10 = Nverts - 1;
		int num11 = num10 - 1;
		int nvertsPerRing2 = NvertsPerRing;
		int num12 = num3 + 1;
		float num13 = flexstart;
		float num14 = flexstop;
		int num15 = flexcycles;
		float num16 = flexdiameter;
		Vector2 zero3 = Vector2.zero;
		Matrix4x4 mat6 = Matrix4x4.identity;
		Matrix4x4 mat7 = Matrix4x4.identity;
		for (int i = 0; i < num12; i++)
		{
			float num17 = (float)i / (float)num3;
			Vector3 vector8;
			Vector3 vector9;
			Vector3 vector11;
			Vector3 vector10;
			if (flag)
			{
				vector8 = new Vector3(0f, 0f, num * num17);
				vector9 = new Vector3(1f, 0f, 0f);
				vector10 = new Vector3(0f, 1f, 0f);
				vector11 = new Vector3(0f, 0f, 1f);
			}
			else
			{
				int k = 0;
				vector8 = hosespline.InterpCurve3D(num17, type: true, ref k);
				vector11 = (hosespline.InterpCurve3D(num17 + 0.001f, type: true, ref k) - vector8).normalized;
				vector10 = starty;
				if (yangle > 0.0001f)
				{
					RotateOnePoint(ref vector10, Vector3.zero, roty, num17 * yangle);
				}
				vector9 = Vector3.Cross(vector10, vector11).normalized;
				vector10 = Vector3.Cross(vector11, vector9);
			}
			mat6.SetColumn(0, vector9);
			mat6.SetColumn(1, vector10);
			mat6.SetColumn(2, vector11);
			MegaMatrix.SetTrans(ref mat6, vector8);
			if (!flag)
			{
				mat6 = Tlocal * mat6;
			}
			if (calcnormals)
			{
				mat7 = mat6;
				MegaMatrix.NoTrans(ref mat7);
			}
			float num20;
			if (num17 > num13 && num17 < num14 && flexon)
			{
				float num18 = num14 - num13;
				if (num18 < 0.01f)
				{
					num18 = 0.01f;
				}
				float num19 = (num17 - num13) / num18;
				float f = (float)num15 * num19 * ((float)Math.PI * 2f) + (float)Math.PI / 2f;
				num20 = 1f + num16 * (1f - Mathf.Sin(f));
			}
			else
			{
				num20 = 0f;
			}
			if (usebulgecurve)
			{
				num20 = ((num20 != 0f) ? (num20 + bulge.Evaluate(num17 + bulgeoffset) * bulgeamount) : (1f + bulge.Evaluate(num17 + bulgeoffset) * bulgeamount));
			}
			zero3.x = 0.999999f * num17 * uvscale.x;
			for (int j = 0; j < NvertsPerRing; j++)
			{
				int num21 = j;
				if (flag2)
				{
					zero3.y = SaveUV[num21].y;
					uvs[num9] = zero3;
				}
				if (num20 != 0f)
				{
					ref Vector3 reference = ref verts[num9];
					reference = mat6.MultiplyPoint(num20 * SaveVertex[num21]);
				}
				else
				{
					ref Vector3 reference2 = ref verts[num9];
					reference2 = mat6.MultiplyPoint(SaveVertex[num21]);
				}
				if (calcnormals)
				{
					ref Vector3 reference3 = ref normals[num9];
					reference3 = mat7.MultiplyPoint(SaveNormals[num21]).normalized;
				}
				num9++;
			}
			if (!capends)
			{
				continue;
			}
			if (i == 0)
			{
				ref Vector3 reference4 = ref verts[num11];
				reference4 = ((!flag) ? Tlocal.MultiplyPoint(vector8) : vector8);
				if (flag2)
				{
					ref Vector2 reference5 = ref uvs[num11];
					reference5 = Vector3.zero;
				}
			}
			else if (i == num3)
			{
				ref Vector3 reference6 = ref verts[num10];
				reference6 = ((!flag) ? Tlocal.MultiplyPoint(vector8) : vector8);
				if (flag2)
				{
					ref Vector2 reference7 = ref uvs[num10];
					reference7 = Vector3.zero;
				}
			}
		}
		int num22 = 0;
		int num23 = num11;
		if (capends)
		{
			for (int l = 0; l < NvertsPerRing - 1; l++)
			{
				int num24 = l;
				int num25 = ((l >= nvertsPerRing2) ? (num24 - nvertsPerRing2) : (num24 + 1));
				faces[num22++] = num25;
				faces[num22++] = num24;
				faces[num22++] = num23;
			}
		}
		int nvertsPerRing3 = NvertsPerRing;
		for (int m = 0; m < num3; m++)
		{
			for (int n = 0; n < NvertsPerRing - 1; n++)
			{
				int num26 = m * nvertsPerRing3 + n;
				int num27 = num26 + 1;
				int num28 = num26 + nvertsPerRing3;
				num23 = num27 + nvertsPerRing3;
				faces[num22++] = num26;
				faces[num22++] = num27;
				faces[num22++] = num23;
				faces[num22++] = num26;
				faces[num22++] = num23;
				faces[num22++] = num28;
			}
		}
		int num29 = num3 * nvertsPerRing3;
		num23 = Nverts - 1;
		if (capends)
		{
			for (int num30 = 0; num30 < NvertsPerRing - 1; num30++)
			{
				int num31 = num30 + num29;
				int num32 = ((num30 >= nvertsPerRing2) ? (num31 - nvertsPerRing2) : (num31 + 1));
				faces[num22++] = num31;
				faces[num22++] = num32;
				faces[num22++] = num23;
			}
		}
		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = faces;
		if (calcnormals)
		{
			mesh.normals = normals;
		}
		else
		{
			mesh.RecalculateNormals();
		}
		mesh.RecalculateBounds();
		if (optimize)
		{
			mesh.Optimize();
		}
		if (calctangents)
		{
			MegaUtils.BuildTangents(mesh);
		}
		if (recalcCollider)
		{
			if (meshCol == null)
			{
				meshCol = GetComponent<MeshCollider>();
			}
			if (meshCol != null)
			{
				meshCol.sharedMesh = null;
				meshCol.sharedMesh = mesh;
			}
		}
	}

	public void CalcMatrix(ref Matrix4x4 mat, float incr)
	{
		mat = Tlocal;
	}

	private static float findmappos(float curpos)
	{
		float num;
		return num = (((num = curpos) < 0f) ? 0f : ((!(num > 1f)) ? num : 1f));
	}

	private void DisplayNormals()
	{
	}

	public Vector3 GetPosition(float alpha)
	{
		Matrix4x4 matrix4x = base.transform.localToWorldMatrix * Tlocal;
		int k = 0;
		return matrix4x.MultiplyPoint(hosespline.InterpCurve3D(alpha, type: true, ref k));
	}
}
