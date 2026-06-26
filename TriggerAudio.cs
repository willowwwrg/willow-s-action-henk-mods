using System.Collections;
using UnityEngine;

public class TriggerAudio : MonoBehaviour
{
	public float delayTime;

	public string audioString = string.Empty;

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
	}

	private IEnumerator Delay()
	{
		yield return new WaitForSeconds(delayTime);
		AudioController.Play(audioString, base.transform);
	}
}
