using System;
using UnityEngine;

public class MegaLoftLayerScatterSimple : MegaLoftLayerBase
{
	public Mesh scatterMesh;

	public float percent;

	public float GlobalScale = 1f;

	public float RemoveDof = 1f;

	public int Count = 4;

	public float tangent = 0.1f;

	public MegaAxis axis;

	public Vector3 rot = Vector3.zero;

	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public bool CalcUp;

	public Vector3 scale = Vector3.one;

	public float start;

	public float length = 1f;

	public float cstart;

	public float clength = 1f;

	public int Seed;

	public bool useDensity;

	public AnimationCurve density = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public Vector3 Offset = Vector3.zero;

	public Vector3 scaleRangeMin = Vector3.zero;

	public Vector3 scaleRangeMax = Vector3.zero;

	public Vector3 rotRange = Vector3.zero;

	public float Alpha;

	public float CAlpha;

	public float Speed = 0.1f;

	private Vector3[] sverts;

	private Vector2[] suvs;

	private int[] stris;

	private Matrix4x4 tm;

	private Quaternion tw;

	private Quaternion meshrot;

	private Matrix4x4 wtm;

	private Matrix4x4 mat;

	private float LayerLength;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2180");
	}

	public override bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		if (surfaceLoft != null && surfaceLoft == loft)
		{
			return true;
		}
		return false;
	}

	public override bool Valid()
	{
		if (LayerEnabled && (bool)scatterMesh && (bool)surfaceLoft && surfaceLayer >= 0)
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
		if (scatterMesh == null && surfaceLoft == null && surfaceLayer < 0)
		{
			return false;
		}
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
		if (surfaceLoft == null && surfaceLayer < 0)
		{
			return triindex;
		}
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
		int num3 = 0;
		Matrix4x4 identity = Matrix4x4.identity;
		UnityEngine.Random.seed = Seed;
		Vector3 up = Vector3.up;
		Vector3 zero = Vector3.zero;
		Vector3 vector = Vector3.Scale(scaleRangeMin, scale);
		Vector3 vector2 = Vector3.Scale(scaleRangeMax, scale);
		float num4 = 0f;
		for (int i = 0; i < Count; i++)
		{
			zero.x = (scale.x + Mathf.Lerp(vector.x, vector2.x, UnityEngine.Random.value)) * GlobalScale;
			zero.y = (scale.y + Mathf.Lerp(vector.y, vector2.y, UnityEngine.Random.value)) * GlobalScale;
			zero.z = (scale.z + Mathf.Lerp(vector.z, vector2.z, UnityEngine.Random.value)) * GlobalScale;
			num4 = ((!useDensity) ? UnityEngine.Random.value : FindScatterAlpha());
			float a = start + num4 * length + Alpha;
			float ca = cstart + UnityEngine.Random.value * clength + CAlpha;
			Vector3 euler = rot + (UnityEngine.Random.value - 0.5f) * 2f * rotRange;
			meshrot = Quaternion.Euler(euler);
			Vector3 p;
			if (CalcUp)
			{
				Vector3 pos = megaLoftLayerSimple.GetPos1(surfaceLoft, ca, a, 0.001f, out p, out up);
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, up);
				tw = Quaternion.AngleAxis(180f, Vector3.forward) * quaternion;
				Vector3 vector3 = p - pos;
				vector3.y *= RemoveDof;
				if (vector3 == Vector3.zero)
				{
					vector3 = Vector3.forward;
				}
				Quaternion q = Quaternion.LookRotation(vector3) * tw * meshrot;
				wtm.SetTRS(pos, q, zero);
				identity = mat * wtm;
			}
			else
			{
				Vector3 pos2 = megaLoftLayerSimple.GetPos1(surfaceLoft, ca, a, 0.001f, out p, out up);
				tw = Quaternion.AngleAxis(180f, Vector3.forward);
				Vector3 forward = p - pos2;
				forward.y *= RemoveDof;
				Quaternion q2 = Quaternion.LookRotation(forward) * tw * meshrot;
				wtm.SetTRS(pos2, q2, zero);
				identity = mat * wtm;
			}
			Vector3 v = Vector3.zero;
			for (int j = 0; j < sverts.Length; j++)
			{
				v.x = sverts[j].x + Offset.x;
				v.y = sverts[j].y + Offset.y;
				v.z = sverts[j].z + Offset.z;
				v = identity.MultiplyPoint3x4(v);
				loftverts[num2].x = v.x;
				loftverts[num2].y = v.y;
				loftverts[num2].z = v.z;
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
		MegaLoftLayerScatterSimple megaLoftLayerScatterSimple = go.AddComponent<MegaLoftLayerScatterSimple>();
		Copy(this, megaLoftLayerScatterSimple);
		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;
		if (megaLoftLayerScatterSimple.surfaceLoft == GetComponent<MegaShapeLoft>())
		{
			megaLoftLayerScatterSimple.surfaceLoft = go.GetComponent<MegaShapeLoft>();
		}
		return null;
	}
}
