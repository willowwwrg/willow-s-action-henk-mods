using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
	private PlatformerPhysics player;

	private RaycastCollider playerCollider;

	private ParticleSystem[] particles;

	public TriggerAction triggerAction;

	public bool ghostOnly;

	private void Start()
	{
		if (!base.transform.root.GetComponent<PlatformerPhysics>())
		{
			Debug.LogError("This class must be attached to a child of the player!");
		}
		player = base.transform.root.GetComponent<PlatformerPhysics>();
		playerCollider = base.transform.root.GetComponent<RaycastCollider>();
		particles = GetComponentsInChildren<ParticleSystem>();
	}

	private void Play()
	{
		if (!ghostOnly || !(player != null) || Singleton<PlayerManager>.SP.IsGhost(player.gameObject))
		{
			ParticleSystem[] array = particles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}
	}

	private void OnEventJump(bool wallJump)
	{
		if (triggerAction == TriggerAction.OnJump)
		{
			Play();
		}
	}

	private void OnEventLand()
	{
		if (triggerAction == TriggerAction.OnLand)
		{
			Play();
		}
	}

	private void OnEventLava()
	{
		if (triggerAction == TriggerAction.OnLava)
		{
			Play();
		}
	}
}
