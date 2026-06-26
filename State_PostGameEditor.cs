using System.IO;
using UnityEngine;

public class State_PostGameEditor : GameState
{
	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_PostGameEditor, "none");
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
		Singleton<PlayerManager>.SP.TogglePlayerControls(PlayerType.Local, state: false);
		if (!Singleton<Workshop>.SP.validating)
		{
			return;
		}
		if (Singleton<Workshop>.SP.Initialize())
		{
			Singleton<Workshop>.SP.SubmitCurrentLevelToWorkshop();
			return;
		}
		Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_FAILEDWORKSHOPCONNECT", "PERMA"));
		string path = Application.dataPath + "/Resources/../../CustomLevels/" + Singleton<LevelBatchManager>.SP.currentLevel.guid.ToString() + "/preview.png";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		if (!Singleton<Workshop>.SP.validating)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Retry))
			{
				AudioController.Play("ButtonBackwards");
				Singleton<GamestateManager>.SP.SetState(typeof(State_PreGame));
			}
			if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.Escape))
			{
				Singleton<GamestateManager>.SP.SetState(typeof(State_InGameLevelEditor));
			}
		}
	}
}
