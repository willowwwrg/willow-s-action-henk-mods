using System;
using UnityEngine;

[Serializable]
public class MegaKnotAnimCurve
{
	public AnimationCurve px = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve py = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve pz = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve ix = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve iy = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve iz = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve ox = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve oy = new AnimationCurve(new Keyframe(0f, 0f));

	public AnimationCurve oz = new AnimationCurve(new Keyframe(0f, 0f));

	public void GetState(MegaKnot knot, float t)
	{
		knot.p.x = px.Evaluate(t);
		knot.p.y = py.Evaluate(t);
		knot.p.z = pz.Evaluate(t);
		knot.invec.x = ix.Evaluate(t);
		knot.invec.y = iy.Evaluate(t);
		knot.invec.z = iz.Evaluate(t);
		knot.outvec.x = ox.Evaluate(t);
		knot.outvec.y = oy.Evaluate(t);
		knot.outvec.z = oz.Evaluate(t);
	}

	public void AddKey(MegaKnot knot, float t)
	{
		px.AddKey(new Keyframe(t, knot.p.x));
		py.AddKey(new Keyframe(t, knot.p.y));
		pz.AddKey(new Keyframe(t, knot.p.z));
		ix.AddKey(new Keyframe(t, knot.invec.x));
		iy.AddKey(new Keyframe(t, knot.invec.y));
		iz.AddKey(new Keyframe(t, knot.invec.z));
		ox.AddKey(new Keyframe(t, knot.outvec.x));
		oy.AddKey(new Keyframe(t, knot.outvec.y));
		oz.AddKey(new Keyframe(t, knot.outvec.z));
	}

	public void MoveKey(MegaKnot knot, float t, int k)
	{
		px.MoveKey(k, new Keyframe(t, knot.p.x));
		py.MoveKey(k, new Keyframe(t, knot.p.y));
		pz.MoveKey(k, new Keyframe(t, knot.p.z));
		ix.MoveKey(k, new Keyframe(t, knot.invec.x));
		iy.MoveKey(k, new Keyframe(t, knot.invec.y));
		iz.MoveKey(k, new Keyframe(t, knot.invec.z));
		ox.MoveKey(k, new Keyframe(t, knot.outvec.x));
		oy.MoveKey(k, new Keyframe(t, knot.outvec.y));
		oz.MoveKey(k, new Keyframe(t, knot.outvec.z));
	}

	public void RemoveKey(int k)
	{
		px.RemoveKey(k);
		py.RemoveKey(k);
		pz.RemoveKey(k);
		ix.RemoveKey(k);
		iy.RemoveKey(k);
		iz.RemoveKey(k);
		ox.RemoveKey(k);
		oy.RemoveKey(k);
		oz.RemoveKey(k);
	}
}
