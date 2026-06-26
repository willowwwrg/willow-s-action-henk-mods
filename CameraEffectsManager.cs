using System.Collections.Generic;
using UnityEngine;

public class CameraEffectsManager : MonoBehaviour
{
	public LevelStyle levelStyle = LevelStyle.KidsRoom_Day;

	public bool manualOverrideStyle;

	public GraphicsStyle[] levelStyles;

	[HideInInspector]
	public GraphicsStyle graphicsStyle;

	private float targetFogDensity;

	private bool enableSSAO;

	private bool enableDOF;

	private bool enableColorCorrections;

	private bool enableBloom;

	[HideInInspector]
	public bool enableDepthEffects;

	private float targetLightning;

	private int lightningstate;

	private GameObject lightingPlane;

	private GameObject lightingPlane2;

	private List<Material> hueCycle = new List<Material>();

	private HSBColor hueCycleColor = new HSBColor(new Color(0.3f, 0f, 0f));

	private AmplifyColorEffect colorCorrections;

	private Bloom bloomEffect;

	private GlobalFog fogEffect;

	private void Awake()
	{
		colorCorrections = GetComponent<AmplifyColorEffect>();
		bloomEffect = GetComponent<Bloom>();
		fogEffect = GetComponent<GlobalFog>();
		ReadPlayerPrefs();
	}

	private void Start()
	{
		if (!manualOverrideStyle || !Application.isEditor)
		{
			if (HenkUtils.IsInALevel())
			{
				levelStyle = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle;
			}
			else
			{
				levelStyle = LevelStyle.KidsRoom_Day;
			}
		}
		Init(levelStyle);
	}

	public void Init(LevelStyle styleArg = LevelStyle.None)
	{
		switch (styleArg)
		{
		case LevelStyle.None:
			return;
		case LevelStyle.KidsRoom_Menu:
			styleArg = LevelStyle.KidsRoom_Day;
			break;
		}
		levelStyle = styleArg;
		hueCycle.Clear();
		GraphicsStyle[] array = levelStyles;
		foreach (GraphicsStyle graphicsStyle in array)
		{
			if (graphicsStyle.style == levelStyle)
			{
				this.graphicsStyle = graphicsStyle;
				break;
			}
		}
		Object.FindObjectOfType<State_OptionsGraphics>().guiScript.RefreshQualitySettings();
		ReadPlayerPrefs();
		UpdateComponents();
		SwapShadersForAllGameplayAssets();
		GetComponent<Skybox>().material = this.graphicsStyle.skybox;
		lightingPlane = GameObject.Find("lightningplane");
		lightingPlane2 = GameObject.Find("lightningplane2");
		CheckMutators();
	}

	public void ReadPlayerPrefs()
	{
		enableSSAO = PlayerPrefs.GetInt("QualitySettings_SSAO", 0) != 0;
		enableDOF = PlayerPrefs.GetInt("QualitySettings_DOF", 0) != 0;
		enableColorCorrections = true;
		enableBloom = PlayerPrefs.GetInt("QualitySettings_BLOOM", 0) != 0;
		enableDepthEffects = PlayerPrefs.GetInt("QualitySettings_DEPTHEFFECTS", 1) != 0;
	}

	public void UpdateComponents()
	{
		if ((bool)GetComponent<SSAOEffect>())
		{
			GetComponent<SSAOEffect>().enabled = enableSSAO;
		}
		if ((bool)GetComponent<SSAOPro>())
		{
			GetComponent<SSAOPro>().enabled = enableSSAO;
		}
		bool flag = Singleton<LevelBatchManager>.SP.GetCurrentLevelCodeOrID() == 97;
		if ((bool)GetComponent<DepthOfFieldScatter>() && !flag)
		{
			GetComponent<DepthOfFieldScatter>().enabled = enableDOF;
		}
		if ((bool)colorCorrections)
		{
			colorCorrections.enabled = enableColorCorrections;
			colorCorrections.LutTexture = graphicsStyle.colorCorrectionSettings.LutTexture;
			colorCorrections.LutBlendTexture = graphicsStyle.colorCorrectionSettings.LutBlendTexture;
			colorCorrections.BlendAmount = graphicsStyle.colorCorrectionSettings.BlendAmount;
		}
		if ((bool)bloomEffect)
		{
			bloomEffect.enabled = enableBloom;
			bloomEffect.bloomIntensity = graphicsStyle.bloomSettings.intensity;
			bloomEffect.bloomThreshhold = graphicsStyle.bloomSettings.threshold;
			bloomEffect.sepBlurSpread = graphicsStyle.bloomSettings.sampleDistance;
		}
		if ((bool)fogEffect)
		{
			fogEffect.enabled = enableDepthEffects && graphicsStyle.fogSettings.enableFog;
			fogEffect.startDistance = graphicsStyle.fogSettings.startDistance;
			fogEffect.globalDensity = graphicsStyle.fogSettings.density;
			fogEffect.heightScale = graphicsStyle.fogSettings.heightScale;
			fogEffect.height = graphicsStyle.fogSettings.height;
			fogEffect.globalFogColor = graphicsStyle.fogSettings.fogColor;
		}
		CheckDepthPass();
	}

	private void CheckDepthPass()
	{
		bool num = !GetComponent<DepthOfFieldScatter>() || !GetComponent<DepthOfFieldScatter>().enabled;
		bool flag = !GetComponent<SSAOEffect>() || !GetComponent<SSAOEffect>().enabled;
		bool flag2 = !GetComponent<SSAOPro>() || !GetComponent<SSAOPro>().enabled;
		if (num && flag && flag2)
		{
			base.camera.depthTextureMode = DepthTextureMode.None;
			if (enableDepthEffects)
			{
				base.camera.depthTextureMode = DepthTextureMode.Depth;
			}
		}
	}

	public void CheckMutators()
	{
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.BlindMode && !GetComponent<CameraFilterPack_FX_Spot>())
		{
			CameraFilterPack_FX_Spot cameraFilterPack_FX_Spot = base.gameObject.AddComponent<CameraFilterPack_FX_Spot>();
			cameraFilterPack_FX_Spot.Radius = 0.03f;
			cameraFilterPack_FX_Spot.center = new Vector2(0.5f, 0.4f);
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.Trippin && !GetComponent<CameraFilterPack_FX_Drunk>())
		{
			base.gameObject.AddComponent<CameraFilterPack_FX_Drunk>();
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.Pixelated && !GetComponent<Pixelate>())
		{
			base.gameObject.AddComponent<Pixelate>();
		}
	}

	private void SwapShadersForAllGameplayAssets()
	{
		string[] array = new string[5] { "LevelBlocks", "LevelBlocks_2", "LevelSupport", "StartFinishCheckpoints", "EnvironmentStyleContainer" };
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = GameObject.Find(array[i]);
			if (gameObject != null)
			{
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					UpdateMaterialForStyle(renderer);
				}
			}
		}
	}

	public void UpdateMaterialForStyle(Renderer renderer, bool isCharacter = false)
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			bool flag = false;
			MaterialOverride[] materialOverrides = graphicsStyle.materialOverrides;
			foreach (MaterialOverride materialOverride in materialOverrides)
			{
				if (materialOverride.newMaterial != null && materialOverride.originalMaterial != null && renderer.sharedMaterials[i].name == materialOverride.originalMaterial.name + " (Instance)")
				{
					Material[] materials = renderer.materials;
					materials[i] = materialOverride.newMaterial;
					renderer.materials = materials;
					flag = true;
				}
			}
			if (isCharacter && graphicsStyle.illuminationSettings.characterMaterialOverride != null)
			{
				Material[] materials2 = renderer.materials;
				Texture texture = materials2[i].GetTexture("_MainTex");
				Color color = Color.white;
				if (materials2[i].HasProperty("_Color"))
				{
					color = materials2[i].GetColor("_Color");
				}
				Texture texture2 = new Texture();
				if (materials2[i].HasProperty("_SpecTex"))
				{
					texture2 = materials2[i].GetTexture("_SpecTex");
				}
				Texture texture3 = new Texture();
				if (materials2[i].HasProperty("_BumpMap"))
				{
					texture3 = materials2[i].GetTexture("_BumpMap");
				}
				Texture texture4 = new Texture();
				if (materials2[i].HasProperty("_Illum"))
				{
					texture4 = materials2[i].GetTexture("_Illum");
				}
				materials2[i] = graphicsStyle.illuminationSettings.characterMaterialOverride;
				materials2[i].SetTexture("_MainTex", texture);
				if (materials2[i].HasProperty("_SpecTex"))
				{
					materials2[i].SetTexture("_SpecTex", texture2);
				}
				if (materials2[i].HasProperty("_BumpMap"))
				{
					materials2[i].SetTexture("_BumpMap", texture3);
				}
				if (materials2[i].HasProperty("_Illum"))
				{
					materials2[i].SetTexture("_Illum", texture4);
				}
				if (materials2[i].HasProperty("_Color"))
				{
					materials2[i].SetColor("_Color", color);
				}
				renderer.materials = materials2;
				flag = true;
				PlayerGraphics component = renderer.transform.root.GetComponent<PlayerGraphics>();
				if (graphicsStyle.illuminationSettings.blackOverride != 0f && (bool)component && component.currentCharacter == CharacterSelect.Characters.Afronaut && component.currentSkinNum == 0)
				{
					renderer.materials[i].SetFloat("_EmissionLM", graphicsStyle.illuminationSettings.blackOverride);
				}
			}
			if (graphicsStyle.illuminationSettings.hueCycleMaterial != null)
			{
				MonoBehaviour.print("name: " + renderer.materials[i].name + "  huemat: " + graphicsStyle.illuminationSettings.hueCycleMaterial.name + " (Instance)");
			}
			if (graphicsStyle.illuminationSettings.hueCycleMaterial != null && renderer.materials[i].name.StartsWith(graphicsStyle.illuminationSettings.hueCycleMaterial.name))
			{
				hueCycle.Add(renderer.materials[i]);
			}
			if (flag)
			{
				continue;
			}
			if (!graphicsStyle.illuminationSettings.illuminateGameplayAssets)
			{
				break;
			}
			Material material = renderer.materials[i];
			string text = material.shader.name;
			if (!text.StartsWith("Marmoset") || text.StartsWith("Marmoset/Self-Illumin"))
			{
				continue;
			}
			if (text.StartsWith("Marmoset/Transparent"))
			{
				if (text.StartsWith("Marmoset/Transparent/Simple"))
				{
					continue;
				}
				material.shader = Shader.Find("Marmoset/Transparent/Simple Glass/Bumped Specular Glow IBL");
			}
			if (text.Split('/').Length - 1 == 1)
			{
				string text2 = "Marmoset/Self-Illumin/" + material.shader.name.Substring(9);
				material.shader = Shader.Find(text2);
			}
			material.SetFloat("_SpecInt", graphicsStyle.illuminationSettings.specularIntensity);
			material.SetFloat("_GlowStrength", graphicsStyle.illuminationSettings.glowStrength);
			material.SetFloat("_EmissionLM", graphicsStyle.illuminationSettings.diffuseEmission);
			if (material.HasProperty("_SpecColor"))
			{
				material.SetColor("_SpecColor", material.GetColor("_SpecColor") * graphicsStyle.illuminationSettings.specularColorMultiply);
			}
		}
	}

	private void FixedUpdate()
	{
		if (hueCycle.Count != 0)
		{
			hueCycleColor.h += 0.01f * Time.fixedDeltaTime;
			hueCycleColor.h = Mathf.Repeat(hueCycleColor.h, 1f);
			Color color = hueCycleColor.ToColor();
			foreach (Material item in hueCycle)
			{
				item.SetColor("_GlowColor", color);
			}
		}
		if (Random.value < 0.00015f)
		{
			lightningstate = 1;
			targetLightning = 0.6f;
		}
		if (graphicsStyle.style == LevelStyle.KidsRoom_Halloween)
		{
			float num = 5f;
			if (lightningstate == 1)
			{
				num = 15f;
			}
			if (lightningstate == 2 || lightningstate == 3)
			{
				num = 10f;
			}
			colorCorrections.BlendAmount -= (colorCorrections.BlendAmount - targetLightning) * Time.fixedDeltaTime * num;
			Color white = Color.white;
			white.a = colorCorrections.BlendAmount;
			if ((bool)lightingPlane)
			{
				lightingPlane.renderer.material.color = white;
			}
			if ((bool)lightingPlane2)
			{
				lightingPlane2.renderer.material.color = white;
			}
			if (Mathf.Abs(targetLightning - colorCorrections.BlendAmount) < 0.05f)
			{
				if (lightningstate == 1)
				{
					AudioController.Play("lightning");
					lightningstate++;
					targetLightning = 0.3f;
				}
				else if (lightningstate == 2)
				{
					lightningstate++;
					targetLightning = 0.6f;
				}
				else if (lightningstate == 3)
				{
					lightningstate = 0;
					targetLightning = 0f;
				}
			}
		}
		float num2 = 5f;
		if (RenderSettings.fogDensity < targetFogDensity)
		{
			num2 = 2f;
		}
		RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * num2);
		if (RenderSettings.fogDensity < 0.001f)
		{
			RenderSettings.fog = false;
		}
		else
		{
			RenderSettings.fog = true;
		}
	}

	public void SetTargetFogAmount(float amount, bool force = false)
	{
		targetFogDensity = amount;
		if (force)
		{
			RenderSettings.fogDensity = targetFogDensity;
		}
	}

	public void SetFogColor(Color color)
	{
		RenderSettings.fogColor = color;
	}
}
