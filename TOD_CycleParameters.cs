using System;
using UnityEngine;

[Serializable]
public class TOD_CycleParameters
{
	public float Hour = 12f;

	public int Day = 1;

	public int Month = 3;

	public int Year = 2000;

	public float MoonPhase;

	public float Latitude;

	public float Longitude;

	public float UTC;

	public void CheckRange()
	{
		Year = Mathf.Clamp(Year, 1, 9999);
		Month = Mathf.Clamp(Month, 1, 12);
		Day = Mathf.Clamp(Day, 1, DateTime.DaysInMonth(Year, Month));
		Hour = Mathf.Repeat(Hour, 24f);
		Longitude = Mathf.Clamp(Longitude, -180f, 180f);
		Latitude = Mathf.Clamp(Latitude, -90f, 90f);
		MoonPhase = Mathf.Clamp(MoonPhase, -1f, 1f);
	}
}
