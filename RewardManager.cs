using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
	public enum UnlockType
	{
		Character,
		ChallengeLevel,
		BonusLevel,
		LevelBundle
	}

	public PlayerGraphics characterReward;

	public bool showingReward;

	private bool canHideReward;

	public List<Reward> rewardQueue = new List<Reward>();

	private bool canTakeRewardFromQueue = true;

	public bool disableRewardQueue;

	public List<GameObject> rewardObjects;

	public List<UITweener> rewardTweens;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.anyKeyDown && showingReward && canHideReward)
		{
			HideRewardScreen();
		}
		if (showingReward)
		{
			Singleton<InputManager>.SP.inputEnabled = false;
		}
		if (!disableRewardQueue && rewardQueue.Count > 0 && canTakeRewardFromQueue)
		{
			canTakeRewardFromQueue = false;
			TakeRewardFromQueue();
		}
	}

	private void TakeRewardFromQueue()
	{
		Reward reward = rewardQueue[0];
		rewardQueue.Remove(reward);
		ShowRewardScreen(reward);
	}

	public void AddRewardtoQueue(Reward reward)
	{
		rewardQueue.Add(reward);
	}

	public void PopUpReward(UnlockType type, CharacterSelect.Characters character, int skinNum)
	{
		if (type == UnlockType.Character)
		{
			AddRewardtoQueue(new Reward
			{
				unlockType = type,
				character = character,
				skinNum = skinNum
			});
		}
	}

	public void PopUpReward(UnlockType type, Level levelObj)
	{
		AddRewardtoQueue(new Reward
		{
			unlockType = type,
			level = levelObj
		});
	}

	public void PopUpReward(UnlockType type, LevelBatch batch)
	{
		AddRewardtoQueue(new Reward
		{
			unlockType = type,
			batch = batch
		});
	}

	public void ShowRewardScreen(Reward reward)
	{
		Singleton<InputManager>.SP.inputEnabled = false;
		canHideReward = false;
		StartCoroutine(ShowRewardRoutine(reward));
		AudioController.Play("Toeter");
		AudioController.Play("crowd");
	}

	private void HideRewardScreen()
	{
		showingReward = false;
		AudioController.Play("ButtonForwards");
	}

	private void ShowLevelUnlockPopup(Level level)
	{
		UILabel batchLevelInfoLabel = Singleton<PermaGUI>.SP.batchLevelInfoLabel;
		string text = Language.Get("REWARD_LEVEL", "PERMA").Replace("{X}", level.levelName);
		Singleton<PermaGUI>.SP.batchLevelInfoLabel.text = text;
		batchLevelInfoLabel.text = text;
		Texture texture = null;
		texture = Resources.Load("LevelScreens/" + level.levelCode) as Texture;
		Singleton<PermaGUI>.SP.batchLevelImagePlane.renderer.material.mainTexture = texture;
		Singleton<PermaGUI>.SP.batchLevelUnlock.SetActive(value: true);
	}

	private void ShowBatchUnlockPopup(LevelBatch batch)
	{
		Singleton<PermaGUI>.SP.batchLevelInfoLabel.text = Language.Get("REWARD_BATCH", "PERMA").Replace("{X}", batch.batchName);
		Texture texture = null;
		texture = Resources.Load("LevelScreens/" + batch.levels[0].levelCode) as Texture;
		Singleton<PermaGUI>.SP.batchLevelImagePlane.renderer.material.mainTexture = texture;
		Singleton<PermaGUI>.SP.batchLevelUnlock.SetActive(value: true);
	}

	private void ShowCharacterUnlockPopup(CharacterSelect.Characters character, int skinNum)
	{
		if (skinNum == 0)
		{
			Singleton<PermaGUI>.SP.characterInfoLabel.text = Language.Get("REWARD_CHARACTER", "PERMA") + " " + Singleton<UnlockManager>.SP.GetCharacterName(character, skinNum) + "!";
		}
		else
		{
			Singleton<PermaGUI>.SP.characterInfoLabel.text = Language.Get("REWARD_SKIN", "PERMA") + " " + Singleton<UnlockManager>.SP.GetCharacterName(character, skinNum) + "!";
		}
		characterReward.gameObject.SetActive(value: true);
		Singleton<PermaGUI>.SP.characterUnlock.SetActive(value: true);
		characterReward.SetModel(character, skinNum);
		Singleton<AudioManager>.SP.PlayCharacterIntro(character, skinNum);
	}

	private IEnumerator ShowRewardRoutine(Reward reward)
	{
		showingReward = true;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < rewardObjects.Count; i++)
		{
			rewardObjects[i].SetActive(value: false);
		}
		for (int j = 0; j < rewardTweens.Count; j++)
		{
			rewardTweens[j].ResetToBeginning();
			rewardTweens[j].Play(forward: true);
		}
		if (reward.unlockType == UnlockType.BonusLevel)
		{
			ShowLevelUnlockPopup(reward.level);
		}
		else if (reward.unlockType == UnlockType.ChallengeLevel)
		{
			ShowLevelUnlockPopup(reward.level);
		}
		else if (reward.unlockType == UnlockType.Character)
		{
			ShowCharacterUnlockPopup(reward.character, reward.skinNum);
			while (characterReward.spawningModel)
			{
				yield return new WaitForEndOfFrame();
			}
			characterReward.animatedModel.transform.localScale = new Vector3(2f, 2f, 2f);
			HenkUtils.SetLayerRecursively(characterReward.gameObject, "3DGUI");
		}
		else if (reward.unlockType == UnlockType.LevelBundle)
		{
			ShowBatchUnlockPopup(reward.batch);
		}
		yield return new WaitForSeconds(0.8f);
		for (int k = 0; k < rewardObjects.Count; k++)
		{
			rewardObjects[k].SetActive(value: true);
		}
		AudioController.GetAudioItem("PickupCoin").subItems[0].PitchShift = 0f;
		AudioController.Play("PickupCoin");
		yield return new WaitForSeconds(0.25f);
		canHideReward = true;
		while (showingReward)
		{
			yield return new WaitForEndOfFrame();
		}
		HideAllUnlockScreens();
		characterReward.gameObject.SetActive(value: false);
		canHideReward = false;
		canTakeRewardFromQueue = true;
		if (rewardQueue.Count < 1)
		{
			yield return new WaitForSeconds(0.1f);
			Singleton<InputManager>.SP.inputEnabled = true;
			Singleton<InputManager>.SP.DisableInputTillEndOfFrame();
		}
	}

	private void HideAllUnlockScreens()
	{
		Singleton<PermaGUI>.SP.batchLevelUnlock.SetActive(value: false);
		Singleton<PermaGUI>.SP.characterUnlock.SetActive(value: false);
		characterReward.gameObject.SetActive(value: false);
	}
}
