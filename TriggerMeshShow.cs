using System.Collections;
using UnityEngine;

public class TriggerMeshShow : MonoBehaviour
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
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	private IEnumerator Delay()
	{
		yield return new WaitForSeconds(delayTime);
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}
}
