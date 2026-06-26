using System.Collections.Generic;
using UnityEngine;

public class GUI_OptionsLanguage : GUI_Base
{
	public InputObject firstButton;

	public List<GameObject> buttons;

	private void TransitionCompleted()
	{
		InitializeScreen();
		Singleton<InputManager>.SP.Select(firstButton, delayedTillEndOfFrame: false, playSound: false);
		string text = Language.CurrentLanguage().ToString();
		foreach (GameObject button in buttons)
		{
			if (button.GetComponentInChildren<LocalizeUILabel>().LocalizationString == text)
			{
				button.GetComponentInChildren<UIToggle>().value = true;
				return;
			}
		}
		Language.SwitchLanguage("EN");
		LocalizeUILabel[] componentsInChildren = Singleton<GetRoot>.SP.Get().GetComponentsInChildren<LocalizeUILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateLabelOnce();
		}
		buttons[0].GetComponentInChildren<UIToggle>().value = true;
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_OptionsGame));
		AudioController.Play("ButtonBackwards");
	}

	private void Button_SetLanguage(Object value)
	{
		GameObject gameObject = value as GameObject;
		if (Language.SwitchLanguage(gameObject.GetComponentInChildren<LocalizeUILabel>().LocalizationString))
		{
			LocalizeUILabel[] componentsInChildren = Singleton<GetRoot>.SP.Get().GetComponentsInChildren<LocalizeUILabel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].UpdateLabelOnce();
			}
			gameObject.GetComponentInChildren<UIToggle>().value = true;
		}
		else
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_LANGUAGENOTSUPPORTED", "PERMA"));
		}
	}
}
