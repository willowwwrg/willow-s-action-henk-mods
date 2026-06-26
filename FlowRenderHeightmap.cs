using Flowmap;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Flowmaps/Heightmap/Render From Scene")]
[RequireComponent(typeof(FlowmapGenerator))]
public class FlowRenderHeightmap : FlowHeightmap
{
	public int resolutionX = 256;

	public int resolutionY = 256;

	public FluidDepth fluidDepth;

	public float heightMax = 1f;

	public float heightMin = 1f;

	public LayerMask cullingMask = 1;

	public bool dynamicUpdating;

	private Camera renderingCamera;

	private RenderTexture heightmap;

	private Material compareMaterial;

	private Material resizeMaterial;

	public static bool Supported => SystemInfo.supportsRenderTextures;

	public static string UnsupportedReason
	{
		get
		{
			string result = string.Empty;
			if (!SystemInfo.supportsRenderTextures)
			{
				result = "System doesn't support RenderTextures.";
			}
			return result;
		}
	}

	public override Texture HeightmapTexture
	{
		get
		{
			return heightmap;
		}
		set
		{
			Debug.LogWarning("Can't set HeightmapTexture.");
		}
	}

	public override Texture PreviewHeightmapTexture => HeightmapTexture;

	private Shader ClippedHeightShader => Shader.Find("Hidden/DepthToHeightClipped");

	private Shader HeightShader => Shader.Find("Hidden/DepthToHeight");

	private Material CompareMaterial
	{
		get
		{
			if (!compareMaterial)
			{
				compareMaterial = new Material(Shader.Find("Hidden/DepthCompare"));
				compareMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return compareMaterial;
		}
	}

	private Material ResizeMaterial
	{
		get
		{
			if (!resizeMaterial)
			{
				resizeMaterial = new Material(Shader.Find("Hidden/RenderHeightmapResize"));
				resizeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return resizeMaterial;
		}
	}

	private void Awake()
	{
		UpdateHeightmap();
	}

	public void UpdateHeightmap()
	{
		if (heightmap == null || heightmap.width != resolutionX || heightmap.height != resolutionY)
		{
			heightmap = new RenderTexture(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
			heightmap.hideFlags = HideFlags.HideAndDontSave;
		}
		if (renderingCamera == null)
		{
			renderingCamera = new GameObject("Render Heightmap", typeof(Camera)).GetComponent<Camera>();
			renderingCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
			renderingCamera.enabled = false;
			renderingCamera.renderingPath = RenderingPath.Forward;
			renderingCamera.clearFlags = CameraClearFlags.Color;
			renderingCamera.backgroundColor = Color.black;
			renderingCamera.orthographic = true;
		}
		renderingCamera.cullingMask = cullingMask;
		renderingCamera.transform.rotation = Quaternion.identity;
		renderingCamera.orthographicSize = Mathf.Max(base.Generator.Dimensions.x, base.Generator.Dimensions.y) * 0.5f;
		renderingCamera.transform.position = base.Generator.transform.position + Vector3.up * heightMax;
		renderingCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
		switch (fluidDepth)
		{
		case FluidDepth.DeepWater:
		{
			RenderTexture temporary = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.sRGB);
			RenderTexture temporary2 = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.sRGB);
			RenderTexture temporary3 = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.sRGB);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - heightMin);
			renderingCamera.targetTexture = temporary;
			renderingCamera.nearClipPlane = 0.01f;
			renderingCamera.farClipPlane = 100f;
			renderingCamera.RenderWithShader(ClippedHeightShader, "RenderType");
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - heightMin);
			renderingCamera.nearClipPlane = heightMax;
			renderingCamera.farClipPlane = heightMin + heightMax;
			renderingCamera.targetTexture = temporary2;
			renderingCamera.RenderWithShader(HeightShader, "RenderType");
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - heightMin);
			renderingCamera.nearClipPlane = 0.01f;
			renderingCamera.farClipPlane = heightMin + heightMax;
			renderingCamera.targetTexture = temporary3;
			renderingCamera.RenderWithShader(HeightShader, "RenderType");
			CompareMaterial.SetTexture("_OverhangMaskTex", temporary);
			CompareMaterial.SetTexture("_HeightBelowSurfaceTex", temporary2);
			CompareMaterial.SetTexture("_HeightIntersectingTex", temporary3);
			Graphics.Blit(null, heightmap, CompareMaterial);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary3);
			break;
		}
		case FluidDepth.Surface:
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", base.Generator.transform.position.y - heightMin);
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", base.Generator.transform.position.y + heightMax);
			renderingCamera.nearClipPlane = 0.001f;
			renderingCamera.farClipPlane = heightMin + heightMax;
			renderingCamera.targetTexture = heightmap;
			renderingCamera.RenderWithShader(HeightShader, "RenderType");
			break;
		}
		if (base.Generator.Dimensions.x != base.Generator.Dimensions.y)
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
			ResizeMaterial.SetTexture("_Heightmap", heightmap);
			if (base.Generator.Dimensions.y > base.Generator.Dimensions.x)
			{
				ResizeMaterial.SetVector("_AspectRatio", new Vector4(base.Generator.Dimensions.x / base.Generator.Dimensions.y, 1f, 0f, 0f));
			}
			else
			{
				ResizeMaterial.SetVector("_AspectRatio", new Vector4(1f, 1f / (base.Generator.Dimensions.x / base.Generator.Dimensions.y), 0f, 0f));
			}
			Graphics.Blit(null, temporary4, ResizeMaterial, 0);
			Graphics.Blit(temporary4, heightmap);
			RenderTexture.ReleaseTemporary(temporary4);
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * (heightMax - heightMin) / 2f, new Vector3(base.Generator.Dimensions.x, heightMax + heightMin, base.Generator.Dimensions.y));
	}
}
