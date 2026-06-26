using System;
using UnityEngine;

namespace CodeStage.AntiCheat.Detectors;

[AddComponentMenu("")]
public abstract class ActDetectorBase : MonoBehaviour
{
	protected const string MENU_PATH = "GameObject/Create Other/Code Stage/Anti-Cheat Toolkit/";

	public bool autoDispose = true;

	public bool keepAlive = true;

	protected Action onDetection;

	protected virtual bool IsPlacedCorrectly(string componentName)
	{
		if (base.name == componentName && GetComponentsInChildren<Component>().Length == 2)
		{
			return base.transform.childCount == 0;
		}
		return false;
	}

	protected virtual bool Init(ActDetectorBase instance, string detectorName)
	{
		if (instance != null)
		{
			Debug.LogWarning("[ACT] Only one " + detectorName + " instance allowed!");
			UnityEngine.Object.Destroy(base.gameObject);
			return false;
		}
		if (!IsPlacedCorrectly(detectorName))
		{
			Debug.LogWarning("[ACT] " + detectorName + " is placed in scene incorrectly and will be auto-destroyed!\nPlease, use \"" + "GameObject/Create Other/Code Stage/Anti-Cheat Toolkit/".Replace("/", "->") + detectorName + "\" menu to correct this!");
			UnityEngine.Object.Destroy(base.gameObject);
			return false;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		return true;
	}

	private void OnDisable()
	{
		StopDetectionInternal();
	}

	private void OnApplicationQuit()
	{
		DisposeInternal();
	}

	private void OnLevelWasLoaded(int index)
	{
		if (!keepAlive)
		{
			DisposeInternal();
		}
	}

	protected abstract void StopDetectionInternal();

	protected virtual void DisposeInternal()
	{
		StopDetectionInternal();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
