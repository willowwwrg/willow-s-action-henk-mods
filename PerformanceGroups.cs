using System.Collections.Generic;
using UnityEngine;

public class PerformanceGroups : MonoBehaviour
{
	[HideInInspector]
	public bool inEditMode;

	public PerformanceGroup[] groups;

	private void Start()
	{
		PerformanceGroup[] array = groups;
		foreach (PerformanceGroup performanceGroup in array)
		{
			performanceGroup.allRenderers = new List<Renderer>();
			if (performanceGroup.findByName == "terrain")
			{
				performanceGroup.terrain = Object.FindObjectOfType<Terrain>();
			}
			else if (performanceGroup.findByName != string.Empty)
			{
				GameObject gameObject = GameObject.Find(performanceGroup.findByName);
				if (gameObject != null)
				{
					performanceGroup.allRenderers.AddRange(gameObject.GetComponentsInChildren<Renderer>());
				}
			}
			GameObject[] gameObjects = performanceGroup.gameObjects;
			foreach (GameObject gameObject2 in gameObjects)
			{
				if (gameObject2 != null)
				{
					performanceGroup.allRenderers.AddRange(gameObject2.GetComponentsInChildren<Renderer>());
				}
			}
		}
	}

	public void ToggleGroup(int groupNum)
	{
		if (groups.Length <= groupNum)
		{
			return;
		}
		PerformanceGroup performanceGroup = groups[groupNum];
		performanceGroup.enabled = !performanceGroup.enabled;
		if (performanceGroup.terrain != null)
		{
			performanceGroup.terrain.enabled = performanceGroup.enabled;
		}
		foreach (Renderer allRenderer in performanceGroup.allRenderers)
		{
			allRenderer.enabled = performanceGroup.enabled;
			ParticleSystem ps = allRenderer.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				if (performanceGroup.enabled)
					ps.Play();
				else
					ps.Stop(true);
			}
		}
	}

	private void Update()
	{
		if (!inEditMode)
		{
			return;
		}
		for (int i = 0; i < groups.Length; i++)
		{
			if (Input.GetKeyDown((KeyCode)(49 + i)))
			{
				ToggleGroup(i);
			}
		}
	}
}
