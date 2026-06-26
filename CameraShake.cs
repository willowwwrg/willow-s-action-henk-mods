using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public float shakeAmount;

	public bool cameraIsControlledExternally = true;

	private Quaternion startRotation;

	private void Start()
	{
		startRotation = base.transform.rotation;
	}

	private void LateUpdate()
	{
		if (!cameraIsControlledExternally)
		{
			base.transform.rotation = startRotation;
		}
		base.transform.Rotate(Random.value * shakeAmount, Random.value * shakeAmount, Random.value * shakeAmount);
	}

	private void FixedUpdate()
	{
	}
}
