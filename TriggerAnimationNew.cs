using System.Collections;
using UnityEngine;

public class TriggerAnimationNew : MonoBehaviour
{
	public float delayTime;

	public float animationSpeed = 1f;

	private void Awake()
	{
		CheckpointReset();
	}

	private void Start()
	{
		foreach (AnimationState item in base.animation)
		{
			item.speed = animationSpeed;
		}
	}

	private void CheckpointTrigger()
	{
		StartCoroutine("Delay");
	}

	private void CheckpointReset()
	{
		StopCoroutine("Delay");
		base.animation.Play();
		base.animation.Sample();
		base.animation.Stop();
	}

	private IEnumerator Delay()
	{
		yield return new WaitForSeconds(delayTime);
		base.animation.Play();
	}
}
