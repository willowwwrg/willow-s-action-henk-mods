using System;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MegaShapeLoft : MegaShapeBase
{
	public bool realtime = true;

	public bool rebuild = true;

	public Mesh mesh;

	public MegaLoftLayerBase[] Layers;

	public Vector3 CrossScale = Vector3.one;

	public bool Tangents;

	public bool Optimize;

	public bool DoBounds = true;

	public bool DoCollider;

	public Vector3 crossrot = Vector3.zero;

	public Vector3 up = Vector3.up;

	public float tangent = 0.001f;

	private Vector3[] verts;

	private Vector2[] uvs;

	private Vector3[] crossverts;

	private Vector2[] crossuvs;

	private int[] crossids;

	private Matrix4x4 wtm = Matrix4x4.identity;

	private MeshCollider meshCol;

	public int vertcount;

	public int polycount;

	public float startLow = -1f;

	public float startHigh = 1f;

	public float lenLow = 0.001f;

	public float lenHigh = 2f;

	public float crossLow = -1f;

	public float crossHigh = 1f;

	public float crossLenLow = 0.001f;

	public float crossLenHigh = 2f;

	public float distlow = 0.025f;

	public float disthigh = 1f;

	public float cdistlow = 0.025f;

	public float cdisthigh = 1f;

	private static bool updating;

	public bool genLightMap;

	public float angleError = 0.08f;

	public float areaError = 0.15f;

	public float hardAngle = 88f;

	public float packMargin = 0.0039f;

	public bool useColors;

	public Color defaultColor = Color.white;

	private Color[] cols;

	public float conformAmount = 1f;

	public bool undo;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2087");
	}

	public override void SplineNotify(MegaShape shape, int reason)
	{
		for (int i = 0; i < Layers.Length; i++)
		{
			if (Layers[i].SplineNotify(shape, reason))
			{
				rebuild = true;
				break;
			}
		}
	}

	public override void LayerNotify(MegaLoftLayerBase layer, int reason)
	{
		if (Layers == null)
		{
			return;
		}
		for (int i = 0; i < Layers.Length; i++)
		{
			if (Layers[i].LayerNotify(layer, reason))
			{
				rebuild = true;
				break;
			}
		}
	}

	public virtual bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		if (Layers != null)
		{
			for (int i = 0; i < Layers.Length; i++)
			{
				if (Layers[i].LoftNotify(loft, reason))
				{
					rebuild = true;
					break;
				}
			}
		}
		return rebuild;
	}

	private void Start()
	{
		if (Layers != null)
		{
			Layers = GetComponents<MegaLoftLayerBase>();
			for (int i = 0; i < Layers.Length; i++)
			{
				Layers[i].FindShapes();
			}
		}
	}

	private void LateUpdate()
	{
		BuildMeshFromLayersNew();
	}

	public void BuildMeshFromLayersNew()
	{
		if (!rebuild)
		{
			return;
		}
		Layers = GetComponents<MegaLoftLayerBase>();
		if (Layers.Length == 0)
		{
			return;
		}
		if (mesh == null)
		{
			MeshFilter meshFilter = base.gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = base.gameObject.AddComponent<MeshFilter>();
				meshFilter.name = base.gameObject.name;
			}
			meshFilter.sharedMesh = new Mesh();
			if (base.gameObject.GetComponent<MeshRenderer>() == null)
			{
				base.gameObject.AddComponent<MeshRenderer>();
			}
			mesh = meshFilter.sharedMesh;
		}
		if (!realtime)
		{
			rebuild = false;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < Layers.Length; i++)
		{
			if (Layers[i].Valid())
			{
				Layers[i].PrepareLoft(this, i);
				num += Layers[i].NumMaterials();
				num2 += Layers[i].NumVerts();
			}
		}
		if (num2 > 65535)
		{
			Debug.LogWarning("Loft Layer will have too many vertices. Lower the detail settings or disable a layer.");
			return;
		}
		if (verts == null || verts.Length != num2)
		{
			verts = new Vector3[num2];
		}
		if (uvs == null || uvs.Length != num2)
		{
			uvs = new Vector2[num2];
		}
		if (useColors && (cols == null || cols.Length != num2))
		{
			cols = new Color[num2];
		}
		int num3 = 0;
		for (int j = 0; j < Layers.Length; j++)
		{
			if (Layers[j].Valid())
			{
				Layers[j].BuildMesh(this, num3);
				if (useColors)
				{
					Layers[j].CopyVertData(ref verts, ref uvs, ref cols, num3);
				}
				else
				{
					Layers[j].CopyVertData(ref verts, ref uvs, num3);
				}
				num3 += Layers[j].NumVerts();
			}
		}
		mesh.Clear();
		mesh.subMeshCount = num;
		mesh.vertices = verts;
		mesh.uv = uvs;
		if (useColors)
		{
			mesh.colors = cols;
		}
		vertcount = verts.Length;
		polycount = 0;
		Material[] array = new Material[num];
		int num4 = 0;
		for (int k = 0; k < Layers.Length; k++)
		{
			if (Layers[k].Valid())
			{
				for (int l = 0; l < Layers[k].NumMaterials(); l++)
				{
					array[num4] = Layers[k].GetMaterial(l);
					int[] tris = Layers[k].GetTris(l);
					mesh.SetTriangles(tris, num4);
					polycount += tris.Length;
					num4++;
				}
			}
		}
		mesh.RecalculateNormals();
		if (Tangents)
		{
			MegaUtils.BuildTangents(mesh);
		}
		if (Optimize)
		{
			mesh.Optimize();
		}
		if (DoBounds)
		{
			mesh.RecalculateBounds();
		}
		MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.sharedMaterials = array;
		}
		if (DoCollider)
		{
			meshCol = GetComponent<MeshCollider>();
			if (meshCol != null)
			{
				meshCol.sharedMesh = null;
				meshCol.sharedMesh = mesh;
			}
		}
		if (updating)
		{
			return;
		}
		updating = true;
		MegaShapeLoft[] array2 = (MegaShapeLoft[])UnityEngine.Object.FindObjectsOfType(typeof(MegaShapeLoft));
		for (int m = 0; m < array2.Length; m++)
		{
			if (array2[m] != this && array2[m].LoftNotify(this, 0))
			{
				array2[m].BuildMeshFromLayersNew();
			}
		}
		updating = false;
	}

	public Matrix4x4 GetDeformMat(MegaSpline spline, float alpha, bool interp)
	{
		int k = -1;
		Vector3 p;
		if (spline.closed)
		{
			alpha = Mathf.Repeat(alpha, 1f);
			p = spline.Interpolate(alpha, interp, ref k);
		}
		else
		{
			p = spline.InterpCurve3D(alpha, interp, ref k);
		}
		alpha += tangent;
		Vector3 forward;
		if (spline.closed)
		{
			alpha %= 1f;
			forward = spline.Interpolate(alpha, interp, ref k);
		}
		else
		{
			forward = spline.InterpCurve3D(alpha, interp, ref k);
		}
		forward.x -= p.x;
		forward.y = 0f;
		forward.z -= p.z;
		MegaMatrix.SetTR(ref wtm, p, Quaternion.LookRotation(forward, up));
		return wtm;
	}

	public Matrix4x4 GetDeformMatNew(MegaSpline spline, float alpha, bool interp, float align)
	{
		int k = -1;
		Vector3 p;
		if (spline.closed)
		{
			alpha = Mathf.Repeat(alpha, 1f);
			p = spline.Interpolate(alpha, interp, ref k);
		}
		else
		{
			p = spline.InterpCurve3D(alpha, interp, ref k);
		}
		alpha += tangent;
		Vector3 forward;
		if (spline.closed)
		{
			alpha %= 1f;
			forward = spline.Interpolate(alpha, interp, ref k);
		}
		else
		{
			forward = spline.InterpCurve3D(alpha, interp, ref k);
		}
		forward.x -= p.x;
		forward.y -= p.y;
		forward.z -= p.z;
		forward.y *= align;
		MegaMatrix.SetTR(ref wtm, p, Quaternion.LookRotation(forward, up));
		return wtm;
	}

	public Matrix4x4 GetDeformMatNewMethod(MegaSpline spline, float alpha, bool interp, float align, ref Vector3 lastup)
	{
		int k = -1;
		Vector3 p;
		if (spline.closed)
		{
			alpha = Mathf.Repeat(alpha, 1f);
			p = spline.Interpolate(alpha, interp, ref k);
		}
		else
		{
			p = spline.InterpCurve3D(alpha, interp, ref k);
		}
		alpha += tangent;
		Vector3 forward;
		if (spline.closed)
		{
			alpha %= 1f;
			forward = spline.Interpolate(alpha, interp, ref k);
		}
		else
		{
			forward = spline.InterpCurve3D(alpha, interp, ref k);
		}
		Vector3 vector = default(Vector3);
		forward.x = (vector.x = forward.x - p.x);
		forward.y = (vector.y = forward.y - p.y);
		forward.z = (vector.z = forward.z - p.z);
		forward.y *= align;
		MegaMatrix.SetTR(ref wtm, p, Quaternion.LookRotation(forward, lastup));
		vector = vector.normalized;
		Vector3 lhs = Vector3.Cross(vector, lastup);
		lastup = Vector3.Cross(lhs, vector);
		return wtm;
	}

	[ContextMenu("Clone")]
	public void Clone()
	{
		GameObject gameObject = new GameObject();
		MegaShapeLoft megaShapeLoft = gameObject.AddComponent<MegaShapeLoft>();
		Copy(megaShapeLoft);
		megaShapeLoft.mesh = null;
		MegaLoftLayerBase[] components = GetComponents<MegaLoftLayerBase>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Copy(gameObject);
		}
		megaShapeLoft.rebuild = true;
		megaShapeLoft.BuildMeshFromLayersNew();
		gameObject.name = base.name + " clone";
	}

	[ContextMenu("Clone New")]
	public void CloneNew()
	{
		GameObject gameObject = new GameObject();
		MegaShapeLoft megaShapeLoft = gameObject.AddComponent<MegaShapeLoft>();
		Copy(megaShapeLoft);
		megaShapeLoft.mesh = null;
		MegaLoftLayerBase[] components = GetComponents<MegaLoftLayerBase>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Copy(gameObject);
		}
		megaShapeLoft.verts = null;
		megaShapeLoft.uvs = null;
		megaShapeLoft.cols = null;
		megaShapeLoft.rebuild = true;
		megaShapeLoft.BuildMeshFromLayersNew();
		gameObject.name = base.name + " clone";
	}

	public void Copy(MegaShapeLoft to)
	{
		Type type = GetType();
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			fields[i].SetValue(to, fields[i].GetValue(this));
		}
		for (int j = 0; j < properties.Length; j++)
		{
			if (properties[j].CanWrite)
			{
				properties[j].SetValue(to, properties[j].GetValue(this, null), null);
			}
		}
	}
}
