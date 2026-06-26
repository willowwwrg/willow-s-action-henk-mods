using UnityEngine;

public class RotateTransformBehaviour : MonoBehaviour
{
	public Vector3 Amount = Vector3.up;

	public float rotVelocity = 1f;

	private void Update()
	{
		base.transform.Rotate(Amount * (Time.smoothDeltaTime * rotVelocity));
	}
}
