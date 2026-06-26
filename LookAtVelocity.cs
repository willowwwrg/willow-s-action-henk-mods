using UnityEngine;

public class LookAtVelocity : MonoBehaviour
{
	public Vector3 extraRotation = Vector3.zero;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		if ((bool)base.rigidbody && !base.rigidbody.isKinematic)
		{
			base.transform.LookAt(base.transform.position + base.rigidbody.velocity);
			base.transform.Rotate(extraRotation);
		}
	}
}
