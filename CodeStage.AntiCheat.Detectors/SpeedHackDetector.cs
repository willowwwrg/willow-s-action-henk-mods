using System;
using UnityEngine;

namespace CodeStage.AntiCheat.Detectors;

[AddComponentMenu("")]
public class SpeedHackDetector : ActDetectorBase
{
	private const string COMPONENT_NAME = "Speed Hack Detector";

	private const long TICKS_PER_SECOND = 10000000L;

	private const int THRESHOLD = 5000000;

	public float interval = 1f;

	public byte maxFalsePositives = 3;

	internal static bool isRunning;

	private static SpeedHackDetector instance;

	private int errorsCount;

	private long ticksOnStart;

	private long ticksOnStartVulnerable;

	private long prevTicks;

	public static SpeedHackDetector Instance
	{
		get
		{
			if (instance == null)
			{
				SpeedHackDetector speedHackDetector = (SpeedHackDetector)UnityEngine.Object.FindObjectOfType(typeof(SpeedHackDetector));
				if (speedHackDetector == null)
				{
					speedHackDetector = new GameObject("Speed Hack Detector").AddComponent<SpeedHackDetector>();
				}
				return speedHackDetector;
			}
			return instance;
		}
	}

	private SpeedHackDetector()
	{
	}

	public static void StartDetection(Action callback)
	{
		StartDetection(callback, Instance.interval);
	}

	public static void StartDetection(Action callback, float checkInterval)
	{
		StartDetection(callback, checkInterval, Instance.maxFalsePositives);
	}

	public static void StartDetection(Action callback, float checkInterval, byte maxErrors)
	{
		Instance.StartDetectionInternal(callback, checkInterval, maxErrors);
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
		if (Init(instance, "Speed Hack Detector"))
		{
			instance = this;
		}
	}

	private void StartDetectionInternal(Action callback, float checkInterval, byte maxErrors)
	{
		if (isRunning)
		{
			Debug.LogWarning("[ACT] Speed Hack Detector already running!");
			return;
		}
		onDetection = callback;
		interval = checkInterval;
		maxFalsePositives = maxErrors;
		ReadTicksOnStart();
		InvokeRepeating("OnTimer", checkInterval, checkInterval);
		errorsCount = 0;
		isRunning = true;
	}

	protected override void StopDetectionInternal()
	{
		if (isRunning)
		{
			CancelInvoke("OnTimer");
			onDetection = null;
			isRunning = false;
		}
	}

	protected override void DisposeInternal()
	{
		base.DisposeInternal();
		instance = null;
	}

	private void ReadTicksOnStart()
	{
		ticksOnStart = DateTime.UtcNow.Ticks;
		ticksOnStartVulnerable = (long)Environment.TickCount * 10000L;
	}

	private void OnTimer()
	{
		long num = 0L;
		num = DateTime.UtcNow.Ticks;
		long num2 = (long)Environment.TickCount * 10000L;
		if (Mathf.Abs(num - prevTicks) > (interval + 1f) * 10000000f)
		{
			ReadTicksOnStart();
		}
		prevTicks = num;
		if (!(Mathf.Abs(num2 - ticksOnStartVulnerable - (num - ticksOnStart)) > 5000000f))
		{
			return;
		}
		errorsCount++;
		Debug.LogWarning("[ACT] SpeedHackDetector: detection! Silent detections left: " + (maxFalsePositives - errorsCount));
		if (errorsCount > maxFalsePositives)
		{
			if (onDetection != null)
			{
				onDetection();
			}
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
