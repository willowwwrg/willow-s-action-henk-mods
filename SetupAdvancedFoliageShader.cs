using System;
using UnityEngine;

[AddComponentMenu("AFS/Setup Advanced Foliage Shader")]
[ExecuteInEditMode]
public class SetupAdvancedFoliageShader : MonoBehaviour
{
	public bool isLinear;

	public bool useIBL;

	public bool controlIBL;

	public float AFS_HDR_Scale = 6f;

	public float AFS_IBL_MasterExposure = 1f;

	public float AFS_IBL_DiffuseExposure = 1f;

	private float DiffuseExposure;

	public float AFS_IBL_SpecularExposure = 1f;

	private float SpecularExposure;

	private Color Unity_Ambient_Lighting;

	public Cubemap diffuseCube;

	public bool diffuseIsHDR;

	public Cubemap specularCube;

	public bool specularIsHDR;

	private Cubemap PlaceHolderCube;

	public float AFS_CameraExposure = 1f;

	public float CameraExposure;

	public Vector4 Wind = new Vector4(0.85f, 0.075f, 0.4f, 0.5f);

	public float WindFrequency = 0.75f;

	public float WaveSizeFoliageShader = 10f;

	public float WindMultiplierForGrassshader = 4f;

	public float WaveSizeForGrassshader = 10f;

	public float RainAmount;

	public float VertexLitAlphaCutOff = 0.3f;

	public Color VertexLitTranslucencyColor = new Color(0.73f, 0.85f, 0.4f, 1f);

	public float VertexLitTranslucencyViewDependency = 0.7f;

	public float VertexLitShadowStrength = 0.8f;

	public float VertexLitShininess = 0.2f;

	public Vector2 AfsSpecFade = new Vector2(60f, 10f);

	public Texture TerrainFoliageNrmSpecMap;

	public bool AutoSyncToTerrain;

	public Terrain SyncedTerrain;

	public bool AutoSyncInPlaymode;

	public float DetailDistanceForGrassShader = 80f;

	public float BillboardStart = 50f;

	public float BillboardFadeLenght = 5f;

	public bool GrassAnimateNormal;

	public Color GrassWavingTint;

	public bool UseLinearLightingFixTrees = true;

	public bool TreeShadowDissolve;

	public bool TreeBillboardShadows;

	public int TreeBillboardLOD = 200;

	public bool TreeShadowEdgeFade;

	public bool BillboardShadowEdgeFade;

	public float BillboardShadowEdgeFadeThreshold = 0.1f;

	public float BillboardFadeOutLength = 60f;

	public bool BillboardAdjustToCamera = true;

	public float BillboardAngleLimit = 30f;

	public Shader AfsTreeBillboardShadowShader;

	public GameObject BillboardLightReference;

	public Color BillboardShadowColor;

	public float BillboardAmbientLightFactor = 1f;

	public float BillboardAmbientLightDesaturationFactor = 0.7f;

	public bool AutosyncShadowColor;

	public bool EnableCameraLayerCulling = true;

	public int SmallDetailsDistance = 70;

	public int MediumDetailsDistance = 90;

	public bool AllGrassObjectsCombined;

	private Vector4 TempWind;

	private float TempWindForce;

	private float GrassWind;

	private Vector3 CameraForward = new Vector3(0f, 0f, 0f);

	private Vector3 ShadowCameraForward = new Vector3(0f, 0f, 0f);

	private Vector3 CameraForwardVec;

	private float rollingX;

	private float rollingXShadow;

	private Vector3 lightDir;

	private Vector3 templightDir;

	private float CameraAngle;

	private Terrain[] allTerrains;

	private float grey;

	private void Awake()
	{
		AfsTreeBillboardShadowShader = Shader.Find("Hidden/TerrainEngine/BillboardTree");
		afsCheckColorSpace();
		afsSetupColorSpace();
		afsLightingSettings();
		afsSetupTerrainEngine();
		afsSetupGrassShader();
		afsUpdateWind();
		afsUpdateRain();
		afsAutoSyncToTerrain();
		afsUpdateTreeAndBillboardShaders();
		afsUpdateGrassTreesBillboards();
		afsSetupCameraLayerCulling();
	}

	private void Start()
	{
		AfsTreeBillboardShadowShader = Shader.Find("Hidden/TerrainEngine/BillboardTree");
		afsSetupColorSpace();
	}

	public void Update()
	{
		afsUpdateGrassTreesBillboards();
		afsSetupColorSpace();
		afsLightingSettings();
		afsUpdateWind();
		afsUpdateRain();
		afsAutoSyncToTerrain();
	}

	private void afsCheckColorSpace()
	{
	}

	private void afsSetupColorSpace()
	{
		if (isLinear)
		{
			Shader.EnableKeyword("LUX_LINEAR");
			Shader.DisableKeyword("LUX_GAMMA");
			Shader.EnableKeyword("MARMO_LINEAR");
			Shader.DisableKeyword("MARMO_GAMMA");
		}
		else
		{
			Shader.EnableKeyword("LUX_GAMMA");
			Shader.DisableKeyword("LUX_LINEAR");
			Shader.EnableKeyword("MARMO_GAMMA");
			Shader.DisableKeyword("MARMO_LINEAR");
		}
	}

	private void afsLightingSettings()
	{
		if (UseLinearLightingFixTrees)
		{
			Shader.EnableKeyword("AFS_LLFIX_MESHTREES_ON");
			Shader.EnableKeyword("AFS_LLFIX_BILLBOARDS_ON");
			Shader.DisableKeyword("AFS_LLFIX_MESHTREES_OFF");
			Shader.DisableKeyword("AFS_LLFIX_BILLBOARDS_OFF");
		}
		else
		{
			Shader.EnableKeyword("AFS_LLFIX_MESHTREES_OFF");
			Shader.EnableKeyword("AFS_LLFIX_BILLBOARDS_OFF");
			Shader.DisableKeyword("AFS_LLFIX_MESHTREES_ON");
			Shader.DisableKeyword("AFS_LLFIX_BILLBOARDS_ON");
		}
		if (useIBL)
		{
			Shader.DisableKeyword("AFS_SH");
			Shader.EnableKeyword("AFS_IBL");
		}
		else
		{
			Shader.EnableKeyword("AFS_SH");
			Shader.DisableKeyword("AFS_IBL");
			if (isLinear)
			{
				Shader.SetGlobalColor("_AfsAmbientColor", RenderSettings.ambientLight.linear);
			}
			else
			{
				Shader.SetGlobalColor("_AfsAmbientColor", RenderSettings.ambientLight);
			}
		}
		if (!controlIBL)
		{
			return;
		}
		if (isLinear)
		{
			DiffuseExposure = AFS_IBL_DiffuseExposure;
			if (diffuseIsHDR)
			{
				DiffuseExposure *= Mathf.Pow(AFS_HDR_Scale, 2.2333333f) * AFS_IBL_MasterExposure;
			}
			SpecularExposure = AFS_IBL_SpecularExposure;
			if (specularIsHDR)
			{
				SpecularExposure *= Mathf.Pow(AFS_HDR_Scale, 2.2333333f) * AFS_IBL_MasterExposure;
			}
			CameraExposure = AFS_CameraExposure;
		}
		else
		{
			DiffuseExposure = Mathf.Pow(AFS_IBL_DiffuseExposure * AFS_IBL_MasterExposure, 0.44776118f);
			if (diffuseIsHDR)
			{
				DiffuseExposure *= AFS_HDR_Scale;
			}
			SpecularExposure = Mathf.Pow(AFS_IBL_SpecularExposure * AFS_IBL_MasterExposure, 0.44776118f);
			if (specularIsHDR)
			{
				SpecularExposure *= AFS_HDR_Scale;
			}
			CameraExposure = Mathf.Pow(AFS_CameraExposure, 0.44776118f);
		}
		Shader.SetGlobalVector("ExposureIBL", new Vector4(DiffuseExposure, SpecularExposure, 1f, CameraExposure));
		if ((bool)diffuseCube)
		{
			Shader.SetGlobalTexture("_DiffCubeIBL", diffuseCube);
		}
		if ((bool)specularCube)
		{
			Shader.SetGlobalTexture("_SpecCubeIBL", specularCube);
			return;
		}
		createPlaceHolderCube();
		Shader.SetGlobalTexture("_SpecCubeIBL", PlaceHolderCube);
	}

	private void createPlaceHolderCube()
	{
		if (!(PlaceHolderCube == null))
		{
			return;
		}
		PlaceHolderCube = new Cubemap(16, TextureFormat.ARGB32, mipmap: true);
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					PlaceHolderCube.SetPixel((CubemapFace)i, j, k, Color.black);
				}
			}
		}
		PlaceHolderCube.Apply(updateMipmaps: true);
	}

	private void afsSetupGrassShader()
	{
		if (Application.isPlaying || AllGrassObjectsCombined)
		{
			Shader.DisableKeyword("IN_EDITMODE");
			Shader.EnableKeyword("IN_PLAYMODE");
		}
		else
		{
			Shader.DisableKeyword("IN_PLAYMODE");
			Shader.EnableKeyword("IN_EDITMODE");
		}
	}

	private void afsSetupTerrainEngine()
	{
		Shader.SetGlobalFloat("_AfsAlphaCutOff", VertexLitAlphaCutOff);
		Shader.SetGlobalColor("_AfsTranslucencyColor", VertexLitTranslucencyColor);
		Shader.SetGlobalFloat("_AfsTranslucencyViewDependency", VertexLitTranslucencyViewDependency);
		Shader.SetGlobalFloat("_AfsShadowStrength", VertexLitShadowStrength);
		Shader.SetGlobalFloat("_AfsShininess", VertexLitShininess);
		Shader.SetGlobalColor("_TranslucencyColor", VertexLitTranslucencyColor);
		Shader.SetGlobalFloat("_TranslucencyViewDependency", VertexLitTranslucencyViewDependency);
		Shader.SetGlobalFloat("_ShadowStrength", VertexLitShadowStrength);
		if (TerrainFoliageNrmSpecMap != null)
		{
			Shader.SetGlobalTexture("_TerrianBumpTransSpecMap", TerrainFoliageNrmSpecMap);
		}
		Shader.SetGlobalVector("_AfsSpecFade", new Vector4(AfsSpecFade.x, AfsSpecFade.y, 1f, 1f));
	}

	private void afsUpdateWind()
	{
		TempWind = Wind;
		TempWindForce = Wind.w;
		TempWind.x *= (1.25f + Mathf.Sin(Time.time * WindFrequency) * Mathf.Sin(Time.time * 0.375f)) * 0.5f;
		TempWind.z *= (1.25f + Mathf.Sin(Time.time * WindFrequency) * Mathf.Sin(Time.time * 0.193f)) * 0.5f;
		TempWind.w = TempWindForce;
		Shader.SetGlobalVector("_Wind", TempWind);
		GrassWind = (TempWind.x + TempWind.z) / 2f * Wind.w;
		Shader.SetGlobalFloat("_AfsWaveSize", 0.5f / WaveSizeFoliageShader);
		Shader.SetGlobalVector("_AfsWaveAndDistance", new Vector4(Time.time * (WindFrequency * 0.05f), 0.5f / WaveSizeForGrassshader, GrassWind * WindMultiplierForGrassshader, DetailDistanceForGrassShader * DetailDistanceForGrassShader));
	}

	private void afsUpdateRain()
	{
		Shader.SetGlobalFloat("_AfsRainamount", RainAmount);
	}

	private void afsAutoSyncToTerrain()
	{
		if (AutoSyncToTerrain && SyncedTerrain != null)
		{
			DetailDistanceForGrassShader = SyncedTerrain.detailObjectDistance;
			BillboardStart = SyncedTerrain.treeBillboardDistance;
			if (!TreeBillboardShadows)
			{
				BillboardFadeLenght = SyncedTerrain.treeCrossFadeLength;
			}
			GrassWavingTint = SyncedTerrain.terrainData.wavingGrassTint;
		}
	}

	private void afsUpdateTreeAndBillboardShaders()
	{
		if (TreeShadowDissolve)
		{
			Shader.EnableKeyword("TREE_SHADOW_DISSOLVE");
			Shader.DisableKeyword("TREE_SHADOW_NO_DISSOLVE");
		}
		else
		{
			Shader.EnableKeyword("TREE_SHADOW_NO_DISSOLVE");
			Shader.DisableKeyword("TREE_SHADOW_DISSOLVE");
		}
		if (TreeBillboardShadows)
		{
			Shader.EnableKeyword("BILLBOARD_SHADOWS");
			Shader.DisableKeyword("BILLBOARD_NO_SHADOWS");
		}
		else
		{
			Shader.EnableKeyword("BILLBOARD_NO_SHADOWS");
			Shader.DisableKeyword("BILLBOARD_SHADOWS");
		}
		if (TreeShadowEdgeFade)
		{
			Shader.EnableKeyword("TREESHADOW_EDGEFADE");
			Shader.DisableKeyword("TREESHADOW_NO_EDGEFADE");
		}
		else
		{
			Shader.EnableKeyword("TREESHADOW_NO_EDGEFADE");
			Shader.DisableKeyword("TREESHADOW_EDGEFADE");
		}
		if (BillboardShadowEdgeFade)
		{
			Shader.EnableKeyword("BILLBOARDSHADOW_EDGEFADE");
			Shader.DisableKeyword("BILLBOARDSHADOW_NO_EDGEFADE");
		}
		else
		{
			Shader.EnableKeyword("BILLBOARDSHADOW_NO_EDGEFADE");
			Shader.DisableKeyword("BILLBOARDSHADOW_EDGEFADE");
		}
		if (GrassAnimateNormal)
		{
			Shader.EnableKeyword("GRASS_ANIMATE_NORMAL");
			Shader.DisableKeyword("GRASS_ANIMATE_COLOR");
		}
		else
		{
			Shader.EnableKeyword("GRASS_ANIMATE_COLOR");
			Shader.DisableKeyword("GRASS_ANIMATE_NORMAL");
		}
		if (TreeBillboardShadows && TreeBillboardLOD != 300 && AfsTreeBillboardShadowShader != null)
		{
			AfsTreeBillboardShadowShader.maximumLOD = 300;
			TreeBillboardLOD = 300;
		}
		if (!TreeBillboardShadows && TreeBillboardLOD != 200 && AfsTreeBillboardShadowShader != null)
		{
			AfsTreeBillboardShadowShader.maximumLOD = 200;
			TreeBillboardLOD = 200;
		}
		Shader.SetGlobalFloat("_AfsBillboardFog", 1f);
	}

	private void afsUpdateGrassTreesBillboards()
	{
		Shader.SetGlobalColor("_AfsWavingTint", GrassWavingTint);
		Shader.SetGlobalVector("_AfsTerrainTrees", new Vector4(BillboardStart, BillboardFadeLenght, BillboardFadeOutLength, 0f));
		if (BillboardAdjustToCamera)
		{
			if ((bool)Camera.main)
			{
				CameraForward = Camera.main.transform.forward;
				ShadowCameraForward = CameraForward;
				rollingX = Camera.main.transform.eulerAngles.x;
			}
			else
			{
				Debug.Log("You have to tag your Camera as MainCamera");
			}
			if (rollingX >= 270f)
			{
				rollingX -= 270f;
				rollingX = 90f - rollingX;
				rollingXShadow = rollingX;
			}
			else
			{
				rollingXShadow = 0f - rollingX;
				if (rollingX > BillboardAngleLimit)
				{
					rollingX = Mathf.Lerp(rollingX, 0f, rollingX / BillboardAngleLimit - 1f);
				}
				rollingX *= -1f;
			}
		}
		else
		{
			rollingX = 0f;
			rollingXShadow = 0f;
		}
		CameraForward *= rollingX / 90f;
		ShadowCameraForward *= rollingXShadow / 90f;
		Shader.SetGlobalVector("_AfsBillboardCameraForward", new Vector4(CameraForward.x, CameraForward.y, CameraForward.z, 1f));
		Shader.SetGlobalVector("_AfsBillboardShadowCameraForward", new Vector4(ShadowCameraForward.x, ShadowCameraForward.y, ShadowCameraForward.z, 1f));
		if (TreeBillboardShadows)
		{
			if (BillboardLightReference != null)
			{
				lightDir = BillboardLightReference.transform.forward;
				templightDir = lightDir;
				lightDir = Vector3.Cross(lightDir, Vector3.up);
				if (Vector3.Dot(templightDir, Camera.main.transform.forward) > 0f)
				{
					lightDir = Quaternion.AngleAxis(180f, Vector3.up) * lightDir;
				}
				Shader.SetGlobalVector("_AfsSunDirection", new Vector4(lightDir.x, lightDir.y, lightDir.z, 1f));
				CameraForwardVec = Camera.main.transform.forward;
				allTerrains = UnityEngine.Object.FindObjectsOfType(typeof(Terrain)) as Terrain[];
				for (int i = 0; i < allTerrains.Length; i++)
				{
					allTerrains[i].treeCrossFadeLength = 0.0001f;
				}
				allTerrains = null;
				CameraAngle = Camera.main.fieldOfView;
				CameraAngle -= CameraAngle * BillboardShadowEdgeFadeThreshold;
				CameraAngle = Mathf.Cos(CameraAngle * ((float)Math.PI / 180f));
				Shader.SetGlobalVector("_CameraForwardVec", new Vector4(CameraForwardVec.x, CameraForwardVec.y, CameraForwardVec.z, CameraAngle));
			}
			else
			{
				Debug.LogWarning("You have to specify a Light Reference!");
			}
		}
		if (AutosyncShadowColor)
		{
			BillboardShadowColor = RenderSettings.ambientLight;
			BillboardShadowColor = Desaturate(BillboardShadowColor.r * BillboardAmbientLightFactor, BillboardShadowColor.g * BillboardAmbientLightFactor, BillboardShadowColor.b * BillboardAmbientLightFactor);
			if ((bool)BillboardLightReference)
			{
				BillboardShadowColor += 0.5f * (BillboardShadowColor * (1f - BillboardLightReference.light.shadowStrength));
			}
		}
		Shader.SetGlobalColor("_AfsAmbientBillboardLight", BillboardShadowColor);
	}

	private void afsSetupCameraLayerCulling()
	{
		if (EnableCameraLayerCulling)
		{
			for (int i = 0; i < Camera.allCameras.Length; i++)
			{
				float[] array = new float[32];
				array = Camera.allCameras[i].layerCullDistances;
				array[8] = SmallDetailsDistance;
				array[9] = MediumDetailsDistance;
				Camera.allCameras[i].layerCullDistances = array;
				array = null;
			}
		}
	}

	private Color Desaturate(float r, float g, float b)
	{
		grey = 0.3f * r + 0.59f * g + 0.11f * b;
		r = grey * BillboardAmbientLightDesaturationFactor + r * (1f - BillboardAmbientLightDesaturationFactor);
		g = grey * BillboardAmbientLightDesaturationFactor + g * (1f - BillboardAmbientLightDesaturationFactor);
		b = grey * BillboardAmbientLightDesaturationFactor + b * (1f - BillboardAmbientLightDesaturationFactor);
		return new Color(r, g, b, 1f);
	}
}
