using System;
using UnityEngine;

public class MegaLoftLayerCloneSplineRules : MegaLoftLayerRules
{
	public int curve;

	public bool snap;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2205");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)layerPath && layerPath.splines != null && rules.Count > 0)
		{
			return true;
		}
		return false;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		if (layerPath == null)
		{
			return false;
		}
		if (rules.Count == 0)
		{
			return false;
		}
		UnityEngine.Random.seed = Seed;
		Init();
		BuildRules();
		float length = layerPath.splines[curve].length * Length;
		BuildLoftObjects(length);
		for (int i = 0; i < rules.Count; i++)
		{
			for (int j = 0; j < rules[i].lofttris.Count; j++)
			{
				rules[i].lofttris[j].offset = 0;
				rules[i].lofttris[j].tris = new int[rules[i].lofttris[j].sourcetris.Length * rules[i].usage];
			}
		}
		int num = 0;
		int num2 = 0;
		for (int k = 0; k < loftobjs.Count; k++)
		{
			num += loftobjs[k].verts.Length;
			num2 += loftobjs[k].tris.Length;
		}
		if (loftverts == null || loftverts.Length != num)
		{
			loftverts = new Vector3[num];
		}
		if (loftuvs == null || loftuvs.Length != num)
		{
			loftuvs = new Vector2[num];
		}
		if (lofttris == null || lofttris.Length != num2)
		{
			lofttris = new int[num2];
		}
		return true;
	}

	private Vector3 Deform(Vector3 p, MegaShape path, float percent, float off, Vector3 scale, float removeDof, Vector3 locoff, Vector3 sploff)
	{
		p = tm.MultiplyPoint3x4(p);
		p.z += off;
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;
		p += locoff;
		float num = p.z / path.splines[curve].length + percent;
		float num2 = 0f;
		Vector3 vector = path.InterpCurve3D(curve, num, path.normalizedInterp, ref num2) + sploff;
		Vector3 vector2 = path.InterpCurve3D(curve, num + tangent * 0.001f, path.normalizedInterp) + sploff;
		num = ((!path.splines[curve].closed) ? Mathf.Clamp01(num) : Mathf.Repeat(num, 1f));
		tw = Quaternion.AngleAxis(twist * twistCrv.Evaluate(num) + num2, Vector3.forward);
		Vector3 forward = vector2 - vector;
		forward.y *= removeDof;
		Quaternion q = Quaternion.LookRotation(forward) * tw;
		MegaMatrix.SetTR(ref wtm, vector, q);
		wtm = mat * wtm;
		p.z = 0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		if (tangent < 0.1f)
		{
			tangent = 0.1f;
		}
		mat = base.transform.localToWorldMatrix * layerPath.transform.worldToLocalMatrix;
		tm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref tm, (float)Math.PI / 180f * tmrot);
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		Vector3 sploff = Vector3.zero;
		if (snap)
		{
			sploff = layerPath.splines[0].knots[0].p - layerPath.splines[curve].knots[0].p;
		}
		for (int i = 0; i < rules.Count; i++)
		{
			for (int j = 0; j < rules[i].lofttris.Count; j++)
			{
				rules[i].lofttris[j].offset = 0;
			}
		}
		for (int k = 0; k < loftobjs.Count; k++)
		{
			MegaLoftRule megaLoftRule = loftobjs[k];
			Vector3 b = (scale + megaLoftRule.scale) * GlobalScale;
			Vector3 locoff = Vector3.Scale(offset + megaLoftRule.offset, b);
			num -= megaLoftRule.bounds.min[(int)axis];
			num += megaLoftRule.gapin * megaLoftRule.bounds.size[(int)axis];
			for (int l = 0; l < megaLoftRule.verts.Length; l++)
			{
				Vector3 p = megaLoftRule.verts[l];
				p = Deform(p, layerPath, start, num, b, RemoveDof, locoff, sploff);
				loftverts[num3] = p;
				ref Vector2 reference = ref loftuvs[num3++];
				reference = megaLoftRule.uvs[l];
			}
			for (int m = 0; m < megaLoftRule.lofttris.Count; m++)
			{
				int num4 = megaLoftRule.lofttris[m].offset;
				for (int n = 0; n < megaLoftRule.lofttris[m].sourcetris.Length; n++)
				{
					megaLoftRule.lofttris[m].tris[num4++] = megaLoftRule.lofttris[m].sourcetris[n] + num2 + triindex;
				}
				megaLoftRule.lofttris[m].offset = num4;
			}
			num += megaLoftRule.bounds.max[(int)axis];
			num += megaLoftRule.gapout * megaLoftRule.bounds.size[(int)axis];
			num2 = num3;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneSplineRules to = go.AddComponent<MegaLoftLayerCloneSplineRules>();
		Copy(this, to);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		return null;
	}
}
