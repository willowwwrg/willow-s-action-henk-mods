using UnityEngine;

public class InGameMenu : Singleton<InGameMenu>
{
	public bool menuEnabled;

	public GameObject menuObject;

	public InputObject buttonToSelect;

	private float timeScaleBeforePause;

	private void Update()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm) && !(Singleton<InputManager>.SP.GetCurrentButton() == null) && (Singleton<InputManager>.SP.GetCurrentButton().name == "mpexit" || Singleton<InputManager>.SP.GetCurrentButton().name == "resume"))
		{
			Singleton<InputManager>.SP.ClickCurrentButton();
		}
	}

	public void ToggleMenu(bool toggle, bool stopTime = false)
	{
		if (menuEnabled == toggle)
		{
			return;
		}
		menuEnabled = toggle;
		menuObject.SetActive(menuEnabled);
		if (menuEnabled)
		{
			AudioController.Play("ButtonForwards");
			if (stopTime)
			{
				timeScaleBeforePause = Time.timeScale;
				Time.timeScale = 0f;
			}
			if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer)
			{
				Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = true;
			}
			Singleton<InputManager>.SP.Select(buttonToSelect);
		}
		else
		{
			AudioController.Play("ButtonBackwards");
			Singleton<InputManager>.SP.Deselect();
			if (stopTime)
			{
				Time.timeScale = timeScaleBeforePause;
			}
			if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer)
			{
				Singleton<GamestateManager>.SP.CurrentState.GetComponent<GameState>().MyInputEnabled = false;
			}
		}
	}
}
