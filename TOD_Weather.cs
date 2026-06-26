using UnityEngine;

public class TOD_Weather : MonoBehaviour
{
	public enum CloudType
	{
		Custom,
		None,
		Few,
		Scattered,
		Broken,
		Overcast
	}

	public enum WeatherType
	{
		Custom,
		Clear,
		Storm,
		Dust,
		Fog
	}

	public float FadeTime = 10f;

	public CloudType Clouds;

	public WeatherType Weather;

	private float cloudBrightnessDefault;

	private float cloudDensityDefault;

	private float atmosphereFogDefault;

	private float cloudBrightness;

	private float cloudDensity;

	private float atmosphereFog;

	private float cloudSharpness;

	private TOD_Sky sky;

	protected void Start()
	{
		sky = GetComponent<TOD_Sky>();
		cloudBrightness = (cloudBrightnessDefault = sky.Clouds.Brightness);
		cloudDensity = (cloudDensityDefault = sky.Clouds.Density);
		atmosphereFog = (atmosphereFogDefault = sky.Atmosphere.Fogginess);
		cloudSharpness = sky.Clouds.Sharpness;
	}

	protected void Update()
	{
		if (Clouds != CloudType.Custom || Weather != WeatherType.Custom)
		{
			switch (Clouds)
			{
			case CloudType.Custom:
				cloudDensity = sky.Clouds.Density;
				cloudSharpness = sky.Clouds.Sharpness;
				break;
			case CloudType.None:
				cloudDensity = 0f;
				cloudSharpness = 1f;
				break;
			case CloudType.Few:
				cloudDensity = cloudDensityDefault;
				cloudSharpness = 6f;
				break;
			case CloudType.Scattered:
				cloudDensity = cloudDensityDefault;
				cloudSharpness = 3f;
				break;
			case CloudType.Broken:
				cloudDensity = cloudDensityDefault;
				cloudSharpness = 1f;
				break;
			case CloudType.Overcast:
				cloudDensity = cloudDensityDefault;
				cloudSharpness = 0.1f;
				break;
			}
			switch (Weather)
			{
			case WeatherType.Custom:
				cloudBrightness = sky.Clouds.Brightness;
				atmosphereFog = sky.Atmosphere.Fogginess;
				break;
			case WeatherType.Clear:
				cloudBrightness = cloudBrightnessDefault;
				atmosphereFog = atmosphereFogDefault;
				break;
			case WeatherType.Storm:
				cloudBrightness = 0.3f;
				atmosphereFog = 1f;
				break;
			case WeatherType.Dust:
				cloudBrightness = cloudBrightnessDefault;
				atmosphereFog = 0.5f;
				break;
			case WeatherType.Fog:
				cloudBrightness = cloudBrightnessDefault;
				atmosphereFog = 1f;
				break;
			}
			float t = Time.deltaTime / FadeTime;
			sky.Clouds.Brightness = Mathf.Lerp(sky.Clouds.Brightness, cloudBrightness, t);
			sky.Clouds.Density = Mathf.Lerp(sky.Clouds.Density, cloudDensity, t);
			sky.Clouds.Sharpness = Mathf.Lerp(sky.Clouds.Sharpness, cloudSharpness, t);
			sky.Atmosphere.Fogginess = Mathf.Lerp(sky.Atmosphere.Fogginess, atmosphereFog, t);
		}
	}
}
