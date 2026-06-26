using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public abstract class TOD_PostEffectsBase : MonoBehaviour
{
	protected bool isSupported = true;

	protected abstract bool CheckResources();

	protected Material CheckShaderAndCreateMaterial(Shader shader, Material material)
	{
		if (!shader)
		{
			Debug.Log("Missing shader in " + ToString());
			base.enabled = false;
			return null;
		}
		if (shader.isSupported && (bool)material && material.shader == shader)
		{
			return material;
		}
		if (!shader.isSupported)
		{
			NotSupported();
			Debug.LogError("The shader " + shader.ToString() + " on effect " + ToString() + " is not supported on this platform!");
			return null;
		}
		material = new Material(shader);
		material.hideFlags = HideFlags.DontSave;
		if ((bool)material)
		{
			return material;
		}
		return null;
	}

	protected Material CreateMaterial(Shader shader, Material material)
	{
		if (!shader)
		{
			Debug.Log("Missing shader in " + ToString());
			return null;
		}
		if ((bool)material && material.shader == shader && shader.isSupported)
		{
			return material;
		}
		if (!shader.isSupported)
		{
			return null;
		}
		material = new Material(shader);
		material.hideFlags = HideFlags.DontSave;
		if ((bool)material)
		{
			return material;
		}
		return null;
	}

	protected void OnEnable()
	{
		isSupported = true;
	}

	protected void Start()
	{
		CheckResources();
	}

	protected bool CheckSupport(bool needDepth)
	{
		isSupported = true;
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
		{
			NotSupported();
			return false;
		}
		if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			NotSupported();
			return false;
		}
		if (needDepth)
		{
			base.camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		return true;
	}

	protected bool CheckSupport(bool needDepth, bool needHdr)
	{
		if (!CheckSupport(needDepth))
		{
			return false;
		}
		if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			NotSupported();
			return false;
		}
		return true;
	}

	protected void ReportAutoDisable()
	{
		Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
	}

	protected void NotSupported()
	{
		base.enabled = false;
		isSupported = false;
	}

	protected void DrawBorder(RenderTexture dest, Material material)
	{
		RenderTexture.active = dest;
		bool flag = true;
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int i = 0; i < material.passCount; i++)
		{
			material.SetPass(i);
			float y;
			float y2;
			if (flag)
			{
				y = 1f;
				y2 = 0f;
			}
			else
			{
				y = 0f;
				y2 = 1f;
			}
			float x = 0f + 1f / ((float)dest.width * 1f);
			float y3 = 0f;
			float y4 = 1f;
			GL.Begin(7);
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			float x2 = 1f - 1f / ((float)dest.width * 1f);
			x = 1f;
			y3 = 0f;
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			x = 1f;
			y3 = 0f;
			y4 = 0f + 1f / ((float)dest.height * 1f);
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			x = 1f;
			y3 = 1f - 1f / ((float)dest.height * 1f);
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			GL.End();
		}
		GL.PopMatrix();
	}
}
