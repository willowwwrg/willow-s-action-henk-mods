using System;
using System.IO;
using UnityEngine;

public class HiResScreenShots : MonoBehaviour
{
	public int resWidth = 3840;

	public int resHeight = 2160;

	private bool takeHiResShot;

	public static string ScreenShotName(int width, int height)
	{
		return string.Format("{0}/../screenshots/screen_{1}x{2}_{3}.png", Application.dataPath, width, height, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	public void TakeHiResShot()
	{
		takeHiResShot = true;
	}

	private void LateUpdate()
	{
		takeHiResShot |= Input.GetKeyDown("s");
		if (takeHiResShot)
		{
			RenderTexture renderTexture = new RenderTexture(resWidth, resHeight, 24);
			base.camera.targetTexture = renderTexture;
			Texture2D texture2D = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, mipmap: false);
			base.camera.Render();
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, resWidth, resHeight), 0, 0);
			base.camera.targetTexture = null;
			RenderTexture.active = null;
			UnityEngine.Object.Destroy(renderTexture);
			byte[] bytes = texture2D.EncodeToPNG();
			string text = ScreenShotName(resWidth, resHeight);
			if (!Directory.Exists(Application.dataPath + "/../screenshots/"))
			{
				Directory.CreateDirectory(Application.dataPath + "/../screenshots/");
			}
			File.WriteAllBytes(text, bytes);
			Debug.Log("Took screenshot to: " + text);
			takeHiResShot = false;
		}
	}
}
