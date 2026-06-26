using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : Singleton<GUIManager>
{
	public enum GUIScreens
	{
		GUIScreen_Splash,
		GUIScreen_MainMenu,
		GUIScreen_Options,
		GUIScreen_Leaderboards,
		GUIScreen_Statistics,
		GUIScreen_Loading,
		GUIScreen_GameModeSelection,
		GUIScreen_CharacterSelectionCampaign,
		GUIScreen_PreGame,
		GUIScreen_InGame,
		GUIScreen_PostGame,
		GUIScreen_None,
		GUIScreen_Credits,
		GUIScreen_Multiplayer,
		GUIScreen_LevelSelectCampaign,
		GUIScreen_LevelSelectMultiplayer,
		GUIScreen_OptionsMultiplayer,
		GUIScreen_OptionsInGame,
		GUIScreen_Pause,
		GUIScreen_Xmo,
		GUIScreen_OptionsAudio,
		GUIScreen_OptionsGame,
		GUIScreen_OptionsGraphics,
		GUIScreen_OptionsGraphicsResolutions,
		GUIScreen_LevelSelectLeaderboards,
		GUIScreen_PreGameMultiplayer,
		GUIScreen_InGameMultiplayer,
		GUIScreen_PostGameMultiplayer,
		GUIScreen_EndGameMultiplayer,
		GUIScreen_ReplayMode,
		GUIScreen_SplashStart,
		GUIScreen_LevelEditorMain,
		GUIScreen_LevelEditorCreateLevel,
		GUIScreen_LevelEditorPlayLevel,
		GUIScreen_LevelEditorSceneSelect,
		GUIScreen_InGameLevelEditor,
		GUIScreen_PostGameEditor,
		GUIScreen_BatchSelect,
		GUIScreen_MultiplayerRoomCreation,
		GUIScreen_MultiplayerLevelRotation,
		GUIScreen_OptionsTwitch,
		GUIScreen_OptionsLanguage,
		GUIScreen_LMP,
		GUIScreen_LMP_Pregame,
		GUIScreen_LMP_Ingame,
		GUIScreen_LMP_Postgame,
		GUIScreen_LMP_Levelrotation,
		GUIScreen_LMP_Settings,
		GUIScreen_MultiplayerPicker,
		GUIScreen_LMP_Endgame,
		GUIScreen_BatchSelectExtraLevels,
		GUIScreen_MultiplayerMutators
	}

	public GUIScreens ActiveScreen = GUIScreens.GUIScreen_None;

	private GUIScreens PrevActiveScreen = GUIScreens.GUIScreen_None;

	public List<GameObject> ListOfGUIScreens = new List<GameObject>();

	private GameObject currentScreenObject;

	private Dictionary<GameObject, GUIScreens> GUIObjectsWithScreens;

	private Dictionary<GUIScreens, GameObject> GUIScreensWithObjects;

	private bool timeOutFrame;

	private void Awake()
	{
		if (GUIObjectsWithScreens == null)
		{
			GUIObjectsWithScreens = new Dictionary<GameObject, GUIScreens>();
		}
		int num = 0;
		foreach (int value in Enum.GetValues(typeof(GUIScreens)))
		{
			GUIObjectsWithScreens.Add(ListOfGUIScreens[num], (GUIScreens)value);
			num++;
		}
		GUIScreensWithObjects = new Dictionary<GUIScreens, GameObject>();
		foreach (KeyValuePair<GameObject, GUIScreens> gUIObjectsWithScreen in GUIObjectsWithScreens)
		{
			GUIScreensWithObjects.Add(gUIObjectsWithScreen.Value, gUIObjectsWithScreen.Key);
		}
	}

	private void Start()
	{
		SetScreen(ActiveScreen);
	}

	private void Update()
	{
		timeOutFrame = false;
		if (PrevActiveScreen != ActiveScreen)
		{
			SwitchScreen(ActiveScreen);
		}
	}

	public void SetScreen(GUIScreens newScreen)
	{
		ActiveScreen = newScreen;
		PrevActiveScreen = newScreen;
		SwitchScreen(newScreen);
	}

	public void NextWindow()
	{
		if (!timeOutFrame)
		{
			timeOutFrame = true;
			GetCurrentScreen().SendMessage("NextWindow", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PrevWindow()
	{
		if (!Singleton<PermaGUI>.SP.confirmationRequestUp)
		{
			GetCurrentScreen().SendMessage("PrevWindow", SendMessageOptions.DontRequireReceiver);
		}
	}

	public GameObject GetCurrentScreen()
	{
		return GetGameObjectFromScreenName(ActiveScreen);
	}

	public GUIScreens GetCurrentScreenName()
	{
		return ActiveScreen;
	}

	private GUIScreens GetScreenNameFromGameObject(GameObject GUIscreen)
	{
		return GUIObjectsWithScreens[GUIscreen];
	}

	public GameObject GetGameObjectFromScreenName(GUIScreens GUIscreen)
	{
		if (!GUIScreensWithObjects.ContainsKey(GUIscreen))
		{
			Debug.LogError("Error: " + GUIscreen.ToString() + " does not exist.");
		}
		return GUIScreensWithObjects[GUIscreen];
	}

	private void SwitchScreen(GUIScreens newScreen)
	{
		if (ActiveScreen != GUIScreens.GUIScreen_None)
		{
			SwitchAllGUIScreensOn(toggle: false);
			GameObject gameObjectFromScreenName = GetGameObjectFromScreenName(newScreen);
			gameObjectFromScreenName.SetActive(value: true);
			currentScreenObject = gameObjectFromScreenName;
			gameObjectFromScreenName.SendMessage("OnActivatedGUIScreen", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			SwitchAllGUIScreensOn(toggle: false);
		}
		PrevActiveScreen = ActiveScreen;
		ActiveScreen = newScreen;
	}

	private void SwitchAllGUIScreensOn(bool toggle)
	{
		foreach (GameObject listOfGUIScreen in ListOfGUIScreens)
		{
			if (listOfGUIScreen != null)
			{
				listOfGUIScreen.SetActive(toggle);
			}
		}
	}

	public void TransitionCompleted()
	{
		if (ActiveScreen != GUIScreens.GUIScreen_None)
		{
			GetGameObjectFromScreenName(ActiveScreen).SendMessage("TransitionCompleted", SendMessageOptions.DontRequireReceiver);
			Singleton<GetRoot>.SP.Get().BroadcastMessage("NewGUIScreen", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void LeaveGUIScreen()
	{
		if (ActiveScreen != GUIScreens.GUIScreen_None)
		{
			GetGameObjectFromScreenName(ActiveScreen).SendMessage("LeaveGUIScreen", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void RefreshLists()
	{
		SwitchAllGUIScreensOn(toggle: true);
		SwitchScreen(ActiveScreen);
	}

	public void SetPanelAlpha(float alpha)
	{
		for (int i = 0; i < currentScreenObject.transform.childCount; i++)
		{
			currentScreenObject.transform.GetChild(i).GetComponent<UIPanel>().alpha = alpha;
		}
	}
}
