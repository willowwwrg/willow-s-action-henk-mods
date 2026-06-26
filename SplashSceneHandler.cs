using System.Collections;
using UnityEngine;

public class SplashSceneHandler : MonoBehaviour
{
	public PlayMovie playMovieScript;

	private bool loading;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.transform.root.gameObject);
		StartCoroutine("SplashSequence");
	}

	private void Update()
	{
		if (!loading && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton16) || Input.GetKeyDown(KeyCode.JoystickButton0)))
		{
			StartLoadingMainMenu();
		}
	}

	private void StartLoadingMainMenu()
	{
		loading = true;
		StopCoroutine("SplashSequence");
		playMovieScript.Stop();
		Application.LoadLevelAsync("MenuScene");
	}

	private IEnumerator SplashSequence()
	{
		yield return new WaitForSeconds(0.5f);
		playMovieScript.Play();
		yield return new WaitForSeconds(8.5f);
		StartLoadingMainMenu();
	}
}
