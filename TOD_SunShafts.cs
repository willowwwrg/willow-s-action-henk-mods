using UnityEngine;

[AddComponentMenu("Time of Day/Camera Sun Shafts")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
internal class TOD_SunShafts : TOD_PostEffectsBase
{
	public enum SunShaftsResolution
	{
		Low,
		Normal,
		High
	}

	public enum SunShaftsBlendMode
	{
		Screen,
		Add
	}

	private const int PASS_DEPTH = 0;

	private const int PASS_NODEPTH = 1;

	private const int PASS_RADIAL = 2;

	private const int PASS_SCREEN = 3;

	private const int PASS_ADD = 4;

	public TOD_Sky sky;

	public SunShaftsResolution Resolution = SunShaftsResolution.Normal;

	public SunShaftsBlendMode BlendMode;

	public int RadialBlurIterations = 2;

	public float SunShaftBlurRadius = 2f;

	public float SunShaftIntensity = 1f;

	public float MaxRadius = 1f;

	public bool UseDepthTexture = true;

	public Shader SunShaftsShader;

	public Shader ScreenClearShader;

	private Material sunShaftsMaterial;

	private Material screenClearMaterial;

	protected void OnDisable()
	{
		if ((bool)sunShaftsMaterial)
		{
			Object.DestroyImmediate(sunShaftsMaterial);
		}
		if ((bool)screenClearMaterial)
		{
			Object.DestroyImmediate(screenClearMaterial);
		}
	}

	protected override bool CheckResources()
	{
		CheckSupport(UseDepthTexture);
		sunShaftsMaterial = CheckShaderAndCreateMaterial(SunShaftsShader, sunShaftsMaterial);
		screenClearMaterial = CheckShaderAndCreateMaterial(ScreenClearShader, screenClearMaterial);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		return isSupported;
	}

	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckResources())
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (UseDepthTexture)
		{
			base.camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		int width;
		int height;
		if (Resolution == SunShaftsResolution.High)
		{
			width = source.width;
			height = source.height;
		}
		else if (Resolution == SunShaftsResolution.Normal)
		{
			width = source.width / 2;
			height = source.height / 2;
		}
		else
		{
			width = source.width / 4;
			height = source.height / 4;
		}
		Vector3 vector = ((!sky) ? new Vector3(0.5f, 0.5f, 0f) : base.camera.WorldToViewportPoint(sky.Components.SunTransform.position));
		sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * SunShaftBlurRadius);
		sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, MaxRadius));
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0);
		if (UseDepthTexture)
		{
			Graphics.Blit(source, temporary, sunShaftsMaterial, 0);
		}
		else
		{
			Graphics.Blit(source, temporary, sunShaftsMaterial, 1);
		}
		DrawBorder(temporary, screenClearMaterial);
		float num = SunShaftBlurRadius * 0.0013020834f;
		sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num, num, 0f, 0f));
		sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, MaxRadius));
		for (int i = 0; i < RadialBlurIterations; i++)
		{
			Graphics.Blit(temporary, temporary2, sunShaftsMaterial, 2);
			num = SunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num, num, 0f, 0f));
			Graphics.Blit(temporary2, temporary, sunShaftsMaterial, 2);
			num = SunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num, num, 0f, 0f));
		}
		Vector4 vector2 = ((!((double)vector.z >= 0.0) || !sky) ? Vector4.zero : ((1f - sky.Atmosphere.Fogginess) * SunShaftIntensity * (Vector4)sky.SunColor));
		sunShaftsMaterial.SetVector("_SunColor", vector2);
		sunShaftsMaterial.SetTexture("_ColorBuffer", temporary);
		if (BlendMode == SunShaftsBlendMode.Screen)
		{
			Graphics.Blit(source, destination, sunShaftsMaterial, 3);
		}
		else
		{
			Graphics.Blit(source, destination, sunShaftsMaterial, 4);
		}
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
