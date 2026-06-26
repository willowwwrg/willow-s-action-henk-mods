using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
	public enum CameraMode
	{
		None,
		CharSelect,
		MainMenu,
		LevelSelect
	}

	public List<Transform> characterCameraTransforms;

	public CharacterPreviewer charPreviewer;

	public bool lookingAtLMPCharacters;

	private HandyCam handyCam;

	public CameraMode camMode = CameraMode.MainMenu;

	private void Awake()
	{
		SetMode(CameraMode.MainMenu);
	}

	private void Start()
	{
		if ((bool)GetComponent<HandyCam>())
		{
			handyCam = GetComponent<HandyCam>();
		}
		else
		{
			handyCam = base.gameObject.AddComponent<HandyCam>();
		}
	}

	public void SetMode(CameraMode mode)
	{
		switch (mode)
		{
		case CameraMode.CharSelect:
			handyCam.SetCameraTarget(null);
			break;
		case CameraMode.LevelSelect:
			handyCam.SetCameraTarget(null);
			break;
		default:
			Debug.LogError("Setting menu camera to a non-existent state.");
			SetMode(CameraMode.MainMenu);
			break;
		case CameraMode.MainMenu:
			break;
		}
		camMode = mode;
	}

	public void LookAtLMPcharacters()
	{
		lookingAtLMPCharacters = true;
		handyCam.SetCameraTarget(GetLMPTransform(), snapToTarget: true);
	}

	public void LookAtCharacter(CharacterSelect.Characters character)
	{
		Transform transformFromCharacter = GetTransformFromCharacter(character);
		handyCam.SetCameraTarget(transformFromCharacter, snapToTarget: true);
	}

	private IEnumerator MoveCamToPathAndStart(float seconds)
	{
		yield return new WaitForSeconds(seconds);
	}

	private void Update()
	{
		switch (camMode)
		{
		case CameraMode.CharSelect:
			return;
		case CameraMode.MainMenu:
			return;
		case CameraMode.LevelSelect:
			return;
		}
		Debug.LogError("Setting menu camera to a non-existent state.");
		SetMode(CameraMode.MainMenu);
	}

	private void FixedUpdate()
	{
		float num = 3f;
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_LMP)))
		{
			num = 9f;
		}
		DepthOfFieldScatter component = base.camera.GetComponent<DepthOfFieldScatter>();
		if ((bool)component && component.enabled)
		{
			component.focalLength -= (component.focalLength - num) * 5f * Time.fixedDeltaTime;
		}
	}

	public Transform GetLMPTransform()
	{
		if ((bool)characterCameraTransforms[5])
		{
			return characterCameraTransforms[5];
		}
		return null;
	}

	public Transform GetTransformFromCharacter(CharacterSelect.Characters character)
	{
		switch (character)
		{
		case CharacterSelect.Characters.Henk:
			if (characterCameraTransforms[0] != null)
			{
				return characterCameraTransforms[0];
			}
			break;
		case CharacterSelect.Characters.Betsy:
			if (characterCameraTransforms[1] != null)
			{
				return characterCameraTransforms[1];
			}
			break;
		case CharacterSelect.Characters.Afronaut:
			if (characterCameraTransforms[2] != null)
			{
				return characterCameraTransforms[2];
			}
			break;
		case CharacterSelect.Characters.Kentony:
			if (characterCameraTransforms[3] != null)
			{
				return characterCameraTransforms[3];
			}
			break;
		case CharacterSelect.Characters.Cedar:
			if (characterCameraTransforms[4] != null)
			{
				return characterCameraTransforms[4];
			}
			break;
		default:
			Debug.LogError("Error: Setting menu camera to look at non-existent character.");
			break;
		}
		if (characterCameraTransforms[0] != null)
		{
			return characterCameraTransforms[0];
		}
		return null;
	}
}
