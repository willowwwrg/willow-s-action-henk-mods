using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
	public Material[] materials;

	public float waitTime;

	public float fadeTime = 4f;

	public bool replaceShaders = true;

	private static Dictionary<Shader, Shader> replacementShaders = new Dictionary<Shader, Shader>();

	public static Shader GetReplacementFor(Shader original)
	{
		if (replacementShaders.TryGetValue(original, out var value))
		{
			return value;
		}
		string text = original.name;
		if (text.StartsWith("Mobile/"))
		{
			text = text.Substring("Mobile/".Length);
		}
		if (!text.StartsWith("Transparent/"))
		{
			value = Shader.Find("Transparent/" + text);
		}
		replacementShaders[original] = value;
		return value;
	}

	private IEnumerator Start()
	{
		Material[] m = materials;
		if (m == null || m.Length == 0)
		{
			Fade fade = this;
			Material[] array;
			m = (array = base.renderer.materials);
			fade.materials = array;
		}
		if (waitTime > 0f)
		{
			yield return new WaitForSeconds(waitTime);
		}
		if (replaceShaders)
		{
			Material[] array2 = m;
			foreach (Material material in array2)
			{
				Shader replacementFor = GetReplacementFor(material.shader);
				if (replacementFor != null)
				{
					material.shader = replacementFor;
				}
			}
		}
		Material[] array3 = m;
		for (int j = 0; j < array3.Length; j++)
		{
			if (!array3[j].HasProperty("_Color"))
			{
				Debug.LogError("Material does not have a color property '_Color' so it cannot be faded.");
				yield break;
			}
		}
		for (float t = 0f; t < fadeTime; t += Time.deltaTime)
		{
			Material[] array2 = m;
			foreach (Material obj in array2)
			{
				Color color = obj.color;
				color.a = 1f - t / fadeTime;
				obj.color = color;
			}
			yield return null;
		}
		SendMessage("FadeCompleted", SendMessageOptions.DontRequireReceiver);
	}
}
