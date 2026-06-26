using UnityEngine;

public class DisableLightsOnStart : MonoBehaviour
{
	private void Start()
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}
}
