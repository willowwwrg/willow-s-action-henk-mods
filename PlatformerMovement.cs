using System;

[Serializable]
public class PlatformerMovement
{
	public float frictionStill = 1.5f;

	public float frictionWalking = 0.55f;

	public float frictionSliding = 0.2f;

	public float frictionAir = 0.2f;

	public float frictionOnWall = 2f;

	public float accelWalking = 20f;

	public float accelWalkingWhenSlow = 30f;

	public float accelInAirWhenSlow = 18f;

	public float accelAir = 5f;

	public float groundAngleToStartGravity = 35f;

	public float groundAngleForFullGravity = 85f;

	public float torqueMultiplier = 1f;

	public float minVelocityToStop = 1f;

	public float maxAngleForConcaveness = 15f;

	public float maxAngleForPumping = 45f;

	public float pumpingSpeedGain = 7f;

	public float maxAngleForConvexFollow = 10f;

	public float maxAngleForConvexFollow_CURVE = 20f;

	public float yVelToLoseWallGrip = 5f;

	public float yVelToLostWallGripLanding = 15f;
}
