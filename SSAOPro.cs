using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/SSAO Pro")]
[ExecuteInEditMode]
public class SSAOPro : MonoBehaviour
{
	public enum BlurMode
	{
		None,
		Gaussian,
		Bilateral
	}

	public enum SampleCount
	{
		VeryLow,
		Low,
		Medium,
		High,
		Ultra
	}

	public enum AOMode
	{
		V11,
		V12
	}

	public AOMode Mode = AOMode.V12;

	public Texture2D NoiseTexture;

	public bool UseHighPrecisionDepthMap = true;

	public SampleCount Samples = SampleCount.Medium;

	[Range(1f, 4f)]
	public int Downsampling = 1;

	[Range(0.01f, 1.25f)]
	public float Radius = 0.125f;

	[Range(0f, 16f)]
	public float Intensity = 2f;

	[Range(0f, 10f)]
	public float Distance = 1f;

	[Range(0f, 1f)]
	public float Bias = 0.1f;

	[Range(0f, 1f)]
	public float LumContribution = 0.5f;

	public Color OcclusionColor = Color.black;

	public float CutoffDistance = 150f;

	public float CutoffFalloff = 50f;

	public BlurMode Blur;

	public bool BlurDownsampling;

	public bool DebugAO;

	protected Shader m_ShaderSSAO_v1;

	protected Shader m_ShaderSSAO_v2;

	protected Shader m_ShaderHighPrecisionDepth;

	protected Material m_Material_v1;

	protected Material m_Material_v2;

	protected Camera m_Camera;

	protected Camera m_RWSCamera;

	protected RenderTextureFormat m_RTFormat = RenderTextureFormat.RFloat;

	private string[] keywords = new string[2];

	public Material Material
	{
		get
		{
			if (Mode == AOMode.V11)
			{
				if (m_Material_v1 == null)
				{
					m_Material_v1 = new Material(ShaderSSAO);
					m_Material_v1.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_Material_v1;
			}
			if (m_Material_v2 == null)
			{
				m_Material_v2 = new Material(ShaderSSAO);
				m_Material_v2.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Material_v2;
		}
	}

	public Shader ShaderSSAO
	{
		get
		{
			if (Mode == AOMode.V11)
			{
				if (m_ShaderSSAO_v1 == null)
				{
					m_ShaderSSAO_v1 = Shader.Find("Hidden/SSAO Pro V1");
				}
				return m_ShaderSSAO_v1;
			}
			if (m_ShaderSSAO_v2 == null)
			{
				m_ShaderSSAO_v2 = Shader.Find("Hidden/SSAO Pro V2");
			}
			return m_ShaderSSAO_v2;
		}
	}

	public Shader ShaderHighPrecisionDepth
	{
		get
		{
			if (m_ShaderHighPrecisionDepth == null)
			{
				m_ShaderHighPrecisionDepth = Shader.Find("Hidden/SSAO Pro - High Precision Depth Map");
			}
			return m_ShaderHighPrecisionDepth;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.LogWarning("Image Effects are not supported on this platform.");
			base.enabled = false;
		}
		else if (SystemInfo.supportsRenderTextures)
		{
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
			{
				if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
				{
					Debug.LogWarning("RFloat && Depth RenderTextures are not supported on this platform.");
					base.enabled = false;
					return;
				}
				m_RTFormat = RenderTextureFormat.Depth;
			}
			if (ShaderSSAO != null && !ShaderSSAO.isSupported)
			{
				Debug.LogWarning("Unsupported shader (SSAO).");
				base.enabled = false;
			}
			else if (ShaderHighPrecisionDepth != null && !ShaderHighPrecisionDepth.isSupported)
			{
				Debug.LogWarning("Unsupported shader (High Precision Depth Map).");
				base.enabled = false;
			}
		}
		else
		{
			Debug.LogWarning("RenderTextures are not supported on this platform.");
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		m_Camera = GetComponent<Camera>();
	}

	private void OnDestroy()
	{
		if (m_Material_v1 != null)
		{
			Object.DestroyImmediate(m_Material_v1);
		}
		if (m_Material_v2 != null)
		{
			Object.DestroyImmediate(m_Material_v2);
		}
		if (m_RWSCamera != null)
		{
			Object.DestroyImmediate(m_RWSCamera.gameObject);
		}
	}

	private void OnPreRender()
	{
		if (UseHighPrecisionDepthMap)
		{
			if (m_RWSCamera == null)
			{
				GameObject gameObject = new GameObject("Depth Normal Camera", typeof(Camera));
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				m_RWSCamera = gameObject.GetComponent<Camera>();
				m_RWSCamera.CopyFrom(m_Camera);
				m_RWSCamera.renderingPath = RenderingPath.Forward;
				m_RWSCamera.clearFlags = CameraClearFlags.Color;
				m_RWSCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
				m_RWSCamera.enabled = false;
			}
			m_RWSCamera.CopyFrom(m_Camera);
			m_RWSCamera.rect = new Rect(0f, 0f, 1f, 1f);
			m_RWSCamera.renderingPath = RenderingPath.Forward;
			m_RWSCamera.clearFlags = CameraClearFlags.Color;
			m_RWSCamera.backgroundColor = new Color(1f, 1f, 1f, 1f);
			m_RWSCamera.farClipPlane = CutoffDistance;
			RenderTexture temporary = RenderTexture.GetTemporary((int)m_Camera.pixelWidth, (int)m_Camera.pixelHeight, 24, m_RTFormat);
			temporary.filterMode = FilterMode.Bilinear;
			m_RWSCamera.targetTexture = temporary;
			m_RWSCamera.RenderWithShader(m_ShaderHighPrecisionDepth, "RenderType");
			temporary.SetGlobalShaderProperty("_DepthNormalMapF32");
			m_RWSCamera.targetTexture = null;
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ShaderSSAO == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (ShaderHighPrecisionDepth == null && UseHighPrecisionDepthMap)
		{
			Graphics.Blit(source, destination);
			return;
		}
		int pass = SetShaderStates();
		if (Mode == AOMode.V11)
		{
			Material.SetMatrix("_InverseViewProject", m_Camera.projectionMatrix.inverse);
		}
		else
		{
			Material.SetMatrix("_InverseViewProject", (m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix).inverse);
			Material.SetMatrix("_CameraModelView", m_Camera.cameraToWorldMatrix);
		}
		Material.SetTexture("_NoiseTex", NoiseTexture);
		Material.SetVector("_Params1", new Vector4((!(NoiseTexture == null)) ? ((float)NoiseTexture.width) : 0f, Radius, Intensity, Distance));
		Material.SetVector("_Params2", new Vector4(Bias, LumContribution, CutoffDistance, CutoffFalloff));
		Material.SetColor("_OcclusionColor", OcclusionColor);
		if (Blur == BlurMode.None)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width / Downsampling, source.height / Downsampling, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(temporary, temporary, Material, 0);
			if (DebugAO)
			{
				Graphics.Blit(source, temporary, Material, pass);
				Graphics.Blit(temporary, destination);
				RenderTexture.ReleaseTemporary(temporary);
			}
			else
			{
				Graphics.Blit(source, temporary, Material, pass);
				Material.SetTexture("_SSAOTex", temporary);
				Graphics.Blit(source, destination, Material, 7);
				RenderTexture.ReleaseTemporary(temporary);
			}
			return;
		}
		int pass2 = ((Blur != BlurMode.Bilateral) ? 5 : 6);
		int num = ((!BlurDownsampling) ? 1 : Downsampling);
		RenderTexture temporary2 = RenderTexture.GetTemporary(source.width / num, source.height / num, 0, RenderTextureFormat.ARGB32);
		RenderTexture temporary3 = RenderTexture.GetTemporary(source.width / Downsampling, source.height / Downsampling, 0, RenderTextureFormat.ARGB32);
		Graphics.Blit(temporary2, temporary2, Material, 0);
		Graphics.Blit(source, temporary2, Material, pass);
		Material.SetVector("_Direction", new Vector2(1f / (float)source.width, 0f));
		Graphics.Blit(temporary2, temporary3, Material, pass2);
		Material.SetVector("_Direction", new Vector2(0f, 1f / (float)source.height));
		if (!DebugAO)
		{
			Graphics.Blit(temporary3, temporary2, Material, pass2);
			Material.SetTexture("_SSAOTex", temporary2);
			Graphics.Blit(source, destination, Material, 7);
		}
		else
		{
			Graphics.Blit(temporary3, destination, Material, pass2);
		}
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
	}

	private int SetShaderStates()
	{
		if (!UseHighPrecisionDepthMap)
		{
			m_Camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		m_Camera.depthTextureMode |= DepthTextureMode.DepthNormals;
		keywords[0] = ((Samples == SampleCount.Low) ? "SAMPLES_LOW" : ((Samples == SampleCount.Medium) ? "SAMPLES_MEDIUM" : ((Samples == SampleCount.High) ? "SAMPLES_HIGH" : ((Samples != SampleCount.Ultra) ? "SAMPLES_VERY_LOW" : "SAMPLES_ULTRA"))));
		keywords[1] = ((!UseHighPrecisionDepthMap) ? "HIGH_PRECISION_DEPTHMAP_OFF" : "HIGH_PRECISION_DEPTHMAP_ON");
		Material.shaderKeywords = keywords;
		int num = 0;
		if (NoiseTexture != null)
		{
			num = 1;
		}
		if (LumContribution >= 0.001f)
		{
			num += 2;
		}
		return 1 + num;
	}
}
