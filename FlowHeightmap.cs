using Flowmap;
using UnityEngine;

[RequireComponent(typeof(FlowmapGenerator))]
[ExecuteInEditMode]
public class FlowHeightmap : MonoBehaviour
{
	public bool previewHeightmap;

	public bool drawPreviewPlane;

	private bool wantsToDrawHeightmap;

	[SerializeField]
	private GameObject previewGameObject;

	[SerializeField]
	private Material previewMaterial;

	private FlowmapGenerator generator;

	public virtual Texture HeightmapTexture { get; set; }

	public virtual Texture PreviewHeightmapTexture { get; set; }

	protected FlowmapGenerator Generator
	{
		get
		{
			if (!generator)
			{
				generator = GetComponent<FlowmapGenerator>();
			}
			return generator;
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		DisplayPreviewHeightmap(state: true);
		UpdatePreviewHeightmap();
	}

	public void DisplayPreviewHeightmap(bool state)
	{
		wantsToDrawHeightmap = state;
		UpdatePreviewHeightmap();
	}

	public void UpdatePreviewHeightmap()
	{
		if (previewGameObject == null || previewMaterial == null)
		{
			Cleanup();
			previewGameObject = new GameObject("Preview Heightmap");
			previewGameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
			previewGameObject.AddComponent<MeshFilter>().sharedMesh = Primitives.PlaneMesh;
			MeshRenderer meshRenderer = previewGameObject.AddComponent<MeshRenderer>();
			previewMaterial = new Material(Shader.Find("Hidden/HeightmapFieldPreview"));
			previewMaterial.hideFlags = HideFlags.HideAndDontSave;
			meshRenderer.sharedMaterial = previewMaterial;
		}
		if (previewHeightmap && wantsToDrawHeightmap)
		{
			previewMaterial.SetTexture("_MainTex", PreviewHeightmapTexture);
			previewMaterial.SetFloat("_Strength", 1f);
			previewGameObject.renderer.enabled = true;
			previewGameObject.transform.position = base.transform.position;
			previewGameObject.transform.localScale = new Vector3(Generator.Dimensions.x, 1f, Generator.Dimensions.y);
		}
		else
		{
			previewGameObject.renderer.enabled = false;
		}
	}

	protected virtual void OnDrawGizmos()
	{
		DisplayPreviewHeightmap(state: false);
		UpdatePreviewHeightmap();
	}

	protected virtual void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if ((bool)previewGameObject)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(previewGameObject);
			}
			else
			{
				Object.DestroyImmediate(previewGameObject);
			}
		}
		if ((bool)previewMaterial)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(previewMaterial);
			}
			else
			{
				Object.DestroyImmediate(previewMaterial);
			}
		}
	}
}
