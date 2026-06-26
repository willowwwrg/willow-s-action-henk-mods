using System;
using UnityEngine;

[Serializable]
public class MegaCarriage
{
	public GameObject carriage;

	public GameObject bogey1;

	public GameObject bogey2;

	public Vector3 bogey1Offset = Vector3.zero;

	public Vector3 bogey2Offset = Vector3.zero;

	public Vector3 carriageOffset = Vector3.zero;

	public float bogeyoff;

	public float length;

	public Vector3 rot = Vector3.zero;

	public Vector3 bogey1Rot = Vector3.zero;

	public Vector3 bogey2Rot = Vector3.zero;

	public Vector3 b1;

	public Vector3 b2;

	public Vector3 cp;

	public Vector3 bp1;

	public Vector3 bp2;
}
