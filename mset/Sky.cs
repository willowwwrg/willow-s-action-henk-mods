using System;
using UnityEngine;

namespace mset;

[Serializable]
public class Sky : MonoBehaviour
{
	public static Sky activeSky;

	public Cubemap diffuseCube;

	public Cubemap specularCube;

	public Cubemap skyboxCube;

	public float masterIntensity = 1f;

	public float skyIntensity = 1f;

	public float specIntensity = 1f;

	public float diffIntensity = 1f;

	public float camExposure = 1f;

	public float specIntensityLM = 1f;

	public float diffIntensityLM = 1f;

	public bool hdrSky;

	public bool hdrSpec;

	public bool hdrDiff;

	public bool showSkybox = true;

	public bool linearSpace = true;

	public bool autoDetectColorSpace = true;

	public bool hasDimensions;

	public SHEncoding SH;

	private Matrix4x4 skyMatrix = Matrix4x4.identity;

	private Vector4 exposures = Vector4.one;

	private Vector2 exposuresLM = Vector2.one;

	private float hdrScale = 1f;

	private Material _skyboxMaterial;

	private Cubemap _blackCube;

	private Material skyboxMaterial
	{
		get
		{
			if (_skyboxMaterial == null)
			{
				Shader shader = Shader.Find("Hidden/Marmoset/Skybox IBL");
				if ((bool)shader)
				{
					_skyboxMaterial = new Material(shader);
					_skyboxMaterial.name = "Internal IBL Skybox";
				}
				else
				{
					Debug.LogError("Failed to create IBL Skybox material. Missing shader?");
				}
			}
			return _skyboxMaterial;
		}
	}

	private Cubemap blackCube
	{
		get
		{
			if (_blackCube == null)
			{
				_blackCube = new Cubemap(16, TextureFormat.ARGB32, mipmap: true);
				for (int i = 0; i < 6; i++)
				{
					for (int j = 0; j < 16; j++)
					{
						for (int k = 0; k < 16; k++)
						{
							_blackCube.SetPixel((CubemapFace)i, j, k, Color.black);
						}
					}
				}
				_blackCube.Apply(updateMipmaps: true);
			}
			return _blackCube;
		}
	}

	public void Apply()
	{
		Apply(null);
	}

	public void Apply(Renderer target)
	{
		if (!base.enabled || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (target == null)
		{
			if (activeSky != null)
			{
				activeSky.UnApply();
			}
			activeSky = this;
			ToggleChildLights(enable: true);
			UpdateExposures();
			ApplySkybox();
			Shader.DisableKeyword("MARMO_GAMMA");
			Shader.DisableKeyword("MARMO_LINEAR");
			if (linearSpace)
			{
				Shader.EnableKeyword("MARMO_LINEAR");
			}
			else
			{
				Shader.EnableKeyword("MARMO_GAMMA");
			}
			Shader.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
			Shader.DisableKeyword("MARMO_BOX_PROJECTION");
			if (hasDimensions)
			{
				Shader.EnableKeyword("MARMO_BOX_PROJECTION");
			}
			else
			{
				Shader.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
			}
		}
		else
		{
			Material material = target.material;
			material.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
			material.DisableKeyword("MARMO_BOX_PROJECTION");
			if (hasDimensions)
			{
				material.EnableKeyword("MARMO_BOX_PROJECTION");
			}
			else
			{
				material.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
			}
		}
		ApplyExposures(target);
		ApplyIBL(target);
		ApplySkyTransform(target);
	}

	public void ApplySkyTransform()
	{
		ApplySkyTransform(null);
	}

	public void ApplySkyTransform(Renderer target)
	{
		if (base.enabled && base.gameObject.activeInHierarchy)
		{
			if (target == null)
			{
				UpdateSkyTransform();
				Shader.SetGlobalMatrix("SkyMatrix", skyMatrix);
				Shader.SetGlobalMatrix("InvSkyMatrix", skyMatrix.inverse);
				Shader.SetGlobalVector("SkyPosition", base.transform.position);
				Shader.SetGlobalVector("_SkySize", 0.5f * base.transform.localScale);
				Shader.SetGlobalFloat("_UseBoxProjection", (!hasDimensions) ? 1f : 1f);
			}
			else
			{
				target.material.SetMatrix("SkyMatrix", skyMatrix);
				target.material.SetMatrix("InvSkyMatrix", skyMatrix.inverse);
				target.material.SetVector("SkyPosition", base.transform.position);
				target.material.SetVector("_SkySize", 0.5f * base.transform.localScale);
				target.material.SetFloat("_UseBoxProjection", (!hasDimensions) ? 1f : 1f);
			}
		}
	}

	public static void SetUniformOcclusion(Renderer target, float diffuse, float specular)
	{
		Vector4 one = Vector4.one;
		one.x = diffuse;
		one.y = specular;
		target.material.SetVector("UniformOcclusion", one);
	}

	public void ToggleChildLights(bool enable)
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enable;
		}
	}

	private void UnApply()
	{
		ToggleChildLights(enable: false);
	}

	private void UpdateSkyTransform()
	{
		skyMatrix.SetTRS(base.transform.position, base.transform.rotation, Vector3.one);
	}

	private void UpdateExposures()
	{
		exposures.x = masterIntensity * diffIntensity;
		exposures.y = masterIntensity * specIntensity;
		exposures.z = masterIntensity * skyIntensity * camExposure;
		exposures.w = camExposure;
		float num = 2.2f;
		float p = 1f / num;
		hdrScale = 6f;
		if (linearSpace)
		{
			hdrScale = Mathf.Pow(6f, num);
		}
		else
		{
			exposures.x = Mathf.Pow(exposures.x, p);
			exposures.y = Mathf.Pow(exposures.y, p);
			exposures.z = Mathf.Pow(exposures.z, p);
			exposures.w = Mathf.Pow(exposures.w, p);
		}
		if (hdrDiff)
		{
			exposures.x *= hdrScale;
		}
		if (hdrSpec)
		{
			exposures.y *= hdrScale;
		}
		if (hdrSky)
		{
			exposures.z *= hdrScale;
		}
		exposuresLM.x = diffIntensityLM;
		exposuresLM.y = specIntensityLM;
	}

	private void ApplyExposures(Renderer target)
	{
		if (target == null)
		{
			Shader.SetGlobalVector("ExposureIBL", exposures);
			Shader.SetGlobalVector("ExposureLM", exposuresLM);
			Shader.SetGlobalFloat("_EmissionLM", 1f);
			Shader.SetGlobalVector("UniformOcclusion", Vector4.one);
		}
		else
		{
			target.material.SetVector("ExposureIBL", exposures);
			target.material.SetVector("ExposureLM", exposuresLM);
		}
	}

	private void ApplyIBL(Renderer target)
	{
		float value = 1f / hdrScale / (float)Math.PI;
		if (target == null)
		{
			if ((bool)diffuseCube)
			{
				Shader.SetGlobalTexture("_DiffCubeIBL", diffuseCube);
			}
			else
			{
				Shader.SetGlobalTexture("_DiffCubeIBL", blackCube);
			}
			if ((bool)specularCube)
			{
				Shader.SetGlobalTexture("_SpecCubeIBL", specularCube);
			}
			else
			{
				Shader.SetGlobalTexture("_SpecCubeIBL", blackCube);
			}
			if ((bool)skyboxCube)
			{
				Shader.SetGlobalTexture("_SkyCubeIBL", skyboxCube);
			}
			else
			{
				Shader.SetGlobalTexture("_SkyCubeIBL", blackCube);
			}
			if (SH != null)
			{
				for (uint num = 0u; num < 9; num++)
				{
					Shader.SetGlobalVector("_SH" + num, SH.cBuffer[num]);
				}
				Shader.SetGlobalFloat("_SHScale", value);
			}
			return;
		}
		if ((bool)diffuseCube)
		{
			target.material.SetTexture("_DiffCubeIBL", diffuseCube);
		}
		else
		{
			target.material.SetTexture("_DiffCubeIBL", blackCube);
		}
		if ((bool)specularCube)
		{
			target.material.SetTexture("_SpecCubeIBL", specularCube);
		}
		else
		{
			target.material.SetTexture("_SpecCubeIBL", blackCube);
		}
		if ((bool)skyboxCube)
		{
			target.material.SetTexture("_SkyCubeIBL", skyboxCube);
		}
		else
		{
			target.material.SetTexture("_SkyCubeIBL", blackCube);
		}
		if (SH != null)
		{
			for (int i = 0; i < 9; i++)
			{
				target.material.SetVector("_SH" + i, SH.cBuffer[i]);
			}
			target.material.SetFloat("_SHScale", value);
		}
	}

	private void ApplySkybox()
	{
		Shader.DisableKeyword("MARMO_RGBM");
		Shader.EnableKeyword("MARMO_RGBA");
		if (showSkybox)
		{
			if (RenderSettings.skybox != skyboxMaterial)
			{
				RenderSettings.skybox = skyboxMaterial;
			}
		}
		else if ((bool)RenderSettings.skybox && RenderSettings.skybox.name == "Internal IBL Skybox")
		{
			RenderSettings.skybox = null;
		}
	}

	private void Reset()
	{
		skyMatrix = Matrix4x4.identity;
		exposures = Vector4.one;
		exposuresLM = Vector2.one;
		hdrScale = 1f;
		diffuseCube = (specularCube = (skyboxCube = null));
		masterIntensity = (skyIntensity = (specIntensity = (diffIntensity = 1f)));
		hdrSky = (hdrSpec = (hdrDiff = false));
	}

	private void OnEnable()
	{
		if (SH == null)
		{
			SH = new SHEncoding();
		}
		SH.copyToBuffer();
	}

	private void Start()
	{
		Apply();
	}

	private void Update()
	{
		if (activeSky == this && base.transform.hasChanged)
		{
			ApplySkyTransform();
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(_skyboxMaterial, allowDestroyingAssets: false);
		SH = null;
		_skyboxMaterial = null;
		_blackCube = null;
		diffuseCube = null;
		specularCube = null;
		skyboxCube = null;
	}

	private void OnDrawGizmos()
	{
		if (activeSky == null)
		{
			Apply();
		}
		Gizmos.DrawIcon(base.transform.position, "cubelight.tga", allowScaling: true);
		if (hasDimensions)
		{
			Color color = new Color(0.4f, 0.7f, 1f, 0.333f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Apply();
		if (hasDimensions)
		{
			Color color = new Color(0.4f, 0.7f, 1f, 1f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
