using UnityEngine;

public class MegaRopeSolverVertlet : MegaRopeSolver
{
	private void doCalculateForces(MegaRope rope)
	{
		for (int i = 0; i < rope.masses.Count; i++)
		{
			rope.masses[i].force.x = rope.masses[i].mass * rope.gravity.x;
			rope.masses[i].force.y = rope.masses[i].mass * rope.gravity.y;
			rope.masses[i].force.z = rope.masses[i].mass * rope.gravity.z;
		}
		for (int j = 0; j < rope.springs.Count; j++)
		{
			rope.springs[j].doCalculateSpringForce1(rope);
		}
	}

	private void DoConstraints(MegaRope rope)
	{
		for (int i = 0; i < rope.iters; i++)
		{
			for (int j = 0; j < rope.constraints.Count; j++)
			{
				rope.constraints[j].Apply(rope);
			}
		}
	}

	public override void doIntegration1(MegaRope rope, float dt)
	{
		doCalculateForces(rope);
		float num = dt * dt;
		for (int i = 0; i < rope.masses.Count; i++)
		{
			Vector3 pos = rope.masses[i].pos;
			rope.masses[i].pos += rope.airdrag * (rope.masses[i].pos - rope.masses[i].last) + rope.masses[i].force * rope.masses[i].oneovermass * num;
			rope.masses[i].vel = (rope.masses[i].pos - pos) / dt;
			rope.masses[i].last = pos;
		}
		if (rope.SelfCollide)
		{
			SelfCollide(rope);
		}
		DoConstraints(rope);
		if (rope.DoCollide)
		{
			DoCollisions(rope, dt);
		}
		if (rope.SelfCollide)
		{
			SelfCollide(rope);
		}
	}

	private void DoCollisions(MegaRope rope, float dt)
	{
		for (int i = 0; i < rope.masses.Count; i++)
		{
			Vector3 origin = rope.masses[i].last - Vector3.down * 10f;
			rope.masses[i].collide = false;
			if (Physics.CheckSphere(rope.masses[i].pos, rope.radius, rope.layer) && Physics.SphereCast(origin, rope.radius, Vector3.down, out var hitInfo, rope.masses[i].vel.magnitude * dt + 20f, rope.layer) && hitInfo.distance < 10f)
			{
				rope.masses[i].pos = hitInfo.point + hitInfo.normal * (rope.radius * 1.001f);
				Response(i, hitInfo, rope);
				rope.masses[i].collide = true;
			}
		}
	}

	private void Response(int i, RaycastHit hit, MegaRope rope)
	{
		float num = Vector3.Dot(hit.normal, rope.masses[i].vel);
		Vector3 vel = hit.normal * num;
		vel *= rope.bounce;
		rope.masses[i].vel = vel;
		rope.masses[i].last = rope.masses[i].pos;
	}

	private void SelfCollide2(MegaRope rope)
	{
		float num = rope.radius * 2f;
		num *= num;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < rope.masses.Count; i++)
		{
			Vector3 vector = rope.masses[i].pos;
			Vector3 last = rope.masses[i].last;
			Vector3 normalized = (vector - last).normalized;
			for (int j = i + 2; j < rope.masses.Count; j++)
			{
				Vector3 pos = rope.masses[j].pos;
				zero.x = pos.x - vector.x;
				zero.y = pos.y - vector.y;
				zero.z = pos.z - vector.z;
				float num2 = zero.sqrMagnitude;
				if (!(num2 < num))
				{
					continue;
				}
				float num3 = 1f / Mathf.Sqrt(num2);
				zero.x *= num3;
				zero.y *= num3;
				zero.z *= num3;
				if (rope.masses[i].collide)
				{
					rope.masses[j].pos = vector + zero * num;
					rope.masses[j].last = vector + zero * num;
					continue;
				}
				if (rope.masses[j].collide)
				{
					vector = rope.masses[j].pos - zero * num;
					rope.masses[i].pos = vector;
					rope.masses[i].last = vector;
					continue;
				}
				Vector3 normalized2 = (rope.masses[j].pos - rope.masses[j].last).normalized;
				int num4 = 8;
				while (num2 < num)
				{
					float num5 = num - num2;
					vector -= normalized * num5;
					rope.masses[j].pos -= normalized2 * num5;
					num2 = Vector3.Magnitude(rope.masses[j].pos - vector);
					num4--;
					if (num4 < 0)
					{
						break;
					}
				}
				rope.masses[i].pos = vector;
				rope.masses[i].last = vector;
				rope.masses[j].last = rope.masses[j].pos;
			}
		}
	}

	private void SelfCollide(MegaRope rope)
	{
		float num = rope.radius * 2f;
		for (int i = 0; i < rope.masses.Count; i++)
		{
			Vector3 vector = rope.masses[i].pos;
			for (int j = i + 2; j < rope.masses.Count; j++)
			{
				if (Vector3.Magnitude(rope.masses[j].pos - vector) < num)
				{
					Vector3 normalized = (rope.masses[j].pos - vector).normalized;
					if (rope.masses[i].collide)
					{
						rope.masses[j].pos = vector + normalized * num;
						rope.masses[j].last = vector + normalized * num;
						continue;
					}
					if (rope.masses[j].collide)
					{
						vector = rope.masses[j].pos - normalized * num;
						rope.masses[i].pos = vector;
						rope.masses[i].last = vector;
						continue;
					}
					Vector3 vector2 = (vector + rope.masses[j].pos) * 0.5f;
					vector = vector2 + normalized * rope.radius;
					rope.masses[j].pos = vector;
					rope.masses[j].last = vector;
					vector = vector2 - normalized * rope.radius;
					rope.masses[i].pos = vector;
					rope.masses[i].last = vector;
				}
			}
		}
	}

	private void SelfCollide1(MegaRope rope)
	{
		float num = rope.radius * 2f;
		for (int i = 0; i < rope.masses.Count; i++)
		{
			Vector3 vector = rope.masses[i].pos;
			Vector3 last = rope.masses[i].last;
			if (Vector3.Distance(vector, last) > num)
			{
				Debug.Log("Mass " + i + " moved too much");
			}
			for (int j = i + 2; j < rope.masses.Count; j++)
			{
				if (Vector3.Magnitude(rope.masses[j].pos - vector) < num)
				{
					Vector3 normalized = (rope.masses[j].pos - vector).normalized;
					if (vector.y > rope.masses[j].pos.y)
					{
						vector = rope.masses[j].pos - normalized * num;
						rope.masses[i].pos = vector;
						rope.masses[i].last = vector;
					}
					else
					{
						rope.masses[j].pos = vector + normalized * num;
						rope.masses[j].last = vector + normalized * num;
					}
				}
			}
		}
	}
}
