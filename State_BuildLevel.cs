using UnityEngine;

public class State_BuildLevel : GameState
{
	public GUI_Loading guiScript;

	public override void OnActivate()
	{
		Singleton<AudioManager>.SP.PlayLoadingTheme();
		Object.FindObjectOfType<State_InGame>().challengerIntroSoundPlayed = false;
		_ = Singleton<LevelBatchManager>.SP.currentLevel;
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LevelEditor)
		{
			Singleton<HenkSWLeaderboards>.SP.RefreshCurrentLeaderboard();
		}
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_Loading, "none");
		if (Singleton<MutatorManager>.SP.GetActiveMutator() != Mutator.None)
		{
			guiScript.SetDailyChallengeLoadingText();
		}
		else
		{
			guiScript.SetLoadingText();
		}
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LevelEditor || Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.PlayWorkshopLevel)
		{
			Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.LevelStart, string.Empty);
		}
		HenkUtils.ToggleMenuSceneItems(state: false);
		HenkUtils.KillLevelObjects();
		(Object.Instantiate(Singleton<Foreman>.SP.mainCamPrefab) as GameObject).name = "instantiatedMainCam";
		Singleton<PlayerManager>.SP.ghostSet = false;
		Singleton<Foreman>.SP.BuildLevel();
	}

	public override void OnDeactivate()
	{
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
	}

	public override void OnUpdate()
	{
	}
}
