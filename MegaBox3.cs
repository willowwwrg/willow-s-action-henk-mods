using System;
using UnityEngine;

[Serializable]
public class MegaBox3
{
	public Vector3 center;

	public Vector3 min;

	public Vector3 max;

	public float radius;

	public Vector3[] verts = new Vector3[8];

	public Vector3 this[int index]
	{
		get
		{
			return verts[index];
		}
		set
		{
			verts[index] = value;
		}
	}

	public Vector3 Size()
	{
		return max - min;
	}

	public void SetSize(Vector3 size)
	{
		min = -(size * 0.5f);
		max = size * 0.5f;
		center = Vector3.zero;
		radius = size.magnitude;
		CalcVerts();
	}

	public float Radius()
	{
		if (radius <= 0f)
		{
			radius = max.magnitude;
		}
		return radius;
	}

	public override string ToString()
	{
		return string.Concat("cen ", center, " min ", min, " max ", max);
	}

	private Vector3 GetVert(int i)
	{
		Vector3 vector = Size() * 0.5f;
		return i switch
		{
			0 => center + vector, 
			1 => center + Vector3.Scale(vector, new Vector3(-1f, 1f, 1f)), 
			2 => center + Vector3.Scale(vector, new Vector3(1f, 1f, -1f)), 
			3 => center + Vector3.Scale(vector, new Vector3(-1f, 1f, -1f)), 
			4 => center + Vector3.Scale(vector, new Vector3(1f, -1f, 1f)), 
			5 => center + Vector3.Scale(vector, new Vector3(-1f, -1f, 1f)), 
			6 => center + Vector3.Scale(vector, new Vector3(1f, -1f, -1f)), 
			_ => center + Vector3.Scale(vector, new Vector3(-1f, -1f, -1f)), 
		};
	}

	private void CalcVerts()
	{
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref verts[i];
			reference = GetVert(i);
		}
	}

	public static Vector3 GetVert(Bounds bounds, int i)
	{
		return i switch
		{
			0 => bounds.center + bounds.extents, 
			1 => bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1f, 1f, 1f)), 
			2 => bounds.center + Vector3.Scale(bounds.extents, new Vector3(1f, 1f, -1f)), 
			3 => bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1f, 1f, -1f)), 
			4 => bounds.center + Vector3.Scale(bounds.extents, new Vector3(1f, -1f, 1f)), 
			5 => bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1f, -1f, 1f)), 
			6 => bounds.center + Vector3.Scale(bounds.extents, new Vector3(1f, -1f, -1f)), 
			_ => bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1f, -1f, -1f)), 
		};
	}
}
