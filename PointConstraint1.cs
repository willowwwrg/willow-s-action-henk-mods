using UnityEngine;

public class PointConstraint1 : MegaRopeConstraint
{
	public int p1;

	public Vector3 off;

	public Transform obj;

	public override void Apply(MegaRope soft)
	{
		if (active)
		{
			soft.masses[p1].pos = obj.position + obj.localToWorldMatrix.MultiplyVector(off);
		}
	}
}
