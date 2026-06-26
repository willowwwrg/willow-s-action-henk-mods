using System.Collections.Generic;
using UnityEngine;

public class GUI_BatchSelectExtraLevels : GUI_Base
{
	public InputObject firstButton;

	public UILabel batchNameLabel;

	public List<GameObject> batchButtons;

	public UILabel unlockCriteriaLabel;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<LevelBatchManager>.SP.BatchUnlockCheck();
		for (int i = 0; i < batchButtons.Count; i++)
		{
			batchButtons[i].SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
			batchButtons[i].GetComponent<GUIBatchImage>().InitImage();
		}
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		InitMedals();
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_BatchSelect));
		AudioController.Play("ButtonBackwards");
	}

	private void Update()
	{
		if (!Singleton<PermaGUI>.SP.inbox.activeInHierarchy && Singleton<InputManager>.SP.CheckAction(InputAction.SwitchCharacter) && !Singleton<PermaGUI>.SP.confirmationRequestUp)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_CharacterSelectionCampaign));
			AudioController.Play("ButtonForwards");
		}
	}

	public void Button_Batch(object value)
	{
		int batchNum = (value as GameObject).GetComponent<GUIBatchImage>().batchNum;
		LevelBatch batchFromNum = Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum);
		if (batchFromNum != null)
		{
			if (batchFromNum.IsUnlocked())
			{
				Singleton<LevelBatchManager>.SP.lookingAtBatchNum = batchNum;
				Singleton<GamestateManager>.SP.SetState(typeof(State_LevelSelectCampaign));
				AudioController.Play("ButtonForwards");
			}
			else
			{
				AudioController.Play("ButtonClick");
			}
		}
		else
		{
			AudioController.Play("ButtonClick");
		}
	}

	public void InitMedals()
	{
		for (int i = 0; i < batchButtons.Count; i++)
		{
			GUIBatchImage component = batchButtons[i].GetComponent<GUIBatchImage>();
			LevelBatch batchFromNum = Singleton<LevelBatchManager>.SP.GetBatchFromNum(component.batchNum);
			if (batchFromNum.IsUnlocked())
			{
				component.bgBar.SetActive(value: true);
				for (int j = 0; j < 7; j++)
				{
					if (j > batchFromNum.levels.Count - 1)
					{
						HenkUtils.FindTransformInHierarchy(batchButtons[i].transform, "medal" + j).GetComponent<UISprite>().spriteName = "none";
					}
					else if (batchFromNum.levels[j].levelType == LevelType.Standard)
					{
						HenkUtils.FindTransformInHierarchy(batchButtons[i].transform, "medal" + j).GetComponent<UISprite>().spriteName = GetSpriteNameFromMedal(batchFromNum.levels[j].bestMedal);
					}
					else if (batchFromNum.levels[j].levelType == LevelType.Challenge || batchFromNum.levels[j].levelType == LevelType.Bonus)
					{
						if (batchFromNum.levels[j].IsChallengeCompleted())
						{
							HenkUtils.FindTransformInHierarchy(batchButtons[i].transform, "medal" + j).GetComponent<UISprite>().spriteName = "challenge_icon";
						}
						else
						{
							HenkUtils.FindTransformInHierarchy(batchButtons[i].transform, "medal" + j).GetComponent<UISprite>().spriteName = "none";
						}
					}
				}
			}
			else
			{
				component.bgBar.SetActive(value: false);
				for (int k = 0; k < 7; k++)
				{
					HenkUtils.FindTransformInHierarchy(batchButtons[i].transform, "medal" + k).GetComponent<UISprite>().spriteName = "none";
				}
			}
		}
	}

	private string GetSpriteNameFromMedal(Medal medal)
	{
		return medal switch
		{
			Medal.Bronze => "bronze", 
			Medal.Silver => "silver", 
			Medal.Gold => "gold", 
			Medal.Rainbow => "rainbow", 
			Medal.None => "none", 
			_ => string.Empty, 
		};
	}

	public void HoverOverBatchButton(GameObject button)
	{
		int batchNum = button.GetComponent<GUIBatchImage>().batchNum;
		LevelBatch batchFromNum = Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum);
		if (batchFromNum != null)
		{
			if (batchFromNum.IsUnlocked())
			{
				batchNameLabel.text = batchFromNum.batchName;
			}
			else
			{
				batchNameLabel.text = "??????";
			}
		}
		else
		{
			batchNameLabel.text = string.Empty;
		}
		firstButton = button.GetComponent<InputObject>();
		if (batchFromNum.IsUnlocked())
		{
			unlockCriteriaLabel.transform.parent.GetComponent<UITweener>().Play(forward: false);
			return;
		}
		bool flag = true;
		bool flag2 = false;
		if (batchFromNum.numMedalsToUnlock > Singleton<LevelBatchManager>.SP.MedalCount())
		{
			flag2 = true;
		}
		if (batchFromNum.unlockDependancy != null)
		{
			flag = Singleton<LevelBatchManager>.SP.GetBatchFromLevel(batchFromNum.unlockDependancy).CheckChallengeLevelUnlocked();
		}
		if (flag2 && flag)
		{
			unlockCriteriaLabel.text = Language.Get("UNLOCKCRITERIAMEDALS", "BATCHSELECT").Replace("{X}", batchFromNum.numMedalsToUnlock.ToString());
		}
		else if (!flag2 && !flag)
		{
			unlockCriteriaLabel.text = Language.Get("UNLOCKCRITERIACHALLENGELEVEL", "BATCHSELECT").Replace("{X}", batchFromNum.unlockDependancy.levelName);
		}
		else
		{
			unlockCriteriaLabel.text = Language.Get("UNLOCKCRITERIABOTH", "BATCHSELECT").Replace("{X}", batchFromNum.unlockDependancy.levelName).Replace("{Y}", batchFromNum.numMedalsToUnlock.ToString());
		}
		unlockCriteriaLabel.transform.parent.GetComponent<UITweener>().Play(forward: true);
	}
}
