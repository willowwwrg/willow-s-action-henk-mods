using UnityEngine;

public class PlatformerPhysics : MonoBehaviour
{
	private RaycastCollider charcollider;

	private GrapplingHook grapplingHook;

	private PlayerDeath playerDeath;

	public PlatformerInput input;

	public PlatformerMovement movement;

	public PlatformerJumping jumping;

	public bool onGround;

	public bool wasOnGround;

	public Vector3 groundDirection = Vector3.right;

	public Vector3 groundNormal = Vector3.up;

	public Vector3 directionToGround = Vector3.down;

	public float groundAngle;

	public bool sliding;

	public bool mustSlide;

	public bool isJumping;

	public float jumpTimeLeft;

	public int framesSinceWeLeftGround = 1000;

	public float lastJumpTime;

	public bool groundTrigger;

	public bool hasWallGrip;

	public float timeToEnableAirAccel;

	public bool drawRays = true;

	private bool doubleJump = true;

	private float adjustedJumpForce;

	private void Awake()
	{
		charcollider = GetComponent<RaycastCollider>();
		grapplingHook = GetComponent<GrapplingHook>();
		playerDeath = GetComponent<PlayerDeath>();
		UpdateMutators();
	}

	public void Start()
	{
		OnReset();
	}

	public void OnReset()
	{
		OnRespawn();
	}

	public void OnRespawn()
	{
		isJumping = false;
		jumpTimeLeft = 0f;
		framesSinceWeLeftGround = 1000;
		onGround = false;
		wasOnGround = false;
		groundDirection = GetRightFlat();
		groundNormal = Vector3.up;
		sliding = false;
		groundAngle = 0f;
		mustSlide = false;
		groundTrigger = false;
		hasWallGrip = false;
		timeToEnableAirAccel = 0f;
		charcollider.velocity = Vector3.zero;
	}

	public void UpdateMutators()
	{
		switch (Singleton<MutatorManager>.SP.GetActiveMutator())
		{
		case Mutator.LowGravity:
			jumping.gravity = 25f;
			break;
		case Mutator.Haste:
			movement.accelWalking = 40f;
			movement.accelWalkingWhenSlow = 60f;
			movement.accelInAirWhenSlow = 46f;
			movement.accelAir = 10f;
			break;
		case Mutator.SuperJump:
			jumping.maxJumpForce = 44f;
			jumping.minJumpForce = 30f;
			jumping.maxWallJumpForce = 50f;
			jumping.minWallJumpForce = 20f;
			break;
		case Mutator.Spiderman:
			movement.yVelToLoseWallGrip = -1000f;
			movement.yVelToLostWallGripLanding = -1000f;
			movement.groundAngleForFullGravity = 250f;
			break;
		case Mutator.TinyWings:
			jumping.extraGravitySlidingInAir = 30f;
			break;
		case Mutator.InvertedControls:
		case Mutator.BlindMode:
		case Mutator.Trippin:
		case Mutator.DoubleJump:
			break;
		}
	}

	public void FixedUpdate()
	{
		wasOnGround = onGround;
		if (onGround)
		{
			CheckGroundBelow();
		}
		ApplyFriction();
		ApplyGravity();
		if (!IsUsingAbility())
		{
			UpdateWalking();
			UpdateSliding();
			UpdateJumping();
		}
		UpdateAbilities();
		if (!onGround)
		{
			UpdatePrediction();
		}
		mustSlide = false;
	}

	public void UpdatePrediction()
	{
		if ((bool)playerDeath && playerDeath.isDying)
		{
			charcollider.predictedNormal = Vector3.up;
			charcollider.predictedPosition = base.transform.position;
			charcollider.predictedTimeLeft = 0f;
			return;
		}
		float num = jumping.factorOfJumpForceInstantly * adjustedJumpForce;
		float num2 = (adjustedJumpForce - num) / jumping.jumpTime;
		Vector3 vector = groundNormal;
		float num3 = 0f;
		if (jumpTimeLeft >= 0f && isJumping)
		{
			num3 = jumpTimeLeft;
		}
		charcollider.PredictLanding(base.transform.position, jumping.gravity, movement.frictionAir, input.walkInput * movement.accelAir * 1.05f, num2 * vector, num3);
	}

	public void UpdateAbilities()
	{
		if (grapplingHook != null && grapplingHook.enabled)
		{
			if (input.abilityInput)
				grapplingHook.EnableAbility();
			else
				grapplingHook.DisableAbility();
		}
	}

	public void UpdateWalking()
	{
		if (IsOnWall() && charcollider.velocity.y < movement.yVelToLoseWallGrip)
		{
			hasWallGrip = false;
		}
		if (onGround)
		{
			if (!sliding && (!IsOnWall() || hasWallGrip))
			{
				float num = movement.accelWalkingWhenSlow;
				float num2 = Vector3.Dot(groundDirection, charcollider.velocity);
				if (Mathf.Sign(num2) == Mathf.Sign(input.walkInput))
				{
					float num3 = movement.accelWalking / movement.frictionWalking;
					float t = Mathf.Abs(num2 / num3);
					num = Mathf.Lerp(movement.accelWalkingWhenSlow, movement.accelWalking, t);
				}
				charcollider.velocity += groundDirection * input.walkInput * num * Time.fixedDeltaTime;
			}
		}
		else if (timeToEnableAirAccel <= 0f)
		{
			float num4 = movement.accelInAirWhenSlow;
			if (Mathf.Sign(GetHorizontalSpeed()) == Mathf.Sign(input.walkInput))
			{
				float num5 = movement.accelAir / movement.frictionAir;
				float t2 = Mathf.Abs(GetHorizontalSpeed() / num5);
				num4 = Mathf.Lerp(movement.accelInAirWhenSlow, movement.accelAir, t2);
			}
			charcollider.velocity += GetRightFlat() * input.walkInput * num4 * Time.fixedDeltaTime;
		}
		else
		{
			timeToEnableAirAccel -= Time.fixedDeltaTime;
		}
	}

	private void UpdateSliding()
	{
		sliding = input.slideInput;
		Mutator mut = Singleton<MutatorManager>.SP.GetActiveMutator();
		if (mustSlide || mut == Mutator.SlideOnly)
		{
			sliding = true;
		}
	}

	private void UpdateJumping()
	{
		if (onGround)
		{
			doubleJump = true;
			framesSinceWeLeftGround = 0;
		}
		else
		{
			framesSinceWeLeftGround++;
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.DoubleJump && doubleJump && framesSinceWeLeftGround >= 6)
		{
			framesSinceWeLeftGround = 5;
		}
		bool flag = Time.fixedTime - lastJumpTime >= jumping.minTimeBetweenJumps || !Application.isPlaying;
		if (!isJumping && input.jumpInput && framesSinceWeLeftGround < 6 && !flag)
		{
			MonoBehaviour.print("tapping jump too fast");
		}
		if (!isJumping && input.jumpInput && framesSinceWeLeftGround < 6 && flag)
		{
			isJumping = true;
			jumpTimeLeft = jumping.jumpTime;
			Vector3 normalized = (groundNormal + Vector3.up).normalized;
			timeToEnableAirAccel = 0f;
			lastJumpTime = Time.fixedTime;
			if (doubleJump && framesSinceWeLeftGround == 5)
			{
				doubleJump = false;
				if (charcollider.velocity.y < 0f)
				{
					charcollider.velocity.y = 0f;
				}
			}
			float y = charcollider.velocity.y;
			if (!IsOnWall())
			{
				SendEventMessage("OnEventJump", false);
				adjustedJumpForce = QuadraticFalloff(jumping.maxJumpForce, jumping.minJumpForce, 2f, jumping.yVelForMinJumpForce, y);
			}
			else
			{
				SendEventMessage("OnEventJump", true);
				if (charcollider.velocity.y < 0f && !sliding)
				{
					charcollider.velocity = Vector3.zero;
				}
				groundNormal = normalized;
				adjustedJumpForce = QuadraticFalloff(jumping.maxWallJumpForce, jumping.minWallJumpForce, 2f, jumping.yVelForMinJumpForce, y);
			}
			onGround = false;
			float num = jumping.factorOfJumpForceInstantly * adjustedJumpForce;
			charcollider.velocity += normalized * num;
		}
		if (isJumping)
		{
			float num2 = jumping.factorOfJumpForceInstantly * adjustedJumpForce;
			float num3 = (adjustedJumpForce - num2) / jumping.jumpTime;
			Vector3 vector = groundNormal;
			jumpTimeLeft -= Time.fixedDeltaTime;
			if (jumpTimeLeft >= 0f)
			{
				charcollider.velocity += vector * num3 * Time.fixedDeltaTime;
			}
			if (!input.jumpInput)
			{
				isJumping = false;
				framesSinceWeLeftGround = 1000;
			}
		}
	}

	private float QuadraticFalloff(float maxValue, float minValue, float quadraticPower, float fullFallofReachedAtValue, float currentValue)
	{
		currentValue = Mathf.Clamp(currentValue, 0f, fullFallofReachedAtValue);
		float num = (maxValue - minValue) / Mathf.Pow(fullFallofReachedAtValue, quadraticPower);
		return minValue + Mathf.Pow(fullFallofReachedAtValue - currentValue, quadraticPower) * num;
	}

	private void ApplyGravity()
	{
		if (!onGround || sliding)
		{
			charcollider.velocity += jumping.gravity * Vector3.down * Time.fixedDeltaTime;
			if (sliding && !onGround)
			{
				charcollider.velocity += jumping.extraGravitySlidingInAir * Vector3.down * Time.fixedDeltaTime;
			}
		}
		else
		{
			float num = Mathf.InverseLerp(movement.groundAngleToStartGravity, movement.groundAngleForFullGravity, groundAngle);
			num = Mathf.Clamp01(num + 0.2f);
			charcollider.velocity += num * jumping.gravity * Vector3.down * Time.fixedDeltaTime;
		}
	}

	private void ApplyFriction()
	{
		Vector3 velocity = charcollider.velocity;
		if (onGround && !IsUsingAbility())
		{
			Vector3 vector = Vector3.Dot(velocity, groundDirection) * groundDirection;
			float t = Mathf.InverseLerp(movement.groundAngleToStartGravity, movement.groundAngleForFullGravity, groundAngle);
			float num = Mathf.Lerp(Mathf.Lerp(movement.frictionStill, movement.frictionWalking, t), movement.frictionWalking, Mathf.Abs(input.walkInput));
			if (IsOnWall() && charcollider.velocity.y < 0f)
			{
				num = movement.frictionOnWall;
			}
			if (sliding)
			{
				num = movement.frictionSliding;
			}
			if (vector.magnitude < movement.minVelocityToStop && input.walkInput == 0f && !input.slideInput && !IsOnWall())
			{
				velocity -= vector;
			}
			else
			{
				velocity -= vector * num * Time.fixedDeltaTime;
			}
		}
		else
		{
			velocity -= velocity * movement.frictionAir * Time.fixedDeltaTime;
		}
		charcollider.velocity = velocity;
	}

	public void HasCollision(RaycastCollisionInfo collision)
	{
		float num = Vector3.Angle(groundNormal, collision.normal);
		float num2 = collision.velocityBeforeImpact.magnitude / collision.velocityAfterImpact.magnitude;
		if (onGround && num != 0f && collision.isConcave)
		{
			if (num < movement.maxAngleForConcaveness)
			{
				charcollider.velocity *= num2;
			}
			if (num < movement.maxAngleForPumping && sliding)
			{
				charcollider.velocity += charcollider.velocity.normalized * (num / 90f) * movement.pumpingSpeedGain;
			}
		}
		bool flag = Mathf.Abs(collision.normal.y) < 0.025f;
		float magnitude = (collision.velocityBeforeImpact - collision.velocityAfterImpact).magnitude;
		if (flag && magnitude > 5f)
		{
			SendEventMessage("OnEventWall", magnitude);
		}
		if (IsInAJump())
		{
			return;
		}
		bool flag2 = collision.normal.y < -0.05f;
		if ((!flag || !onGround || !(num > movement.maxAngleForConcaveness) || !(groundNormal.y > 0.95f)) && (onGround || !flag2) && (!onGround || !flag2 || !(num > movement.maxAngleForConcaveness)))
		{
			if (!onGround && !flag)
			{
				SendEventMessage("OnEventLand", magnitude);
			}
			SetGroundNormal(collision.normal);
			directionToGround = collision.directionToGround;
			onGround = true;
			if (flag && GetVerticalSpeed() > movement.yVelToLostWallGripLanding)
			{
				hasWallGrip = true;
			}
			else
			{
				hasWallGrip = false;
			}
		}
	}

	private void CheckGroundBelow()
	{
		int layerMask = -49157;
		float num = 0.5f;
		float num2 = 1f;
		if (Physics.Raycast(base.transform.position, directionToGround, out var hitInfo, 1000f, layerMask))
		{
			if (hitInfo.distance < charcollider.radius + num2)
			{
				Vector3 vector = groundNormal;
				vector.y = 0f;
				Vector3 normal = hitInfo.normal;
				normal.y = 0f;
				float num3 = Vector3.Angle(groundNormal, hitInfo.normal);
				bool num4 = hitInfo.transform.name.StartsWith("curve_");
				bool flag = Vector3.Dot(hitInfo.normal, charcollider.velocity) > 0.1f;
				float num5 = movement.maxAngleForConvexFollow_CURVE;
				if (!num4)
				{
					num5 = movement.maxAngleForConvexFollow;
				}
				if (num3 != 0f && groundNormal.y >= 0f && num3 < num5 && flag)
				{
					float magnitude = charcollider.velocity.magnitude;
					charcollider.velocity -= Vector3.Dot(hitInfo.normal, charcollider.velocity) * hitInfo.normal;
					charcollider.velocity = charcollider.velocity.normalized * magnitude;
					SetGroundNormal(hitInfo.normal);
				}
				else if (hitInfo.distance > charcollider.radius + num)
				{
					LeftGround();
				}
			}
			else if (hitInfo.distance > charcollider.radius + num)
			{
				LeftGround();
			}
		}
		else
		{
			LeftGround();
		}
	}

	public void LeftGround(bool dontAllowLateJump = false)
	{
		onGround = false;
		timeToEnableAirAccel = 0.15f;
		if (dontAllowLateJump)
		{
			framesSinceWeLeftGround = 1000;
			isJumping = false;
		}
	}

	private void SetGroundNormal(Vector3 normal)
	{
		groundNormal = normal;
		groundDirection = Vector3.Cross(groundNormal, base.transform.forward);
		groundAngle = Vector3.Angle(groundNormal, Vector3.up);
	}

	public void TriggerHit(PhysicsTriggerType type)
	{
		if (type == PhysicsTriggerType.Ground)
		{
			groundTrigger = true;
		}
	}

	public bool IsInAJump()
	{
		if (isJumping)
		{
			return jumpTimeLeft >= 0f;
		}
		return false;
	}

	public bool IsOnWall()
	{
		if (Mathf.Abs(groundNormal.y) < 0.02f)
		{
			return onGround;
		}
		return false;
	}

	public Vector3 GetRightFlat()
	{
		Vector3 right = base.transform.right;
		right.y = 0f;
		return right.normalized;
	}

	public float GetGroundNormalX()
	{
		return Vector3.Dot(base.transform.right, groundNormal);
	}

	public float GetHorizontalSpeed()
	{
		return Vector3.Dot(charcollider.velocity, GetRightFlat());
	}

	public float GetVerticalSpeed()
	{
		return charcollider.velocity.y;
	}

	public void SendEventMessage(string eventMessage)
	{
		if (Application.isPlaying)
		{
			BroadcastMessage(eventMessage, SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage(eventMessage, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SendEventMessage(string eventMessage, object parameter)
	{
		if (Application.isPlaying)
		{
			BroadcastMessage(eventMessage, parameter, SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage(eventMessage, parameter, SendMessageOptions.DontRequireReceiver);
		}
	}

	public bool IsUsingAbility()
	{
		return grapplingHook != null && grapplingHook.enabled && grapplingHook.abilityEnabled;
	}
}
