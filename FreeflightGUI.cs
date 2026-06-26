using UnityEngine;

public class FreeflightGUI : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private Freeflight freeflight;

	[SerializeField]
	private Texture2D background;

	private GUIStyle style = new GUIStyle();

	private void Start()
	{
		freeflight = GetComponent<Freeflight>();
	}

	private void OnGUI()
	{
		style.normal.background = background;
		GUILayout.BeginArea(new Rect(16f, 16f, 192f, 24f));
		if (freeflight.enabled)
		{
			GUILayout.Box("Press escape to exit freeflight");
			if (Input.GetKey(KeyCode.Escape))
			{
				freeflight.enabled = false;
				Screen.showCursor = true;
				Screen.lockCursor = false;
			}
		}
		else if (GUILayout.Button("Click to active freeflight"))
		{
			freeflight.enabled = true;
			Screen.showCursor = false;
			Screen.lockCursor = true;
		}
		GUILayout.EndArea();
		GUI.backgroundColor = new Color(0f, 0f, 0f, 0.8f);
		GUI.Box(new Rect(16f, Screen.height - 128, 260f, 108f), string.Empty);
		GUILayout.BeginArea(new Rect(16f, Screen.height - 128, 260f, 108f), style);
		GUILayout.Label("Freeflight controls:");
		GUILayout.Label("WASD: move forward/backward and strafe");
		GUILayout.Label("QE: move up and down");
		GUILayout.Label("Mouse Wheel: +/- movement speed");
		GUILayout.EndArea();
	}
}
