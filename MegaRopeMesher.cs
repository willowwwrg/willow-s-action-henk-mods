using System;
using UnityEngine;

[Serializable]
public class MegaRopeMesher
{
	public Vector3[] verts;

	public Vector2[] uvs;

	public int[] tris;

	public bool show;

	public virtual void BuildMesh(MegaRope rope)
	{
	}
}
