using UnityEngine;

public class PogoStick : MonoBehaviour
{
	public Transform pogoPrefab;

	private Transform pogoModel;

	private RaycastCollider playerCollider;

	private PlatformerPhysics physics;

	private PlayerGraphics graphics;

	public float bounceFactor = 1.1f;

	public float bounceForce = 50f;

	public float rotationForce = 100f;

	public bool allowAirControl = true;

	public float lerpBetweenNormalAndDir = 0.5f;

	public float maxAngleWithGround = 60f;

	[HideInInspector]
	public float rotationInput;

	private void OnReset()
	{
		if (base.enabled)
		{
			base.enabled = false;
		}
	}

	private void Awake()
	{
		playerCollider = GetComponent<RaycastCollider>();
		physics = GetComponent<PlatformerPhysics>();
		graphics = GetComponent<PlayerGraphics>();
	}

	private void OnEnable()
	{
		pogoModel = (Transform)Object.Instantiate(pogoPrefab);
		pogoModel.parent = graphics.rotatingChild;
		pogoModel.localPosition = Vector3.zero;
		pogoModel.localRotation = Quaternion.identity;
	}

	private void OnDisable()
	{
		if ((bool)pogoModel)
		{
			Object.Destroy(pogoModel.gameObject);
		}
	}

	private void FixedUpdate()
	{
		graphics.rotationSpeed -= rotationForce * rotationInput * Time.fixedDeltaTime;
	}

	public void HasCollision(RaycastCollisionInfo collision)
	{
		float num = Vector3.Angle(collision.normal, graphics.rotatingChild.up);
		if (IsEnabled() && num < maxAngleWithGround)
		{
			playerCollider.BreakMotionLoop();
			Vector3 normalized = Vector3.Lerp(collision.normal, graphics.rotatingChild.up, lerpBetweenNormalAndDir).normalized;
			playerCollider.velocity = Vector3.Reflect(collision.velocityBeforeImpact, normalized) * bounceFactor;
			playerCollider.velocity += bounceForce * graphics.rotatingChild.up;
			physics.LeftGround();
		}
	}

	public bool IsEnabled()
	{
		return base.enabled;
	}
}
