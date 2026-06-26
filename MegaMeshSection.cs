using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaMeshSection
{
	public int mat;

	public int vertstart;

	public List<Vector3> verts = new List<Vector3>();

	public List<Vector3> verts1 = new List<Vector3>();

	public List<Vector2> uvs = new List<Vector2>();

	public List<Vector3> cverts = new List<Vector3>();

	public List<Vector2> cuvs = new List<Vector2>();

	public List<Color> ccols = new List<Color>();

	public List<Color> cols = new List<Color>();

	public List<Vector3> cnorms = new List<Vector3>();

	public List<Vector3> norms = new List<Vector3>();

	public List<int> tris = new List<int>();

	public float castart;

	public float caend = 1f;

	public float[] offsets;

	public float[] last;

	public int firstknot;

	public int lastknot;

	public float len;
}
