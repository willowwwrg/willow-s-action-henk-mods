using UnityEngine;

public class State_OptionsGraphics : GameState
{
	public GUI_OptionsGraphics guiScript;

	private GameObject fpsThrusters;

	public void Start()
	{
		fpsThrusters = GameObject.Find("FPSThrusters");
		if ((bool)fpsThrusters)
		{
			fpsThrusters.SetActive(value: false);
		}
	}

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_OptionsGraphics, "none");
		if ((bool)fpsThrusters)
		{
			fpsThrusters.SetActive(value: true);
		}
	}

	public override void OnDeactivate()
	{
		if ((bool)fpsThrusters)
		{
			fpsThrusters.SetActive(value: false);
		}
	}

	public override void OnUpdate()
	{
	}
}
