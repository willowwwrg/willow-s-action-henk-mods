using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLoftLayerCloneSpline : MegaLoftLayerBase
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

	public MegaAxis axis;

	public Vector3 rot = Vector3.zero;

	public GameObject startObj;

	public GameObject mainObj;

	public GameObject endObj;

	public float twist;

	public float damage;

	public AnimationCurve ScaleCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public List<Material> mats = new List<Material>();

	public int starttris;

	public int maintris;

	public int endtris;

	public bool useTwist;

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Vector3 StartOff = Vector3.zero;

	public Vector3 MainOff = Vector3.zero;

	public Vector3 EndOff = Vector3.zero;

	public Vector3 Offset = Vector3.zero;

	public int curve;

	public bool snap;

	public Vector3 rotPath = Vector3.zero;

	public Vector3 offPath = Vector3.zero;

	public Vector3 sclPath = Vector3.one;

	private Vector3[] sverts;

	private Vector2[] suvs;

	private Vector3[] mverts;

	private Vector2[] muvs;

	private Vector3[] everts;

	private Vector2[] euvs;

	private Matrix4x4 meshtm;

	private Matrix4x4 pathtm;

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

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2187");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)layerPath && layerPath.splines != null && ((bool)startObj || (bool)mainObj || (bool)endObj))
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
		starttris = (maintris = (endtris = 0));
		mats.Clear();
		startlofttris.Clear();
		mainlofttris.Clear();
		endlofttris.Clear();
		if ((bool)startObj && StartEnabled)
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
		if ((bool)mainObj && MainEnabled)
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
		if (!endObj || !EndEnabled)
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
		if (layerPath == null)
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
				float num3 = layerPath.splines[curve].length * Length;
				Vector3 vector = MainScale * GlobalScale;
				Vector3 zero = Vector3.zero;
				zero.x = mainBounds.size.x * vector.x + Gap * GlobalScale;
				zero.y = mainBounds.size.y * vector.y + Gap * GlobalScale;
				zero.z = mainBounds.size.z * vector.z + Gap * GlobalScale;
				repeat = (int)(num3 / zero[(int)axis]);
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

	private Vector3 Deform(Vector3 p, MegaShape path, float percent, float off, Vector3 scale, float removeDof, Vector3 locoff, Vector3 sploff)
	{
		p = tm.MultiplyPoint3x4(p);
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;
		p.z += off;
		p += locoff;
		float num = p.z / path.splines[curve].length + percent;
		float num2 = 0f;
		Vector3 vector = pathtm.MultiplyPoint(path.InterpCurve3D(curve, num, path.normalizedInterp, ref twist) + sploff);
		Vector3 vector2 = pathtm.MultiplyPoint(path.InterpCurve3D(curve, num + tangent * 0.001f, path.normalizedInterp) + sploff);
		num = ((!path.splines[curve].closed) ? Mathf.Clamp01(num) : Mathf.Repeat(num, 1f));
		if (useTwist)
		{
			tw = Quaternion.AngleAxis(twist * twistCrv.Evaluate(num) + num2, Vector3.forward);
		}
		Vector3 forward = vector2 - vector;
		forward.y *= removeDof;
		Quaternion q = Quaternion.LookRotation(forward) * tw * meshrot;
		MegaMatrix.SetTR(ref wtm, vector, q);
		wtm = mat * wtm;
		p.z = 0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		MegaShape megaShape = layerPath;
		if (megaShape != null)
		{
			if (tangent < 0.1f)
			{
				tangent = 0.1f;
			}
			mat = megaShape.transform.localToWorldMatrix;
			tm = Matrix4x4.identity;
			tw = Quaternion.identity;
			switch (axis)
			{
			case MegaAxis.X:
				MegaMatrix.RotateY(ref tm, -(float)Math.PI / 2f);
				break;
			case MegaAxis.Y:
				MegaMatrix.RotateX(ref tm, -(float)Math.PI / 2f);
				break;
			}
			meshtm = Matrix4x4.identity;
			MegaMatrix.Rotate(ref meshtm, (float)Math.PI / 180f * rot);
			meshrot = Quaternion.Euler(rot);
			pathtm.SetTRS(offPath, Quaternion.Euler(rotPath), sclPath);
			float num = 0f;
			int num2 = 0;
			int num3 = 0;
			int index = (int)axis;
			Vector3 sploff = Vector3.zero;
			if (snap)
			{
				sploff = megaShape.splines[0].knots[0].p - megaShape.splines[curve].knots[0].p;
			}
			if (startObj != null && StartEnabled)
			{
				Vector3 vector = StartScale * GlobalScale;
				Vector3 locoff = Vector3.Scale(StartOff + Offset, vector);
				num -= startBounds.min[(int)axis] * vector[index];
				for (int i = 0; i < sverts.Length; i++)
				{
					Vector3 p = sverts[i];
					p = Deform(p, megaShape, start, num, vector, RemoveDof, locoff, sploff);
					loftverts[num3] = p;
					ref Vector2 reference = ref loftuvs[num3++];
					reference = suvs[i];
				}
				for (int j = 0; j < startlofttris.Count; j++)
				{
					for (int k = 0; k < startlofttris[j].sourcetris.Length; k++)
					{
						startlofttris[j].tris[k] = startlofttris[j].sourcetris[k] + triindex;
					}
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
				float num4 = mainBounds.size[(int)axis];
				Vector3 vector2 = MainScale * GlobalScale;
				Vector3 locoff2 = Vector3.Scale(MainOff + Offset, vector2);
				num -= mainBounds.min[(int)axis] * vector2[index];
				num4 *= vector2[(int)axis];
				float num5 = Gap * GlobalScale;
				for (int m = 0; m < repeat; m++)
				{
					for (int n = 0; n < mverts.Length; n++)
					{
						Vector3 p2 = mverts[n];
						p2 = Deform(p2, megaShape, start, num, vector2, RemoveDof, locoff2, sploff);
						loftverts[num3] = p2;
						ref Vector2 reference2 = ref loftuvs[num3++];
						reference2 = muvs[n];
					}
					for (int num6 = 0; num6 < mainlofttris.Count; num6++)
					{
						int offset = mainlofttris[num6].offset;
						for (int num7 = 0; num7 < mainlofttris[num6].sourcetris.Length; num7++)
						{
							mainlofttris[num6].tris[offset++] = mainlofttris[num6].sourcetris[num7] + num2 + triindex;
						}
						mainlofttris[num6].offset = offset;
					}
					num += num4;
					num += num5;
					num2 = num3;
				}
				num -= num5;
				num += mainBounds.max[(int)axis] * vector2[index] - num4;
			}
			if (endObj != null && EndEnabled)
			{
				Vector3 vector3 = EndScale * GlobalScale;
				Vector3 locoff3 = Vector3.Scale(EndOff + Offset, vector3);
				num -= endBounds.min[(int)axis] * vector3[index];
				num += EndGap * GlobalScale;
				for (int num8 = 0; num8 < everts.Length; num8++)
				{
					Vector3 p3 = everts[num8];
					p3 = Deform(p3, megaShape, start, num, vector3, RemoveDof, locoff3, sploff);
					loftverts[num3] = p3;
					ref Vector2 reference3 = ref loftuvs[num3++];
					reference3 = euvs[num8];
				}
				for (int num9 = 0; num9 < endlofttris.Count; num9++)
				{
					for (int num10 = 0; num10 < endlofttris[num9].sourcetris.Length; num10++)
					{
						endlofttris[num9].tris[num10] = endlofttris[num9].sourcetris[num10] + triindex + num2;
					}
				}
				num2 += everts.Length;
			}
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneSpline to = go.AddComponent<MegaLoftLayerCloneSpline>();
		Copy(this, to);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		return null;
	}
}
