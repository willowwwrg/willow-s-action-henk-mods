using System.Collections.Generic;
using UnityEngine;

public class Transitions : Singleton<Transitions>
{
	public GameObject[] TransitionEffects;

	private UICamera UICam;

	private Dictionary<string, GameObject> TransitionEffectDictionary;

	private Dictionary<GameObject, List<UITweener>> TransitionTweens;

	private GUIManager.GUIScreens TransitionTarget = GUIManager.GUIScreens.GUIScreen_MainMenu;

	private string TransitionString;

	private void Awake()
	{
		TransitionEffectDictionary = new Dictionary<string, GameObject>();
		TransitionTweens = new Dictionary<GameObject, List<UITweener>>();
	}

	public void TransitionTo(GUIManager.GUIScreens screen, string transition)
	{
		TransitionTarget = screen;
		TransitionString = transition;
		if (TransitionString == "None" || TransitionString == "none")
		{
			Singleton<GUIManager>.SP.SetScreen(TransitionTarget);
			Singleton<GUIManager>.SP.TransitionCompleted();
			return;
		}
		if (UICam != null)
		{
			ToggleControls(toggle: false);
		}
		foreach (UITweener item in TransitionTweens[TransitionEffectDictionary[transition]])
		{
			item.ResetToBeginning();
			item.PlayForward();
		}
	}

	public void onHalfway()
	{
		Singleton<GUIManager>.SP.SetScreen(TransitionTarget);
	}

	public void onFinished()
	{
		ToggleControls(toggle: true);
		Singleton<GUIManager>.SP.TransitionCompleted();
	}

	public void ToggleControls(bool toggle)
	{
		UICam.useController = toggle;
		UICam.useMouse = toggle;
		UICam.useKeyboard = toggle;
		UICam.useTouch = toggle;
	}
}
