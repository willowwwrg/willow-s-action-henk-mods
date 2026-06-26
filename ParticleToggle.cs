using UnityEngine;

public class ParticleToggle : MonoBehaviour
{
	private PlatformerPhysics player;

	private RaycastCollider playerCollider;

	private ParticleSystem[] particles;

	public bool enableOnGround;

	public bool enableInAir;

	public bool enableSliding;

	public bool enableSlidingOnGround;

	public bool enableOnWall;

	public bool enableRunOnly;

	public float minimumVelocity;

	private void Start()
	{
		if (!base.transform.root.GetComponent<PlatformerPhysics>())
		{
			Debug.LogError("This class must be attached to a child of the player!");
		}
		player = base.transform.root.GetComponent<PlatformerPhysics>();
		playerCollider = base.transform.root.GetComponent<RaycastCollider>();
		particles = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		bool num = player.onGround && enableOnGround;
		bool flag = !player.onGround && enableInAir;
		bool flag2 = player.sliding && enableSliding;
		bool flag3 = player.sliding && player.onGround && enableSlidingOnGround;
		bool flag4 = player.IsOnWall() && !player.hasWallGrip && enableOnWall;
		bool flag5 = player.onGround && !player.sliding && (!player.IsOnWall() || player.hasWallGrip) && enableRunOnly;
		bool flag6 = playerCollider.velocity.magnitude > minimumVelocity;
		bool enableEmission = false;
		if ((num || flag || flag2 || flag4 || flag3 || flag5) && flag6)
		{
			enableEmission = true;
		}
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enableEmission = enableEmission;
		}
	}
}
