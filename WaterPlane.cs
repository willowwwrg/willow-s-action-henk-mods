using UnityEngine;

public class WaterPlane : MonoBehaviour
{
	public Shader opaque;

	public Shader transparent;

	private void Start()
	{
		if ((bool)base.renderer && (bool)Camera.main && (bool)Camera.main.GetComponent<CameraEffectsManager>())
		{
			if (Camera.main.GetComponent<CameraEffectsManager>().enableDepthEffects)
			{
				base.renderer.material.shader = Shader.Find("FlowmapGenerator/Depth Fog/Water Foam");
			}
			else
			{
				base.renderer.material.shader = Shader.Find("FlowmapGenerator/Opaque/Water Foam");
			}
		}
	}
}
