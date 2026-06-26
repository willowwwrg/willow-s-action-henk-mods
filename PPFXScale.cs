using UnityEngine;

[ExecuteInEditMode]
public class PPFXScale : MonoBehaviour
{
	public float particleScale = 1f;

	private float prevScale;

	private void Start()
	{
		prevScale = particleScale;
	}

	private void Update()
	{
	}

	private void ScaleShurikenSystems(float _scaleFactor)
	{
	}

	private void ScaleLegacySystems(float _scaleFactor)
	{
	}

	private void ScaleTrailRenderers(float _scaleFactor)
	{
		TrailRenderer[] componentsInChildren = GetComponentsInChildren<TrailRenderer>();
		foreach (TrailRenderer obj in componentsInChildren)
		{
			obj.startWidth *= _scaleFactor;
			obj.endWidth *= _scaleFactor;
		}
	}

	private void ScaleShockwave(float _scaleFactor)
	{
		PPFXShockwave componentInChildren = GetComponentInChildren<PPFXShockwave>();
		if (componentInChildren != null)
		{
			componentInChildren.scale *= _scaleFactor;
		}
	}
}
