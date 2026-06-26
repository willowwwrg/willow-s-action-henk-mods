using System;
using UnityEngine;

public class MegaLoftLayerScatterSpline : MegaLoftLayerBase
{
	public Mesh scatterMesh;

	public float GlobalScale = 1f;

	public int Count = 4;

	public float tangent = 0.1f;

	public MegaAxis axis;

	public Vector3 rot = Vector3.zero;

	public Vector3 scale = Vector3.one;

	public float start;

	public float length = 1f;

	public int Seed;

	public bool useDensity;

	public int curve;

	public bool snap;

	public AnimationCurve density = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 Offset = Vector3.zero;

	public Vector3 scaleRangeMin = Vector3.zero;

	public Vector3 scaleRangeMax = Vector3.zero;

	public Vector3 rotRange = Vector3.zero;

	public float Alpha;

	public float Speed = 0.1f;

	public bool useTwist;

	public float twist;

	public AnimationCurve twistCrv = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float RemoveDof = 1f;

	public Vector3 rotPath = Vector3.zero;

	public Vector3 offPath = Vector3.zero;

	public Vector3 sclPath = Vector3.one;

	private Vector3[] sverts;

	private Vector2[] suvs;

	private int[] stris;

	private Matrix4x4 tm;

	private Quaternion tw;

	private Quaternion meshrot;

	private Matrix4x4 wtm;

	private Matrix4x4 pathtm;

	private Matrix4x4 mat;

	private float LayerLength;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2184");
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)scatterMesh && (bool)layerPath)
		{
			return true;
		}
		return false;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	private void Init()
	{
		if (scatterMesh != null)
		{
			sverts = scatterMesh.vertices;
			suvs = scatterMesh.uv;
			stris = scatterMesh.triangles;
		}
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		Init();
		int num = 0;
		int num2 = 0;
		if ((bool)scatterMesh)
		{
			num += sverts.Length * Count;
			num2 += stris.Length * Count;
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

	private Vector3 Deform(MegaShape path, float alpha, float off, Vector3 scale, float removeDof, Vector3 locoff, out Matrix4x4 omat, Vector3 sploff)
	{
		float num = 0f;
		Vector3 vector = pathtm.MultiplyPoint(path.InterpCurve3D(curve, alpha, path.normalizedInterp, ref num) + sploff);
		Vector3 vector2 = pathtm.MultiplyPoint(path.InterpCurve3D(curve, alpha + tangent * 0.001f, path.normalizedInterp) + sploff);
		alpha = ((!path.splines[curve].closed) ? Mathf.Clamp01(alpha) : Mathf.Repeat(alpha, 1f));
		if (useTwist)
		{
			tw = meshrot * Quaternion.AngleAxis(twist * twistCrv.Evaluate(alpha) + num, Vector3.forward);
		}
		Vector3 forward = vector2 - vector;
		forward.y *= removeDof;
		Quaternion q = Quaternion.LookRotation(forward) * tw;
		wtm.SetTRS(vector, q, scale);
		omat = mat * wtm;
		return vector;
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
		if (layerPath == null)
		{
			return triindex;
		}
		LayerLength = 1f / layerPath.splines[curve].length;
		if (tangent < 0.1f)
		{
			tangent = 0.1f;
		}
		mat = base.transform.localToWorldMatrix * layerPath.transform.worldToLocalMatrix;
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
		tw = meshrot;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		pathtm.SetTRS(offPath, Quaternion.Euler(rotPath), sclPath);
		Matrix4x4 omat = Matrix4x4.identity;
		UnityEngine.Random.seed = Seed;
		Vector3 zero = Vector3.zero;
		Vector3 vector = Vector3.Scale(scaleRangeMin, scale);
		Vector3 vector2 = Vector3.Scale(scaleRangeMax, scale);
		Vector3 sploff = Vector3.zero;
		if (snap)
		{
			sploff = layerPath.splines[0].knots[0].p - layerPath.splines[curve].knots[0].p;
		}
		float num4 = 0f;
		for (int i = 0; i < Count; i++)
		{
			zero.x = (scale.x + Mathf.Lerp(vector.x, vector2.x, UnityEngine.Random.value)) * GlobalScale;
			zero.y = (scale.y + Mathf.Lerp(vector.y, vector2.y, UnityEngine.Random.value)) * GlobalScale;
			zero.z = (scale.z + Mathf.Lerp(vector.z, vector2.z, UnityEngine.Random.value)) * GlobalScale;
			num4 = ((!useDensity) ? UnityEngine.Random.value : FindScatterAlpha());
			float alpha = start + num4 * length + Alpha;
			Vector3 euler = rot + (UnityEngine.Random.value - 0.5f) * 2f * rotRange;
			meshrot = Quaternion.Euler(euler);
			Vector3 vector3 = Deform(layerPath, alpha, 0f, zero, RemoveDof, Vector3.zero, out omat, sploff);
			Vector3 v = Vector3.zero;
			for (int j = 0; j < sverts.Length; j++)
			{
				v.x = sverts[j].x + Offset.x;
				v.y = sverts[j].y + Offset.y;
				v.z = sverts[j].z + Offset.z;
				v = omat.MultiplyPoint3x4(v);
				loftverts[num2].x = v.x + vector3.x;
				loftverts[num2].y = v.y + vector3.y;
				loftverts[num2].z = v.z + vector3.z;
				ref Vector2 reference = ref loftuvs[num2++];
				reference = suvs[j];
			}
			for (int k = 0; k < stris.Length; k++)
			{
				lofttris[num3++] = stris[k] + num + triindex;
			}
			num = num2;
		}
		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerScatterSpline to = go.AddComponent<MegaLoftLayerScatterSpline>();
		Copy(this, to);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		return null;
	}
}
