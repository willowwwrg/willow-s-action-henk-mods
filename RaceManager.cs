using System.Collections;
using Henk;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
	private int lap;

	private float bestTime;

	private float lapStartTime;

	private float lap3Time;

	private float lap5Time;

	private float lap10Time;

	public UILabel cpLabel;

	public UILabel debugLabel;

	private void Start()
	{
		if (cpLabel == null || debugLabel == null)
		{
			MonoBehaviour.print("Were missing stuff for racemanager!");
			base.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (!(col.transform.root.name == "Player"))
		{
			return;
		}
		float currentTime = Singleton<Stopwatch>.SP.GetCurrentTime();
		if (lap != 0)
		{
			float num = currentTime - lapStartTime;
			StartCoroutine(ShowLapTime(num));
			if (num < bestTime || bestTime == 0f)
			{
				bestTime = num;
			}
		}
		if (lap == 3)
		{
			lap3Time = currentTime;
		}
		if (lap == 5)
		{
			lap5Time = currentTime;
		}
		if (lap == 10)
		{
			lap10Time = currentTime;
		}
		lap++;
		lapStartTime = currentTime;
	}

	private void Update()
	{
		debugLabel.text = "Lap nr: " + lap + "/10 \nBest Lap: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(bestTime);
		if (lap3Time != 0f)
		{
			debugLabel.text = debugLabel.text + "\n\n 3 Laps: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(lap3Time);
		}
		if (lap5Time != 0f)
		{
			debugLabel.text = debugLabel.text + "\n 5 Laps: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(lap5Time);
		}
		if (lap10Time != 0f)
		{
			debugLabel.text = debugLabel.text + "\n 10 Laps: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(lap10Time);
		}
	}

	private IEnumerator ShowLapTime(float lapTime)
	{
		if (lapTime < bestTime || bestTime == 0f)
		{
			cpLabel.color = Color.green;
		}
		else
		{
			cpLabel.color = Color.red;
		}
		cpLabel.text = "Lap: " + Singleton<HighscoreManager>.SP.ConvertTimeToString(lapTime);
		yield return new WaitForSeconds(1.5f);
		cpLabel.color = Color.red;
		cpLabel.text = string.Empty;
	}
}
