using System;
using UnityEngine;

namespace BlackfireStudio;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ImageEffects : MonoBehaviour
{
	private static int screenHeight = -1;

	private static int screenwidth = -1;

	private static Texture2D renderTexture;

	public static Texture2D RenderTexture
	{
		get
		{
			if (renderTexture == null)
			{
				renderTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipmap: false);
			}
			return renderTexture;
		}
	}

	public static bool IsPro<T>(GameObject go, Type type, Shader s) where T : MonoBehaviour
	{
		return Application.HasProLicense();
	}

	public static void RenderImageEffect(Material m)
	{
		GL.PushMatrix();
		for (int i = 0; i < m.passCount; i++)
		{
			m.SetPass(i);
			GL.LoadOrtho();
			GL.Begin(7);
			GL.Color(new Color(1f, 1f, 1f, 1f));
			GL.MultiTexCoord(0, new Vector3(0f, 0f, 0f));
			GL.Vertex3(0f, 0f, 0f);
			GL.MultiTexCoord(0, new Vector3(0f, 1f, 0f));
			GL.Vertex3(0f, 1f, 0f);
			GL.MultiTexCoord(0, new Vector3(1f, 1f, 0f));
			GL.Vertex3(1f, 1f, 0f);
			GL.MultiTexCoord(0, new Vector3(1f, 0f, 0f));
			GL.Vertex3(1f, 0f, 0f);
			GL.End();
		}
		GL.PopMatrix();
	}

	public void OnPostRender()
	{
		if (Screen.width != screenwidth || Screen.height != screenHeight)
		{
			RenderTexture.Resize(Screen.width, Screen.height, TextureFormat.RGB24, hasMipMap: false);
			screenwidth = Screen.width;
			screenHeight = Screen.height;
		}
		RenderTexture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		RenderTexture.Apply();
	}
}
