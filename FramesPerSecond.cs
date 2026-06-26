using UnityEngine;

public class FramesPerSecond : MonoBehaviour
{
	private float updateInterval = 1f;

	private float seconds;

	private float frames;

	private string text = string.Empty;

	private GUIStyle style_label = new GUIStyle();

	private GUIStyle style_shadow = new GUIStyle();

	private Rect pos_label = new Rect(5f, 5f, 100f, 25f);

	private Rect pos_shadow = new Rect(6f, 6f, 100f, 25f);

	protected void Start()
	{
		Application.targetFrameRate = 60;
	}

	protected void OnGUI()
	{
		GUI.Label(pos_shadow, text, style_shadow);
		GUI.Label(pos_label, text, style_label);
	}

	protected void Update()
	{
		seconds += Time.deltaTime;
		frames += 1f;
		if (seconds >= updateInterval)
		{
			float num = frames / seconds;
			text = $"{num:F2} FPS";
			if (num < 30f)
			{
				style_label.normal.textColor = Color.yellow;
			}
			else if (num < 10f)
			{
				style_label.normal.textColor = Color.red;
			}
			else
			{
				style_label.normal.textColor = Color.green;
			}
			seconds = 0f;
			frames = 0f;
		}
	}
}
