using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GUI_LevelEditorCreateLevel : GUI_Base
{
	public InputObject FirstButton;

	public List<GameObject> levelButtons;

	public List<FileInfo> allLevelFiles = new List<FileInfo>();

	public int scrollOffset;

	private int maxOffset;

	private int numFiles;

	private bool inputFieldUp;

	public UISprite topArrow;

	public UISprite botArrow;

	private void TransitionCompleted()
	{
		InitializeScreen();
		scrollOffset = 0;
		RefreshLevelObjects();
		RefreshButtonList();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
	}

	private void PrevWindow()
	{
		if (!inputFieldUp)
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorMain, "none");
			AudioController.Play("ButtonBackwards");
		}
	}

	private void NextWindow()
	{
		if (!inputFieldUp)
		{
			FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
			Singleton<InputManager>.SP.ClickCurrentButton();
		}
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_6")
		{
			maxOffset = numFiles - 6;
			if (scrollOffset < maxOffset)
			{
				scrollOffset++;
				RefreshButtonList();
			}
			Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true, playSound: false);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Up) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_0")
		{
			if (scrollOffset > 0)
			{
				scrollOffset--;
				RefreshButtonList();
			}
			Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true, playSound: false);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.RenameLevel))
		{
			if (inputFieldUp)
			{
				return;
			}
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			RenameLevel();
		}
		if (inputFieldUp && Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
		{
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
			ToggleInputField(state: false, string.Empty);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.DeleteInboxMessage) && !inputFieldUp)
		{
			if (Singleton<InputManager>.SP.GetCurrentButton().gameObject.name == "Button_0")
			{
				return;
			}
			if (Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetStrings()[1] != string.Empty)
			{
				DeleteLevel();
			}
		}
		if (scrollOffset > 0)
		{
			topArrow.enabled = true;
		}
		else
		{
			topArrow.enabled = false;
		}
		if (scrollOffset > allLevelFiles.Count - 7)
		{
			botArrow.enabled = false;
		}
		else
		{
			botArrow.enabled = true;
		}
	}

	private void DeleteLevel()
	{
		Singleton<PermaGUI>.SP.RequestConfirmation("ConfirmedDeleteLevel", base.gameObject, Language.Get("DELETELEVEL", "LEVELEDITOR") + "?");
	}

	public void ConfirmedDeleteLevel(bool confirm)
	{
		if (confirm)
		{
			string text = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetStrings()[1];
			Level levelFromGuid = Singleton<LevelBatchManager>.SP.GetLevelFromGuid(text);
			Singleton<LevelBatchManager>.SP.RemoveLevel(levelFromGuid);
			string path = Application.dataPath + "/Resources/../../CustomLevels/" + text;
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
			TransitionCompleted();
			AudioController.Play("inbox_messageremove");
		}
	}

	private void RefreshLevelObjects()
	{
		allLevelFiles.Clear();
		string path = Application.dataPath + "/Resources/../../CustomLevels/";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] directories = Directory.GetDirectories(path);
		for (int i = 0; i < directories.Length; i++)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
			allLevelFiles.AddRange(directoryInfo.GetFiles().OfType<FileInfo>().ToList());
		}
		List<FileInfo> list = new List<FileInfo>();
		foreach (FileInfo allLevelFile in allLevelFiles)
		{
			if (!allLevelFile.Name.Contains(".txt"))
			{
				list.Add(allLevelFile);
			}
		}
		foreach (FileInfo item in list)
		{
			allLevelFiles.Remove(item);
		}
		foreach (FileInfo allLevelFile2 in allLevelFiles)
		{
			if (allLevelFile2.Name.Contains(".txt"))
			{
				string a_levelName = string.Empty;
				string a_guid = string.Empty;
				HenkUtils.GetNameAndGUIDFromLevelFile(out a_levelName, out a_guid, allLevelFile2);
				if (!Singleton<LevelBatchManager>.SP.DoesLevelExist(a_levelName, a_guid))
				{
					GameObject obj = new GameObject();
					obj.transform.parent = Singleton<ActionHenk>.SP.levels.transform;
					Level level = obj.AddComponent<Level>();
					level.levelName = a_levelName;
					level.guid = a_guid;
					obj.name = a_levelName;
					level.levelCode = -1;
					level.isSceneLess = true;
					level.isEditorLevel = true;
					level.levelStyle = HenkUtils.GetLevelStyleFromLevelFile(allLevelFile2);
					Singleton<LevelBatchManager>.SP.AddEditorLevelToAllLevels(level);
				}
			}
		}
	}

	private void RefreshButtonList()
	{
		numFiles = allLevelFiles.Count;
		for (int i = 0; i < levelButtons.Count; i++)
		{
			GameObject gameObject = levelButtons[i];
			if (i > numFiles - 1)
			{
				gameObject.GetComponent<UIButtonMessage>().functionName = "Button_NewLevel";
				gameObject.GetComponentInChildren<UILabel>().text = "<Empty>";
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = string.Empty;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[1] = string.Empty;
			}
			else
			{
				gameObject.GetComponent<UIButtonMessage>().functionName = "Button_StartLevel";
				string a_levelName = string.Empty;
				string a_guid = string.Empty;
				HenkUtils.GetNameAndGUIDFromLevelFile(out a_levelName, out a_guid, allLevelFiles[i + scrollOffset]);
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = a_levelName;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[1] = a_guid;
				gameObject.GetComponentInChildren<UILabel>().text = a_levelName;
			}
		}
	}

	public void Button_StartLevel(Object value)
	{
		UIButtonMessageArguments component = (value as GameObject).GetComponent<UIButtonMessageArguments>();
		Level levelFromLevelNameAndGUID = Singleton<LevelBatchManager>.SP.GetLevelFromLevelNameAndGUID(component.GetStrings()[0], component.GetStrings()[1]);
		if (!(levelFromLevelNameAndGUID == null))
		{
			Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.LevelEditor);
			AudioController.Play("LevelStart");
			Singleton<LevelBatchManager>.SP.LoadLevelSceneless(levelFromLevelNameAndGUID);
		}
	}

	public void Button_NewLevel()
	{
		Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.LevelEditor);
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorSceneSelect, "none");
	}

	private void RenameLevel()
	{
		string text = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetStrings()[0];
		if (!(text == string.Empty))
		{
			ToggleInputField(state: true, text);
		}
	}

	private void ToggleInputField(bool state, string levelName = "")
	{
		Singleton<PermaGUI>.SP.inputField.transform.parent.gameObject.SetActive(state);
		Singleton<PermaGUI>.SP.inputFieldTitle.text = Language.Get("NAMELEVEL", "LEVELEDITOR");
		if (state)
		{
			Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = false;
			Singleton<PermaGUI>.SP.inputField.value = levelName;
			StartCoroutine(GiveFocusToInputField());
			inputFieldUp = true;
		}
		else
		{
			inputFieldUp = false;
			Singleton<PermaGUI>.SP.inputField.RemoveFocus();
			Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = true;
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
		}
	}

	private IEnumerator GiveFocusToInputField()
	{
		yield return new WaitForSeconds(0.1f);
		Singleton<PermaGUI>.SP.inputField.isSelected = true;
	}

	public void ConfirmName(UILabel input)
	{
		if (!inputFieldUp)
		{
			return;
		}
		if (input.text.Length < 2)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_LEVELNAMETOOSHORT", "PERMA"));
			return;
		}
		string text = input.text;
		string levelName = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetStrings()[0];
		string text2 = Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetStrings()[1];
		if (text2 == string.Empty)
		{
			ToggleInputField(state: false, string.Empty);
			return;
		}
		Level levelFromLevelNameAndGUID = Singleton<LevelBatchManager>.SP.GetLevelFromLevelNameAndGUID(levelName, text2);
		if ((bool)levelFromLevelNameAndGUID)
		{
			levelFromLevelNameAndGUID.levelName = text;
			levelFromLevelNameAndGUID.gameObject.name = text;
			HenkUtils.ChangeLevelNameInFile(levelFromLevelNameAndGUID, text);
			ToggleInputField(state: false, string.Empty);
			RefreshButtonList();
		}
		else
		{
			ToggleInputField(state: false, string.Empty);
		}
	}
}
