using System;
using Rapid.Tools;
using UnityEngine;

public class GraphExample : MonoBehaviour
{
	public Texture2D descriptionImage;

	private float _oldDeltaTime;

	private void Awake()
	{
		Graph.Initialize();
		Graph.Instance.AddStyle(new GraphLogStyle("blue", new Color(0.5f, 0.5f, 1f), Color.cyan, new Color[2]
		{
			new Color(0.5f, 0.2f, 0.92f, 1f),
			new Color(0.2f, 0.1f, 0.86f, 1f)
		}));
		Graph.Instance.CreateLog("sin_cos", new string[2] { "sin", "cos" }, "blue");
		Graph.Instance.CreateLog("deltatime_times_100", new string[2] { "delta", "smooth delta" });
	}

	private void OnApplicationQuit()
	{
		Graph.Dispose();
	}

	private void Update()
	{
		Graph.Log("mouse", (Vector2)Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			Graph.LogEvent("mouse", "Clicked LEFT mouse button!");
		}
		if (Input.GetMouseButtonDown(1))
		{
			Graph.LogEvent("mouse", "Clicked RIGHT mouse button!", Color.yellow);
		}
		float num = Math.Min(Time.deltaTime * 100f, 10f);
		Graph.Log("deltatime_times_100", num, Time.smoothDeltaTime * 100f);
		if (num - _oldDeltaTime >= 5f)
		{
			Graph.LogEvent("deltatime_times_100", "A framerate spike occured.");
		}
		_oldDeltaTime = num;
		if (Time.timeSinceLevelLoad < 20f)
		{
			Graph.Log("sin_cos", Mathf.Sin(Time.time), Mathf.Cos(Time.time));
		}
	}

	private void OnGUI()
	{
		GUILayout.Space(40f);
		GUILayout.Label("    Please open the Graph editor window by clicking the menu item \"Window->Graph Debugger\" to see the debugger in action (while the application is running).");
		GUILayout.Space(40f);
		GUILayout.Box(new GUIContent(descriptionImage));
		GUILayout.Space(40f);
		GUILayout.Label("    By default, all variables recorded gets streamed to different log files to your \"Project\\Logs\" folder with the .graphlog extension.");
		GUILayout.Label("    After you stop running the application, you will be able to load in the graphs to look at them closer by zooming in with the mouse wheel and dragging the timeline with the left mouse button.");
		GUILayout.Label("    The code that logs the variables in this example is inside the script called \"GraphExample.cs\" inside the Examples folder.");
		GUILayout.Space(40f);
		GUILayout.Label("    Please email me at rapidgamedev@gmail.com if you experience any bugs or issues, or for general feedback & questions.");
		if (GUILayout.Button("Email me", GUILayout.Width(100f)))
		{
			Application.OpenURL("mailto:rapidgamedev@gmail.com");
		}
	}
}
