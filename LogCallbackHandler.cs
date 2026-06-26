using System.Collections.Generic;
using UnityEngine;

public static class LogCallbackHandler
{
	private static readonly List<Application.LogCallback> callbacks;

	static LogCallbackHandler()
	{
		callbacks = new List<Application.LogCallback>();
		Application.RegisterLogCallback(HandleLog);
	}

	public static void RegisterLogCallback(Application.LogCallback logCallback)
	{
		callbacks.Add(logCallback);
	}

	public static void RemoveLogCallback(Application.LogCallback logCallback)
	{
		callbacks.Remove(logCallback);
	}

	private static void HandleLog(string condition, string stackTrace, LogType type)
	{
		for (int i = 0; i < callbacks.Count; i++)
		{
			callbacks[i](condition, stackTrace, type);
		}
	}
}
