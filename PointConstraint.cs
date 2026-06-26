using UnityEngine;

public class PointConstraint : MegaRopeConstraint
{
	public int p1;

	public Transform obj;

	public PointConstraint(int _p1, Transform trans)
	{
		p1 = _p1;
		obj = trans;
		active = true;
	}

	public override void Apply(MegaRope soft)
	{
		if (active)
		{
			soft.masses[p1].pos = obj.position;
		}
	}
}
