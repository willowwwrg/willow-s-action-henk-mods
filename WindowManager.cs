using UnityEngine;

public class WindowManager : Singleton<WindowManager>
{
	public void SetFullscreen(bool state)
	{
		Screen.fullScreen = state;
	}

	public void SetResolution(int width, int height)
	{
		Screen.SetResolution(width, height, Screen.fullScreen);
	}
}
