using UnityEngine;

public class BoostPad : MonoBehaviour
{
	public float force = 50f;

	public new GameObject particleSystem;

	private void Start()
	{
		CFX_AutoRotate[] componentsInChildren = GetComponentsInChildren<CFX_AutoRotate>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].rotation.z *= Mathf.Sign(base.transform.localScale.x);
		}
	}

	private void OnTriggerStay(Collider col)
	{
		if (Time.timeScale != 0f)
		{
			Boost(col);
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		Boost(col);
		RaycastCollider component = col.transform.root.GetComponent<RaycastCollider>();
		if (!component || !Singleton<PlayerManager>.SP.IsLocalPlayer(component.gameObject))
		{
			return;
		}
		AudioController.Play("SpeedBoost", base.transform);
		component.gameObject.BroadcastMessage("AddSpeedLines", 2f, SendMessageOptions.DontRequireReceiver);
		if ((bool)particleSystem)
		{
			Singleton<AudioManager>.SP.PlayCharacterBoost(component.gameObject);
			ParticleSystem[] componentsInChildren = particleSystem.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
		}
	}

	private void Boost(Collider col)
	{
		RaycastCollider component = col.transform.root.GetComponent<RaycastCollider>();
		if ((bool)component)
		{
			component.velocity += base.transform.right * force * Time.fixedDeltaTime * Mathf.Sign(base.transform.localScale.x);
			float num = component.GetComponent<PlatformerPhysics>().movement.minVelocityToStop * 1.1f;
			if (component.velocity.magnitude < num)
			{
				component.velocity = base.transform.right * Mathf.Sign(base.transform.localScale.x) * num;
			}
		}
	}
}
