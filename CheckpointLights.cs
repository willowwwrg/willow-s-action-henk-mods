using UnityEngine;

public class CheckpointLights : MonoBehaviour
{
	public Texture2D noLight;

	public Texture2D greenLight;

	public Texture2D redLight;

	private bool lightsFlickering = true;

	private void Start()
	{
	}

	private void Update()
	{
		if (lightsFlickering)
		{
			if (Mathf.Repeat(Time.time, 0.5f) > 0.25f)
			{
				base.renderer.materials[1].mainTexture = noLight;
				base.renderer.materials[2].mainTexture = redLight;
			}
			else
			{
				base.renderer.materials[1].mainTexture = redLight;
				base.renderer.materials[2].mainTexture = noLight;
			}
		}
	}

	public void ResetLights()
	{
		lightsFlickering = true;
	}

	public void TurnOnLights()
	{
		lightsFlickering = false;
		base.renderer.materials[1].mainTexture = greenLight;
		base.renderer.materials[2].mainTexture = greenLight;
	}
}
