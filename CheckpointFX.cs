using System.Collections.Generic;
using UnityEngine;

public class CheckpointFX : MonoBehaviour
{
	public List<GameObject> baseFX;

	public List<GameObject> improvedFX;

	private void Awake()
	{
	}

	public void PlayFX(bool improved)
	{
		Play(baseFX);
		if (improved)
		{
			Play(improvedFX);
		}
	}

	private void Play(List<GameObject> objectsToTrigger)
	{
		foreach (GameObject item in objectsToTrigger)
		{
			if ((bool)item)
			{
				item.SendMessage("CheckpointTrigger", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void ResetFX()
	{
		Reset(baseFX);
		Reset(improvedFX);
	}

	private void Reset(List<GameObject> objectsToReset)
	{
		foreach (GameObject item in objectsToReset)
		{
			if ((bool)item)
			{
				item.SendMessage("CheckpointReset", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
