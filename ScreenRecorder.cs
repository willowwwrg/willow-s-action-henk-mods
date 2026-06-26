using System.IO;
using UnityEngine;

public class ScreenRecorder : MonoBehaviour
{
	public string folder = "C:/ScreenshotMovieOutput";

	public int frameRate = 30;

	public int sizeMultiplier = 1;

	private string realFolder = string.Empty;

	private void Start()
	{
		Time.captureFramerate = frameRate;
		realFolder = folder;
		int num = 1;
		while (Directory.Exists(realFolder))
		{
			realFolder = folder + num;
			num++;
		}
		Directory.CreateDirectory(realFolder);
	}

	private void Update()
	{
		Application.CaptureScreenshot($"{realFolder}/shot {Time.frameCount:D04}.png", sizeMultiplier);
	}
}
