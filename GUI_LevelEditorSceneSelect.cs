using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GUI_LevelEditorSceneSelect : GUI_Base
{
	public InputObject FirstButton;

	public GameObject imagePlane;

	private Dictionary<int, Texture> sceneScreenshots = new Dictionary<int, Texture>();

	private string levelName;

	private string style;

	private string userLevelName = string.Empty;

	private bool inputFieldUp;

	public List<GameObject> locks;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		userLevelName = string.Empty;
		locks[0].SetActive(!Singleton<LevelBatchManager>.SP.batches[2].IsUnlocked());
		locks[1].SetActive(!Singleton<LevelBatchManager>.SP.batches[4].IsUnlocked());
		locks[2].SetActive(!Singleton<LevelBatchManager>.SP.batches[5].IsUnlocked());
		locks[3].SetActive(!Singleton<LevelBatchManager>.SP.batches[6].IsUnlocked());
		locks[4].SetActive(!Singleton<LevelBatchManager>.SP.batches[7].IsUnlocked());
		locks[5].SetActive(!Singleton<LevelBatchManager>.SP.batches[8].IsUnlocked());
	}

	private void Update()
	{
		if (inputFieldUp && Singleton<InputManager>.SP.CheckAction(InputAction.Cancel, forceThroughDisabledInput: true))
		{
			ToggleInputField(state: false);
		}
	}

	private void PrevWindow()
	{
		if (!inputFieldUp)
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorCreateLevel, "none");
			AudioController.Play("ButtonBackwards");
		}
	}

	private void NextWindow()
	{
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	public void Button_ChooseScene(object value)
	{
		style = (value as GameObject).GetComponent<UIButtonMessageArguments>().GetStrings()[0];
		bool flag = true;
		if (style == "KidsRoom_Halloween")
		{
			flag = Singleton<LevelBatchManager>.SP.batches[4].IsUnlocked();
		}
		else if (style == "KidsRoom_Disco")
		{
			flag = Singleton<LevelBatchManager>.SP.batches[2].IsUnlocked();
		}
		else if (style == "Island_Water")
		{
			flag = Singleton<LevelBatchManager>.SP.batches[5].IsUnlocked();
		}
		else if (style == "Island_Beach")
		{
			flag = Singleton<LevelBatchManager>.SP.batches[6].IsUnlocked();
		}
		else if (style == "Island_Jungle")
		{
			flag = Singleton<LevelBatchManager>.SP.batches[7].IsUnlocked();
		}
		else if (style == "Island_Night")
		{
			flag = Singleton<LevelBatchManager>.SP.batches[8].IsUnlocked();
		}
		if (!flag)
		{
			AudioController.Play("ButtonClick");
		}
		else if (userLevelName == string.Empty)
		{
			ToggleInputField(state: true);
		}
		else
		{
			LoadScene();
		}
	}

	public void InputFieldSubmit(UILabel input)
	{
		if (inputFieldUp)
		{
			if (input.text.Length < 2)
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_LEVELNAMETOOSHORT", "PERMA"));
				AudioController.Play("ButtonClick");
			}
			else
			{
				userLevelName = input.text;
				Singleton<LevelEditorFileWriter>.SP.levelName = userLevelName;
				ToggleInputField(state: false);
			}
		}
	}

	public void ToggleInputField(bool state)
	{
		Singleton<PermaGUI>.SP.inputField.transform.parent.gameObject.SetActive(state);
		Singleton<PermaGUI>.SP.inputFieldTitle.text = Language.Get("NAMELEVEL", "LEVELEDITOR");
		if (state)
		{
			Singleton<InputManager>.SP.inputEnabled = false;
			Singleton<PermaGUI>.SP.inputField.isSelected = true;
			Singleton<PermaGUI>.SP.inputField.value = string.Empty;
			inputFieldUp = true;
			AudioController.Play("ButtonForwards");
		}
		else
		{
			inputFieldUp = false;
			Singleton<PermaGUI>.SP.inputField.RemoveFocus();
			Singleton<InputManager>.SP.inputEnabled = true;
		}
	}

	private string RemoveExtraWhitespace(string nameString)
	{
		return Regex.Replace(nameString, "\\s+", " ");
	}

	private void LoadScene()
	{
		GameObject obj = new GameObject();
		obj.transform.parent = Singleton<ActionHenk>.SP.levels.transform;
		Level level = obj.AddComponent<Level>();
		level.levelStyle = HenkUtils.EnumParse<LevelStyle>(style);
		level.levelName = "NewLevel";
		obj.name = "CUSTOM";
		level.levelCode = -1;
		level.guid = Guid.NewGuid().ToString();
		level.isSceneLess = true;
		level.isEditorLevel = true;
		Singleton<LevelBatchManager>.SP.AddEditorLevelToAllLevels(level);
		AudioController.Play("LevelStart");
		Singleton<LevelBatchManager>.SP.LoadLevelSceneless(level);
	}

	public void SceneSelectItemSelected(LevelStyle style)
	{
		if (!sceneScreenshots.ContainsKey((int)style))
		{
			Texture texture = null;
			int num = (int)style;
			texture = Resources.Load("LevelScreens/empty_" + num) as Texture;
			if (texture != null)
			{
				sceneScreenshots.Add((int)style, texture);
			}
		}
		if (sceneScreenshots.ContainsKey((int)style))
		{
			imagePlane.renderer.material.mainTexture = sceneScreenshots[(int)style];
			imagePlane.SetActive(value: true);
		}
		else
		{
			imagePlane.SetActive(value: false);
		}
	}
}
