using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Force")]
public class FlowForceField : FlowSimulationField
{
	public FluidForce force;

	[SerializeField]
	private Texture2D vectorTexture;

	private Vector2 vectorTextureDimensions;

	private Color[] vectorTexturePixels;

	private Vector3 cachedForwardVector;

	[HideInInspector]
	public Texture2D attractVectorPreview;

	[HideInInspector]
	public Texture2D repulseVectorPreview;

	[HideInInspector]
	public Texture2D vortexClockwiseVectorPreview;

	[HideInInspector]
	public Texture2D vortexCounterClockwiseVectorPreview;

	[HideInInspector]
	public Texture2D directionalVectorPreview;

	public override FieldPass Pass => FieldPass.Force;

	protected override Shader RenderShader => Shader.Find("Hidden/ForceFieldPreview");

	public override void Init()
	{
		base.Init();
		UpdateVectorTexture();
	}

	protected override void Update()
	{
		base.Update();
	}

	public void UpdateVectorTexture()
	{
		int num = 64;
		int num2 = 64;
		vectorTexture = new Texture2D(num, num2, TextureFormat.ARGB32, mipmap: false, linear: true);
		vectorTexture.hideFlags = HideFlags.HideAndDontSave;
		vectorTexture.name = "VectorTexture";
		Color[] array = new Color[num * num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float a = 1f - Mathf.Clamp01(Vector2.zero.magnitude);
				Color color = Color.white;
				switch (force)
				{
				case FluidForce.Attract:
				case FluidForce.Repulse:
				{
					Vector2 normalized2 = new Vector2(((float)j / (float)num - 0.5f) * 2f, ((float)i / (float)num2 - 0.5f) * 2f).normalized;
					normalized2 = new Vector2(normalized2.x * 0.5f + 0.5f, normalized2.y * 0.5f + 0.5f);
					color = new Color(normalized2.x, normalized2.y, 0f, a);
					break;
				}
				case FluidForce.VortexCounterClockwise:
				case FluidForce.VortexClockwise:
				{
					Vector2 normalized = new Vector2(((float)j / (float)num - 0.5f) * 2f, ((float)i / (float)num2 - 0.5f) * 2f).normalized;
					Vector3 vector = Vector3.Cross(new Vector3(normalized.x, 0f, normalized.y), Vector3.down);
					normalized = new Vector2(vector.x * 0.5f + 0.5f, vector.z * 0.5f + 0.5f);
					color = new Color(normalized.x, normalized.y, 0f, a);
					break;
				}
				case FluidForce.Directional:
				{
					Vector2 one = Vector2.one;
					color = new Color(one.x, one.y, 0f, a);
					break;
				}
				case FluidForce.Calm:
					color = new Color(0.5f, 0.5f, 1f, a);
					break;
				}
				array[j + i * num] = color;
			}
		}
		vectorTexture.SetPixels(array);
		vectorTexture.Apply(updateMipmaps: false);
		vectorTexturePixels = vectorTexture.GetPixels();
		vectorTextureDimensions = new Vector2(vectorTexture.width, vectorTexture.height);
	}

	public override void UpdateRenderPlane()
	{
		base.UpdateRenderPlane();
		if (FlowmapGenerator.SimulationPath == SimulationPath.GPU)
		{
			base.FalloffMaterial.SetTexture("_VectorTex", vectorTexture);
			switch (force)
			{
			case FluidForce.Repulse:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			case FluidForce.Attract:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 1f);
				break;
			case FluidForce.VortexClockwise:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			case FluidForce.VortexCounterClockwise:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 1f);
				break;
			case FluidForce.Directional:
			{
				Vector2 vector = new Vector2(base.transform.forward.x * 0.5f + 0.5f, base.transform.forward.z * 0.5f + 0.5f);
				base.FalloffMaterial.SetVector("_VectorScale", vector);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			}
			case FluidForce.Calm:
				base.FalloffMaterial.SetVector("_VectorScale", Vector2.one);
				base.FalloffMaterial.SetFloat("_VectorInvert", 0f);
				break;
			}
		}
		if ((wantsToDrawPreviewTexture || FlowSimulationField.DrawFalloffUnselected) && base.enabled)
		{
			switch (force)
			{
			case FluidForce.Attract:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", attractVectorPreview);
				break;
			case FluidForce.Repulse:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", repulseVectorPreview);
				break;
			case FluidForce.VortexClockwise:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", vortexClockwiseVectorPreview);
				break;
			case FluidForce.VortexCounterClockwise:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", vortexCounterClockwiseVectorPreview);
				break;
			case FluidForce.Directional:
				base.FalloffMaterial.SetTexture("_VectorPreviewTex", directionalVectorPreview);
				break;
			}
		}
		else
		{
			base.FalloffMaterial.SetTexture("_VectorPreviewTex", null);
		}
	}

	public override void TickStart()
	{
		base.TickStart();
		if (FlowmapGenerator.SimulationPath == SimulationPath.CPU)
		{
			if (vectorTexturePixels == null)
			{
				Init();
			}
			cachedForwardVector = base.transform.forward;
		}
	}

	public Vector3 GetForceCpu(FlowmapGenerator generator, Vector2 uv)
	{
		Vector2 vector = TransformSampleUv(generator, uv, force == FluidForce.Attract || force == FluidForce.VortexCounterClockwise);
		Color color = ((FlowmapGenerator.ThreadCount <= 1) ? vectorTexture.GetPixelBilinear(vector.x, vector.y) : TextureUtilities.SampleColorBilinear(vectorTexturePixels, (int)vectorTextureDimensions.x, (int)vectorTextureDimensions.y, vector.x, vector.y));
		if (force == FluidForce.Directional)
		{
			color = new Color(cachedForwardVector.x * 0.5f + 0.5f, cachedForwardVector.z * 0.5f + 0.5f, 0f, 0f);
		}
		Vector3 vector2 = new Vector3(color.r * 2f - 1f, color.g * 2f - 1f, color.b);
		return strength * vector2 * ((vector.x >= 0f && vector.x <= 1f && vector.y >= 0f && vector.y <= 1f) ? 1 : 0) * ((!hasFalloffTexture) ? 1f : ((FlowmapGenerator.ThreadCount <= 1) ? falloffTexture.GetPixelBilinear(vector.x, vector.y).r : TextureUtilities.SampleColorBilinear(falloffTexturePixels, (int)falloffTextureDimensions.x, (int)falloffTextureDimensions.y, vector.x, vector.y).r));
	}

	protected override void Cleaup()
	{
		base.Cleaup();
		if ((bool)vectorTexture)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(vectorTexture);
			}
			else
			{
				Object.DestroyImmediate(vectorTexture);
			}
		}
	}
}
