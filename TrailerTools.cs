using System.Collections;
using UnityEngine;

public class TrailerTools : MonoBehaviour
{
	public bool NoGUI = true;

	public int levelNumber;

	public CharacterSelect.Characters characterToPlay = CharacterSelect.Characters.Henk;

	public int characterSkin;

	public CameraPathAnimator[] cameraPaths;

	private void Awake()
	{
		Singleton<CharacterSelect>.SP.SetSelectedCharacter(characterToPlay);
		Singleton<CharacterSelect>.SP.SetSelectedSkinNum(characterSkin);
	}

	private IEnumerator Start()
	{
		Singleton<LevelBatchManager>.SP.SetCurrentLevel(levelNumber);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (NoGUI)
		{
			Object.FindObjectOfType<State_PreGame>().SelectGhost(GhostType.None, 0uL);
			GameObject.Find("3DCamera").camera.enabled = false;
			Object.FindObjectOfType<UICamera>().camera.enabled = false;
		}
	}

	private void Update()
	{
		UpdateCameraPaths();
		if (Input.GetKeyDown(KeyCode.Q))
		{
			PlayRainbowGhost();
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
			player.GetComponent<ReplayController>().Stop();
			player.GetComponent<ReplayController>().Play();
		}
	}

	private void PlayRainbowGhost()
	{
		GameObject player = Singleton<PlayerManager>.SP.GetPlayer();
		player.GetComponent<ReplayController>().LoadReplay(GhostType.MedalRainbow, 0uL);
		player.GetComponent<ReplayController>().Stop();
		player.GetComponent<ReplayController>().Play();
	}

	private void UpdateCameraPaths()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1) && cameraPaths.Length != 0)
		{
			cameraPaths[0].Stop();
			cameraPaths[0].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha2) && cameraPaths.Length > 1)
		{
			cameraPaths[1].Stop();
			cameraPaths[1].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha3) && cameraPaths.Length > 2)
		{
			cameraPaths[2].Stop();
			cameraPaths[2].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha4) && cameraPaths.Length > 3)
		{
			cameraPaths[3].Stop();
			cameraPaths[3].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha5) && cameraPaths.Length > 4)
		{
			cameraPaths[4].Stop();
			cameraPaths[4].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha6) && cameraPaths.Length > 5)
		{
			cameraPaths[5].Stop();
			cameraPaths[5].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha7) && cameraPaths.Length > 6)
		{
			cameraPaths[6].Stop();
			cameraPaths[6].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha8) && cameraPaths.Length > 7)
		{
			cameraPaths[7].Stop();
			cameraPaths[7].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha9) && cameraPaths.Length > 8)
		{
			cameraPaths[8].Stop();
			cameraPaths[8].Play();
		}
		if (Input.GetKeyDown(KeyCode.Alpha0) && cameraPaths.Length > 9)
		{
			cameraPaths[9].Stop();
			cameraPaths[9].Play();
		}
	}
}
