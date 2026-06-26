using UnityEngine;

public struct RaycastCollisionInfo
{
	public Vector3 point;

	public Vector3 normal;

	public Vector3 directionToGround;

	public Vector3 velocityAfterImpact;

	public Vector3 velocityBeforeImpact;

	public bool isConcave;
}
