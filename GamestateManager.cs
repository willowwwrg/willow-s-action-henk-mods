using System;
using UnityEngine;

public class GamestateManager : Singleton<GamestateManager>
{
	private GameState currentState;

	public GameObject CurrentState;

	private GameMode currentGameMode;

	public GameState State => currentState;

	public GameMode GetCurrentGameMode()
	{
		return currentGameMode;
	}

	public void SetCurrentGameMode(GameMode mode)
	{
		currentGameMode = mode;
	}

	public bool IsCurrentState(Type state)
	{
		return currentState.GetType() == state;
	}

	public void SetState(Type newStateType)
	{
		if (currentState != null)
		{
			currentState.OnDeactivate();
			currentState.gameObject.SendMessage("NextState", newStateType, SendMessageOptions.DontRequireReceiver);
		}
		currentState = UnityEngine.Object.FindObjectOfType(newStateType) as GameState;
		CurrentState = currentState.gameObject;
		if (currentState != null)
		{
			currentState.firstFrameActive = true;
			currentState.OnActivate();
		}
	}

	public void Update()
	{
		if (currentState != null)
		{
			currentState.OnUpdate();
			if (currentState.firstFrameActive)
			{
				HenkUtils.ForceGUIUpdate();
				currentState.firstFrameActive = false;
			}
		}
	}
}
