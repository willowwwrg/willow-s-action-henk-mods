using Rapid.Tools;
using UnityEngine;

public class GraphExampleInGame : MonoBehaviour
{
	private GraphLogStyle _style;

	private GraphLogBuffer _buffer;

	private GraphGridInGame _drawer;

	public Material RenderMaterial;

	private int _screenWidth = -1;

	private bool _wasScreenSpace = true;

	private void Awake()
	{
		_style = new GraphLogStyle("MyStyle", Color.white, Color.cyan, new Color[3]
		{
			Color.red,
			Color.green,
			Color.blue
		});
		int bufferSize = 300;
		_buffer = new GraphLogBuffer("MyGraph", string.Empty, 2, new string[2] { "x", "y" }, null, LogTimeMode.TimeSinceStartup, _style, bufferSize);
		_drawer = new GraphGridInGame(base.camera, RenderMaterial, screenSpace: false, _buffer);
		_drawer.SetGridColor(Color.white);
		_drawer.Rows = 4;
		_wasScreenSpace = !_drawer.ScreenSpace;
	}

	private void UpdateAreaScreenSpace()
	{
		Vector2 vector = Vector2.one * 2f;
		_drawer.SetArea(new Vector2(40f, 40f), new Vector2((float)Screen.width - 40f, 240f), vector, vector);
		_screenWidth = Screen.width;
		_drawer.Columns = _screenWidth / 100;
	}

	private void UpdateAreaWorldSpace()
	{
		Vector2 vector = new Vector2(0.1f, 0.1f);
		_drawer.SetArea(new Vector2(-8f, -2f), new Vector2(8f, 2f), vector, vector);
		_drawer.Columns = 10;
	}

	private void Update()
	{
		if (_drawer.ScreenSpace)
		{
			if (!_wasScreenSpace || _screenWidth != Screen.width)
			{
				UpdateAreaScreenSpace();
			}
		}
		else if (_wasScreenSpace)
		{
			UpdateAreaWorldSpace();
		}
		_wasScreenSpace = _drawer.ScreenSpace;
		Vector2 vector = Input.mousePosition;
		_buffer.Log(vector.x, vector.y);
		_drawer.SetTimeValueBounds(_buffer.TimeStart, _buffer.TimeLast, _buffer.Min, _buffer.Max);
	}

	private void OnPostRender()
	{
		_drawer.Draw();
	}

	private void OnGUI()
	{
		_drawer.ScreenSpace = GUILayout.Toggle(_drawer.ScreenSpace, "Screen space");
	}
}
