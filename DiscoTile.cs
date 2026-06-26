using UnityEngine;

public class DiscoTile : MonoBehaviour
{
	public HSBColor color;

	private int colorID;

	private int glowColorID;

	public float cycleSpeed;

	private bool isOn = true;

	private float lastTime;

	public float onChance = 0.8f;

	private DiscoFloor parent;

	public void Start()
	{
		colorID = Shader.PropertyToID("_Color");
		glowColorID = Shader.PropertyToID("_GlowColor");
		parent = base.transform.parent.GetComponent<DiscoFloor>();
	}

	public void Update()
	{
		if (isOn)
		{
			this.color.h += cycleSpeed * Time.deltaTime;
			this.color.h = Mathf.Repeat(this.color.h, 1f);
			Color color = this.color.ToColor();
			base.renderer.material.SetColor(colorID, color);
			base.renderer.material.SetColor(glowColorID, color);
		}
		if (!(Camera.main != null))
		{
			return;
		}
		float num = Singleton<AudioManager>.SP.GetCurrentMusicTime() - 0.45f;
		if (num < 0f)
		{
			num = 0f;
		}
		float num2 = 132f;
		float length = 60f / num2;
		float num3 = Mathf.Repeat(num, length);
		if (num3 < lastTime)
		{
			bool turnOn = Random.value < onChance;
			TurnOnOrOff(turnOn);
			if (parent.state == DiscoFloorState.Brabant)
			{
				if (this.color.s == 1f)
				{
					this.color.s = 0f;
				}
				else
				{
					this.color.s = 1f;
				}
			}
		}
		lastTime = num3;
	}

	public void TurnOnOrOff(bool turnOn)
	{
		isOn = turnOn;
		if (!isOn)
		{
			Color black = Color.black;
			base.renderer.material.SetColor(colorID, black);
			base.renderer.material.SetColor(glowColorID, black);
		}
	}
}
