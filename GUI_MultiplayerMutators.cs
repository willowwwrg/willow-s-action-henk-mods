using System.Collections.Generic;
using UnityEngine;

public class GUI_MultiplayerMutators : GUI_Base
{
	public InputObject firstButton;

	public List<GameObject> buttons;

	private List<Mutator> selectedMutators;

	private bool selectAllMutators = true;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		selectedMutators = Singleton<MutatorManager>.SP.GetMultiplayerMutatorsFromPlayerPrefs();
		for (int i = 0; i < buttons.Count; i++)
		{
			if (selectedMutators.Contains(Singleton<MutatorManager>.SP.GetMutatorFromString(buttons[i].GetComponentInChildren<UILabel>().text)))
			{
				buttons[i].GetComponentInChildren<UIToggle>().value = true;
			}
			else
			{
				buttons[i].GetComponentInChildren<UIToggle>().value = false;
			}
		}
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		string text = string.Empty;
		for (int i = 0; i < selectedMutators.Count; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			text += Singleton<MutatorManager>.SP.GetStringFromMutator(selectedMutators[i]);
		}
		Singleton<PlayerPrefsManager>.SP.SetString("MultiplayerSettings_MUTATORS", text);
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_MultiplayerRoomCreation, "none");
		AudioController.Play("ButtonBackwards");
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.SelectAllMutators))
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				ToggleMutator(buttons[i], selectAllMutators ? 1 : 2);
			}
			selectAllMutators = !selectAllMutators;
			Singleton<AudioManager>.SP.PlayCheckboxToggleSound(selectAllMutators);
		}
	}

	private void Button_ToggleMutator(Object value)
	{
		GameObject gameObject = value as GameObject;
		ToggleMutator(gameObject);
		Singleton<AudioManager>.SP.PlayCheckboxToggleSound(gameObject.GetComponentInChildren<UIToggle>().value);
	}

	private void ToggleMutator(GameObject button, int stateOverride = 2)
	{
		bool flag = !button.GetComponentInChildren<UIToggle>().value;
		switch (stateOverride)
		{
		case 0:
			flag = false;
			break;
		case 1:
			flag = true;
			break;
		}
		string text = button.GetComponentInChildren<UILabel>().text;
		Mutator mutatorFromString = Singleton<MutatorManager>.SP.GetMutatorFromString(text);
		if (flag)
		{
			if (!selectedMutators.Contains(mutatorFromString))
			{
				selectedMutators.Add(mutatorFromString);
			}
		}
		else if (selectedMutators.Contains(mutatorFromString))
		{
			selectedMutators.Remove(mutatorFromString);
		}
		button.GetComponentInChildren<UIToggle>().value = flag;
	}
}
