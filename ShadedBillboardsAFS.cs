using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
[ExecuteInEditMode]
[AddComponentMenu("AFS/Trees/AFS Shaded Billboards")]
public class ShadedBillboardsAFS : MonoBehaviour
{
	public bool showRays;

	public bool shadeOnlyWithinShadowRange;

	public GameObject lightRef;

	public List<GameObject> shadowCasters;

	public int treesPerFrame = 10;

	public float changeStateSpeed = 4f;

	public float treeYOffset;

	public Vector3[] treesPos;

	public bool[] treesShadowed;

	public float[] treesStates;

	public int curIdx;

	private Terrain terrainComp;

	private TreeInstance[] treeInstances;

	private void Awake()
	{
		terrainComp = base.gameObject.GetComponent(typeof(Terrain)) as Terrain;
		treeInstances = terrainComp.terrainData.treeInstances;
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			ShadeTrees();
		}
	}

	public void UpdateTrees()
	{
		if (treeInstances == null)
		{
			treeInstances = terrainComp.terrainData.treeInstances;
		}
		Vector3 size = terrainComp.terrainData.size;
		Vector3 position = terrainComp.GetPosition();
		treesPos = new Vector3[treeInstances.Length];
		treesShadowed = new bool[treeInstances.Length];
		treesStates = new float[treeInstances.Length];
		curIdx = 0;
		for (int i = 0; i < treeInstances.Length; i++)
		{
			ref Vector3 reference = ref treesPos[i];
			reference = new Vector3(treeInstances[i].position.x * size.x, treeInstances[i].position.y * size.y, treeInstances[i].position.z * size.z) + position + Vector3.up;
			treesShadowed[i] = false;
			treesStates[i] = 0f;
		}
	}

	public void RemoveLightMap()
	{
		if (treeInstances == null)
		{
			treeInstances = terrainComp.terrainData.treeInstances;
		}
		for (int i = 0; i < treeInstances.Length; i++)
		{
			treeInstances[i].lightmapColor = new Color(1f, 1f, 1f, 1f);
		}
		terrainComp.terrainData.treeInstances = treeInstances;
	}

	public void RemoveShadowing()
	{
		if (treeInstances == null)
		{
			treeInstances = terrainComp.terrainData.treeInstances;
		}
		for (int i = 0; i < treeInstances.Length; i++)
		{
			treeInstances[i].color = new Color(treeInstances[i].color.r, treeInstances[i].color.g, treeInstances[i].color.b, 1f);
		}
		terrainComp.terrainData.treeInstances = treeInstances;
	}

	public void ShadeTrees()
	{
		if (lightRef == null || shadowCasters == null)
		{
			return;
		}
		if (treesPos == null || treesPos.Length == 0)
		{
			UpdateTrees();
		}
		float deltaTime = Time.deltaTime;
		if (treeInstances == null)
		{
			treeInstances = terrainComp.terrainData.treeInstances;
		}
		int num = treeInstances.Length;
		if (lightRef.light.shadows != LightShadows.None)
		{
			for (int i = 0; i < treesPerFrame; i++)
			{
				int num2 = (curIdx + i) % num;
				Vector3 vector = treesPos[num2];
				treesShadowed[num2] = false;
				for (int j = 0; j < shadowCasters.Count; j++)
				{
					RaycastHit hitInfo;
					if ((bool)shadowCasters[j].collider)
					{
						if (shadowCasters[j].collider.Raycast(new Ray(vector + new Vector3(0f, treeYOffset, 0f), lightRef.transform.rotation * new Vector3(0f, 0f, -1f)), out hitInfo, float.PositiveInfinity))
						{
							treesShadowed[num2] = true;
							break;
						}
						continue;
					}
					for (int k = 0; k < shadowCasters[j].transform.childCount; k++)
					{
						if ((bool)shadowCasters[j].transform.GetChild(k).collider && shadowCasters[j].collider.Raycast(new Ray(vector + new Vector3(0f, treeYOffset, 0f), lightRef.transform.rotation * new Vector3(0f, 0f, -1f)), out hitInfo, float.PositiveInfinity))
						{
							treesShadowed[num2] = true;
							break;
						}
					}
					if (treesShadowed[num2])
					{
						break;
					}
				}
			}
			curIdx += treesPerFrame;
			if (curIdx >= num)
			{
				curIdx -= num;
			}
			for (int l = 0; l < num; l++)
			{
				Vector3 b = treesPos[l];
				bool flag = Vector3.Distance(Camera.main.transform.position, b) > QualitySettings.shadowDistance;
				if (shadeOnlyWithinShadowRange && flag)
				{
					treeInstances[l].color = new Color(treeInstances[l].color.r, treeInstances[l].color.g, treeInstances[l].color.b, 1f);
					continue;
				}
				if (treesShadowed[l] && treesStates[l] < 1f)
				{
					treesStates[l] += changeStateSpeed * deltaTime;
				}
				else if (!treesShadowed[l] && treesStates[l] > 0f)
				{
					treesStates[l] -= changeStateSpeed * deltaTime;
				}
				treesStates[l] = Mathf.Clamp01(treesStates[l]);
				treeInstances[l].color = Color.Lerp(new Color(treeInstances[l].color.r, treeInstances[l].color.g, treeInstances[l].color.b, 1f), new Color(treeInstances[l].color.r, treeInstances[l].color.g, treeInstances[l].color.b, 0f), treesStates[l]);
			}
		}
		else
		{
			for (int m = 0; m < num; m++)
			{
				treeInstances[m].color = new Color(treeInstances[m].color.r, treeInstances[m].color.g, treeInstances[m].color.b, 1f);
			}
		}
		terrainComp.terrainData.treeInstances = treeInstances;
	}
}
