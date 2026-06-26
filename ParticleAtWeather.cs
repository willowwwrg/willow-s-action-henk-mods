using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAtWeather : MonoBehaviour
{
	public TOD_Sky sky;

	public TOD_Weather.WeatherType type;

	public float fadeTime = 1f;

	private float lerpTime;

	private ParticleSystem particleComponent;

	private float particleEmission;

	protected void OnEnable()
	{
		if (!sky)
		{
			Debug.LogError("Sky instance reference not set. Disabling script.");
			base.enabled = false;
		}
		particleComponent = base.particleSystem;
		particleEmission = particleComponent.emissionRate;
	}

	protected void Update()
	{
		int num = ((sky.Components.Weather.Weather == type) ? 1 : (-1));
		lerpTime = Mathf.Clamp01(lerpTime + (float)num * Time.deltaTime / fadeTime);
		particleComponent.emissionRate = Mathf.Lerp(0f, particleEmission, lerpTime);
	}
}
