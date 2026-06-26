using System;
using UnityEngine;

public class PlatformerCameraMultiplayer : MonoBehaviour
{
	public GameObject[] targets;

	public float baseDistance = 20f;

	public float extraDistAtMaxSpeed = 20f;

	public float extraDistForHook = 10f;

	public float extraHeight = 3f;

	public float heightRoomNeeded = 14f;

	public float posSmoothing = 7f;

	public float rotSmoothing = 5f;

	private float speedToStartDistanceShift = 20f;

	private float maxSpeedForDistanceShift = 70f;

	private float distanceSmoothing = 2.5f;

	public float distanceSmoothingWhenHittingGeometry = 2.5f;

	private float distance;

	public Vector2 maxDistanceFromBestPlayer = new Vector2(13f, 23f);

	public float maxDistance = 55f;

	public float distanceToKeepFromGeometry = 5f;

	public float horizontalAdd = 5f;

	public float horizontalAccel = 0.1f;

	private float direction = 1f;

	public float verticalAccel = 0.85f;

	public float verticalAccelFromBestPlayer = 0.25f;

	public float verticalFriction = 1f;

	private float directionV;

	private float maxVerticalAccel = 45f;

	public float splineSmoothing = 5f;

	private Vector3 splineOffset = Vector3.zero;

	private Vector3 previousPhysicsPos = Vector3.zero;

	private Vector3 physicsPosition = Vector3.zero;

	private Quaternion previousPhysicsRotation = Quaternion.identity;

	private Quaternion physicsRotation = Quaternion.identity;

	private Vector3 targetPosOnPlane;

	private Vector3 targetPos;

	[HideInInspector]
	public float yPosOfDeathVolume = -1000f;

	public void Start()
	{
		FindTargets();
	}

	public void FindTargets()
	{
		targets = Singleton<PlayerManager>.SP.GetLocalPlayers();
	}

	private void Update()
	{
		if (targets.Length != 0)
		{
			base.transform.position = Vector3.Lerp(previousPhysicsPos, physicsPosition, Singleton<PhysicsTimeManager>.SP.interpolationValue);
			base.transform.rotation = Quaternion.Slerp(previousPhysicsRotation, physicsRotation, Singleton<PhysicsTimeManager>.SP.interpolationValue);
		}
	}

	private void FixedUpdate()
	{
		previousPhysicsPos = physicsPosition;
		previousPhysicsRotation = physicsRotation;
		UpdateFuction();
	}

	private void UpdateFuction()
	{
		targets = Singleton<LocalMultiManager>.SP.GetAlivePlayers();
		if (targets.Length != 0)
		{
			GameSpline splineToFollow = targets[0].GetComponent<PlayerWaypointManager>().splineToFollow;
			Vector2 vector = new Vector2(10000f, 10000f);
			Vector2 vector2 = new Vector2(-10000f, -10000f);
			float num = -1000f;
			Vector2 vector3 = Vector2.zero;
			Vector3 vector4 = Vector3.zero;
			Vector3 zero = Vector3.zero;
			float num2 = 0f;
			float num3 = 0f;
			bool flag = false;
			GameObject[] array = targets;
			foreach (GameObject gameObject in array)
			{
				GrapplingHook component = gameObject.GetComponent<GrapplingHook>();
				bool flag2 = (bool)component && component.enabled;
				if (flag2)
				{
					flag = true;
				}
				zero += gameObject.GetComponent<RaycastCollider>().velocity;
				num2 += gameObject.GetComponent<PlatformerPhysics>().GetHorizontalSpeed();
				Vector2 offset2D = gameObject.GetComponent<PlayerWaypointManager>().GetOffset2D();
				if (!gameObject.GetComponent<PlatformerPhysics>().onGround && !flag2)
				{
					float num4 = (gameObject.GetComponent<RaycastCollider>().predictedPosition.y - gameObject.transform.position.y) * verticalAccel;
					if (num4 < 0f - maxVerticalAccel)
					{
						num4 = 0f - maxVerticalAccel;
					}
					num3 += num4;
				}
				float positionAlongTrack = gameObject.GetComponent<PlayerWaypointManager>().GetPositionAlongTrack();
				if (positionAlongTrack > num || (positionAlongTrack == num && offset2D.x > vector3.x))
				{
					num = positionAlongTrack;
					vector3 = offset2D;
					vector4 = gameObject.GetComponent<RaycastCollider>().velocity;
				}
				vector = Vector2.Min(vector, offset2D);
				vector2 = Vector2.Max(vector2, offset2D);
			}
			Vector2 vector5 = Vector2.Lerp(vector, vector2, 0.5f);
			Vector3 vector6 = zero / targets.Length;
			float f = num2 / (float)targets.Length;
			float num5 = num3 / (float)targets.Length;
			float num6 = vector2.y - vector.y;
			float num7 = Mathf.InverseLerp(speedToStartDistanceShift, maxSpeedForDistanceShift, vector6.magnitude) * extraDistAtMaxSpeed;
			float num8 = baseDistance + num7;
			if (flag)
			{
				num8 += extraDistForHook;
			}
			float num9 = (num6 * 0.5f + heightRoomNeeded) / Mathf.Tan(base.camera.fieldOfView * ((float)Math.PI / 180f) * 0.5f);
			if (num8 < num9)
			{
				num8 = num9;
			}
			if (num8 > maxDistance)
			{
				num8 = maxDistance;
			}
			int layerMask = 36864;
			Vector3 vector7 = targetPosOnPlane;
			Vector3 vector8 = targetPos;
			if (Physics.Raycast(vector7, vector8 - vector7, out var hitInfo, 1000f, layerMask))
			{
				Debug.DrawLine(vector7, hitInfo.point, Color.green);
				float num10 = hitInfo.distance - distanceToKeepFromGeometry;
				if (num8 > num10)
				{
					num8 = num10;
					distance -= (distance - num8) * distanceSmoothingWhenHittingGeometry * Time.deltaTime;
				}
			}
			distance -= (distance - num8) * distanceSmoothing * Time.deltaTime;
			Vector3 to = new Vector3(vector5.x, vector5.y, 0f);
			float num11 = distance / baseDistance * 0.75f;
			float num12 = maxDistanceFromBestPlayer.x * num11;
			Vector2 vector9 = vector3 - vector5;
			if (Mathf.Abs(vector9.x) > num12)
			{
				to.x += (Mathf.Abs(vector9.x) - num12) * Mathf.Sign(vector9.x);
			}
			if (Mathf.Abs(vector9.y) > maxDistanceFromBestPlayer.y)
			{
				to.y += (Mathf.Abs(vector9.y) - maxDistanceFromBestPlayer.y) * Mathf.Sign(vector9.y);
			}
			splineOffset = Vector3.Lerp(splineOffset, to, splineSmoothing * Time.deltaTime);
			Vector3 vector10 = splineToFollow.DeformVertex(Vector3.zero, splineOffset);
			Vector3 vector11 = -splineToFollow.GetNormal3DSmooth(splineOffset.x);
			Vector3 vector12 = new Vector3(vector11.z, 0f, 0f - vector11.x);
			float num13 = Mathf.Sign(f);
			direction -= (direction - num13) * Time.deltaTime * Mathf.Abs(f) * horizontalAccel;
			if (vector4.y > 0f)
			{
				directionV -= vector4.y * verticalAccelFromBestPlayer * Time.deltaTime * 0.5f;
			}
			else
			{
				directionV -= vector4.y * verticalAccelFromBestPlayer * Time.deltaTime;
			}
			directionV -= num5 * Time.deltaTime;
			directionV -= Mathf.Sign(directionV) * directionV * directionV * verticalFriction * Time.deltaTime;
			Vector3 vector13 = directionV * Vector3.down;
			targetPosOnPlane = vector10 + extraHeight * Vector3.up + vector12 * direction * horizontalAdd + vector13;
			targetPos = targetPosOnPlane - distance * vector11;
			if (targetPos.y < yPosOfDeathVolume)
			{
				targetPos.y = yPosOfDeathVolume;
			}
			Vector3 vector14 = (physicsPosition - targetPos) * posSmoothing * Time.deltaTime;
			physicsPosition -= vector14;
			Quaternion to2 = Quaternion.LookRotation(targetPosOnPlane + vector13 - physicsPosition);
			physicsRotation = Quaternion.Slerp(physicsRotation, to2, rotSmoothing * Time.deltaTime);
			Vector3 vector15 = splineToFollow.DeformVertex(Vector3.zero, new Vector3(vector.x, vector.y, 0f));
			Vector3 vector16 = splineToFollow.DeformVertex(Vector3.zero, new Vector3(vector.x, vector2.y, 0f));
			Vector3 vector17 = splineToFollow.DeformVertex(Vector3.zero, new Vector3(vector2.x, vector.y, 0f));
			Vector3 vector18 = splineToFollow.DeformVertex(Vector3.zero, new Vector3(vector2.x, vector2.y, 0f));
			Vector3 start = splineToFollow.DeformVertex(Vector3.zero, new Vector3(vector5.x, vector5.y, 0f));
			Debug.DrawLine(vector15, vector16);
			Debug.DrawLine(vector16, vector18);
			Debug.DrawLine(vector18, vector17);
			Debug.DrawLine(vector17, vector15);
			Debug.DrawLine(vector15, vector18);
			Debug.DrawLine(vector16, vector17);
			Debug.DrawLine(start, vector10, Color.green);
			Debug.DrawRay(vector10, vector11, Color.blue);
			Debug.DrawRay(vector10, vector12, Color.blue);
			Debug.DrawLine(vector7, vector8);
		}
		yPosOfDeathVolume = -1000f;
	}

	public void OnRespawn()
	{
		distance = baseDistance;
		direction = 1f;
		directionV = 0f;
		SnapCamera();
	}

	public void OnReset()
	{
		OnRespawn();
	}

	public void SnapCamera()
	{
		float num = posSmoothing;
		float num2 = distanceSmoothing;
		float num3 = rotSmoothing;
		float num4 = splineSmoothing;
		posSmoothing = 1f / Time.deltaTime;
		distanceSmoothing = 1f / Time.deltaTime;
		rotSmoothing = 1f / Time.deltaTime;
		splineSmoothing = 1f / Time.deltaTime;
		UpdateFuction();
		posSmoothing = num;
		distanceSmoothing = num2;
		rotSmoothing = num3;
		splineSmoothing = num4;
	}
}
