using UnityEngine;

public class HandyCam : MonoBehaviour
{
	private Vector3 vel = Vector3.zero;

	private Vector3 velrot = Vector3.zero;

	private float handyCamDamping = 0.25f;

	private float handyCamMultiplier = 0.66f;

	private float handyCamAccel = 0.0066f;

	private float handyCamFriction = 0.2f;

	private float handyCamRotationMultiplier = 1f;

	private Transform cameraTargetTransform;

	public void SetCameraTarget(Transform target, bool snapToTarget = false)
	{
		cameraTargetTransform = target;
		if (snapToTarget)
		{
			base.transform.position = cameraTargetTransform.position;
			base.transform.rotation = cameraTargetTransform.rotation;
		}
	}

	private void Update()
	{
		if (cameraTargetTransform != null)
		{
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			vel += insideUnitSphere * handyCamAccel;
			vel -= vel * Time.deltaTime * handyCamFriction;
			base.transform.position += vel * handyCamMultiplier * Time.deltaTime;
			base.transform.position = Vector3.Lerp(base.transform.position, cameraTargetTransform.position, Time.deltaTime * handyCamDamping);
			insideUnitSphere = Random.insideUnitSphere;
			insideUnitSphere.z = 0f;
			velrot += insideUnitSphere * handyCamAccel * handyCamRotationMultiplier;
			velrot -= vel * Time.deltaTime * handyCamFriction / handyCamRotationMultiplier;
			base.transform.rotation *= Quaternion.Euler(velrot * handyCamMultiplier * handyCamRotationMultiplier * Time.deltaTime);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, cameraTargetTransform.rotation, Time.deltaTime * handyCamDamping / handyCamRotationMultiplier);
		}
	}
}
