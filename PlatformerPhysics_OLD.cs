using UnityEngine;

public class PlatformerPhysics_OLD : MonoBehaviour
{
	private float gravity = 45f;

	private float slidingUpwardsGravity;

	private float slidingDownwardsGravity = 45f;

	private float frictionStill = 0.8f;

	private float frictionWalking = 0.95f;

	private float frictionSliding = 0.99f;

	private float frictionAir = 0.995f;

	private float accelSliding;

	private float accelWalking = 50f;

	private float accelAir = 4.775f;

	private float wallJumpForce = 800f;

	private float jumpForceInitial = 600f;

	private float jumpForceExtra = 50f;

	private int jumpFrames = 10;

	private float wallStickyness = 0.25f;

	private float rotationAirMax = 4f;

	private float rotationAirMult = 0.2f;

	private float rotationGroundMax = 10f;

	private float rotationGroundMult = 0.25f;

	public bool slidePressed;

	public bool jumpPressed;

	public bool upPressed;

	public float walkingInput;

	private bool isJumping;

	private int jumpFramesLeft;

	private bool onWall;

	private bool onGround;

	private bool wasOnWall;

	private bool wasOnGround;

	private Vector3 groundDirection = Vector3.right;

	private Vector3 groundNormal = Vector3.up;

	private float wallStickynessLeft;

	public bool inTrigger;

	private bool sliding;

	public Material mat1;

	public Material mat2;

	public Material mat3;

	public Material mat4;

	private RaycastCollider charcollider;

	public void Start()
	{
		charcollider = GetComponent<RaycastCollider>();
		Reset();
	}

	public void Reset()
	{
		slidePressed = false;
		jumpPressed = false;
		walkingInput = 0f;
		isJumping = false;
		jumpFramesLeft = 0;
		onWall = false;
		onGround = false;
		wasOnWall = false;
		wasOnGround = false;
		groundDirection = Vector3.right;
		groundNormal = Vector3.up;
		sliding = false;
		charcollider.velocity = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
	}

	private void FixedUpdate()
	{
		wasOnWall = onWall;
		wasOnGround = onGround;
		UpdateSliding();
		UpdateJumping();
		UpdateWalking();
		ApplyGravity();
		ApplyFriction();
		ApplyRotation();
		if ((bool)base.transform.Find("Collider"))
		{
			if (sliding)
			{
				base.transform.Find("Collider").renderer.material = mat3;
			}
			else if (onWall)
			{
				base.transform.Find("Collider").renderer.material = mat4;
			}
			else if (!onGround)
			{
				base.transform.Find("Collider").renderer.material = mat1;
			}
			else
			{
				base.transform.Find("Collider").renderer.material = mat2;
			}
		}
		if (!inTrigger)
		{
			onGround = false;
			onWall = false;
		}
		inTrigger = false;
	}

	public void UpdateWalking()
	{
		if (onGround)
		{
			float num = accelWalking;
			if (sliding)
			{
				num = accelSliding;
			}
			charcollider.velocity += groundDirection * walkingInput * num * Time.fixedDeltaTime;
			return;
		}
		float num2 = accelWalking * 0.5f;
		float num3 = num2;
		if (Mathf.Sign(charcollider.velocity.x) == Mathf.Sign(walkingInput))
		{
			float t = Mathf.Abs(charcollider.velocity.x / Time.fixedDeltaTime) * (1f - frictionAir) / accelAir;
			num3 = Mathf.Lerp(num2, accelAir, t);
		}
		if (onWall && Mathf.Sign(groundNormal.x) == Mathf.Sign(walkingInput))
		{
			wallStickynessLeft -= Time.fixedDeltaTime;
			if (wallStickynessLeft > 0f)
			{
				return;
			}
		}
		charcollider.velocity += Vector3.right * walkingInput * num3 * Time.fixedDeltaTime;
	}

	private void UpdateSliding()
	{
		sliding = slidePressed;
	}

	private void UpdateJumping()
	{
		if (!isJumping && jumpPressed && (onGround || onWall))
		{
			isJumping = true;
			jumpFramesLeft = jumpFrames;
			if (onWall)
			{
				charcollider.velocity = Vector3.zero;
				Vector3 normalized = (groundNormal + Vector3.up).normalized;
				charcollider.velocity += normalized * wallJumpForce * Time.fixedDeltaTime;
				wallStickynessLeft = 0f;
				groundDirection = Vector3.right;
				groundNormal = Vector3.up;
				onWall = false;
			}
			else
			{
				onGround = false;
				charcollider.velocity += groundNormal * jumpForceInitial * Time.fixedDeltaTime;
			}
		}
		if (isJumping)
		{
			jumpFramesLeft--;
			if (jumpFramesLeft >= 0)
			{
				charcollider.velocity += groundNormal * jumpForceExtra * Time.fixedDeltaTime;
			}
			if (!jumpPressed)
			{
				isJumping = false;
			}
		}
	}

	private void ApplyGravity()
	{
		if (!onGround)
		{
			charcollider.velocity += gravity * Vector3.down * Time.fixedDeltaTime;
		}
		else if (sliding)
		{
			if ((charcollider.velocity.x > 0f && groundDirection.y < 0f) || (charcollider.velocity.x < 0f && groundDirection.y > 0f))
			{
				charcollider.velocity += slidingDownwardsGravity * Vector3.down * Time.fixedDeltaTime;
			}
			else
			{
				charcollider.velocity += slidingUpwardsGravity * Vector3.down * Time.fixedDeltaTime;
			}
		}
	}

	private void ApplyFriction()
	{
		Vector3 velocity = charcollider.velocity;
		if (onGround)
		{
			Vector3 vector = Vector3.Dot(velocity, groundDirection) * groundDirection;
			float num = frictionSliding;
			if (!sliding)
			{
				num = Mathf.Lerp(frictionStill, frictionWalking, Mathf.Abs(walkingInput));
			}
			Vector3 vector2 = vector * num;
			velocity -= vector - vector2;
		}
		else
		{
			float num2 = frictionAir;
			velocity *= num2;
		}
		charcollider.velocity = velocity;
	}

	private void ApplyRotation()
	{
		if (!onGround)
		{
			if (sliding && charcollider.velocity.y < 0f)
			{
				Vector3 vector = new Vector3(0f - charcollider.velocity.y, charcollider.velocity.x, 0f);
				if (vector.y < 0f)
				{
					vector = -vector;
				}
				RotateTowards(vector, rotationAirMult, rotationAirMax, Vector3.down);
			}
			else
			{
				RotateTowards(Vector3.up, rotationAirMult, rotationAirMax);
			}
		}
		else
		{
			RotateTowards(groundNormal, rotationGroundMult, rotationGroundMax);
		}
	}

	public void RotateTowards(Vector3 upVector, float rotationMultiplier, float maxRotationSpeed)
	{
		RotateTowards(upVector, rotationMultiplier, maxRotationSpeed, Vector3.zero);
	}

	public void RotateTowards(Vector3 upVector, float rotationMultiplier, float maxRotationSpeed, Vector3 rotationOrigin)
	{
		Vector3 vector = base.transform.TransformPoint(rotationOrigin);
		Debug.DrawLine(base.transform.position - Vector3.forward, vector - Vector3.forward, Color.magenta);
		float num = Vector3.Angle(upVector, base.transform.up);
		Vector3 axis = Vector3.Cross(base.transform.up, upVector);
		float b = num * rotationMultiplier;
		if (num < 0.5f)
		{
			b = num;
		}
		b = Mathf.Min(maxRotationSpeed, b);
		base.transform.RotateAround(vector, axis, b);
	}

	private void OnTriggerEnter(Collider other)
	{
		inTrigger = true;
	}

	private void OnTriggerStay(Collider other)
	{
		inTrigger = true;
	}

	private void OnCharacterColliderHit(ControllerColliderHit hit)
	{
		HasCollision(hit.normal);
	}

	private void HasCollision(Vector3 normal)
	{
		if (!IsInAJump() && normal.y > -0.2f)
		{
			bool flag = Mathf.Abs(normal.y) < 0.2f;
			groundNormal = normal;
			groundDirection = Vector3.Cross(groundNormal, Vector3.forward);
			onWall = flag;
			onGround = !flag;
			if (!wasOnWall && flag)
			{
				wallStickynessLeft = wallStickyness;
			}
		}
	}

	public bool IsInAJump()
	{
		if (isJumping)
		{
			return jumpFramesLeft >= 0;
		}
		return false;
	}

	public bool IsOnGround()
	{
		return onGround;
	}

	public bool IsOnWall()
	{
		return onWall;
	}

	public bool WasOnWall()
	{
		return wasOnWall;
	}

	public bool WasOnGround()
	{
		return wasOnGround;
	}

	public bool IsSliding()
	{
		return sliding;
	}

	public Vector3 GetGroundDir()
	{
		return groundDirection;
	}

	public Vector3 GetGroundNormal()
	{
		return groundNormal;
	}

	private void _ResetGame()
	{
		Reset();
	}

	private void _ResetToLastCheckpoint()
	{
		Reset();
	}
}
