using System.Collections.Generic;
using UnityEngine;

public class AdditionalSkinParticles : MonoBehaviour
{
	public List<AdditionalParticlesStruct> additionalParticles;

	private void Awake()
	{
	}

	public void Play(SfxEvents sfxEvent)
	{
		if (additionalParticles.Count <= 0)
		{
			return;
		}
		foreach (AdditionalParticlesStruct additionalParticle in additionalParticles)
		{
			if (additionalParticle.trigger == sfxEvent)
			{
				additionalParticle.particleEffect.particleSystem.Play();
			}
		}
	}
}
