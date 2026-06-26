using UnityEngine;

public class HueCycler : MonoBehaviour
{
	public Color startingColor = Color.white;

	public float cycleSpeed = 0.2f;

	public Projector projector;

	public Renderer lightMesh;

	private HSBColor cyclerColor;

	private void Start()
	{
		cyclerColor = new HSBColor(startingColor);
	}

	private void Update()
	{
		cyclerColor.h += cycleSpeed * Time.deltaTime;
		cyclerColor.h = Mathf.Repeat(cyclerColor.h, 1f);
		Color color = cyclerColor.ToColor();
		if ((bool)projector)
		{
			projector.material.color = color;
		}
		if ((bool)lightMesh)
		{
			color.a = lightMesh.renderer.material.GetColor("_TintColor").a;
			lightMesh.renderer.material.SetColor("_TintColor", color);
		}
	}
}
