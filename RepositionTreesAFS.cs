using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
[AddComponentMenu("AFS/Trees/AFS Reposition Trees")]
public class RepositionTreesAFS : MonoBehaviour
{
	public bool[] ExcludetreePrototype;

	public string[] TreePrototypeName;

	public List<GameObject> additionalTerrainObjects;

	public Vector3[] treesPos;

	public Terrain myterrainComp;

	public void UpdatePrototypes()
	{
		myterrainComp = base.gameObject.GetComponent(typeof(Terrain)) as Terrain;
		if (ExcludetreePrototype == null)
		{
			ExcludetreePrototype = new bool[myterrainComp.terrainData.treePrototypes.Length];
			TreePrototypeName = new string[myterrainComp.terrainData.treePrototypes.Length];
			for (int i = 0; i < myterrainComp.terrainData.treePrototypes.Length; i++)
			{
				TreePrototypeName[i] = myterrainComp.terrainData.treePrototypes[i].prefab.name;
			}
		}
		else if (ExcludetreePrototype.Length != myterrainComp.terrainData.treePrototypes.Length)
		{
			ExcludetreePrototype = new bool[myterrainComp.terrainData.treePrototypes.Length];
			TreePrototypeName = new string[myterrainComp.terrainData.treePrototypes.Length];
			for (int j = 0; j < myterrainComp.terrainData.treePrototypes.Length; j++)
			{
				TreePrototypeName[j] = myterrainComp.terrainData.treePrototypes[j].prefab.name;
			}
		}
	}

	public void RepositionTrees()
	{
		Terrain terrain = base.gameObject.GetComponent(typeof(Terrain)) as Terrain;
		TreeInstance[] treeInstances = terrain.terrainData.treeInstances;
		Vector3 size = terrain.terrainData.size;
		Vector3 position = terrain.GetPosition();
		treesPos = new Vector3[treeInstances.Length];
		for (int i = 0; i < treeInstances.Length; i++)
		{
			ref Vector3 reference = ref treesPos[i];
			reference = new Vector3(treeInstances[i].position.x * size.x, treeInstances[i].position.y * size.y, treeInstances[i].position.z * size.z) + position + Vector3.up;
		}
		for (int j = 0; j < treeInstances.Length; j++)
		{
			int prototypeIndex = treeInstances[j].prototypeIndex;
			if (ExcludetreePrototype[prototypeIndex])
			{
				continue;
			}
			Vector3 origin = new Vector3(treesPos[j].x, 2000f, treesPos[j].z);
			float num = treeInstances[j].position.y;
			float x = treeInstances[j].position.x;
			float z = treeInstances[j].position.z;
			if (terrain.collider.Raycast(new Ray(origin, new Vector3(0f, -1f, 0f)), out var hitInfo, float.PositiveInfinity))
			{
				num = hitInfo.point.y / size.y;
			}
			for (int k = 0; k < additionalTerrainObjects.Count; k++)
			{
				if (!additionalTerrainObjects[k].collider)
				{
					continue;
				}
				if (additionalTerrainObjects[k].collider.Raycast(new Ray(origin, new Vector3(0f, -1f, 0f)), out var hitInfo2, float.PositiveInfinity))
				{
					float num2 = hitInfo2.point.y / size.y;
					if (num < num2)
					{
						num = num2;
					}
				}
				Vector3 position2 = new Vector3(x, num, z);
				treeInstances[j].position = position2;
			}
		}
		terrain.terrainData.treeInstances = treeInstances;
	}
}
