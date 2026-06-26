using System;
using UnityEngine;

public class DeskLamp : MonoBehaviour
{
	public GameObject brokenObj;

	public GameObject[] hidingObjs;

	public string[] excludedTags = new string[3] { "Untagged", "Player", "GameController" };

	private void Start()
	{
		brokenObj.SetActive(value: false);
		if ((bool)GetComponent<AudioSource>())
		{
			brokenObj.audio.pitch = UnityEngine.Random.Range(0.7f, 1.1f);
		}
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision other)
	{
		if (Array.IndexOf(excludedTags, other.gameObject.tag) == -1)
		{
			GameObject[] array = hidingObjs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			base.renderer.enabled = false;
			base.collider.enabled = false;
			brokenObj.SetActive(value: true);
			brokenObj.animation.Play();
		}
	}
}
