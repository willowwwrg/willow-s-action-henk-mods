using System;
using Flowmap;
using UnityEngine;

[ExecuteInEditMode]
public class FlowSimulationField : MonoBehaviour
{
	public static bool DrawFalloffTextures = true;

	public static bool DrawFalloffUnselected;

	public float strength = 1f;

	public Texture2D falloffTexture;

	protected Transform cachedTransform;

	protected Vector3 cachedPosition;

	protected Quaternion cachedRotation;

	protected Vector3 cachedScale;

	protected Vector2 falloffTextureDimensions;

	protected Color[] falloffTexturePixels;

	private bool initialized;

	protected bool wantsToDrawPreviewTexture;

	protected bool hasFalloffTexture;

	private Material falloffMaterial;

	[SerializeField]
	[HideInInspector]
	protected GpuRenderPlane renderPlane;

	public virtual FieldPass Pass => FieldPass.Force;

	protected virtual Shader RenderShader => null;

	public Material FalloffMaterial
	{
		get
		{
			if (!falloffMaterial)
			{
				falloffMaterial = new Material(RenderShader);
				falloffMaterial.hideFlags = HideFlags.HideAndDontSave;
				falloffMaterial.name = "FlowFieldFalloff";
			}
			if (falloffMaterial.shader != RenderShader)
			{
				falloffMaterial.shader = RenderShader;
			}
			return falloffMaterial;
		}
	}

	public GpuRenderPlane RenderPlane => renderPlane;

	protected void CreateMesh()
	{
		if ((bool)renderPlane && (bool)renderPlane.gameObject)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderPlane.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(renderPlane.gameObject);
			}
		}
		if (!(this == null))
		{
			if (renderPlane == null)
			{
				GameObject gameObject = new GameObject(base.name + " render plane");
				gameObject.hideFlags = HideFlags.HideInHierarchy;
				gameObject.layer = FlowmapGenerator.GpuRenderLayer;
				renderPlane = gameObject.AddComponent<GpuRenderPlane>();
				renderPlane.field = this;
			}
			MeshFilter meshFilter = renderPlane.GetComponent<MeshFilter>();
			if (!meshFilter)
			{
				meshFilter = renderPlane.gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.sharedMesh = Primitives.PlaneMesh;
			MeshRenderer meshRenderer = renderPlane.GetComponent<MeshRenderer>();
			if (!meshRenderer)
			{
				meshRenderer = renderPlane.gameObject.AddComponent<MeshRenderer>();
			}
			meshRenderer.material = FalloffMaterial;
			meshRenderer.enabled = false;
		}
	}

	private void Awake()
	{
		Init();
	}

	protected virtual void Update()
	{
		if (!initialized)
		{
			Init();
		}
		if (Application.isPlaying)
		{
			UpdateRenderPlane();
		}
	}

	public void DisableRenderPlane()
	{
		if ((bool)renderPlane)
		{
			renderPlane.renderer.enabled = false;
		}
	}

	public void DrawFalloffTextureEnabled(bool state)
	{
		wantsToDrawPreviewTexture = state;
	}

	public virtual void UpdateRenderPlane()
	{
		if (renderPlane == null || renderPlane.field != this)
		{
			CreateMesh();
		}
		renderPlane.transform.position = base.transform.position;
		renderPlane.transform.localScale = base.transform.lossyScale;
		renderPlane.transform.rotation = base.transform.rotation;
		FalloffMaterial.SetTexture("_MainTex", falloffTexture);
		FalloffMaterial.SetFloat("_Strength", strength);
		renderPlane.renderer.enabled = DrawFalloffTextures && (wantsToDrawPreviewTexture || DrawFalloffUnselected) && base.enabled;
	}

	public virtual void Init()
	{
		if (!initialized)
		{
			cachedTransform = base.transform;
			CreateMesh();
			renderPlane.renderer.enabled = wantsToDrawPreviewTexture;
			cachedTransform = base.transform;
			cachedPosition = cachedTransform.position;
			cachedRotation = cachedTransform.rotation;
			cachedScale = cachedTransform.lossyScale;
			hasFalloffTexture = falloffTexture != null;
			if ((bool)falloffTexture)
			{
				falloffTextureDimensions = new Vector2(falloffTexture.width, falloffTexture.height);
				falloffTexturePixels = falloffTexture.GetPixels();
			}
			else
			{
				falloffTextureDimensions = Vector2.zero;
			}
			initialized = true;
		}
	}

	public virtual void TickStart()
	{
		if (!base.enabled)
		{
			return;
		}
		switch (FlowmapGenerator.SimulationPath)
		{
		case SimulationPath.GPU:
			UpdateRenderPlane();
			FalloffMaterial.SetFloat("_Renderable", 1f);
			renderPlane.renderer.enabled = true;
			break;
		case SimulationPath.CPU:
			cachedTransform = base.transform;
			cachedPosition = cachedTransform.position;
			cachedRotation = cachedTransform.rotation;
			cachedScale = cachedTransform.lossyScale;
			hasFalloffTexture = falloffTexture != null;
			if ((bool)falloffTexture)
			{
				falloffTextureDimensions = new Vector2(falloffTexture.width, falloffTexture.height);
				falloffTexturePixels = falloffTexture.GetPixels();
			}
			else
			{
				falloffTextureDimensions = Vector2.zero;
			}
			break;
		}
	}

	public virtual void TickEnd()
	{
		if (FlowmapGenerator.SimulationPath == SimulationPath.GPU)
		{
			UpdateRenderPlane();
			FalloffMaterial.SetFloat("_Renderable", 0f);
		}
	}

	public Vector2 GetUvScale(FlowmapGenerator generator)
	{
		return new Vector2(cachedScale.x / generator.Dimensions.x, cachedScale.z / generator.Dimensions.y);
	}

	public Vector2 GetUvTransform(FlowmapGenerator generator)
	{
		return new Vector2((generator.Position.x - cachedPosition.x) / generator.Dimensions.x, (generator.Position.z - cachedPosition.z) / generator.Dimensions.y);
	}

	public float GetUvRotation(FlowmapGenerator generator)
	{
		return cachedRotation.eulerAngles.y * ((float)Math.PI / 180f);
	}

	public float GetStrengthCpu(FlowmapGenerator generator, Vector2 uv)
	{
		Vector2 vector = TransformSampleUv(generator, uv, invertY: false);
		float num = strength;
		if (vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
		{
			num = 0f;
		}
		if (FlowmapGenerator.ThreadCount > 1)
		{
			return num * ((!hasFalloffTexture) ? 1f : TextureUtilities.SampleColorBilinear(falloffTexturePixels, (int)falloffTextureDimensions.x, (int)falloffTextureDimensions.y, vector.x, vector.y).r);
		}
		return num * ((!hasFalloffTexture) ? 1f : falloffTexture.GetPixelBilinear(vector.x, vector.y).r);
	}

	protected Vector2 TransformSampleUv(FlowmapGenerator generator, Vector2 uv, bool invertY)
	{
		Vector2 vector = uv;
		vector = new Vector2(vector.x + GetUvTransform(generator).x, vector.y + GetUvTransform(generator).y);
		vector -= Vector2.one * 0.5f;
		vector = new Vector2(vector.x * Mathf.Cos(GetUvRotation(generator)) - vector.y * Mathf.Sin(GetUvRotation(generator)), vector.x * Mathf.Sin(GetUvRotation(generator)) + vector.y * Mathf.Cos(GetUvRotation(generator)));
		vector = new Vector2(vector.x / GetUvScale(generator).x * (float)((!invertY) ? 1 : (-1)), vector.y / GetUvScale(generator).y * (float)((!invertY) ? 1 : (-1)));
		return vector + Vector2.one * 0.5f;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Vector3 vector = cachedTransform.position + cachedTransform.right * ((0f - cachedTransform.lossyScale.x) / 2f) + cachedTransform.forward * ((0f - cachedTransform.lossyScale.z) / 2f);
		Vector3 vector2 = cachedTransform.position + cachedTransform.right * (cachedTransform.lossyScale.x / 2f) + cachedTransform.forward * ((0f - cachedTransform.lossyScale.z) / 2f);
		Vector3 vector3 = cachedTransform.position + cachedTransform.right * ((0f - cachedTransform.lossyScale.x) / 2f) + cachedTransform.forward * (cachedTransform.lossyScale.z / 2f);
		Vector3 to = cachedTransform.position + cachedTransform.right * (cachedTransform.lossyScale.x / 2f) + cachedTransform.forward * (cachedTransform.lossyScale.z / 2f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector3, to);
		Gizmos.DrawLine(vector, vector3);
		Gizmos.DrawLine(vector2, to);
		wantsToDrawPreviewTexture = true;
		UpdateRenderPlane();
	}

	protected virtual void OnDrawGizmos()
	{
		wantsToDrawPreviewTexture = false;
		UpdateRenderPlane();
	}

	private void OnDisable()
	{
		wantsToDrawPreviewTexture = false;
		if ((bool)renderPlane)
		{
			renderPlane.renderer.enabled = DrawFalloffTextures && wantsToDrawPreviewTexture;
		}
	}

	private void OnDestroy()
	{
		Cleaup();
	}

	protected virtual void Cleaup()
	{
		if ((bool)renderPlane && (bool)renderPlane.gameObject)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderPlane.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(renderPlane.gameObject);
			}
		}
		if ((bool)falloffMaterial)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(falloffMaterial);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(falloffMaterial);
			}
		}
	}
}
