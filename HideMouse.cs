using UnityEngine;

public class HideMouse : MonoBehaviour
{
	private void Start()
	{
		Screen.showCursor = false;
		Screen.lockCursor = true;
	}
}
