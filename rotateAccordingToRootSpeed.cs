using UnityEngine;

public class rotateAccordingToRootSpeed : MonoBehaviour
{
	private void Start()
	{
	}

	private void FixedUpdate()
	{
		float magnitude = base.transform.root.GetComponent<RaycastCollider>().velocity.magnitude;
		base.transform.Rotate(0f - magnitude, 0f, 0f);
	}
}
