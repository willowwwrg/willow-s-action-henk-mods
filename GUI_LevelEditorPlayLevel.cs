using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

public class GUI_LevelEditorPlayLevel : GUI_Base
{
	public InputObject FirstButton;

	public List<GameObject> levelButtons;

	private GameObject currentObj;

	private bool deleting;

	private bool delayingRefresh;

	public GameObject imagePlane;

	public UILabel authorlabel;

	public UILabel descriptionLabel;

	private int scrollOffset;

	private int maxOffset;

	private int numLevels;

	private void TransitionCompleted()
	{
		InitializeScreen();
		scrollOffset = 0;
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		Singleton<PermaGUI>.SP.RedrawAllGUI();
		if (Singleton<LevelBatchManager>.SP.workshopLevelsUpToDate)
		{
			WorkshopLevelsUpToDate();
		}
	}

	private void PrevWindow()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelEditorMain, "none");
		AudioController.Play("ButtonBackwards");
		Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
	}

	private void NextWindow()
	{
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<InputManager>.SP.ClickCurrentButton();
		Singleton<PermaGUI>.SP.ToggleSubSubTitle(state: false, string.Empty);
	}

	private void RefreshButtonList()
	{
		List<Level> workshopLevels = Singleton<LevelBatchManager>.SP.GetWorkshopLevels();
		numLevels = workshopLevels.Count;
		int num = 0;
		for (int i = 0; i < levelButtons.Count; i++)
		{
			GameObject gameObject = levelButtons[i];
			if (i > numLevels - 1 || i + scrollOffset > workshopLevels.Count - 1)
			{
				gameObject.GetComponent<UIButtonMessage>().functionName = string.Empty;
				gameObject.GetComponentInChildren<UILabel>().text = string.Empty;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = string.Empty;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[1] = string.Empty;
			}
			else
			{
				num++;
				gameObject.GetComponent<UIButtonMessage>().target = base.gameObject;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = workshopLevels[i + scrollOffset].levelName;
				gameObject.GetComponent<UIButtonMessageArguments>().GetStrings()[1] = workshopLevels[i + scrollOffset].guid;
				gameObject.GetComponentInChildren<UILabel>().text = workshopLevels[i + scrollOffset].levelName;
			}
			SetVoteOnButton(gameObject);
		}
		SetInputObjects(num);
	}

	public void SetSelectedItem(InputObject obj)
	{
		currentObj = obj.gameObject;
		if (currentObj.GetComponent<UIButtonMessageArguments>().GetStrings()[1] == string.Empty)
		{
			authorlabel.text = string.Empty;
			imagePlane.renderer.enabled = false;
			currentObj.GetComponentInChildren<UISprite>();
			return;
		}
		string levelCreator = Singleton<LevelBatchManager>.SP.GetLevelCreator(currentObj.GetComponent<UIButtonMessageArguments>().GetStrings()[1]);
		Level levelFromGuid = Singleton<LevelBatchManager>.SP.GetLevelFromGuid(currentObj.GetComponent<UIButtonMessageArguments>().GetStrings()[1]);
		if ((bool)levelFromGuid)
		{
			if (File.Exists(levelFromGuid.workshopFolderName + "/preview.png"))
			{
				byte[] data = File.ReadAllBytes(levelFromGuid.workshopFolderName + "/preview.png");
				Texture2D texture2D = new Texture2D(512, 512);
				texture2D.LoadImage(data);
				imagePlane.renderer.material.mainTexture = texture2D;
				imagePlane.renderer.enabled = true;
			}
			else
			{
				imagePlane.renderer.enabled = false;
			}
		}
		if ((bool)levelFromGuid)
		{
			authorlabel.text = levelFromGuid.levelName + " - " + Language.Get("MADEBY", "LEVELEDITOR") + " " + levelCreator;
			descriptionLabel.text = levelFromGuid.workshopLevelDescription;
		}
	}

	public void WorkshopLevelsUpToDate()
	{
		RefreshButtonList();
	}

	public void RefreshingWorkshopLevelsCompleted()
	{
		WorkshopLevelsUpToDate();
	}

	private void Update()
	{
		if (Singleton<LevelBatchManager>.SP.refreshingWorkshopLevels)
		{
			return;
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.DeleteInboxMessage))
		{
			DeleteCurrentItem();
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			GiveCurrentItemAVote(upvote: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			GiveCurrentItemAVote(upvote: false);
		}
		if (Singleton<InputManager>.SP.GetCurrentButton() == null && FirstButton == null && Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
		{
			PrevWindow();
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Down) && Singleton<InputManager>.SP.GetCurrentButton().name == "Button_6")
		{
			maxOffset = numLevels - 7;
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
			if (numLevels > 7)
			{
				Singleton<InputManager>.SP.Select(Singleton<InputManager>.SP.GetCurrentButton(), delayedTillEndOfFrame: true, playSound: false);
			}
		}
	}

	public void ItemVoteOnObjectUpdated()
	{
		Singleton<LevelBatchManager>.SP.GetAllItemVotes();
	}

	public void GiveCurrentItemAVote(bool upvote)
	{
		if (!(currentObj == null))
		{
			string text = currentObj.GetComponent<UIButtonMessageArguments>().GetStrings()[1];
			if (text != string.Empty)
			{
				Singleton<Workshop>.SP.SetItemVote(Singleton<LevelBatchManager>.SP.GetPublishedFileIDFromGuid(text), upvote);
			}
		}
	}

	public void SetItemVoteOnObject(RemoteStorageUserVoteDetails_t pCallback)
	{
		for (int i = 0; i < levelButtons.Count; i++)
		{
			string guid = levelButtons[i].GetComponent<UIButtonMessageArguments>().GetStrings()[1];
			if (pCallback.m_nPublishedFileId.m_PublishedFileId == Singleton<LevelBatchManager>.SP.GetPublishedFileIDFromGuid(guid))
			{
				SetVoteOnButton(levelButtons[i]);
			}
		}
	}

	public void SetVoteOnButton(GameObject button)
	{
		string text = button.GetComponent<UIButtonMessageArguments>().GetStrings()[1];
		if (text == string.Empty)
		{
			button.GetComponentInChildren<UISprite>().spriteName = string.Empty;
			button.GetComponentInChildren<UISprite>().enabled = false;
			return;
		}
		Level levelFromGuid = Singleton<LevelBatchManager>.SP.GetLevelFromGuid(text);
		if (!(levelFromGuid == null))
		{
			UISprite componentInChildren = button.GetComponentInChildren<UISprite>();
			switch (levelFromGuid.workshopVote)
			{
			case EWorkshopVote.k_EWorkshopVoteUnvoted:
				componentInChildren.enabled = false;
				componentInChildren.spriteName = string.Empty;
				break;
			case EWorkshopVote.k_EWorkshopVoteFor:
				componentInChildren.enabled = true;
				componentInChildren.spriteName = "up_vote_icon";
				break;
			case EWorkshopVote.k_EWorkshopVoteAgainst:
				componentInChildren.enabled = true;
				componentInChildren.spriteName = "down_vote_icon";
				break;
			}
		}
	}

	public void DeleteCurrentItem()
	{
		if (currentObj == null)
		{
			return;
		}
		string text = currentObj.GetComponent<UIButtonMessageArguments>().GetStrings()[1];
		if (text == string.Empty)
		{
			return;
		}
		Level levelFromGuid = Singleton<LevelBatchManager>.SP.GetLevelFromGuid(text);
		if (levelFromGuid != null)
		{
			Singleton<Workshop>.SP.UnsubscribeFrom(levelFromGuid);
			if (levelFromGuid.workshopFolderName != string.Empty && Directory.Exists(levelFromGuid.workshopFolderName))
			{
				Directory.Delete(levelFromGuid.workshopFolderName, recursive: true);
			}
			Singleton<LevelBatchManager>.SP.RemoveLevel(levelFromGuid);
		}
		AudioController.Play("inbox_messageremove");
		GameObject obj = Singleton<InputManager>.SP.GetCurrentButton().gameObject;
		obj.GetComponentInChildren<UISprite>().enabled = false;
		obj.GetComponent<UIButtonMessage>().functionName = string.Empty;
		obj.GetComponentInChildren<UILabel>().text = string.Empty;
		obj.GetComponent<UIButtonMessageArguments>().GetStrings()[0] = string.Empty;
		obj.GetComponent<UIButtonMessageArguments>().GetStrings()[1] = string.Empty;
		Singleton<InputManager>.SP.Deselect();
		RefreshButtonList();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
	}

	public void SetInputObjects(int numUsedButtons)
	{
		for (int i = 0; i < numUsedButtons; i++)
		{
			InputObject component = levelButtons[i].GetComponent<InputObject>();
			if (i < numUsedButtons - 1)
			{
				component.selectOnDown = levelButtons[i + 1].GetComponent<InputObject>();
			}
			else
			{
				component.selectOnDown = levelButtons[0].GetComponent<InputObject>();
			}
			if (i > 0)
			{
				component.selectOnUp = levelButtons[i - 1].GetComponent<InputObject>();
			}
			else
			{
				component.selectOnUp = levelButtons[numUsedButtons - 1].GetComponent<InputObject>();
			}
		}
	}

	public void Button_StartLevel(Object value)
	{
		UIButtonMessageArguments component = (value as GameObject).GetComponent<UIButtonMessageArguments>();
		Level levelFromLevelNameAndGUID = Singleton<LevelBatchManager>.SP.GetLevelFromLevelNameAndGUID(component.GetStrings()[0], component.GetStrings()[1]);
		if (!(levelFromLevelNameAndGUID == null))
		{
			Singleton<GamestateManager>.SP.SetCurrentGameMode(GameMode.PlayWorkshopLevel);
			AudioController.Play("LevelStart");
			Singleton<LevelBatchManager>.SP.LoadLevelSceneless(levelFromLevelNameAndGUID);
		}
	}
}
