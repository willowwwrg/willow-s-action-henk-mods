using UnityEngine;

public static class PPFXRigidbodyExtensions
{
	public static void AddExplosionForce(this Rigidbody body, float explosionForce, Vector3 explosionRadiusCenter, float explosionRadius)
	{
		body.AddExplosionForce(explosionForce, explosionRadiusCenter, explosionRadius, new Vector3(0f, 0f, 0f));
	}

	public static void AddExplosionForce(this Rigidbody body, float explosionForce, Vector3 explosionRadiusCenter, float explosionRadius, Vector3 explosionOriginPoint)
	{
		body.AddExplosionForce(explosionForce, explosionRadiusCenter, explosionRadius, explosionOriginPoint, ForceMode.Force);
	}

	public static void AddExplosionForce(this Rigidbody body, float explosionForce, Vector3 explosionRadiusCenter, float explosionRadius, Vector3 explosionOriginPoint, ForceMode mode)
	{
		if (Vector3.Distance(body.transform.position, explosionRadiusCenter) <= explosionRadius)
		{
			Vector3 vector = body.transform.position - (explosionRadiusCenter + explosionOriginPoint);
			body.AddForce(vector * (explosionForce / 5f), mode);
		}
	}
}
