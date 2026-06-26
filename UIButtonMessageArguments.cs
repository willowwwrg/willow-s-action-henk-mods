using System.Collections.Generic;
using UnityEngine;

public class UIButtonMessageArguments : MonoBehaviour
{
	public List<float> floats = new List<float>();

	public List<int> ints = new List<int>();

	public List<string> strings = new List<string>();

	public List<GameObject> gameobjects = new List<GameObject>();

	public CharacterSelect.Characters character = CharacterSelect.Characters.Henk;

	public List<float> GetFloats()
	{
		return floats;
	}

	public List<int> GetInts()
	{
		return ints;
	}

	public List<string> GetStrings()
	{
		return strings;
	}

	public List<GameObject> GetGameobjects()
	{
		return gameobjects;
	}

	public CharacterSelect.Characters GetCharacter()
	{
		return character;
	}
}
