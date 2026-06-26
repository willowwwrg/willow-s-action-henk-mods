using System;
using UnityEngine;

public class TOD_Time : MonoBehaviour
{
	public float DayLengthInMinutes = 30f;

	public bool ProgressDate = true;

	public bool ProgressMoonPhase = true;

	private TOD_Sky sky;

	protected void Start()
	{
		sky = GetComponent<TOD_Sky>();
	}

	protected void Update()
	{
		float num = DayLengthInMinutes * 60f;
		float num2 = num / 24f;
		float num3 = Time.deltaTime / num2;
		float num4 = Time.deltaTime / (30f * num) * 2f;
		sky.Cycle.Hour += num3;
		if (ProgressMoonPhase)
		{
			sky.Cycle.MoonPhase += num4;
			if (sky.Cycle.MoonPhase < -1f)
			{
				sky.Cycle.MoonPhase += 2f;
			}
			else if (sky.Cycle.MoonPhase > 1f)
			{
				sky.Cycle.MoonPhase -= 2f;
			}
		}
		if (!(sky.Cycle.Hour >= 24f))
		{
			return;
		}
		sky.Cycle.Hour = 0f;
		if (!ProgressDate)
		{
			return;
		}
		int num5 = DateTime.DaysInMonth(sky.Cycle.Year, sky.Cycle.Month);
		if (++sky.Cycle.Day > num5)
		{
			sky.Cycle.Day = 1;
			if (++sky.Cycle.Month > 12)
			{
				sky.Cycle.Month = 1;
				sky.Cycle.Year++;
			}
		}
	}
}
