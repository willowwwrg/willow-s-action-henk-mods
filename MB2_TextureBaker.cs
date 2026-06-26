using System;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB2_TextureBaker : MB2_MeshBakerRoot
{
	private static bool VERBOSE;

	[HideInInspector]
	public int maxTilingBakeSize = 1024;

	[HideInInspector]
	public bool doMultiMaterial;

	[HideInInspector]
	public bool fixOutOfBoundsUVs;

	[HideInInspector]
	public Material resultMaterial;

	public MB_MultiMaterial[] resultMaterials = new MB_MultiMaterial[0];

	[HideInInspector]
	public int atlasPadding = 1;

	[HideInInspector]
	public bool resizePowerOfTwoTextures = true;

	[HideInInspector]
	public MB2_PackingAlgorithmEnum texturePackingAlgorithm;

	public List<string> customShaderPropNames = new List<string>();

	public List<GameObject> objsToMesh;

	public override List<GameObject> GetObjectsToCombine()
	{
		if (objsToMesh == null)
		{
			objsToMesh = new List<GameObject>();
		}
		return objsToMesh;
	}

	[Obsolete("CreateAndSaveAtlases is depricated please use CreateAtlases(progressInfo, true, editorFunctions) instead.")]
	public void CreateAndSaveAtlases(ProgressUpdateDelegate progressInfo, MB2_EditorMethodsInterface textureFormatTracker)
	{
		CreateAtlases(progressInfo, saveAtlasesAsAssets: true, textureFormatTracker);
	}

	public MB_AtlasesAndRects[] CreateAtlases()
	{
		return CreateAtlases(null);
	}

	public MB_AtlasesAndRects[] CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface textureFormatTracker = null)
	{
		MB_AtlasesAndRects[] array = null;
		try
		{
			array = _CreateAtlases(progressInfo, saveAtlasesAsAssets, textureFormatTracker);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		finally
		{
			if (saveAtlasesAsAssets && array != null)
			{
				MB_AtlasesAndRects[] array2 = array;
				foreach (MB_AtlasesAndRects mB_AtlasesAndRects in array2)
				{
					if (mB_AtlasesAndRects == null || mB_AtlasesAndRects.atlases == null)
					{
						continue;
					}
					for (int j = 0; j < mB_AtlasesAndRects.atlases.Length; j++)
					{
						if (mB_AtlasesAndRects.atlases[j] != null)
						{
							if (textureFormatTracker != null)
							{
								textureFormatTracker.Destroy(mB_AtlasesAndRects.atlases[j]);
							}
							else
							{
								MB_Utility.Destroy(mB_AtlasesAndRects.atlases[j]);
							}
						}
					}
				}
			}
		}
		return array;
	}

	private MB_AtlasesAndRects[] _CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface textureFormatTracker = null)
	{
		if (saveAtlasesAsAssets && textureFormatTracker == null)
		{
			Debug.LogError("Error in CreateAtlases If saveAtlasesAsAssets = true then textureFormatTracker cannot be null.");
			return null;
		}
		if (saveAtlasesAsAssets && !Application.isEditor)
		{
			Debug.LogError("Error in CreateAtlases If saveAtlasesAsAssets = true it must be called from the Unity Editor.");
			return null;
		}
		if (!MB2_MeshBakerRoot.doCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, textureFormatTracker))
		{
			return null;
		}
		if (doMultiMaterial && !_ValidateResultMaterials())
		{
			return null;
		}
		if (!doMultiMaterial)
		{
			if (resultMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return null;
			}
			Shader shader = resultMaterial.shader;
			for (int i = 0; i < objsToMesh.Count; i++)
			{
				Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
				foreach (Material material in gOMaterials)
				{
					if (material != null && material.shader != shader)
					{
						Debug.LogWarning(string.Concat("Game object ", objsToMesh[i], " does not use shader ", shader, " it may not have the required textures. If not 2x2 clear textures will be generated."));
					}
				}
			}
		}
		for (int k = 0; k < objsToMesh.Count; k++)
		{
			Material[] gOMaterials2 = MB_Utility.GetGOMaterials(objsToMesh[k]);
			for (int l = 0; l < gOMaterials2.Length; l++)
			{
				if (gOMaterials2[l] == null)
				{
					Debug.LogError(string.Concat("Game object ", objsToMesh[k], " has a null material. Can't build atlases"));
					return null;
				}
			}
		}
		MB_TextureCombiner mB_TextureCombiner = new MB_TextureCombiner();
		if (!Application.isPlaying)
		{
			Material[] array;
			if (!doMultiMaterial)
			{
				array = new Material[1] { resultMaterial };
			}
			else
			{
				array = new Material[resultMaterials.Length];
				for (int m = 0; m < array.Length; m++)
				{
					array[m] = resultMaterials[m].combinedMaterial;
				}
			}
			mB_TextureCombiner.SuggestTreatment(objsToMesh, array, customShaderPropNames);
		}
		int num = 1;
		if (doMultiMaterial)
		{
			num = resultMaterials.Length;
		}
		MB_AtlasesAndRects[] array2 = new MB_AtlasesAndRects[num];
		for (int n = 0; n < array2.Length; n++)
		{
			array2[n] = new MB_AtlasesAndRects();
		}
		for (int num2 = 0; num2 < array2.Length; num2++)
		{
			Material material2 = null;
			List<Material> allowedMaterialsFilter = null;
			if (doMultiMaterial)
			{
				allowedMaterialsFilter = resultMaterials[num2].sourceMaterials;
				material2 = resultMaterials[num2].combinedMaterial;
			}
			else
			{
				material2 = resultMaterial;
			}
			Debug.Log("Creating atlases for result material " + material2);
			if (!mB_TextureCombiner.combineTexturesIntoAtlases(progressInfo, array2[num2], material2, objsToMesh, allowedMaterialsFilter, atlasPadding, customShaderPropNames, resizePowerOfTwoTextures, fixOutOfBoundsUVs, maxTilingBakeSize, saveAtlasesAsAssets, texturePackingAlgorithm, textureFormatTracker))
			{
				return null;
			}
		}
		textureBakeResults.combinedMaterialInfo = array2;
		textureBakeResults.doMultiMaterial = doMultiMaterial;
		textureBakeResults.resultMaterial = resultMaterial;
		textureBakeResults.resultMaterials = resultMaterials;
		textureBakeResults.fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		unpackMat2RectMap(textureBakeResults);
		MB2_MeshBakerCommon component = GetComponent<MB2_MeshBakerCommon>();
		if (component != null)
		{
			component.textureBakeResults = textureBakeResults;
		}
		if (VERBOSE)
		{
			Debug.Log("Created Atlases");
		}
		return array2;
	}

	private void unpackMat2RectMap(MB2_TextureBakeResults resultAtlasesAndRects)
	{
		List<Material> list = new List<Material>();
		List<Rect> list2 = new List<Rect>();
		for (int i = 0; i < resultAtlasesAndRects.combinedMaterialInfo.Length; i++)
		{
			Dictionary<Material, Rect> mat2rect_map = resultAtlasesAndRects.combinedMaterialInfo[i].mat2rect_map;
			foreach (Material key in mat2rect_map.Keys)
			{
				list.Add(key);
				list2.Add(mat2rect_map[key]);
			}
		}
		resultAtlasesAndRects.materials = list.ToArray();
		resultAtlasesAndRects.prefabUVRects = list2.ToArray();
	}

	public static void ConfigureNewMaterialToMatchOld(Material newMat, Material original)
	{
		if (original == null)
		{
			Debug.LogWarning(string.Concat("Original material is null, could not copy properties to ", newMat, ". Setting shader to ", newMat.shader));
			return;
		}
		newMat.shader = original.shader;
		newMat.CopyPropertiesFromMaterial(original);
		string[] shaderTexPropertyNames = MB_TextureCombiner.shaderTexPropertyNames;
		for (int i = 0; i < shaderTexPropertyNames.Length; i++)
		{
			Vector2 one = Vector2.one;
			Vector2 zero = Vector2.zero;
			if (newMat.HasProperty(shaderTexPropertyNames[i]))
			{
				newMat.SetTextureOffset(shaderTexPropertyNames[i], zero);
				newMat.SetTextureScale(shaderTexPropertyNames[i], one);
			}
		}
	}

	private string PrintSet(HashSet<Material> s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Material item in s)
		{
			stringBuilder.Append(string.Concat(item, ","));
		}
		return stringBuilder.ToString();
	}

	public bool _ValidateResultMaterials()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		for (int i = 0; i < objsToMesh.Count; i++)
		{
			if (!(objsToMesh[i] != null))
			{
				continue;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
			for (int j = 0; j < gOMaterials.Length; j++)
			{
				if (gOMaterials[j] != null)
				{
					hashSet.Add(gOMaterials[j]);
				}
			}
		}
		HashSet<Material> hashSet2 = new HashSet<Material>();
		for (int k = 0; k < resultMaterials.Length; k++)
		{
			MB_MultiMaterial mB_MultiMaterial = resultMaterials[k];
			if (mB_MultiMaterial.combinedMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return false;
			}
			Shader shader = mB_MultiMaterial.combinedMaterial.shader;
			for (int l = 0; l < mB_MultiMaterial.sourceMaterials.Count; l++)
			{
				if (mB_MultiMaterial.sourceMaterials[l] == null)
				{
					Debug.LogError("There are null entries in the list of Source Materials");
					return false;
				}
				if (shader != mB_MultiMaterial.sourceMaterials[l].shader)
				{
					Debug.LogWarning(string.Concat("Source material ", mB_MultiMaterial.sourceMaterials[l], " does not use shader ", shader, " it may not have the required textures. If not empty textures will be generated."));
				}
				if (hashSet2.Contains(mB_MultiMaterial.sourceMaterials[l]))
				{
					Debug.LogError(string.Concat("A Material ", mB_MultiMaterial.sourceMaterials[l], " appears more than once in the list of source materials in the source material to combined mapping. Each source material must be unique."));
					return false;
				}
				hashSet2.Add(mB_MultiMaterial.sourceMaterials[l]);
			}
		}
		if (hashSet.IsProperSubsetOf(hashSet2))
		{
			hashSet2.ExceptWith(hashSet);
			Debug.LogWarning("There are materials in the mapping that are not used on your source objects: " + PrintSet(hashSet2));
		}
		if (hashSet2.IsProperSubsetOf(hashSet))
		{
			hashSet.ExceptWith(hashSet2);
			Debug.LogError("There are materials on the objects to combine that are not in the mapping: " + PrintSet(hashSet));
			return false;
		}
		return true;
	}
}
