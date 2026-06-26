using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MegaShape : MonoBehaviour
{
	public enum CrossSectionType
	{
		Circle,
		Box
	}

	private const float CIRCLE_VECTOR_LENGTH = 0.5517862f;

	public MegaAxis axis = MegaAxis.Y;

	public Color col1 = new Color(1f, 1f, 1f, 1f);

	public Color col2 = new Color(0.1f, 0.1f, 0.1f, 1f);

	public Color KnotCol = new Color(0f, 1f, 0f, 1f);

	public Color HandleCol = new Color(1f, 0f, 0f, 1f);

	public Color VecCol = new Color(0.1f, 0.1f, 0.2f, 0.5f);

	public float KnotSize = 10f;

	public float stepdist = 1f;

	public bool normalizedInterp = true;

	public bool drawHandles = true;

	public bool drawKnots = true;

	public bool drawspline = true;

	public bool drawTwist;

	public bool lockhandles = true;

	public bool showorigin = true;

	public bool usesnap;

	public bool usesnaphandles;

	public Vector3 snap = Vector3.one;

	public MegaHandleType handleType;

	public float CursorPos;

	public List<MegaSpline> splines = new List<MegaSpline>();

	public bool showanimations;

	public float keytime;

	public float defRadius = 1f;

	public float testtime;

	public float time;

	public bool animate;

	public float speed = 1f;

	public int selcurve;

	public bool imported;

	public float CursorPercent;

	private float t;

	public float MaxTime = 1f;

	public MegaRepeatMode LoopMode;

	public bool dolateupdate;

	public bool makeMesh;

	public MeshShapeType meshType;

	public bool DoubleSided = true;

	public bool CalcTangents;

	public bool GenUV = true;

	public bool PhysUV;

	public float Height;

	public int HeightSegs = 1;

	public int Sides = 4;

	public float TubeStep = 0.1f;

	public float Start;

	public float End = 100f;

	public float Rotate;

	public Vector3 Pivot = Vector3.zero;

	public Vector2 UVOffset = Vector2.zero;

	public Vector2 UVRotate = Vector2.zero;

	public Vector2 UVScale = Vector2.one;

	public Vector2 UVOffset1 = Vector2.zero;

	public Vector2 UVRotate1 = Vector2.zero;

	public Vector2 UVScale1 = Vector2.one;

	public Vector2 UVOffset2 = Vector2.zero;

	public Vector2 UVRotate3 = Vector2.zero;

	public Vector2 UVScale3 = Vector2.one;

	public bool autosmooth;

	public float smoothness = 1.1f;

	public Material mat1;

	public Material mat2;

	public Material mat3;

	public bool UseHeightCurve;

	public AnimationCurve heightCrv = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public float heightOff;

	public Mesh shapemesh;

	private static float lastout;

	private static float lastin = -9999f;

	private List<Vector3> verts = new List<Vector3>();

	private List<Vector2> uvs = new List<Vector2>();

	private List<int> tris = new List<int>();

	private List<int> tris1 = new List<int>();

	private List<int> tris2 = new List<int>();

	private Vector3[] cross;

	public int tsides = 8;

	public CrossSectionType crossType;

	public float Twist;

	public int strands = 1;

	public float tradius = 0.1f;

	public float offset;

	public float uvtilex = 1f;

	public float uvtiley = 1f;

	public float uvtwist;

	public float TubeLength = 1f;

	public float TubeStart;

	public float SegsPerUnit = 20f;

	public float TwistPerUnit;

	public float strandRadius;

	public float startAng;

	public float rotate;

	private int segments;

	public bool cap;

	private Vector3[] tverts;

	private Vector2[] tuvs;

	private int[] ttris;

	private Matrix4x4 tm;

	private Matrix4x4 mat;

	private Matrix4x4 wtm;

	public MegaAxis RopeUp = MegaAxis.Y;

	private Vector3 ropeup = Vector3.up;

	public AnimationCurve scaleX = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public AnimationCurve scaleY = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool unlinkScale;

	public float boxwidth = 0.2f;

	public float boxheight = 0.2f;

	private float[] boxuv = new float[8];

	public MegaAxis raxis;

	public int ribsegs = 1;

	private static int CURVELENGTHSTEPS = 5;

	public bool conform;

	public GameObject target;

	public Collider conformCollider;

	public float[] offsets;

	public float[] last;

	public float conformAmount = 1f;

	public float raystartoff;

	public float raydist = 10f;

	public float conformOffset;

	private float minz;

	public float conformWeight = 1f;

	public virtual void MakeShape()
	{
		Matrix4x4 matrix = GetMatrix();
		float num = 0.5517862f * defRadius;
		MegaSpline megaSpline = NewSpline();
		for (int i = 0; i < 4; i++)
		{
			float f = (float)Math.PI * 2f * (float)i / 4f;
			float num2 = Mathf.Sin(f);
			float num3 = Mathf.Cos(f);
			Vector3 vector = new Vector3(num3 * defRadius, num2 * defRadius, 0f);
			Vector3 vector2 = new Vector3(num2 * num, (0f - num3) * num, 0f);
			megaSpline.AddKnot(vector, vector + vector2, vector - vector2, matrix);
		}
		megaSpline.closed = true;
		CalcLength();
	}

	public virtual string GetHelpURL()
	{
		return "?page_id=390";
	}

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/" + GetHelpURL());
	}

	[ContextMenu("Reset Mesh Info")]
	public void ResetMesh()
	{
		shapemesh = null;
		BuildMesh();
	}

	public Matrix4x4 GetMatrix()
	{
		Matrix4x4 identity = Matrix4x4.identity;
		switch (axis)
		{
		case MegaAxis.X:
			MegaMatrix.RotateY(ref identity, -(float)Math.PI / 2f);
			MegaMatrix.Scale(ref identity, -Vector3.one, trans: false);
			break;
		case MegaAxis.Y:
			MegaMatrix.RotateX(ref identity, (float)Math.PI / 2f);
			break;
		}
		return identity;
	}

	public void CopyIDS(int curve)
	{
		if (curve > 0 && curve < splines.Count)
		{
			MegaSpline megaSpline = splines[curve];
			MegaSpline megaSpline2 = splines[curve - 1];
			for (int i = 0; i < megaSpline2.knots.Count && i < megaSpline.knots.Count; i++)
			{
				megaSpline.knots[i].id = megaSpline2.knots[i].id;
			}
		}
	}

	public void Reverse(int c)
	{
		if (c >= 0 && c < splines.Count)
		{
			splines[c].Reverse();
		}
	}

	public void SetHeight(int c, float y)
	{
		if (c >= 0 && c < splines.Count)
		{
			splines[c].SetHeight(y);
		}
	}

	public void SetTwist(int c, float twist)
	{
		if (c >= 0 && c < splines.Count)
		{
			splines[c].SetTwist(twist);
		}
	}

	public MegaSpline NewSpline()
	{
		if (splines.Count == 0)
		{
			MegaSpline item = new MegaSpline();
			splines.Add(item);
		}
		MegaSpline megaSpline = splines[0];
		megaSpline.knots.Clear();
		megaSpline.closed = false;
		return megaSpline;
	}

	private void Reset()
	{
		MakeShape();
	}

	private void Awake()
	{
		if (splines.Count == 0)
		{
			MakeShape();
		}
	}

	private void Update()
	{
		if (!dolateupdate)
		{
			DoUpdate();
		}
	}

	private void LateUpdate()
	{
		if (dolateupdate)
		{
			DoUpdate();
		}
	}

	private void DoUpdate()
	{
		if (!animate)
		{
			return;
		}
		BuildMesh();
		time += Time.deltaTime * speed;
		switch (LoopMode)
		{
		case MegaRepeatMode.Loop:
			t = Mathf.Repeat(time, MaxTime);
			break;
		case MegaRepeatMode.PingPong:
			t = Mathf.PingPong(time, MaxTime);
			break;
		case MegaRepeatMode.Clamp:
			t = Mathf.Clamp(time, 0f, MaxTime);
			break;
		}
		for (int i = 0; i < splines.Count; i++)
		{
			if (splines[i].splineanim != null && splines[i].splineanim.Enabled)
			{
				splines[i].splineanim.GetState1(splines[i], t);
				splines[i].CalcLength();
			}
			else
			{
				if (splines[i].animations == null || splines[i].animations.Count <= 0)
				{
					continue;
				}
				for (int j = 0; j < splines[i].animations.Count; j++)
				{
					Vector3 vector = splines[i].animations[j].con.GetVector3(t);
					switch (splines[i].animations[j].t)
					{
					case 0:
						splines[splines[i].animations[j].s].knots[splines[i].animations[j].p].invec = vector;
						break;
					case 1:
						splines[splines[i].animations[j].s].knots[splines[i].animations[j].p].p = vector;
						break;
					case 2:
						splines[splines[i].animations[j].s].knots[splines[i].animations[j].p].outvec = vector;
						break;
					}
				}
				splines[i].CalcLength();
			}
		}
	}

	public void Centre(float scale, Vector3 axis)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = 0; i < splines.Count; i++)
		{
			num += splines[i].knots.Count;
			for (int j = 0; j < splines[i].knots.Count; j++)
			{
				zero += splines[i].knots[j].p;
			}
		}
		zero /= (float)num;
		for (int k = 0; k < splines.Count; k++)
		{
			for (int l = 0; l < splines[k].knots.Count; l++)
			{
				splines[k].knots[l].p -= zero;
				splines[k].knots[l].invec -= zero;
				splines[k].knots[l].outvec -= zero;
				splines[k].knots[l].p *= scale;
				splines[k].knots[l].invec *= scale;
				splines[k].knots[l].outvec *= scale;
				splines[k].knots[l].p = Vector3.Scale(splines[k].knots[l].p, axis);
				splines[k].knots[l].invec = Vector3.Scale(splines[k].knots[l].invec, axis);
				splines[k].knots[l].outvec = Vector3.Scale(splines[k].knots[l].outvec, axis);
			}
		}
	}

	public void Centre(float scale, Vector3 axis, int start)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = start; i < splines.Count; i++)
		{
			num += splines[i].knots.Count;
			for (int j = 0; j < splines[i].knots.Count; j++)
			{
				zero += splines[i].knots[j].p;
			}
		}
		zero /= (float)num;
		for (int k = start; k < splines.Count; k++)
		{
			for (int l = 0; l < splines[k].knots.Count; l++)
			{
				splines[k].knots[l].p -= zero;
				splines[k].knots[l].invec -= zero;
				splines[k].knots[l].outvec -= zero;
				splines[k].knots[l].p *= scale;
				splines[k].knots[l].invec *= scale;
				splines[k].knots[l].outvec *= scale;
				splines[k].knots[l].p = Vector3.Scale(splines[k].knots[l].p, axis);
				splines[k].knots[l].invec = Vector3.Scale(splines[k].knots[l].invec, axis);
				splines[k].knots[l].outvec = Vector3.Scale(splines[k].knots[l].outvec, axis);
			}
		}
	}

	public void Scale(float scale)
	{
		for (int i = 0; i < splines.Count; i++)
		{
			for (int j = 0; j < splines[i].knots.Count; j++)
			{
				splines[i].knots[j].invec *= scale;
				splines[i].knots[j].p *= scale;
				splines[i].knots[j].outvec *= scale;
			}
			if (splines[i].animations == null)
			{
				continue;
			}
			for (int k = 0; k < splines[i].animations.Count; k++)
			{
				if (splines[i].animations[k].con != null)
				{
					splines[i].animations[k].con.Scale(scale);
				}
			}
		}
		CalcLength();
	}

	public void Scale(float scale, int start)
	{
		for (int i = start; i < splines.Count; i++)
		{
			for (int j = 0; j < splines[i].knots.Count; j++)
			{
				splines[i].knots[j].invec *= scale;
				splines[i].knots[j].p *= scale;
				splines[i].knots[j].outvec *= scale;
			}
			if (splines[i].animations == null)
			{
				continue;
			}
			for (int k = 0; k < splines[i].animations.Count; k++)
			{
				if (splines[i].animations[k].con != null)
				{
					splines[i].animations[k].con.Scale(scale);
				}
			}
		}
		CalcLength();
	}

	public void Scale(Vector3 scale)
	{
		for (int i = 0; i < splines.Count; i++)
		{
			for (int j = 0; j < splines[i].knots.Count; j++)
			{
				splines[i].knots[j].invec.x *= scale.x;
				splines[i].knots[j].invec.y *= scale.y;
				splines[i].knots[j].invec.z *= scale.z;
				splines[i].knots[j].p.x *= scale.x;
				splines[i].knots[j].p.y *= scale.y;
				splines[i].knots[j].p.z *= scale.z;
				splines[i].knots[j].outvec.x *= scale.x;
				splines[i].knots[j].outvec.y *= scale.y;
				splines[i].knots[j].outvec.z *= scale.z;
			}
			if (splines[i].animations == null)
			{
				continue;
			}
			for (int k = 0; k < splines[i].animations.Count; k++)
			{
				if (splines[i].animations[k].con != null)
				{
					splines[i].animations[k].con.Scale(scale);
				}
			}
		}
		CalcLength();
	}

	public void MoveSpline(Vector3 delta)
	{
		for (int i = 0; i < splines.Count; i++)
		{
			MoveSpline(delta, i, calc: false);
		}
		CalcLength();
	}

	public void MoveSpline(Vector3 delta, int c, bool calc)
	{
		for (int i = 0; i < splines[c].knots.Count; i++)
		{
			splines[c].knots[i].invec += delta;
			splines[c].knots[i].p += delta;
			splines[c].knots[i].outvec += delta;
		}
		if (splines[c].animations != null)
		{
			for (int j = 0; j < splines[c].animations.Count; j++)
			{
				if (splines[c].animations[j].con != null)
				{
					splines[c].animations[j].con.Move(delta);
				}
			}
		}
		if (calc)
		{
			CalcLength(c);
		}
	}

	public void RotateSpline(Vector3 rot, int c, bool calc)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(rot), Vector3.one);
		for (int i = 0; i < splines[c].knots.Count; i++)
		{
			splines[c].knots[i].invec = matrix4x.MultiplyPoint3x4(splines[c].knots[i].invec);
			splines[c].knots[i].outvec = matrix4x.MultiplyPoint3x4(splines[c].knots[i].outvec);
			splines[c].knots[i].p = matrix4x.MultiplyPoint3x4(splines[c].knots[i].p);
		}
		if (splines[c].animations != null)
		{
			for (int j = 0; j < splines[c].animations.Count; j++)
			{
				if (splines[c].animations[j].con != null)
				{
					splines[c].animations[j].con.Rotate(matrix4x);
				}
			}
		}
		if (calc)
		{
			CalcLength(c);
		}
	}

	public int GetSpline(int p, ref MegaKnotAnim ma)
	{
		int num = 0;
		int num2 = p / 3;
		for (int i = 0; i < splines.Count; i++)
		{
			int num3 = num + splines[i].knots.Count;
			if (num2 < num3)
			{
				ma.s = i;
				ma.p = num2 - num;
				ma.t = p % 3;
				return i;
			}
			num = num3;
		}
		Debug.Log("Cant find point in spline");
		return 0;
	}

	public float GetCurveLength(int curve)
	{
		if (curve < splines.Count)
		{
			return splines[curve].length;
		}
		return splines[0].length;
	}

	public float CalcLength(int curve, int step)
	{
		if (curve < splines.Count)
		{
			return splines[curve].CalcLength(step);
		}
		return 0f;
	}

	[ContextMenu("Recalc Length")]
	public void ReCalcLength()
	{
		CalcLength();
	}

	public float CalcLength()
	{
		float num = 0f;
		for (int i = 0; i < splines.Count; i++)
		{
			num += splines[i].CalcLength();
		}
		return num;
	}

	public float CalcLength(int curve)
	{
		return splines[curve].CalcLength();
	}

	public Vector3 GetKnotPos(int curve, int knot)
	{
		return splines[curve].knots[knot].p;
	}

	public Vector3 GetKnotInVec(int curve, int knot)
	{
		return splines[curve].knots[knot].invec;
	}

	public Vector3 GetKnotOutVec(int curve, int knot)
	{
		return splines[curve].knots[knot].outvec;
	}

	public void SetKnotPos(int curve, int knot, Vector3 p)
	{
		splines[curve].knots[knot].p = p;
		CalcLength();
	}

	public void SetKnot(int curve, int knot, Vector3 p, Vector3 intan, Vector3 outtan)
	{
		splines[curve].knots[knot].p = p;
		splines[curve].knots[knot].invec = intan;
		splines[curve].knots[knot].outvec = outtan;
		CalcLength();
	}

	public void MoveKnot(int curve, int knot, Vector3 p)
	{
		Vector3 vector = p - splines[curve].knots[knot].p;
		splines[curve].knots[knot].p = p;
		splines[curve].knots[knot].invec += vector;
		splines[curve].knots[knot].outvec += vector;
		CalcLength();
	}

	public Quaternion GetRotate(int curve, float alpha)
	{
		Vector3 vector = InterpCurve3D(curve, alpha, normalizedInterp);
		Vector3 vector2 = InterpCurve3D(curve, alpha + 0.001f, normalizedInterp);
		return Quaternion.LookRotation(vector - vector2);
	}

	public Vector3 InterpCurve3D(int curve, float alpha, bool type)
	{
		int k = 0;
		if (curve < splines.Count)
		{
			if (alpha < 0f)
			{
				if (!splines[curve].closed)
				{
					Vector3 vector = splines[curve].Interpolate(0f, type, ref k);
					Vector3 vector2 = splines[curve].Interpolate(0.01f, type, ref k) - vector;
					vector2.Normalize();
					return vector + splines[curve].length * alpha * vector2;
				}
				alpha = Mathf.Repeat(alpha, 1f);
			}
			else if (alpha > 1f)
			{
				if (!splines[curve].closed)
				{
					Vector3 vector3 = splines[curve].Interpolate(1f, type, ref k);
					Vector3 vector4 = splines[curve].Interpolate(0.99f, type, ref k) - vector3;
					vector4.Normalize();
					return vector3 + splines[curve].length * (1f - alpha) * vector4;
				}
				alpha %= 1f;
			}
			return splines[curve].Interpolate(alpha, type, ref k);
		}
		return splines[0].Interpolate(1f, type, ref k);
	}

	public Vector3 InterpCurve3D(int curve, float alpha, bool type, ref float twist)
	{
		int k = 0;
		if (curve < splines.Count)
		{
			if (alpha < 0f)
			{
				if (!splines[curve].closed)
				{
					Vector3 vector = splines[curve].Interpolate(0f, type, ref k, ref twist);
					Vector3 vector2 = splines[curve].Interpolate(0.01f, type, ref k, ref twist) - vector;
					vector2.Normalize();
					return vector + splines[curve].length * alpha * vector2;
				}
				alpha = Mathf.Repeat(alpha, 1f);
			}
			else if (alpha > 1f)
			{
				if (!splines[curve].closed)
				{
					Vector3 vector3 = splines[curve].Interpolate(1f, type, ref k, ref twist);
					Vector3 vector4 = splines[curve].Interpolate(0.99f, type, ref k, ref twist) - vector3;
					vector4.Normalize();
					return vector3 + splines[curve].length * (1f - alpha) * vector4;
				}
				alpha %= 1f;
			}
			return splines[curve].Interpolate(alpha, type, ref k, ref twist);
		}
		return splines[0].Interpolate(1f, type, ref k, ref twist);
	}

	public static float veccalc(float angstep)
	{
		if (lastin == angstep)
		{
			return lastout;
		}
		float num = Mathf.Sin(angstep);
		float num2 = Mathf.Cos(angstep);
		MegaSpline megaSpline = new MegaSpline();
		Vector3 vector = new Vector3(Mathf.Cos(0f), Mathf.Sin(0f), 0f);
		Vector3 vector2 = new Vector3(num2, num, 0f);
		float num3 = 1.5f;
		float num4 = 0f;
		int num5 = 200;
		float num6;
		while (true)
		{
			megaSpline.knots.Clear();
			num6 = (num3 + num4) / 2f;
			Vector3 outvec = vector + new Vector3(0f, num6, 0f);
			Vector3 invec = vector2 + new Vector3(num * num6, (0f - num2) * num6, 0f);
			megaSpline.AddKnot(vector, vector, outvec);
			megaSpline.AddKnot(vector2, invec, vector2);
			float num7 = 0f;
			int k = 0;
			for (int i = 0; i < 10; i++)
			{
				Vector3 vector3 = megaSpline.Interpolate((float)i / 10f, type: false, ref k);
				num7 += Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y);
			}
			num7 /= 10f;
			num5--;
			if (num7 == 1f || num5 <= 0)
			{
				break;
			}
			if (num7 > 1f)
			{
				num3 = num6;
			}
			else
			{
				num4 = num6;
			}
		}
		lastin = angstep;
		lastout = num6;
		return num6;
	}

	public Vector3 FindNearestPointWorld(Vector3 p, int iterations, ref int kn, ref Vector3 tangent, ref float alpha)
	{
		Vector3 result = base.transform.TransformPoint(FindNearestPoint(base.transform.worldToLocalMatrix.MultiplyPoint(p), iterations, ref kn, ref tangent, ref alpha));
		tangent = base.transform.TransformPoint(tangent);
		return result;
	}

	public Vector3 FindNearestPoint(Vector3 p, int iterations, ref int kn, ref Vector3 tangent, ref float alpha)
	{
		float num = float.PositiveInfinity;
		float num2 = 0f;
		iterations = Mathf.Clamp(iterations, 0, 5);
		int k = 0;
		int num3 = selcurve;
		if (num3 >= splines.Count)
		{
			num3 = splines.Count - 1;
		}
		for (float num4 = 0f; num4 <= 1f; num4 += 0.01f)
		{
			float sqrMagnitude = (splines[num3].Interpolate(num4, type: true, ref k) - p).sqrMagnitude;
			if (num > sqrMagnitude)
			{
				num = sqrMagnitude;
				num2 = num4;
			}
		}
		for (int i = 0; i < iterations; i++)
		{
			float num5 = 0.01f * Mathf.Pow(10f, 0f - (float)i);
			float num6 = num5 * 0.1f;
			for (float num7 = Mathf.Clamp01(num2 - num5); num7 <= Mathf.Clamp01(num2 + num5); num7 += num6)
			{
				float sqrMagnitude2 = (splines[num3].Interpolate(num7, type: true, ref k) - p).sqrMagnitude;
				if (num > sqrMagnitude2)
				{
					num = sqrMagnitude2;
					num2 = num7;
				}
			}
		}
		kn = k;
		tangent = InterpCurve3D(num3, num2 + 0.01f, type: true);
		alpha = num2;
		return InterpCurve3D(num3, num2, type: true);
	}

	public Vector3 FindNearestPoint(int crv, Vector3 p, int iterations, ref int kn, ref Vector3 tangent, ref float alpha)
	{
		float num = float.PositiveInfinity;
		float num2 = 0f;
		iterations = Mathf.Clamp(iterations, 0, 5);
		int k = 0;
		if (crv >= splines.Count)
		{
			crv = splines.Count - 1;
		}
		for (float num3 = 0f; num3 <= 1f; num3 += 0.01f)
		{
			float sqrMagnitude = (splines[crv].Interpolate(num3, type: true, ref k) - p).sqrMagnitude;
			if (num > sqrMagnitude)
			{
				num = sqrMagnitude;
				num2 = num3;
			}
		}
		for (int i = 0; i < iterations; i++)
		{
			float num4 = 0.01f * Mathf.Pow(10f, 0f - (float)i);
			float num5 = num4 * 0.1f;
			for (float num6 = Mathf.Clamp01(num2 - num4); num6 <= Mathf.Clamp01(num2 + num4); num6 += num5)
			{
				float sqrMagnitude2 = (splines[crv].Interpolate(num6, type: true, ref k) - p).sqrMagnitude;
				if (num > sqrMagnitude2)
				{
					num = sqrMagnitude2;
					num2 = num6;
				}
			}
		}
		kn = k;
		tangent = InterpCurve3D(crv, num2 + 0.01f, type: true);
		alpha = num2;
		return InterpCurve3D(crv, num2, type: true);
	}

	public void BuildSplineWorld(int curve, Vector3[] points, bool closed)
	{
		if (curve >= 0 && curve < splines.Count)
		{
			MegaSpline megaSpline = splines[curve];
			megaSpline.knots = new List<MegaKnot>(points.Length);
			for (int i = 0; i < points.Length; i++)
			{
				MegaKnot megaKnot = new MegaKnot();
				megaKnot.p = base.transform.worldToLocalMatrix.MultiplyPoint(points[i]);
				megaSpline.knots.Add(megaKnot);
			}
			megaSpline.closed = closed;
			AutoCurve(megaSpline);
		}
	}

	public void BuildSpline(int curve, Vector3[] points, bool closed)
	{
		if (curve >= 0 && curve < splines.Count)
		{
			MegaSpline megaSpline = splines[curve];
			megaSpline.knots = new List<MegaKnot>(points.Length);
			for (int i = 0; i < points.Length; i++)
			{
				MegaKnot megaKnot = new MegaKnot();
				megaKnot.p = points[i];
				megaSpline.knots.Add(megaKnot);
			}
			megaSpline.closed = closed;
			AutoCurve(megaSpline);
		}
	}

	public void BuildSpline(Vector3[] points, bool closed)
	{
		MegaSpline megaSpline = new MegaSpline();
		megaSpline.knots = new List<MegaKnot>(points.Length);
		for (int i = 0; i < points.Length; i++)
		{
			MegaKnot megaKnot = new MegaKnot();
			megaKnot.p = points[i];
			megaSpline.knots.Add(megaKnot);
		}
		megaSpline.closed = closed;
		splines.Add(megaSpline);
		AutoCurve(megaSpline);
	}

	public void AddToSpline(int curve, Vector3[] points)
	{
		if (curve >= 0 && curve < splines.Count)
		{
			MegaSpline megaSpline = splines[curve];
			int count = megaSpline.knots.Count;
			for (int i = 0; i < points.Length; i++)
			{
				MegaKnot megaKnot = new MegaKnot();
				megaKnot.p = points[i];
				megaSpline.knots.Add(megaKnot);
			}
			AutoCurve(megaSpline, count, count + points.Length);
		}
	}

	public void AddToSpline(int curve, Vector3 point)
	{
		if (curve >= 0 && curve < splines.Count)
		{
			MegaSpline megaSpline = splines[curve];
			MegaKnot megaKnot = new MegaKnot();
			megaKnot.p = point;
			megaSpline.knots.Add(megaKnot);
			AutoCurve(megaSpline, megaSpline.knots.Count - 2, megaSpline.knots.Count - 1);
		}
	}

	public void AutoCurve(int s)
	{
		AutoCurve(splines[s]);
	}

	public void AutoCurve(MegaSpline spline)
	{
		if (spline.closed)
		{
			Vector3 vector = (spline.knots[spline.knots.Count - 1].p + spline.knots[0].p) * 0.5f;
			for (int i = 0; i < spline.knots.Count; i++)
			{
				int index = (i + 1) % spline.knots.Count;
				Vector3 vector2 = (spline.knots[index].p + spline.knots[i].p) * 0.5f;
				Vector3 vector3 = (vector2 + vector) * 0.5f;
				spline.knots[i].invec = spline.knots[i].p + (vector - vector3) * smoothness;
				spline.knots[i].outvec = spline.knots[i].p + (vector2 - vector3) * smoothness;
				vector = vector2;
			}
		}
		else
		{
			Vector3 vector4 = (spline.knots[1].p + spline.knots[0].p) * 0.5f;
			for (int j = 1; j < spline.knots.Count - 1; j++)
			{
				Vector3 vector5 = (spline.knots[j + 1].p + spline.knots[j].p) * 0.5f;
				Vector3 vector6 = (vector5 + vector4) * 0.5f;
				spline.knots[j].invec = spline.knots[j].p + (vector4 - vector6) * smoothness;
				spline.knots[j].outvec = spline.knots[j].p + (vector5 - vector6) * smoothness;
				vector4 = vector5;
			}
		}
		spline.CalcLength();
	}

	public void AutoCurve(MegaSpline spline, int start, int end)
	{
		if (spline.closed)
		{
			int index = (start - 1) % spline.knots.Count;
			Vector3 vector = (spline.knots[index].p + spline.knots[start].p) * 0.5f;
			for (int i = start; i < end; i++)
			{
				int index2 = (i + 1) % spline.knots.Count;
				Vector3 vector2 = (spline.knots[index2].p + spline.knots[i].p) * 0.5f;
				Vector3 vector3 = (vector2 + vector) * 0.5f;
				spline.knots[i].invec = spline.knots[i].p + (vector - vector3) * smoothness;
				spline.knots[i].outvec = spline.knots[i].p + (vector2 - vector3) * smoothness;
				vector = vector2;
			}
		}
		else
		{
			int index3 = (start - 1) % spline.knots.Count;
			Vector3 vector4 = (spline.knots[index3].p + spline.knots[start].p) * 0.5f;
			for (int j = start; j < end - 1; j++)
			{
				Vector3 vector5 = (spline.knots[j + 1].p + spline.knots[j].p) * 0.5f;
				Vector3 vector6 = (vector5 + vector4) * 0.5f;
				spline.knots[j].invec = spline.knots[j].p + (vector4 - vector6) * smoothness;
				spline.knots[j].outvec = spline.knots[j].p + (vector5 - vector6) * smoothness;
				vector4 = vector5;
			}
		}
		spline.CalcLength();
	}

	public void AutoCurve()
	{
		for (int i = 0; i < splines.Count; i++)
		{
			MegaSpline spline = splines[i];
			AutoCurve(spline);
		}
	}

	private void BuildCrossSection(float rad)
	{
		if (cross == null || cross.Length != tsides)
		{
			cross = new Vector3[tsides];
		}
		float num = rotate * ((float)Math.PI / 180f);
		for (int i = 0; i < tsides; i++)
		{
			float f = num + (float)i / (float)tsides * (float)Math.PI * 2f;
			ref Vector3 reference = ref cross[i];
			reference = new Vector3(Mathf.Sin(f) * rad, 0f, Mathf.Cos(f) * rad);
		}
	}

	public void BuildTubeMesh()
	{
		BuildMultiStrandMesh();
	}

	private Matrix4x4 GetDeformMat(float percent)
	{
		float twist = 0f;
		Vector3 vector = InterpCurve3D(selcurve, percent, normalizedInterp, ref twist);
		Quaternion quaternion = Quaternion.LookRotation(InterpCurve3D(selcurve, percent + 0.001f, normalizedInterp, ref twist) - vector, ropeup);
		Quaternion quaternion2 = Quaternion.Euler(0f, 0f, twist);
		MegaMatrix.SetTR(ref wtm, vector, quaternion * quaternion2);
		wtm = mat * wtm;
		return wtm;
	}

	public void BuildBoxCrossSection(float width, float height)
	{
		if (cross == null || cross.Length != 8)
		{
			cross = new Vector3[8];
		}
		float ang = rotate * ((float)Math.PI / 180f);
		Matrix4x4 identity = Matrix4x4.identity;
		MegaMatrix.RotateY(ref identity, ang);
		ref Vector3 reference = ref cross[0];
		reference = new Vector3(width * 0.5f, 0f, height * 0.5f);
		ref Vector3 reference2 = ref cross[1];
		reference2 = new Vector3(width * 0.5f, 0f, (0f - height) * 0.5f);
		ref Vector3 reference3 = ref cross[2];
		reference3 = new Vector3(width * 0.5f, 0f, (0f - height) * 0.5f);
		ref Vector3 reference4 = ref cross[3];
		reference4 = new Vector3((0f - width) * 0.5f, 0f, (0f - height) * 0.5f);
		ref Vector3 reference5 = ref cross[4];
		reference5 = new Vector3((0f - width) * 0.5f, 0f, (0f - height) * 0.5f);
		ref Vector3 reference6 = ref cross[5];
		reference6 = new Vector3((0f - width) * 0.5f, 0f, height * 0.5f);
		ref Vector3 reference7 = ref cross[6];
		reference7 = new Vector3((0f - width) * 0.5f, 0f, height * 0.5f);
		ref Vector3 reference8 = ref cross[7];
		reference8 = new Vector3(width * 0.5f, 0f, height * 0.5f);
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference9 = ref cross[i];
			reference9 = identity.MultiplyPoint(cross[i]);
		}
		float num = 2f * boxwidth + 2f * boxheight;
		float num2 = 0f;
		boxuv[0] = 0f;
		num2 += boxheight;
		boxuv[1] = num2 / num;
		boxuv[2] = boxuv[1];
		num2 += boxwidth;
		boxuv[3] = num2 / num;
		boxuv[4] = boxuv[3];
		num2 += boxheight;
		boxuv[5] = num2 / num;
		boxuv[6] = boxuv[5];
		num2 += boxwidth;
		boxuv[7] = num2 / num;
	}

	public void BuildRibbonCrossSection(float width)
	{
		if (cross == null || cross.Length != ribsegs + 1)
		{
			cross = new Vector3[ribsegs + 1];
		}
		float ang = rotate * ((float)Math.PI / 180f);
		for (int i = 0; i <= ribsegs; i++)
		{
			float num = (float)i / (float)ribsegs * width - width * 0.5f;
			switch (raxis)
			{
			case MegaAxis.X:
			{
				ref Vector3 reference3 = ref cross[i];
				reference3 = new Vector3(num, 0f, 0f);
				break;
			}
			case MegaAxis.Y:
			{
				ref Vector3 reference2 = ref cross[i];
				reference2 = new Vector3(0f, num, 0f);
				break;
			}
			case MegaAxis.Z:
			{
				ref Vector3 reference = ref cross[i];
				reference = new Vector3(0f, 0f, num);
				break;
			}
			}
		}
		Matrix4x4 identity = Matrix4x4.identity;
		MegaMatrix.RotateY(ref identity, ang);
		for (int j = 0; j < cross.Length; j++)
		{
			ref Vector3 reference4 = ref cross[j];
			reference4 = identity.MultiplyPoint(cross[j]);
		}
	}

	private void BuildRibbonMesh()
	{
		TubeLength = Mathf.Clamp01(TubeLength);
		if (TubeLength == 0f || strands < 1)
		{
			shapemesh.Clear();
			return;
		}
		BuildRibbonCrossSection(boxwidth);
		segments = (int)(splines[0].length * TubeLength / (stepdist * 0.1f));
		Twist = TwistPerUnit;
		float num = startAng * ((float)Math.PI / 180f);
		int num2 = (segments + 1) * (ribsegs + 1) * strands;
		int num3 = ribsegs * 2 * segments * strands;
		float num4 = tradius * 0.5f + offset;
		if (strands == 1)
		{
			num4 = offset;
		}
		if (tverts == null || tverts.Length != num2)
		{
			tverts = new Vector3[num2];
		}
		if (GenUV && (tuvs == null || tuvs.Length != num2))
		{
			tuvs = new Vector2[num2];
		}
		if (ttris == null || ttris.Length != num3 * 3)
		{
			ttris = new int[num3 * 3];
		}
		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;
		switch (axis)
		{
		case MegaAxis.X:
			MegaMatrix.RotateY(ref tm, -(float)Math.PI / 2f);
			break;
		case MegaAxis.Y:
			MegaMatrix.RotateX(ref tm, -(float)Math.PI / 2f);
			break;
		}
		MegaMatrix.SetTrans(ref tm, Pivot);
		switch (RopeUp)
		{
		case MegaAxis.X:
			ropeup = Vector3.right;
			break;
		case MegaAxis.Y:
			ropeup = Vector3.up;
			break;
		case MegaAxis.Z:
			ropeup = Vector3.forward;
			break;
		}
		int num5 = 0;
		int num6 = 0;
		Vector2 zero = Vector2.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 one = Vector3.one;
		for (int i = 0; i < strands; i++)
		{
			float num7 = (float)i / (float)strands * (float)Math.PI * 2f;
			zero2.x = Mathf.Sin(num7) * num4;
			zero2.z = Mathf.Cos(num7) * num4;
			int num8 = num5;
			num8 = num5;
			for (int j = 0; j <= segments; j++)
			{
				float num9 = TubeStart + (float)j / (float)segments * TubeLength;
				wtm = GetDeformMat(num9);
				float num10 = num9 * uvtwist;
				float num11 = num + (num9 - TubeStart) * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num11) * num4;
				zero2.z = Mathf.Cos(num7 + num11) * num4;
				one.x = scaleX.Evaluate(num9);
				float num12 = cross.Length - 1;
				for (int k = 0; k < cross.Length; k++)
				{
					Vector3 vector = cross[k];
					vector.x *= one.x;
					Vector3 v = tm.MultiplyPoint3x4(vector + zero2);
					ref Vector3 reference = ref tverts[num5];
					reference = wtm.MultiplyPoint3x4(v);
					if (GenUV)
					{
						zero.y = (num9 - TubeStart) * splines[0].length * uvtiley + UVOffset.y;
						zero.x = (float)k / num12 * uvtilex + num10 + UVOffset.x;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
			}
			if (!GenUV)
			{
				continue;
			}
			int num13 = ribsegs + 1;
			for (int l = 0; l < segments; l++)
			{
				for (int m = 0; m < cross.Length - 1; m++)
				{
					ttris[num6++] = (l + 1) * num13 + m + num8;
					ttris[num6++] = (l + 1) * num13 + (m + 1) % num13 + num8;
					ttris[num6++] = l * num13 + m + num8;
					ttris[num6++] = (l + 1) * num13 + (m + 1) % num13 + num8;
					ttris[num6++] = l * num13 + (m + 1) % num13 + num8;
					ttris[num6++] = l * num13 + m + num8;
				}
			}
		}
		if (conform)
		{
			CalcBounds(tverts);
			DoConform(tverts);
		}
		shapemesh.Clear();
		shapemesh.subMeshCount = 1;
		shapemesh.vertices = tverts;
		shapemesh.triangles = ttris;
		if (GenUV)
		{
			shapemesh.uv = tuvs;
		}
		shapemesh.RecalculateBounds();
		shapemesh.RecalculateNormals();
		if (CalcTangents)
		{
			MegaUtils.BuildTangents(shapemesh);
		}
	}

	private void BuildBoxMesh()
	{
		TubeLength = Mathf.Clamp01(TubeLength);
		if (TubeLength == 0f || strands < 1)
		{
			shapemesh.Clear();
			return;
		}
		BuildBoxCrossSection(boxwidth, boxheight);
		segments = (int)(splines[0].length * TubeLength / (stepdist * 0.1f));
		Twist = TwistPerUnit;
		float num = startAng * ((float)Math.PI / 180f);
		int num2 = 9 * (segments + 1) * strands;
		int num3 = 8 * segments * strands;
		float num4 = tradius * 0.5f + offset;
		if (strands == 1)
		{
			num4 = offset;
		}
		if (cap)
		{
			num2 += 8 * strands;
			num3 += 4 * strands;
		}
		if (tverts == null || tverts.Length != num2)
		{
			tverts = new Vector3[num2];
		}
		bool flag = false;
		if (GenUV && (tuvs == null || tuvs.Length != num2))
		{
			tuvs = new Vector2[num2];
			flag = true;
		}
		if (ttris == null || ttris.Length != num3 * 3)
		{
			ttris = new int[num3 * 3];
		}
		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;
		switch (axis)
		{
		case MegaAxis.X:
			MegaMatrix.RotateY(ref tm, -(float)Math.PI / 2f);
			break;
		case MegaAxis.Y:
			MegaMatrix.RotateX(ref tm, -(float)Math.PI / 2f);
			break;
		}
		switch (RopeUp)
		{
		case MegaAxis.X:
			ropeup = Vector3.right;
			break;
		case MegaAxis.Y:
			ropeup = Vector3.up;
			break;
		case MegaAxis.Z:
			ropeup = Vector3.forward;
			break;
		}
		MegaMatrix.SetTrans(ref tm, Pivot);
		int num5 = 0;
		int num6 = 0;
		Vector2 zero = Vector2.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 one = Vector3.one;
		for (int i = 0; i < strands; i++)
		{
			float num7 = (float)i / (float)strands * (float)Math.PI * 2f;
			zero2.x = Mathf.Sin(num7) * num4;
			zero2.z = Mathf.Cos(num7) * num4;
			int num8 = num5;
			if (cap)
			{
				float tubeStart = TubeStart;
				wtm = GetDeformMat(tubeStart);
				float num9 = num + 0f * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num9) * num4;
				zero2.z = Mathf.Cos(num7 + num9) * num4;
				one.x = scaleX.Evaluate(tubeStart);
				if (unlinkScale)
				{
					one.z = scaleY.Evaluate(tubeStart);
				}
				else
				{
					one.z = one.x;
				}
				for (int j = 0; j < 4; j++)
				{
					Vector3 vector = cross[j * 2];
					vector.x *= one.x;
					vector.z *= one.z;
					Vector3 v = tm.MultiplyPoint3x4(vector + zero2);
					ref Vector3 reference = ref tverts[num5];
					reference = wtm.MultiplyPoint3x4(v);
					if (flag)
					{
						zero.y = 0f;
						zero.x = 0f;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
				ttris[num6++] = num8;
				ttris[num6++] = num8 + 2;
				ttris[num6++] = num8 + 1;
				ttris[num6++] = num8;
				ttris[num6++] = num8 + 3;
				ttris[num6++] = num8 + 2;
				num8 = num5;
				tubeStart = TubeStart + TubeLength;
				wtm = GetDeformMat(tubeStart);
				num9 = num + TubeLength * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num9) * num4;
				zero2.z = Mathf.Cos(num7 + num9) * num4;
				one.x = scaleX.Evaluate(tubeStart);
				if (unlinkScale)
				{
					one.z = scaleY.Evaluate(tubeStart);
				}
				else
				{
					one.z = one.x;
				}
				for (int k = 0; k < 4; k++)
				{
					Vector3 vector2 = cross[k * 2];
					vector2.x *= one.x;
					vector2.z *= one.z;
					Vector3 v2 = tm.MultiplyPoint3x4(vector2 + zero2);
					ref Vector3 reference2 = ref tverts[num5];
					reference2 = wtm.MultiplyPoint3x4(v2);
					if (GenUV)
					{
						zero.y = 0f;
						zero.x = 0f;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
				ttris[num6++] = num8;
				ttris[num6++] = num8 + 1;
				ttris[num6++] = num8 + 2;
				ttris[num6++] = num8;
				ttris[num6++] = num8 + 2;
				ttris[num6++] = num8 + 3;
			}
			num8 = num5;
			for (int l = 0; l <= segments; l++)
			{
				float num10 = TubeStart + (float)l / (float)segments * TubeLength;
				wtm = GetDeformMat(num10);
				float num11 = num10 * uvtwist;
				float num12 = num + (num10 - TubeStart) * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num12) * num4;
				zero2.z = Mathf.Cos(num7 + num12) * num4;
				one.x = scaleX.Evaluate(num10);
				if (unlinkScale)
				{
					one.z = scaleY.Evaluate(num10);
				}
				else
				{
					one.z = one.x;
				}
				for (int m = 0; m < cross.Length; m++)
				{
					Vector3 vector3 = cross[m];
					vector3.x *= one.x;
					vector3.z *= one.z;
					Vector3 v3 = tm.MultiplyPoint3x4(vector3 + zero2);
					ref Vector3 reference3 = ref tverts[num5];
					reference3 = wtm.MultiplyPoint3x4(v3);
					if (GenUV)
					{
						zero.y = (num10 - TubeStart) * splines[0].length * uvtiley + UVOffset.y;
						zero.x = boxuv[m] * uvtilex + num11 + UVOffset.x;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
			}
			if (!GenUV)
			{
				continue;
			}
			int num13 = 8;
			for (int n = 0; n < segments; n++)
			{
				for (int num14 = 0; num14 < 4; num14++)
				{
					int num15 = num14 * 2;
					ttris[num6++] = n * num13 + num15 + num8;
					ttris[num6++] = (n + 1) * num13 + (num15 + 1) + num8;
					ttris[num6++] = (n + 1) * num13 + num15 + num8;
					ttris[num6++] = n * num13 + num15 + num8;
					ttris[num6++] = n * num13 + (num15 + 1) + num8;
					ttris[num6++] = (n + 1) * num13 + (num15 + 1) + num8;
				}
			}
		}
		if (conform)
		{
			CalcBounds(tverts);
			DoConform(tverts);
		}
		shapemesh.Clear();
		shapemesh.subMeshCount = 1;
		shapemesh.vertices = tverts;
		shapemesh.triangles = ttris;
		if (GenUV)
		{
			shapemesh.uv = tuvs;
		}
		shapemesh.RecalculateBounds();
		shapemesh.RecalculateNormals();
		if (CalcTangents)
		{
			MegaUtils.BuildTangents(shapemesh);
		}
	}

	private void BuildMultiStrandMesh()
	{
		TubeLength = Mathf.Clamp01(TubeLength);
		if (TubeLength == 0f || strands < 1)
		{
			shapemesh.Clear();
			return;
		}
		Twist = TwistPerUnit;
		segments = (int)(splines[selcurve].length * TubeLength / (stepdist * 0.1f));
		float num = startAng * ((float)Math.PI / 180f);
		float num2 = tradius * 0.5f + offset;
		if (strands == 1)
		{
			num2 = offset;
		}
		float rad = tradius * 0.5f + strandRadius;
		BuildCrossSection(rad);
		int num3 = (segments + 1) * (tsides + 1) * strands;
		int num4 = tsides * 2 * segments * strands;
		if (cap)
		{
			num3 += (tsides + 1) * 2 * strands;
			num4 += tsides * 2 * strands;
		}
		if (tverts == null || tverts.Length != num3)
		{
			tverts = new Vector3[num3];
		}
		bool flag = false;
		if (GenUV && (tuvs == null || tuvs.Length != num3))
		{
			tuvs = new Vector2[num3];
			flag = true;
		}
		if (ttris == null || ttris.Length != num4 * 3)
		{
			ttris = new int[num4 * 3];
		}
		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;
		switch (axis)
		{
		case MegaAxis.X:
			MegaMatrix.RotateY(ref tm, -(float)Math.PI / 2f);
			break;
		case MegaAxis.Y:
			MegaMatrix.RotateX(ref tm, -(float)Math.PI / 2f);
			break;
		}
		MegaMatrix.SetTrans(ref tm, Pivot);
		switch (RopeUp)
		{
		case MegaAxis.X:
			ropeup = Vector3.right;
			break;
		case MegaAxis.Y:
			ropeup = Vector3.up;
			break;
		case MegaAxis.Z:
			ropeup = Vector3.forward;
			break;
		}
		int num5 = 0;
		int num6 = 0;
		Vector2 zero = Vector2.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 one = Vector3.one;
		for (int i = 0; i < strands; i++)
		{
			float num7 = (float)i / (float)strands * (float)Math.PI * 2f;
			zero2.x = Mathf.Sin(num7) * num2;
			zero2.z = Mathf.Cos(num7) * num2;
			int num8 = num5;
			if (cap)
			{
				float tubeStart = TubeStart;
				wtm = GetDeformMat(tubeStart);
				float num9 = num + (tubeStart - TubeStart) * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num9) * num2;
				zero2.z = Mathf.Cos(num7 + num9) * num2;
				one.x = scaleX.Evaluate(tubeStart);
				if (unlinkScale)
				{
					one.z = scaleY.Evaluate(tubeStart);
				}
				else
				{
					one.z = one.x;
				}
				for (int j = 0; j <= cross.Length; j++)
				{
					Vector3 vector = cross[j % cross.Length];
					vector.x *= one.x;
					vector.z *= one.z;
					Vector3 v = tm.MultiplyPoint3x4(vector + zero2);
					ref Vector3 reference = ref tverts[num5];
					reference = wtm.MultiplyPoint3x4(v);
					if (flag)
					{
						zero.y = 0f;
						zero.x = 0f;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
				if (GenUV)
				{
					for (int k = 1; k < tsides; k++)
					{
						ttris[num6++] = num8;
						ttris[num6++] = num8 + k + 1;
						ttris[num6++] = num8 + k;
					}
				}
				num8 = num5;
				tubeStart = TubeStart + TubeLength;
				wtm = GetDeformMat(tubeStart);
				num9 = num + (tubeStart - TubeStart) * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num9) * num2;
				zero2.z = Mathf.Cos(num7 + num9) * num2;
				one.x = scaleX.Evaluate(tubeStart);
				if (unlinkScale)
				{
					one.z = scaleY.Evaluate(tubeStart);
				}
				else
				{
					one.z = one.x;
				}
				for (int l = 0; l <= cross.Length; l++)
				{
					Vector3 vector2 = cross[l % cross.Length];
					vector2.x *= one.x;
					vector2.z *= one.z;
					Vector3 v2 = tm.MultiplyPoint3x4(vector2 + zero2);
					ref Vector3 reference2 = ref tverts[num5];
					reference2 = wtm.MultiplyPoint3x4(v2);
					if (GenUV)
					{
						zero.y = 0f;
						zero.x = 0f;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
				if (GenUV)
				{
					for (int m = 1; m < tsides; m++)
					{
						ttris[num6++] = num8;
						ttris[num6++] = num8 + m;
						ttris[num6++] = num8 + m + 1;
					}
				}
			}
			num8 = num5;
			for (int n = 0; n <= segments; n++)
			{
				float num10 = TubeStart + (float)n / (float)segments * TubeLength;
				one.x = scaleX.Evaluate(num10);
				if (unlinkScale)
				{
					one.z = scaleY.Evaluate(num10);
				}
				else
				{
					one.z = one.x;
				}
				wtm = GetDeformMat(num10);
				float num11 = num10 * uvtwist;
				float num12 = num + (num10 - TubeStart) * Twist * (float)Math.PI * 2f;
				zero2.x = Mathf.Sin(num7 + num12) * num2;
				zero2.z = Mathf.Cos(num7 + num12) * num2;
				for (int num13 = 0; num13 <= cross.Length; num13++)
				{
					Vector3 vector3 = cross[num13 % cross.Length];
					vector3.x *= one.x;
					vector3.z *= one.z;
					Vector3 v3 = tm.MultiplyPoint3x4(vector3 + zero2);
					ref Vector3 reference3 = ref tverts[num5];
					reference3 = wtm.MultiplyPoint3x4(v3);
					if (GenUV)
					{
						zero.y = (num10 - TubeStart) * splines[0].length * uvtiley + UVOffset.y;
						zero.x = (float)num13 / (float)cross.Length * uvtilex + num11 + UVOffset.x;
						tuvs[num5++] = zero;
					}
					else
					{
						num5++;
					}
				}
			}
			if (!GenUV)
			{
				continue;
			}
			int num14 = tsides + 1;
			for (int num15 = 0; num15 < segments; num15++)
			{
				for (int num16 = 0; num16 < cross.Length; num16++)
				{
					ttris[num6++] = num15 * num14 + num16 + num8;
					ttris[num6++] = (num15 + 1) * num14 + (num16 + 1) % num14 + num8;
					ttris[num6++] = (num15 + 1) * num14 + num16 + num8;
					ttris[num6++] = num15 * num14 + num16 + num8;
					ttris[num6++] = num15 * num14 + (num16 + 1) % num14 + num8;
					ttris[num6++] = (num15 + 1) * num14 + (num16 + 1) % num14 + num8;
				}
			}
		}
		if (conform)
		{
			CalcBounds(tverts);
			DoConform(tverts);
		}
		shapemesh.Clear();
		shapemesh.subMeshCount = 1;
		shapemesh.vertices = tverts;
		shapemesh.triangles = ttris;
		if (GenUV)
		{
			shapemesh.uv = tuvs;
		}
		shapemesh.RecalculateBounds();
		shapemesh.RecalculateNormals();
		if (CalcTangents)
		{
			MegaUtils.BuildTangents(shapemesh);
		}
	}

	public void ClearMesh()
	{
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		if (component != null)
		{
			component.sharedMesh = null;
			shapemesh = null;
		}
	}

	public void SetMats()
	{
		MeshRenderer meshRenderer = base.gameObject.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (meshType == MeshShapeType.Fill)
		{
			meshRenderer.sharedMaterials = new Material[3] { mat1, mat2, mat3 };
		}
		else
		{
			meshRenderer.sharedMaterials = new Material[1] { mat1 };
		}
	}

	public void BuildMesh()
	{
		if (!makeMesh)
		{
			return;
		}
		if (shapemesh == null)
		{
			MeshFilter meshFilter = base.gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.sharedMesh = new Mesh();
			if (base.gameObject.GetComponent<MeshRenderer>() == null)
			{
				base.gameObject.AddComponent<MeshRenderer>();
			}
			SetMats();
			shapemesh = meshFilter.sharedMesh;
		}
		if (meshType == MeshShapeType.Tube)
		{
			BuildTubeMesh();
			return;
		}
		if (meshType == MeshShapeType.Box)
		{
			BuildBoxMesh();
			return;
		}
		if (meshType == MeshShapeType.Ribbon)
		{
			BuildRibbonMesh();
			return;
		}
		float num = stepdist * 0.1f;
		if (splines[selcurve].length / num > 1500f)
		{
			num = splines[selcurve].length / 1500f;
		}
		Vector3 size = Vector3.zero;
		verts.Clear();
		uvs.Clear();
		tris.Clear();
		tris1.Clear();
		tris2.Clear();
		tris = MegaTriangulator.Triangulate(this, splines[selcurve], num, ref verts, ref uvs, ref tris, Pivot, ref size);
		if (axis != MegaAxis.Y)
		{
			for (int i = 0; i < tris.Count; i += 3)
			{
				int value = tris[i];
				tris[i] = tris[i + 2];
				tris[i + 2] = value;
			}
		}
		int count = verts.Count;
		int count2 = tris.Count;
		if (Height < 0f)
		{
			Height = 0f;
		}
		float height = Height;
		Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate.x, UVRotate.y, 0f), new Vector3(UVScale.x, 1f, UVScale.y));
		if (GenUV)
		{
			uvs.Clear();
			Vector2 zero = Vector2.zero;
			Vector3 v = Vector3.zero;
			int index = 0;
			int index2 = 2;
			switch (axis)
			{
			case MegaAxis.X:
				index = 1;
				break;
			case MegaAxis.Z:
				index2 = 1;
				break;
			}
			for (int j = 0; j < verts.Count; j++)
			{
				v.x = verts[j][index];
				v.z = verts[j][index2];
				if (!PhysUV)
				{
					v.x /= size[index];
					v.z /= size[index2];
				}
				v = matrix4x.MultiplyPoint3x4(v);
				zero.x = v.x + UVOffset.x;
				zero.y = v.z + UVOffset.y;
				uvs.Add(zero);
			}
		}
		if (DoubleSided && height != 0f)
		{
			for (int k = 0; k < count; k++)
			{
				Vector3 item = verts[k];
				if (UseHeightCurve)
				{
					float num2 = MegaTriangulator.m_points[k].z / splines[selcurve].length;
					int index4;
					int index3 = (index4 = (int)axis);
					float num3 = item[index4];
					item[index3] = num3 - height * heightCrv.Evaluate(num2 + heightOff);
				}
				else
				{
					int index6;
					int index5 = (index6 = (int)axis);
					float num4 = item[index6];
					item[index5] = num4 - height;
				}
				verts.Add(item);
				uvs.Add(uvs[k]);
			}
			for (int num5 = count2 - 1; num5 >= 0; num5--)
			{
				tris1.Add(tris[num5] + count);
			}
		}
		if (height != 0f)
		{
			int count3 = verts.Count;
			Vector3 zero2 = Vector3.zero;
			Vector2 zero3 = Vector2.zero;
			matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate1.x, UVRotate1.y, 0f), new Vector3(UVScale1.x, 1f, UVScale1.y));
			for (int l = 0; l < MegaTriangulator.m_points.Count; l++)
			{
				zero2 = verts[l];
				verts.Add(zero2);
				zero2.x = MegaTriangulator.m_points[l].z;
				if (!PhysUV)
				{
					zero2.x /= size.x;
				}
				zero2.y = 0f;
				zero2.z = 0f;
				zero2 = matrix4x.MultiplyPoint3x4(zero2);
				zero3.x = zero2.x + UVOffset1.x;
				zero3.y = zero2.z + UVOffset1.y;
				uvs.Add(zero3);
			}
			zero2 = verts[0];
			verts.Add(zero2);
			zero3.x = splines[selcurve].length * UVScale1.x + UVOffset1.x;
			if (!PhysUV)
			{
				zero3.x /= size.x;
			}
			zero3.y = 0f + UVOffset1.y;
			uvs.Add(zero3);
			float num6 = 1f;
			int index8;
			float num8;
			for (int m = 0; m < MegaTriangulator.m_points.Count; m++)
			{
				float num7 = MegaTriangulator.m_points[m].z / splines[selcurve].length;
				zero2 = verts[m];
				if (UseHeightCurve)
				{
					num6 = heightCrv.Evaluate(num7 + heightOff);
				}
				int index7 = (index8 = (int)axis);
				num8 = zero2[index8];
				zero2[index7] = num8 - height * num6;
				verts.Add(zero2);
				zero2.x = MegaTriangulator.m_points[m].z;
				zero2.z = zero2.y;
				zero2.y = 0f;
				if (!PhysUV)
				{
					zero2.x /= size.x;
					zero2.z /= height * num6;
				}
				zero2 = matrix4x.MultiplyPoint3x4(zero2);
				zero3.x = zero2.x + UVOffset1.x;
				zero3.y = zero2.z + UVOffset1.y;
				uvs.Add(zero3);
			}
			zero2 = verts[0];
			if (UseHeightCurve)
			{
				num6 = heightCrv.Evaluate(0f + heightOff);
			}
			int index9 = (index8 = (int)axis);
			num8 = zero2[index8];
			zero2[index9] = num8 - height * num6;
			verts.Add(zero2);
			zero2.x = MegaTriangulator.m_points[0].z;
			zero2.z = zero2.y;
			zero2.y = 0f;
			if (!PhysUV)
			{
				zero2.x /= size.x;
				zero2.z /= height * num6;
			}
			zero2 = matrix4x.MultiplyPoint3x4(zero2);
			zero3.x = zero2.x + UVOffset1.x;
			zero3.y = zero2.z + UVOffset1.y;
			uvs.Add(zero3);
			int num9 = MegaTriangulator.m_points.Count + 1;
			int num10 = 0;
			if (splines[selcurve].reverse)
			{
				for (num10 = 0; num10 < MegaTriangulator.m_points.Count; num10++)
				{
					tris2.Add(num10 + count3 + 1);
					tris2.Add(num10 + count3 + num9);
					tris2.Add(num10 + count3);
					tris2.Add(num10 + count3 + num9 + 1);
					tris2.Add(num10 + count3 + num9);
					tris2.Add(num10 + count3 + 1);
				}
			}
			else
			{
				for (num10 = 0; num10 < MegaTriangulator.m_points.Count; num10++)
				{
					tris2.Add(num10 + count3);
					tris2.Add(num10 + count3 + num9);
					tris2.Add(num10 + count3 + 1);
					tris2.Add(num10 + count3 + 1);
					tris2.Add(num10 + count3 + num9);
					tris2.Add(num10 + count3 + num9 + 1);
				}
			}
		}
		Vector3[] vertices = verts.ToArray();
		if (conform)
		{
			CalcBounds(vertices);
			DoConform(vertices);
		}
		shapemesh.Clear();
		shapemesh.vertices = vertices;
		shapemesh.uv = uvs.ToArray();
		shapemesh.subMeshCount = 3;
		shapemesh.SetTriangles(tris.ToArray(), 0);
		shapemesh.SetTriangles(tris1.ToArray(), 1);
		shapemesh.SetTriangles(tris2.ToArray(), 2);
		shapemesh.RecalculateNormals();
		shapemesh.RecalculateBounds();
		if (CalcTangents)
		{
			MegaUtils.BuildTangents(shapemesh);
		}
	}

	public static float CurveLength(MegaSpline spline, int knot, float v1, float v2, float size)
	{
		float num = 0f;
		if (size == 0f)
		{
			Vector3 vector = spline.InterpBezier3D(knot, v1);
			float num2 = (v2 - v1) / (float)CURVELENGTHSTEPS;
			int num3 = 1;
			float num4 = num2;
			while (num3 < CURVELENGTHSTEPS)
			{
				Vector3 vector2 = spline.InterpBezier3D(knot, v1 + num4);
				num += Vector3.Magnitude(vector2 - vector);
				vector = vector2;
				num3++;
				num4 += num2;
			}
			return num + Vector3.Magnitude(spline.InterpBezier3D(knot, v2) - vector);
		}
		int count = spline.knots.Count;
		int num5 = (knot + count - 1) % count;
		int num6 = (knot + 1) % count;
		float num7 = v1 - 0.01f;
		int knot2 = knot;
		if (num7 < 0f)
		{
			if (spline.closed)
			{
				num7 += 1f;
				knot2 = num5;
			}
			else
			{
				num7 = 0f;
			}
		}
		float a = v1 + 0.01f;
		Vector3 vector3 = Vector3.Normalize(spline.InterpBezier3D(knot, a) - spline.InterpBezier3D(knot2, num7));
		vector3.y = 0f;
		Vector3 vector4 = new Vector3(vector3.z * size, 0f, (0f - vector3.x) * size);
		Vector3 vector5 = spline.InterpBezier3D(knot, v1) + vector4;
		float num8 = (v2 - v1) / (float)CURVELENGTHSTEPS;
		int num9 = 1;
		float num10 = num8;
		while (num9 < CURVELENGTHSTEPS)
		{
			num7 = v1 + num10 - 0.01f;
			a = v1 + num10 + 0.01f;
			vector3 = Vector3.Normalize(spline.InterpBezier3D(knot, a) - spline.InterpBezier3D(knot, num7));
			vector3.y = 0f;
			vector4 = new Vector3(vector3.z * size, 0f, (0f - vector3.x) * size);
			Vector3 vector6 = spline.InterpBezier3D(knot, v1 + num10) + vector4;
			num += Vector3.Magnitude(vector6 - vector5);
			vector5 = vector6;
			num9++;
			num10 += num8;
		}
		num7 = v2 - 0.01f;
		int knot3 = knot;
		a = v2 + 0.01f;
		if (a > 1f)
		{
			if (spline.closed)
			{
				a -= 1f;
				knot3 = num6;
			}
			else
			{
				a = 1f;
			}
		}
		vector3 = Vector3.Normalize(spline.InterpBezier3D(knot3, a) - spline.InterpBezier3D(knot, num7));
		vector3.y = 0f;
		vector4 = new Vector3(vector3.z * size, 0f, (0f - vector3.x) * size);
		return num + Vector3.Magnitude(spline.InterpBezier3D(knot, v2) + vector4 - vector5);
	}

	public void OutlineSpline(MegaShape shape, int poly, float size, bool centered)
	{
		MegaSpline inSpline = shape.splines[poly];
		MegaSpline megaSpline = new MegaSpline();
		OutlineSpline(inSpline, megaSpline, size, centered);
		shape.splines.Add(megaSpline);
		megaSpline.CalcLength();
	}

	public void OutlineSpline(MegaSpline inSpline, MegaSpline outSpline, float size, bool centered)
	{
		float num = ((!centered) ? 0f : (size / 2f));
		int count = inSpline.knots.Count;
		outSpline.knots.Clear();
		if (inSpline.closed)
		{
			for (int i = 0; i < count; i++)
			{
				int knot = (i + count - 1) % count;
				float num2 = CurveLength(inSpline, knot, 0.5f, 1f, 0f);
				float num3 = CurveLength(inSpline, i, 0f, 0.5f, 0f);
				Vector3 p = inSpline.knots[i].p;
				Vector3 vector = Vector3.Normalize(inSpline.InterpBezier3D(knot, 0.99f) - p);
				Vector3 vector2 = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - p);
				Vector3 vector3 = Vector3.Normalize(vector2 - vector);
				vector3.y = 0f;
				float num4 = Vector3.Dot(vector, vector2);
				float f = ((!(num4 >= -0.9999939f)) ? ((float)Math.PI / 2f) : ((0f - Mathf.Acos(num4)) / 2f));
				float num5 = num / Mathf.Tan(f);
				float num6 = ((!(num < 0f)) ? 1f : (-1f));
				float num7 = Mathf.Sqrt(num5 * num5 + num * num) * num6;
				Vector3 vector4 = new Vector3(vector3.z * num7, 0f, (0f - vector3.x) * num7);
				float num8 = CurveLength(inSpline, knot, 0.5f, 1f, num);
				float num9 = CurveLength(inSpline, i, 0f, 0.5f, num);
				Vector3 vector5 = p + vector4;
				float num10 = num8 / num2;
				float num11 = num9 / num3;
				outSpline.AddKnot(vector5, vector5 + (inSpline.knots[i].invec - p) * num10, vector5 + (inSpline.knots[i].outvec - p) * num11);
			}
			outSpline.closed = true;
			return;
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 p2 = inSpline.knots[j].p;
			float num12 = ((j != 0) ? CurveLength(inSpline, j - 1, 0.5f, 1f, 0f) : 1f);
			float num13 = ((j != count - 1) ? CurveLength(inSpline, j, 0f, 0.5f, 0f) : 1f);
			float num14 = 0f;
			Vector3 vector6;
			if (j == 0)
			{
				vector6 = Vector3.Normalize(inSpline.InterpBezier3D(j, 0.01f) - p2);
				num14 = num;
			}
			else if (j == count - 1)
			{
				vector6 = Vector3.Normalize(p2 - inSpline.InterpBezier3D(j - 1, 0.99f));
				num14 = num;
			}
			else
			{
				Vector3 vector7 = Vector3.Normalize(inSpline.InterpBezier3D(j - 1, 0.99f) - p2);
				Vector3 vector8 = Vector3.Normalize(inSpline.InterpBezier3D(j, 0.01f) - p2);
				vector6 = Vector3.Normalize(vector8 - vector7);
				float num15 = Vector3.Dot(vector7, vector8);
				if (num15 >= -0.9999939f)
				{
					float f2 = (0f - Mathf.Acos(num15)) / 2f;
					float num16 = num / Mathf.Tan(f2);
					float num17 = ((!(num < 0f)) ? 1f : (-1f));
					num14 = Mathf.Sqrt(num16 * num16 + num * num) * num17;
				}
				else
				{
					num14 = num;
				}
			}
			vector6.y = 0f;
			Vector3 vector9 = new Vector3(vector6.z * num14, 0f, (0f - vector6.x) * num14);
			float num18 = ((j != 0) ? CurveLength(inSpline, j - 1, 0.5f, 1f, num) : 1f);
			float num19 = ((j != count - 1) ? CurveLength(inSpline, j, 0f, 0.5f, num) : 1f);
			float num20 = num18 / num12;
			float num21 = num19 / num13;
			Vector3 vector10 = p2 + vector9;
			outSpline.AddKnot(vector10, vector10 + (inSpline.knots[j].invec - p2) * num20, vector10 + (inSpline.knots[j].outvec - p2) * num21);
		}
		outSpline.closed = false;
	}

	public void SetTarget(GameObject targ)
	{
		target = targ;
		if ((bool)target)
		{
			conformCollider = target.collider;
		}
	}

	private void CalcBounds(Vector3[] verts)
	{
		minz = verts[0].y;
		for (int i = 1; i < verts.Length; i++)
		{
			if (verts[i].y < minz)
			{
				minz = verts[i].y;
			}
		}
	}

	public void InitConform(Vector3[] verts)
	{
		if (offsets == null || offsets.Length != verts.Length)
		{
			offsets = new float[verts.Length];
			last = new float[verts.Length];
			for (int i = 0; i < verts.Length; i++)
			{
				offsets[i] = verts[i].y - minz;
			}
		}
		if ((bool)target)
		{
			conformCollider = target.collider;
		}
	}

	private void DoConform(Vector3[] verts)
	{
		InitConform(verts);
		if (!target || !conformCollider)
		{
			return;
		}
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		Matrix4x4 inverse = localToWorldMatrix.inverse;
		Ray ray = default(Ray);
		float num = conformAmount;
		for (int i = 0; i < verts.Length; i++)
		{
			Vector3 origin = localToWorldMatrix.MultiplyPoint(verts[i]);
			origin.y += raystartoff;
			ray.origin = origin;
			ray.direction = Vector3.down;
			if (conformCollider.Raycast(ray, out var hitInfo, raydist))
			{
				Vector3 vector = inverse.MultiplyPoint(hitInfo.point);
				verts[i].y = Mathf.Lerp(verts[i].y, vector.y + offsets[i] + conformOffset, num);
				last[i] = verts[i].y;
			}
			else
			{
				Vector3 origin2 = ray.origin;
				origin2.y -= raydist;
				verts[i].y = last[i];
			}
		}
	}

	private void ConformTarget()
	{
	}
}
