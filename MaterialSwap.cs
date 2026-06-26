using UnityEngine;

public class MaterialSwap : MonoBehaviour
{
	public int material1;

	public int material2 = 1;

	public ParticleSystem particle1;

	public ParticleSystem particle2;

	private Material mat1;

	private Material mat2;

	public float switchTime = 1f;

	private float timeToSwitch;

	private bool swapBack;

	private void Start()
	{
		mat1 = base.renderer.materials[material1];
		mat2 = base.renderer.materials[material2];
	}

	private void Update()
	{
		timeToSwitch -= Time.deltaTime;
		if (timeToSwitch <= 0f)
		{
			timeToSwitch = switchTime;
			Material[] materials = base.renderer.materials;
			if (swapBack)
			{
				materials[material1] = mat1;
				materials[material2] = mat2;
				particle2.Play();
			}
			else
			{
				materials[material1] = mat2;
				materials[material2] = mat1;
				particle1.Play();
			}
			swapBack = !swapBack;
			base.renderer.materials = materials;
		}
	}
}
