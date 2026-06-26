using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerComplex : MegaLoftLayerSimple
{
	public List<MegaLoftSection> sections = new List<MegaLoftSection>();

	public bool advancedParams = true;

	public bool useScaleXCrv;

	public bool useScaleYCrv;

	public bool showsections = true;

	public bool useStepsPerKnotPath;

	public int stepsPerKnotPath = 1;

	public bool useStepsPerKnotCross;

	public int stepsPerKnotCross = 1;

	public AnimationCurve scaleCrvX = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public AnimationCurve scaleCrvY = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public int CrossSteps = 4;

	public int PathSteps = 16;

	public bool SnapToPath;

	public MegaLoftEaseType easeType;

	public MegaLoftEase ease = new MegaLoftEase();

	public float handlesize = 0.5f;

	public bool planaruv;

	public List<int> capfacesend = new List<int>();

	public Vector3[] crossvertsend;

	public int ActualCrossVerts;

	public int ActualPathSteps;

	private Matrix4x4 wtm1;

	private Vector3 locup = Vector3.up;

	public float PathTeeter = 1f;

	public override void FindShapes()
	{
		if (layerPath == null && pathName.Length > 0)
		{
			GameObject gameObject = GameObject.Find(pathName);
			if ((bool)gameObject)
			{
				layerPath = gameObject.GetComponent<MegaShape>();
			}
		}
		if (layerSection == null && sectionName.Length > 0)
		{
			GameObject gameObject2 = GameObject.Find(sectionName);
			if ((bool)gameObject2)
			{
				layerSection = gameObject2.GetComponent<MegaShape>();
			}
		}
		for (int i = 0; i < sections.Count; i++)
		{
			GameObject gameObject3 = GameObject.Find(sections[i].shapeName);
			if ((bool)gameObject3)
			{
				sections[i].shape = gameObject3.GetComponent<MegaShape>();
			}
		}
	}

	public override void CopyLayer(MegaLoftLayerBase from)
	{
		MegaLoftLayerComplex megaLoftLayerComplex = (MegaLoftLayerComplex)from;
		layerPath = from.layerPath;
		layerSection = from.layerSection;
		if ((bool)layerPath)
		{
			pathName = layerPath.gameObject.name;
		}
		if ((bool)layerSection)
		{
			sectionName = layerSection.gameObject.name;
		}
		for (int i = 0; i < sections.Count; i++)
		{
			sections[i].shape = megaLoftLayerComplex.sections[i].shape;
			if ((bool)sections[i].shape)
			{
				sections[i].shapeName = sections[i].shape.gameObject.name;
			}
		}
	}

	public override int GetHelp()
	{
		return 2121;
	}

	public override bool Valid()
	{
		if (LayerEnabled && sections != null && sections.Count > 0)
		{
			for (int i = 0; i < sections.Count; i++)
			{
				if (sections[i].shape == null)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private Vector3 GetCross(int csect, float ca, Vector3 off)
	{
		MegaLoftSection megaLoftSection = sections[csect];
		int index = megaLoftSection.curve;
		int k = -1;
		Matrix4x4 mat = Matrix4x4.identity;
		float start = crossStart;
		float length = crossEnd;
		if (megaLoftSection.uselen)
		{
			start = megaLoftSection.start;
			length = megaLoftSection.length;
		}
		MegaShape shape = megaLoftSection.shape;
		MegaMatrix.Translate(ref mat, pivot);
		Vector3 vector = crossRot + megaLoftSection.rot;
		MegaMatrix.Rotate(ref mat, new Vector3((float)Math.PI / 180f * vector.x, (float)Math.PI / 180f * vector.y, (float)Math.PI / 180f * vector.z));
		float num = start + ca * length;
		if (shape.splines[index].closed)
		{
			num = Mathf.Repeat(num, 1f);
		}
		Vector3 result = mat.MultiplyPoint3x4(shape.splines[index].InterpCurve3D(num, shape.normalizedInterp, ref k) + off + megaLoftSection.offset);
		result.x *= megaLoftSection.scale.x;
		result.y *= megaLoftSection.scale.y;
		result.z *= megaLoftSection.scale.z;
		return result;
	}

	public override Vector3 SampleSplines(MegaShapeLoft loft, float ca, float pa)
	{
		Vector3 zero = Vector3.zero;
		float lerp = 0f;
		Matrix4x4 matrix4x = Matrix4x4.identity;
		if (SnapToPath)
		{
			matrix4x = layerPath.transform.localToWorldMatrix;
		}
		Matrix4x4 mat = Matrix4x4.identity;
		Vector3 vector = Vector2.one;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		bool closed = layerPath.splines[curve].closed;
		Vector3 lastup = locup;
		float num4 = pathStart + pathLength * pa;
		if (closed)
		{
			num4 = Mathf.Repeat(num4, 1f);
		}
		if (useScaleXCrv)
		{
			vector.x = scaleCrvX.Evaluate(num4);
		}
		if (useScaleYCrv)
		{
			vector.y = scaleCrvY.Evaluate(num4);
		}
		Matrix4x4 matrix4x2;
		if (!useTwistCrv)
		{
			matrix4x2 = ((frameMethod != MegaFrameMethod.Old) ? (matrix4x * GetDeformMatNewMethod(layerPath.splines[curve], num4, layerPath.normalizedInterp, ref lastup)) : (matrix4x * GetDeformMat(layerPath.splines[curve], num4, layerPath.normalizedInterp)));
		}
		else
		{
			float num5 = twistCrv.Evaluate(num4);
			float twist = layerPath.splines[curve].GetTwist(num4);
			MegaShapeUtils.RotateZ(ref mat, (float)Math.PI / 180f * (num5 - twist));
			matrix4x2 = ((frameMethod != MegaFrameMethod.Old) ? (matrix4x * GetDeformMatNewMethod(layerPath.splines[curve], num4, layerPath.normalizedInterp, ref lastup) * mat) : (matrix4x * GetDeformMat(layerPath.splines[curve], num4, layerPath.normalizedInterp) * mat));
		}
		int section = GetSection(num4, out lerp);
		lerp = ease.easing(0f, 1f, lerp);
		Vector3 zero2 = Vector3.zero;
		zero2 = ((!sections[section].snap) ? Vector3.zero : (sections[0].shape.splines[sections[0].curve].knots[0].p - sections[section].shape.splines[sections[section].curve].knots[0].p));
		Vector3 cross = GetCross(section, ca, zero2);
		zero2 = ((!sections[section + 1].snap) ? Vector3.zero : (sections[0].shape.splines[sections[0].curve].knots[0].p - sections[section + 1].shape.splines[sections[section + 1].curve].knots[0].p));
		Vector3 cross2 = GetCross(section + 1, ca, zero2);
		if (useOffsetX)
		{
			num = offsetCrvX.Evaluate(num4);
		}
		if (useOffsetY)
		{
			num2 = offsetCrvY.Evaluate(num4);
		}
		if (useOffsetZ)
		{
			num3 = offsetCrvZ.Evaluate(num4);
		}
		zero = Vector3.Lerp(cross, cross2, lerp);
		if (useScaleXCrv)
		{
			zero.x *= vector.x;
		}
		if (useScaleYCrv)
		{
			zero.y *= vector.y;
		}
		zero.x += num;
		zero.y += num2;
		zero.z += num3;
		zero = matrix4x2.MultiplyPoint3x4(zero);
		return zero + offset;
	}

	public override Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		ca = Mathf.Repeat(ca, 1f);
		int actualCrossVerts = ActualCrossVerts;
		float num = (float)(actualCrossVerts - 1) * ca;
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
				num4 = ActualPathSteps - 2;
				num6 = ActualPathSteps - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(ActualPathSteps - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(ActualPathSteps - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * actualCrossVerts + num2];
		Vector3 to = loftverts[num4 * actualCrossVerts + num3];
		Vector3 vector2 = loftverts[num6 * actualCrossVerts + num2];
		Vector3 to2 = loftverts[num6 * actualCrossVerts + num3];
		Vector3 vector3 = Vector3.Lerp(vector, to, t);
		Vector3 vector4 = Vector3.Lerp(vector2, to2, t);
		Vector3 vector5 = vector4 - vector3;
		float num9 = num5 + at;
		if (flag)
		{
			p.x = vector4.x + vector5.x * num9;
			p.y = vector4.y + vector5.y * num9;
			p.z = vector4.z + vector5.z * num9;
			vector2.x = vector4.x + vector5.x * num5;
			vector2.y = vector4.y + vector5.y * num5;
			vector2.z = vector4.z + vector5.z * num5;
		}
		else
		{
			p.x = vector3.x + vector5.x * num9;
			p.y = vector3.y + vector5.y * num9;
			p.z = vector3.z + vector5.z * num9;
			vector2.x = vector3.x + vector5.x * num5;
			vector2.y = vector3.y + vector5.y * num5;
			vector2.z = vector3.z + vector5.z * num5;
		}
		return vector2;
	}

	public override Vector3 GetPosAndUp(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		int actualCrossVerts = ActualCrossVerts;
		float num = (float)(actualCrossVerts - 1) * ca;
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
				num4 = ActualPathSteps - 2;
				num6 = ActualPathSteps - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(ActualPathSteps - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(ActualPathSteps - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * actualCrossVerts + num2];
		Vector3 vector2 = loftverts[num4 * actualCrossVerts + num3];
		Vector3 vector3 = loftverts[num6 * actualCrossVerts + num2];
		Vector3 to = loftverts[num6 * actualCrossVerts + num3];
		Vector3 vector4 = Vector3.Lerp(vector, vector2, t);
		Vector3 vector5 = Vector3.Lerp(vector3, to, t);
		Vector3 vector6 = vector5 - vector4;
		float num9 = num5 + at;
		p.x = vector4.x + vector6.x * num9;
		p.y = vector4.y + vector6.y * num9;
		p.z = vector4.z + vector6.z * num9;
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

	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		int actualCrossVerts = ActualCrossVerts;
		float num = (float)(actualCrossVerts - 1) * ca;
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
				num4 = ActualPathSteps - 2;
				num6 = ActualPathSteps - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(ActualPathSteps - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(ActualPathSteps - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * actualCrossVerts + num2];
		Vector3 to = loftverts[num4 * actualCrossVerts + num3];
		Vector3 vector2 = loftverts[num6 * actualCrossVerts + num2];
		Vector3 to2 = loftverts[num6 * actualCrossVerts + num3];
		Vector3 vector3 = Vector3.Lerp(vector, to, t);
		Vector3 vector4 = Vector3.Lerp(vector2, to2, t);
		Vector3 vector5 = vector4 - vector3;
		float num9 = num5 + at;
		p.x = vector3.x + vector5.x * num9;
		p.y = vector3.y + vector5.y * num9;
		p.z = vector3.z + vector5.z * num9;
		right.x = to.x - vector.x;
		right.y = to.y - vector.y;
		right.z = to.z - vector.z;
		fwd.x = vector2.x - vector.x;
		fwd.y = vector2.y - vector.y;
		fwd.z = vector2.z - vector.z;
		up = Vector3.Cross(right, fwd);
		if (flag)
		{
			p.x = vector4.x + vector5.x * num9;
			p.y = vector4.y + vector5.y * num9;
			p.z = vector4.z + vector5.z * num9;
			vector2.x = vector4.x + vector5.x * num9;
			vector2.y = vector4.y + vector5.y * num9;
			vector2.z = vector4.z + vector5.z * num9;
		}
		else
		{
			p.x = vector3.x + vector5.x * num9;
			p.y = vector3.y + vector5.y * num9;
			p.z = vector3.z + vector5.z * num9;
			vector2.x = vector3.x + vector5.x * num9;
			vector2.y = vector3.y + vector5.y * num9;
			vector2.z = vector3.z + vector5.z * num9;
		}
		return vector2;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	private void BuildPolyShape(MegaLoftSection lsection, int steps, Vector3 off, float width)
	{
		int index = lsection.curve;
		int k = -1;
		Matrix4x4 mat = Matrix4x4.identity;
		Vector2 zero = Vector2.zero;
		float start = crossStart;
		float length = crossEnd;
		if (lsection.uselen)
		{
			start = lsection.start;
			length = lsection.length;
		}
		MegaShape shape = lsection.shape;
		verts.Clear();
		uvs.Clear();
		norms.Clear();
		MegaMatrix.Translate(ref mat, pivot);
		Vector3 vector = crossRot + lsection.rot;
		MegaMatrix.Rotate(ref mat, new Vector3((float)Math.PI / 180f * vector.x, (float)Math.PI / 180f * vector.y, (float)Math.PI / 180f * vector.z));
		svert = verts.Count;
		float t = start;
		if (shape.splines[index].closed)
		{
			t = Mathf.Repeat(t, 1f);
		}
		zero.y = start;
		float num = 0f;
		Vector3 b = Vector3.zero;
		for (int i = 0; i <= steps; i++)
		{
			t = start + (float)i / (float)steps * length;
			if (shape.splines[index].closed)
			{
				t = Mathf.Repeat(t, 1f);
			}
			Vector3 vector2 = mat.MultiplyPoint3x4(shape.splines[index].InterpCurve3D(t, shape.normalizedInterp, ref k) + off + lsection.offset);
			vector2.x *= lsection.scale.x;
			vector2.y *= lsection.scale.y;
			vector2.z *= lsection.scale.z;
			verts.Add(vector2);
			if (physuv)
			{
				if (i > 0)
				{
					num += Vector3.Distance(vector2, b);
				}
				b = vector2;
				zero.x = num / width;
			}
			else
			{
				zero.x = t;
			}
			uvs.Add(zero);
		}
		evert = verts.Count - 1;
		zero.y = start + length;
		lsection.crossverts = verts.ToArray();
		lsection.crossuvs = uvs.ToArray();
		lsection.crosssize = MegaUtils.Extents(lsection.crossverts, out lsection.crossmin, out lsection.crossmax);
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		if (layerPath == null || layerPath.splines == null || layerPath.splines.Count == 0)
		{
			return false;
		}
		float loftLength = layerPath.splines[curve].length * pathLength;
		LoftLength = loftLength;
		ease.SetEasing(easeType);
		locup = loft.up;
		float width = 1f;
		if (sections.Count > 0)
		{
			width = sections[0].shape.splines[sections[0].curve].length;
		}
		for (int i = 0; i < sections.Count; i++)
		{
			BuildPolyShape(off: (!sections[i].snap) ? Vector3.zero : (sections[0].shape.splines[sections[0].curve].knots[0].p - sections[i].shape.splines[sections[i].curve].knots[0].p), lsection: sections[i], steps: CrossSteps, width: width);
		}
		if (useStepsPerKnotPath)
		{
			ActualPathSteps = (sections.Count - 1) * stepsPerKnotPath + 1;
		}
		else
		{
			ActualPathSteps = PathSteps + 1;
		}
		if (useStepsPerKnotCross)
		{
			ActualCrossVerts = sections[0].crossverts.Length;
		}
		else
		{
			ActualCrossVerts = CrossSteps + 1;
		}
		int num = ActualCrossVerts * ActualPathSteps;
		if (loftverts == null || num != loftverts.Length)
		{
			loftverts = new Vector3[num];
		}
		if (loftuvs == null || num != loftuvs.Length)
		{
			loftuvs = new Vector2[num];
		}
		int num2 = (ActualCrossVerts - 1) * 2 * (ActualPathSteps - 1) * 3;
		if (lofttris == null || num2 != lofttris.Length)
		{
			lofttris = new int[num2];
		}
		CalcCaps();
		return true;
	}

	public int GetSection(float alpha, out float lerp)
	{
		if (sections.Count < 2)
		{
			lerp = alpha;
			return 0;
		}
		int num = 0;
		for (num = 0; num < sections.Count - 1 && !(alpha < sections[num + 1].alpha); num++)
		{
		}
		if (num == sections.Count - 1)
		{
			lerp = 1f;
			num--;
		}
		else
		{
			lerp = (alpha - sections[num].alpha) / (sections[num + 1].alpha - sections[num].alpha);
		}
		return num;
	}

	public Matrix4x4 GetDeformMat(MegaSpline spline, float alpha, bool interp)
	{
		int k = -1;
		Vector3 p = spline.InterpCurve3D(alpha, interp, ref k);
		alpha += 0.01f;
		if (spline.closed)
		{
			alpha %= 1f;
		}
		Vector3 forward = spline.InterpCurve3D(alpha, interp, ref k);
		forward.x -= p.x;
		forward.y -= p.y;
		forward.z -= p.z;
		forward.y *= PathTeeter;
		MegaMatrix.SetTR(ref wtm1, p, Quaternion.LookRotation(forward, locup));
		return wtm1;
	}

	public Matrix4x4 GetDeformMatNewMethod(MegaSpline spline, float alpha, bool interp, ref Vector3 lastup)
	{
		int k = -1;
		Vector3 p = spline.InterpCurve3D(alpha, interp, ref k);
		alpha += 0.01f;
		if (spline.closed)
		{
			alpha %= 1f;
		}
		Vector3 forward = spline.InterpCurve3D(alpha, interp, ref k);
		Vector3 vector = default(Vector3);
		forward.x = (vector.x = forward.x - p.x);
		forward.y = (vector.y = forward.y - p.y);
		forward.z = (vector.z = forward.z - p.z);
		forward.y *= PathTeeter;
		MegaMatrix.SetTR(ref wtm1, p, Quaternion.LookRotation(forward, lastup));
		vector = vector.normalized;
		Vector3 lhs = Vector3.Cross(vector, lastup);
		lastup = Vector3.Cross(lhs, vector);
		return wtm1;
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		if (Lock)
		{
			return triindex;
		}
		if (layerPath == null || layerPath.splines == null || layerPath.splines.Count == 0)
		{
			return triindex;
		}
		if (sections.Count < 2)
		{
			return triindex;
		}
		Vector2 zero = Vector2.zero;
		Vector3 zero2 = Vector3.zero;
		int actualCrossVerts = ActualCrossVerts;
		float lerp = 0f;
		Matrix4x4 matrix4x = Matrix4x4.identity;
		if (SnapToPath)
		{
			matrix4x = layerPath.transform.localToWorldMatrix;
		}
		Matrix4x4 mat = Matrix4x4.identity;
		Vector3 vector = Vector2.one;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		bool closed = layerPath.splines[curve].closed;
		float num4 = pathStart;
		if (UVOrigin == MegaLoftUVOrigin.SplineStart)
		{
			num4 = 0f;
		}
		Vector3 lastup = locup;
		for (int i = 0; i < PathSteps + 1; i++)
		{
			float num5 = (float)i / (float)PathSteps;
			float num6 = pathStart + pathLength * num5;
			if (closed)
			{
				num6 = Mathf.Repeat(num6, 1f);
			}
			if (useScaleXCrv)
			{
				vector.x = scaleCrvX.Evaluate(num6);
			}
			if (useScaleYCrv)
			{
				vector.y = scaleCrvY.Evaluate(num6);
			}
			Matrix4x4 matrix4x2;
			if (!useTwistCrv)
			{
				matrix4x2 = ((frameMethod != MegaFrameMethod.Old) ? (matrix4x * GetDeformMatNewMethod(layerPath.splines[curve], num6, layerPath.normalizedInterp, ref lastup)) : (matrix4x * GetDeformMat(layerPath.splines[curve], num6, layerPath.normalizedInterp)));
			}
			else
			{
				float num7 = twistCrv.Evaluate(num6);
				float twist = layerPath.splines[curve].GetTwist(num6);
				MegaShapeUtils.RotateZ(ref mat, (float)Math.PI / 180f * (num7 - twist));
				matrix4x2 = ((frameMethod != MegaFrameMethod.Old) ? (matrix4x * GetDeformMatNewMethod(layerPath.splines[curve], num6, layerPath.normalizedInterp, ref lastup) * mat) : (matrix4x * GetDeformMat(layerPath.splines[curve], num6, layerPath.normalizedInterp) * mat));
			}
			int section = GetSection(num6, out lerp);
			lerp = ease.easing(0f, 1f, lerp);
			MegaLoftSection megaLoftSection = sections[section];
			MegaLoftSection megaLoftSection2 = sections[section + 1];
			if (useOffsetX)
			{
				num = offsetCrvX.Evaluate(num6);
			}
			if (useOffsetY)
			{
				num2 = offsetCrvY.Evaluate(num6);
			}
			if (useOffsetZ)
			{
				num3 = offsetCrvZ.Evaluate(num6);
			}
			if (planaruv)
			{
				float num8 = 1f / layerPath.splines[curve].length;
				Matrix4x4 matrix4x3 = Matrix4x4.TRS(new Vector3(UVOffset.x, 0f, UVOffset.y), Quaternion.Euler(UVRotate.x, UVRotate.y, 0f), new Vector3(num8 * UVScale.x, 1f, num8 * UVScale.y));
				for (int j = 0; j < megaLoftSection.crossverts.Length; j++)
				{
					zero2 = Vector3.Lerp(megaLoftSection.crossverts[j], megaLoftSection2.crossverts[j], lerp);
					if (useScaleXCrv)
					{
						zero2.x *= vector.x;
					}
					if (useScaleYCrv)
					{
						zero2.y *= vector.y;
					}
					zero2.x += num;
					zero2.y += num2;
					zero2.z += num3;
					zero2 = matrix4x2.MultiplyPoint3x4(zero2);
					zero2 += offset;
					int num9 = i * actualCrossVerts + j;
					loftverts[num9] = zero2;
					zero2.y = 0f;
					zero2 = matrix4x3.MultiplyPoint(zero2);
					loftuvs[num9].x = zero2.x;
					loftuvs[num9].y = zero2.z;
				}
				continue;
			}
			for (int k = 0; k < megaLoftSection.crossverts.Length; k++)
			{
				zero2 = Vector3.Lerp(megaLoftSection.crossverts[k], megaLoftSection2.crossverts[k], lerp);
				if (useScaleXCrv)
				{
					zero2.x *= vector.x;
				}
				if (useScaleYCrv)
				{
					zero2.y *= vector.y;
				}
				zero2.x += num;
				zero2.y += num2;
				zero2.z += num3;
				zero2 = matrix4x2.MultiplyPoint3x4(zero2);
				zero2 += offset;
				int num10 = i * actualCrossVerts + k;
				loftverts[num10] = zero2;
				zero = Vector3.Lerp(megaLoftSection.crossuvs[k], megaLoftSection2.crossuvs[k], lerp);
				zero.y = num4 + num5;
				zero.x *= UVScale.x;
				zero.y *= UVScale.y;
				zero.x += UVOffset.x;
				zero.y += UVOffset.y;
				loftuvs[num10] = zero;
			}
		}
		int num11 = 0;
		int num12 = triindex;
		if (flip)
		{
			for (int l = 0; l < ActualPathSteps - 1; l++)
			{
				int num13 = l * ActualCrossVerts + num12;
				for (int m = 0; m < ActualCrossVerts - 1; m++)
				{
					lofttris[num11] = num13;
					lofttris[num11 + 1] = num13 + actualCrossVerts;
					lofttris[num11 + 2] = num13 + actualCrossVerts + 1;
					lofttris[num11 + 3] = num13 + actualCrossVerts + 1;
					lofttris[num11 + 4] = num13 + 1;
					lofttris[num11 + 5] = num13;
					num11 += 6;
					num13++;
				}
			}
		}
		else
		{
			for (int n = 0; n < ActualPathSteps - 1; n++)
			{
				int num14 = n * ActualCrossVerts + num12;
				for (int num15 = 0; num15 < ActualCrossVerts - 1; num15++)
				{
					lofttris[num11 + 2] = num14;
					lofttris[num11 + 1] = num14 + actualCrossVerts;
					lofttris[num11] = num14 + actualCrossVerts + 1;
					lofttris[num11 + 5] = num14 + actualCrossVerts + 1;
					lofttris[num11 + 4] = num14 + 1;
					lofttris[num11 + 3] = num14;
					num11 += 6;
					num14++;
				}
			}
		}
		num12 = triindex + loftverts.Length;
		if (capStart)
		{
			Matrix4x4 matrix4x4 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, capStartUVRot), Vector3.one);
			for (int num16 = 0; num16 < capStartVerts.Length; num16++)
			{
				Vector3 vector2 = loftverts[num16];
				capStartVerts[num16] = vector2;
				Vector3 v = crossverts[num16];
				v.y = vector2.y;
				v = matrix4x4.MultiplyPoint(v);
				capStartUVS[num16].x = v.x * capStartUVScale.x + capStartUVOffset.x;
				capStartUVS[num16].y = v.y * capStartUVScale.y + capStartUVOffset.y;
			}
			if (capflip)
			{
				for (int num17 = 0; num17 < capfaces.Count; num17 += 3)
				{
					capStartTris[num17 + 2] = capfaces[num17] + num12;
					capStartTris[num17 + 1] = capfaces[num17 + 1] + num12;
					capStartTris[num17] = capfaces[num17 + 2] + num12;
				}
			}
			else
			{
				for (int num18 = 0; num18 < capfaces.Count; num18 += 3)
				{
					capStartTris[num18] = capfaces[num18] + num12;
					capStartTris[num18 + 1] = capfaces[num18 + 1] + num12;
					capStartTris[num18 + 2] = capfaces[num18 + 2] + num12;
				}
			}
			num11 += capfaces.Count;
			num12 += capStartVerts.Length;
		}
		if (capEnd)
		{
			Matrix4x4 matrix4x5 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, capEndUVRot), Vector3.one);
			int num19 = loftverts.Length - capEndVerts.Length;
			for (int num20 = 0; num20 < capEndVerts.Length; num20++)
			{
				Vector3 vector3 = loftverts[num19 + num20];
				capEndVerts[num20] = vector3;
				Vector3 v2 = crossvertsend[num20];
				v2.y = vector3.y;
				v2 = matrix4x5.MultiplyPoint(v2);
				capEndUVS[num20].x = v2.x * capEndUVScale.x + capEndUVOffset.x;
				capEndUVS[num20].y = v2.y * capEndUVScale.y + capEndUVOffset.y;
			}
			if (capflip)
			{
				for (int num21 = 0; num21 < capfacesend.Count; num21 += 3)
				{
					capEndTris[num21] = capfacesend[num21] + num12;
					capEndTris[num21 + 1] = capfacesend[num21 + 1] + num12;
					capEndTris[num21 + 2] = capfacesend[num21 + 2] + num12;
				}
			}
			else
			{
				for (int num22 = 0; num22 < capfacesend.Count; num22 += 3)
				{
					capEndTris[num22 + 2] = capfacesend[num22] + num12;
					capEndTris[num22 + 1] = capfacesend[num22 + 1] + num12;
					capEndTris[num22] = capfacesend[num22 + 2] + num12;
				}
			}
			num11 += capfacesend.Count;
		}
		return triindex + num11;
	}

	private void CalcCaps()
	{
		if (capStart)
		{
			GetSectionVerts(pathStart, ref crossverts);
			int num = sections[0].crossverts.Length;
			capfaces = MegaShapeTriangulator.Triangulate(crossverts, ref capfaces);
			if (capStartVerts == null || num != capStartVerts.Length)
			{
				capStartVerts = new Vector3[num];
				capStartUVS = new Vector2[num];
			}
			if (capStartTris == null || capStartTris.Length != capfaces.Count)
			{
				capStartTris = new int[capfaces.Count];
			}
		}
		if (capEnd)
		{
			GetSectionVerts(pathStart + pathLength, ref crossvertsend);
			int num2 = sections[0].crossverts.Length;
			capfacesend = MegaShapeTriangulator.Triangulate(crossvertsend, ref capfacesend);
			if (capEndVerts == null || num2 != capEndVerts.Length)
			{
				capEndVerts = new Vector3[num2];
				capEndUVS = new Vector2[num2];
			}
			if (capEndTris == null || capEndTris.Length != capfacesend.Count)
			{
				capEndTris = new int[capfacesend.Count];
			}
		}
	}

	private void GetSectionVerts(float pathalpha, ref Vector3[] verts)
	{
		float lerp = 0f;
		int section = GetSection(pathalpha, out lerp);
		lerp = ease.easing(0f, 1f, lerp);
		MegaLoftSection megaLoftSection = sections[section];
		MegaLoftSection megaLoftSection2 = sections[section + 1];
		if (verts == null || verts.Length != megaLoftSection.crossverts.Length)
		{
			verts = new Vector3[megaLoftSection.crossverts.Length];
		}
		for (int i = 0; i < megaLoftSection.crossverts.Length; i++)
		{
			Vector3 vector = Vector3.Lerp(megaLoftSection.crossverts[i], megaLoftSection2.crossverts[i], lerp);
			verts[i] = vector;
		}
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerComplex to = go.AddComponent<MegaLoftLayerComplex>();
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

	public override Vector3 GetPos1(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Repeat(ca, 1f);
		a = Mathf.Repeat(a, 1f);
		float num = (float)(ActualCrossVerts - 1) * ca;
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
				num4 = ActualPathSteps - 2;
				num6 = ActualPathSteps - 1;
				num5 = (a - 1f) * GetLength(loft);
				flag = true;
			}
			else
			{
				float num7 = (float)(ActualPathSteps - 1) * a;
				num4 = (int)num7;
				num5 = num7 - (float)num4;
				num6 = num4 + 1;
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);
			float num8 = (float)(ActualPathSteps - 1) * a;
			num4 = (int)num8;
			num5 = num8 - (float)num4;
			num6 = num4 + 1;
		}
		Vector3 vector = loftverts[num4 * ActualCrossVerts + num2];
		Vector3 vector2 = loftverts[num4 * ActualCrossVerts + num3];
		Vector3 vector3 = loftverts[num6 * ActualCrossVerts + num2];
		Vector3 to = loftverts[num6 * ActualCrossVerts + num3];
		Vector3 vector4 = Vector3.Lerp(vector, vector2, t);
		Vector3 vector5 = Vector3.Lerp(vector3, to, t);
		Vector3 vector6 = vector5 - vector4;
		float num9 = num5 + at;
		p.x = vector4.x + vector6.x * num9;
		p.y = vector4.y + vector6.y * num9;
		p.z = vector4.z + vector6.z * num9;
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
}
