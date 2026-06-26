using System;
using UnityEngine;

[Serializable]
public class MegaMaterialSection
{
	public int id;

	public Material mat;

	public Vector2 UVOffset = Vector2.zero;

	public Vector2 UVRotate = Vector2.zero;

	public Vector2 UVScale = Vector2.one;

	public bool includeknots = true;

	public bool swapuv = true;

	public bool physuv = true;

	public bool uvcalcy;

	public float cdist = 1f;

	public int steps = 1;

	public bool show;

	public bool Enabled = true;

	public MegaLoftColMode colmode = MegaLoftColMode.Path;

	public float coloffset;

	public Color color = Color.white;

	public AnimationCurve colR = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve colG = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve colB = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve colA = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public float vertscale = 1f;

	public bool collider;
}
