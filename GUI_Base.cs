using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_Base : MonoBehaviour
{
	public bool acceptInput = true;

	public bool showLeftBackground;

	public bool showBackButton;

	public string backbuttonOverrideText = string.Empty;

	public bool showForwardButton;

	public string forwardbuttonOverrideText = string.Empty;

	public bool showMedals;

	public bool showInboxButton;

	public bool showArrows;

	public List<UITweener> transitionCompletedTweens;

	public void InitializeScreen(float seconds = 0f)
	{
		Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(showBackButton, showForwardButton, backbuttonOverrideText, forwardbuttonOverrideText);
		Singleton<PermaGUI>.SP.ToggleBGPanelLeft(showLeftBackground, string.Empty);
		Singleton<PermaGUI>.SP.ShowMedalCount(showMedals);
		Singleton<PermaGUI>.SP.ToggleArrows(showArrows);
		Singleton<PermaGUI>.SP.ShowInboxInfo(showInboxButton);
		PlayTweens();
		StartCoroutine(DelayInput(seconds));
	}

	private void PlayTweens()
	{
		foreach (UITweener transitionCompletedTween in transitionCompletedTweens)
		{
			transitionCompletedTween.ResetToBeginning();
			transitionCompletedTween.Play(forward: true);
		}
	}

	private IEnumerator DelayInput(float seconds = 0f)
	{
		acceptInput = false;
		if (seconds == 0f)
		{
			yield return new WaitForEndOfFrame();
		}
		else
		{
			yield return new WaitForSeconds(seconds);
		}
		acceptInput = true;
	}

	public void RedrawGUI()
	{
		StartCoroutine(RedrawGUIRoutine());
	}

	private IEnumerator RedrawGUIRoutine()
	{
		yield return new WaitForEndOfFrame();
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetDirty();
		}
	}
}
