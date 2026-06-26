using System;
using System.Reflection;
using UnityEngine;

public class MegaLoftLayerBase : MonoBehaviour
{
	public string LayerName = "No-name";

	public bool LayerEnabled = true;

	public Vector3[] loftverts;

	public Vector2[] loftuvs;

	public Vector3[] loftnormals;

	public Color[] loftcols;

	public int[] lofttris;

	public Material material;

	public MegaShape layerPath;

	public MegaShape layerSection;

	public Color paramcol = Color.white;

	public bool Lock;

	public int linked = -1;

	public MegaFrameMethod frameMethod;

	public string pathName = string.Empty;

	public string sectionName = string.Empty;

	public bool optimize;

	public float maxdeviation = 5f;

	public Vector3[] optverts;

	public Vector2[] optuvs;

	public Vector3[] optnormals;

	public Color[] optcols;

	public int[] opttris;

	public int trisstart;

	public virtual Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public virtual bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		return false;
	}

	public virtual int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		return triindex;
	}

	public virtual bool MeshBased()
	{
		return true;
	}

	public virtual bool SplineNotify(MegaShape shape, int reason)
	{
		if (shape == layerPath)
		{
			return true;
		}
		if (shape == layerSection)
		{
			return true;
		}
		return false;
	}

	public virtual bool LayerNotify(MegaLoftLayerBase layer, int reason)
	{
		return false;
	}

	public virtual bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		return false;
	}

	public virtual bool Valid()
	{
		return false;
	}

	public virtual int NumVerts()
	{
		return loftverts.Length;
	}

	public virtual int NumMaterials()
	{
		return 1;
	}

	public virtual Material GetMaterial(int i)
	{
		return material;
	}

	public virtual int[] GetTris(int i)
	{
		return lofttris;
	}

	public virtual void FindShapes()
	{
		if (layerPath == null && pathName.Length > 0)
		{
			GameObject gameObject = GameObject.Find(pathName);
			if ((bool)gameObject)
			{
				layerPath = gameObject.GetComponent<MegaShape>();
			}
		}
		if (layerSection == null && sectionName.Length > 0)
		{
			GameObject gameObject2 = GameObject.Find(sectionName);
			if ((bool)gameObject2)
			{
				layerSection = gameObject2.GetComponent<MegaShape>();
			}
		}
	}

	public virtual void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
	}

	public virtual void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
		Array.Copy(loftcols, 0, cols, offset, loftcols.Length);
	}

	public virtual MegaLoftLayerBase Copy(GameObject go)
	{
		return null;
	}

	public void Copy(MegaLoftLayerBase from, MegaLoftLayerBase to)
	{
		bool flag = false;
		Type type = from.GetType();
		flag = (type.IsSubclassOf(typeof(Behaviour)) ? from.enabled : (!type.IsSubclassOf(typeof(Component)) || type.GetProperty("enabled") == null || (bool)type.GetProperty("enabled").GetValue(from, null)));
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		if (type.IsSubclassOf(typeof(Behaviour)))
		{
			to.enabled = flag;
		}
		else if (type.IsSubclassOf(typeof(Component)) && type.GetProperty("enabled") != null)
		{
			type.GetProperty("enabled").SetValue(to, flag, null);
		}
		for (int i = 0; i < fields.Length; i++)
		{
			fields[i].SetValue(to, fields[i].GetValue(from));
		}
		for (int j = 0; j < properties.Length; j++)
		{
			if (properties[j].CanWrite)
			{
				properties[j].SetValue(to, properties[j].GetValue(from, null), null);
			}
		}
	}

	public virtual void CopyLayer(MegaLoftLayerBase from)
	{
		layerPath = from.layerPath;
		layerSection = from.layerSection;
		if ((bool)layerPath)
		{
			pathName = layerPath.gameObject.name;
		}
		if ((bool)layerSection)
		{
			sectionName = layerSection.gameObject.name;
		}
	}
}
