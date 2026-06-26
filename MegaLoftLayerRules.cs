using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerRules : MegaLoftLayerBase
{
	public List<MegaLoftRule> rules = new List<MegaLoftRule>();

	public List<MegaLoftRule> loftobjs = new List<MegaLoftRule>();

	public List<MegaLoftRule> startrules = new List<MegaLoftRule>();

	public List<MegaLoftRule> fillerrules = new List<MegaLoftRule>();

	public List<MegaLoftRule> regularrules = new List<MegaLoftRule>();

	public List<MegaLoftRule> placedrules = new List<MegaLoftRule>();

	public List<MegaLoftRule> endrules = new List<MegaLoftRule>();

	public int Seed;

	public float GlobalScale = 1f;

	public MegaAxis axis;

	public Vector3 scale = Vector3.one;

	public bool showmainparams = true;

	public float start;

	public float RemoveDof = 1f;

	public int repeat = 1;

	public float Length;

	public float tangent = 0.1f;

	public Vector3 rot = Vector3.zero;

	public float twist;

	public float damage;

	public AnimationCurve ScaleCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public List<Material> mats = new List<Material>();

	public int matcount;

	public int submesh;

	public bool useTwistCrv;

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Vector3 tmrot = Vector3.zero;

	public Vector3 offset = Vector3.zero;

	public float LayerLength;

	public Matrix4x4 tm;

	public Matrix4x4 mat;

	public Quaternion tw;

	public Matrix4x4 wtm;

	public override int NumMaterials()
	{
		return mats.Count;
	}

	public override Material GetMaterial(int i)
	{
		return mats[i];
	}

	public override int[] GetTris(int i)
	{
		for (int j = 0; j < rules.Count; j++)
		{
			if (i < rules[j].lofttris.Count)
			{
				return rules[j].lofttris[i].tris;
			}
			i -= rules[j].lofttris.Count;
		}
		return null;
	}

	public void Init()
	{
		matcount = 0;
		mats.Clear();
		for (int i = 0; i < rules.Count; i++)
		{
			MegaLoftRule megaLoftRule = rules[i];
			megaLoftRule.lofttris.Clear();
			if (!megaLoftRule.obj)
			{
				continue;
			}
			MeshFilter component = megaLoftRule.obj.GetComponent<MeshFilter>();
			if ((bool)component)
			{
				Mesh sharedMesh = component.sharedMesh;
				megaLoftRule.bounds = sharedMesh.bounds;
				megaLoftRule.verts = sharedMesh.vertices;
				megaLoftRule.uvs = sharedMesh.uv;
				megaLoftRule.tris = sharedMesh.triangles;
				MeshRenderer component2 = megaLoftRule.obj.GetComponent<MeshRenderer>();
				megaLoftRule.mats = component2.sharedMaterials;
				matcount += megaLoftRule.mats.Length;
				mats.AddRange(megaLoftRule.mats);
				megaLoftRule.numtris = 0;
				for (int j = 0; j < sharedMesh.subMeshCount; j++)
				{
					MegaLoftTris megaLoftTris = new MegaLoftTris();
					megaLoftTris.sourcetris = sharedMesh.GetTriangles(j);
					megaLoftRule.numtris += megaLoftTris.sourcetris.Length;
					megaLoftRule.lofttris.Add(megaLoftTris);
				}
			}
		}
	}

	public void BuildRules()
	{
		startrules.Clear();
		fillerrules.Clear();
		regularrules.Clear();
		placedrules.Clear();
		endrules.Clear();
		for (int i = 0; i < rules.Count; i++)
		{
			if (rules[i].enabled && rules[i].obj != null)
			{
				switch (rules[i].type)
				{
				case MegaLoftRuleType.Start:
					startrules.Add(rules[i]);
					break;
				case MegaLoftRuleType.End:
					endrules.Add(rules[i]);
					break;
				case MegaLoftRuleType.Regular:
					regularrules.Add(rules[i]);
					break;
				case MegaLoftRuleType.Filler:
					fillerrules.Add(rules[i]);
					break;
				case MegaLoftRuleType.Placed:
					placedrules.Add(rules[i]);
					break;
				}
			}
		}
		float num = 0f;
		for (int j = 0; j < fillerrules.Count; j++)
		{
			num += fillerrules[j].weight;
			fillerrules[j].tweight = num;
		}
	}

	private MegaLoftRule GetStart()
	{
		if (startrules.Count > 0)
		{
			_ = startrules.Count;
			return startrules[0];
		}
		return null;
	}

	private MegaLoftRule GetEnd()
	{
		if (endrules.Count > 0)
		{
			_ = endrules.Count;
			return endrules[0];
		}
		return null;
	}

	private MegaLoftRule GetPlaced(float alpha)
	{
		if (placedrules.Count > 0)
		{
			for (int i = 0; i < placedrules.Count; i++)
			{
				if (placedrules[i].alpha > alpha)
				{
					placedrules[i].used = false;
				}
				if (placedrules[i].alpha < alpha && !placedrules[i].used)
				{
					placedrules[i].used = true;
					return placedrules[i];
				}
			}
		}
		return null;
	}

	public MegaLoftRule GetFiller()
	{
		if (fillerrules.Count > 0)
		{
			if (fillerrules.Count == 1)
			{
				return fillerrules[0];
			}
			float num = Random.value * fillerrules[fillerrules.Count - 1].tweight;
			int num2 = 0;
			for (num2 = 0; num2 < fillerrules.Count - 1; num2++)
			{
				if (num < fillerrules[num2].tweight)
				{
					return fillerrules[num2];
				}
			}
			return fillerrules[num2];
		}
		return null;
	}

	public MegaLoftRule GetRegular(int count)
	{
		if (regularrules.Count > 0)
		{
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < regularrules.Count; i++)
			{
				if (regularrules[i].count != 0 && count % regularrules[i].count == 0 && regularrules[i].count > num2)
				{
					num2 = regularrules[i].count;
					num = i;
				}
			}
			if (num > -1)
			{
				return regularrules[num];
			}
		}
		return null;
	}

	public void BuildLoftObjects(float length)
	{
		loftobjs.Clear();
		for (int i = 0; i < rules.Count; i++)
		{
			rules[i].usage = 0;
		}
		MegaLoftRule megaLoftRule = GetStart();
		if (megaLoftRule != null)
		{
			loftobjs.Add(megaLoftRule);
			megaLoftRule.usage++;
		}
		int num = 0;
		int num2 = 20;
		float num3 = 0f;
		while (num3 < length)
		{
			megaLoftRule = GetPlaced(num3);
			if (megaLoftRule == null)
			{
				megaLoftRule = GetRegular(num);
			}
			if (megaLoftRule == null)
			{
				megaLoftRule = GetFiller();
			}
			if (megaLoftRule == null)
			{
				break;
			}
			loftobjs.Add(megaLoftRule);
			megaLoftRule.usage++;
			float num4 = (megaLoftRule.gapin + megaLoftRule.gapout) * megaLoftRule.bounds.size[(int)axis];
			float f = (megaLoftRule.bounds.size[(int)axis] + num4) * (scale[(int)axis] + megaLoftRule.scale[(int)axis]) * GlobalScale;
			f = Mathf.Abs(f);
			if (f == 0f)
			{
				num2--;
				if (num2 < 0)
				{
					Debug.Log("Found too many 0 width rules, Exiting Loft early.");
					break;
				}
			}
			num3 += f;
			num++;
		}
		megaLoftRule = GetEnd();
		if (megaLoftRule != null)
		{
			loftobjs.Add(megaLoftRule);
			megaLoftRule.usage++;
		}
		int num5 = 0;
		int num6 = 0;
		for (int j = 0; j < loftobjs.Count; j++)
		{
			num5 += loftobjs[j].verts.Length;
			num6 += loftobjs[j].tris.Length;
		}
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerRules to = go.AddComponent<MegaLoftLayerRules>();
		Copy(this, to);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		return null;
	}
}
