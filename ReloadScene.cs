using UnityEngine;

public class ReloadScene : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUILayout.Button("Reload"))
		{
			Reload();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Reload();
		}
	}

	private void Reload()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
}
