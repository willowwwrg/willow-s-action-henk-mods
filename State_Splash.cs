using System.Collections;
using UnityEngine;

public class State_Splash : GameState
{
	public bool EnableSplash = true;

	private float splashTimer = 8.5f;

	public PlayMovie moviePlane;

	private Camera mainCam;

	public UILabel loadingLabel;

	private bool loadMainMenu;

	public override void OnActivate()
	{
		Screen.showCursor = false;
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
	}

	private void Start()
	{
	}

	public override void OnDeactivate()
	{
		_ = EnableSplash;
	}

	public override void OnUpdate()
	{
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Confirm) || Singleton<InputManager>.SP.CheckAction(InputAction.Start))
		{
			StopCoroutine("SplashSequence");
			loadMainMenu = true;
		}
		if (loadMainMenu)
		{
			moviePlane.Stop();
			loadMainMenu = false;
			loadingLabel.enabled = true;
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		}
	}

	private IEnumerator SplashSequence()
	{
		yield return new WaitForSeconds(0.25f);
		float categoryVolume = AudioController.GetCategoryVolume("InGameSFX");
		moviePlane.GetComponent<AudioSource>().volume = categoryVolume;
		moviePlane.Play();
		yield return new WaitForSeconds(splashTimer);
		loadMainMenu = true;
	}
}
