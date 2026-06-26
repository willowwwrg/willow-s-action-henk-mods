using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerMultiMat : MegaLoftLayerSimple
{
	public List<MegaMaterialSection> sections = new List<MegaMaterialSection>();

	public List<MegaMeshSection> meshsections = new List<MegaMeshSection>();

	public override int GetHelp()
	{
		return 5218;
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		for (int i = 0; i < meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = meshsections[i];
			Array.Copy(megaMeshSection.verts.ToArray(), 0, verts, offset, megaMeshSection.verts.Count);
			Array.Copy(megaMeshSection.uvs.ToArray(), 0, uvs, offset, megaMeshSection.uvs.Count);
			offset += megaMeshSection.verts.Count;
		}
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		for (int i = 0; i < meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = meshsections[i];
			Array.Copy(megaMeshSection.verts.ToArray(), 0, verts, offset, megaMeshSection.verts.Count);
			Array.Copy(megaMeshSection.uvs.ToArray(), 0, uvs, offset, megaMeshSection.uvs.Count);
			Array.Copy(megaMeshSection.cols.ToArray(), 0, cols, offset, megaMeshSection.cols.Count);
			offset += megaMeshSection.verts.Count;
		}
	}

	public override int NumVerts()
	{
		int num = 0;
		for (int i = 0; i < meshsections.Count; i++)
		{
			num += meshsections[i].cverts.Count * crosses;
		}
		return num;
	}

	public override Material GetMaterial(int i)
	{
		if (i >= 0 && i < meshsections.Count)
		{
			int mat = meshsections[i].mat;
			if (mat >= 0 && mat < sections.Count)
			{
				return sections[meshsections[i].mat].mat;
			}
		}
		return null;
	}

	public override int[] GetTris(int i)
	{
		return meshsections[i].tris.ToArray();
	}

	public override int NumMaterials()
	{
		return meshsections.Count;
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)layerSection && (bool)layerPath && layerSection.splines != null && layerPath.splines != null && sections.Count > 0)
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

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1f);
		a = Mathf.Repeat(a, 1f);
		MegaMeshSection megaMeshSection = null;
		for (int i = 0; i < meshsections.Count; i++)
		{
			megaMeshSection = meshsections[i];
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

	public override Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		MegaMeshSection megaMeshSection = null;
		for (int i = 0; i < meshsections.Count; i++)
		{
			megaMeshSection = meshsections[i];
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
		for (int i = 0; i < meshsections.Count; i++)
		{
			megaMeshSection = meshsections[i];
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

	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		MegaMeshSection megaMeshSection = null;
		for (int i = 0; i < meshsections.Count; i++)
		{
			megaMeshSection = meshsections[i];
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

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * value / 1f) - 1f) + start;
	}

	public override Vector3 GetPos1(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		MegaMeshSection megaMeshSection = null;
		for (int i = 0; i < meshsections.Count; i++)
		{
			megaMeshSection = meshsections[i];
			if (ca >= megaMeshSection.castart && ca <= megaMeshSection.caend)
			{
				break;
			}
		}
		ca = (ca - megaMeshSection.castart) / (megaMeshSection.caend - megaMeshSection.castart);
		ca = Mathf.Repeat(ca, 1f);
		a = Mathf.Repeat(a, 1f);
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
		Vector3 to = megaMeshSection.verts[num4 * megaMeshSection.cverts.Count + num3];
		Vector3 vector2 = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num2];
		Vector3 to2 = megaMeshSection.verts[num6 * megaMeshSection.cverts.Count + num3];
		Vector3 vector3 = Vector3.Lerp(vector, to, t);
		Vector3 vector4 = Vector3.Lerp(vector2, to2, t);
		Vector3 vector5 = vector4 - vector3;
		float num9 = num5 + at;
		up = Vector3.Lerp(megaMeshSection.cnorms[num2], megaMeshSection.cnorms[num3], t);
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

	public int FindMaterial(int id)
	{
		for (int i = 0; i < sections.Count; i++)
		{
			if (sections[i].id == id)
			{
				return i;
			}
		}
		return 0;
	}

	private void FindSections(MegaSpline spline)
	{
		if (spline == null)
		{
			return;
		}
		meshsections.Clear();
		int num = spline.knots[0].id - 1;
		for (int i = 0; i < spline.knots.Count; i++)
		{
			if (spline.knots[i].id != num)
			{
				num = spline.knots[i].id;
				MegaMeshSection megaMeshSection = new MegaMeshSection();
				megaMeshSection.mat = FindMaterial(num);
				meshsections.Add(megaMeshSection);
			}
		}
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		MegaShape megaShape = layerSection;
		FindSections(megaShape.splines[crosscurve]);
		Matrix4x4 mat = Matrix4x4.identity;
		MegaMatrix.Translate(ref mat, pivot);
		MegaMatrix.Rotate(ref mat, new Vector3((float)Math.PI / 180f * crossRot.x, (float)Math.PI / 180f * crossRot.y, (float)Math.PI / 180f * crossRot.z));
		if (base.enabled)
		{
			_ = crossDist;
			int k = -1;
			int k2 = -1;
			svert = verts.Count;
			Vector2 zero = Vector2.zero;
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
			MegaSpline megaSpline = megaShape.splines[crosscurve];
			if (megaSpline.closed && num < 0f)
			{
				num += 1f;
			}
			Vector3 vector = Vector3.zero;
			if (snap)
			{
				vector = megaSpline.knots[0].p - megaSpline.knots[0].p;
			}
			if (megaSpline.closed)
			{
				num = Mathf.Repeat(num, 1f);
			}
			meshsections.Clear();
			Vector3 item = mat.MultiplyPoint3x4(megaSpline.InterpCurve3D(num, megaShape.normalizedInterp, ref k2) + vector);
			int id = megaSpline.knots[k2].id;
			MegaMeshSection megaMeshSection = new MegaMeshSection();
			megaMeshSection.castart = 0f;
			float y = crossStart;
			zero.y = y;
			megaMeshSection.vertstart = 0;
			megaMeshSection.mat = FindMaterial(id);
			meshsections.Add(megaMeshSection);
			MegaMaterialSection megaMaterialSection = sections[megaMeshSection.mat];
			megaMeshSection.cverts.Add(item);
			bool flag = true;
			while (num <= num2)
			{
				if (flag)
				{
					num += megaMaterialSection.cdist;
					flag = false;
				}
				if (num > num2)
				{
					num = num2;
				}
				item = mat.MultiplyPoint3x4(megaSpline.InterpCurve3D(num, megaShape.normalizedInterp, ref k) + vector);
				if (k != k2)
				{
					while (k2 != k)
					{
						k2++;
						int num3 = k2 % megaSpline.knots.Count;
						k2 = num3;
						if (megaSpline.knots[k2].id != id)
						{
							megaMeshSection.cverts.Add(mat.MultiplyPoint3x4(megaSpline.knots[k2].p + vector));
							float castart = (megaMeshSection.caend = (megaSpline.knots[k2].length / megaSpline.length - crossStart) / (crossEnd - crossStart));
							megaMeshSection = new MegaMeshSection();
							megaMeshSection.castart = castart;
							item = mat.MultiplyPoint3x4(megaSpline.knots[k2].p + vector);
							megaMeshSection.vertstart = 0;
							id = megaSpline.knots[k2].id;
							megaMeshSection.mat = FindMaterial(id);
							meshsections.Add(megaMeshSection);
							megaMaterialSection = sections[megaMeshSection.mat];
							megaMeshSection.cverts.Add(item);
							num3 = k2 - 1;
							if (num3 < 0)
							{
								num3 = megaSpline.knots.Count - 1;
							}
							num = megaSpline.knots[num3].length / megaSpline.length;
							break;
						}
						if (megaMaterialSection.includeknots)
						{
							megaMeshSection.cverts.Add(mat.MultiplyPoint3x4(megaSpline.knots[k2].p + vector));
							continue;
						}
						if (megaSpline.knots[k].id != id)
						{
							int i;
							for (i = k2; megaSpline.knots[i].id == id && i < megaSpline.knots.Count; i++)
							{
							}
							if (i >= megaSpline.knots.Count)
							{
								i = megaSpline.knots.Count - 1;
							}
							megaMeshSection.cverts.Add(mat.MultiplyPoint3x4(megaSpline.knots[i].p + vector));
							break;
						}
						megaMeshSection.cverts.Add(item);
					}
				}
				else
				{
					megaMeshSection.cverts.Add(item);
				}
				if (num == num2)
				{
					if (megaMeshSection.cverts[megaMeshSection.cverts.Count - 1] != item)
					{
						megaMeshSection.cverts.Add(item);
					}
					break;
				}
				num += megaMaterialSection.cdist;
				if (num > num2)
				{
					num = num2;
				}
			}
			megaMeshSection.caend = 1f;
			for (int j = 0; j < meshsections.Count; j++)
			{
				MegaMeshSection megaMeshSection2 = meshsections[j];
				Vector2 zero2 = Vector2.zero;
				megaMeshSection2.cuvs.Add(zero2);
				megaMeshSection2.ccols.Add(Color.white);
				for (int l = 1; l < megaMeshSection2.cverts.Count; l++)
				{
					zero2.y += Vector3.Distance(megaMeshSection2.cverts[l], megaMeshSection2.cverts[l - 1]);
					megaMeshSection2.cuvs.Add(zero2);
					megaMeshSection2.ccols.Add(Color.white);
				}
			}
			if (megaShape.splines[crosscurve].closed)
			{
				num = Mathf.Repeat(num2, 1f);
			}
		}
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		for (int m = 0; m < meshsections.Count; m++)
		{
			MegaMeshSection megaMeshSection3 = meshsections[m];
			if (megaMeshSection3.cverts.Count > 1)
			{
				for (int n = 0; n < megaMeshSection3.cverts.Count; n++)
				{
					zero4 = ((n >= megaMeshSection3.cverts.Count - 1) ? (megaMeshSection3.cverts[n] - megaMeshSection3.cverts[n - 1]) : (megaMeshSection3.cverts[n + 1] - megaMeshSection3.cverts[n]));
					zero3.x = 0f - zero4.y;
					zero3.y = zero4.x;
					zero3.z = 0f;
					megaMeshSection3.cnorms.Add(zero3);
				}
			}
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
		Vector3 vector = Vector3.zero;
		Vector3 one = Vector3.one;
		float num2 = 1f;
		float num3 = 1f;
		Vector3 vector2 = crossmax;
		vector2.x = 0f;
		vector2.z = 0f;
		Vector3 vector3 = crossmin;
		vector3.x = 0f;
		vector3.z = 0f;
		Vector3 zero2 = Vector3.zero;
		float num4 = pathStart;
		if (UVOrigin == MegaLoftUVOrigin.SplineStart)
		{
			num4 = 0f;
		}
		Matrix4x4 mat = Matrix4x4.identity;
		Color item = color;
		Vector3 lastup = loft.up;
		float num5 = 0f;
		for (int i = 0; i < crosses; i++)
		{
			float num6 = (float)i / (float)(crosses - 1);
			float num7 = pathStart + pathLength * num6;
			zero2 = offset;
			if (useOffsetX)
			{
				zero2.x += offsetCrvX.Evaluate(num7);
			}
			if (useOffsetY)
			{
				zero2.y += offsetCrvY.Evaluate(num7);
			}
			if (useOffsetZ)
			{
				zero2.z += offsetCrvZ.Evaluate(num7);
			}
			Matrix4x4 matrix4x = ((frameMethod != MegaFrameMethod.New) ? loft.GetDeformMatNew(megaSpline, num7, interp: true, alignCross) : loft.GetDeformMatNewMethod(megaSpline, num7, interp: true, alignCross, ref lastup));
			if (useTwistCrv)
			{
				float num8 = twistCrv.Evaluate(num7) * twistAmt;
				float twist = megaSpline.GetTwist(num7);
				MegaShapeUtils.RotateZ(ref mat, (float)Math.PI / 180f * (num8 - twist));
				matrix4x *= mat;
			}
			if (useCrossScaleCrv)
			{
				float time = Mathf.Repeat(num6 + scaleoff, 1f);
				num2 = crossScaleCrv.Evaluate(time);
			}
			if (!sepscale)
			{
				num3 = num2;
			}
			else if (useCrossScaleCrvY)
			{
				float time2 = Mathf.Repeat(num6 + scaleoffY, 1f);
				num3 = crossScaleCrvY.Evaluate(time2);
			}
			one.x = crossScale.x * num2;
			one.y = crossScale.y * num3;
			Vector3 vector4 = vector2;
			vector4.y *= one.y;
			Vector3 vector5 = vector3;
			vector5.y *= one.y;
			for (int j = 0; j < meshsections.Count; j++)
			{
				MegaMeshSection megaMeshSection = meshsections[j];
				MegaMaterialSection megaMaterialSection = sections[megaMeshSection.mat];
				if (!megaMaterialSection.Enabled)
				{
					continue;
				}
				if (loft.useColors)
				{
					num5 = ((megaMaterialSection.colmode != MegaLoftColMode.Loft) ? num7 : num6);
					num5 = Mathf.Repeat(num5 + megaMaterialSection.coloffset, 1f);
					item.r = megaMaterialSection.colR.Evaluate(num5);
					item.g = megaMaterialSection.colG.Evaluate(num5);
					item.b = megaMaterialSection.colB.Evaluate(num5);
					item.a = megaMaterialSection.colA.Evaluate(num5);
				}
				for (int k = 0; k < megaMeshSection.cverts.Count; k++)
				{
					vector.x = megaMeshSection.cverts[k].x * one.x;
					vector.y = megaMeshSection.cverts[k].y * one.y;
					vector.z = megaMeshSection.cverts[k].z * one.z;
					vector = matrix4x.MultiplyPoint3x4(vector);
					zero.x = num7 - num4;
					zero.y = megaMeshSection.cuvs[k].y;
					if (megaMaterialSection.physuv)
					{
						zero.x *= megaSpline.length;
						zero.y *= megaSpline2.length;
					}
					else if (megaMaterialSection.uvcalcy)
					{
						zero.x = num7 * megaSpline.length / megaSpline2.length - num4;
					}
					if (conform)
					{
						megaMeshSection.verts1.Add(vector + zero2);
					}
					else
					{
						megaMeshSection.verts.Add(vector + zero2);
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
					num++;
				}
			}
		}
		int num9 = triindex;
		int num10 = 0;
		if (base.enabled)
		{
			if (flip)
			{
				for (int l = 0; l < meshsections.Count; l++)
				{
					MegaMeshSection megaMeshSection2 = meshsections[l];
					if (!sections[megaMeshSection2.mat].Enabled)
					{
						continue;
					}
					for (int m = 0; m < crosses - 1; m++)
					{
						for (int n = 0; n < megaMeshSection2.cverts.Count - 1; n++)
						{
							megaMeshSection2.tris.Add(num9 + n);
							megaMeshSection2.tris.Add(num9 + n + 1);
							megaMeshSection2.tris.Add(num9 + n + 1 + megaMeshSection2.cverts.Count);
							megaMeshSection2.tris.Add(num9 + n);
							megaMeshSection2.tris.Add(num9 + n + 1 + megaMeshSection2.cverts.Count);
							megaMeshSection2.tris.Add(num9 + n + megaMeshSection2.cverts.Count);
							num10 += 6;
						}
						num9 += megaMeshSection2.cverts.Count;
					}
					num9 += megaMeshSection2.cverts.Count;
				}
			}
			else
			{
				for (int num11 = 0; num11 < meshsections.Count; num11++)
				{
					MegaMeshSection megaMeshSection3 = meshsections[num11];
					if (!sections[megaMeshSection3.mat].Enabled)
					{
						continue;
					}
					for (int num12 = 0; num12 < crosses - 1; num12++)
					{
						for (int num13 = 0; num13 < megaMeshSection3.cverts.Count - 1; num13++)
						{
							megaMeshSection3.tris.Add(num9 + num13 + 1 + megaMeshSection3.cverts.Count);
							megaMeshSection3.tris.Add(num9 + num13 + 1);
							megaMeshSection3.tris.Add(num9 + num13);
							megaMeshSection3.tris.Add(num9 + num13 + megaMeshSection3.cverts.Count);
							megaMeshSection3.tris.Add(num9 + num13 + 1 + megaMeshSection3.cverts.Count);
							megaMeshSection3.tris.Add(num9 + num13);
							num10 += 6;
						}
						num9 += megaMeshSection3.cverts.Count;
					}
					num9 += megaMeshSection3.cverts.Count;
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
		MegaLoftLayerMultiMat megaLoftLayerMultiMat = go.AddComponent<MegaLoftLayerMultiMat>();
		Copy(this, megaLoftLayerMultiMat);
		megaLoftLayerMultiMat.meshsections = new List<MegaMeshSection>();
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
		for (int i = 0; i < meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = meshsections[i];
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
		for (int i = 0; i < meshsections.Count; i++)
		{
			MegaMeshSection megaMeshSection = meshsections[i];
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
			for (int i = 0; i < meshsections.Count; i++)
			{
				MegaMeshSection megaMeshSection = meshsections[i];
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
		for (int k = 0; k < meshsections.Count; k++)
		{
			MegaMeshSection megaMeshSection2 = meshsections[k];
			for (int l = 0; l < megaMeshSection2.verts1.Count; l++)
			{
				megaMeshSection2.verts.Add(megaMeshSection2.verts1[l]);
			}
		}
	}
}
