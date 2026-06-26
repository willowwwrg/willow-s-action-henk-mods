using System.Collections;
using UnityEngine;

public class FlickeringMaterial : MonoBehaviour
{
	private bool flickering;

	public float initialDelay;

	private void Start()
	{
		base.renderer.enabled = false;
	}

	private void Update()
	{
		if (RenderSettings.fogDensity > 0.01f)
		{
			if (!flickering)
			{
				StartCoroutine("FlickerRoutine");
			}
		}
		else
		{
			base.renderer.enabled = false;
			StopCoroutine("FlickerRoutine");
			flickering = false;
		}
	}

	private IEnumerator FlickerRoutine()
	{
		flickering = true;
		yield return new WaitForSeconds(initialDelay);
		base.renderer.enabled = true;
		yield return new WaitForSeconds(0.5f);
		base.renderer.enabled = false;
		yield return new WaitForSeconds(0.2f);
		base.renderer.enabled = true;
		yield return new WaitForSeconds(1f);
		base.renderer.enabled = false;
		flickering = false;
	}
}
