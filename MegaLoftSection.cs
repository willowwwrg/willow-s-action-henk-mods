using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaLoftSection
{
	public MegaShape shape;

	public string shapeName = string.Empty;

	public int curve;

	public float alpha;

	public bool snap = true;

	public Vector3[] crossverts;

	public Vector2[] crossuvs;

	public Vector3 crosssize = Vector3.zero;

	public Vector3 crossmin = Vector3.zero;

	public Vector3 crossmax = Vector3.zero;

	public Vector3 offset = Vector3.zero;

	public Vector3 rot = Vector3.zero;

	public Vector3 scale = Vector3.one;

	public bool uselen;

	public float start;

	public float length = 1f;

	public List<MegaMeshSection> meshsections = new List<MegaMeshSection>();
}
