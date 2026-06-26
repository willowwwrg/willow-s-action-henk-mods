using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB_TextureCombiner
{
	public class MeshBakerMaterialTexture
	{
		public Vector2 offset = new Vector2(0f, 0f);

		public Vector2 scale = new Vector2(1f, 1f);

		public Vector2 obUVoffset = new Vector2(0f, 0f);

		public Vector2 obUVscale = new Vector2(1f, 1f);

		public Texture2D t;

		public MeshBakerMaterialTexture()
		{
		}

		public MeshBakerMaterialTexture(Texture2D tx)
		{
			t = tx;
		}

		public MeshBakerMaterialTexture(Texture2D tx, Vector2 o, Vector2 s, Vector2 oUV, Vector2 sUV)
		{
			t = tx;
			offset = o;
			scale = s;
			obUVoffset = oUV;
			obUVscale = sUV;
		}
	}

	private class MB_TexSet
	{
		public MeshBakerMaterialTexture[] ts;

		public List<Material> mats;

		public List<GameObject> gos;

		public int idealWidth;

		public int idealHeight;

		public MB_TexSet(MeshBakerMaterialTexture[] tss)
		{
			ts = tss;
			mats = new List<Material>();
			gos = new List<GameObject>();
		}

		public bool IsEqual(object obj, bool fixOutOfBoundsUVs)
		{
			if (!(obj is MB_TexSet))
			{
				return false;
			}
			MB_TexSet mB_TexSet = (MB_TexSet)obj;
			if (mB_TexSet.ts.Length != ts.Length)
			{
				return false;
			}
			for (int i = 0; i < ts.Length; i++)
			{
				if (ts[i].offset != mB_TexSet.ts[i].offset)
				{
					return false;
				}
				if (ts[i].scale != mB_TexSet.ts[i].scale)
				{
					return false;
				}
				if (ts[i].t != mB_TexSet.ts[i].t)
				{
					return false;
				}
				if (fixOutOfBoundsUVs && ts[i].obUVoffset != mB_TexSet.ts[i].obUVoffset)
				{
					return false;
				}
				if (fixOutOfBoundsUVs && ts[i].obUVscale != mB_TexSet.ts[i].obUVscale)
				{
					return false;
				}
			}
			return true;
		}
	}

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	public static string[] shaderTexPropertyNames = new string[14]
	{
		"_MainTex", "_BumpMap", "_Normal", "_BumpSpecMap", "_DecalTex", "_Detail", "_GlossMap", "_Illum", "_LightTextureB0", "_ParallaxMap",
		"_ShadowOffset", "_TranslucencyMap", "_SpecMap", "_TranspMap"
	};

	public int atlasPadding = 1;

	public bool resizePowerOfTwoTextures;

	public bool fixOutOfBoundsUVs;

	public int maxTilingBakeSize = 1024;

	public bool saveAtlasesAsAssets;

	public MB2_PackingAlgorithmEnum packingAlgorithm;

	private List<string> customShaderPropNames = new List<string>();

	private List<Texture2D> _temporaryTextures = new List<Texture2D>();

	public bool combineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, int atlasPadding, List<string> customShaderPropNames, bool resizePowerOfTwoTextures, bool fixOutOfBoundsUVs, int maxTilingBakeSize, bool saveAtlasesAsAssets = false, MB2_PackingAlgorithmEnum packingAlgorithm = MB2_PackingAlgorithmEnum.UnitysPackTextures, MB2_EditorMethodsInterface textureEditorMethods = null)
	{
		this.atlasPadding = atlasPadding;
		this.resizePowerOfTwoTextures = resizePowerOfTwoTextures;
		this.fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		this.maxTilingBakeSize = maxTilingBakeSize;
		this.saveAtlasesAsAssets = saveAtlasesAsAssets;
		this.packingAlgorithm = packingAlgorithm;
		this.customShaderPropNames = customShaderPropNames;
		return _combineTexturesIntoAtlases(progressInfo, resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, textureEditorMethods);
	}

	private bool _collectPropertyNames(Material resultMaterial, List<string> texPropertyNames)
	{
		int i;
		for (i = 0; i < texPropertyNames.Count; i++)
		{
			string text = customShaderPropNames.Find((string x) => x.Equals(texPropertyNames[i]));
			if (text != null)
			{
				customShaderPropNames.Remove(text);
			}
		}
		if (resultMaterial == null)
		{
			Debug.LogError("Please assign a result material. The combined mesh will use this material.");
			return false;
		}
		string text2 = string.Empty;
		for (int num = 0; num < shaderTexPropertyNames.Length; num++)
		{
			if (resultMaterial.HasProperty(shaderTexPropertyNames[num]))
			{
				text2 = text2 + ", " + shaderTexPropertyNames[num];
				if (!texPropertyNames.Contains(shaderTexPropertyNames[num]))
				{
					texPropertyNames.Add(shaderTexPropertyNames[num]);
				}
				if (resultMaterial.GetTextureOffset(shaderTexPropertyNames[num]) != new Vector2(0f, 0f) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Result material has non-zero offset. This is may be incorrect.");
				}
				if (resultMaterial.GetTextureScale(shaderTexPropertyNames[num]) != new Vector2(1f, 1f) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Result material should may be have tiling of 1,1");
				}
			}
		}
		for (int num2 = 0; num2 < customShaderPropNames.Count; num2++)
		{
			if (resultMaterial.HasProperty(customShaderPropNames[num2]))
			{
				text2 = text2 + ", " + customShaderPropNames[num2];
				texPropertyNames.Add(customShaderPropNames[num2]);
				if (resultMaterial.GetTextureOffset(customShaderPropNames[num2]) != new Vector2(0f, 0f) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Result material has non-zero offset. This is probably incorrect.");
				}
				if (resultMaterial.GetTextureScale(customShaderPropNames[num2]) != new Vector2(1f, 1f) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Result material should probably have tiling of 1,1.");
				}
			}
			else if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Result material shader does not use property " + customShaderPropNames[num2] + " in the list of custom shader property names");
			}
		}
		return true;
	}

	private bool _combineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods)
	{
		bool result = false;
		try
		{
			_temporaryTextures.Clear();
			textureEditorMethods?.Clear();
			if (objsToMesh == null || objsToMesh.Count == 0)
			{
				Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
				return false;
			}
			if (atlasPadding < 0)
			{
				Debug.LogError("Atlas padding must be zero or greater.");
				return false;
			}
			if (maxTilingBakeSize < 2 || maxTilingBakeSize > 4096)
			{
				Debug.LogError("Invalid value for max tiling bake size.");
				return false;
			}
			progressInfo?.Invoke("Collecting textures for " + objsToMesh.Count + " meshes.", 0.01f);
			List<string> texPropertyNames = new List<string>();
			if (!_collectPropertyNames(resultMaterial, texPropertyNames))
			{
				return false;
			}
			result = __combineTexturesIntoAtlases(progressInfo, resultAtlasesAndRects, resultMaterial, texPropertyNames, objsToMesh, allowedMaterialsFilter, textureEditorMethods);
		}
		catch (MissingReferenceException message)
		{
			Debug.LogError("Creating atlases failed a MissingReferenceException was thrown. This is normally only happens when trying to create very large atlases and Unity is running out of Memory. Try changing the 'Texture Packer' to a different option, it may work with an alternate packer. This error is sometimes intermittant. Try baking again.");
			Debug.LogError(message);
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
		}
		finally
		{
			_destroyTemporaryTextures();
			textureEditorMethods?.SetReadFlags(progressInfo);
		}
		return result;
	}

	private bool __combineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<string> texPropertyNames, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("__combineTexturesIntoAtlases atlases:" + texPropertyNames.Count + " objsToMesh:" + objsToMesh.Count + " fixOutOfBoundsUVs:" + fixOutOfBoundsUVs);
		}
		progressInfo?.Invoke("Collecting textures ", 0.01f);
		List<MB_TexSet> distinctMaterialTextures = new List<MB_TexSet>();
		List<GameObject> usedObjsToMesh = new List<GameObject>();
		if (!__Step1_CollectDistinctMatTexturesAndUsedObjects(objsToMesh, allowedMaterialsFilter, texPropertyNames, textureEditorMethods, distinctMaterialTextures, usedObjsToMesh))
		{
			return false;
		}
		int padding = __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(distinctMaterialTextures);
		__Step3_BuildAndSaveAtlasesAndStoreResults(progressInfo, distinctMaterialTextures, texPropertyNames, padding, textureEditorMethods, resultAtlasesAndRects, resultMaterial);
		return true;
	}

	private bool __Step1_CollectDistinctMatTexturesAndUsedObjects(List<GameObject> allObjsToMesh, List<Material> allowedMaterialsFilter, List<string> texPropertyNames, MB2_EditorMethodsInterface textureEditorMethods, List<MB_TexSet> distinctMaterialTextures, List<GameObject> usedObjsToMesh)
	{
		bool flag = false;
		for (int i = 0; i < allObjsToMesh.Count; i++)
		{
			GameObject gameObject = allObjsToMesh[i];
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Collecting textures for object " + gameObject);
			}
			if (gameObject == null)
			{
				Debug.LogError("The list of objects to mesh contained nulls.");
				return false;
			}
			Mesh mesh = MB_Utility.GetMesh(gameObject);
			if (mesh == null)
			{
				Debug.LogError("Object " + gameObject.name + " in the list of objects to mesh has no mesh.");
				return false;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(gameObject);
			if (gOMaterials == null)
			{
				Debug.LogError("Object " + gameObject.name + " in the list of objects has no materials.");
				return false;
			}
			for (int j = 0; j < gOMaterials.Length; j++)
			{
				Material material = gOMaterials[j];
				if (allowedMaterialsFilter != null && !allowedMaterialsFilter.Contains(material))
				{
					continue;
				}
				Rect uvBounds = default(Rect);
				bool flag2 = MB_Utility.hasOutOfBoundsUVs(mesh, ref uvBounds, j);
				flag = flag || flag2;
				if (material.name.Contains("(Instance)"))
				{
					Debug.LogError("The sharedMaterial on object " + gameObject.name + " has been 'Instanced'. This was probably caused by a script accessing the meshRender.material property in the editor.  The material to UV Rectangle mapping will be incorrect. To fix this recreate the object from its prefab or re-assign its material from the correct asset.");
					return false;
				}
				if (fixOutOfBoundsUVs && !MB_Utility.validateOBuvsMultiMaterial(gOMaterials) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Object " + gameObject.name + " uses the same material on multiple submeshes. This may generate strange resultAtlasesAndRects especially when used with fix out of bounds uvs. Try duplicating the material.");
				}
				MeshBakerMaterialTexture[] array = new MeshBakerMaterialTexture[texPropertyNames.Count];
				for (int k = 0; k < texPropertyNames.Count; k++)
				{
					Texture2D texture2D = null;
					Vector2 s = Vector2.one;
					Vector2 o = Vector2.zero;
					Vector2 sUV = Vector2.one;
					Vector2 oUV = Vector2.zero;
					if (material.HasProperty(texPropertyNames[k]))
					{
						Texture texture = material.GetTexture(texPropertyNames[k]);
						if (texture != null)
						{
							if (!(texture is Texture2D))
							{
								Debug.LogError("Object " + gameObject.name + " in the list of objects to mesh uses a Texture that is not a Texture2D. Cannot build atlases.");
								return false;
							}
							texture2D = (Texture2D)texture;
							TextureFormat format = texture2D.format;
							bool flag3 = false;
							if (!Application.isPlaying && textureEditorMethods != null)
							{
								flag3 = textureEditorMethods.IsNormalMap(texture2D);
							}
							if ((format != TextureFormat.ARGB32 && format != TextureFormat.RGBA32 && format != TextureFormat.BGRA32 && format != TextureFormat.RGB24 && format != TextureFormat.Alpha8) || flag3)
							{
								if (Application.isPlaying)
								{
									Debug.LogError(string.Concat("Object ", gameObject.name, " in the list of objects to mesh uses Texture ", texture2D.name, " uses format ", format, " that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These textures cannot be resized at runtime. Try changing texture format. If format says 'compressed' try changing it to 'truecolor'"));
									return false;
								}
								textureEditorMethods?.AddTextureFormat(texture2D, flag3);
								texture2D = (Texture2D)material.GetTexture(texPropertyNames[k]);
							}
						}
						o = material.GetTextureOffset(texPropertyNames[k]);
						s = material.GetTextureScale(texPropertyNames[k]);
					}
					if (texture2D == null && LOG_LEVEL >= MB2_LogLevel.warn)
					{
						Debug.LogWarning("No texture selected for " + texPropertyNames[k] + " in object " + allObjsToMesh[i].name + ". A 2x2 clear texture will be generated and used in the atlas.");
					}
					if (flag2)
					{
						sUV = new Vector2(uvBounds.width, uvBounds.height);
						oUV = new Vector2(uvBounds.x, uvBounds.y);
					}
					array[k] = new MeshBakerMaterialTexture(texture2D, o, s, oUV, sUV);
				}
				MB_TexSet setOfTexs = new MB_TexSet(array);
				MB_TexSet mB_TexSet = distinctMaterialTextures.Find((MB_TexSet x) => x.IsEqual(setOfTexs, fixOutOfBoundsUVs));
				if (mB_TexSet != null)
				{
					setOfTexs = mB_TexSet;
				}
				else
				{
					distinctMaterialTextures.Add(setOfTexs);
				}
				if (!setOfTexs.mats.Contains(material))
				{
					setOfTexs.mats.Add(material);
				}
				if (!setOfTexs.gos.Contains(gameObject))
				{
					setOfTexs.gos.Add(gameObject);
					if (!usedObjsToMesh.Contains(gameObject))
					{
						usedObjsToMesh.Add(gameObject);
					}
				}
			}
		}
		return true;
	}

	private int __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(List<MB_TexSet> distinctMaterialTextures)
	{
		int num = atlasPadding;
		if (distinctMaterialTextures.Count == 1)
		{
			if (LOG_LEVEL >= MB2_LogLevel.info)
			{
				Debug.Log("All objects use the same textures in this set of atlases. Original textures will be reused instead of creating atlases.");
			}
			num = 0;
		}
		else
		{
			for (int i = 0; i < distinctMaterialTextures.Count; i++)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Calculating ideal sizes for texSet TexSet " + i + " of " + distinctMaterialTextures.Count);
				}
				MB_TexSet mB_TexSet = distinctMaterialTextures[i];
				mB_TexSet.idealWidth = 1;
				mB_TexSet.idealHeight = 1;
				int num2 = 1;
				int num3 = 1;
				for (int j = 0; j < mB_TexSet.ts.Length; j++)
				{
					MeshBakerMaterialTexture meshBakerMaterialTexture = mB_TexSet.ts[j];
					if (!meshBakerMaterialTexture.scale.Equals(Vector2.one) && distinctMaterialTextures.Count > 1 && LOG_LEVEL >= MB2_LogLevel.warn)
					{
						Debug.LogWarning(string.Concat("Texture ", meshBakerMaterialTexture.t, "is tiled by ", meshBakerMaterialTexture.scale, " tiling will be baked into a texture with maxSize:", maxTilingBakeSize));
					}
					if (!meshBakerMaterialTexture.obUVscale.Equals(Vector2.one) && distinctMaterialTextures.Count > 1 && fixOutOfBoundsUVs && LOG_LEVEL >= MB2_LogLevel.warn)
					{
						Debug.LogWarning(string.Concat("Texture ", meshBakerMaterialTexture.t, "has out of bounds UVs that effectively tile by ", meshBakerMaterialTexture.obUVscale, " tiling will be baked into a texture with maxSize:", maxTilingBakeSize));
					}
					if (!(meshBakerMaterialTexture.t != null))
					{
						continue;
					}
					Vector2 adjustedForScaleAndOffset2Dimensions = getAdjustedForScaleAndOffset2Dimensions(meshBakerMaterialTexture);
					if ((int)(adjustedForScaleAndOffset2Dimensions.x * adjustedForScaleAndOffset2Dimensions.y) > num2 * num3)
					{
						if (LOG_LEVEL >= MB2_LogLevel.trace)
						{
							Debug.Log(string.Concat("    matTex ", meshBakerMaterialTexture.t, " ", adjustedForScaleAndOffset2Dimensions, " has a bigger size than ", num2, " ", num3));
						}
						num2 = (int)adjustedForScaleAndOffset2Dimensions.x;
						num3 = (int)adjustedForScaleAndOffset2Dimensions.y;
					}
				}
				if (resizePowerOfTwoTextures)
				{
					if (IsPowerOfTwo(num2))
					{
						num2 -= num * 2;
					}
					if (IsPowerOfTwo(num3))
					{
						num3 -= num * 2;
					}
					if (num2 < 1)
					{
						num2 = 1;
					}
					if (num3 < 1)
					{
						num3 = 1;
					}
				}
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					Debug.Log("    Ideal size is " + num2 + " " + num3);
				}
				mB_TexSet.idealWidth = num2;
				mB_TexSet.idealHeight = num3;
			}
		}
		return num;
	}

	private void __Step3_BuildAndSaveAtlasesAndStoreResults(ProgressUpdateDelegate progressInfo, List<MB_TexSet> distinctMaterialTextures, List<string> texPropertyNames, int _padding, MB2_EditorMethodsInterface textureEditorMethods, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial)
	{
		int count = texPropertyNames.Count;
		StringBuilder stringBuilder = new StringBuilder();
		if (count > 0)
		{
			stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Report");
			for (int i = 0; i < distinctMaterialTextures.Count; i++)
			{
				MB_TexSet mB_TexSet = distinctMaterialTextures[i];
				stringBuilder.AppendLine("----------");
				stringBuilder.Append("This set of textures will be resized to:" + mB_TexSet.idealWidth + "x" + mB_TexSet.idealHeight + "\n");
				for (int j = 0; j < mB_TexSet.ts.Length; j++)
				{
					if (mB_TexSet.ts[j].t != null)
					{
						stringBuilder.Append("   [" + texPropertyNames[j] + " " + mB_TexSet.ts[j].t.name + " " + mB_TexSet.ts[j].t.width + "x" + mB_TexSet.ts[j].t.height + "]");
						if (mB_TexSet.ts[j].scale != Vector2.one || mB_TexSet.ts[j].offset != Vector2.zero)
						{
							stringBuilder.AppendFormat(" material scale {0} offset{1} ", mB_TexSet.ts[j].scale.ToString("G4"), mB_TexSet.ts[j].offset.ToString("G4"));
						}
						if (mB_TexSet.ts[j].obUVscale != Vector2.one || mB_TexSet.ts[j].obUVoffset != Vector2.zero)
						{
							stringBuilder.AppendFormat(" obUV scale {0} offset{1} ", mB_TexSet.ts[j].obUVscale.ToString("G4"), mB_TexSet.ts[j].obUVoffset.ToString("G4"));
						}
						stringBuilder.AppendLine(string.Empty);
					}
					else
					{
						stringBuilder.Append("   [" + texPropertyNames[j] + " null a blank texture will be created]\n");
					}
				}
				stringBuilder.AppendLine(string.Empty);
				stringBuilder.Append("Materials using:");
				for (int k = 0; k < mB_TexSet.mats.Count; k++)
				{
					stringBuilder.Append(mB_TexSet.mats[k].name + ", ");
				}
				stringBuilder.AppendLine(string.Empty);
			}
		}
		progressInfo?.Invoke("Creating txture atlases.", 0.1f);
		GC.Collect();
		Texture2D[] array = new Texture2D[count];
		Rect[] array2 = ((packingAlgorithm != MB2_PackingAlgorithmEnum.UnitysPackTextures) ? __CreateAtlasesMBTexturePacker(progressInfo, count, distinctMaterialTextures, texPropertyNames, resultMaterial, array, textureEditorMethods, _padding) : __CreateAtlasesUnityTexturePacker(progressInfo, count, distinctMaterialTextures, texPropertyNames, resultMaterial, array, textureEditorMethods, _padding));
		progressInfo?.Invoke("Building Report", 0.7f);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("---- Atlases ------");
		for (int l = 0; l < count; l++)
		{
			if (array[l] != null)
			{
				stringBuilder2.AppendLine("Created Atlas For: " + texPropertyNames[l] + " h=" + array[l].height + " w=" + array[l].width);
			}
		}
		stringBuilder.Append(stringBuilder2.ToString());
		Dictionary<Material, Rect> dictionary = new Dictionary<Material, Rect>();
		for (int m = 0; m < distinctMaterialTextures.Count; m++)
		{
			List<Material> mats = distinctMaterialTextures[m].mats;
			for (int n = 0; n < mats.Count; n++)
			{
				if (!dictionary.ContainsKey(mats[n]))
				{
					dictionary.Add(mats[n], array2[m]);
				}
			}
		}
		resultAtlasesAndRects.atlases = array;
		resultAtlasesAndRects.texPropertyNames = texPropertyNames.ToArray();
		resultAtlasesAndRects.mat2rect_map = dictionary;
		progressInfo?.Invoke("Restoring Texture Formats & Read Flags", 0.8f);
		_destroyTemporaryTextures();
		textureEditorMethods?.SetReadFlags(progressInfo);
		if (stringBuilder != null && LOG_LEVEL >= MB2_LogLevel.info)
		{
			Debug.Log(stringBuilder.ToString());
		}
	}

	private Rect[] __CreateAtlasesMBTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<string> texPropertyNames, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding)
	{
		Rect[] array;
		if (distinctMaterialTextures.Count == 1)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Only one image per atlas. Will re-use original texture");
			}
			array = new Rect[1]
			{
				new Rect(0f, 0f, 1f, 1f)
			};
			for (int i = 0; i < numAtlases; i++)
			{
				MeshBakerMaterialTexture meshBakerMaterialTexture = distinctMaterialTextures[0].ts[i];
				atlases[i] = meshBakerMaterialTexture.t;
				resultMaterial.SetTexture(texPropertyNames[i], atlases[i]);
				resultMaterial.SetTextureScale(texPropertyNames[i], meshBakerMaterialTexture.scale);
				resultMaterial.SetTextureOffset(texPropertyNames[i], meshBakerMaterialTexture.offset);
			}
		}
		else
		{
			List<Vector2> list = new List<Vector2>();
			for (int j = 0; j < distinctMaterialTextures.Count; j++)
			{
				list.Add(new Vector2(distinctMaterialTextures[j].idealWidth, distinctMaterialTextures[j].idealHeight));
			}
			MB2_TexturePacker mB2_TexturePacker = new MB2_TexturePacker();
			int outW = 1;
			int outH = 1;
			int maxDimension = 4096;
			if (textureEditorMethods != null)
			{
				maxDimension = textureEditorMethods.GetMaximumAtlasDimension();
			}
			array = mB2_TexturePacker.GetRects(list, maxDimension, _padding, out outW, out outH);
			if (LOG_LEVEL >= MB2_LogLevel.info)
			{
				Debug.Log("Generated atlas will be " + outW + "x" + outH + " (Max atlas size for platform: " + maxDimension + ")");
			}
			for (int k = 0; k < numAtlases; k++)
			{
				GC.Collect();
				progressInfo?.Invoke("Creating Atlas '" + texPropertyNames[k] + "'", 0.01f);
				Color[] array2 = new Color[outW * outH];
				for (int l = 0; l < array2.Length; l++)
				{
					ref Color reference = ref array2[l];
					reference = Color.clear;
				}
				for (int m = 0; m < distinctMaterialTextures.Count; m++)
				{
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						MB2_Log.Trace("Adding texture {0} to atlas {1}", (!(distinctMaterialTextures[m].ts[k].t == null)) ? distinctMaterialTextures[m].ts[k].t.ToString() : "null", texPropertyNames[k]);
					}
					Rect rect = array[m];
					Texture2D t = distinctMaterialTextures[m].ts[k].t;
					int targX = Mathf.RoundToInt(rect.x * (float)outW);
					int targY = Mathf.RoundToInt(rect.y * (float)outH);
					int num = Mathf.RoundToInt(rect.width * (float)outW);
					int num2 = Mathf.RoundToInt(rect.height * (float)outH);
					if (num == 0 || num2 == 0)
					{
						Debug.LogError("Image in atlas has no height or width");
					}
					textureEditorMethods?.SetReadWriteFlag(t, isReadable: true, addToList: true);
					progressInfo?.Invoke(string.Concat("Copying to atlas: '", distinctMaterialTextures[m].ts[k].t, "'"), 0.02f);
					CopyScaledAndTiledToAtlas(distinctMaterialTextures[m].ts[k], targX, targY, num, num2, fixOutOfBoundsUVs, maxTilingBakeSize, array2, outW, progressInfo);
				}
				progressInfo?.Invoke("Applying changes to atlas: '" + texPropertyNames[k] + "'", 0.03f);
				Texture2D texture2D = new Texture2D(outW, outH, TextureFormat.ARGB32, mipmap: true);
				texture2D.SetPixels(array2);
				texture2D.Apply();
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Saving atlas " + texPropertyNames[k] + " w=" + texture2D.width + " h=" + texture2D.height);
				}
				atlases[k] = texture2D;
				progressInfo?.Invoke("Saving atlas: '" + texPropertyNames[k] + "'", 0.04f);
				if (saveAtlasesAsAssets && textureEditorMethods != null)
				{
					textureEditorMethods.SaveAtlasToAssetDatabase(atlases[k], texPropertyNames[k], k, resultMaterial);
				}
				else
				{
					resultMaterial.SetTexture(texPropertyNames[k], atlases[k]);
				}
				resultMaterial.SetTextureOffset(texPropertyNames[k], Vector2.zero);
				resultMaterial.SetTextureScale(texPropertyNames[k], Vector2.one);
				_destroyTemporaryTextures();
			}
		}
		return array;
	}

	private Rect[] __CreateAtlasesUnityTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<string> texPropertyNames, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding)
	{
		Rect[] array;
		if (distinctMaterialTextures.Count == 1)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Only one image per atlas. Will re-use original texture");
			}
			array = new Rect[1]
			{
				new Rect(0f, 0f, 1f, 1f)
			};
			for (int i = 0; i < numAtlases; i++)
			{
				MeshBakerMaterialTexture meshBakerMaterialTexture = distinctMaterialTextures[0].ts[i];
				atlases[i] = meshBakerMaterialTexture.t;
				resultMaterial.SetTexture(texPropertyNames[i], atlases[i]);
				resultMaterial.SetTextureScale(texPropertyNames[i], meshBakerMaterialTexture.scale);
				resultMaterial.SetTextureOffset(texPropertyNames[i], meshBakerMaterialTexture.offset);
			}
		}
		else
		{
			long num = 0L;
			int w = 1;
			int h = 1;
			array = null;
			for (int j = 0; j < numAtlases; j++)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.LogWarning("Beginning loop " + j + " num temporary textures " + _temporaryTextures.Count);
				}
				for (int k = 0; k < distinctMaterialTextures.Count; k++)
				{
					MB_TexSet mB_TexSet = distinctMaterialTextures[k];
					int idealWidth = mB_TexSet.idealWidth;
					int idealHeight = mB_TexSet.idealHeight;
					Texture2D texture2D = mB_TexSet.ts[j].t;
					if (texture2D == null)
					{
						texture2D = (mB_TexSet.ts[j].t = _createTemporaryTexture(idealWidth, idealHeight, TextureFormat.ARGB32, mipMaps: true));
					}
					progressInfo?.Invoke("Adjusting for scale and offset " + texture2D, 0.01f);
					textureEditorMethods?.SetReadWriteFlag(texture2D, isReadable: true, addToList: true);
					texture2D = getAdjustedForScaleAndOffset2(mB_TexSet.ts[j]);
					if (texture2D.width != idealWidth || texture2D.height != idealHeight)
					{
						progressInfo?.Invoke(string.Concat("Resizing texture '", texture2D, "'"), 0.01f);
						if (LOG_LEVEL >= MB2_LogLevel.debug)
						{
							Debug.LogWarning("Copying and resizing texture " + texPropertyNames[j] + " from " + texture2D.width + "x" + texture2D.height + " to " + idealWidth + "x" + idealHeight);
						}
						textureEditorMethods?.SetReadWriteFlag(texture2D, isReadable: true, addToList: true);
						texture2D = _resizeTexture(texture2D, idealWidth, idealHeight);
					}
					mB_TexSet.ts[j].t = texture2D;
				}
				Texture2D[] array2 = new Texture2D[distinctMaterialTextures.Count];
				for (int l = 0; l < distinctMaterialTextures.Count; l++)
				{
					Texture2D t = distinctMaterialTextures[l].ts[j].t;
					num += t.width * t.height;
					array2[l] = t;
				}
				textureEditorMethods?.CheckBuildSettings(num);
				if (Math.Sqrt(num) > 3500.0 && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("The maximum possible atlas size is 4096. Textures may be shrunk");
				}
				atlases[j] = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: true);
				progressInfo?.Invoke("Packing texture atlas " + texPropertyNames[j], 0.25f);
				if (j == 0)
				{
					progressInfo?.Invoke("Estimated min size of atlases: " + Math.Sqrt(num).ToString("F0"), 0.1f);
					if (LOG_LEVEL >= MB2_LogLevel.info)
					{
						Debug.Log("Estimated atlas minimum size:" + Math.Sqrt(num).ToString("F0"));
					}
					_addWatermark(array2);
					if (distinctMaterialTextures.Count == 1)
					{
						array = new Rect[1]
						{
							new Rect(0f, 0f, 1f, 1f)
						};
						atlases[j] = _copyTexturesIntoAtlas(array2, _padding, array, array2[0].width, array2[0].height);
					}
					else
					{
						int maximumAtlasSize = 4096;
						array = atlases[j].PackTextures(array2, _padding, maximumAtlasSize, makeNoLongerReadable: false);
					}
					if (LOG_LEVEL >= MB2_LogLevel.info)
					{
						Debug.Log("After pack textures atlas size " + atlases[j].width + " " + atlases[j].height);
					}
					w = atlases[j].width;
					h = atlases[j].height;
					atlases[j].Apply();
				}
				else
				{
					progressInfo?.Invoke("Copying Textures Into: " + texPropertyNames[j], 0.1f);
					atlases[j] = _copyTexturesIntoAtlas(array2, _padding, array, w, h);
				}
				if (saveAtlasesAsAssets && textureEditorMethods != null)
				{
					textureEditorMethods.SaveAtlasToAssetDatabase(atlases[j], texPropertyNames[j], j, resultMaterial);
				}
				else
				{
					resultMaterial.SetTexture(texPropertyNames[j], atlases[j]);
				}
				resultMaterial.SetTextureOffset(texPropertyNames[j], Vector2.zero);
				resultMaterial.SetTextureScale(texPropertyNames[j], Vector2.one);
				_destroyTemporaryTextures();
				GC.Collect();
			}
		}
		return array;
	}

	private void _addWatermark(Texture2D[] texToPack)
	{
	}

	private Texture2D _addWatermark(Texture2D texToPack)
	{
		return texToPack;
	}

	private Texture2D _copyTexturesIntoAtlas(Texture2D[] texToPack, int padding, Rect[] rs, int w, int h)
	{
		Texture2D texture2D = new Texture2D(w, h, TextureFormat.ARGB32, mipmap: true);
		MB_Utility.setSolidColor(texture2D, Color.clear);
		for (int i = 0; i < rs.Length; i++)
		{
			Rect rect = rs[i];
			Texture2D texture2D2 = texToPack[i];
			int x = Mathf.RoundToInt(rect.x * (float)w);
			int y = Mathf.RoundToInt(rect.y * (float)h);
			int num = Mathf.RoundToInt(rect.width * (float)w);
			int num2 = Mathf.RoundToInt(rect.height * (float)h);
			if (texture2D2.width != num && texture2D2.height != num2)
			{
				texture2D2 = MB_Utility.resampleTexture(texture2D2, num, num2);
				_temporaryTextures.Add(texture2D2);
			}
			texture2D.SetPixels(x, y, num, num2, texture2D2.GetPixels());
		}
		texture2D.Apply();
		return texture2D;
	}

	private bool IsPowerOfTwo(int x)
	{
		return (x & (x - 1)) == 0;
	}

	private Vector2 getAdjustedForScaleAndOffset2Dimensions(MeshBakerMaterialTexture source)
	{
		if (source.offset.x == 0f && source.offset.y == 0f && source.scale.x == 1f && source.scale.y == 1f)
		{
			if (!fixOutOfBoundsUVs)
			{
				return new Vector2(source.t.width, source.t.height);
			}
			if (source.obUVoffset.x == 0f && source.obUVoffset.y == 0f && source.obUVscale.x == 1f && source.obUVscale.y == 1f)
			{
				return new Vector2(source.t.width, source.t.height);
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.LogWarning(string.Concat("getAdjustedForScaleAndOffset2Dimensions: ", source.t, " ", source.obUVoffset, " ", source.obUVscale));
		}
		float num = (float)source.t.width * source.scale.x;
		float num2 = (float)source.t.height * source.scale.y;
		if (fixOutOfBoundsUVs)
		{
			num *= source.obUVscale.x;
			num2 *= source.obUVscale.y;
		}
		if (num > (float)maxTilingBakeSize)
		{
			num = maxTilingBakeSize;
		}
		if (num2 > (float)maxTilingBakeSize)
		{
			num2 = maxTilingBakeSize;
		}
		if (num < 1f)
		{
			num = 1f;
		}
		if (num2 < 1f)
		{
			num2 = 1f;
		}
		return new Vector2(num, num2);
	}

	public Texture2D getAdjustedForScaleAndOffset2(MeshBakerMaterialTexture source)
	{
		if (source.offset.x == 0f && source.offset.y == 0f && source.scale.x == 1f && source.scale.y == 1f)
		{
			if (!fixOutOfBoundsUVs)
			{
				return source.t;
			}
			if (source.obUVoffset.x == 0f && source.obUVoffset.y == 0f && source.obUVscale.x == 1f && source.obUVscale.y == 1f)
			{
				return source.t;
			}
		}
		Vector2 adjustedForScaleAndOffset2Dimensions = getAdjustedForScaleAndOffset2Dimensions(source);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.LogWarning(string.Concat("getAdjustedForScaleAndOffset2: ", source.t, " ", source.obUVoffset, " ", source.obUVscale));
		}
		float x = adjustedForScaleAndOffset2Dimensions.x;
		float y = adjustedForScaleAndOffset2Dimensions.y;
		float num = source.scale.x;
		float num2 = source.scale.y;
		float num3 = source.offset.x;
		float num4 = source.offset.y;
		if (fixOutOfBoundsUVs)
		{
			num *= source.obUVscale.x;
			num2 *= source.obUVscale.y;
			num3 += source.obUVoffset.x;
			num4 += source.obUVoffset.y;
		}
		Texture2D texture2D = _createTemporaryTexture((int)x, (int)y, TextureFormat.ARGB32, mipMaps: true);
		for (int i = 0; i < texture2D.width; i++)
		{
			for (int j = 0; j < texture2D.height; j++)
			{
				float u = (float)i / x * num + num3;
				float v = (float)j / y * num2 + num4;
				texture2D.SetPixel(i, j, source.t.GetPixelBilinear(u, v));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public void CopyScaledAndTiledToAtlas(MeshBakerMaterialTexture source, int targX, int targY, int targW, int targH, bool fixOutOfBoundsUVs, int maxSize, Color[] atlasPixels, int atlasWidth, ProgressUpdateDelegate progressInfo = null)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log(string.Concat("CopyScaledAndTiledToAtlas: ", source.t, " inAtlasX=", targX, " inAtlasY=", targY, " inAtlasW=", targW, " inAtlasH=", targH));
		}
		float num = targW;
		float num2 = targH;
		float num3 = source.scale.x;
		float num4 = source.scale.y;
		float num5 = source.offset.x;
		float num6 = source.offset.y;
		if (fixOutOfBoundsUVs)
		{
			num3 *= source.obUVscale.x;
			num4 *= source.obUVscale.y;
			num5 += source.obUVoffset.x;
			num6 += source.obUVoffset.y;
		}
		int num7 = (int)num;
		int num8 = (int)num2;
		Texture2D texture2D = source.t;
		if (texture2D == null)
		{
			texture2D = _createTemporaryTexture(2, 2, TextureFormat.ARGB32, mipMaps: true);
			MB_Utility.setSolidColor(texture2D, Color.clear);
		}
		texture2D = _addWatermark(texture2D);
		for (int i = 0; i < num7; i++)
		{
			if (progressInfo != null && num7 > 0)
			{
				progressInfo("CopyScaledAndTiledToAtlas " + ((float)i / (float)num7 * 100f).ToString("F0"), 0.2f);
			}
			for (int j = 0; j < num8; j++)
			{
				float u = (float)i / num * num3 + num5;
				float v = (float)j / num2 * num4 + num6;
				ref Color reference = ref atlasPixels[(targY + j) * atlasWidth + targX + i];
				reference = texture2D.GetPixelBilinear(u, v);
			}
		}
	}

	private Texture2D _createTemporaryTexture(int w, int h, TextureFormat texFormat, bool mipMaps)
	{
		Texture2D texture2D = new Texture2D(w, h, texFormat, mipMaps);
		MB_Utility.setSolidColor(texture2D, Color.clear);
		_temporaryTextures.Add(texture2D);
		return texture2D;
	}

	private Texture2D _createTextureCopy(Texture2D t)
	{
		Texture2D texture2D = MB_Utility.createTextureCopy(t);
		_temporaryTextures.Add(texture2D);
		return texture2D;
	}

	private Texture2D _resizeTexture(Texture2D t, int w, int h)
	{
		Texture2D texture2D = MB_Utility.resampleTexture(t, w, h);
		_temporaryTextures.Add(texture2D);
		return texture2D;
	}

	private void _destroyTemporaryTextures()
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Destroying " + _temporaryTextures.Count + " temporary textures");
		}
		for (int i = 0; i < _temporaryTextures.Count; i++)
		{
			MB_Utility.Destroy(_temporaryTextures[i]);
		}
		_temporaryTextures.Clear();
	}

	public void SuggestTreatment(List<GameObject> objsToMesh, Material[] resultMaterials, List<string> customShaderPropNames)
	{
		this.customShaderPropNames = customShaderPropNames;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < objsToMesh.Count; i++)
		{
			GameObject gameObject = objsToMesh[i];
			if (gameObject == null)
			{
				continue;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
			if (gOMaterials.Length > 1)
			{
				stringBuilder.AppendFormat("\nObject {0} uses {1} materials. Possible treatments:\n", objsToMesh[i].name, gOMaterials.Length);
				stringBuilder.AppendFormat("  1) Collapse the submeshes together into one submesh in the combined mesh. Each of the original submesh materials will map to a different UV rectangle in the atlas(es) used by the combined material.\n");
				stringBuilder.AppendFormat("  2) Use the multiple materials feature to map submeshes in the source mesh to submeshes in the combined mesh.\n");
			}
			Mesh mesh = MB_Utility.GetMesh(gameObject);
			Rect uvBounds = default(Rect);
			for (int j = 0; j < gOMaterials.Length; j++)
			{
				if (MB_Utility.hasOutOfBoundsUVs(mesh, ref uvBounds, j))
				{
					stringBuilder.AppendFormat("\nObject {0} submesh={1} material={2} uses UVs outside the range 0,0 .. 1,1 to create tiling that tiles the box {3},{4} .. {5},{6}. This is a problem because the UVs outside the 0,0 .. 1,1 rectangle will pick up neighboring textures in the atlas. Possible Treatments:\n", gameObject, j, gOMaterials[j], uvBounds.x.ToString("G4"), uvBounds.y.ToString("G4"), (uvBounds.x + uvBounds.width).ToString("G4"), (uvBounds.y + uvBounds.height).ToString("G4"));
					stringBuilder.AppendFormat("    1) Ignore the problem. The tiling may not affect result significantly.\n");
					stringBuilder.AppendFormat("    2) Use the 'fix out of bounds UVs' feature to bake the tiling and scale the UVs to fit in the 0,0 .. 1,1 rectangle.\n");
					stringBuilder.AppendFormat("    3) Use the Multiple Materials feature to map the material on this submesh to its own submesh in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
					stringBuilder.AppendFormat("    4) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
				}
			}
			if (MB_Utility.doSubmeshesShareVertsOrTris(mesh) != 0)
			{
				stringBuilder.AppendFormat("\nObject {0} has submeshes that share vertices. This is a problem because each vertex can have only one UV coordinate and may be required to map to different positions in the various atlases that are generated. Possible treatments:\n", objsToMesh[i]);
				stringBuilder.AppendFormat(" 1) Ignore the problem. The vertices may not affect the result.\n");
				stringBuilder.AppendFormat(" 2) Use the Multiple Materials feature to map the submeshs that overlap to their own submeshs in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
				stringBuilder.AppendFormat(" 3) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
			}
		}
		Dictionary<Material, List<GameObject>> dictionary = new Dictionary<Material, List<GameObject>>();
		for (int k = 0; k < objsToMesh.Count; k++)
		{
			if (!(objsToMesh[k] != null))
			{
				continue;
			}
			Material[] gOMaterials2 = MB_Utility.GetGOMaterials(objsToMesh[k]);
			for (int l = 0; l < gOMaterials2.Length; l++)
			{
				if (gOMaterials2[l] != null)
				{
					if (!dictionary.TryGetValue(gOMaterials2[l], out var value))
					{
						value = new List<GameObject>();
						dictionary.Add(gOMaterials2[l], value);
					}
					if (!value.Contains(objsToMesh[k]))
					{
						value.Add(objsToMesh[k]);
					}
				}
			}
		}
		List<string> list = new List<string>();
		for (int m = 0; m < resultMaterials.Length; m++)
		{
			_collectPropertyNames(resultMaterials[m], list);
			foreach (Material key in dictionary.Keys)
			{
				for (int n = 0; n < list.Count; n++)
				{
					if (!key.HasProperty(list[n]))
					{
						continue;
					}
					Texture texture = key.GetTexture(list[n]);
					if (texture != null)
					{
						Vector2 textureOffset = key.GetTextureOffset(list[n]);
						Vector3 vector = key.GetTextureScale(list[n]);
						if (textureOffset.x < 0f || textureOffset.x + vector.x > 1f || textureOffset.y < 0f || textureOffset.y + vector.y > 1f)
						{
							stringBuilder.AppendFormat("\nMaterial {0} used by objects {1} uses texture {2} that is tiled (scale={3} offset={4}). If there is more than one texture in the atlas  then Mesh Baker will bake the tiling into the atlas. If the baked tiling is large then quality can be lost. Possible treatments:\n", key, PrintList(dictionary[key]), texture, vector, textureOffset);
							stringBuilder.AppendFormat("  1) Use the baked tiling.\n");
							stringBuilder.AppendFormat("  2) Use the Multiple Materials feature to map the material on this object/submesh to its own submesh in the combined mesh. No other materials should map to this submesh. The original material can be applied to this submesh.\n");
							stringBuilder.AppendFormat("  3) Combine only meshes that use the same (or subset of) the set of textures on this mesh. The original material can be applied to the result.\n");
						}
					}
				}
			}
		}
		_ = string.Empty;
		Debug.Log((stringBuilder.Length != 0) ? ("====== There are possible problems with these meshes that may prevent them from combining well. TREATMENT SUGGESTIONS (copy and paste to text editor if too big) =====\n" + stringBuilder.ToString()) : "====== No problems detected. These meshes should combine well ====\n  If there are problems with the combined meshes please report the problem to digitalOpus.ca so we can improve Mesh Baker.");
	}

	private string PrintList(List<GameObject> gos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < gos.Count; i++)
		{
			stringBuilder.Append(string.Concat(gos[i], ","));
		}
		return stringBuilder.ToString();
	}
}
