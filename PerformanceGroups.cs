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
		// If re-enabling, refresh the renderer list in case new objects spawned since the initial scan
		if (performanceGroup.enabled && performanceGroup.gameObjects != null)
		{
			performanceGroup.allRenderers.Clear();
			foreach (GameObject go in performanceGroup.gameObjects)
			{
				if (go != null)
					performanceGroup.allRenderers.AddRange(go.GetComponentsInChildren<Renderer>());
			}
			if (performanceGroup.findByName != string.Empty)
			{
				GameObject found = GameObject.Find(performanceGroup.findByName);
				if (found != null)
					performanceGroup.allRenderers.AddRange(found.GetComponentsInChildren<Renderer>());
			}
		}
		foreach (Renderer allRenderer in performanceGroup.allRenderers)
		{
			if (allRenderer == null) continue;
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
	}
}
