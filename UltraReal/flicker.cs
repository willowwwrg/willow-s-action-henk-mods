using UnityEngine;

namespace UltraReal;

public class flicker : MonoBehaviour
{
	public float lightIntensity = 1f;

	public AnimationCurveProperty flickerCurve;

	private void Update()
	{
		base.light.intensity = lightIntensity * flickerCurve.EvaluateStep(Time.deltaTime);
	}
}
