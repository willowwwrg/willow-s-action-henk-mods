using UnityEngine;

public class MegaRopeSolverDefault : MegaRopeSolver
{
	private void doCalculateForces(MegaRope rope)
	{
		for (int i = 0; i < rope.masses.Count; i++)
		{
			rope.masses[i].force = rope.masses[i].mass * rope.gravity;
			rope.masses[i].force += -rope.masses[i].vel * rope.airdrag;
		}
		for (int j = 0; j < rope.springs.Count; j++)
		{
			rope.springs[j].doCalculateSpringForce(rope);
		}
	}

	public override void doIntegration1(MegaRope rope, float dt)
	{
		doCalculateForces(rope);
		for (int i = 0; i < rope.masses.Count; i++)
		{
			rope.masses[i].last = rope.masses[i].pos;
			rope.masses[i].vel += dt * rope.masses[i].force * rope.masses[i].oneovermass;
			rope.masses[i].pos += rope.masses[i].vel * dt;
			rope.masses[i].vel *= rope.friction;
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
			if (Physics.CheckSphere(rope.masses[i].last, rope.radius, rope.layer) && Physics.SphereCast(origin, rope.radius, Vector3.down, out var hitInfo, rope.masses[i].vel.magnitude * dt + 20f, rope.layer) && hitInfo.distance < 10f)
			{
				rope.masses[i].pos = hitInfo.point + hitInfo.normal * (rope.radius * 1.001f);
				Response(i, hitInfo, rope);
			}
		}
	}

	private void DoCollisions1(MegaRope rope, float dt)
	{
		for (int i = 0; i < rope.masses.Count; i++)
		{
			if (Physics.CheckSphere(rope.masses[i].pos, rope.radius, rope.layer) && Physics.SphereCast(rope.masses[i].last, rope.radius, rope.masses[i].vel.normalized, out var hitInfo, rope.masses[i].vel.magnitude * dt * 2f, rope.layer))
			{
				rope.masses[i].pos = hitInfo.point + hitInfo.normal * (rope.radius * 1.05f);
				Response(i, hitInfo, rope);
			}
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

	private void Response(int i, RaycastHit hit, MegaRope rope)
	{
		float num = Vector3.Dot(hit.normal, rope.masses[i].vel);
		Vector3 vel = hit.normal * num;
		vel *= rope.bounce;
		rope.masses[i].vel = vel;
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
					if (vector.y > rope.masses[j].pos.y)
					{
						vector = rope.masses[j].pos - normalized * num;
						rope.masses[i].pos = vector;
					}
					else
					{
						rope.masses[j].pos = vector + normalized * num;
					}
				}
			}
		}
	}

	private void Collide(int i, float dt, MegaRope rope)
	{
	}

	private void CollideCapsule(int i, float dt, MegaRope rope)
	{
		if (i < rope.masses.Count - 1 && Physics.CapsuleCast(rope.masses[i].pos, rope.masses[i + 1].pos, rope.radius, rope.masses[i].vel.normalized, out var hitInfo, 0.1f, rope.layer))
		{
			rope.masses[i].pos = hitInfo.point + hitInfo.normal * (rope.radius * 1.01f);
			Response(i, hitInfo, rope);
		}
	}
}
