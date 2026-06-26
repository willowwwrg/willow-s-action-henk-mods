using UnityEngine;

public class FollowCam2D : MonoBehaviour
{
	public Transform target;

	private PlatformerPhysics physics;

	private RaycastCollider playerCollider;

	public float standardDistance = 10f;

	public float extraHeight = 2f;

	public float smoothingMultiplier = 0.25f;

	public float speedToStartDistanceShift = 20f;

	public float maxSpeedForDistanceShift = 70f;

	public float extraDistanceAtMaxSpeed = 35f;

	public float distanceSmoothing = 0.5f;

	public float velocityMultiplier = 0.1f;

	public float directionMultiplier = 5f;

	public float predictionMultiplier = 1f;

	public float hookDirMultiplier = 1f;

	public float extraTilt = 5f;

	public float panForHorizontalVel = 0.3f;

	public float tiltForVerticalVel = 0.3f;

	public float rotationSmoothing = 0.35f;

	public float rotateTowardsPrediction = 0.5f;

	public float rotateTowardsHookPoint = 0.5f;

	public float extraTiltWithHook = 5f;

	public float camShakeFactor = 0.1f;

	public float camShakeFactorAir = 0.1f;

	public float impactCamShake = 0.1f;

	private float camShake;

	private float camShakeFriction = 5f;

	private float targetDistance;

	private float baseDistance;

	private float distance;

	private Vector3 positionAdd;

	private Vector3 lastDirection = Vector3.right;

	private Vector3 prevVel = Vector3.zero;

	private Quaternion camShakeAdd = Quaternion.identity;

	private void Start()
	{
		physics = target.GetComponent<PlatformerPhysics>();
		playerCollider = target.GetComponent<RaycastCollider>();
		base.transform.rotation = target.rotation;
		distance = standardDistance;
		baseDistance = standardDistance;
	}

	private void OnEventLand(float impactAmount)
	{
		camShake += impactCamShake * impactAmount;
	}

	private void FixedUpdate()
	{
		if (!target)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if ((bool)playerCollider)
		{
			vector = playerCollider.velocity;
		}
		float num = (0f - physics.GetVerticalSpeed()) * tiltForVerticalVel;
		float y = physics.GetHorizontalSpeed() * panForHorizontalVel;
		Quaternion quaternion = target.rotation * Quaternion.Euler(extraTilt + num, y, 0f);
		if (!physics.onGround && playerCollider.predictedPosition != Vector3.zero)
		{
			Quaternion to = Quaternion.LookRotation(playerCollider.predictedPosition - base.transform.position);
			quaternion = Quaternion.Slerp(quaternion, to, rotateTowardsPrediction);
		}
		GrapplingHook component = target.GetComponent<GrapplingHook>();
		if ((bool)component && component.enabled)
		{
			if (component.abilityEnabled)
			{
				Quaternion to2 = Quaternion.LookRotation(component.GetHookPoint() - base.transform.position);
				quaternion = Quaternion.Slerp(quaternion, to2, rotateTowardsHookPoint);
			}
			else
			{
				quaternion *= Quaternion.Euler(extraTiltWithHook, 0f, 0f);
			}
		}
		base.transform.rotation *= Quaternion.Inverse(camShakeAdd);
		if (physics.onGround && !physics.sliding)
		{
			camShake += playerCollider.velocity.magnitude * camShakeFactor * Time.fixedDeltaTime;
		}
		else
		{
			camShake += playerCollider.velocity.magnitude * camShakeFactorAir * Time.fixedDeltaTime;
		}
		camShakeAdd = Quaternion.Euler(Random.onUnitSphere * camShake);
		camShake -= camShake * camShakeFriction * Time.fixedDeltaTime;
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, rotationSmoothing * Time.fixedDeltaTime);
		base.transform.rotation *= camShakeAdd;
		float num2 = Mathf.InverseLerp(speedToStartDistanceShift, maxSpeedForDistanceShift, vector.magnitude) * extraDistanceAtMaxSpeed;
		targetDistance = baseDistance + num2;
		distance -= (distance - targetDistance) * distanceSmoothing * Time.fixedDeltaTime;
		if (vector.magnitude != 0f)
		{
			lastDirection = vector.normalized;
		}
		else
		{
			lastDirection = Vector3.right;
		}
		Vector3 vector2 = Vector3.zero;
		if (playerCollider.predictedPosition != Vector3.zero)
		{
			vector2 = playerCollider.predictedPosition - target.position;
			if (vector2.magnitude > 30f)
			{
				vector2 = vector2.normalized * 30f;
			}
		}
		Vector3 vector3 = Vector3.zero;
		if ((bool)component && component.enabled)
		{
			Vector3 vector4 = component.GetHookPoint() - target.position;
			vector3 = vector4;
			vector3 -= target.forward * vector4.magnitude * 0.5f;
		}
		Vector3 vector5 = vector2 * predictionMultiplier + vector * velocityMultiplier + lastDirection * directionMultiplier + vector3 * hookDirMultiplier;
		positionAdd -= (positionAdd - vector5) * 1.5f * Time.fixedDeltaTime;
		Vector3 vector6 = target.position + positionAdd;
		vector6 += Vector3.up * extraHeight;
		vector6 += target.forward * (0f - distance);
		base.transform.position -= (base.transform.position - vector6) * smoothingMultiplier;
		prevVel = vector;
	}

	public void SetTarget(Transform inTarget)
	{
		target = inTarget;
	}

	public void MultiplyBaseDistance(float multiplier)
	{
		baseDistance *= multiplier;
	}
}
