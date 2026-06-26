using System;
using UnityEngine;

public class GUIView : MonoBehaviour
{
	private const string PROGRESS_KEY = "Progress";

	private const string MUTED_KEY = "IsSoundMuted";

	private const string SCORE_KEY = "Highscore";

	private const string PLAYERNAME_KEY = "PlayerName";

	private float progress;

	private bool muted;

	private int score;

	private string playername = "Noname";

	private void Start()
	{
		RefreshData();
	}

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			RefreshData();
		}
	}

	private void OnGUI()
	{
		int num = 450;
		int num2 = 400;
		GUILayout.BeginArea(new Rect(Screen.width / 2 - num / 2, Screen.height / 2 - num2 / 2, num, num2));
		GUILayout.BeginVertical();
		string text = "Unfortunately, this will take a a few seconds. This is due Unity working different on a Mac :(";
		GUILayout.TextArea(string.Format("Instructions: {0} 1) Open the Advanced PlayerPrefs Window and dock it somewhere. {0} 2) Change the values in the scene using the gui widgets. {0} 3) Go back to the Advanced PlayerPrefs Window and click the refresh button. " + ((Application.platform != RuntimePlatform.OSXEditor) ? string.Empty : text) + " {0} 4) Observe that the values in the Advanced PlayerPrefs Window has changed to your scene input. {0}{0} 5) Now in the Advanced PlayerPrefs Window, change the values and save those changes {0} 6) Go give the scene focus by clicking in the sceneview. {0} 7) Watch the gui values update to your changes", Environment.NewLine));
		GUILayout.Space(12f);
		GUILayout.Label("Progress: " + (int)progress + "%");
		float a = GUILayout.HorizontalSlider(progress, 0f, 100f);
		if (!Mathf.Approximately(a, progress))
		{
			progress = a;
			SaveData();
		}
		GUILayout.Space(12f);
		bool flag = GUILayout.Toggle(muted, "Is Audio Muted?");
		if (flag != muted)
		{
			muted = flag;
			SaveData();
		}
		GUILayout.Space(12f);
		GUILayout.Label("Highscore: " + score);
		GUILayout.Space(12f);
		GUILayout.Label("Playername");
		string text2 = GUILayout.TextField(playername);
		if (text2 != playername)
		{
			playername = text2;
			SaveData();
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public void RefreshData()
	{
		progress = PlayerPrefs.GetFloat("Progress", 100f);
		muted = PlayerPrefs.GetString("IsSoundMuted", "true") == "true";
		score = PlayerPrefs.GetInt("Highscore", 123);
		playername = PlayerPrefs.GetString("PlayerName", "Noname");
	}

	public void SaveData()
	{
		PlayerPrefs.SetFloat("Progress", progress);
		PlayerPrefs.SetString("IsSoundMuted", (!muted) ? "false" : "true");
		PlayerPrefs.SetInt("Highscore", score);
		PlayerPrefs.SetString("PlayerName", playername);
	}
}
