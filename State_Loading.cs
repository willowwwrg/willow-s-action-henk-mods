using System.Collections;
using UnityEngine;

public class State_Loading : GameState
{
	private bool waitingForSceneLoad;

	public AsyncOperation levelLoader;

	public GUI_Loading guiScreen;

	private int sceneToLoad;

	public override void OnActivate()
	{
		Singleton<AudioManager>.SP.PlayLoadingTheme();
		sceneToLoad = Singleton<LevelBatchManager>.SP.GetCurrentLevel();
		Singleton<LevelBatchManager>.SP.SetCurrentLevel(sceneToLoad);
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			guiScreen.SetDailyChallengeLoadingText();
		}
		else
		{
			guiScreen.SetLoadingText();
		}
		Singleton<HenkSWLeaderboards>.SP.RefreshCurrentLeaderboard();
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.LevelStart, string.Empty);
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().isSceneLess)
		{
			StartCoroutine("DelayLoadingSceneless");
		}
		else if (Application.loadedLevelName != "LoadingScene")
		{
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_None, "none");
			Application.LoadLevelAsync("LoadingScene");
			waitingForSceneLoad = true;
		}
		else
		{
			StartCoroutine("DelayLoading");
		}
		HenkUtils.FindTransformInHierarchy(base.transform.parent, "3.1. Pre Game").GetComponent<State_PreGame>().cameFromLoadingScreen = true;
		Camera.main.GetComponent<CameraEffectsManager>().Init(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle);
		PlayerPrefs.SetInt("MOSTRECENTLYCOMPLETEDLEVEL", 0);
	}

	private void InitState()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Loading, "none");
		Singleton<PermaGUI>.SP.BGPanelSetTitles("Loading", Singleton<LevelBatchManager>.SP.GetLevelName(sceneToLoad));
		Singleton<PlayerManager>.SP.ghostSet = false;
	}

	private void OnLevelWasLoaded()
	{
		if (waitingForSceneLoad)
		{
			waitingForSceneLoad = false;
			StartCoroutine("DelayLoading");
		}
		_ = Application.loadedLevelName;
	}

	public override void OnDeactivate()
	{
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
	}

	public override void OnUpdate()
	{
	}

	private IEnumerator DelayLoading()
	{
		InitState();
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.Replay || Application.isEditor)
		{
			yield return new WaitForSeconds(2f);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
		}
		levelLoader = Application.LoadLevelAsync(sceneToLoad);
	}

	private IEnumerator DelayLoadingSceneless()
	{
		float seconds = 2.5f;
		if (Application.isEditor)
		{
			seconds = 0.1f;
		}
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Loading, "none");
		yield return new WaitForSeconds(seconds);
		Singleton<GamestateManager>.SP.SetState(typeof(State_BuildLevel));
	}
}
