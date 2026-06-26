using UnityEngine;

public class GUI_OptionsGraphicsResolutions : MonoBehaviour
{
	public InputObject FirstButton;

	private void Awake()
	{
	}

	private void TransitionCompleted()
	{
		Singleton<InputManager>.SP.Select(FirstButton, delayedTillEndOfFrame: false, playSound: false);
	}

	private void NextWindow()
	{
		Singleton<InputManager>.SP.ClickCurrentButton();
		AudioController.Play("ButtonForwards");
	}

	private void PrevWindow()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Options, "none");
		AudioController.Play("ButtonBackwards");
	}

	private void Update()
	{
	}
}
