using UnityEngine;

public class ObjectShake : MonoBehaviour
{
	public float speed = 60f;

	public float intensity = 8f;

	public float lerpFactor = 2f;

	public bool nudgeX = true;

	public bool nudgeY = true;

	private float initialIntensity;

	public bool continuous;

	private void Awake()
	{
		if (!continuous)
		{
			base.enabled = false;
		}
		initialIntensity = intensity;
	}

	private void Update()
	{
		Vector3 localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z);
		if (nudgeX)
		{
			localPosition.x += Mathf.Sin(Time.time * speed) * intensity * 0.9f;
		}
		if (nudgeY)
		{
			localPosition.y += Mathf.Cos(Time.time * speed) * intensity;
		}
		base.transform.localPosition = localPosition;
		if (!continuous)
		{
			intensity = Mathf.Lerp(intensity, 0f, Time.deltaTime * lerpFactor);
			if (intensity < 0.1f)
			{
				intensity = 0f;
				base.enabled = false;
			}
		}
	}

	public void NudgeObject()
	{
		intensity = initialIntensity;
		base.enabled = true;
	}
}
