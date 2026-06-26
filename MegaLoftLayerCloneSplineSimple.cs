using System;
using UnityEngine;

public class MegaLoftLayerCloneSplineSimple : MegaLoftLayerBase
{
	public bool showstartparams = true;

	public bool showmainparams = true;

	public bool showendparams = true;

	public bool StartEnabled = true;

	public bool MainEnabled = true;

	public bool EndEnabled = true;

	public Vector3 StartScale = Vector3.one;

	public Vector3 MainScale = Vector3.one;

	public Vector3 EndScale = Vector3.one;

	public float Start;

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

	public Mesh startObj;

	public Mesh mainObj;

	public Mesh endObj;

	public float twist;

	public float damage;

	public int curve;

	public bool useTwist;

	public AnimationCurve ScaleCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Vector3 StartOff = Vector3.zero;

	public Vector3 MainOff = Vector3.zero;

	public Vector3 EndOff = Vector3.zero;

	public Vector3 rotPath = Vector3.zero;

	public Vector3 offPath = Vector3.zero;

	public Vector3 sclPath = Vector3.one;

	public bool snap;

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

	private Matrix4x4 pathtm = Matrix4x4.identity;

	[ContextMenu("Help")]
	public void HelpCom()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2159");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)layerPath && layerPath.splines != null && ((bool)startObj || (bool)mainObj || (bool)endObj))
		{
			return true;
		}
		return false;
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
				MegaShape megaShape = layerPath;
				if ((bool)megaShape)
				{
					float num3 = megaShape.splines[curve].length * Length;
					Vector3 vector = MainScale * GlobalScale;
					Vector3 zero = Vector3.zero;
					zero.x = (mainObj.bounds.size.x + Gap) * vector.x;
					zero.y = (mainObj.bounds.size.y + Gap) * vector.y;
					zero.z = (mainObj.bounds.size.z + Gap) * vector.z;
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
		Vector3 vector = pathtm.MultiplyPoint(path.InterpCurve3D(curve, num, path.normalizedInterp, ref num2) + sploff);
		Vector3 vector2 = pathtm.MultiplyPoint(path.InterpCurve3D(curve, num + tangent * 0.001f, path.normalizedInterp) + sploff);
		num = ((!path.splines[curve].closed) ? Mathf.Clamp01(num) : Mathf.Repeat(num, 1f));
		if (useTwist)
		{
			tw = meshrot * Quaternion.AngleAxis(twist * twistCrv.Evaluate(num) + num2, Vector3.forward);
		}
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
		MegaShape megaShape = layerPath;
		if (tangent < 0.1f)
		{
			tangent = 0.1f;
		}
		mat = megaShape.transform.localToWorldMatrix;
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
		meshtm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref meshtm, (float)Math.PI / 180f * rot);
		meshrot = Quaternion.Euler(rot);
		tw = meshrot;
		pathtm.SetTRS(offPath, Quaternion.Euler(rotPath), sclPath);
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		Vector3 sploff = Vector3.zero;
		if (snap)
		{
			sploff = megaShape.splines[0].knots[0].p - megaShape.splines[curve].knots[0].p;
		}
		int index = (int)axis;
		float start = Start;
		if (startObj != null && StartEnabled)
		{
			Vector3 vector = StartScale * GlobalScale;
			Vector3 locoff = Vector3.Scale(StartOff, vector);
			num -= startObj.bounds.min[(int)axis] * vector[index];
			for (int i = 0; i < sverts.Length; i++)
			{
				Vector3 p = sverts[i];
				p = Deform(p, megaShape, start, num, vector, RemoveDof, locoff, sploff);
				loftverts[num3] = p;
				ref Vector2 reference = ref loftuvs[num3++];
				reference = suvs[i];
			}
			for (int j = 0; j < stris.Length; j++)
			{
				lofttris[num4++] = stris[j] + triindex;
			}
			num += startObj.bounds.max[(int)axis] * vector[index];
			num += StartGap;
			num2 = num3;
		}
		if (mainObj != null && MainEnabled)
		{
			float num5 = mainObj.bounds.size[(int)axis];
			Vector3 vector2 = MainScale * GlobalScale;
			Vector3 locoff2 = Vector3.Scale(MainOff, vector2);
			num -= mainObj.bounds.min[(int)axis] * vector2[index];
			num5 *= vector2[(int)axis];
			for (int k = 0; k < repeat; k++)
			{
				for (int l = 0; l < mverts.Length; l++)
				{
					Vector3 p2 = mverts[l];
					p2 = Deform(p2, megaShape, start, num, vector2, RemoveDof, locoff2, sploff);
					loftverts[num3] = p2;
					ref Vector2 reference2 = ref loftuvs[num3++];
					reference2 = muvs[l];
				}
				for (int m = 0; m < mtris.Length; m++)
				{
					lofttris[num4++] = mtris[m] + num2 + triindex;
				}
				num += num5;
				num += Gap;
				num2 = num3;
			}
			num -= Gap;
			num += mainObj.bounds.max[(int)axis] * vector2[index] - num5;
		}
		if (endObj != null && EndEnabled)
		{
			Vector3 vector3 = EndScale * GlobalScale;
			Vector3 locoff3 = Vector3.Scale(EndOff, vector3);
			num -= endObj.bounds.min[(int)axis] * vector3[index];
			num += EndGap;
			for (int n = 0; n < everts.Length; n++)
			{
				Vector3 p3 = everts[n];
				p3 = Deform(p3, megaShape, start, num, vector3, RemoveDof, locoff3, sploff);
				loftverts[num3] = p3;
				ref Vector2 reference3 = ref loftuvs[num3++];
				reference3 = euvs[n];
			}
			for (int num6 = 0; num6 < etris.Length; num6++)
			{
				lofttris[num4++] = etris[num6] + num2 + triindex;
			}
			num2 += everts.Length;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneSplineSimple to = go.AddComponent<MegaLoftLayerCloneSplineSimple>();
		Copy(this, to);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		return null;
	}
}
