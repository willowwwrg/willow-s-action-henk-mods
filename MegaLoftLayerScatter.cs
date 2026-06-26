using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerScatter : MegaLoftLayerBase
{
	public bool showstartparams = true;

	public bool showmainparams = true;

	public bool showendparams = true;

	public bool StartEnabled = true;

	public bool MainEnabled = true;

	public bool EndEnabled = true;

	public Vector3 MainScale = Vector3.one;

	public float GlobalScale = 1f;

	public float RemoveDof = 1f;

	public int Count = 1;

	public float tangent = 0.1f;

	public MegaAxis axis;

	public Vector3 rot = Vector3.zero;

	public GameObject mainObj;

	public float twist;

	public float damage;

	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public AnimationCurve ScaleCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public List<Material> mats = new List<Material>();

	public int matcount;

	public int maintris;

	public float Alpha;

	public float Speed;

	public float CAlpha;

	public int Seed;

	public Vector3 scaleRangeMin = Vector3.zero;

	public Vector3 scaleRangeMax = Vector3.zero;

	public bool useDensity;

	public AnimationCurve density = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 scale = Vector3.one;

	public Vector3 rotRange = Vector3.zero;

	public float start;

	public float length = 1f;

	public float cstart;

	public float clength = 1f;

	public float LayerLength;

	public bool CalcUp = true;

	public Vector3 Offset = Vector3.zero;

	private Material[] mainMats;

	private Vector3[] mverts;

	private Vector2[] muvs;

	private Matrix4x4 meshtm;

	private Matrix4x4 tm;

	private Matrix4x4 mat;

	private Quaternion meshrot;

	private Quaternion tw;

	private Matrix4x4 wtm;

	private List<MegaLoftTris> mainlofttris = new List<MegaLoftTris>();

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2169");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)surfaceLoft && surfaceLayer >= 0 && (bool)mainObj)
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
		return mainlofttris[i].tris;
	}

	private void Init()
	{
		matcount = 0;
		maintris = 0;
		mats.Clear();
		if (mainObj != null)
		{
			Mesh sharedMesh = mainObj.GetComponent<MeshFilter>().sharedMesh;
			mverts = sharedMesh.vertices;
			muvs = sharedMesh.uv;
			MeshRenderer component = mainObj.GetComponent<MeshRenderer>();
			mainMats = component.sharedMaterials;
			matcount += mainMats.Length;
			mats.AddRange(mainMats);
			mainlofttris.Clear();
			for (int i = 0; i < sharedMesh.subMeshCount; i++)
			{
				MegaLoftTris megaLoftTris = new MegaLoftTris();
				megaLoftTris.sourcetris = sharedMesh.GetTriangles(i);
				maintris += megaLoftTris.sourcetris.Length;
				mainlofttris.Add(megaLoftTris);
			}
		}
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		Init();
		int num = 0;
		int num2 = 0;
		if ((bool)mainObj && MainEnabled)
		{
			num += mverts.Length * Count;
			num2 += maintris * Count;
			for (int i = 0; i < mainlofttris.Count; i++)
			{
				mainlofttris[i].tris = new int[mainlofttris[i].sourcetris.Length * Count];
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

	private void Update()
	{
		if (Application.isPlaying && LayerEnabled && Speed != 0f)
		{
			Alpha += Speed * LayerLength * Time.deltaTime;
			Alpha = Mathf.Repeat(Alpha, 1f);
			MegaShapeLoft component = GetComponent<MegaShapeLoft>();
			if ((bool)component)
			{
				component.rebuild = true;
			}
		}
	}

	private float FindScatterAlpha()
	{
		float value;
		float value2;
		do
		{
			value = UnityEngine.Random.value;
			value2 = UnityEngine.Random.value;
		}
		while (!(value2 < density.Evaluate(value)));
		return value;
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
		switch (axis)
		{
		case MegaAxis.X:
			MegaMatrix.RotateY(ref tm, -(float)Math.PI / 2f);
			break;
		case MegaAxis.Y:
			MegaMatrix.RotateX(ref tm, -(float)Math.PI / 2f);
			break;
		}
		meshrot = Quaternion.Euler(rot);
		int num = 0;
		int num2 = 0;
		Matrix4x4 identity = Matrix4x4.identity;
		UnityEngine.Random.seed = Seed;
		Vector3 up = Vector3.up;
		Vector3 zero = Vector3.zero;
		Vector3 vector = Vector3.Scale(scaleRangeMin, scale);
		Vector3 vector2 = Vector3.Scale(scaleRangeMax, scale);
		float num3 = 0f;
		for (int i = 0; i < Count; i++)
		{
			zero.x = (scale.x + Mathf.Lerp(vector.x, vector2.x, UnityEngine.Random.value)) * GlobalScale;
			zero.y = (scale.y + Mathf.Lerp(vector.y, vector2.y, UnityEngine.Random.value)) * GlobalScale;
			zero.z = (scale.z + Mathf.Lerp(vector.z, vector2.z, UnityEngine.Random.value)) * GlobalScale;
			num3 = ((!useDensity) ? UnityEngine.Random.value : FindScatterAlpha());
			float a = start + num3 * length + Alpha;
			float ca = cstart + UnityEngine.Random.value * clength + CAlpha;
			Vector3 euler = rot + (UnityEngine.Random.value - 0.5f) * 2f * rotRange;
			meshrot = Quaternion.Euler(euler);
			Vector3 p;
			if (CalcUp)
			{
				Vector3 pos = megaLoftLayerSimple.GetPos1(surfaceLoft, ca, a, 0.001f, out p, out up);
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, up);
				tw = Quaternion.AngleAxis(180f, Vector3.forward) * quaternion;
				Vector3 forward = p - pos;
				forward.y *= RemoveDof;
				Quaternion q = Quaternion.LookRotation(forward) * tw * meshrot;
				wtm.SetTRS(pos, q, zero);
				identity = mat * wtm;
			}
			else
			{
				Vector3 pos2 = megaLoftLayerSimple.GetPos1(surfaceLoft, ca, a, 0.001f, out p, out up);
				tw = Quaternion.AngleAxis(180f, Vector3.forward);
				Vector3 forward2 = p - pos2;
				forward2.y *= RemoveDof;
				Quaternion q2 = Quaternion.LookRotation(forward2) * tw * meshrot;
				wtm.SetTRS(pos2, q2, zero);
				identity = mat * wtm;
			}
			Vector3 v = Vector3.zero;
			for (int j = 0; j < mverts.Length; j++)
			{
				v.x = mverts[j].x + Offset.x;
				v.y = mverts[j].y + Offset.y;
				v.z = mverts[j].z + Offset.z;
				v = identity.MultiplyPoint3x4(v);
				loftverts[num2].x = v.x;
				loftverts[num2].y = v.y;
				loftverts[num2].z = v.z;
				ref Vector2 reference = ref loftuvs[num2++];
				reference = muvs[j];
			}
			for (int k = 0; k < mainlofttris.Count; k++)
			{
				int offset = mainlofttris[k].offset;
				for (int l = 0; l < mainlofttris[k].sourcetris.Length; l++)
				{
					mainlofttris[k].tris[offset++] = mainlofttris[k].sourcetris[l] + num + triindex;
				}
				mainlofttris[k].offset = offset;
			}
			num = num2;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerScatter megaLoftLayerScatter = go.AddComponent<MegaLoftLayerScatter>();
		Copy(this, megaLoftLayerScatter);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		if (megaLoftLayerScatter.surfaceLoft == GetComponent<MegaShapeLoft>())
		{
			megaLoftLayerScatter.surfaceLoft = go.GetComponent<MegaShapeLoft>();
		}
		return null;
	}
}
