using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectableCharacter : MonoBehaviour
{
	[Serializable]
	public class Skin
	{
		public List<Material> materials;

		public List<GameObject> objects;
	}

	public int materialCount;

	public List<Skin> skins = new List<Skin>();

	public int currentSkinNumber;

	public CharacterSelect.Characters character = CharacterSelect.Characters.Henk;

	public void SetSkin(int skinNum)
	{
		if (skinNum > skins.Count - 1)
		{
			Debug.LogError("Trying to set nonexistant skin. Defaulting to base skin.");
			skinNum = 0;
		}
		if (currentSkinNumber == skinNum)
		{
			return;
		}
		foreach (Skin skin in skins)
		{
			foreach (GameObject @object in skin.objects)
			{
				@object.SetActive(value: false);
			}
		}
		foreach (GameObject object2 in skins[skinNum].objects)
		{
			object2.SetActive(value: true);
		}
		SwapMaterials(skins[skinNum]);
		currentSkinNumber = skinNum;
	}

	private void SwapMaterials(Skin skin)
	{
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		int num = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] array = new Material[componentsInChildren[i].materials.Length];
			for (int j = 0; j < componentsInChildren[i].materials.Length; j++)
			{
				array[j] = skin.materials[num];
				num++;
			}
			componentsInChildren[i].materials = array;
		}
	}
}
