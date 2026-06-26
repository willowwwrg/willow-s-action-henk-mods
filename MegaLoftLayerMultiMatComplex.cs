using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerMultiMatComplex : MegaLoftLayerMultiMat
{
	public List<MegaLoftSection> loftsections = new List<MegaLoftSection>();

	public MegaLoftEaseType easeType;

	public MegaLoftEase ease = new MegaLoftEase();

	public int PathSteps = 16;

	public float PathTeeter = 1f;

	private Matrix4x4 wtm1;

	private Vector3 locup = Vector3.up;

	public bool advancedParams;

	public float handlesize = 1f;

	public bool showsections;

	public override void Notify(MegaSpline spline, int reason)
	{
		if ((bool)layerPath && layerPath.splines[curve] == spline)
		{
			MegaShapeLoft component = GetComponent<MegaShapeLoft>();
			component.rebuild = true;
			component.BuildMeshFromLayersNew();
			return;
		}
		for (int i = 0; i < loftsections.Count; i++)
		{
			if ((bool)loftsections[i].shape && loftsections[i].shape.splines[loftsections[i].curve] == spline)
			{
				MegaShapeLoft component2 = GetComponent<MegaShapeLoft>();
				component2.rebuild = true;
				component2.BuildMeshFromLayersNew();
				break;
			}
		}
	}

	public override int GetHelp()
	{
		return 5218;
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = megaLoftSection.meshsections[i];
			Array.Copy(megaMeshSection.verts.ToArray(), 0, verts, offset, megaMeshSection.verts.Count);
			Array.Copy(megaMeshSection.uvs.ToArray(), 0, uvs, offset, megaMeshSection.uvs.Count);
			offset += megaMeshSection.verts.Count;
		}
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = megaLoftSection.meshsections[i];
			Array.Copy(megaMeshSection.verts.ToArray(), 0, verts, offset, megaMeshSection.verts.Count);
			Array.Copy(megaMeshSection.uvs.ToArray(), 0, uvs, offset, megaMeshSection.uvs.Count);
			Array.Copy(megaMeshSection.cols.ToArray(), 0, cols, offset, megaMeshSection.cols.Count);
			offset += megaMeshSection.verts.Count;
		}
	}

	public override int NumVerts()
	{
		int num = 0;
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			num += megaLoftSection.meshsections[i].cverts.Count * crosses;
		}
		return num;
	}

	public override Material GetMaterial(int i)
	{
		MegaLoftSection megaLoftSection = loftsections[0];
		if (i >= 0 && i < megaLoftSection.meshsections.Count)
		{
			int mat = megaLoftSection.meshsections[i].mat;
			if (mat >= 0 && mat < sections.Count)
			{
				return sections[megaLoftSection.meshsections[i].mat].mat;
			}
		}
		return null;
	}

	public override int[] GetTris(int i)
	{
		return loftsections[0].meshsections[i].tris.ToArray();
	}

	public override int NumMaterials()
	{
		return loftsections[0].meshsections.Count;
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)layerPath && layerPath.splines != null && loftsections.Count > 0 && sections.Count > 0)
		{
			for (int i = 0; i < loftsections.Count; i++)
			{
				if (loftsections[i].shape == null)
				{
					return false;
				}
			}
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

	public void FindSections(MegaLoftSection lsection, MegaSpline spline)
	{
		if (spline == null)
		{
			return;
		}
		lsection.meshsections.Clear();
		int num = spline.knots[0].id - 1;
		MegaMeshSection megaMeshSection = null;
		for (int i = 0; i < spline.knots.Count; i++)
		{
			if (spline.knots[i].id != num)
			{
				num = spline.knots[i].id;
				if (megaMeshSection != null)
				{
					megaMeshSection.lastknot = i;
				}
				megaMeshSection = new MegaMeshSection();
				megaMeshSection.firstknot = i;
				megaMeshSection.mat = FindMaterial(num);
				lsection.meshsections.Add(megaMeshSection);
			}
		}
		lsection.meshsections[lsection.meshsections.Count - 1].lastknot = spline.knots.Count - 1;
	}

	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		MegaMeshSection megaMeshSection = null;
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			megaMeshSection = megaLoftSection.meshsections[i];
			if (ca >= megaMeshSection.castart && ca <= megaMeshSection.caend)
			{
				break;
			}
		}
		ca = (ca - megaMeshSection.castart) / (megaMeshSection.caend - megaMeshSection.castart);
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		float num = (float)(megaMeshSection.cverts.Count - 1) * ca;
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
		Vector3 vector = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num2];
		Vector3 vector2 = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num3];
		Vector3 vector3 = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num2];
		Vector3 to = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num3];
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

	public override Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		MegaMeshSection megaMeshSection = null;
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			megaMeshSection = megaLoftSection.meshsections[i];
			if (ca >= megaMeshSection.castart && ca <= megaMeshSection.caend)
			{
				break;
			}
		}
		ca = (ca - megaMeshSection.castart) / (megaMeshSection.caend - megaMeshSection.castart);
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		float num = (float)(megaMeshSection.cverts.Count - 1) * ca;
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
		Vector3 vector = megaMeshSection.verts[num5 * megaMeshSection.cverts.Count + num2];
		Vector3 vector2 = megaMeshSection.verts[num5 * megaMeshSection.cverts.Count + num4];
		Vector3 result = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num2];
		Vector3 vector3 = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num4];
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

	public override Vector3 GetPosAndUp(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		MegaMeshSection megaMeshSection = null;
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			megaMeshSection = megaLoftSection.meshsections[i];
			if (ca >= megaMeshSection.castart && ca <= megaMeshSection.caend)
			{
				break;
			}
		}
		ca = (ca - megaMeshSection.castart) / (megaMeshSection.caend - megaMeshSection.castart);
		ca = Mathf.Clamp(ca, 0f, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);
		float num = (float)(megaMeshSection.cverts.Count - 1) * ca;
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
		Vector3 vector = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num2];
		Vector3 vector2 = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num3];
		Vector3 vector3 = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num2];
		Vector3 to = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num3];
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

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1f);
		a = Mathf.Repeat(a, 1f);
		MegaMeshSection megaMeshSection = null;
		MegaLoftSection megaLoftSection = loftsections[0];
		for (int i = 0; i < megaLoftSection.meshsections.Count; i++)
		{
			megaMeshSection = megaLoftSection.meshsections[i];
			if (ca >= megaMeshSection.castart && ca <= megaMeshSection.caend)
			{
				break;
			}
		}
		ca = (ca - megaMeshSection.castart) / (megaMeshSection.caend - megaMeshSection.castart);
		float num = (float)(megaMeshSection.cverts.Count - 1) * ca;
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
		Vector3 vector = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num2];
		Vector3 to = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num3];
		Vector3 vector2 = megaMeshSection.verts[num5 * megaMeshSection.cverts.Count + num2];
		Vector3 to2 = megaMeshSection.verts[num5 * megaMeshSection.cverts.Count + num3];
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

	private void BuildPolyShape(MegaLoftSection lsection, int steps, Vector3 off1, float width)
	{
		int index = lsection.curve;
		int k = -1;
		Matrix4x4 mat = Matrix4x4.identity;
		MegaShape shape = lsection.shape;
		MegaMatrix.Translate(ref mat, pivot);
		Vector3 vector = crossRot + lsection.rot;
		MegaMatrix.Rotate(ref mat, new Vector3((float)Math.PI / 180f * vector.x, (float)Math.PI / 180f * vector.y, (float)Math.PI / 180f * vector.z));
		svert = verts.Count;
		_ = crossDist;
		svert = verts.Count;
		float num = crossStart;
		float num2 = crossStart + crossEnd;
		if (num < 0f)
		{
			num = 0f;
		}
		if (num2 > 1f)
		{
			num2 = 1f;
		}
		MegaSpline megaSpline = shape.splines[index];
		if (megaSpline.closed && num < 0f)
		{
			num += 1f;
		}
		Vector3 vector2 = off1;
		if (snap)
		{
			vector2 = megaSpline.knots[0].p - megaSpline.knots[0].p;
		}
		if (megaSpline.closed)
		{
			num = Mathf.Repeat(num, 1f);
		}
		lsection.meshsections.Clear();
		FindSections(lsection, megaSpline);
		Vector3 vector3 = mat.MultiplyPoint3x4(megaSpline.InterpCurve3D(num, shape.normalizedInterp, ref k) + vector2);
		for (int i = 0; i < lsection.meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = lsection.meshsections[i];
			MegaMaterialSection megaMaterialSection = sections[megaMeshSection.mat];
			int firstknot = megaMeshSection.firstknot;
			int lastknot = megaMeshSection.lastknot;
			float length = megaSpline.knots[firstknot].length;
			float length2 = megaSpline.knots[lastknot].length;
			float castart = length / megaSpline.length;
			float caend = length2 / megaSpline.length;
			megaMeshSection.castart = castart;
			megaMeshSection.caend = caend;
			for (int j = firstknot; j < lastknot; j++)
			{
				for (int l = 0; l <= megaMaterialSection.steps; l++)
				{
					float t = (float)l / (float)megaMaterialSection.steps;
					vector3 = megaSpline.knots[j].Interpolate(t, megaSpline.knots[j + 1]) + lsection.offset;
					vector3.x *= lsection.scale.x;
					vector3.y *= lsection.scale.y;
					vector3.z *= lsection.scale.z;
					vector3 = mat.MultiplyPoint3x4(vector3 + vector2);
					megaMeshSection.cverts.Add(vector3);
				}
			}
		}
		for (int m = 0; m < lsection.meshsections.Count; m++)
		{
			MegaMeshSection megaMeshSection2 = lsection.meshsections[m];
			megaMeshSection2.len = 0f;
			Vector2 zero = Vector2.zero;
			megaMeshSection2.cuvs.Add(zero);
			megaMeshSection2.ccols.Add(Color.white);
			for (int n = 1; n < megaMeshSection2.cverts.Count; n++)
			{
				megaMeshSection2.len += Vector3.Distance(megaMeshSection2.cverts[n], megaMeshSection2.cverts[n - 1]);
			}
			for (int num3 = 1; num3 < megaMeshSection2.cverts.Count; num3++)
			{
				zero.y += Vector3.Distance(megaMeshSection2.cverts[num3], megaMeshSection2.cverts[num3 - 1]) / megaMeshSection2.len;
				megaMeshSection2.cuvs.Add(zero);
				megaMeshSection2.ccols.Add(Color.white);
			}
		}
		if (megaSpline.closed)
		{
			num = Mathf.Repeat(num2, 1f);
		}
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		for (int num4 = 0; num4 < lsection.meshsections.Count; num4++)
		{
			MegaMeshSection megaMeshSection3 = lsection.meshsections[num4];
			if (megaMeshSection3.cverts.Count > 1)
			{
				for (int num5 = 0; num5 < megaMeshSection3.cverts.Count; num5++)
				{
					zero3 = ((num5 >= megaMeshSection3.cverts.Count - 1) ? (megaMeshSection3.cverts[num5] - megaMeshSection3.cverts[num5 - 1]) : (megaMeshSection3.cverts[num5 + 1] - megaMeshSection3.cverts[num5]));
					zero2.x = 0f - zero3.y;
					zero2.y = zero3.x;
					zero2.z = 0f;
					megaMeshSection3.cnorms.Add(zero2);
				}
			}
		}
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
		if (loftsections.Count > 0)
		{
			width = loftsections[0].shape.splines[loftsections[0].curve].length;
		}
		for (int i = 0; i < loftsections.Count; i++)
		{
			BuildPolyShape(off1: (!loftsections[i].snap) ? Vector3.zero : (loftsections[0].shape.splines[loftsections[0].curve].knots[0].p - loftsections[i].shape.splines[loftsections[i].curve].knots[0].p), lsection: loftsections[i], steps: 0, width: width);
		}
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
		if (base.enabled)
		{
			uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate.x, UVRotate.y, 0f), Vector3.one);
		}
	}

	public int GetSection(float alpha, out float lerp)
	{
		if (loftsections.Count < 2)
		{
			lerp = alpha;
			return 0;
		}
		int num = 0;
		for (num = 0; num < loftsections.Count - 1 && !(alpha < loftsections[num + 1].alpha); num++)
		{
		}
		if (num == loftsections.Count - 1)
		{
			lerp = 1f;
			num--;
		}
		else
		{
			lerp = (alpha - loftsections[num].alpha) / (loftsections[num + 1].alpha - loftsections[num].alpha);
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
		if (loftsections.Count < 2)
		{
			return triindex;
		}
		Vector2 zero = Vector2.zero;
		Vector3 zero2 = Vector3.zero;
		float lerp = 0f;
		Matrix4x4 identity = Matrix4x4.identity;
		Matrix4x4 mat = Matrix4x4.identity;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		MegaSpline megaSpline = layerPath.splines[curve];
		float num4 = pathStart;
		if (UVOrigin == MegaLoftUVOrigin.SplineStart)
		{
			num4 = 0f;
		}
		Vector3 lastup = locup;
		Color item = color;
		float num5 = 0f;
		for (int i = 0; i < crosses; i++)
		{
			float num6 = (float)i / (float)(crosses - 1);
			float num7 = pathStart + pathLength * num6;
			Matrix4x4 matrix4x;
			if (!useTwistCrv)
			{
				matrix4x = ((frameMethod != MegaFrameMethod.Old) ? (identity * GetDeformMatNewMethod(layerPath.splines[curve], num7, layerPath.normalizedInterp, ref lastup)) : (identity * GetDeformMat(layerPath.splines[curve], num7, layerPath.normalizedInterp)));
			}
			else
			{
				float num8 = twistCrv.Evaluate(num7);
				float twist = layerPath.splines[curve].GetTwist(num7);
				MegaShapeUtils.RotateZ(ref mat, (float)Math.PI / 180f * (num8 - twist));
				matrix4x = ((frameMethod != MegaFrameMethod.Old) ? (identity * GetDeformMatNewMethod(layerPath.splines[curve], num7, layerPath.normalizedInterp, ref lastup) * mat) : (identity * GetDeformMat(layerPath.splines[curve], num7, layerPath.normalizedInterp) * mat));
			}
			int section = GetSection(num7, out lerp);
			lerp = ease.easing(0f, 1f, lerp);
			MegaLoftSection megaLoftSection = loftsections[section];
			MegaLoftSection megaLoftSection2 = loftsections[section + 1];
			if (useOffsetX)
			{
				num = offsetCrvX.Evaluate(num7);
			}
			if (useOffsetY)
			{
				num2 = offsetCrvY.Evaluate(num7);
			}
			if (useOffsetZ)
			{
				num3 = offsetCrvZ.Evaluate(num7);
			}
			for (int j = 0; j < megaLoftSection.meshsections.Count; j++)
			{
				MegaMeshSection megaMeshSection = loftsections[0].meshsections[j];
				MegaMaterialSection megaMaterialSection = sections[megaMeshSection.mat];
				if (!megaMaterialSection.Enabled)
				{
					continue;
				}
				MegaMeshSection megaMeshSection2 = megaLoftSection.meshsections[j];
				MegaMeshSection megaMeshSection3 = megaLoftSection2.meshsections[j];
				if (loft.useColors)
				{
					num5 = ((megaMaterialSection.colmode != MegaLoftColMode.Loft) ? num7 : num6);
					num5 = Mathf.Repeat(num5 + megaMaterialSection.coloffset, 1f);
					item.r = megaMaterialSection.colR.Evaluate(num5);
					item.g = megaMaterialSection.colG.Evaluate(num5);
					item.b = megaMaterialSection.colB.Evaluate(num5);
					item.a = megaMaterialSection.colA.Evaluate(num5);
				}
				for (int k = 0; k < megaMeshSection2.cverts.Count; k++)
				{
					zero2 = Vector3.Lerp(megaMeshSection2.cverts[k], megaMeshSection3.cverts[k], lerp);
					zero2.x += num;
					zero2.y += num2;
					zero2.z += num3;
					zero2 = matrix4x.MultiplyPoint3x4(zero2);
					zero2 += offset;
					if (conform)
					{
						megaMeshSection.verts1.Add(zero2);
					}
					else
					{
						megaMeshSection.verts.Add(zero2);
					}
					zero.y = Mathf.Lerp(megaMeshSection2.cuvs[k].y, megaMeshSection3.cuvs[k].y, lerp);
					zero.x = num6 - num4;
					if (megaMaterialSection.physuv)
					{
						zero.x *= megaSpline.length;
						zero.y *= megaMeshSection.len;
					}
					else if (megaMaterialSection.uvcalcy)
					{
						zero.x = num6 * megaSpline.length / megaMeshSection.len - num4;
					}
					if (megaMaterialSection.swapuv)
					{
						float x = zero.x;
						zero.x = zero.y;
						zero.y = x;
					}
					zero.x *= megaMaterialSection.UVScale.x;
					zero.y *= megaMaterialSection.UVScale.y;
					zero.x += megaMaterialSection.UVOffset.x;
					zero.y += megaMaterialSection.UVOffset.y;
					megaMeshSection.uvs.Add(zero);
					if (loft.useColors)
					{
						megaMeshSection.cols.Add(item);
					}
				}
			}
		}
		int num9 = triindex;
		int num10 = 0;
		if (base.enabled)
		{
			if (flip)
			{
				for (int l = 0; l < loftsections[0].meshsections.Count; l++)
				{
					MegaMeshSection megaMeshSection4 = loftsections[0].meshsections[l];
					if (!sections[megaMeshSection4.mat].Enabled)
					{
						continue;
					}
					for (int m = 0; m < crosses - 1; m++)
					{
						for (int n = 0; n < megaMeshSection4.cverts.Count - 1; n++)
						{
							megaMeshSection4.tris.Add(num9 + n);
							megaMeshSection4.tris.Add(num9 + n + 1);
							megaMeshSection4.tris.Add(num9 + n + 1 + megaMeshSection4.cverts.Count);
							megaMeshSection4.tris.Add(num9 + n);
							megaMeshSection4.tris.Add(num9 + n + 1 + megaMeshSection4.cverts.Count);
							megaMeshSection4.tris.Add(num9 + n + megaMeshSection4.cverts.Count);
							num10 += 6;
						}
						num9 += megaMeshSection4.cverts.Count;
					}
					num9 += megaMeshSection4.cverts.Count;
				}
			}
			else
			{
				for (int num11 = 0; num11 < loftsections[0].meshsections.Count; num11++)
				{
					MegaMeshSection megaMeshSection5 = loftsections[0].meshsections[num11];
					if (!sections[megaMeshSection5.mat].Enabled)
					{
						continue;
					}
					for (int num12 = 0; num12 < crosses - 1; num12++)
					{
						for (int num13 = 0; num13 < megaMeshSection5.cverts.Count - 1; num13++)
						{
							megaMeshSection5.tris.Add(num9 + num13 + 1 + megaMeshSection5.cverts.Count);
							megaMeshSection5.tris.Add(num9 + num13 + 1);
							megaMeshSection5.tris.Add(num9 + num13);
							megaMeshSection5.tris.Add(num9 + num13 + megaMeshSection5.cverts.Count);
							megaMeshSection5.tris.Add(num9 + num13 + 1 + megaMeshSection5.cverts.Count);
							megaMeshSection5.tris.Add(num9 + num13);
							num10 += 6;
						}
						num9 += megaMeshSection5.cverts.Count;
					}
					num9 += megaMeshSection5.cverts.Count;
				}
			}
		}
		if (conform)
		{
			CalcBounds();
			DoConform(loft);
		}
		return triindex + num10;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerMultiMatComplex megaLoftLayerMultiMatComplex = go.AddComponent<MegaLoftLayerMultiMatComplex>();
		Copy(this, megaLoftLayerMultiMatComplex);
		for (int i = 0; i < megaLoftLayerMultiMatComplex.loftsections.Count; i++)
		{
			megaLoftLayerMultiMatComplex.loftsections[i].meshsections = new List<MegaMeshSection>();
		}
		megaLoftLayerMultiMatComplex.meshsections = new List<MegaMeshSection>();
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

	private void CalcBounds()
	{
		conminz = float.MaxValue;
		for (int i = 0; i < loftsections[0].meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = loftsections[0].meshsections[i];
			for (int j = 0; j < megaMeshSection.verts1.Count; j++)
			{
				if (megaMeshSection.verts1[j].y < conminz)
				{
					conminz = megaMeshSection.verts1[j].y;
				}
			}
		}
	}

	private void InitConform()
	{
		for (int i = 0; i < loftsections[0].meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = loftsections[0].meshsections[i];
			megaMeshSection.offsets = new float[megaMeshSection.verts1.Count];
			megaMeshSection.last = new float[megaMeshSection.verts1.Count];
			for (int j = 0; j < megaMeshSection.verts1.Count; j++)
			{
				megaMeshSection.offsets[j] = megaMeshSection.verts1[j].y - conminz;
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
			for (int i = 0; i < loftsections[0].meshsections.Count; i++)
			{
				MegaMeshSection megaMeshSection = loftsections[0].meshsections[i];
				for (int j = 0; j < megaMeshSection.verts1.Count; j++)
				{
					Vector3 origin = localToWorldMatrix.MultiplyPoint(megaMeshSection.verts1[j]);
					origin.y += raystartoff;
					ray.origin = origin;
					ray.direction = Vector3.down;
					megaMeshSection.verts.Add(megaMeshSection.verts1[j]);
					if (conformCollider.Raycast(ray, out var hitInfo, raydist))
					{
						Vector3 vector = inverse.MultiplyPoint(hitInfo.point);
						Vector3 value = megaMeshSection.verts[j];
						value.y = Mathf.Lerp(value.y, vector.y + megaMeshSection.offsets[j] + conformOffset, t);
						megaMeshSection.verts[j] = value;
						megaMeshSection.last[j] = value.y;
					}
					else
					{
						Vector3 origin2 = ray.origin;
						origin2.y -= raydist;
						Vector3 value2 = megaMeshSection.verts[j];
						value2.y = megaMeshSection.last[j];
						megaMeshSection.verts[j] = value2;
					}
				}
			}
			return;
		}
		for (int k = 0; k < loftsections[0].meshsections.Count; k++)
		{
			MegaMeshSection megaMeshSection2 = loftsections[0].meshsections[k];
			for (int l = 0; l < megaMeshSection2.verts1.Count; l++)
			{
				megaMeshSection2.verts.Add(megaMeshSection2.verts1[l]);
			}
		}
	}
}
