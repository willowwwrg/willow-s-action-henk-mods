using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GUI_MultiplayerLevelRotation : GUI_Base
{
	public InputObject FirstButton;

	public List<GameObject> buttons;

	public GameObject imagePlane;

	private Dictionary<int, Texture> levelScreenshots = new Dictionary<int, Texture>();

	public UILabel difficultyLabel;

	private List<Level> playableLevels = new List<Level>();

	private List<Level> levelsInRotation = new List<Level>();

	private int scrollOffset;

	private int maxOffset;

	public UISprite topArrow;

	public UISprite botArrow;

	private void TransitionCompleted()
	{
		InitializeScreen();
		scrollOffset = 0;
		Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: true, Language.Get("APPLY", "PERMA"), Language.Get("TOGGLE", "PERMA"));
		levelsInRotation = GetLevelsFromPlayerPrefs();
		BuildLevelList();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
	}

	private List<Level> GetLevelsFromPlayerPrefs()
	{
		List<Level> list = new List<Level>();
		string empty = string.Empty;
		empty = ((Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LocalMultiplayer) ? "MultiplayerSettings_LEVELROTATION" : "LMPSettings_LEVELROTATION");
		string text = Singleton<PlayerPrefsManager>.SP.GetString(empty, string.Empty);
		if (text == string.Empty)
		{
			return list;
		}
		List<string> list2 = new List<string>(text.Split(','));
		for (int i = 0; i < list2.Count; i++)
		{
			Level levelFromCodeOrID = Singleton<LevelBatchManager>.SP.GetLevelFromCodeOrID(Convert.ToUInt64(list2[i]));
			if ((bool)levelFromCodeOrID)
			{
				list.Add(levelFromCodeOrID);
			}
		}
		return list;
	}

	private void Update()
	{
		int count = playableLevels.Count;
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_6")
		{
			maxOffset = count - 7;
			if (scrollOffset < maxOffset)
			{
				scrollOffset++;
				BuildLevelList();
			}
			Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Up) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_0")
		{
			if (scrollOffset > 0)
			{
				scrollOffset--;
				BuildLevelList();
			}
			if (count > 7)
			{
				Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true);
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm))
		{
			Singleton<InputManager>.SP.ClickCurrentButton();
			Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: false, playSound: false);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.CreateServer))
		{
			Button_ClearSelection();
			AudioController.Play("ButtonForwards");
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Randomize))
		{
			Button_Randomize();
			AudioController.Play("inbox_open");
		}
	}

	private void PrevWindow()
	{
		GoToPrevWindow(playsound: true);
	}

	private void GoToPrevWindow(bool playsound)
	{
		string text = string.Empty;
		for (int i = 0; i < levelsInRotation.Count; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			text += levelsInRotation[i].GetLevelID();
		}
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LocalMultiplayer)
		{
			Singleton<LocalMultiManager>.SP.SetLevelRotation(levelsInRotation);
			Singleton<PlayerPrefsManager>.SP.SetString("LMPSettings_LEVELROTATION", text);
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP, "none");
		}
		else
		{
			Singleton<MultiManager>.SP.SetLevelRotation(levelsInRotation);
			Singleton<PlayerPrefsManager>.SP.SetString("MultiplayerSettings_LEVELROTATION", text);
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerRoomCreation, "none");
		}
		if (playsound)
		{
			AudioController.Play("ButtonBackwards");
		}
	}

	public void ConfirmLevelRotation()
	{
		levelsInRotation = GetLevelsFromPlayerPrefs();
		Singleton<MultiManager>.SP.SetLevelRotation(levelsInRotation);
	}

	private void Button_ClearSelection()
	{
		levelsInRotation.Clear();
		Singleton<MultiManager>.SP.SetLevelRotation(levelsInRotation);
		BuildLevelList();
		Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: false, playSound: false);
	}

	private void Button_Randomize()
	{
		List<Level> list = new List<Level>(playableLevels);
		levelsInRotation.Clear();
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			Level item = list[UnityEngine.Random.Range(0, list.Count)];
			levelsInRotation.Add(item);
			list.Remove(item);
		}
		BuildLevelList();
		Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: false, playSound: false);
	}

	private void BuildLevelList()
	{
		GetPlayableLevels();
		for (int i = 0; i < buttons.Count; i++)
		{
			GameObject obj = buttons[i];
			obj.SetActive(value: true);
			obj.GetComponentInChildren<UILabel>().text = string.Empty;
			obj.GetComponentInChildren<UIToggle>().value = false;
			obj.SetActive(value: false);
		}
		int count = playableLevels.Count;
		for (int j = 0; j < buttons.Count; j++)
		{
			GameObject gameObject = buttons[j];
			if (j + scrollOffset > count - 1)
			{
				continue;
			}
			gameObject.SetActive(value: true);
			gameObject.GetComponent<UIButtonMessage>().target = base.gameObject;
			gameObject.GetComponent<UIButtonMessageArguments>().GetGameobjects()[0] = playableLevels[j + scrollOffset].gameObject;
			gameObject.GetComponentInChildren<UILabel>().text = playableLevels[j + scrollOffset].levelName;
			if (!levelsInRotation.Contains(playableLevels[j + scrollOffset]))
			{
				continue;
			}
			gameObject.GetComponentInChildren<UIToggle>().value = true;
			int num = 0;
			for (int k = 0; k < levelsInRotation.Count; k++)
			{
				if (levelsInRotation[k] == playableLevels[j + scrollOffset])
				{
					num = k + 1;
				}
			}
			UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
			componentInChildren.text = componentInChildren.text + " (" + num + ")";
		}
		SetInputObjects();
		if (scrollOffset > 0)
		{
			topArrow.enabled = true;
		}
		else
		{
			topArrow.enabled = false;
		}
		if (scrollOffset > playableLevels.Count - 8)
		{
			botArrow.enabled = false;
		}
		else
		{
			botArrow.enabled = true;
		}
	}

	private void GetPlayableLevels()
	{
		playableLevels.Clear();
		for (int i = 0; i < Singleton<LevelBatchManager>.SP.batches.Count; i++)
		{
			if (Singleton<LevelBatchManager>.SP.batches[i].IsUnlocked() || i < 2)
			{
				playableLevels.AddRange(Singleton<LevelBatchManager>.SP.batches[i].GetStandardLevels());
			}
		}
		playableLevels.AddRange(Singleton<LevelBatchManager>.SP.GetWorkshopLevels());
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LocalMultiplayer)
		{
			playableLevels.Remove(Singleton<LevelBatchManager>.SP.GetLevelFromCode(13));
		}
	}

	private void SetInputObjects()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			InputObject component = buttons[i].GetComponent<InputObject>();
			component.selectOnDown = (component.selectOnLeft = (component.selectOnRight = (component.selectOnUp = null)));
		}
		for (int j = 0; j < buttons.Count; j++)
		{
			if (buttons[j].activeInHierarchy)
			{
				InputObject component2 = buttons[j].GetComponent<InputObject>();
				if (j < buttons.Count - 1 && buttons[j + 1].activeInHierarchy)
				{
					component2.selectOnDown = buttons[j + 1].GetComponent<InputObject>();
				}
				if (j > 0)
				{
					component2.selectOnUp = buttons[j - 1].GetComponent<InputObject>();
				}
			}
		}
	}

	public void HoverOverLevelSelectButton(GameObject button)
	{
		Level component = button.GetComponent<UIButtonMessageArguments>().GetGameobjects()[0].GetComponent<Level>();
		if (component.levelType == LevelType.Workshop)
		{
			difficultyLabel.text = "Workshop level";
		}
		else
		{
			difficultyLabel.text = Language.Get(component.difficulty.ToString().ToUpper(), "LEVELSELECT");
		}
		if (component.levelType == LevelType.Workshop)
		{
			if (File.Exists(component.workshopFolderName + "/preview.png"))
			{
				byte[] data = File.ReadAllBytes(component.workshopFolderName + "/preview.png");
				Texture2D texture2D = new Texture2D(512, 512);
				texture2D.LoadImage(data);
				imagePlane.renderer.material.mainTexture = texture2D;
				imagePlane.renderer.enabled = true;
			}
			else
			{
				imagePlane.renderer.enabled = false;
			}
			return;
		}
		if (!levelScreenshots.ContainsKey(component.levelCode))
		{
			Texture texture = null;
			texture = Resources.Load("LevelScreens/" + component.levelCode) as Texture;
			if (texture != null)
			{
				levelScreenshots.Add(component.levelCode, texture);
			}
		}
		if (levelScreenshots.ContainsKey(component.levelCode))
		{
			imagePlane.renderer.material.mainTexture = levelScreenshots[component.levelCode];
			imagePlane.SetActive(value: true);
		}
		else
		{
			imagePlane.SetActive(value: false);
		}
	}

	public void LevelButton(object value)
	{
		GameObject gameObject = value as GameObject;
		gameObject.GetComponentInChildren<UIToggle>().value = !gameObject.GetComponentInChildren<UIToggle>().value;
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(gameObject.GetComponentInChildren<UIToggle>().value);
		Level component = gameObject.GetComponent<UIButtonMessageArguments>().GetGameobjects()[0].GetComponent<Level>();
		if (gameObject.GetComponentInChildren<UIToggle>().value)
		{
			levelsInRotation.Add(component);
		}
		else if (levelsInRotation.Contains(component))
		{
			levelsInRotation.Remove(component);
		}
		BuildLevelList();
	}
}
