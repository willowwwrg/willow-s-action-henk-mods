using System.Collections;
using UnityEngine;

public class State_CharacterSelectionCampaign : GameState
{
	public GUI_CharacterSelectionCampaign guiScript;

	private CharacterPreviewer charPreviewerScript;

	private bool swappingSkin;

	private bool fading;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_CharacterSelectionCampaign, "none");
		charPreviewerScript = Singleton<CharacterSelect>.SP.InitCharacterPreviewer();
		if (!AudioController.IsPlaying("MainTheme"))
		{
			AudioController.StopCategory("Music", 0.5f);
			AudioController.PlayMusic("MainTheme");
		}
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		if (Singleton<InputManager>.SP.inputEnabled)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.NextCharacter))
			{
				StartCoroutine("SwitchCharacter", true);
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.PreviousCharacter))
			{
				StartCoroutine("SwitchCharacter", false);
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.NextSkin))
			{
				charPreviewerScript.NextSkin();
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.PreviousSkin))
			{
				charPreviewerScript.PrevSkin();
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm) && !fading)
			{
				Singleton<InputManager>.SP.GoToNextWindow();
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel) && !fading)
			{
				charPreviewerScript.GoToPrevWindow();
				Singleton<InputManager>.SP.GoToPrevWindow();
			}
		}
	}

	private IEnumerator SwitchCharacter(bool next)
	{
		fading = true;
		AudioController.Play("SelectCharacterFade");
		Singleton<PermaGUI>.SP.FadeInOrOut(0.15f, fadeIn: false);
		yield return new WaitForSeconds(0.2f);
		if (next)
		{
			charPreviewerScript.NextCharacter();
			AudioController.Play("NextMenuItem");
		}
		else
		{
			AudioController.Play("PrevMenuItem");
			charPreviewerScript.PrevCharacter();
		}
		Singleton<PermaGUI>.SP.FadeInOrOut(0.15f, fadeIn: true);
		fading = false;
	}

	public bool Confirm()
	{
		if (charPreviewerScript.ConfirmSkinChoice())
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
			return true;
		}
		if (charPreviewerScript.ignoreErrorSound)
		{
			charPreviewerScript.ignoreErrorSound = false;
		}
		else
		{
			AudioController.Play("ButtonClick");
		}
		return false;
	}
}
