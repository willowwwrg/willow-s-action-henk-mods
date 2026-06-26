using System;
using UnityEngine;

[Serializable]
public class MegaCloneObj
{
	public Mesh mesh;

	public float Gap;

	public Vector3 Offset;

	public Vector3 Scale;

	public float Weight;

	private Vector3[] mverts;

	private Vector2[] muvs;

	private int[] mtris;
}
