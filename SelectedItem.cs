using System.Collections.Generic;
using UnityEngine;

public class SelectedItem : MonoBehaviour
{
	private List<Material> allMaterials = new List<Material>();

	public EditorObjBaseValues[] allBaseValues;

	private Transform editorCollider;

	private Mesh originalMesh;

	private Mesh roundedMesh;

	private List<GameObject> arrows = new List<GameObject>();

	public bool newlyCreated;

	public bool pickedUp;

	public Transform originalParent;

	public Vector3 pickedupAtOffset = new Vector3(0f, 0f, 0f);

	public Vector3 offsetFromCursor = new Vector3(0f, 0f, 0f);

	public void Awake()
	{
		InitChildren();
		InitMaterials();
	}

	public void OnDestroy()
	{
		DeselectItem();
	}

	private void InitChildren()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.name == "LevelEditorCollider")
			{
				editorCollider = child;
				editorCollider.gameObject.SetActive(value: true);
			}
			if (child.name == "ArrowAnchor")
			{
				if (child.gameObject.activeSelf)
				{
					Object.Destroy(child.GetChild(0).gameObject);
				}
				GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("LevelEditor/Arrow"));
				gameObject.transform.parent = child;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				arrows.Add(gameObject);
				child.gameObject.SetActive(value: true);
			}
		}
		if (editorCollider == null && (bool)GetComponent<MeshCollider>())
		{
			ComputeCollider();
			GameObject gameObject2 = new GameObject("LevelEditorCollider");
			gameObject2.transform.parent = base.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.localRotation = Quaternion.identity;
			editorCollider = gameObject2.transform;
			MeshCollider meshCollider = gameObject2.AddComponent<MeshCollider>();
			meshCollider.sharedMesh = roundedMesh;
			meshCollider.convex = true;
			meshCollider.isTrigger = true;
		}
	}

	private void ComputeCollider()
	{
		if (base.collider.GetType() == typeof(MeshCollider))
		{
			originalMesh = GetComponent<MeshCollider>().sharedMesh;
			roundedMesh = SplineDeform.DuplicateMesh(originalMesh);
			Vector3[] vertices = roundedMesh.vertices;
			float num = 10000f;
			float num2 = -10000f;
			float num3 = 10000f;
			float num4 = -10000f;
			for (int i = 0; i < vertices.Length; i++)
			{
				num = Mathf.Min(num, vertices[i].x);
				num2 = Mathf.Max(num2, vertices[i].x);
				num3 = Mathf.Min(num3, vertices[i].y);
				num4 = Mathf.Max(num4, vertices[i].y);
			}
			if (base.name.StartsWith("plastic_ramp") || base.name.StartsWith("curve"))
			{
				num = Mathf.Round(num) + 0.01f;
				num2 = Mathf.Round(num2) - 0.01f;
				num3 = Mathf.Round(num3) + 0.01f;
				num4 = Mathf.Round(num4) - 0.01f;
			}
			else
			{
				num += 0.01f;
				num2 -= 0.01f;
				num3 += 0.01f;
				num4 -= 0.01f;
			}
			for (int j = 0; j < vertices.Length; j++)
			{
				vertices[j].x = Mathf.Clamp(vertices[j].x, num, num2);
				vertices[j].y = Mathf.Clamp(vertices[j].y, num3, num4);
			}
			roundedMesh.vertices = vertices;
		}
	}

	public void SwitchToInGame()
	{
		DeselectItem();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.transform == editorCollider)
			{
				collider.enabled = false;
			}
			else
			{
				collider.enabled = true;
			}
		}
		foreach (GameObject arrow in arrows)
		{
			arrow.SetActive(value: false);
		}
		SetShaders(transparent: false);
	}

	public void SwitchToEditMode()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.transform == editorCollider)
			{
				collider.enabled = true;
			}
			else
			{
				collider.enabled = false;
			}
		}
		foreach (GameObject arrow in arrows)
		{
			arrow.SetActive(value: true);
		}
		SetShaders(transparent: true);
	}

	private void InitMaterials()
	{
		allMaterials.Clear();
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material item in materials)
			{
				allMaterials.Add(item);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren2 = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			Material[] materials = componentsInChildren2[k].materials;
			foreach (Material item2 in materials)
			{
				allMaterials.Add(item2);
			}
		}
		allBaseValues = new EditorObjBaseValues[allMaterials.Count];
		for (int l = 0; l < allMaterials.Count; l++)
		{
			Material material = allMaterials[l];
			allBaseValues[l] = new EditorObjBaseValues();
			EditorObjBaseValues editorObjBaseValues = allBaseValues[l];
			editorObjBaseValues.originalShader = material.shader.name;
			editorObjBaseValues.transparentShader = material.shader.name;
			if (editorObjBaseValues.originalShader.StartsWith("Marmoset"))
			{
				if (editorObjBaseValues.originalShader.Split('/').Length - 1 == 1)
				{
					string transparentShader = "Marmoset/Transparent/" + material.shader.name.Substring(9);
					editorObjBaseValues.transparentShader = transparentShader;
				}
				if (editorObjBaseValues.originalShader.StartsWith("Marmoset/Self-Illumin"))
				{
					editorObjBaseValues.transparentShader = "Marmoset/Transparent/Simple Glass/Bumped Specular Glow IBL";
				}
			}
			if (material.HasProperty("_Color"))
			{
				editorObjBaseValues.color = material.GetColor("_Color");
			}
			if (material.HasProperty("_SpecColor"))
			{
				editorObjBaseValues.specColor = material.GetColor("_SpecColor");
			}
			if (material.HasProperty("_SpecInt"))
			{
				editorObjBaseValues.specInt = material.GetFloat("_SpecInt");
			}
			if (material.HasProperty("_GlowStrength"))
			{
				editorObjBaseValues.glowStrength = material.GetFloat("_GlowStrength");
			}
		}
	}

	public void MakeGreen()
	{
		SetShaders(transparent: true);
		SetItemColor(new Color(0.4f, 1f, 0.4f, 0.9f));
	}

	public void MakeRed()
	{
		SetShaders(transparent: true);
		SetItemColor(new Color(1f, 0.4f, 0.4f, 0.9f));
	}

	public void MakeHovered()
	{
		SetShaders(transparent: false);
		SetItemColor(new Color(1f, 1f, 0.1f, 1f));
		AudioController.Play("lvledit_rollover");
	}

	public void SetItemColor(Color color)
	{
		for (int i = 0; i < allMaterials.Count; i++)
		{
			Material material = allMaterials[i];
			EditorObjBaseValues editorObjBaseValues = allBaseValues[i];
			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", color);
			}
			if (material.HasProperty("_SpecColor"))
			{
				material.SetColor("_SpecColor", color);
			}
			if (material.HasProperty("_SpecInt"))
			{
				material.SetFloat("_SpecInt", editorObjBaseValues.specInt * color.a);
			}
			if (material.HasProperty("_GlowStrength"))
			{
				material.SetFloat("_GlowStrength", editorObjBaseValues.glowStrength * color.a);
			}
		}
	}

	public void SetShaders(bool transparent)
	{
		for (int i = 0; i < allMaterials.Count; i++)
		{
			Material material = allMaterials[i];
			EditorObjBaseValues editorObjBaseValues = allBaseValues[i];
			if (transparent)
			{
				material.shader = Shader.Find(editorObjBaseValues.transparentShader);
			}
			else
			{
				material.shader = Shader.Find(editorObjBaseValues.originalShader);
			}
		}
	}

	public void RevertMaterialChanges()
	{
		for (int i = 0; i < allMaterials.Count; i++)
		{
			Material material = allMaterials[i];
			EditorObjBaseValues editorObjBaseValues = allBaseValues[i];
			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", editorObjBaseValues.color);
			}
			if (material.HasProperty("_SpecColor"))
			{
				material.SetColor("_SpecColor", editorObjBaseValues.specColor);
			}
			if (material.HasProperty("_SpecInt"))
			{
				material.SetFloat("_SpecInt", editorObjBaseValues.specInt);
			}
			if (material.HasProperty("_GlowStrength"))
			{
				material.SetFloat("_GlowStrength", editorObjBaseValues.glowStrength);
			}
		}
		SetShaders(transparent: false);
	}

	public void DeselectItem()
	{
		RevertMaterialChanges();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}
}
