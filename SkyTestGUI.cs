using UnityEngine;

public class SkyTestGUI : MonoBehaviour
{
	private const float label_width = 100f;

	public TOD_Sky sky;

	public bool fog;

	public bool sunShafts;

	public bool progressTime;

	private Rect rect = new Rect(-3f, -3f, 200f, 456f);

	private string[] cloudTypes = new string[5]
	{
		TOD_Weather.CloudType.None.ToString(),
		TOD_Weather.CloudType.Few.ToString(),
		TOD_Weather.CloudType.Scattered.ToString(),
		TOD_Weather.CloudType.Broken.ToString(),
		TOD_Weather.CloudType.Overcast.ToString()
	};

	private string[] weatherTypes = new string[4]
	{
		TOD_Weather.WeatherType.Clear.ToString(),
		TOD_Weather.WeatherType.Storm.ToString(),
		TOD_Weather.WeatherType.Dust.ToString(),
		TOD_Weather.WeatherType.Fog.ToString()
	};

	protected void OnEnable()
	{
		if (!sky)
		{
			Debug.LogError("Sky instance reference not set. Disabling script.");
			base.enabled = false;
		}
	}

	protected void OnGUI()
	{
		GUILayout.BeginArea(rect, string.Empty, "Box");
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Time of Day", GUILayout.Width(100f));
		sky.Cycle.Hour = GUILayout.HorizontalSlider(sky.Cycle.Hour, 0f, 24f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Moon Phase", GUILayout.Width(100f));
		sky.Cycle.MoonPhase = GUILayout.HorizontalSlider(sky.Cycle.MoonPhase, -1f, 1f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Sky Contrast", GUILayout.Width(100f));
		sky.Atmosphere.Contrast = GUILayout.HorizontalSlider(sky.Atmosphere.Contrast, 0.5f, 1.5f);
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		if (progressTime)
		{
			GUILayout.BeginHorizontal();
			sky.Components.Time.enabled = GUILayout.Toggle(sky.Components.Time.enabled, " Progress Time");
			GUILayout.EndHorizontal();
		}
		if (sunShafts)
		{
			GUILayout.BeginHorizontal();
			TOD_SunShafts component = GetComponent<TOD_SunShafts>();
			component.enabled = GUILayout.Toggle(component.enabled, " Sun Shafts");
			GUILayout.EndHorizontal();
		}
		if (fog)
		{
			GUILayout.BeginHorizontal();
			RenderSettings.fog = GUILayout.Toggle(RenderSettings.fog, " Enable Fog");
			GUILayout.EndHorizontal();
		}
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("CLOUDS");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		int clouds = GUILayout.SelectionGrid((int)(sky.Components.Weather.Clouds - 1), cloudTypes, 1) + 1;
		sky.Components.Weather.Clouds = (TOD_Weather.CloudType)clouds;
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("WEATHER");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		int weather = GUILayout.SelectionGrid((int)(sky.Components.Weather.Weather - 1), weatherTypes, 1) + 1;
		sky.Components.Weather.Weather = (TOD_Weather.WeatherType)weather;
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
