using System.Collections;
using UnityEngine;

public class Foreman : Singleton<Foreman>
{
	public GameObject mainCamPrefab;

	public LevelStyle currentEnvironmentStyle;

	public void BuildLevel()
	{
		StartCoroutine(LoadLevelRoutine());
	}

	private IEnumerator LoadLevelRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		if (!Singleton<LevelFileLoader>.SP.Load())
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_LOADFAILED", "PERMA"));
			Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
			yield break;
		}
		yield return new WaitForSeconds(1f);
		Singleton<PermaGUI>.SP.FadeInOrOut(0.01f, fadeIn: false);
		yield return new WaitForSeconds(0.5f);
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Replay)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("LevelFileLoadedReplay");
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("LevelFileLoadedLevelEditor");
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("LevelFileLoaded");
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("LevelFileLoadedMultiplayer");
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LocalMultiplayer)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("LevelFileLoadedLocalMultiplayer");
		}
		else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.PlayWorkshopLevel)
		{
			Singleton<GetRoot>.SP.Get().BroadcastMessage("LevelFileLoadedWorkshop");
		}
		Singleton<AudioManager>.SP.PlayEnvironmentAudio(pregame: true);
	}

	public void StylizeLevel(LevelStyle style)
	{
		if (currentEnvironmentStyle == style)
		{
			MonoBehaviour.print("same style: " + style);
			return;
		}
		MonoBehaviour.print(string.Concat("new style: ", style, " current style:", currentEnvironmentStyle));
		LightmapSettings.lightmaps = new LightmapData[0];
		Resources.UnloadUnusedAssets();
		GameObject gameObject = GameObject.Find("FloorCollider");
		if (gameObject != null)
		{
			gameObject.collider.enabled = false;
		}
		GameObject gameObject2 = GameObject.Find("FloorCollider2");
		if (gameObject2 != null)
		{
			gameObject2.collider.enabled = false;
		}
		Object.Destroy(GameObject.Find("EnvironmentStyleContainer"));
		StartCoroutine(LoadLevelDelayed());
		currentEnvironmentStyle = style;
		PlayerPrefs.SetString("MostRecentLevelStyle", currentEnvironmentStyle.ToString());
		if ((bool)Camera.main)
		{
			Camera.main.GetComponent<CameraEffectsManager>().Init(style);
		}
	}

	private IEnumerator LoadLevelDelayed()
	{
		yield return new WaitForEndOfFrame();
		Application.LoadLevelAdditive(currentEnvironmentStyle.ToString());
	}

	public void BackToMainMenu()
	{
		StylizeLevel(LevelStyle.KidsRoom_Menu);
	}
}
