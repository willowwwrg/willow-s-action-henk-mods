using UnityEngine;

public class HookContainer : MonoBehaviour
{
	public Transform ropeModel;

	public Transform suctionModel;

	public GameObject shootHookParticle;

	public GameObject suctionHitWallParticle;

	public GameObject hookReelUpParticle;

	public GameObject hookReelDownParticle;

	private AudioObject reelAudio;

	private AudioObject swingAudio;

	private void OnDestroy()
	{
		if ((bool)reelAudio)
		{
			reelAudio.Stop();
		}
		if ((bool)swingAudio)
		{
			swingAudio.Stop();
		}
	}

	public void TriggerShootHook()
	{
		AudioController.Play("Hook_shot");
		if ((bool)shootHookParticle)
		{
			ParticleSystem[] componentsInChildren = shootHookParticle.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
		}
	}

	public void TriggerHookRelease()
	{
		AudioController.Play("Hook_hit");
	}

	public void TriggerSuctionHitWall()
	{
		if ((bool)suctionHitWallParticle)
		{
			ParticleSystem[] componentsInChildren = suctionHitWallParticle.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
		}
	}

	public void TriggerHookReel(int direction)
	{
		bool enableEmission = false;
		bool enableEmission2 = false;
		switch (direction)
		{
		case -1:
			enableEmission = true;
			break;
		case 1:
			enableEmission2 = true;
			break;
		}
		if ((bool)hookReelUpParticle)
		{
			ParticleSystem[] componentsInChildren = hookReelUpParticle.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enableEmission = enableEmission;
			}
		}
		if ((bool)hookReelDownParticle)
		{
			ParticleSystem[] componentsInChildren2 = hookReelDownParticle.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].enableEmission = enableEmission2;
			}
		}
		if (reelAudio == null)
		{
			reelAudio = AudioController.Play("Hook_reel");
			reelAudio.volume = 0f;
		}
		float num = 1f;
		if (direction == -1)
		{
			num = 1.25f;
		}
		if (direction == 1)
		{
			num = 0.8f;
		}
		reelAudio.pitch -= (reelAudio.pitch - num) * 10f * Time.fixedDeltaTime;
		float num2 = 0f;
		if (direction != 0)
		{
			num2 = 0.3f;
		}
		reelAudio.volume -= (reelAudio.volume - num2) * 10f * Time.fixedDeltaTime;
	}

	public void UpdateSwing(float speed)
	{
		if (swingAudio == null)
		{
			swingAudio = AudioController.Play("Hook_swing");
			if (swingAudio == null)
			{
				return;
			}
			swingAudio.volume = 0f;
		}
		float t = Mathf.InverseLerp(40f, 100f, speed);
		swingAudio.pitch = Mathf.Lerp(1f, 1.5f, t);
		t = Mathf.InverseLerp(30f, 80f, speed);
		swingAudio.volumeItem = t;
	}
}
