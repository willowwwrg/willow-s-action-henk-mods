using UnityEngine;

public class PhysicsTrigger : MonoBehaviour
{
	public PhysicsTriggerType triggerType = PhysicsTriggerType.Ground;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.transform.GetComponent<CharacterController>())
		{
			Hit();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!other.transform.GetComponent<CharacterController>())
		{
			Hit();
		}
	}

	private void Hit()
	{
		base.transform.parent.parent.SendMessage("TriggerHit", triggerType);
	}
}
