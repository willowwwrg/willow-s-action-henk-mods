using System.Collections;
using UnityEngine;

public class TriggerParticles : MonoBehaviour
{
	public float delayTime;

	private void Awake()
	{
		CheckpointReset();
	}

	private void CheckpointTrigger()
	{
		StartCoroutine("Delay");
	}

	private void CheckpointReset()
	{
		StopCoroutine("Delay");
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop();
		}
	}

	private IEnumerator Delay()
	{
		yield return new WaitForSeconds(delayTime);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
	}
}
