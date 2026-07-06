using UnityEngine;

public class RaycastCollider : MonoBehaviour
{
	private PlatformerPhysics platformerPhysics;

	public float radius = 1f;

	public Vector3 velocity = Vector3.zero;

	[HideInInspector]
	public Vector3 lastNormal;

	[HideInInspector]
	public Vector3 predictedNormal;

	[HideInInspector]
	public float predictedTimeLeft;

	[HideInInspector]
	public Vector3 predictedPosition;

	[HideInInspector]
	public Vector3 prevVelocity;

	public bool drawRays = true;

	private bool breakMotion;

	private void Start()
	{
		lastNormal = Vector3.up;
		platformerPhysics = GetComponent<PlatformerPhysics>();
	}

	public void AddVelocity(Vector3 vel)
	{
		velocity += vel * Time.fixedDeltaTime;
	}

	public void FixedUpdate()
	{
		IterativeCollisionCheck(0);
		if (!platformerPhysics.onGround)
		{
			Move(velocity * Time.fixedDeltaTime);
		}
		else
		{
			Vector3 vector = Vector3.Dot(velocity, lastNormal) * lastNormal;
			Vector3 vector2 = velocity - vector;
			Move(vector * Time.fixedDeltaTime);
			Move(vector2 * Time.fixedDeltaTime);
		}
		prevVelocity = velocity;
	}

	public void Move(Vector3 motion)
	{
		breakMotion = false;
		float num = 0.1f;
		float num2 = 0.02f;
		int num3 = 0;
		RaycastCollisionInfo raycastCollisionInfo = default(RaycastCollisionInfo);
		while (motion.magnitude > 0.001f && !breakMotion)
		{
			num3++;
			float magnitude = motion.magnitude;
			Vector3 normalized = motion.normalized;
			Vector3 vector = Vector3.up;
			Vector3 vector2 = Vector3.zero;
			float num4 = 10000f;
			int layerMask = -49157;
			if (Physics.SphereCast(base.transform.position - normalized * num, radius, normalized, out var hitInfo, 1000f, layerMask))
			{
				vector = hitInfo.normal;
				num4 = hitInfo.distance - num2 - num;
				vector2 = hitInfo.point - num4 * normalized;
			}
			if (magnitude > num4 && num3 < 5)
			{
			if (Mathf.Abs(vector.y) < 0.05f)
			{
			vector.y = 0f;
			vector.Normalize();
			}
			Vector3 vector3 = vector;
			Vector3 vector4 = Vector3.up;
			Vector3 vector5 = Vector3.up;
			Vector3 vector6 = hitInfo.point - base.transform.position;
			Vector3 normalized2 = Vector3.Cross(vector6, base.transform.forward).normalized;
			if (Physics.Raycast(base.transform.position, vector6 + normalized2 * 0.05f, out var hitInfo2, 1000f, layerMask))
			{
			vector4 = hitInfo2.normal;
			}
			if (Physics.Raycast(base.transform.position, vector6 - normalized2 * 0.05f, out hitInfo2, 1000f, layerMask))
			{
			vector5 = hitInfo2.normal;
			}
				float num5 = Vector3.Angle(vector4, vector5);
				if (num5 > 45f && num5 < 120f && velocity.magnitude > 10f)
				{
					vector = ((!(Vector3.Angle(vector, vector4) < Vector3.Angle(vector, vector5))) ? vector5 : vector4);
				}
				float num6 = num4 / magnitude;
				Vector3 vector7 = motion * num6;
				base.transform.position += vector7;
				motion -= vector7;
				Vector3 vector8 = vector * Vector3.Dot(vector, motion);
				motion -= vector8;
				Vector3 velocityBeforeImpact = velocity;
				Vector3 vector9 = Vector3.Dot(vector, velocity) * vector;
				bool isConcave = false;
				if (Vector3.Dot(vector, velocity) < 0f)
				{
					isConcave = true;
					velocity -= vector9;
				}
				raycastCollisionInfo.point = vector2 + vector7;
				raycastCollisionInfo.normal = vector;
				raycastCollisionInfo.directionToGround = -vector3;
				raycastCollisionInfo.velocityBeforeImpact = velocityBeforeImpact;
				raycastCollisionInfo.velocityAfterImpact = velocity;
				raycastCollisionInfo.isConcave = isConcave;
				if (!Application.isPlaying)
				{
					platformerPhysics.HasCollision(raycastCollisionInfo);
				}
				else
				{
					SendMessage("HasCollision", raycastCollisionInfo);
				}
				lastNormal = vector;
				continue;
			}
			base.transform.position += motion;
			break;
		}
	}

	private void IterativeCollisionCheck(int iteration)
	{
		int layerMask = -49157;
		Vector3[] obj = new Vector3[4]
		{
			base.transform.position + base.transform.right * radius,
			base.transform.position - base.transform.right * radius,
			base.transform.position - base.transform.up * radius,
			base.transform.position + base.transform.up * radius
		};
		Vector3 vector = Vector3.zero;
		Vector3 position = base.transform.position;
		Vector3[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = array[i] - position;
			if (Physics.Raycast(position, vector2, out var hitInfo, vector2.magnitude, layerMask))
			{
				Vector3 normal = hitInfo.normal;
				normal -= Vector3.Dot(normal, base.transform.forward) * base.transform.forward;
				normal.Normalize();
				Vector3 vector3 = Vector3.Dot(vector2 - vector2.normalized * hitInfo.distance, normal) * -normal;
				if (vector3.magnitude > vector.magnitude)
				{
					vector = vector3;
				}
			}
		}
		if (vector.magnitude > 0f)
		{
			base.transform.position += vector * 1.05f;
			if (iteration < 5)
			{
				IterativeCollisionCheck(++iteration);
			}
		}
	}

	public void PredictLanding(Vector3 startPos, float gravity, float airFriction, float addedAccel, Vector3 jumpForceDir, float jumpTimeLeft)
	{
		Vector3 vector = base.transform.right;
		Vector3 vector2 = startPos;
		Vector3 vector3 = velocity;
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		GameSpline gameSpline = null;
		PlayerWaypointManager component = GetComponent<PlayerWaypointManager>();
		if ((bool)component)
		{
			gameSpline = component.splineToFollow;
			float offset = component.GetOffset();
			num3 = component.GetWaypoint();
			num2 = offset;
		}
		if (gameSpline == null)
		{
			predictedNormal = Vector3.up;
			predictedPosition = vector2;
			predictedTimeLeft = 1f;
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			Vector3 vector4 = vector2;
			Vector3 vector5 = vector3;
			float num4 = 0.025f;
			for (int j = 0; j < 8; j++)
			{
				vector5 -= vector5 * airFriction * num4;
				vector5 += Vector3.down * gravity * num4;
				vector5 += addedAccel * vector * num4;
				if (jumpTimeLeft >= 0f)
				{
					vector5 += jumpForceDir * num4;
				}
				jumpTimeLeft -= num4;
				Vector3 vector6 = vector4 + vector5 * num4;
				vector4 = vector6;
				if (gameSpline != null)
				{
					Vector3 edge3D = gameSpline.GetEdge3D(num3);
					num2 += Vector3.Dot(vector5, edge3D) * num4;
					if ((int)num2 > num3 || (int)num2 < num3)
					{
						num3 = (int)num2;
						Vector3 edge3D2 = gameSpline.GetEdge3D(num3);
						Vector3 edge3D3 = gameSpline.GetEdge3D(num3 - 1);
						vector = edge3D2;
						vector5 = vector5.y * Vector3.up + Vector3.Dot(vector5, edge3D3) * edge3D2;
					}
				}
				num += num4;
			}
			Vector3 vector7 = vector4 - vector2;
			Ray ray = new Ray(vector2, vector7);
			if (Physics.Raycast(ray, out var hitInfo, vector7.magnitude) && !hitInfo.collider.isTrigger)
			{
				predictedNormal = hitInfo.normal;
				predictedPosition = hitInfo.point;
				predictedTimeLeft = num;
				return;
			}
			vector2 = vector4;
			vector3 = vector5;
		}
		predictedNormal = Vector3.up;
		predictedPosition = vector2;
		predictedTimeLeft = 1f;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, radius);
	}

	public void BreakMotionLoop()
	{
		breakMotion = true;
	}
}
