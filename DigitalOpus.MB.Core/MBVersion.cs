using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MBVersion
{
	public static string version => "2.11.6";

	public static int GetMajorVersion()
	{
		return 4;
	}

	public static int GetMinorVersion()
	{
		return 0;
	}

	public static bool GetActive(GameObject go)
	{
		return go.activeInHierarchy;
	}

	public static void SetActive(GameObject go, bool isActive)
	{
		go.SetActive(isActive);
	}

	public static void SetActiveRecursively(GameObject go, bool isActive)
	{
		go.SetActive(isActive);
	}

	public static UnityEngine.Object[] FindSceneObjectsOfType(Type t)
	{
		return UnityEngine.Object.FindObjectsOfType(t);
	}
}
