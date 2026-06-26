using System;
using UnityEngine;

public class MegaLoftLayerCloneRules : MegaLoftLayerRules
{
	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public bool useCrossCrv;

	public AnimationCurve CrossCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float CrossAlpha;

	public bool CalcUp = true;

	public float calcUpAmount = 1f;

	private Vector3 lastrel = Vector3.zero;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2191");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)surfaceLoft && surfaceLayer >= 0 && rules.Count > 0)
		{
			return true;
		}
		return false;
	}

	public override bool LayerNotify(MegaLoftLayerBase layer, int reason)
	{
		if (surfaceLoft != null && surfaceLayer >= 0 && surfaceLoft.Layers[surfaceLayer] == layer)
		{
			return true;
		}
		return false;
	}

	public override bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		if (surfaceLoft != null && surfaceLoft == loft)
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
		if (surfaceLayer < 0 || surfaceLoft == null)
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
		float length = ((MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer]).LoftLength * Length;
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

	private Vector3 Deform(Vector3 p, MegaShapeLoft loft, MegaLoftLayerSimple layer, float percent, float ca, float off, Vector3 scale, float removeDof, Vector3 locoff)
	{
		p = tm.MultiplyPoint3x4(p);
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;
		p.z += off;
		p += locoff;
		float num = p.z * LayerLength + percent;
		if (useCrossCrv)
		{
			ca += CrossCrv.Evaluate(num);
		}
		Vector3 p2;
		Vector3 posAndFrame;
		if (CalcUp)
		{
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;
			posAndFrame = layer.GetPosAndFrame(loft, ca, num, tangent * 0.001f, out p2, out up, out right, out fwd);
			tw = Quaternion.LookRotation(fwd, up);
			Quaternion q = tw;
			if (useTwistCrv)
			{
				q *= Quaternion.AngleAxis(180f + twist * twistCrv.Evaluate(num), Vector3.forward);
			}
			else
			{
				q *= Quaternion.AngleAxis(180f, Vector3.forward);
			}
			MegaMatrix.SetTR(ref wtm, posAndFrame, q);
			wtm = mat * wtm;
			p.z = 0f;
			return wtm.MultiplyPoint3x4(p);
		}
		posAndFrame = layer.GetPosAndLook(loft, ca, num, tangent * 0.001f, out p2);
		if (useTwistCrv)
		{
			tw = Quaternion.AngleAxis(twist * twistCrv.Evaluate(num), Vector3.forward);
		}
		else
		{
			tw = Quaternion.AngleAxis(0f, Vector3.forward);
		}
		Vector3 vector = p2 - posAndFrame;
		vector.y *= removeDof;
		if (vector == Vector3.zero)
		{
			vector = lastrel;
		}
		lastrel = vector;
		Quaternion q2 = Quaternion.LookRotation(vector) * tw;
		MegaMatrix.SetTR(ref wtm, posAndFrame, q2);
		wtm = mat * wtm;
		p.z = 0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
		LayerLength = 1f / megaLoftLayerSimple.GetLength(surfaceLoft);
		if (tangent < 0.1f)
		{
			tangent = 0.1f;
		}
		mat = surfaceLoft.transform.localToWorldMatrix * base.transform.worldToLocalMatrix;
		tm = Matrix4x4.identity;
		tw = Quaternion.identity;
		MegaMatrix.Rotate(ref tm, (float)Math.PI / 180f * tmrot);
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		float num4 = CrossAlpha;
		if (num4 > 0.99999f)
		{
			num4 = 0.99999f;
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
				p = Deform(p, surfaceLoft, megaLoftLayerSimple, start, num4, num, b, RemoveDof, locoff);
				loftverts[num3] = p;
				ref Vector2 reference = ref loftuvs[num3++];
				reference = megaLoftRule.uvs[l];
			}
			for (int m = 0; m < megaLoftRule.lofttris.Count; m++)
			{
				int num5 = megaLoftRule.lofttris[m].offset;
				for (int n = 0; n < megaLoftRule.lofttris[m].sourcetris.Length; n++)
				{
					megaLoftRule.lofttris[m].tris[num5++] = megaLoftRule.lofttris[m].sourcetris[n] + num2 + triindex;
				}
				megaLoftRule.lofttris[m].offset = num5;
			}
			num += megaLoftRule.bounds.max[(int)axis];
			num += megaLoftRule.gapout * megaLoftRule.bounds.size[(int)axis];
			num2 = num3;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneRules megaLoftLayerCloneRules = go.AddComponent<MegaLoftLayerCloneRules>();
		Copy(this, megaLoftLayerCloneRules);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		if (megaLoftLayerCloneRules.surfaceLoft == GetComponent<MegaShapeLoft>())
		{
			megaLoftLayerCloneRules.surfaceLoft = go.GetComponent<MegaShapeLoft>();
		}
		return null;
	}
}
