using UnityEngine;

public class GUI_PreGameMultiplayer : GUI_Base
{
	public GameObject scoreBoardGUI;

	public GUI_InGameMultiplayer inGameGUI;

	private void TransitionCompleted()
	{
		InitializeScreen();
		scoreBoardGUI.SetActive(value: true);
		Singleton<PermaGUI>.SP.ToggleMultiplayerMutatorText(Singleton<MutatorManager>.SP.mutatorActive, "Mutator: " + Singleton<MutatorManager>.SP.GetActiveMutatorString());
	}

	private void NextWindow()
	{
	}

	private void PrevWindow()
	{
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Start))
		{
			Singleton<InGameMenu>.SP.ToggleMenu(!Singleton<InGameMenu>.SP.menuEnabled);
		}
	}
}
