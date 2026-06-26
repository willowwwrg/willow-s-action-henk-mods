using UnityEngine;

public class LangDemo : MonoBehaviour
{
	private Vector2 scrollView;

	public GUISkin mySkin;

	private void OnGUI()
	{
		GUI.skin = mySkin;
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.Label(Language.Get("MAIN_WELCOME"), GUILayout.Width(300f));
		GUILayout.Label(Language.Get("MAIN_INTRO"));
		GUILayout.Space(20f);
		float num = (int)(Time.time % 4f * 25f);
		GUILayout.Label(Language.Get("MAIN_PROGRESSBAR").Replace("{X}", num + string.Empty));
		GUILayout.Label(Language.Get("MAIN_SELECT_LANGUAGE"));
		scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.Height(100f));
		string[] languages = Language.GetLanguages();
		foreach (string text in languages)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			if (GUILayout.Button(text, GUILayout.Width(50f)))
			{
				Language.SwitchLanguage(text);
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
		GUILayout.Label(Language.Get("TEST_CHARACTERS"));
	}

	private void ChangedLanguage(LanguageCode code)
	{
		Debug.Log("DEMO We switched to: " + code);
		mySkin.font = (Font)Language.GetAsset("font");
		Debug.Log("DEMO Font: " + mySkin.font);
	}
}
