using System.Collections;
using UnityEngine;

public class State_Cutscene : GameState
{
	public GUI_Loading guiScript;

	private AsyncOperation levelLoading;

	private AsyncOperation clearLevel;

	private float loadingStartTime;

	private float minLoadTime = 1f;

	private bool pressed;

	private bool isPostGameScene;

	public override void OnActivate()
	{
		Singleton<AudioManager>.SP.StopMusic();
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Loading, "none");
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
		guiScript.SetCustomText(string.Empty, string.Empty);
	}

	public void LoadCutscene(string cutScene, bool isPostGame)
	{
		pressed = false;
		isPostGameScene = isPostGame;
		loadingStartTime = Time.time;
		GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
		for (int i = 0; i < allPlayers.Length; i++)
		{
			allPlayers[i].SetActive(value: false);
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_Cutscene));
		MonoBehaviour.print("Start loading cutscene");
		levelLoading = Application.LoadLevelAsync(cutScene);
		levelLoading.allowSceneActivation = false;
	}

	private void OnLevelWasLoaded()
	{
		if (levelLoading != null)
		{
			Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
			MonoBehaviour.print("Done loading cutscene");
			HenkUtils.KillLevelObjects();
			HenkUtils.ToggleMenuSceneItems(state: false);
			Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_None, "None");
			levelLoading = null;
		}
		if (clearLevel != null)
		{
			MonoBehaviour.print("Done clearing scene");
			clearLevel = null;
			StartCoroutine(DoneClearingScene());
		}
	}

	private IEnumerator DoneClearingScene()
	{
		yield return new WaitForEndOfFrame();
		if (isPostGameScene)
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_LevelSelectCampaign));
		}
		else
		{
			Singleton<GamestateManager>.SP.SetState(typeof(State_BuildLevel));
		}
	}

	public override void OnUpdate()
	{
		if (levelLoading != null)
		{
			float num = Time.time - loadingStartTime;
			if (levelLoading.progress == 0.9f && num > minLoadTime)
			{
				levelLoading.allowSceneActivation = true;
			}
		}
		else if (clearLevel == null && (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel) || Singleton<InputManager>.SP.CheckAction(InputAction.Confirm)))
		{
			if (!pressed)
			{
				pressed = true;
				EndOfCutscene();
			}
			else
			{
				MonoBehaviour.print("pressed == true!");
			}
		}
	}

	public void EndOfCutscene()
	{
		MonoBehaviour.print("endofcutscene");
		if (Application.loadedLevelName == "Cutscene_Ending")
		{
			isPostGameScene = false;
			Singleton<LevelBatchManager>.SP.currentLevel = Singleton<LevelBatchManager>.SP.GetLevelFromCodeOrID(97uL);
		}
		if (clearLevel == null)
		{
			clearLevel = Application.LoadLevelAsync("EmptyScene");
		}
		Singleton<LevelBatchManager>.SP.showingCutscene = false;
	}

	public override void OnDeactivate()
	{
	}
}
