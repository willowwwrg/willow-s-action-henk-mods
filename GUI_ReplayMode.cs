using System.Collections.Generic;
using Henk;
using UnityEngine;

public class GUI_ReplayMode : MonoBehaviour
{
	public UILabel textLabel;

	public bool demoMode;

	public ReplayController replayController;

	public UISprite upArrow;

	public UISprite downArrow;

	public UISprite leftArrow;

	public UISprite rightArrow;

	public UISprite jumpKey;

	public UISprite abilityKey;

	public UILabel timeLabel;

	public List<GameObject> interfaceGroups;

	private int currentInterfaceState;

	private void TransitionCompleted()
	{
		replayController = null;
		SetInterfaceState(0);
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
		{
			SetInterfaceState(3);
			Singleton<GamestateManager>.SP.SetState(typeof(State_LevelSelectCampaign));
		}
		if ((bool)replayController)
		{
			SetSpriteState(jumpKey, replayController.GetCurrentReplayFrame().jumpInput);
			SetSpriteState(abilityKey, replayController.GetCurrentReplayFrame().abilityInput);
			SetSpriteState(downArrow, replayController.GetCurrentReplayFrame().slideInput);
			float walkInput = replayController.GetCurrentReplayFrame().walkInput;
			if (walkInput > 0f)
			{
				SetSpriteState(rightArrow, state: true);
				SetSpriteState(leftArrow, state: false);
			}
			else if (walkInput < 0f)
			{
				SetSpriteState(rightArrow, state: false);
				SetSpriteState(leftArrow, state: true);
			}
			else
			{
				SetSpriteState(rightArrow, state: false);
				SetSpriteState(leftArrow, state: false);
			}
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			CycleInterface();
		}
		timeLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Singleton<Stopwatch>.SP.GetCurrentTime());
	}

	private void CycleInterface()
	{
		currentInterfaceState++;
		if (currentInterfaceState > 3)
		{
			currentInterfaceState = 0;
		}
		SetInterfaceState(currentInterfaceState);
	}

	private void SetInterfaceState(int state)
	{
		currentInterfaceState = state;
		switch (state)
		{
		case 0:
		{
			foreach (GameObject interfaceGroup in interfaceGroups)
			{
				interfaceGroup.SetActive(value: true);
			}
			break;
		}
		case 1:
			interfaceGroups[0].SetActive(value: true);
			interfaceGroups[1].SetActive(value: true);
			interfaceGroups[2].SetActive(value: false);
			break;
		case 2:
			interfaceGroups[0].SetActive(value: false);
			interfaceGroups[1].SetActive(value: true);
			interfaceGroups[2].SetActive(value: false);
			break;
		case 3:
		{
			foreach (GameObject interfaceGroup2 in interfaceGroups)
			{
				interfaceGroup2.SetActive(value: false);
			}
			break;
		}
		}
	}

	private void SetSpriteState(UISprite sprite, bool state)
	{
		if (state)
		{
			sprite.color = Color.green;
		}
		else
		{
			sprite.color = Color.white;
		}
	}
}
