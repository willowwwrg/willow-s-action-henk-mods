using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PerformanceGroup
{
	public string name;

	public string findByName;

	public GameObject[] gameObjects;

	[HideInInspector]
	public Terrain terrain;

	[HideInInspector]
	public List<Renderer> allRenderers;

	[HideInInspector]
	public bool enabled = true;
}
