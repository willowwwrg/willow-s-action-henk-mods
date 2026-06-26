using System;
using UnityEngine;

[AddComponentMenu("")]
public class AmplifyColorBase : MonoBehaviour
{
	public float BlendAmount;

	private RenderTexture blendCacheLut;

	private bool blending;

	private float blendingTime;

	private float blendingTimeCountdown;

	private ColorSpace colorSpace = ColorSpace.Uninitialized;

	internal bool JustCopy;

	public Texture2D LutTexture;

	private Texture lutTexture3d = new Texture();

	public Texture2D LutBlendTexture;

	private Texture lutBlendTexture3d = new Texture();

	public Texture MaskTexture;

	private Material materialBase;

	private Material materialBlend;

	private Material materialBlendCache;

	private Material materialBlendMask;

	private Material materialMask;

	private Texture2D normalLut;

	private Action onFinishBlend;

	private Shader shaderBase;

	private Shader shaderBlend;

	private Shader shaderBlendCache;

	private Shader shaderBlendMask;

	private Shader shaderMask;

	private bool use3d;

	public bool IsBlending => blending;

	public bool WillItBlend
	{
		get
		{
			if (LutTexture != null && LutBlendTexture != null)
			{
				return !blending;
			}
			return false;
		}
	}

	public void BlendTo(Texture2D blendTargetLUT, float blendTimeInSec, Action onFinishBlend)
	{
		LutBlendTexture = blendTargetLUT;
		BlendAmount = 0f;
		this.onFinishBlend = onFinishBlend;
		blendingTime = blendTimeInSec;
		blendingTimeCountdown = blendTimeInSec;
		blending = true;
	}

	private bool CheckMaterialAndShader(Material material, string name)
	{
		if (material == null || material.shader == null)
		{
			Debug.LogError("[AmplifyColor] Error creating " + name + " material. Effect disabled.");
			base.enabled = false;
		}
		else if (!material.shader.isSupported)
		{
			Debug.LogError("[AmplifyColor] " + name + " shader not supported on this platform. Effect disabled.");
			base.enabled = false;
		}
		else
		{
			material.hideFlags = HideFlags.HideAndDontSave;
		}
		return base.enabled;
	}

	private bool CheckShader(Shader s)
	{
		if (s == null)
		{
			ReportMissingShaders();
			return false;
		}
		if (!s.isSupported)
		{
			ReportNotSupported();
			return false;
		}
		return true;
	}

	private bool CheckShaders()
	{
		if (CheckShader(shaderBase) && CheckShader(shaderBlend) && CheckShader(shaderBlendCache) && CheckShader(shaderMask))
		{
			return CheckShader(shaderBlendMask);
		}
		return false;
	}

	private bool CheckSupport()
	{
		if (SystemInfo.supportsImageEffects && SystemInfo.supportsRenderTextures)
		{
			return true;
		}
		ReportNotSupported();
		return false;
	}

	private void CreateHelperTextures()
	{
		int num = 1024;
		int num2 = 32;
		ReleaseTextures();
		RenderTexture renderTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		blendCacheLut = renderTexture;
		blendCacheLut.name = "BlendCacheLut";
		blendCacheLut.wrapMode = TextureWrapMode.Clamp;
		blendCacheLut.useMipMap = false;
		blendCacheLut.anisoLevel = 0;
		blendCacheLut.Create();
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, mipmap: false, linear: true)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		normalLut = texture2D;
		normalLut.name = "NormalLut";
		normalLut.hideFlags = HideFlags.DontSave;
		Color32[] array = new Color32[num * num2];
		for (int i = 0; i < 32; i++)
		{
			int num3 = i * 32;
			for (int j = 0; j < 32; j++)
			{
				int num4 = num3 + j * num;
				for (int k = 0; k < 32; k++)
				{
					float num5 = (float)k / 31f;
					float num6 = (float)j / 31f;
					float num7 = (float)i / 31f;
					byte r = (byte)(num5 * 255f);
					byte g = (byte)(num6 * 255f);
					byte b = (byte)(num7 * 255f);
					ref Color32 reference = ref array[num4 + k];
					reference = new Color32(r, g, b, byte.MaxValue);
				}
			}
		}
		normalLut.SetPixels32(array);
		normalLut.Apply();
	}

	private void CreateMaterials()
	{
		SetupShader();
		ReleaseMaterials();
		materialBase = new Material(shaderBase);
		materialBlend = new Material(shaderBlend);
		materialBlendCache = new Material(shaderBlendCache);
		materialMask = new Material(shaderMask);
		materialBlendMask = new Material(shaderBlendMask);
		CheckMaterialAndShader(materialBase, "BaseMaterial");
		CheckMaterialAndShader(materialBlend, "BlendMaterial");
		CheckMaterialAndShader(materialBlendCache, "BlendCacheMaterial");
		CheckMaterialAndShader(materialMask, "MaskMaterial");
		CheckMaterialAndShader(materialBlendMask, "BlendMaskMaterial");
		if (base.enabled)
		{
			CreateHelperTextures();
		}
	}

	private void OnDisable()
	{
		ReleaseMaterials();
		ReleaseTextures();
	}

	private void OnEnable()
	{
		if (CheckSupport())
		{
			CreateMaterials();
			if ((LutTexture != null && LutTexture.mipmapCount > 1) || (LutBlendTexture != null && LutBlendTexture.mipmapCount > 1))
			{
				Debug.LogError("[AmplifyColor] Please disable \"Generate Mip Maps\" import settings on all LUT textures to avoid visual glitches. Change Texture Type to \"Advanced\" to access Mip settings.");
			}
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		BlendAmount = Mathf.Clamp01(BlendAmount);
		if (colorSpace != QualitySettings.activeColorSpace)
		{
			CreateMaterials();
		}
		bool flag = ValidateLutDimensions(LutTexture);
		bool flag2 = ValidateLutDimensions(LutBlendTexture);
		if (JustCopy || !flag || !flag2)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if ((LutTexture == null && lutTexture3d == null) || (BlendAmount == 1f && LutBlendTexture == null))
		{
			Graphics.Blit(source, destination);
			return;
		}
		int pass = (base.camera.hdr ? 1 : 0);
		bool flag3 = BlendAmount != 0f;
		int num;
		if (!flag3)
		{
			if (!flag3)
			{
				num = 0;
				goto IL_00f6;
			}
			if (!(LutBlendTexture != null))
			{
				num = ((lutBlendTexture3d != null) ? 1 : 0);
				if (num == 0)
				{
					goto IL_00f6;
				}
			}
			else
			{
				num = 1;
			}
		}
		else
		{
			num = 1;
		}
		int num2 = ((!use3d) ? 1 : 0);
		goto IL_00f9;
		IL_00f6:
		num2 = 0;
		goto IL_00f9;
		IL_00f9:
		bool num3 = (byte)num2 != 0;
		Material material = ((num == 0) ? ((!(MaskTexture != null)) ? materialBase : materialMask) : ((!(MaskTexture != null)) ? materialBlend : materialBlendMask));
		material.SetFloat("_lerpAmount", BlendAmount);
		if (MaskTexture != null)
		{
			material.SetTexture("_MaskTex", MaskTexture);
		}
		if (num3)
		{
			materialBlendCache.SetFloat("_lerpAmount", BlendAmount);
			materialBlendCache.SetTexture("_RgbTex", LutTexture);
			materialBlendCache.SetTexture("_LerpRgbTex", (!(LutBlendTexture != null)) ? normalLut : LutBlendTexture);
			Graphics.Blit(LutTexture, blendCacheLut, materialBlendCache);
			material.SetTexture("_RgbBlendCacheTex", blendCacheLut);
		}
		else if (!use3d)
		{
			if (LutTexture != null)
			{
				material.SetTexture("_RgbTex", LutTexture);
			}
			if (LutBlendTexture != null)
			{
				material.SetTexture("_LerpRgbTex", LutBlendTexture);
			}
		}
		Graphics.Blit(source, destination, material, pass);
		if (num3)
		{
			blendCacheLut.DiscardContents();
		}
	}

	private void ReleaseMaterials()
	{
		if (materialBase != null)
		{
			UnityEngine.Object.DestroyImmediate(materialBase);
			materialBase = null;
		}
		if (materialBlend != null)
		{
			UnityEngine.Object.DestroyImmediate(materialBlend);
			materialBlend = null;
		}
		if (materialBlendCache != null)
		{
			UnityEngine.Object.DestroyImmediate(materialBlendCache);
			materialBlendCache = null;
		}
		if (materialMask != null)
		{
			UnityEngine.Object.DestroyImmediate(materialMask);
			materialMask = null;
		}
		if (materialBlendMask != null)
		{
			UnityEngine.Object.DestroyImmediate(materialBlendMask);
			materialBlendMask = null;
		}
	}

	private void ReleaseTextures()
	{
		if (blendCacheLut != null)
		{
			UnityEngine.Object.DestroyImmediate(blendCacheLut);
			blendCacheLut = null;
		}
		if (normalLut != null)
		{
			UnityEngine.Object.DestroyImmediate(normalLut);
			normalLut = null;
		}
	}

	private void ReportMissingShaders()
	{
		Debug.LogError("[AmplifyColor] Error initializing shaders. Please reinstall Amplify Color.");
		base.enabled = false;
	}

	private void ReportNotSupported()
	{
		Debug.LogError("[AmplifyColor] This image effect is not supported on this platform. Please make sure your Unity license supports Full-Screen Post-Processing Effects which is usually reserved forn Pro licenses.");
		base.enabled = false;
	}

	private void SetupShader()
	{
		colorSpace = QualitySettings.activeColorSpace;
		string text = ((colorSpace != ColorSpace.Linear) ? string.Empty : "Linear");
		string empty = string.Empty;
		shaderBase = Shader.Find("Hidden/Amplify Color/Base" + text + empty);
		shaderBlend = Shader.Find("Hidden/Amplify Color/Blend" + text + empty);
		shaderBlendCache = Shader.Find("Hidden/Amplify Color/BlendCache");
		shaderMask = Shader.Find("Hidden/Amplify Color/Mask" + text + empty);
		shaderBlendMask = Shader.Find("Hidden/Amplify Color/BlendMask" + text + empty);
	}

	private void Update()
	{
		if (blending)
		{
			BlendAmount = (blendingTime - blendingTimeCountdown) / blendingTime;
			blendingTimeCountdown -= Time.smoothDeltaTime;
			if (BlendAmount >= 1f)
			{
				LutTexture = LutBlendTexture;
				BlendAmount = 0f;
				blending = false;
				LutBlendTexture = null;
				if (onFinishBlend != null)
				{
					onFinishBlend();
				}
			}
		}
		else
		{
			BlendAmount = Mathf.Clamp01(BlendAmount);
		}
	}

	public static bool ValidateLutDimensions(Texture2D lut)
	{
		if (lut != null)
		{
			if (lut.width / lut.height != lut.height)
			{
				Debug.LogWarning("[AmplifyColor] Lut " + lut.name + " has invalid dimensions.");
				return false;
			}
			if (lut.anisoLevel != 0)
			{
				lut.anisoLevel = 0;
			}
		}
		return true;
	}
}
