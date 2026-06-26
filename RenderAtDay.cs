using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RenderAtDay : MonoBehaviour
{
	public TOD_Sky sky;

	private Renderer rendererComponent;

	protected void OnEnable()
	{
		if (!sky)
		{
			Debug.LogError("Sky instance reference not set. Disabling script.");
			base.enabled = false;
		}
		rendererComponent = base.renderer;
	}

	protected void Update()
	{
		rendererComponent.enabled = sky.IsDay;
	}
}
