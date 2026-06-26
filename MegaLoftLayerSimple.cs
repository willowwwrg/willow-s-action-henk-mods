using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerSimple : MegaLoftLayerBase
{
	public int svert;

	public int evert;

	public bool usemain;

	public float pathStart;

	public float pathLength = 1f;

	public float pathDist = 0.5f;

	public float crossStart;

	public float crossEnd = 1f;

	public float crossDist = 0.5f;

	public Vector3 crossScale = Vector3.one;

	public Vector3 pivot = Vector3.zero;

	public bool useCrossScaleCrv;

	public AnimationCurve crossScaleCrv = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool useCrossScaleCrvY;

	public AnimationCurve crossScaleCrvY = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 crossRot = new Vector3(90f, 0f, 0f);

	public Vector3 offset = Vector3.zero;

	public Vector2 UVOffset = Vector2.zero;

	public Vector2 UVRotate = Vector2.zero;

	public Vector2 UVScale = Vector2.one;

	public bool includeknots = true;

	public bool swapuv = true;

	public bool physuv = true;

	public bool uvcalcy;

	public bool planarMapping;

	public MegaAxis planarAxis;

	public MegaPlanarMode planarMode;

	public bool lockWorld;

	public Matrix4x4 lockedTM = Matrix4x4.identity;

	public bool sideViewUV;

	public MegaAxis sideViewAxis;

	public bool flip;

	public bool snapBottom;

	public float Bottom;

	public bool snapTop;

	public float Top;

	public bool clipBottom;

	public float clipBottomVal;

	public bool clipTop;

	public float clipTopVal;

	public bool showuvparams = true;

	public bool showcrossparams;

	public bool showadvancedparams;

	public Matrix4x4 uvtm = Matrix4x4.identity;

	public Vector3[] crossverts;

	public Vector2[] crossuvs;

	public Vector3[] crossnorms;

	public Vector3 crosssize = Vector3.zero;

	public Vector3 crossmin = Vector3.zero;

	public Vector3 crossmax = Vector3.zero;

	public int numverts;

	public int numtris;

	public int crosses;

	public MegaLoftUVOrigin UVOrigin;

	public List<Vector3> verts = new List<Vector3>();

	public List<Vector3> norms = new List<Vector3>();

	public List<Vector2> uvs = new List<Vector2>();

	public List<Color> cols = new List<Color>();

	public float scaleoff;

	public float scaleoffY;

	public bool sepscale;

	public int curve;

	public int crosscurve;

	public bool showCapParams;

	public bool capStart;

	public bool capEnd;

	public Material capStartMat;

	public Material capEndMat;

	public Vector3[] capStartVerts;

	public Vector2[] capStartUVS;

	public int[] capStartTris;

	public Color[] capStartCols;

	public Vector3[] capEndVerts;

	public Vector2[] capEndUVS;

	public int[] capEndTris;

	public Color[] capEndCols;

	public bool snap = true;

	public Vector2 capStartUVScale = Vector2.one;

	public Vector2 capStartUVOffset = Vector2.zero;

	public float capStartUVRot;

	public bool capStartPhysUV;

	public Vector2 capEndUVScale = Vector2.one;

	public Vector2 capEndUVOffset = Vector2.zero;

	public float capEndUVRot;

	public bool capEndPhysUV;

	public List<int> capfaces = new List<int>();

	public bool capflip;

	public bool useOffsetX;

	public AnimationCurve offsetCrvX = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool useOffsetY;

	public AnimationCurve offsetCrvY = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool useOffsetZ;

	public AnimationCurve offsetCrvZ = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float LoftLength;

	public bool useTwistCrv;

	public float twistAmt = 90f;

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Color color = Color.white;

	public AnimationCurve colR = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve colG = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve colB = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve colA = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float alignCross;

	public bool conform;

	public Vector3 direction = Vector3.down;

	public GameObject target;

	public float[] offsets;

	public float[] capstartoffsets;

	public float[] capendoffsets;

	public Collider conformCollider;

	public Bounds bounds;

	public float[] last;

	public float[] capstartlast;

	public float[] capendlast;

	public Vector3[] loftverts1;

	public float raystartoff;

	public float conformOffset;

	public float raydist = 100f;

	public bool showConformParams;

	public float conformAmount = 1f;

	public float conminz;

	private List<int> addsections = new List<int>();

	public virtual void Notify(MegaSpline spline, int reason)
	{
		if (layerPath.splines[curve] == spline || layerSection.splines[crosscurve] == spline)
		{
			MegaShapeLoft component = GetComponent<MegaShapeLoft>();
			component.rebuild = true;
			component.BuildMeshFromLayersNew();
		}
	}

	public virtual int GetHelp()
	{
		return 2098;
	}

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=" + GetHelp());
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
		offset += loftverts.Length;
		if (capStart)
		{
			Array.Copy(capStartVerts, 0, verts, offset, capStartVerts.Length);
			Array.Copy(capStartUVS, 0, uvs, offset, capStartUVS.Length);
			offset += capStartVerts.Length;
		}
		if (capEnd)
		{
			Array.Copy(capEndVerts, 0, verts, offset, capEndVerts.Length);
			Array.Copy(capEndUVS, 0, uvs, offset, capEndUVS.Length);
			offset += capEndVerts.Length;
		}
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
		Array.Copy(loftcols, 0, cols, offset, loftcols.Length);
		offset += loftverts.Length;
		if (capStart)
		{
			Array.Copy(capStartVerts, 0, verts, offset, capStartVerts.Length);
			Array.Copy(capStartUVS, 0, uvs, offset, capStartUVS.Length);
			Array.Copy(capStartCols, 0, cols, offset, capStartCols.Length);
			offset += capStartVerts.Length;
		}
		if (capEnd)
		{
			Array.Copy(capEndVerts, 0, verts, offset, capEndVerts.Length);
			Array.Copy(capEndUVS, 0, uvs, offset, capEndUVS.Length);
			Array.Copy(capEndCols, 0, cols, offset, capEndCols.Length);
			offset += capEndVerts.Length;
		}
	}

	public override int NumVerts()
	{
		int num = loftverts.Length;
		if (capStart)
		{
			num += capStartVerts.Length;
		}
		if (capEnd)
		{
			num += capEndVerts.Length;
		}
		return num;
	}

	public override Material GetMaterial(int i)
	{
		switch (i)
		{
		case 0:
			return material;
		case 1:
			if (capStart)
			{
				return capStartMat;
			}
			break;
		}
		return capEndMat;
	}

	public override int[] GetTris(int i)
	{
		switch (i)
		{
		case 0:
			return lofttris;
		case 1:
			if (capStart)
			{
				return capStartTris;
			}
			break;
		}
		return capEndTris;
	}

	public override int NumMaterials()
	{
		int num = 1;
		if (capStart)
		{
			num++;
		}
		if (capEnd)
		{
			num++;
		}
		return num;
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)layerSection && (bool)layerPath && layerSection.splines != null && layerPath.splines != null)
		{
			if (curve < 0)
			{
				curve = 0;
			}
			if (curve > layerPath.splines.Count - 1)
			{
				curve = layerPath.splines.Count - 1;
			}
			if (crosscurve < 0)
			{
				crosscurve = 0;
			}
			if (crosscurve > layerSection.splines.Count - 1)
			{
				crosscurve = layerSection.splines.Count - 1;
			}
			return true;
		}
		return false;
	}

	public void InitCurves()
	{
		MegaShape megaShape = layerPath;
		if ((bool)megaShape)
		{
			MegaSpline megaSpline = megaShape.splines[curve];
			while (twistCrv.keys.Length != 0)
			{
				twistCrv.RemoveKey(0);
			}
			while (offsetCrvX.keys.Length != 0)
			{
				offsetCrvX.RemoveKey(0);
			}
			while (offsetCrvY.keys.Length != 0)
			{
				offsetCrvY.RemoveKey(0);
			}
			while (offsetCrvZ.keys.Length != 0)
			{
				offsetCrvZ.RemoveKey(0);
			}
			while (crossScaleCrv.keys.Length != 0)
			{
				crossScaleCrv.RemoveKey(0);
			}
			while (crossScaleCrvY.keys.Length != 0)
			{
				crossScaleCrvY.RemoveKey(0);
			}
			for (int i = 0; i < megaSpline.knots.Count; i++)
			{
				float time = megaSpline.knots[i].length / megaSpline.length;
				twistCrv.AddKey(new Keyframe(time, 0f));
				offsetCrvX.AddKey(new Keyframe(time, 0f));
				offsetCrvY.AddKey(new Keyframe(time, 0f));
				offsetCrvZ.AddKey(new Keyframe(time, 0f));
				crossScaleCrv.AddKey(new Keyframe(time, 1f));
				crossScaleCrvY.AddKey(new Keyframe(time, 1f));
			}
		}
	}

	public float GetLength(MegaShapeLoft loft)
	{
		MegaShape megaShape = layerPath;
		if ((bool)megaShape)
		{
			return megaShape.splines[curve].length * pathLength;
		}
		return 1f;
	}

	public virtual float GetCrossLength(float alpha)
	{
		MegaShape megaShape = layerSection;
		if ((bool)megaShape)
		{
			return megaShape.splines[crosscurve].length;
		}
		return 1f;
	}

	private int GetIndexInterp(float val, ref float interp)
	{
		int num = (int)val;
		interp = val - (float)num;
		return num;
	}

	private Vector3 GetCross(MegaShapeLoft loft, float ca)
	{
		MegaShape megaShape = layerSection;
		Matrix4x4 mat = Matrix4x4.identity;
		MegaMatrix.Translate(ref mat, pivot);
		MegaMatrix.Rotate(ref mat, new Vector3((float)Math.PI / 180f * crossRot.x, (float)Math.PI / 180f * crossRot.y, (float)Math.PI / 180f * crossRot.z));
		int k = -1;
		float num = crossStart + ca;
		MegaSpline megaSpline = megaShape.splines[crosscurve];
		if (megaSpline.closed && num < 0f)
		{
			num += 1f;
		}
		Vector3 vector = Vector3.zero;
		if (snap)
		{
			vector = megaShape.splines[0].knots[0].p - megaSpline.knots[0].p;
		}
		if (megaSpline.closed)
		{
			num = Mathf.Repeat(num, 1f);
		}
		return mat.MultiplyPoint3x4(megaShape.splines[crosscurve].InterpCurve3D(num, megaShape.normalizedInterp, ref k) + vector);
	}

	public virtual Vector3 SampleSplines(MegaShapeLoft loft, float ca, float pa)
	{
		MegaSpline megaSpline = layerPath.splines[curve];
		Vector3 zero = Vector3.zero;
		Vector3 one = Vector3.one;
		float num = 1f;
		float num2 = 1f;
		Vector3 vector = crossmax;
		vector.x = 0f;
		vector.z = 0f;
		Vector3 vector2 = crossmin;
		vector2.x = 0f;
		vector2.z = 0f;
		Vector3 zero2 = Vector3.zero;
		Matrix4x4 mat = Matrix4x4.identity;
		Vector3 lastup = loft.up;
		float num3 = pathStart + pathLength * pa;
		zero2 = offset;
		if (useOffsetX)
		{
			zero2.x += offsetCrvX.Evaluate(num3);
		}
		if (useOffsetY)
		{
			zero2.y += offsetCrvY.Evaluate(num3);
		}
		if (useOffsetZ)
		{
			zero2.z += offsetCrvZ.Evaluate(num3);
		}
		Matrix4x4 matrix4x = ((frameMethod != MegaFrameMethod.New) ? loft.GetDeformMatNew(megaSpline, num3, interp: true, alignCross) : loft.GetDeformMatNewMethod(megaSpline, num3, interp: true, alignCross, ref lastup));
		if (useTwistCrv)
		{
			float twist = megaSpline.GetTwist(num3);
			float num4 = twistCrv.Evaluate(num3) * twistAmt;
			MegaShapeUtils.RotateZ(ref mat, (float)Math.PI / 180f * (num4 - twist));
			matrix4x *= mat;
		}
		if (useCrossScaleCrv)
		{
			float time = Mathf.Repeat(pa + scaleoff, 1f);
			num = crossScaleCrv.Evaluate(time);
		}
		if (!sepscale)
		{
			num2 = num;
		}
		else if (useCrossScaleCrvY)
		{
			float time2 = Mathf.Repeat(pa + scaleoffY, 1f);
			num2 = crossScaleCrvY.Evaluate(time2);
		}
		one.x = crossScale.x * num;
		one.y = crossScale.y * num2;
		Vector3 v = vector;
		v.y *= one.y;
		Vector3 v2 = vector2;
		v2.y *= one.y;
		Vector3 vector3 = matrix4x.MultiplyPoint(v);
		Vector3 vector4 = matrix4x.MultiplyPoint(v2);
		float num5 = 1f / (crosssize.y * one.y);
		float bottom = Bottom;
		float top = Top;
		zero = GetCross(loft, ca);
		zero.x *= one.x;
		zero.y *= one.y;
		zero.z *= one.z;
		zero = matrix4x.MultiplyPoint3x4(zero);
		zero += zero2;
		if (clipBottom && zero.y < clipBottomVal)
		{
			zero.y = clipBottomVal;
		}
		if (snapBottom)
		{
			float y = zero.y;
			y = 1f - (vector3.y - y) * num5;
			zero.y = Mathf.Lerp(bottom, vector3.y, y);
		}
		if (clipTop && zero.y > clipTopVal)
		{
			zero.y = clipTopVal;
		}
		if (snapTop)
		{
			float y2 = zero.y;
			y2 = (y2 - vector4.y) * num5;
			zero.y = Mathf.Lerp(vector4.y, top, y2);
		}
		return zero;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1f);
		a = Mathf.Repeat(a, 1f);
		float num = (float)(crossverts.Length - 1) * ca;
		int num2 = (int)num;
		float t = num - (float)num2;
		int num3 = num2 + 1;
		bool flag = false;
		int num4;
		int num5;
		float num6;
		if (pathLength < 0.9999f || !layerPath.splines[curve].closed)
		{
			if (a < 0f)
			{
				num4 = 0;
				num5 = 1;
				num6 = a * GetLength(loft);
			}
			else if (a >= 0.9999f)
			{
				num4 = crosses - 1;
				num5 = crosses - 2;
				num6 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(crosses - 1) * a;
				num4 = (int)num7;
				num6 = num7 - (float)num4;
				num5 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(crosses - 1) * a;
			num4 = (int)num8;
			num6 = num8 - (float)num4;
			num5 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * crossverts.Length + num2];
		Vector3 to = loftverts[num4 * crossverts.Length + num3];
		Vector3 vector2 = loftverts[num5 * crossverts.Length + num2];
		Vector3 to2 = loftverts[num5 * crossverts.Length + num3];
		Vector3 vector3 = Vector3.Lerp(vector, to, t);
		Vector3 vector4 = Vector3.Lerp(vector2, to2, t);
		Vector3 vector5 = vector4 - vector3;
		if (flag)
		{
			vector2.x = vector4.x + vector5.x * num6;
			vector2.y = vector4.y + vector5.y * num6;
			vector2.z = vector4.z + vector5.z * num6;
		}
		else
		{
			vector2.x = vector3.x + vector5.x * num6;
			vector2.y = vector3.y + vector5.y * num6;
			vector2.z = vector3.z + vector5.z * num6;
		}
		return vector2;
	}

	public virtual Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		float num = (float)(crossverts.Length - 1) * ca;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		int num4 = num2 + 1;
		bool flag = false;
		int num5;
		int num6;
		float num7;
		if (pathLength < 0.9999f || !layerPath.splines[curve].closed)
		{
			if (a < 0f)
			{
				num5 = 0;
				num6 = 1;
				num7 = a * GetLength(loft);
			}
			else if (a >= 0.9999f)
			{
				num5 = crosses - 1;
				num6 = crosses - 2;
				num7 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num8 = (float)(crosses - 1) * a;
				num5 = (int)num8;
				num7 = num8 - (float)num5;
				num6 = num5 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num9 = (float)(crosses - 1) * a;
			num5 = (int)num9;
			num7 = num9 - (float)num5;
			num6 = num5 + 1;
		}
		Vector3 vector = loftverts[num5 * crossverts.Length + num2];
		Vector3 vector2 = loftverts[num5 * crossverts.Length + num4];
		Vector3 result = loftverts[num6 * crossverts.Length + num2];
		Vector3 vector3 = loftverts[num6 * crossverts.Length + num4];
		float num10 = num7 + at;
		vector.x += (vector2.x - vector.x) * num3;
		vector.y += (vector2.y - vector.y) * num3;
		vector.z += (vector2.z - vector.z) * num3;
		vector2.x = result.x + (vector3.x - result.x) * num3;
		vector2.y = result.y + (vector3.y - result.y) * num3;
		vector2.z = result.z + (vector3.z - result.z) * num3;
		if (flag)
		{
			vector2.x = vector.x - vector2.x;
			vector2.y = vector.y - vector2.y;
			vector2.z = vector.z - vector2.z;
		}
		else
		{
			vector2.x -= vector.x;
			vector2.y -= vector.y;
			vector2.z -= vector.z;
		}
		result.x = vector.x + vector2.x * num7;
		result.y = vector.y + vector2.y * num7;
		result.z = vector.z + vector2.z * num7;
		p.x = vector.x + vector2.x * num10;
		p.y = vector.y + vector2.y * num10;
		p.z = vector.z + vector2.z * num10;
		return result;
	}

	public virtual Vector3 GetPosAndUp(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);
		float num = (float)(crossverts.Length - 1) * ca;
		int num2 = (int)num;
		float t = num - (float)num2;
		int num3 = num2 + 1;
		bool flag = false;
		int num4;
		float num5;
		int num6;
		if (pathLength < 0.9999f || !layerPath.splines[curve].closed)
		{
			if (a < 0f)
			{
				num4 = 0;
				num5 = a * GetLength(loft);
				num6 = num4 + 1;
			}
			else if (a >= 0.999f)
			{
				num4 = crosses - 2;
				num6 = crosses - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(crosses - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(crosses - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * crossverts.Length + num2];
		Vector3 vector2 = loftverts[num4 * crossverts.Length + num3];
		Vector3 vector3 = loftverts[num6 * crossverts.Length + num2];
		Vector3 to = loftverts[num6 * crossverts.Length + num3];
		Vector3 vector4 = Vector3.Lerp(vector, vector2, t);
		Vector3 vector5 = Vector3.Lerp(vector3, to, t);
		Vector3 vector6 = vector5 - vector4;
		float num9 = num5 + at;
		Vector3 lhs = vector2 - vector;
		Vector3 rhs = vector3 - vector;
		up = Vector3.Cross(lhs, rhs).normalized;
		if (flag)
		{
			p.x = vector5.x + vector6.x * num9;
			p.y = vector5.y + vector6.y * num9;
			p.z = vector5.z + vector6.z * num9;
			vector3.x = vector5.x + vector6.x * num5;
			vector3.y = vector5.y + vector6.y * num5;
			vector3.z = vector5.z + vector6.z * num5;
		}
		else
		{
			p.x = vector4.x + vector6.x * num9;
			p.y = vector4.y + vector6.y * num9;
			p.z = vector4.z + vector6.z * num9;
			vector3.x = vector4.x + vector6.x * num5;
			vector3.y = vector4.y + vector6.y * num5;
			vector3.z = vector4.z + vector6.z * num5;
		}
		return vector3;
	}

	public virtual Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		float num = (float)(crossverts.Length - 1) * ca;
		int num2 = (int)num;
		float t = num - (float)num2;
		int num3 = num2 + 1;
		bool flag = false;
		int num4;
		float num5;
		int num6;
		if (pathLength < 0.9999f || !layerPath.splines[curve].closed)
		{
			if (a < 0f)
			{
				num4 = 0;
				num5 = a * GetLength(loft);
				num6 = num4 + 1;
			}
			else if (a >= 0.999f)
			{
				num4 = crosses - 2;
				num6 = crosses - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(crosses - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(crosses - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * crossverts.Length + num2];
		Vector3 vector2 = loftverts[num4 * crossverts.Length + num3];
		Vector3 vector3 = loftverts[num6 * crossverts.Length + num2];
		Vector3 to = loftverts[num6 * crossverts.Length + num3];
		Vector3 vector4 = Vector3.Lerp(vector, vector2, t);
		Vector3 vector5 = Vector3.Lerp(vector3, to, t);
		Vector3 vector6 = vector5 - vector4;
		float num9 = num5 + at;
		Vector3 lhs = vector2 - vector;
		Vector3 rhs = vector3 - vector;
		right = lhs.normalized;
		fwd = rhs.normalized;
		up = Vector3.Cross(lhs, rhs).normalized;
		if (flag)
		{
			vector3.x = vector5.x + vector6.x * num5;
			vector3.y = vector5.y + vector6.y * num5;
			vector3.z = vector5.z + vector6.z * num5;
			p.x = vector5.x + vector6.x * num9;
			p.y = vector5.y + vector6.y * num9;
			p.z = vector5.z + vector6.z * num9;
		}
		else
		{
			vector3.x = vector4.x + vector6.x * num5;
			vector3.y = vector4.y + vector6.y * num5;
			vector3.z = vector4.z + vector6.z * num5;
			p.x = vector4.x + vector6.x * num9;
			p.y = vector4.y + vector6.y * num9;
			p.z = vector4.z + vector6.z * num9;
		}
		return vector3;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * value / 1f) - 1f) + start;
	}

	public virtual Vector3 GetPos1(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Repeat(ca, 1f);
		a = Mathf.Repeat(a, 1f);
		float num = (float)(crossverts.Length - 1) * ca;
		int num2 = (int)num;
		float t = num - (float)num2;
		int num3 = num2 + 1;
		bool flag = false;
		int num4;
		float num5;
		int num6;
		if (pathLength < 0.9999f || !layerPath.splines[curve].closed)
		{
			if (a < 0f)
			{
				num4 = 0;
				num5 = a * GetLength(loft);
				num6 = num4 + 1;
			}
			else if (a >= 0.999f)
			{
				num4 = crosses - 2;
				num6 = crosses - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(crosses - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(crosses - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * crossverts.Length + num2];
		Vector3 to = loftverts[num4 * crossverts.Length + num3];
		Vector3 vector2 = loftverts[num6 * crossverts.Length + num2];
		Vector3 to2 = loftverts[num6 * crossverts.Length + num3];
		Vector3 vector3 = Vector3.Lerp(vector, to, t);
		Vector3 vector4 = Vector3.Lerp(vector2, to2, t);
		Vector3 vector5 = vector4 - vector3;
		float num9 = num5 + at;
		up = Vector3.Lerp(crossnorms[num2], crossnorms[num3], t);
		if (flag)
		{
			vector2.x = vector4.x + vector5.x * num5;
			vector2.y = vector4.y + vector5.y * num5;
			vector2.z = vector4.z + vector5.z * num5;
			p.x = vector4.x + vector5.x * num9;
			p.y = vector4.y + vector5.y * num9;
			p.z = vector4.z + vector5.z * num9;
		}
		else
		{
			vector2.x = vector3.x + vector5.x * num5;
			vector2.y = vector3.y + vector5.y * num5;
			vector2.z = vector3.z + vector5.z * num5;
			p.x = vector3.x + vector5.x * num9;
			p.y = vector3.y + vector5.y * num9;
			p.z = vector3.z + vector5.z * num9;
		}
		return vector2;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		MegaShape megaShape = layerSection;
		verts.Clear();
		uvs.Clear();
		norms.Clear();
		cols.Clear();
		Matrix4x4 mat = Matrix4x4.identity;
		MegaMatrix.Translate(ref mat, pivot);
		MegaMatrix.Rotate(ref mat, new Vector3((float)Math.PI / 180f * crossRot.x, (float)Math.PI / 180f * crossRot.y, (float)Math.PI / 180f * crossRot.z));
		if (base.enabled)
		{
			float num = crossDist;
			if (num < 0.01f)
			{
				num = 0.01f;
			}
			int k = -1;
			int k2 = -1;
			svert = verts.Count;
			Vector2 zero = Vector2.zero;
			float num2 = crossStart;
			float num3 = crossStart + crossEnd;
			MegaSpline megaSpline = megaShape.splines[crosscurve];
			if (megaSpline.closed && num2 < 0f)
			{
				num2 += 1f;
			}
			Vector3 vector = Vector3.zero;
			if (snap)
			{
				vector = megaShape.splines[0].knots[0].p - megaSpline.knots[0].p;
			}
			if (megaSpline.closed)
			{
				num2 = Mathf.Repeat(num2, 1f);
			}
			Vector3 item = mat.MultiplyPoint3x4(megaShape.splines[crosscurve].InterpCurve3D(num2, megaShape.normalizedInterp, ref k2) + vector);
			verts.Add(item);
			float y = crossStart;
			zero.y = y;
			uvs.Add(zero);
			cols.Add(Color.white);
			int num4 = (int)(megaShape.splines[crosscurve].length * crossEnd / num);
			for (int i = 1; i <= num4; i++)
			{
				float num5 = (float)i / (float)num4;
				num2 = crossStart + num5 * crossEnd;
				y = num2;
				if (megaShape.splines[crosscurve].closed)
				{
					num2 = Mathf.Repeat(num2, 1f);
				}
				Vector3 item2 = mat.MultiplyPoint3x4(megaShape.splines[crosscurve].InterpCurve3D(num2, megaShape.normalizedInterp, ref k) + vector);
				if (includeknots && k != k2)
				{
					for (k2++; k2 <= k; k2++)
					{
						verts.Add(mat.MultiplyPoint3x4(megaShape.splines[crosscurve].knots[k2].p + vector));
						zero.y = megaShape.splines[crosscurve].knots[k2 - 1].length / megaShape.splines[crosscurve].length + Mathf.Floor(num5);
						uvs.Add(zero);
						cols.Add(Color.white);
					}
				}
				k2 = k;
				verts.Add(item2);
				zero.y = y;
				uvs.Add(zero);
				cols.Add(Color.white);
			}
			if (megaShape.splines[crosscurve].closed)
			{
				num2 = Mathf.Repeat(num3, 1f);
			}
			evert = verts.Count - 1;
			zero.y = num3;
			uvs.Add(zero);
			cols.Add(Color.white);
		}
		if (planarMapping && planarMode == MegaPlanarMode.Normal)
		{
			int index = (int)planarAxis;
			float num6 = verts[0][index];
			float num7 = num6;
			for (int j = 1; j < verts.Count; j++)
			{
				if (verts[j][index] < num6)
				{
					num6 = verts[j][index];
				}
				if (verts[j][index] > num7)
				{
					num7 = verts[j][index];
				}
			}
			for (int l = 0; l < verts.Count; l++)
			{
				Vector2 value = uvs[l];
				value.y = (verts[l][index] - num6) / (num7 - num6);
				uvs[l] = value;
			}
		}
		crossverts = verts.ToArray();
		crossuvs = uvs.ToArray();
		crossnorms = verts.ToArray();
		Vector3 zero2 = Vector3.zero;
		int num8 = 0;
		for (num8 = 0; num8 < crossnorms.Length - 1; num8++)
		{
			Vector3 vector2 = crossverts[num8 + 1] - crossverts[num8];
			zero2.x = 0f - vector2.y;
			zero2.y = vector2.x;
			zero2.z = 0f;
			crossnorms[num8] = zero2;
		}
		if (num8 > 0)
		{
			ref Vector3 reference = ref crossnorms[num8];
			reference = crossnorms[num8 - 1];
		}
		crosssize = MegaUtils.Extents(crossverts, out crossmin, out crossmax);
		Prepare(loft);
		return true;
	}

	private void Prepare(MegaShapeLoft loft)
	{
		MegaShape megaShape = layerPath;
		crosses = Mathf.CeilToInt((LoftLength = megaShape.splines[curve].length * pathLength) / pathDist);
		if (crosses < 2)
		{
			crosses = 2;
		}
		numtris = crosses * (evert - svert) * 2 * 3;
		if (base.enabled && (lofttris == null || numtris != lofttris.Length))
		{
			lofttris = new int[numtris];
		}
		if (base.enabled)
		{
			uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate.x, UVRotate.y, 0f), Vector3.one);
		}
		numverts = crosses * crossverts.Length;
		if (loftverts == null || numverts != loftverts.Length)
		{
			loftverts = new Vector3[numverts];
		}
		if (conform && (loftverts1 == null || loftverts1.Length != loftverts.Length))
		{
			loftverts1 = new Vector3[loftverts.Length];
		}
		if (loftuvs == null || numverts != loftuvs.Length)
		{
			loftuvs = new Vector2[numverts];
		}
		if (loft.useColors && (loftcols == null || numverts != loftcols.Length))
		{
			loftcols = new Color[numverts];
		}
		CalcCaps();
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		trisstart = triindex;
		if (Lock)
		{
			return triindex + (crosses - 2) * (evert - svert);
		}
		if (layerPath == null || layerSection == null)
		{
			return triindex;
		}
		MegaSpline megaSpline = layerPath.splines[curve];
		MegaSpline megaSpline2 = layerSection.splines[crosscurve];
		int num = 0;
		Vector2 zero = Vector2.zero;
		Vector3 v = Vector3.zero;
		Vector3 one = Vector3.one;
		float num2 = 1f;
		float num3 = 1f;
		Vector3 vector = crossmax;
		vector.x = 0f;
		vector.z = 0f;
		Vector3 vector2 = crossmin;
		vector2.x = 0f;
		vector2.z = 0f;
		Vector3 zero2 = Vector3.zero;
		float num4 = pathStart;
		if (UVOrigin == MegaLoftUVOrigin.SplineStart)
		{
			num4 = 0f;
		}
		Matrix4x4 mat = Matrix4x4.identity;
		Matrix4x4 matrix4x = Matrix4x4.identity;
		if (planarMapping && planarMode == MegaPlanarMode.World)
		{
			matrix4x = ((!lockWorld) ? base.transform.localToWorldMatrix : lockedTM);
		}
		Color color = this.color;
		Vector3 lastup = loft.up;
		for (int i = 0; i < crosses; i++)
		{
			float num5 = (float)i / (float)(crosses - 1);
			float num6 = pathStart + pathLength * num5;
			zero2 = offset;
			if (useOffsetX)
			{
				zero2.x += offsetCrvX.Evaluate(num6);
			}
			if (useOffsetY)
			{
				zero2.y += offsetCrvY.Evaluate(num6);
			}
			if (useOffsetZ)
			{
				zero2.z += offsetCrvZ.Evaluate(num6);
			}
			Matrix4x4 matrix4x2 = ((frameMethod != MegaFrameMethod.New) ? loft.GetDeformMatNew(megaSpline, num6, interp: true, alignCross) : loft.GetDeformMatNewMethod(megaSpline, num6, interp: true, alignCross, ref lastup));
			if (useTwistCrv)
			{
				float num7 = twistCrv.Evaluate(num6) * twistAmt;
				float twist = megaSpline.GetTwist(num6);
				MegaShapeUtils.RotateZ(ref mat, (float)Math.PI / 180f * (num7 - twist));
				matrix4x2 *= mat;
			}
			if (useCrossScaleCrv)
			{
				float time = Mathf.Repeat(num5 + scaleoff, 1f);
				num2 = crossScaleCrv.Evaluate(time);
			}
			if (!sepscale)
			{
				num3 = num2;
			}
			else if (useCrossScaleCrvY)
			{
				float time2 = Mathf.Repeat(num5 + scaleoffY, 1f);
				num3 = crossScaleCrvY.Evaluate(time2);
			}
			one.x = crossScale.x * num2;
			one.y = crossScale.y * num3;
			Vector3 v2 = vector;
			v2.y *= one.y;
			Vector3 v3 = vector2;
			v3.y *= one.y;
			Vector3 vector3 = matrix4x2.MultiplyPoint(v2);
			Vector3 vector4 = matrix4x2.MultiplyPoint(v3);
			float num8 = 1f / (crosssize.y * one.y);
			float bottom = Bottom;
			float top = Top;
			if (loft.useColors)
			{
				color.r = colR.Evaluate(num5);
				color.g = colG.Evaluate(num5);
				color.b = colB.Evaluate(num5);
				color.a = colA.Evaluate(num5);
			}
			for (int j = 0; j < crossverts.Length; j++)
			{
				v.x = crossverts[j].x * one.x;
				v.y = crossverts[j].y * one.y;
				v.z = crossverts[j].z * one.z;
				v = matrix4x2.MultiplyPoint3x4(v);
				if (clipBottom && v.y < clipBottomVal)
				{
					v.y = clipBottomVal;
				}
				if (snapBottom)
				{
					float y = v.y;
					y = 1f - (vector3.y - y) * num8;
					v.y = Mathf.Lerp(bottom, vector3.y, y);
				}
				if (clipTop && v.y > clipTopVal)
				{
					v.y = clipTopVal;
				}
				if (snapTop)
				{
					float y2 = v.y;
					y2 = (y2 - vector4.y) * num8;
					v.y = Mathf.Lerp(vector4.y, top, y2);
				}
				if (conform)
				{
					loftverts1[num].x = v.x + zero2.x;
					loftverts1[num].y = v.y + zero2.y;
					loftverts1[num].z = v.z + zero2.z;
				}
				else
				{
					loftverts[num].x = v.x + zero2.x;
					loftverts[num].y = v.y + zero2.y;
					loftverts[num].z = v.z + zero2.z;
				}
				if (planarMode == MegaPlanarMode.World)
				{
					v = matrix4x.MultiplyPoint(v);
				}
				if (sideViewUV)
				{
					zero.x = v[(int)sideViewAxis];
					zero.y = crossuvs[j].y - crossStart;
				}
				else
				{
					zero.x = num6 - num4;
					zero.y = crossuvs[j].y - crossStart;
					if (physuv)
					{
						zero.x *= megaSpline.length;
						zero.y *= megaSpline2.length;
					}
					else if (uvcalcy)
					{
						zero.x = num5 * LoftLength / megaSpline2.length - num4;
					}
				}
				if (planarMapping)
				{
					switch (planarMode)
					{
					case MegaPlanarMode.World:
						zero.y = v[(int)planarAxis];
						break;
					case MegaPlanarMode.Local:
						zero.y = v[(int)planarAxis];
						break;
					}
				}
				if (swapuv)
				{
					float x = zero.x;
					zero.x = zero.y;
					zero.y = x;
				}
				zero.x *= UVScale.x;
				zero.y *= UVScale.y;
				zero.x += UVOffset.x;
				zero.y += UVOffset.y;
				loftuvs[num] = zero;
				if (loft.useColors)
				{
					loftcols[num] = color;
				}
				num++;
			}
		}
		int num9 = triindex;
		int num10 = 0;
		if (base.enabled)
		{
			if (flip)
			{
				for (int k = 0; k < crosses - 1; k++)
				{
					for (int l = svert; l < evert; l++)
					{
						lofttris[num10] = num9 + l;
						lofttris[num10 + 1] = num9 + l + 1;
						lofttris[num10 + 2] = num9 + l + 1 + crossverts.Length;
						lofttris[num10 + 3] = num9 + l;
						lofttris[num10 + 4] = num9 + l + 1 + crossverts.Length;
						lofttris[num10 + 5] = num9 + l + crossverts.Length;
						num10 += 6;
					}
					num9 += crossverts.Length;
				}
			}
			else
			{
				for (int m = 0; m < crosses - 1; m++)
				{
					for (int n = svert; n < evert; n++)
					{
						lofttris[num10 + 2] = num9 + n;
						lofttris[num10 + 1] = num9 + n + 1;
						lofttris[num10] = num9 + n + 1 + crossverts.Length;
						lofttris[num10 + 5] = num9 + n;
						lofttris[num10 + 4] = num9 + n + 1 + crossverts.Length;
						lofttris[num10 + 3] = num9 + n + crossverts.Length;
						num10 += 6;
					}
					num9 += crossverts.Length;
				}
			}
		}
		num9 = triindex + loftverts.Length;
		if (capStart)
		{
			Matrix4x4 matrix4x3 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, capStartUVRot), Vector3.one);
			Color color2 = this.color;
			if (loft.useColors)
			{
				color2.r = colR.Evaluate(0f);
				color2.g = colG.Evaluate(0f);
				color2.b = colB.Evaluate(0f);
				color2.a = colA.Evaluate(0f);
			}
			for (int num11 = 0; num11 < capStartVerts.Length; num11++)
			{
				Vector3 vector5 = ((!conform) ? loftverts[num11] : loftverts1[num11]);
				capStartVerts[num11] = vector5;
				Vector3 v4 = crossverts[num11];
				v4.y = vector5.y;
				v4 = matrix4x3.MultiplyPoint(v4);
				capStartUVS[num11].x = v4.x * capStartUVScale.x + capStartUVOffset.x;
				capStartUVS[num11].y = v4.y * capStartUVScale.y + capStartUVOffset.y;
				if (loft.useColors)
				{
					capStartCols[num11] = color2;
				}
			}
			if (capflip)
			{
				for (int num12 = 0; num12 < capfaces.Count; num12 += 3)
				{
					capStartTris[num12 + 2] = capfaces[num12] + num9;
					capStartTris[num12 + 1] = capfaces[num12 + 1] + num9;
					capStartTris[num12] = capfaces[num12 + 2] + num9;
				}
			}
			else
			{
				for (int num13 = 0; num13 < capfaces.Count; num13 += 3)
				{
					capStartTris[num13] = capfaces[num13] + num9;
					capStartTris[num13 + 1] = capfaces[num13 + 1] + num9;
					capStartTris[num13 + 2] = capfaces[num13 + 2] + num9;
				}
			}
			num10 += capfaces.Count;
			num9 += capStartVerts.Length;
		}
		if (capEnd)
		{
			Matrix4x4 matrix4x4 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, capEndUVRot), Vector3.one);
			Color color3 = this.color;
			if (loft.useColors)
			{
				color3.r = colR.Evaluate(1f);
				color3.g = colG.Evaluate(1f);
				color3.b = colB.Evaluate(1f);
				color3.a = colA.Evaluate(1f);
			}
			int num14 = ((!conform) ? (loftverts.Length - crossverts.Length) : (loftverts1.Length - crossverts.Length));
			for (int num15 = 0; num15 < capEndVerts.Length; num15++)
			{
				Vector3 vector6 = ((!conform) ? loftverts[num14 + num15] : loftverts1[num14 + num15]);
				capEndVerts[num15] = vector6;
				Vector3 v5 = crossverts[num15];
				v5.y = vector6.y;
				v5 = matrix4x4.MultiplyPoint(v5);
				capEndUVS[num15].x = v5.x * capEndUVScale.x + capEndUVOffset.x;
				capEndUVS[num15].y = v5.y * capEndUVScale.y + capEndUVOffset.y;
				if (loft.useColors)
				{
					capEndCols[num15] = color3;
				}
			}
			if (capflip)
			{
				for (int num16 = 0; num16 < capfaces.Count; num16 += 3)
				{
					capEndTris[num16] = capfaces[num16] + num9;
					capEndTris[num16 + 1] = capfaces[num16 + 1] + num9;
					capEndTris[num16 + 2] = capfaces[num16 + 2] + num9;
				}
			}
			else
			{
				for (int num17 = 0; num17 < capfaces.Count; num17 += 3)
				{
					capEndTris[num17 + 2] = capfaces[num17] + num9;
					capEndTris[num17 + 1] = capfaces[num17 + 1] + num9;
					capEndTris[num17] = capfaces[num17 + 2] + num9;
				}
			}
			num10 += capfaces.Count;
		}
		if (conform)
		{
			CalcBounds();
			DoConform(loft);
		}
		return triindex + num10;
	}

	private void CalcCaps()
	{
		if (capStart || capEnd)
		{
			capfaces = MegaShapeTriangulator.Triangulate(crossverts, ref capfaces);
		}
		if (capStart)
		{
			if (capStartVerts == null || crossverts.Length != capStartVerts.Length)
			{
				capStartVerts = new Vector3[crossverts.Length];
				capStartUVS = new Vector2[crossverts.Length];
			}
			if (capStartCols == null || capStartCols.Length != crossverts.Length)
			{
				capStartCols = new Color[crossverts.Length];
			}
			if (capStartTris == null || capStartTris.Length != capfaces.Count)
			{
				capStartTris = new int[capfaces.Count];
			}
		}
		if (capEnd)
		{
			if (capEndVerts == null || crossverts.Length != capEndVerts.Length)
			{
				capEndVerts = new Vector3[crossverts.Length];
				capEndUVS = new Vector2[crossverts.Length];
			}
			if (capEndCols == null || capEndCols.Length != crossverts.Length)
			{
				capEndCols = new Color[crossverts.Length];
			}
			if (capEndTris == null || capEndTris.Length != capfaces.Count)
			{
				capEndTris = new int[capfaces.Count];
			}
		}
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerSimple to = go.AddComponent<MegaLoftLayerSimple>();
		Copy(this, to);
		loftverts = null;
		loftverts1 = null;
		loftuvs = null;
		loftcols = null;
		capStartVerts = null;
		capStartUVS = null;
		capStartCols = null;
		capEndVerts = null;
		capEndUVS = null;
		capEndCols = null;
		capStartTris = null;
		capEndTris = null;
		lofttris = null;
		return null;
	}

	public void SetTarget(GameObject targ)
	{
		target = targ;
		if ((bool)target)
		{
			conformCollider = target.collider;
		}
	}

	private void CalcBounds()
	{
		if (loftverts1 == null || loftverts1.Length == 0)
		{
			return;
		}
		conminz = loftverts1[0].y;
		for (int i = 1; i < loftverts1.Length; i++)
		{
			if (loftverts1[i].y < conminz)
			{
				conminz = loftverts1[i].y;
			}
		}
	}

	private void InitConform()
	{
		if (offsets == null || offsets.Length != loftverts1.Length)
		{
			offsets = new float[loftverts1.Length];
			last = new float[loftverts1.Length];
		}
		for (int i = 0; i < loftverts1.Length; i++)
		{
			offsets[i] = loftverts1[i].y - conminz;
		}
		if (capStart)
		{
			if (capstartlast == null || capstartlast.Length != capStartVerts.Length)
			{
				capstartlast = new float[capStartVerts.Length];
				capstartoffsets = new float[capStartVerts.Length];
			}
			for (int j = 0; j < capStartVerts.Length; j++)
			{
				capstartoffsets[j] = capStartVerts[j].y - conminz;
			}
		}
		if (capEnd)
		{
			if (capendlast == null || capendlast.Length != capEndVerts.Length)
			{
				capendlast = new float[capEndVerts.Length];
				capendoffsets = new float[capEndVerts.Length];
			}
			for (int k = 0; k < capEndVerts.Length; k++)
			{
				capendoffsets[k] = capEndVerts[k].y - conminz;
			}
		}
		if ((bool)target)
		{
			conformCollider = target.collider;
		}
	}

	private void DoConform(MegaShapeLoft loft)
	{
		InitConform();
		if ((bool)target && (bool)conformCollider)
		{
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			Matrix4x4 inverse = localToWorldMatrix.inverse;
			Ray ray = default(Ray);
			float t = conformAmount * loft.conformAmount;
			RaycastHit hitInfo;
			for (int i = 0; i < loftverts1.Length; i++)
			{
				Vector3 origin = localToWorldMatrix.MultiplyPoint(loftverts1[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;
				ref Vector3 reference = ref loftverts[i];
				reference = loftverts1[i];
				if (conformCollider.Raycast(ray, out hitInfo, raydist))
				{
					Vector3 vector = inverse.MultiplyPoint(hitInfo.point);
					loftverts[i].y = Mathf.Lerp(loftverts1[i].y, vector.y + offsets[i] + conformOffset, t);
					last[i] = loftverts[i].y;
				}
				else
				{
					Vector3 origin2 = ray.origin;
					origin2.y -= raydist;
					loftverts[i].y = last[i];
				}
			}
			if (capStart)
			{
				for (int j = 0; j < capStartVerts.Length; j++)
				{
					Vector3 origin3 = localToWorldMatrix.MultiplyPoint(capStartVerts[j]);
					origin3.y += raystartoff;
					ray.origin = origin3;
					ray.direction = Vector3.down;
					ref Vector3 reference2 = ref capStartVerts[j];
					reference2 = capStartVerts[j];
					if (conformCollider.Raycast(ray, out hitInfo, raydist))
					{
						Vector3 vector2 = inverse.MultiplyPoint(hitInfo.point);
						capStartVerts[j].y = Mathf.Lerp(capStartVerts[j].y, vector2.y + capstartoffsets[j] + conformOffset, t);
						capstartlast[j] = capStartVerts[j].y;
					}
					else
					{
						Vector3 origin4 = ray.origin;
						origin4.y -= raydist;
						capStartVerts[j].y = capstartlast[j];
					}
				}
			}
			if (!capEnd)
			{
				return;
			}
			for (int k = 0; k < capEndVerts.Length; k++)
			{
				Vector3 origin5 = localToWorldMatrix.MultiplyPoint(capEndVerts[k]);
				origin5.y += raystartoff;
				ray.origin = origin5;
				ray.direction = Vector3.down;
				ref Vector3 reference3 = ref capEndVerts[k];
				reference3 = capEndVerts[k];
				if (conformCollider.Raycast(ray, out hitInfo, raydist))
				{
					Vector3 vector3 = inverse.MultiplyPoint(hitInfo.point);
					capEndVerts[k].y = Mathf.Lerp(capEndVerts[k].y, vector3.y + capendoffsets[k] + conformOffset, t);
					capendlast[k] = capEndVerts[k].y;
				}
				else
				{
					Vector3 origin6 = ray.origin;
					origin6.y -= raydist;
					capEndVerts[k].y = capendlast[k];
				}
			}
		}
		else
		{
			for (int l = 0; l < loftverts1.Length; l++)
			{
				ref Vector3 reference4 = ref loftverts[l];
				reference4 = loftverts1[l];
			}
		}
	}

	private void OptmizeMesh()
	{
		if (!optimize)
		{
			return;
		}
		addsections.Clear();
		Vector3 vector = loftverts[0];
		Vector3 vector2 = loftverts[crossverts.Length];
		Vector3 lhs = (vector2 - vector).normalized;
		float num = Mathf.Cos(maxdeviation * ((float)Math.PI / 180f));
		int num2 = 0;
		addsections.Add(num2);
		for (int i = crossverts.Length; i < loftverts.Length - crossverts.Length; i += crossverts.Length)
		{
			num2++;
			vector = vector2;
			vector2 = loftverts[i + crossverts.Length];
			Vector3 normalized = (vector2 - vector).normalized;
			if (Vector3.Dot(lhs, normalized) < num)
			{
				addsections.Add(num2);
				lhs = normalized;
			}
		}
		addsections.Add(num2 + 1);
		int num3 = loftverts.Length;
		int num4 = addsections.Count * crossverts.Length;
		Debug.Log("Optimized mesh uses " + (num3 - num4) + " less vertices");
		Debug.Log("sections " + addsections.Count);
	}
}
