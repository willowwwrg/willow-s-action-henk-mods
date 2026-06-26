using System;
using UnityEngine;

public class DeviceTime : MonoBehaviour
{
	public TOD_Sky sky;

	protected void OnEnable()
	{
		if (!sky)
		{
			Debug.LogError("Sky instance reference not set. Disabling script.");
			base.enabled = false;
			return;
		}
		DateTime now = DateTime.Now;
		sky.Cycle.Year = now.Year;
		sky.Cycle.Month = now.Month;
		sky.Cycle.Day = now.Day;
		sky.Cycle.Hour = (float)now.Hour + (float)now.Minute / 60f;
	}
}
