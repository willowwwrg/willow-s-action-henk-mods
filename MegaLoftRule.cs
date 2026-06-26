using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaLoftRule
{
	public string rulename = "No name";

	public GameObject obj;

	public bool enabled;

	public Vector3 offset;

	public Vector3 scale;

	public float gapin;

	public float gapout;

	public Material[] mats;

	public Bounds bounds;

	public List<MegaLoftTris> lofttris = new List<MegaLoftTris>();

	public Vector3[] verts;

	public Vector2[] uvs;

	public int[] tris;

	public int usage;

	public float tweight;

	public MegaLoftRuleType type = MegaLoftRuleType.Filler;

	public float weight = 1f;

	public int count = 1;

	public float alpha = 0.5f;

	public bool used;

	public int numtris;
}
