using UnityEngine;

public class NewPointConstraint : MegaRopeConstraint
{
	public int p1;

	public int p2;

	public float length;

	public Transform obj;

	public NewPointConstraint(int _p1, int _p2, float _len, Transform trans)
	{
		p1 = _p1;
		p2 = _p2;
		length = _len;
		obj = trans;
		active = true;
	}

	public override void Apply(MegaRope soft)
	{
		if (active)
		{
			soft.masses[p1].pos = obj.position;
			soft.masses[p1].last = soft.masses[p1].pos;
			Vector3 vector = soft.masses[p2].pos - soft.masses[p1].pos;
			float magnitude = vector.magnitude;
			if (vector != Vector3.zero)
			{
				vector.Normalize();
				Vector3 vector2 = 1f * (magnitude - length) * vector;
				soft.masses[p2].pos += -vector2;
				soft.masses[p2].last = soft.masses[p2].pos;
			}
		}
	}
}
