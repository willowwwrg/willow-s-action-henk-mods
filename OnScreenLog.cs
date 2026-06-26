using UnityEngine;

public class OnScreenLog : MonoBehaviour
{
	private static string myLog = string.Empty;

	private string output = string.Empty;

	public static OnScreenLog SP;

	public GUISkin mSkin;

	public bool stopOnError = true;

	private bool handle = true;

	private void Awake()
	{
		SP = this;
		LogCallbackHandler.RegisterLogCallback(HandleLog);
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (handle)
		{
			output = logString;
			string text = myLog;
			myLog = text + "\n\n" + output + "\n" + stackTrace;
			if ((type == LogType.Error || type == LogType.Exception) && stopOnError)
			{
				handle = false;
			}
		}
	}

	private void OnGUI()
	{
		GUI.skin = mSkin;
		GUI.Box(new Rect(10f, 10f, Screen.width - 20, Screen.height - 20), myLog);
	}
}
