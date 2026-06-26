using UnityEngine;

public class State_InGameLevelEditor : GameState
{
	private bool initialDOFSetting = true;

	private bool initialized;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_InGameLevelEditor, "none");
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.LevelEditor);
		GoToEditMode();
		Singleton<PermaGUI>.SP.KillCountdownLabels();
		Singleton<AudioManager>.SP.PlayEditorTheme();
		Singleton<PermaGUI>.SP.FadeInOrOut(1.5f, fadeIn: true);
		if (!initialized)
		{
			Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerGraphics>().SetModel(Singleton<CharacterSelect>.SP.GetSelectedCharacter(), Singleton<CharacterSelect>.SP.GetSelectedSkin());
		}
		Camera.main.GetComponent<PlatformerCamera>().FindTarget();
		if (Object.FindObjectOfType<LavaPlane>() != null)
		{
			Object.FindObjectOfType<LavaPlane>().enabled = false;
		}
		initialDOFSetting = Camera.main.GetComponent<DepthOfFieldScatter>().enabled;
		Camera.main.GetComponent<DepthOfFieldScatter>().enabled = false;
		Singleton<Workshop>.SP.validating = false;
		Singleton<Workshop>.SP.validationMsgShown = false;
		initialized = true;
	}

	public void SpawnEditorCursor()
	{
		GameObject obj = Object.Instantiate(Resources.Load("LevelEditor/LevelEditorCursor")) as GameObject;
		obj.name = "LevelEditorCursor";
		obj.transform.position = Singleton<PlayerManager>.SP.GetPlayer().transform.position;
		SplineFollow component = obj.GetComponent<SplineFollow>();
		component.splineOffset = Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerWaypointManager>().GetOffset2D();
		component.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		component.ForceOneUpdate();
		component.SnapCoordinates(toggle: false);
		GameObject obj2 = Object.Instantiate(Resources.Load("LevelEditor/LevelEditorCameraFollow")) as GameObject;
		obj2.name = "LevelEditorCameraObject";
		SplineFollow component2 = obj2.GetComponent<SplineFollow>();
		component2.splineOffset = Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerWaypointManager>().GetOffset2D();
		component2.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		component2.ForceOneUpdate();
		component2.SnapCoordinates(toggle: false);
	}

	public void LevelFileLoadedLevelEditor()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_InGameLevelEditor, "None");
		Singleton<PlayerManager>.SP.SpawnPlayer();
		SpawnEditorCursor();
		Camera.main.GetComponent<PlatformerCamera>().FindTarget();
		Camera.main.GetComponent<PlatformerCamera>().OnReset();
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
	}

	public override void OnDeactivate()
	{
		if (Object.FindObjectOfType<LavaPlane>() != null)
		{
			Object.FindObjectOfType<LavaPlane>().enabled = true;
		}
		Camera.main.GetComponent<DepthOfFieldScatter>().enabled = initialDOFSetting;
		GoToEditMode(state: false);
	}

	public override void OnUpdate()
	{
		Screen.showCursor = true;
		if (Input.GetKeyDown(KeyCode.F2))
		{
			if (!Singleton<EditorCursor>.SP.DoesStartLineExist())
			{
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOSTARTLINE", "PERMA"));
			}
			else if (!Singleton<EditorCursor>.SP.menuVisible)
			{
				GoToInGame();
			}
		}
	}

	public void GoToInGame()
	{
		Singleton<EditorCursor>.SP.GoToIngame();
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGame));
		SelectedItem[] array = Object.FindObjectsOfType<SelectedItem>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SwitchToInGame();
		}
	}

	private void GoToEditMode(bool state = true)
	{
		GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
		if ((bool)player)
		{
			player.GetComponent<PlatformerPhysics>().enabled = !state;
			player.GetComponent<RaycastCollider>().enabled = !state;
			player.GetComponent<PlayerGraphics>().enabled = !state;
			if ((bool)player.GetComponent<PlayerGraphics>().rotatingChild.GetComponentInChildren<Animator>())
			{
				player.GetComponent<PlayerGraphics>().rotatingChild.GetComponentInChildren<Animator>().enabled = !state;
			}
		}
		Camera.main.GetComponent<PlatformerCamera>().cameraShake = !state;
		if (state)
		{
			Singleton<EditorCursor>.SP.GoToEditorMode();
			SelectedItem[] array = Object.FindObjectsOfType<SelectedItem>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchToEditMode();
			}
		}
	}

	public void CloseLevelEditor()
	{
		initialized = false;
		Object.Destroy(Singleton<EditorCameraFollow>.SP.gameObject);
		Object.Destroy(Singleton<EditorCursor>.SP.gameObject);
	}
}
