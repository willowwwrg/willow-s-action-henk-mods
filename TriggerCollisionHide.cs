using System.Collections;
using UnityEngine;

public class TriggerCollisionHide : MonoBehaviour
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
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	private IEnumerator Delay()
	{
		yield return new WaitForSeconds(delayTime);
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}
}
