using UnityEngine;

public class SlideVolume : MonoBehaviour
{
	private bool obstructed;

	private PlatformerPhysics physics;

	private void Start()
	{
		physics = Object.FindObjectOfType<PlatformerPhysics>();
	}

	private void FixedUpdate()
	{
		if (physics.sliding)
		{
			base.gameObject.layer = 2;
			base.collider.isTrigger = true;
		}
		else if (!obstructed)
		{
			base.gameObject.layer = 0;
			base.collider.isTrigger = false;
		}
		obstructed = false;
	}

	private void OnTriggerStay(Collider other)
	{
		PlatformerPhysics component = other.transform.root.GetComponent<PlatformerPhysics>();
		if ((bool)component)
		{
			component.mustSlide = true;
			obstructed = true;
		}
	}
}
