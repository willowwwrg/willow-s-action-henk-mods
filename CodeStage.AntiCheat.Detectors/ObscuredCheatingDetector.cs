using System;
using UnityEngine;

namespace CodeStage.AntiCheat.Detectors;

[AddComponentMenu("")]
public class ObscuredCheatingDetector : ActDetectorBase
{
	private const string COMPONENT_NAME = "Obscured Cheating Detector";

	internal static bool isRunning;

	private static ObscuredCheatingDetector instance;

	public static ObscuredCheatingDetector Instance
	{
		get
		{
			if (instance == null)
			{
				ObscuredCheatingDetector obscuredCheatingDetector = (ObscuredCheatingDetector)UnityEngine.Object.FindObjectOfType(typeof(ObscuredCheatingDetector));
				if (obscuredCheatingDetector == null)
				{
					obscuredCheatingDetector = new GameObject("Obscured Cheating Detector").AddComponent<ObscuredCheatingDetector>();
				}
				return obscuredCheatingDetector;
			}
			return instance;
		}
	}

	private ObscuredCheatingDetector()
	{
	}

	public static void StartDetection(Action callback)
	{
		Instance.StartDetectionInternal(callback);
	}

	public static void StopDetection()
	{
		Instance.StopDetectionInternal();
	}

	public static void Dispose()
	{
		Instance.DisposeInternal();
	}

	private void Awake()
	{
		if (Init(instance, "Obscured Cheating Detector"))
		{
			instance = this;
		}
	}

	private void StartDetectionInternal(Action callback)
	{
		if (isRunning)
		{
			Debug.LogWarning("[ACT] Obscured Cheating Detector already running!");
			return;
		}
		onDetection = callback;
		isRunning = true;
	}

	protected override void StopDetectionInternal()
	{
		if (isRunning)
		{
			onDetection = null;
			isRunning = false;
		}
	}

	protected override void DisposeInternal()
	{
		base.DisposeInternal();
		instance = null;
	}

	internal void OnCheatingDetected()
	{
		if (onDetection != null)
		{
			onDetection();
			if (autoDispose)
			{
				Dispose();
			}
			else
			{
				StopDetection();
			}
		}
	}
}
