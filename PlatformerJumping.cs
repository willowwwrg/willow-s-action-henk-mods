using System;

[Serializable]
public class PlatformerJumping
{
	public float gravity = 50f;

	public float extraGravitySlidingInAir = 10f;

	public float maxJumpForce = 22f;

	public float minJumpForce = 13f;

	public float factorOfJumpForceInstantly = 0.5f;

	public float yVelForMinJumpForce = 40f;

	public float maxWallJumpForce = 25f;

	public float minWallJumpForce = 10f;

	public float jumpTime = 0.2f;

	public float minTimeBetweenJumps = 0.1f;

	public float wallJumpExtraSideForce = 10f;
}
