using UnityEngine;

public class EffectsTrigger : MonoBehaviour
{
	public GameObject[] effects;

	private bool isTriggered;

	public bool disabled;

	private void Awake()
	{
		OnReset();
	}

	private void OnTriggerEnter(Collider col)
	{
		if (!disabled && col.transform.root.name == "Player" && !isTriggered)
		{
			Play();
		}
	}

	private void Play()
	{
		if (disabled)
		{
			return;
		}
		GameObject[] array = effects;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SendMessage("CheckpointTrigger", SendMessageOptions.DontRequireReceiver);
			}
		}
		isTriggered = true;
	}

	public void OnReset()
	{
		if (disabled || !isTriggered)
		{
			return;
		}
		GameObject[] array = effects;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SendMessage("CheckpointReset", SendMessageOptions.DontRequireReceiver);
			}
		}
		isTriggered = false;
	}
}
