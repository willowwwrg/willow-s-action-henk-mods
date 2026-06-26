using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Henk;

public class Stopwatch : Singleton<Stopwatch>
{
	private bool isRunning;

	private bool isPaused;

	public UILabel TimeLabel;

	private int originalFontSize;

	private ObscuredInt stopwatchTimer = 0;

	private int minutes;

	private int seconds;

	private int fraction;

	private void Update()
	{
		TimeLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(GetCurrentTime());
	}

	private void SlomoOn()
	{
		TimeLabel.GetComponent<TweenColor>().Play(forward: true);
		TimeLabel.GetComponent<TweenScale>().Play(forward: true);
	}

	private void SlomoOff()
	{
		TimeLabel.GetComponent<TweenColor>().Play(forward: false);
		TimeLabel.GetComponent<TweenScale>().Play(forward: false);
	}

	private void SlomoOffHard()
	{
		TimeLabel.GetComponent<TweenColor>().ResetToBeginning();
		TimeLabel.GetComponent<TweenColor>().enabled = false;
		TimeLabel.GetComponent<TweenScale>().ResetToBeginning();
		TimeLabel.GetComponent<TweenScale>().enabled = false;
	}

	public float GetCurrentTime()
	{
		return (float)(int)stopwatchTimer * 0.0001f;
	}

	public void StartTimer()
	{
		isRunning = true;
		if (!isPaused)
		{
			stopwatchTimer = 0;
		}
	}

	private void FixedUpdate()
	{
		if (isRunning && !isPaused)
		{
			float num = Time.fixedDeltaTime * 10000f;
			stopwatchTimer = (int)stopwatchTimer + (int)num;
		}
	}

	public void OffsetTime(int frames)
	{
		int num = (int)(Time.fixedDeltaTime * 10000f);
		stopwatchTimer = (int)stopwatchTimer + num * frames;
	}

	public void PauseResumeTimer()
	{
		isPaused = !isPaused;
	}

	public void StopTimer()
	{
		isRunning = false;
	}

	public void ResetTimer()
	{
		stopwatchTimer = 0;
		isPaused = false;
		isRunning = false;
	}
}
