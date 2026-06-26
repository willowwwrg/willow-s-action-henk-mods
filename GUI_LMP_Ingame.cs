using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_LMP_Ingame : GUI_Base
{
	public GameObject pauseMenu;

	public InputObject firstButton;

	public bool menuActive;

	private float timeScaleBeforePause = 1f;

	public State_LMP_Ingame stateObj;

	public List<GameObject> offscreenIndicators;

	public List<UISprite> radialIndicators;

	public GameObject respawnIndicator;

	public UILabel countdownLabel;

	private void TransitionCompleted()
	{
		for (int i = 0; i < offscreenIndicators.Count; i++)
		{
			offscreenIndicators[i].SetActive(value: false);
		}
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Start))
		{
			if (menuActive)
			{
				Button_Resume();
				AudioController.Play("ButtonForwards");
			}
			else
			{
				ToggleMenu(state: true);
				AudioController.Play("ButtonBackwards");
			}
		}
		if (stateObj.timeLeftToFinish != stateObj.timeLeftAfterFirstFinish)
		{
			countdownLabel.transform.parent.GetComponent<TweenPosition>().Play(forward: true);
			float num = stateObj.timeLeftToFinish;
			if (num < 0f)
			{
				num = 0f;
			}
			countdownLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(num);
		}
		else
		{
			countdownLabel.transform.parent.GetComponent<TweenPosition>().Play(forward: false);
		}
	}

	private void LateUpdate()
	{
		if (Singleton<PlayerManager>.SP.GetLocalPlayers().Length != Singleton<LocalMultiManager>.SP.GetAlivePlayers().Length)
		{
			Checkpoint nextCheckpoint = Singleton<CheckpointManager>.SP.GetNextCheckpoint(Singleton<LocalMultiManager>.SP.GetFurthestCheckpointReached());
			if (nextCheckpoint != null && nextCheckpoint != Singleton<CheckpointManager>.SP.Finishline && Singleton<LocalMultiManager>.SP.allowCheckpointRespawn)
			{
				Vector3 position = nextCheckpoint.transform.position + nextCheckpoint.transform.TransformDirection(nextCheckpoint.extraOffsetToRespawnIndicator);
				Vector3 vector = Camera.main.WorldToViewportPoint(position);
				if (vector.z > 0f)
				{
					float x = vector.x;
					float y = vector.y;
					Vector3 localPosition = new Vector3(x * 1920f, y * 1080f, 0f);
					respawnIndicator.SetActive(value: true);
					respawnIndicator.transform.localPosition = localPosition;
					float num = 25f;
					float num2 = 0.65f;
					float z = vector.z;
					if (z > num)
					{
						float num3 = num / z;
						float num4 = 1f - (1f - num3) * num2;
						respawnIndicator.transform.localScale = Vector3.one * num4;
					}
					else
					{
						respawnIndicator.transform.localScale = Vector3.one;
					}
				}
				else
				{
					respawnIndicator.SetActive(value: false);
				}
			}
			else
			{
				respawnIndicator.SetActive(value: false);
			}
		}
		else
		{
			respawnIndicator.SetActive(value: false);
		}
	}

	public void OffscreenIndicatorUpdate(bool turnIndicatorOn, int playerNum, Vector3 viewportPos, float radialFillAmount = 0f)
	{
		if (turnIndicatorOn)
		{
			offscreenIndicators[playerNum].SetActive(value: true);
			float num = 0.03f;
			float num2 = Mathf.Clamp(viewportPos.x, num, 1f - num);
			float num3 = Mathf.Clamp(viewportPos.y, num, 1f - num);
			viewportPos = new Vector3(num2 * 1920f, num3 * 1080f + 10f, 0f);
			offscreenIndicators[playerNum].transform.localPosition = viewportPos;
			radialIndicators[playerNum].fillAmount = radialFillAmount;
		}
		else
		{
			offscreenIndicators[playerNum].SetActive(value: false);
		}
	}

	private void Button_Resume()
	{
		ToggleMenu(state: false);
	}

	private void Button_Quit()
	{
		ToggleMenu(state: false);
		Time.timeScale = 1f;
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		HenkUtils.BackToMenu();
		Singleton<GamestateManager>.SP.SetState(typeof(State_LMP));
	}

	private void ToggleMenu(bool state)
	{
		if (state)
		{
			menuActive = true;
			pauseMenu.SetActive(value: true);
			stateObj.MyInputEnabled = true;
			Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
			timeScaleBeforePause = Time.timeScale;
			Time.timeScale = 0f;
		}
		else
		{
			menuActive = false;
			pauseMenu.SetActive(value: false);
			Time.timeScale = timeScaleBeforePause;
			stateObj.MyInputEnabled = false;
			Singleton<InputManager>.SP.Deselect();
		}
	}

	private IEnumerator DelayInputEnable()
	{
		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.2f));
		stateObj.MyInputEnabled = true;
	}
}
