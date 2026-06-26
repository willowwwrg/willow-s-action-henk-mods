using UnityEngine;

public class PPFXDemoScene : MonoBehaviour
{
	public PPFXSpawnOnClick spawnScript;

	public GameObject plane;

	public GameObject pyramid;

	public GameObject cameraRotate;

	public Camera cam;

	private bool hideGUI;

	private bool hidePlane;

	private bool rotateCamera;

	private float zoomSlider = 60f;

	private int selectedIndex;

	public Texture2D logo;

	public GameObject[] prefabs;

	public Texture2D[] previews;

	private void Start()
	{
		prefabs = Resources.LoadAll<GameObject>("Library");
		previews = Resources.LoadAll<Texture2D>("library");
		spawnScript.inst = prefabs[0];
	}

	private void OnGUI()
	{
		GUILayout.Label("Press H to hide/show GUI\nPress R to reset scene");
		if (!hideGUI)
		{
			if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 95, 50f, 50f), "<<"))
			{
				if (selectedIndex - 1 > -1)
				{
					selectedIndex--;
					spawnScript.inst = prefabs[selectedIndex];
				}
				else
				{
					selectedIndex = prefabs.Length - 1;
					spawnScript.inst = prefabs[selectedIndex];
				}
			}
			if (GUI.Button(new Rect(Screen.width / 2 + 50, Screen.height - 95, 50f, 50f), ">>"))
			{
				if (selectedIndex + 1 < prefabs.Length)
				{
					selectedIndex++;
					spawnScript.inst = prefabs[selectedIndex];
				}
				else
				{
					selectedIndex = 0;
					spawnScript.inst = prefabs[selectedIndex];
				}
			}
			GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height - 120, 200f, 100f), previews[selectedIndex]);
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 20, 200f, 20f), prefabs[selectedIndex].name);
			hidePlane = GUI.Toggle(new Rect(0f, Screen.height - 100, 150f, 20f), hidePlane, "Hide Plane");
			if (hidePlane)
			{
				plane.transform.renderer.material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
			}
			else
			{
				plane.transform.renderer.material.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
			}
			rotateCamera = GUI.Toggle(new Rect(0f, Screen.height - 80, 150f, 20f), rotateCamera, "Rotate Camera");
			GUI.Label(new Rect(0f, Screen.height - 60, 150f, 20f), "Field of view:");
			zoomSlider = GUI.HorizontalSlider(new Rect(0f, Screen.height - 40, 150f, 20f), zoomSlider, 60f, 125f);
			if (GUI.Button(new Rect(0f, Screen.height - 20, 150f, 20f), "Reset"))
			{
				Reset();
			}
		}
		if (rotateCamera)
		{
			cameraRotate.transform.Rotate(Vector3.up * Time.deltaTime * 5f);
		}
		cam.fieldOfView = (int)zoomSlider;
		GUI.DrawTexture(new Rect(Screen.width - 90, Screen.height - 155, 64f, 155f), logo);
	}

	private void Update()
	{
		if (Input.GetKeyDown("h"))
		{
			if (hideGUI)
			{
				hideGUI = false;
			}
			else
			{
				hideGUI = true;
			}
		}
		if (Input.GetKeyDown("r"))
		{
			Reset();
		}
	}

	private void Reset()
	{
		GameObject gameObject = GameObject.Find("_Container");
		if (gameObject != null)
		{
			Object.Destroy(gameObject.gameObject);
		}
		Object.Destroy(GameObject.FindWithTag("pyramid").gameObject);
		Object.Instantiate(pyramid);
		cameraRotate.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
	}
}
