using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_LevelSelectCampaign : GUI_Base
{
	public InputObject FirstButton;

	private InputObject nextLevelButton;

	public GUI_LevelSelectLeaderboards gui_levelselectleaderboards;

	public GameObject SelectedLevelButton;

	private LevelBatch currentBatch;

	public UILabel pbLabel;

	public UILabel difficultyLabel;

	public GameObject imagePlane;

	private Dictionary<int, Texture> levelScreenshots = new Dictionary<int, Texture>();

	public List<GameObject> buttons;

	public GameObject subTitleObj;

	public UILabel titleLabel;

	public List<UISprite> batchScrollIndicators;

	private Vector3 originalScrollIndicatorsPos;

	public List<GameObject> LeftRightButtons;

	private void Awake()
	{
		originalScrollIndicatorsPos = batchScrollIndicators[0].transform.parent.localPosition;
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		Singleton<GamestateManager>.SP.SetState(typeof(State_BatchSelect));
		AudioController.Play("ButtonBackwards");
	}

	public void HoverOverLevelSelectButton(GameObject button)
	{
		Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode(button.GetComponent<UIButtonMessageArguments>().GetInts()[0]);
		difficultyLabel.text = Language.Get(levelFromCode.difficulty.ToString().ToUpper(), "LEVELSELECT");
		if (!levelScreenshots.ContainsKey(levelFromCode.levelCode))
		{
			Texture texture = null;
			texture = Resources.Load("LevelScreens/" + levelFromCode.levelCode) as Texture;
			if (texture != null)
			{
				levelScreenshots.Add(levelFromCode.levelCode, texture);
			}
		}
		if (levelScreenshots.ContainsKey(levelFromCode.levelCode))
		{
			imagePlane.renderer.material.mainTexture = levelScreenshots[levelFromCode.levelCode];
			imagePlane.SetActive(value: true);
		}
		else
		{
			imagePlane.SetActive(value: false);
		}
		pbLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Singleton<PlayerPrefsManager>.SP.GetHighscore(levelFromCode));
	}

	public void UpdateBatchScrollIndicators()
	{
		int batchNum = Singleton<LevelBatchManager>.SP.GetBatchNum(currentBatch);
		if (batchNum > 9)
		{
			for (int i = 0; i < batchScrollIndicators.Count; i++)
			{
				batchScrollIndicators[i].alpha = 0f;
			}
			ToggleLeftRightButtons(left: true, right: true);
			return;
		}
		for (int j = 0; j < batchScrollIndicators.Count; j++)
		{
			batchScrollIndicators[j].alpha = 0.2f;
		}
		batchScrollIndicators[batchNum].alpha = 1f;
		int num = 0;
		for (int k = 0; k < Singleton<LevelBatchManager>.SP.batches.Count; k++)
		{
			if (k <= 9)
			{
				if (!Singleton<LevelBatchManager>.SP.batches[k].IsUnlocked())
				{
					batchScrollIndicators[k].GetComponent<UISprite>().enabled = false;
					num++;
				}
				else
				{
					batchScrollIndicators[k].GetComponent<UISprite>().enabled = true;
				}
			}
		}
		batchScrollIndicators[0].transform.parent.localPosition = originalScrollIndicatorsPos;
		batchScrollIndicators[0].transform.parent.localPosition = originalScrollIndicatorsPos + new Vector3(12.5f * (float)num, 0f, 0f);
		bool left = false;
		bool right = false;
		if ((bool)Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum + 1))
		{
			if (!Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum + 1).IsUnlocked())
			{
				left = true;
			}
		}
		else
		{
			left = true;
		}
		if ((bool)Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum - 1))
		{
			if (!Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum - 1).IsUnlocked())
			{
				right = true;
			}
		}
		else
		{
			right = true;
		}
		ToggleLeftRightButtons(left, right);
	}

	private void ToggleLeftRightButtons(bool left, bool right)
	{
		UISprite[] componentsInChildren = LeftRightButtons[0].GetComponentsInChildren<UISprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = !left;
		}
		componentsInChildren = LeftRightButtons[1].GetComponentsInChildren<UISprite>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].enabled = !right;
		}
	}

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<LevelBatchManager>.SP.RefreshLevels();
		if (currentBatch != null && currentBatch.batchName != Singleton<LevelBatchManager>.SP.GetBatchFromNum(Singleton<LevelBatchManager>.SP.lookingAtBatchNum).batchName)
		{
			FirstButton = buttons[0].GetComponent<InputObject>();
		}
		currentBatch = Singleton<LevelBatchManager>.SP.GetBatchFromNum(Singleton<LevelBatchManager>.SP.lookingAtBatchNum);
		UpdateBatchScrollIndicators();
		SetLevelLabels();
		SetInputObjects();
		int num = PlayerPrefs.GetInt("MOSTRECENTLYCOMPLETEDLEVEL", 0);
		if (num != 0)
		{
			int num2 = -1;
			for (int i = 0; i < buttons.Count; i++)
			{
				if (num == buttons[i].GetComponent<UIButtonMessageArguments>().GetInts()[0])
				{
					num2 = i;
				}
			}
			if (num2 != -1 && num2 + 1 < buttons.Count && buttons[num2 + 1].activeInHierarchy)
			{
				FirstButton = buttons[num2 + 1].GetComponent<InputObject>();
			}
		}
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		Singleton<RewardManager>.SP.disableRewardQueue = false;
	}

	private IEnumerator SelectNextLevelDelayed()
	{
		Singleton<InputManager>.SP.inputEnabled = false;
		yield return new WaitForSeconds(0.3f);
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		Singleton<InputManager>.SP.inputEnabled = true;
	}

	private void GotoNextBatch(bool forwards = true)
	{
		if (forwards)
		{
			if (Singleton<LevelBatchManager>.SP.lookingAtBatchNum + 1 >= Singleton<LevelBatchManager>.SP.batches.Count)
			{
				AudioController.Play("ButtonClick");
				return;
			}
			if (!Singleton<LevelBatchManager>.SP.batches[Singleton<LevelBatchManager>.SP.lookingAtBatchNum + 1].IsUnlocked())
			{
				AudioController.Play("ButtonClick");
				return;
			}
			Singleton<LevelBatchManager>.SP.lookingAtBatchNum++;
			AudioController.Play("NextMenuItem");
		}
		else
		{
			if (Singleton<LevelBatchManager>.SP.lookingAtBatchNum - 1 < 0)
			{
				AudioController.Play("ButtonClick");
				return;
			}
			if (!Singleton<LevelBatchManager>.SP.batches[Singleton<LevelBatchManager>.SP.lookingAtBatchNum - 1].IsUnlocked())
			{
				AudioController.Play("ButtonClick");
				return;
			}
			Singleton<LevelBatchManager>.SP.lookingAtBatchNum--;
			AudioController.Play("PrevMenuItem");
		}
		currentBatch = Singleton<LevelBatchManager>.SP.GetBatchFromNum(Singleton<LevelBatchManager>.SP.lookingAtBatchNum);
		currentBatch.playUnlockAnim = false;
		FirstButton = buttons[0].GetComponent<InputObject>();
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
		SetLevelLabels();
		SetInputObjects();
		HoverOverLevelSelectButton(buttons[0]);
		UpdateBatchScrollIndicators();
	}

	private void SetLevelLabels()
	{
		titleLabel.text = currentBatch.batchName;
		for (int i = 0; i < buttons.Count; i++)
		{
			if (i > currentBatch.levels.Count - 1)
			{
				buttons[i].SetActive(value: false);
				continue;
			}
			if (currentBatch.levels[i].levelType == LevelType.Challenge && i == 5)
			{
				UISprite[] componentsInChildren = HenkUtils.FindTransformInHierarchy(buttons[i].transform, "lock").gameObject.GetComponentsInChildren<UISprite>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].enabled = !currentBatch.CheckChallengeLevelUnlocked();
				}
			}
			if (i == 6 && !currentBatch.CheckBonusLevelUnlocked())
			{
				buttons[i].SetActive(value: false);
				continue;
			}
			buttons[i].SetActive(value: true);
			buttons[i].GetComponentInChildren<UILabel>().text = currentBatch.levels[i].levelName;
			buttons[i].GetComponent<UIButtonMessageArguments>().GetInts()[0] = currentBatch.levels[i].levelCode;
			Medal medalsEarned = (Medal)Singleton<PlayerPrefsManager>.SP.GetMedalsEarned(currentBatch.levels[i]);
			if (currentBatch.levels[i].levelType == LevelType.Bonus || currentBatch.levels[i].levelType == LevelType.Challenge)
			{
				HenkUtils.FindTransformInHierarchy(buttons[i].transform, "medalChallenge").GetComponent<UISprite>().enabled = false;
			}
			switch (medalsEarned)
			{
			case Medal.Bronze:
				ToggleMedals(buttons[i].transform, bronze: true, silver: false, gold: false, rainbow: false);
				if (currentBatch.levels[i].levelType == LevelType.Bonus || currentBatch.levels[i].levelType == LevelType.Challenge)
				{
					ToggleMedals(buttons[i].transform, bronze: false, silver: false, gold: false, rainbow: false);
					HenkUtils.FindTransformInHierarchy(buttons[i].transform, "medalChallenge").GetComponent<UISprite>().enabled = true;
					PlayTweens(buttons[i], currentBatch.levels[i], challengeBonus: true);
				}
				else
				{
					PlayTweens(buttons[i], currentBatch.levels[i]);
				}
				break;
			case Medal.Silver:
				ToggleMedals(buttons[i].transform, bronze: true, silver: true, gold: false, rainbow: false);
				PlayTweens(buttons[i], currentBatch.levels[i]);
				break;
			case Medal.Gold:
				ToggleMedals(buttons[i].transform, bronze: true, silver: true, gold: true, rainbow: false);
				PlayTweens(buttons[i], currentBatch.levels[i]);
				break;
			case Medal.Rainbow:
				ToggleMedals(buttons[i].transform, bronze: true, silver: true, gold: true, rainbow: true);
				PlayTweens(buttons[i], currentBatch.levels[i]);
				break;
			case Medal.None:
				ToggleMedals(buttons[i].transform, bronze: false, silver: false, gold: false, rainbow: false);
				PlayTweens(buttons[i], currentBatch.levels[i]);
				break;
			default:
				ToggleMedals(buttons[i].transform, bronze: false, silver: false, gold: false, rainbow: false);
				PlayTweens(buttons[i], currentBatch.levels[i]);
				break;
			}
		}
	}

	private void ToggleMedals(Transform parentTransform, bool bronze, bool silver, bool gold, bool rainbow)
	{
		GameObject gameObject = parentTransform.gameObject;
		gameObject.GetComponent<LevelSelectButton>().buttons[0].GetComponent<UISprite>().enabled = bronze;
		gameObject.GetComponent<LevelSelectButton>().buttons[1].GetComponent<UISprite>().enabled = silver;
		gameObject.GetComponent<LevelSelectButton>().buttons[2].GetComponent<UISprite>().enabled = gold;
		gameObject.GetComponent<LevelSelectButton>().buttons[3].GetComponent<UISprite>().enabled = rainbow;
		for (int i = 0; i < 4; i++)
		{
			if (gameObject.GetComponent<LevelSelectButton>().buttons[i].GetComponent<UISprite>().enabled)
			{
				UITweener[] components = gameObject.GetComponent<LevelSelectButton>().buttons[i].GetComponents<UITweener>();
				foreach (UITweener obj in components)
				{
					obj.ResetToEnd();
					obj.enabled = false;
				}
			}
		}
	}

	private void PlayTweens(GameObject button, Level level, bool challengeBonus = false)
	{
		if (level.newMedals < 1)
		{
			return;
		}
		int newMedals = level.newMedals;
		level.newMedals = 0;
		AudioController.GetAudioItem("PickupCoin").subItems[0].PitchShift = 0f;
		if (challengeBonus)
		{
			UITweener[] components = button.GetComponent<LevelSelectButton>().buttons[4].GetComponents<UITweener>();
			foreach (UITweener obj in components)
			{
				obj.delay = 0.5f;
				obj.ResetToBeginning();
				obj.Play(forward: true);
			}
			return;
		}
		int num = (int)(level.bestMedal - newMedals);
		float num2 = 0.5f;
		for (int j = num; j < num + newMedals; j++)
		{
			UITweener[] components = button.GetComponent<LevelSelectButton>().buttons[j].GetComponents<UITweener>();
			foreach (UITweener obj2 in components)
			{
				obj2.delay = num2;
				obj2.ResetToBeginning();
				obj2.Play(forward: true);
				num2 += 0.15f;
			}
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
			if (!buttons[j].activeInHierarchy)
			{
				continue;
			}
			InputObject component2 = buttons[j].GetComponent<InputObject>();
			if (j < buttons.Count - 1)
			{
				if (buttons[j + 1].activeInHierarchy)
				{
					component2.selectOnDown = buttons[j + 1].GetComponent<InputObject>();
				}
				else
				{
					component2.selectOnDown = buttons[0].GetComponent<InputObject>();
				}
			}
			else
			{
				component2.selectOnDown = buttons[0].GetComponent<InputObject>();
			}
			if (j > 0)
			{
				component2.selectOnUp = buttons[j - 1].GetComponent<InputObject>();
			}
			else
			{
				component2.selectOnUp = buttons[GetLastPlayableLevelInCurrentBatch()].GetComponent<InputObject>();
			}
		}
	}

	private int GetLastPlayableLevelInCurrentBatch()
	{
		if (currentBatch.levels[currentBatch.levels.Count - 1].bonusTime != 0f)
		{
			if (currentBatch.CheckBonusLevelUnlocked())
			{
				return currentBatch.levels.Count - 1;
			}
			return currentBatch.levels.Count - 2;
		}
		return currentBatch.levels.Count - 1;
	}

	public void Button_Leaderboards()
	{
		AudioController.Play("ButtonForwards");
		gui_levelselectleaderboards.selectedLevel = Singleton<LevelBatchManager>.SP.GetLevelFromCode(Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetInts()[0]);
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LevelSelectLeaderboards, "none");
	}

	private void Update()
	{
		if (Singleton<PermaGUI>.SP.inbox.activeInHierarchy)
		{
			return;
		}
		if (Singleton<LevelBatchManager>.SP.GetBatchNum(currentBatch) < 10)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.NextCharacter))
			{
				GotoNextBatch();
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.PreviousCharacter))
			{
				GotoNextBatch(forwards: false);
			}
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.SwitchCharacter) && !Singleton<PermaGUI>.SP.confirmationRequestUp)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_CharacterSelectionCampaign));
			AudioController.Play("ButtonForwards");
		}
		Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode(Singleton<InputManager>.SP.GetCurrentButton().GetComponent<UIButtonMessageArguments>().GetInts()[0]);
		if (levelFromCode.levelType == LevelType.Challenge)
		{
			if (!currentBatch.CheckChallengeLevelUnlocked())
			{
				ToggleSubTitle(state: true, Language.Get("UNLOCKCHALLENGE", "LEVELSELECT"));
			}
			else if (levelFromCode.IsChallengeCompleted())
			{
				ToggleSubTitle(state: true, Language.Get("UNLOCKEDCHARACTER", "LEVELSELECT").Replace("{X}", levelFromCode.GetChallengerName()));
			}
			else
			{
				ToggleSubTitle(state: true, Language.Get("UNLOCKCHARACTER", "LEVELSELECT").Replace("{X}", levelFromCode.GetChallengerName()));
			}
		}
		else if (levelFromCode.levelType == LevelType.Bonus)
		{
			if (!currentBatch.CheckBonusLevelUnlocked())
			{
				ToggleSubTitle(state: true, Language.Get("UNLOCKBONUS", "LEVELSELECT"));
			}
			else if (levelFromCode.IsChallengeCompleted())
			{
				ToggleSubTitle(state: true, Language.Get("UNLOCKEDCHARACTER", "LEVELSELECT").Replace("{X}", levelFromCode.GetChallengerName()));
			}
			else
			{
				ToggleSubTitle(state: true, Language.Get("UNLOCKCHARACTER", "LEVELSELECT").Replace("{X}", levelFromCode.GetChallengerName()));
			}
		}
		else if (levelFromCode.bestMedal == Medal.None || levelFromCode.bestMedal == Medal.Bronze || levelFromCode.bestMedal == Medal.Silver || levelFromCode.bestMedal == Medal.Gold)
		{
			string text = string.Empty;
			switch (levelFromCode.bestMedal)
			{
			case Medal.None:
				text = Language.Get("BRONZE", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(levelFromCode.bronzeTime));
				break;
			case Medal.Bronze:
				text = Language.Get("SILVER", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(levelFromCode.silverTime));
				break;
			case Medal.Silver:
				text = Language.Get("GOLD", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(levelFromCode.goldTime));
				break;
			case Medal.Gold:
				text = Language.Get("RAINBOW", "LEVELSELECT").Replace("{X}", Singleton<HighscoreManager>.SP.ConvertTimeToString(levelFromCode.rainbowTime));
				break;
			}
			ToggleSubTitle(state: true, text);
			if (levelFromCode.wipState == WIPState.Draft)
			{
				ToggleSubTitle(state: true, "Work in progress level, no medals here.");
			}
		}
		else
		{
			ToggleSubTitle(state: false, string.Empty);
		}
		if (Singleton<InputManager>.SP.CheckAction(InputAction.LevelSelectLeaderboard))
		{
			if (!SteamManager.Initialized)
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDSTEAMCONNECT", "PERMA"));
				return;
			}
			FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
			Button_Leaderboards();
		}
	}

	private void ToggleSubTitle(bool state, string text)
	{
		if (subTitleObj.GetComponentInChildren<UILabel>() != null)
		{
			subTitleObj.GetComponentInChildren<UILabel>().text = text;
		}
		subTitleObj.GetComponent<UITweener>().Play(state);
	}

	public void LevelButton(object value)
	{
		if (!acceptInput)
		{
			return;
		}
		Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode((value as GameObject).GetComponent<UIButtonMessageArguments>().GetInts()[0]);
		if (!Singleton<LevelBatchManager>.SP.GetPlayableLevels().Contains(levelFromCode))
		{
			AudioController.Play("ButtonClick");
			return;
		}
		FirstButton = Singleton<InputManager>.SP.GetCurrentButton();
		AudioController.Play("LevelStart");
		if (levelFromCode.isSceneLess)
		{
			Singleton<LevelBatchManager>.SP.LoadLevelSceneless(levelFromCode);
		}
		else
		{
			Singleton<LevelBatchManager>.SP.LoadLevel(levelFromCode.levelCode);
		}
	}
}
