using UnityEngine;

public class ToggleOnOff : MonoBehaviour
{
	public GameObject[] toggleObjects;

	private void Start()
	{
	}

	public void ToggleOn()
	{
		if (!base.enabled)
		{
			return;
		}
		GameObject[] array = toggleObjects;
		foreach (GameObject obj in array)
		{
			obj.SetActive(value: true);
			ParticleSystem[] componentsInChildren = obj.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enableEmission = true;
			}
		}
	}

	public void ToggleOff()
	{
		if (!base.enabled)
		{
			return;
		}
		GameObject[] array = toggleObjects;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem[] componentsInChildren = array[i].GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enableEmission = false;
			}
		}
	}
}
