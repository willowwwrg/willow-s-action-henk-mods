using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GUI_InGameLevelEditor : GUI_Base
{
	public GameObject toyboxObj;

	public GameObject legendObj;

	public GameObject prevNextObj;

	public List<InputObject> levelEditorSelectableObjects;

	public UILabel firstLabel;

	public UILabel secondLabel;

	public UILabel levelNameLabel;

	public GameObject menuItem;

	public InputObject menuInputObj;

	public UILabel toggleToyboxLabel;

	public UILabel toggleHelpLabel;

	private bool legendIsShowing;

	public GameObject prevItemAnchor;

	public GameObject nextItemAnchor;

	public GameObject curItemAnchor;

	public bool takingScreenshot;

	public GameObject screenshotInfoLabel;

	private void Awake()
	{
	}

	private void Update()
	{
		if (Singleton<EditorCursor>.SP.selection.Count > 0)
		{
			firstLabel.text = Language.Get("PLACE", "LEVELEDITOR");
			secondLabel.text = Language.Get("PLACECLONE", "LEVELEDITOR");
		}
		else if (Singleton<EditorCursor>.SP.hoverTargets.Count != 0)
		{
			firstLabel.text = Language.Get("PICKUP", "LEVELEDITOR");
			secondLabel.text = Language.Get("CLONEPICKUP", "LEVELEDITOR");
		}
		else
		{
			firstLabel.text = "...";
			secondLabel.text = "...";
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
		{
			ToggleMenu(!menuItem.activeInHierarchy);
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			ToggleLegend(!legendIsShowing);
		}
		if (Input.GetKeyDown(KeyCode.F4))
		{
			Button_SaveLevel();
		}
		if (takingScreenshot && Singleton<InputManager>.SP.CheckAction(InputAction.Confirm))
		{
			SnapScreenshot();
		}
	}

	private void NextWindow()
	{
		if (Singleton<EditorCursor>.SP.toyboxOpen)
		{
			Singleton<EditorCursor>.SP.selectedGUIItem = Singleton<InputManager>.SP.GetCurrentButton();
		}
		if (Singleton<InputManager>.SP.GetCurrentButton() != null)
		{
			Singleton<InputManager>.SP.ClickCurrentButton();
		}
	}

	private void TransitionCompleted()
	{
		levelNameLabel.text = Singleton<LevelEditorFileWriter>.SP.levelName;
		InitializeScreen();
		Singleton<InputManager>.SP.Deselect();
		Singleton<PlayerManager>.SP.ghostSet = true;
		toyboxObj.GetComponent<LerpToTargetTransform>().Default(hard: true);
		ToggleLegend(state: true);
	}

	private void ToggleMenu(bool state)
	{
		if (Singleton<EditorCursor>.SP.selection.Count <= 0 && !Singleton<EditorCursor>.SP.toyboxOpen)
		{
			menuItem.SetActive(state);
			if (state)
			{
				ToggleToybox(state: false);
				Singleton<InputManager>.SP.Select(menuInputObj);
			}
			else
			{
				Singleton<InputManager>.SP.Deselect();
			}
			Singleton<EditorCursor>.SP.menuVisible = state;
			ToggleLegend(!state);
		}
	}

	public void LevelEditorGUIButton(object value)
	{
		Singleton<EditorCursor>.SP.ToggleToybox(!Singleton<EditorCursor>.SP.toyboxOpen, resetCursorState: true);
	}

	public void ToggleToybox(bool state)
	{
		if (state)
		{
			toyboxObj.GetComponent<LerpToTargetTransform>().SetTarget(new Vector3(0f, 0f, 0f));
			toggleToyboxLabel.text = Language.Get("HIDETOYBOX", "LEVELEDITOR");
		}
		else
		{
			toyboxObj.GetComponent<LerpToTargetTransform>().SetTarget(new Vector3(1300f, 0f, 0f));
			toggleToyboxLabel.text = Language.Get("SHOWTOYBOX", "LEVELEDITOR");
		}
	}

	public void ToggleLegend(bool state)
	{
		if (state)
		{
			legendObj.GetComponent<LerpToTargetTransform>().SetTarget(new Vector3(435f, 0f, 0f));
			toggleHelpLabel.text = Language.Get("HIDEHELP", "LEVELEDITOR");
		}
		else
		{
			legendObj.GetComponent<LerpToTargetTransform>().SetTarget(new Vector3(0f, 0f, 0f));
			toggleHelpLabel.text = Language.Get("SHOWHELP", "LEVELEDITOR");
		}
		legendIsShowing = state;
	}

	private void Button_Resume()
	{
		ToggleMenu(state: false);
	}

	private void Button_PlayMode()
	{
		if (!Singleton<EditorCursor>.SP.DoesStartLineExist())
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOSTARTLINE", "PERMA"));
			return;
		}
		ToggleMenu(state: false);
		if (!Singleton<EditorCursor>.SP.menuVisible)
		{
			Singleton<EditorCursor>.SP.GoToIngame();
			Singleton<GamestateManager>.SP.SetState(typeof(State_InGame));
			SelectedItem[] array = Object.FindObjectsOfType<SelectedItem>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchToInGame();
			}
		}
	}

	private void Button_SaveLevel()
	{
		Singleton<CheckpointManager>.SP.LevelEditorUpdate();
		Singleton<EditorCursor>.SP.SaveLevel(string.Empty);
		ToggleMenu(state: false);
	}

	private void Button_ExitLevel()
	{
		Object.FindObjectOfType<State_InGameLevelEditor>().CloseLevelEditor();
		ToggleMenu(state: false);
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
	}

	private void Button_WorkshopPublish()
	{
		if (!SteamManager.Initialized)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDPUBLISH", "PERMA"));
			return;
		}
		Singleton<Workshop>.SP.validating = true;
		ToggleMenu(state: false);
		Singleton<CheckpointManager>.SP.LevelEditorUpdate();
		if (Singleton<EditorCursor>.SP.SaveLevel(string.Empty))
		{
			StartTakingScreenshot();
		}
		else
		{
			Singleton<Workshop>.SP.validating = false;
		}
	}

	private void StartValidating()
	{
		Singleton<EditorCursor>.SP.GoToIngame();
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGame));
		SelectedItem[] array = Object.FindObjectsOfType<SelectedItem>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SwitchToInGame();
		}
	}

	private void StartTakingScreenshot()
	{
		takingScreenshot = true;
		screenshotInfoLabel.SetActive(value: true);
		toyboxObj.SetActive(value: false);
		legendObj.SetActive(value: false);
		Singleton<EditorCursor>.SP.visualsObject.SetActive(value: false);
	}

	public void SnapScreenshot()
	{
		StartCoroutine(ScreenshotRoutine());
	}

	private IEnumerator ScreenshotRoutine()
	{
		screenshotInfoLabel.SetActive(value: false);
		Singleton<EditorCursor>.SP.visualsObject.SetActive(value: false);
		yield return new WaitForEndOfFrame();
		string text = Application.dataPath + "/Resources/../../CustomLevels/" + Singleton<LevelBatchManager>.SP.currentLevel.guid.ToString() + "/";
		int num = 636;
		int num2 = 358;
		Resolution currentResolution = Screen.currentResolution;
		RenderTexture renderTexture = new RenderTexture(num, num2, 24);
		Camera.main.targetTexture = renderTexture;
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipmap: false);
		Camera.main.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
		Camera.main.targetTexture = null;
		RenderTexture.active = null;
		Object.Destroy(renderTexture);
		File.WriteAllBytes(bytes: texture2D.EncodeToPNG(), path: text + "preview.png");
		toyboxObj.SetActive(value: true);
		legendObj.SetActive(value: true);
		Singleton<EditorCursor>.SP.visualsObject.SetActive(value: true);
		takingScreenshot = false;
		Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
		StartValidating();
	}
}
