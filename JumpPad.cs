using UnityEngine;

public class JumpPad : MonoBehaviour
{
	public float force = 50f;

	public float flatVelocityMultiplier = 0.5f;

	public float verticalVelocityMultiplier = 0.25f;

	public Transform animatedModel;

	public new GameObject particleSystem;

	private bool playParticles;

	private void OnTriggerEnter(Collider col)
	{
		RaycastCollider component = col.transform.root.GetComponent<RaycastCollider>();
		if (!component)
		{
			return;
		}
		float num = Vector3.Dot(component.velocity, base.transform.up);
		Vector3 vector = component.velocity - num * base.transform.up;
		vector *= flatVelocityMultiplier;
		if (num > 0f)
		{
			num = 0f;
		}
		Vector3 vector2 = (0f - num) * base.transform.up * verticalVelocityMultiplier;
		component.velocity = vector + vector2 + force * base.transform.up;
		component.GetComponent<PlatformerPhysics>().LeftGround(dontAllowLateJump: true);
		if (!Singleton<PlayerManager>.SP.IsLocalPlayer(component.gameObject))
		{
			return;
		}
		animatedModel.animation["Take 001"].speed = 0.7f;
		animatedModel.animation.Play("Take 001");
		AudioController.Play("Jump_pad", base.transform);
		Singleton<AudioManager>.SP.PlayCharacterJumppad(component.gameObject);
		component.gameObject.BroadcastMessage("AddSpeedLines", 3f, SendMessageOptions.DontRequireReceiver);
		if ((bool)particleSystem && playParticles)
		{
			ParticleSystem[] componentsInChildren = particleSystem.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
		}
	}

	private void OnDrawGizmos()
	{
		float num = 0.2f;
		float num2 = 50f;
		float num3 = force - force * num * 0.45f;
		float num4 = 0.5f * num2 * (num3 / num2) * (num3 / num2);
		Gizmos.color = Color.yellow;
		Gizmos.DrawRay(base.transform.position, num4 * base.transform.up);
	}
}
