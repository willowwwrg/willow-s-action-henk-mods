using System;
using UnityEngine;

public class MegaLoftLayerCloneSimple : MegaLoftLayerBase
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

	public float start;

	public float GlobalScale = 1f;

	public float StartGap;

	public float EndGap;

	public float Gap;

	public float RemoveDof = 1f;

	public int repeat = 1;

	public float Length;

	public float tangent = 0.1f;

	public MegaAxis axis = MegaAxis.Z;

	public Vector3 rot = Vector3.zero;

	public Mesh startObj;

	public Mesh mainObj;

	public Mesh endObj;

	public float twist;

	public float damage;

	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public AnimationCurve ScaleCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool useCrossCrv;

	public AnimationCurve CrossCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool useTwist;

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float CrossAlpha;

	public bool CalcUp = true;

	public float calcUpAmount = 1f;

	public Vector3 StartOff = Vector3.zero;

	public Vector3 MainOff = Vector3.zero;

	public Vector3 EndOff = Vector3.zero;

	public Vector3 tmrot = Vector3.zero;

	public Vector3 Offset = Vector3.zero;

	private Vector3[] sverts;

	private Vector2[] suvs;

	private int[] stris;

	private Vector3[] mverts;

	private Vector2[] muvs;

	private int[] mtris;

	private Vector3[] everts;

	private Vector2[] euvs;

	private int[] etris;

	private Matrix4x4 meshtm;

	private Matrix4x4 tm;

	private Matrix4x4 mat;

	private Quaternion meshrot;

	private Quaternion tw;

	private Matrix4x4 wtm;

	private float LayerLength;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2146");
	}

	public void SetMesh(Mesh newmesh, int which)
	{
		switch (which)
		{
		case 0:
			startObj = newmesh;
			break;
		case 1:
			mainObj = newmesh;
			break;
		case 2:
			endObj = newmesh;
			break;
		}
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)surfaceLoft && surfaceLayer >= 0 && ((bool)startObj || (bool)mainObj || (bool)endObj) && (bool)surfaceLoft && surfaceLayer >= 0)
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

	public int NumSubMeshes()
	{
		return 1;
	}

	public int SubMeshTris(int i)
	{
		return 0;
	}

	public int SubMeshVerts(int i)
	{
		return 0;
	}

	public Material GetMaterials(int i)
	{
		return material;
	}

	private void Init()
	{
		if (startObj != null)
		{
			sverts = startObj.vertices;
			suvs = startObj.uv;
			stris = startObj.triangles;
		}
		if (endObj != null)
		{
			everts = endObj.vertices;
			euvs = endObj.uv;
			etris = endObj.triangles;
		}
		if (mainObj != null)
		{
			mverts = mainObj.vertices;
			muvs = mainObj.uv;
			mtris = mainObj.triangles;
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
		if ((bool)startObj && StartEnabled)
		{
			num += sverts.Length;
			num2 += stris.Length;
		}
		if ((bool)endObj && EndEnabled)
		{
			num += everts.Length;
			num2 += etris.Length;
		}
		if ((bool)mainObj && MainEnabled)
		{
			if (Length != 0f)
			{
				MegaShape megaShape = null;
				float num3 = 0f;
				MegaLoftLayerSimple obj = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
				megaShape = obj.layerPath;
				num3 = obj.LoftLength * Length;
				if ((bool)megaShape)
				{
					Vector3 vector = MainScale * GlobalScale;
					Vector3 zero = Vector3.zero;
					zero.x = mainObj.bounds.size.x * vector.x + Gap * GlobalScale;
					zero.y = mainObj.bounds.size.y * vector.y + Gap * GlobalScale;
					zero.z = mainObj.bounds.size.z * vector.z + Gap * GlobalScale;
					repeat = (int)(num3 / zero[(int)axis]);
				}
			}
			num += mverts.Length * repeat;
			num2 += mtris.Length * repeat;
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
				q *= Quaternion.AngleAxis(twist * twistCrv.Evaluate(num), Vector3.forward);
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
			tw = meshrot;
		}
		Vector3 forward = p2 - posAndFrame;
		forward.y *= removeDof;
		Quaternion q2 = Quaternion.LookRotation(forward) * tw;
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
		meshtm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref meshtm, (float)Math.PI / 180f * rot);
		meshrot = Quaternion.Euler(rot);
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		float crossAlpha = CrossAlpha;
		if (startObj != null && StartEnabled)
		{
			Vector3 vector = StartScale * GlobalScale;
			Vector3 locoff = Vector3.Scale(StartOff + Offset, vector);
			num -= startObj.bounds.min[(int)axis] * vector[(int)axis];
			for (int i = 0; i < sverts.Length; i++)
			{
				Vector3 p = sverts[i];
				p = Deform(p, surfaceLoft, megaLoftLayerSimple, start, crossAlpha, num, vector, RemoveDof, locoff);
				loftverts[num3] = p;
				ref Vector2 reference = ref loftuvs[num3++];
				reference = suvs[i];
			}
			for (int j = 0; j < stris.Length; j++)
			{
				lofttris[num4++] = stris[j] + triindex;
			}
			num += startObj.bounds.max[(int)axis] * vector[(int)axis];
			num += StartGap * GlobalScale;
			num2 = num3;
		}
		if (mainObj != null && MainEnabled)
		{
			float num5 = mainObj.bounds.size[(int)axis];
			Vector3 vector2 = MainScale * GlobalScale;
			Vector3 locoff2 = Vector3.Scale(MainOff + Offset, vector2);
			num -= mainObj.bounds.min[(int)axis] * vector2[(int)axis];
			num5 *= vector2[(int)axis];
			float num6 = Gap * GlobalScale;
			for (int k = 0; k < repeat; k++)
			{
				for (int l = 0; l < mverts.Length; l++)
				{
					Vector3 p2 = mverts[l];
					p2 = Deform(p2, surfaceLoft, megaLoftLayerSimple, start, crossAlpha, num, vector2, RemoveDof, locoff2);
					loftverts[num3] = p2;
					ref Vector2 reference2 = ref loftuvs[num3++];
					reference2 = muvs[l];
				}
				for (int m = 0; m < mtris.Length; m++)
				{
					lofttris[num4++] = mtris[m] + num2 + triindex;
				}
				num += num5;
				num += num6;
				num2 = num3;
			}
			num -= num6;
			num += mainObj.bounds.max[(int)axis] * vector2[(int)axis] - num5;
		}
		if (endObj != null && EndEnabled)
		{
			Vector3 vector3 = EndScale * GlobalScale;
			Vector3 locoff3 = Vector3.Scale(EndOff + Offset, vector3);
			num -= endObj.bounds.min[(int)axis] * vector3[(int)axis];
			num += EndGap * GlobalScale;
			for (int n = 0; n < everts.Length; n++)
			{
				Vector3 p3 = everts[n];
				p3 = Deform(p3, surfaceLoft, megaLoftLayerSimple, start, crossAlpha, num, vector3, RemoveDof, locoff3);
				loftverts[num3] = p3;
				ref Vector2 reference3 = ref loftuvs[num3++];
				reference3 = euvs[n];
			}
			for (int num7 = 0; num7 < etris.Length; num7++)
			{
				lofttris[num4++] = etris[num7] + num2 + triindex;
			}
			num2 += everts.Length;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneSimple megaLoftLayerCloneSimple = go.AddComponent<MegaLoftLayerCloneSimple>();
		Copy(this, megaLoftLayerCloneSimple);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		if (megaLoftLayerCloneSimple.surfaceLoft == GetComponent<MegaShapeLoft>())
		{
			megaLoftLayerCloneSimple.surfaceLoft = go.GetComponent<MegaShapeLoft>();
		}
		return null;
	}
}
