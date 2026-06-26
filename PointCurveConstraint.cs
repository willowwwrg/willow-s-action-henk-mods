using UnityEngine;

public class PointCurveConstraint : MegaRopeConstraint
{
	public int p1;

	public MegaShape shape;

	public float alpha;

	public override void Apply(MegaRope soft)
	{
		if (shape != null)
		{
			Vector3 pos = shape.InterpCurve3D(0, alpha, type: true);
			soft.masses[p1].pos = pos;
		}
	}
}
