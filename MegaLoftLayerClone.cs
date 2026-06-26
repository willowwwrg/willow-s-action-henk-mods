using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerClone : MegaLoftLayerBase
{
	public Mesh cloneMesh;

	public bool showstartparams = true;

	public bool showmainparams = true;

	public bool showendparams = true;

	public bool StartEnabled = true;

	public bool MainEnabled = true;

	public bool EndEnabled = true;

	public Vector3 StartScale = Vector3.one;

	public Vector3 MainScale = Vector3.one;

	public Vector3 EndScale = Vector3.one;

	public float start = 0.5f;

	public float GlobalScale = 1f;

	public float StartGap;

	public float EndGap;

	public float Gap;

	public float RemoveDof = 1f;

	public int repeat = 1;

	public float Length = 0.1f;

	public float tangent = 0.1f;

	public MegaAxis axis;

	public Vector3 rot = Vector3.zero;

	public GameObject startObj;

	public GameObject mainObj;

	public GameObject endObj;

	public float twist;

	public float damage;

	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public AnimationCurve ScaleCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool useCrossCrv;

	public AnimationCurve CrossCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public List<Material> mats = new List<Material>();

	public int matcount;

	public int starttris;

	public int maintris;

	public int endtris;

	public int submesh;

	public bool useTwist;

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Vector3 StartOff = Vector3.zero;

	public Vector3 MainOff = Vector3.zero;

	public Vector3 EndOff = Vector3.zero;

	public Vector3 Offset = Vector3.zero;

	public float CrossAlpha = 0.5f;

	public bool CalcUp = true;

	public Vector3 tmrot = new Vector3(0f, 90f, 0f);

	private Vector3[] sverts;

	private Vector2[] suvs;

	private Vector3[] mverts;

	private Vector2[] muvs;

	private Vector3[] everts;

	private Vector2[] euvs;

	private Matrix4x4 meshtm;

	private Matrix4x4 tm;

	private Matrix4x4 mat;

	private Quaternion meshrot;

	private Quaternion tw;

	private Matrix4x4 wtm;

	private List<MegaLoftTris> startlofttris = new List<MegaLoftTris>();

	private List<MegaLoftTris> mainlofttris = new List<MegaLoftTris>();

	private List<MegaLoftTris> endlofttris = new List<MegaLoftTris>();

	private Material[] startMats;

	private Material[] mainMats;

	private Material[] endMats;

	private Bounds startBounds;

	private Bounds mainBounds;

	private Bounds endBounds;

	private float LayerLength;

	private Vector3 lastrel = Vector3.zero;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2165");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)surfaceLoft && surfaceLayer >= 0 && ((bool)startObj || (bool)mainObj || (bool)endObj))
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
		if (i < startlofttris.Count && StartEnabled)
		{
			return startlofttris[i].tris;
		}
		i -= startlofttris.Count;
		if (i < mainlofttris.Count && MainEnabled)
		{
			return mainlofttris[i].tris;
		}
		i -= mainlofttris.Count;
		return endlofttris[i].tris;
	}

	private void Init()
	{
		matcount = 0;
		starttris = (maintris = (endtris = 0));
		mats.Clear();
		startlofttris.Clear();
		mainlofttris.Clear();
		endlofttris.Clear();
		if (startObj != null && StartEnabled)
		{
			MeshFilter component = startObj.GetComponent<MeshFilter>();
			if ((bool)component)
			{
				Mesh sharedMesh = component.sharedMesh;
				startBounds = sharedMesh.bounds;
				sverts = sharedMesh.vertices;
				suvs = sharedMesh.uv;
				MeshRenderer component2 = startObj.GetComponent<MeshRenderer>();
				startMats = component2.sharedMaterials;
				matcount += startMats.Length;
				mats.AddRange(startMats);
				for (int i = 0; i < sharedMesh.subMeshCount; i++)
				{
					MegaLoftTris megaLoftTris = new MegaLoftTris();
					megaLoftTris.sourcetris = sharedMesh.GetTriangles(i);
					starttris += megaLoftTris.sourcetris.Length;
					startlofttris.Add(megaLoftTris);
				}
			}
		}
		if (mainObj != null && MainEnabled)
		{
			MeshFilter component3 = mainObj.GetComponent<MeshFilter>();
			if ((bool)component3)
			{
				Mesh sharedMesh2 = component3.sharedMesh;
				mainBounds = sharedMesh2.bounds;
				mverts = sharedMesh2.vertices;
				muvs = sharedMesh2.uv;
				MeshRenderer component4 = mainObj.GetComponent<MeshRenderer>();
				mainMats = component4.sharedMaterials;
				matcount += mainMats.Length;
				mats.AddRange(mainMats);
				for (int j = 0; j < sharedMesh2.subMeshCount; j++)
				{
					MegaLoftTris megaLoftTris2 = new MegaLoftTris();
					megaLoftTris2.sourcetris = sharedMesh2.GetTriangles(j);
					maintris += megaLoftTris2.sourcetris.Length;
					mainlofttris.Add(megaLoftTris2);
				}
			}
		}
		if (!(endObj != null) || !EndEnabled)
		{
			return;
		}
		MeshFilter component5 = endObj.GetComponent<MeshFilter>();
		if ((bool)component5)
		{
			Mesh sharedMesh3 = component5.sharedMesh;
			endBounds = sharedMesh3.bounds;
			everts = sharedMesh3.vertices;
			euvs = sharedMesh3.uv;
			MeshRenderer component6 = endObj.GetComponent<MeshRenderer>();
			endMats = component6.sharedMaterials;
			matcount += endMats.Length;
			mats.AddRange(endMats);
			for (int k = 0; k < sharedMesh3.subMeshCount; k++)
			{
				MegaLoftTris megaLoftTris3 = new MegaLoftTris();
				megaLoftTris3.sourcetris = sharedMesh3.GetTriangles(k);
				endtris += megaLoftTris3.sourcetris.Length;
				endlofttris.Add(megaLoftTris3);
			}
		}
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
		if (startObj == null && mainObj == null && endObj == null)
		{
			return false;
		}
		Init();
		int num = 0;
		int num2 = 0;
		if ((bool)startObj && StartEnabled)
		{
			num += sverts.Length;
			num2 += starttris;
			for (int i = 0; i < startlofttris.Count; i++)
			{
				startlofttris[i].tris = new int[startlofttris[i].sourcetris.Length];
			}
		}
		if ((bool)mainObj && MainEnabled)
		{
			if (Length != 0f)
			{
				MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
				if ((bool)megaLoftLayerSimple.layerPath)
				{
					float num3 = megaLoftLayerSimple.LoftLength * Length;
					Vector3 vector = MainScale * GlobalScale;
					Vector3 zero = Vector3.zero;
					zero.x = mainBounds.size.x * vector.x + Gap * GlobalScale;
					zero.y = mainBounds.size.y * vector.y + Gap * GlobalScale;
					zero.z = mainBounds.size.z * vector.z + Gap * GlobalScale;
					repeat = (int)(num3 / zero[(int)axis]);
				}
			}
			num += mverts.Length * repeat;
			num2 += maintris * repeat;
			for (int j = 0; j < mainlofttris.Count; j++)
			{
				mainlofttris[j].tris = new int[mainlofttris[j].sourcetris.Length * repeat];
			}
		}
		if ((bool)endObj && EndEnabled)
		{
			num += everts.Length;
			num2 += endtris;
			for (int k = 0; k < endlofttris.Count; k++)
			{
				endlofttris[k].tris = new int[endlofttris[k].sourcetris.Length];
			}
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
			Quaternion q = tw * meshrot;
			if (useTwist)
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
		if (useTwist)
		{
			tw = meshrot * Quaternion.AngleAxis(twist * twistCrv.Evaluate(num), Vector3.forward);
		}
		else
		{
			tw = meshrot * Quaternion.AngleAxis(0f, Vector3.forward);
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
		MegaMatrix.Rotate(ref tm, (float)Math.PI / 180f * tmrot);
		MegaMatrix.Rotate(ref meshtm, (float)Math.PI / 180f * rot);
		meshtm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref meshtm, (float)Math.PI / 180f * rot);
		meshrot = Quaternion.Euler(rot);
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		float num4 = CrossAlpha;
		if (num4 > 0.99999f)
		{
			num4 = 0.99999f;
		}
		int index = (int)axis;
		if (startObj != null && StartEnabled)
		{
			Vector3 vector = StartScale * GlobalScale;
			Vector3 locoff = Vector3.Scale(StartOff + Offset, vector);
			num -= startBounds.min[(int)axis] * vector[index];
			for (int i = 0; i < sverts.Length; i++)
			{
				Vector3 p = sverts[i];
				p = Deform(p, surfaceLoft, megaLoftLayerSimple, start, num4, num, vector, RemoveDof, locoff);
				loftverts[num3] = p;
				ref Vector2 reference = ref loftuvs[num3++];
				reference = suvs[i];
			}
			for (int j = 0; j < startlofttris.Count; j++)
			{
				int offset = startlofttris[j].offset;
				for (int k = 0; k < startlofttris[j].sourcetris.Length; k++)
				{
					startlofttris[j].tris[offset++] = startlofttris[j].sourcetris[k] + num2 + triindex;
				}
				startlofttris[j].offset = offset;
			}
			num += startBounds.max[(int)axis] * vector[index];
			num += StartGap * GlobalScale;
			num2 = num3;
		}
		if (mainObj != null && MainEnabled)
		{
			for (int l = 0; l < mainlofttris.Count; l++)
			{
				mainlofttris[l].offset = 0;
			}
			float num5 = mainBounds.size[(int)axis];
			Vector3 vector2 = MainScale * GlobalScale;
			Vector3 locoff2 = Vector3.Scale(MainOff + Offset, vector2);
			num -= mainBounds.min[(int)axis] * vector2[index];
			num5 *= vector2[(int)axis];
			float num6 = Gap * GlobalScale;
			for (int m = 0; m < repeat; m++)
			{
				for (int n = 0; n < mverts.Length; n++)
				{
					Vector3 p2 = mverts[n];
					p2 = Deform(p2, surfaceLoft, megaLoftLayerSimple, start, num4, num, vector2, RemoveDof, locoff2);
					loftverts[num3] = p2;
					ref Vector2 reference2 = ref loftuvs[num3++];
					reference2 = muvs[n];
				}
				for (int num7 = 0; num7 < mainlofttris.Count; num7++)
				{
					int offset2 = mainlofttris[num7].offset;
					for (int num8 = 0; num8 < mainlofttris[num7].sourcetris.Length; num8++)
					{
						mainlofttris[num7].tris[offset2++] = mainlofttris[num7].sourcetris[num8] + num2 + triindex;
					}
					mainlofttris[num7].offset = offset2;
				}
				num += num5;
				num += num6;
				num2 = num3;
			}
			num -= num6;
			num += mainBounds.max[(int)axis] * vector2[index] - num5;
		}
		if (endObj != null && EndEnabled)
		{
			Vector3 vector3 = EndScale * GlobalScale;
			Vector3 locoff3 = Vector3.Scale(EndOff + Offset, vector3);
			num -= endBounds.min[(int)axis] * vector3[index];
			num += EndGap * GlobalScale;
			for (int num9 = 0; num9 < everts.Length; num9++)
			{
				Vector3 p3 = everts[num9];
				p3 = Deform(p3, surfaceLoft, megaLoftLayerSimple, start, num4, num, vector3, RemoveDof, locoff3);
				loftverts[num3] = p3;
				ref Vector2 reference3 = ref loftuvs[num3++];
				reference3 = euvs[num9];
			}
			for (int num10 = 0; num10 < endlofttris.Count; num10++)
			{
				int offset3 = endlofttris[num10].offset;
				for (int num11 = 0; num11 < endlofttris[num10].sourcetris.Length; num11++)
				{
					endlofttris[num10].tris[offset3++] = endlofttris[num10].sourcetris[num11] + num2 + triindex;
				}
				endlofttris[num10].offset = offset3;
			}
			num2 += everts.Length;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerClone megaLoftLayerClone = go.AddComponent<MegaLoftLayerClone>();
		Copy(this, megaLoftLayerClone);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		if (megaLoftLayerClone.surfaceLoft == GetComponent<MegaShapeLoft>())
		{
			megaLoftLayerClone.surfaceLoft = go.GetComponent<MegaShapeLoft>();
		}
		return megaLoftLayerClone;
	}
}
