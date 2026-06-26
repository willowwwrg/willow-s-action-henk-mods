using System.Collections;
using UnityEngine;

public class GUI_OptionsGraphics : GUI_Base
{
	public InputObject FirstButton;

	public UIToggle toggleDepthOfField;

	public UIToggle toggleBloom;

	public UIToggle toggleVsync;

	public UIToggle toggleSSAO;

	public UIToggle toggleDE;

	public UILabel AALabel;

	public UILabel shadowQualLabel;

	public UILabel fpsLabel;

	public UISprite fpsDial;

	public UISprite fpsDialGlow;

	public UILabel greatLabel;

	public ArrowPositions sliderArrowPositions;

	public ArrowPositions resolutionArrowPositions;

	public UIToggle toggleFullscreen;

	private Resolution[] resolutions;

	public UILabel resolutionLabel;

	private Resolution currentResolution;

	private int currentResolutionNumber;

	private int currentShadowQuality;

	private int currentAALevel;

	public UILabel settingTitle;

	public UILabel settingDescription;

	public UILabel settingPerformanceImpact;

	public UILabel settingPerformanceImpactVariable;

	private bool resolutionChanged;

	private float smootherDeltaTime;

	private float maxLOD = 2f;

	private float minLOD = 0.5f;

	private void Awake()
	{
		InitQualityPrefs();
		if (!Screen.fullScreen)
		{
			toggleFullscreen.value = false;
		}
		else
		{
			toggleFullscreen.value = true;
		}
		resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
			{
				currentResolution = resolutions[i];
				currentResolutionNumber = i;
				SetResolutionLabel(currentResolution);
				break;
			}
		}
	}

	private void InitQualityPrefs()
	{
		QualitySettings.SetQualityLevel(currentShadowQuality = PlayerPrefs.GetInt("QualitySettings_Shadow", 1));
		SetShadowQualityLabel(currentShadowQuality);
		int num = PlayerPrefs.GetInt("QualitySettings_AA", 0);
		currentAALevel = num;
		SetAALevel(forwards: false, hardSet: true);
		SetAALabel(currentAALevel);
		toggleDepthOfField.value = PlayerPrefs.GetInt("QualitySettings_DOF", 0) != 0;
		toggleVsync.value = PlayerPrefs.GetInt("QualitySettings_VS", 0) != 0;
		toggleBloom.value = PlayerPrefs.GetInt("QualitySettings_BLOOM", 0) != 0;
		toggleSSAO.value = PlayerPrefs.GetInt("QualitySettings_SSAO", 0) != 0;
		toggleDE.value = PlayerPrefs.GetInt("QualitySettings_DEPTHEFFECTS", 1) != 0;
		QualitySettings.lodBias = ((!toggleDE.value) ? minLOD : maxLOD);
		SetAnisotropicFiltering(anisotropicFiltering: true);
		ToggleVsync(toggleVsync.value);
	}

	public void RefreshQualitySettings()
	{
		InitQualityPrefs();
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		resolutionChanged = false;
		StartCoroutine(DelayedQualityPrefsRefresh());
		smootherDeltaTime = 0.1f;
	}

	private IEnumerator DelayedQualityPrefsRefresh()
	{
		yield return new WaitForSeconds(0.01f);
		InitQualityPrefs();
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_Options));
		AudioController.Play("ButtonBackwards");
		Singleton<PermaGUI>.SP.ToggleArrows(state: false);
	}

	public void Button_Vsync()
	{
		toggleVsync.value = !toggleVsync.value;
		PlayerPrefs.SetInt("QualitySettings_VS", toggleVsync.value ? 1 : 0);
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleVsync.value);
		ToggleVsync(toggleVsync.value);
	}

	public void Button_DE()
	{
		toggleDE.value = !toggleDE.value;
		PlayerPrefs.SetInt("QualitySettings_DEPTHEFFECTS", toggleDE.value ? 1 : 0);
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleDE.value);
		QualitySettings.lodBias = ((!toggleDE.value) ? minLOD : maxLOD);
		UpdateCameraEffects();
	}

	public void Button_DepthOfField()
	{
		toggleDepthOfField.value = !toggleDepthOfField.value;
		PlayerPrefs.SetInt("QualitySettings_DOF", toggleDepthOfField.value ? 1 : 0);
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleDepthOfField.value);
		UpdateCameraEffects();
	}

	public void Button_Bloom()
	{
		toggleBloom.value = !toggleBloom.value;
		PlayerPrefs.SetInt("QualitySettings_BLOOM", toggleBloom.value ? 1 : 0);
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleBloom.value);
		UpdateCameraEffects();
	}

	public void Button_SSAO()
	{
		toggleSSAO.value = !toggleSSAO.value;
		PlayerPrefs.SetInt("QualitySettings_SSAO", toggleSSAO.value ? 1 : 0);
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleSSAO.value);
		UpdateCameraEffects();
	}

	private void UpdateCameraEffects()
	{
		Camera.main.GetComponent<CameraEffectsManager>().ReadPlayerPrefs();
		Camera.main.GetComponent<CameraEffectsManager>().UpdateComponents();
	}

	private void ToggleVsync(bool toggle)
	{
		if (toggle)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Left))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_9 res")
			{
				SetPendingResolution(forwards: false);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_7 sq")
			{
				SetShadowQualityLevel(forwards: false);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_4 aa")
			{
				SetAALevel(forwards: false);
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Right))
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_9 res")
			{
				SetPendingResolution(forwards: true);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_7 sq")
			{
				SetShadowQualityLevel(forwards: true);
			}
			else if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_4 aa")
			{
				SetAALevel(forwards: true);
			}
		}
		smootherDeltaTime = Mathf.Lerp(smootherDeltaTime, Time.smoothDeltaTime, Time.deltaTime * 4f);
		int num = (int)(1f / smootherDeltaTime);
		fpsLabel.text = num.ToString();
		greatLabel.text = Language.Get("FPSLABELBLAZIN", "SETTINGS");
		if (num < 110)
		{
			greatLabel.text = Language.Get("FPSLABELINTENSE", "SETTINGS");
		}
		if (num < 70)
		{
			greatLabel.text = Language.Get("FPSLABELGREAT", "SETTINGS");
		}
		if (num < 45)
		{
			greatLabel.text = Language.Get("FPSLABELGOOD", "SETTINGS");
		}
		if (num < 30)
		{
			greatLabel.text = Language.Get("FPSLABELLOW", "SETTINGS");
		}
		if (num < 20)
		{
			greatLabel.text = Language.Get("FPSLABELPOOR", "SETTINGS");
		}
		float fillAmount = Mathf.Lerp(0f, 0.705f, Mathf.InverseLerp(0f, 127.5f, num));
		fpsDial.fillAmount = fillAmount;
		float t = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 60f, num));
		fpsDialGlow.color = Color.Lerp(Color.red, Color.green, t);
		string text = Singleton<InputManager>.SP.GetCurrentButton().gameObject.name;
		switch (text)
		{
		case "Button_7 tq":
		case "Button_4 aa":
		case "Button_8 fs":
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: false, string.Empty, string.Empty);
			break;
		default:
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, string.Empty, Language.Get("TOGGLE", "PERMA"));
			break;
		}
		if (resolutionChanged && text == "Button_9 res")
		{
			Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, string.Empty, Language.Get("APPLY", "PERMA"));
		}
	}

	private void SetAALevel(bool forwards, bool hardSet = false)
	{
		if (!hardSet)
		{
			if (forwards)
			{
				currentAALevel++;
				if (currentAALevel > 3)
				{
					currentAALevel = 0;
				}
			}
			else
			{
				currentAALevel--;
				if (currentAALevel < 0)
				{
					currentAALevel = 3;
				}
			}
		}
		if (currentAALevel == 0)
		{
			AALabel.text = "< off >";
			QualitySettings.antiAliasing = 0;
		}
		else if (currentAALevel == 1)
		{
			AALabel.text = "< 2x >";
			QualitySettings.antiAliasing = 2;
		}
		else if (currentAALevel == 2)
		{
			AALabel.text = "< 4x >";
			QualitySettings.antiAliasing = 4;
		}
		else if (currentAALevel == 3)
		{
			AALabel.text = "< 8x >";
			QualitySettings.antiAliasing = 8;
		}
		else
		{
			AALabel.text = "< off >";
			QualitySettings.antiAliasing = 0;
		}
		PlayerPrefs.SetInt("QualitySettings_AA", currentAALevel);
		SetAALabel(currentAALevel);
		_ = base.gameObject.activeInHierarchy;
	}

	public void SetAnisotropicFiltering(bool anisotropicFiltering)
	{
		if (!anisotropicFiltering)
		{
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
		}
		else
		{
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
		}
	}

	public void OnHover(GameObject button)
	{
		string text = button.name;
		settingPerformanceImpact.enabled = true;
		switch (text)
		{
		case "Button_3 dof":
			settingTitle.text = Language.Get("DOF", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_DOF", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("HIGH", "SETTINGS");
			break;
		case "Button_5 ssao":
			settingTitle.text = Language.Get("SSAO", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_SSAO", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("HIGH", "SETTINGS");
			break;
		case "Button_2 bloom":
			settingTitle.text = Language.Get("BLOOM", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_BLOOM", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("MEDIUM", "SETTINGS");
			break;
		case "Button_4 aa":
			settingTitle.text = Language.Get("AA", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_AA", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("LOW", "SETTINGS");
			break;
		case "Button_1 vsync":
			settingTitle.text = Language.Get("VSYNC", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_VSYNC", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("HIGH", "SETTINGS");
			break;
		case "Button_7 sq":
			settingTitle.text = Language.Get("SHADOWQUALITY", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_SQ", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("HIGH", "SETTINGS");
			break;
		case "Button_8 fs":
			settingTitle.text = Language.Get("FULLSCREEN", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_FULLSCREEN", "SETTINGS");
			settingPerformanceImpactVariable.text = string.Empty;
			settingPerformanceImpact.enabled = false;
			break;
		case "Button_9 res":
			settingTitle.text = Language.Get("RESOLUTION", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_RESOLUTION", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("MEDIUM", "SETTINGS");
			break;
		case "Button_6 de":
			settingTitle.text = Language.Get("ENVQUALITY", "SETTINGS");
			settingDescription.text = Language.Get("EXPL_ENVQUALITY", "SETTINGS");
			settingPerformanceImpactVariable.text = Language.Get("MEDIUM", "SETTINGS");
			break;
		default:
			settingTitle.text = string.Empty;
			settingDescription.text = string.Empty;
			settingPerformanceImpactVariable.text = string.Empty;
			break;
		}
	}

	private void SetAALabel(int AAlevel)
	{
		switch (AAlevel)
		{
		case 0:
			AALabel.text = "< off >";
			break;
		case 1:
			AALabel.text = "< 2x >";
			break;
		case 2:
			AALabel.text = "< 4x >";
			break;
		case 3:
			AALabel.text = "< 8x >";
			break;
		default:
			AALabel.text = "< undefined >";
			break;
		}
	}

	private void SetShadowQualityLevel(bool forwards)
	{
		if (forwards)
		{
			AudioController.Play("NextMenuItem");
			currentShadowQuality++;
			if (currentShadowQuality > 4)
			{
				currentShadowQuality = 0;
			}
		}
		else
		{
			AudioController.Play("PrevMenuItem");
			currentShadowQuality--;
			if (currentShadowQuality < 0)
			{
				currentShadowQuality = 4;
			}
		}
		PlayerPrefs.SetInt("QualitySettings_Shadow", currentShadowQuality);
		SetShadowQualityLabel(currentShadowQuality);
		InitQualityPrefs();
	}

	private void SetShadowQualityLabel(int qualityLevel)
	{
		switch (qualityLevel)
		{
		case 0:
			shadowQualLabel.text = "< " + Language.Get("NONE", "SETTINGS") + " >";
			break;
		case 1:
			shadowQualLabel.text = "< " + Language.Get("LOW", "SETTINGS") + " >";
			break;
		case 2:
			shadowQualLabel.text = "< " + Language.Get("MEDIUM", "SETTINGS") + " >";
			break;
		case 3:
			shadowQualLabel.text = "< " + Language.Get("HIGH", "SETTINGS") + " >";
			break;
		case 4:
			shadowQualLabel.text = "< " + Language.Get("VERYHIGH", "SETTINGS") + " >";
			break;
		default:
			shadowQualLabel.text = "< undefined >";
			break;
		}
	}

	private void SetPendingResolution(bool forwards)
	{
		if (forwards)
		{
			currentResolutionNumber++;
			if (currentResolutionNumber > resolutions.Length - 1)
			{
				currentResolutionNumber = 0;
			}
		}
		else
		{
			currentResolutionNumber--;
			if (currentResolutionNumber < 0)
			{
				currentResolutionNumber = resolutions.Length - 1;
			}
		}
		resolutionChanged = true;
		Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, string.Empty, Language.Get("APPLY", "PERMA"));
		currentResolution = resolutions[currentResolutionNumber];
		SetResolutionLabel(currentResolution);
	}

	private void SetResolutionLabel(Resolution resolution)
	{
		resolutionLabel.text = "< " + resolution.width + " x " + resolution.height + " >";
	}

	public void Button_Resolutions()
	{
		Button_SelectResolution(currentResolution);
	}

	public void Button_SelectResolution(Resolution resolution)
	{
		Singleton<WindowManager>.SP.SetResolution(currentResolution.width, currentResolution.height);
		resolutionChanged = false;
		Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, string.Empty, Language.Get("TOGGLE", "PERMA"));
	}

	public void Button_Fullscreen()
	{
		if (toggleFullscreen.value)
		{
			Singleton<WindowManager>.SP.SetFullscreen(state: false);
			toggleFullscreen.value = false;
		}
		else
		{
			Singleton<WindowManager>.SP.SetFullscreen(state: true);
			toggleFullscreen.value = true;
		}
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(toggleFullscreen.value);
	}
}
