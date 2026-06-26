using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB2_MeshBakerRoot : MonoBehaviour
{
	[HideInInspector]
	public MB2_TextureBakeResults textureBakeResults;

	public virtual List<GameObject> GetObjectsToCombine()
	{
		return null;
	}

	public static bool doCombinedValidate(MB2_MeshBakerRoot mom, MB_ObjsToCombineTypes objToCombineType, MB2_EditorMethodsInterface editorMethods)
	{
		if (mom.textureBakeResults == null)
		{
			Debug.LogError("Need to set Material Bake Result on " + mom);
			return false;
		}
		if (!(mom is MB2_TextureBaker))
		{
			MB2_TextureBaker component = mom.GetComponent<MB2_TextureBaker>();
			if (component != null && component.textureBakeResults != mom.textureBakeResults)
			{
				Debug.LogWarning("Material Bake Result on this component is not the same as the Material Bake Result on the MB2_TextureBaker.");
			}
		}
		List<GameObject> objectsToCombine = mom.GetObjectsToCombine();
		for (int i = 0; i < objectsToCombine.Count; i++)
		{
			GameObject gameObject = objectsToCombine[i];
			if (gameObject == null)
			{
				Debug.LogError("The list of objects to combine contains a null at position." + i + " Select and use [shift] delete to remove");
				return false;
			}
			for (int j = i + 1; j < objectsToCombine.Count; j++)
			{
				if (objectsToCombine[i] == objectsToCombine[j])
				{
					Debug.LogError("The list of objects to combine contains duplicates.");
					return false;
				}
			}
			if (MB_Utility.GetGOMaterials(gameObject) == null)
			{
				Debug.LogError(string.Concat("Object ", gameObject, " in the list of objects to be combined does not have a material"));
				return false;
			}
			if (MB_Utility.GetMesh(gameObject) == null)
			{
				Debug.LogError(string.Concat("Object ", gameObject, " in the list of objects to be combined does not have a mesh"));
				return false;
			}
		}
		if (mom.textureBakeResults.doMultiMaterial && !validateSubmeshOverlap(mom))
		{
			return false;
		}
		List<GameObject> list = objectsToCombine;
		if (mom is MB2_MeshBaker)
		{
			MB2_TextureBaker component2 = mom.GetComponent<MB2_TextureBaker>();
			if (((MB2_MeshBaker)mom).useObjsToMeshFromTexBaker && component2 != null)
			{
				list = component2.objsToMesh;
			}
			if (list == null || list.Count == 0)
			{
				Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
				return false;
			}
			if (mom is MB2_MeshBaker && ((MB2_MeshBaker)mom).renderType == MB_RenderType.skinnedMeshRenderer && !editorMethods.ValidateSkinnedMeshes(list))
			{
				return false;
			}
		}
		editorMethods?.CheckPrefabTypes(objToCombineType, objectsToCombine);
		return true;
	}

	private static bool validateSubmeshOverlap(MB2_MeshBakerRoot mom)
	{
		List<GameObject> objectsToCombine = mom.GetObjectsToCombine();
		for (int i = 0; i < objectsToCombine.Count; i++)
		{
			if (MB_Utility.doSubmeshesShareVertsOrTris(MB_Utility.GetMesh(objectsToCombine[i])) != 0)
			{
				Debug.LogWarning(string.Concat("Object ", objectsToCombine[i], " in the list of objects to combine has overlapping submeshes (submeshes share vertices). If the UVs associated with the shared vertices are important then this bake may not work. If you are using multiple materials then this object can only be combined with objects that use the exact same set of textures (each atlas contains one texture). There may be other undesirable side affects as well. Mesh Master, available in the asset store can fix overlapping submeshes."));
				return true;
			}
		}
		return true;
	}
}
