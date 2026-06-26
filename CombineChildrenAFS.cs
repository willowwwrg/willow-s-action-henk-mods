using System;
using System.Collections;
using UnityEngine;

[AddComponentMenu("AFS/Mesh/AFS Combine Children")]
public class CombineChildrenAFS : MonoBehaviour
{
	public Terrain UnderlayingTerrain;

	public float GroundMaxDistance = 0.25f;

	public bool bakeGroundLightingGrass;

	public Color HealthyColor = new Color(1f, 1f, 1f, 1f);

	public Color DryColor = new Color(1f, 1f, 1f, 1f);

	public float NoiseSpread = 0.1f;

	public bool bakeGroundLightingFoliage;

	public float randomBrightness = 0.25f;

	public float randomPulse = 0.3f;

	public float randomBending = 0.3f;

	public float randomFluttering = 0.3f;

	public float NoiseSpreadFoliage = 0.1f;

	public bool bakeScale = true;

	public bool debugNormals;

	public bool destroyChildObjectsInPlaymode = true;

	public bool CastShadows = true;

	public float RealignGroundMaxDistance = 4f;

	public bool ForceRealignment;

	public bool createUniqueUV2;

	private bool createUniqueUV2playmode;

	public bool isStaticallyCombined;

	public bool simplyCombine;

	private void Start()
	{
		Combine();
	}

	[ContextMenu("Realign Objects")]
	public void Realign()
	{
		Component[] componentsInChildren = GetComponentsInChildren(typeof(MeshFilter));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = componentsInChildren[i].renderer;
			Vector3 position = renderer.transform.position;
			if ((!Physics.Raycast(position + Vector3.up * GroundMaxDistance * 0.5f, Vector3.down, out var hitInfo, GroundMaxDistance) || ForceRealignment) && Physics.Raycast(position + Vector3.up * RealignGroundMaxDistance * 0.5f, Vector3.down, out hitInfo, RealignGroundMaxDistance))
			{
				renderer.transform.position = hitInfo.point;
				Quaternion rotation = renderer.transform.rotation;
				renderer.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
				rotation = new Quaternion(renderer.transform.rotation.x, rotation.y, renderer.transform.rotation.z, renderer.transform.rotation.w);
				renderer.transform.rotation = rotation;
			}
		}
	}

	public bool GetDebugNormalsScript()
	{
		if (base.gameObject.GetComponent<DebugNormalsInEditmode>() == null)
		{
			return false;
		}
		return true;
	}

	public void EnableDebugging()
	{
		GameObject gameObject = base.gameObject;
		if (!GetDebugNormalsScript())
		{
			gameObject.AddComponent<DebugNormalsInEditmode>();
		}
		gameObject.GetComponent<DebugNormalsInEditmode>().enabled = true;
	}

	public void DisableDebugging()
	{
		GameObject gameObject = base.gameObject;
		if (GetDebugNormalsScript())
		{
			gameObject.GetComponent<DebugNormalsInEditmode>().enabled = false;
		}
	}

	[ContextMenu("Combine Now")]
	public void Combine()
	{
		if (isStaticallyCombined)
		{
			return;
		}
		Component[] componentsInChildren = GetComponentsInChildren(typeof(MeshFilter));
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		Hashtable hashtable = new Hashtable();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshFilter meshFilter = (MeshFilter)componentsInChildren[i];
			Renderer renderer = componentsInChildren[i].renderer;
			Vector3 position = renderer.transform.position;
			if (Physics.Raycast(position + Vector3.up * GroundMaxDistance * 0.5f, Vector3.down, out var hitInfo, GroundMaxDistance))
			{
				if (debugNormals)
				{
					Debug.DrawLine(position + Vector3.up * GroundMaxDistance * 0.5f, position - GroundMaxDistance * Vector3.up * 0.5f, Color.green, 5f, depthTest: false);
					if (debugNormals)
					{
						Debug.DrawLine(position, position + hitInfo.normal, Color.red, 5f, depthTest: false);
					}
					if ((bool)UnderlayingTerrain)
					{
						Vector3 vector = (hitInfo.point - UnderlayingTerrain.transform.position) / UnderlayingTerrain.terrainData.size.x;
						if (hitInfo.transform.gameObject.name == UnderlayingTerrain.name)
						{
							if (debugNormals)
							{
								Debug.DrawLine(position, position + UnderlayingTerrain.terrainData.GetInterpolatedNormal(vector.x, vector.z), Color.blue, 5f, depthTest: false);
							}
							hitInfo.normal = UnderlayingTerrain.terrainData.GetInterpolatedNormal(vector.x, vector.z);
						}
					}
				}
			}
			else
			{
				hitInfo.normal = new Vector3(0f, 1f, 0f);
				if (debugNormals)
				{
					Debug.DrawLine(position, position + 1f * hitInfo.normal, Color.yellow, 5f, depthTest: false);
				}
			}
			MeshCombineUtilityAFS.MeshInstance meshInstance = new MeshCombineUtilityAFS.MeshInstance
			{
				mesh = meshFilter.sharedMesh,
				groundNormal = hitInfo.normal,
				scale = meshFilter.transform.localScale.x,
				pivot = meshFilter.transform.position
			};
			if (!(renderer != null) || !renderer.enabled || !(meshInstance.mesh != null))
			{
				continue;
			}
			meshInstance.transform = worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
			Material[] sharedMaterials = renderer.sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				meshInstance.subMeshIndex = Math.Min(j, meshInstance.mesh.subMeshCount - 1);
				ArrayList arrayList = (ArrayList)hashtable[sharedMaterials[j]];
				if (arrayList != null)
				{
					arrayList.Add(meshInstance);
					continue;
				}
				arrayList = new ArrayList();
				arrayList.Add(meshInstance);
				hashtable.Add(sharedMaterials[j], arrayList);
			}
			if (Application.isPlaying)
			{
				if (destroyChildObjectsInPlaymode)
				{
					UnityEngine.Object.Destroy(renderer.gameObject);
				}
			}
			else if (destroyChildObjectsInPlaymode)
			{
				UnityEngine.Object.DestroyImmediate(renderer.gameObject);
				isStaticallyCombined = true;
			}
			else
			{
				renderer.gameObject.SetActive(value: false);
				isStaticallyCombined = true;
			}
		}
		foreach (DictionaryEntry item in hashtable)
		{
			MeshCombineUtilityAFS.MeshInstance[] combines = (MeshCombineUtilityAFS.MeshInstance[])((ArrayList)item.Value).ToArray(typeof(MeshCombineUtilityAFS.MeshInstance));
			if (hashtable.Count == 1)
			{
				if (GetComponent(typeof(MeshFilter)) == null)
				{
					base.gameObject.AddComponent(typeof(MeshFilter));
				}
				if (!GetComponent("MeshRenderer"))
				{
					base.gameObject.AddComponent("MeshRenderer");
				}
				MeshFilter meshFilter2 = (MeshFilter)GetComponent(typeof(MeshFilter));
				base.renderer.material = (Material)item.Key;
				bakeGroundLightingGrass = false;
				bakeGroundLightingFoliage = false;
				simplyCombine = false;
				if (Application.isPlaying)
				{
					switch (base.renderer.material.GetTag("RenderType", searchFallbacks: true, string.Empty))
					{
					case "AfsGrassModel":
					case "AfsGrassModelSingleSided":
						bakeGroundLightingGrass = true;
						break;
					case "AtsFoliage":
						if (base.renderer.material.HasProperty("_GroundLightingAttunation"))
						{
							bakeGroundLightingFoliage = true;
						}
						else
						{
							simplyCombine = true;
						}
						break;
					}
					meshFilter2.mesh = MeshCombineUtilityAFS.Combine(combines, bakeGroundLightingGrass, bakeGroundLightingFoliage, randomBrightness, randomPulse, randomBending, randomFluttering, HealthyColor, DryColor, NoiseSpread, bakeScale, simplyCombine, NoiseSpreadFoliage, createUniqueUV2playmode);
				}
				else
				{
					switch (base.renderer.sharedMaterial.GetTag("RenderType", searchFallbacks: true, string.Empty))
					{
					case "AfsGrassModel":
					case "AfsGrassModelSingleSided":
						bakeGroundLightingGrass = true;
						break;
					case "AtsFoliage":
						if (base.renderer.sharedMaterial.HasProperty("_GroundLightingAttunation"))
						{
							bakeGroundLightingFoliage = true;
						}
						else
						{
							simplyCombine = true;
						}
						break;
					}
					meshFilter2.sharedMesh = MeshCombineUtilityAFS.Combine(combines, bakeGroundLightingGrass, bakeGroundLightingFoliage, randomBrightness, randomPulse, randomBending, randomFluttering, HealthyColor, DryColor, NoiseSpread, bakeScale, simplyCombine, NoiseSpreadFoliage, createUniqueUV2);
				}
				base.renderer.enabled = true;
				if (CastShadows)
				{
					base.renderer.castShadows = true;
				}
				else
				{
					base.renderer.castShadows = false;
				}
				continue;
			}
			GameObject gameObject = new GameObject("Combined mesh");
			gameObject.transform.parent = base.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.AddComponent(typeof(MeshFilter));
			gameObject.AddComponent("MeshRenderer");
			gameObject.renderer.material = (Material)item.Key;
			gameObject.layer = gameObject.transform.parent.gameObject.layer;
			bakeGroundLightingGrass = false;
			bakeGroundLightingFoliage = false;
			simplyCombine = false;
			switch ((!Application.isPlaying) ? gameObject.renderer.sharedMaterial.GetTag("RenderType", searchFallbacks: true, string.Empty) : gameObject.renderer.material.GetTag("RenderType", searchFallbacks: true, string.Empty))
			{
			case "AfsGrassModel":
			case "AfsGrassModelSingleSided":
				bakeGroundLightingGrass = true;
				break;
			case "AtsFoliage":
				if (Application.isPlaying)
				{
					if (gameObject.renderer.material.HasProperty("_GroundLightingAttunation"))
					{
						bakeGroundLightingFoliage = true;
					}
					else
					{
						simplyCombine = true;
					}
				}
				else if (gameObject.renderer.sharedMaterial.HasProperty("_GroundLightingAttunation"))
				{
					bakeGroundLightingFoliage = true;
				}
				else
				{
					simplyCombine = true;
				}
				break;
			}
			MeshFilter meshFilter3 = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
			if (Application.isPlaying)
			{
				meshFilter3.mesh = MeshCombineUtilityAFS.Combine(combines, bakeGroundLightingGrass, bakeGroundLightingFoliage, randomBrightness, randomPulse, randomBending, randomFluttering, HealthyColor, DryColor, NoiseSpread, bakeScale, simplyCombine, NoiseSpreadFoliage, createUniqueUV2playmode);
			}
			else
			{
				meshFilter3.sharedMesh = MeshCombineUtilityAFS.Combine(combines, bakeGroundLightingGrass, bakeGroundLightingFoliage, randomBrightness, randomPulse, randomBending, randomFluttering, HealthyColor, DryColor, NoiseSpread, bakeScale, simplyCombine, NoiseSpreadFoliage, createUniqueUV2);
			}
		}
	}
}
