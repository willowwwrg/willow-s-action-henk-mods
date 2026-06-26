using System;
using UnityEngine;

public class CorridorLamp : MonoBehaviour
{
	public GameObject NormalMesh;

	public Mesh BrokenMesh;

	public GameObject FX;

	private bool Crushed;

	public string[] excludedTags = new string[3] { "Untagged", "Player", "GameController" };

	private void Start()
	{
		if ((bool)GetComponent<AudioSource>())
		{
			base.audio.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
		}
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision other)
	{
		if (Array.IndexOf(excludedTags, other.gameObject.tag) != -1 || Crushed)
		{
			return;
		}
		Crushed = true;
		base.animation.Play();
		if ((bool)GetComponent<AudioSource>())
		{
			base.audio.Play();
		}
		FX.particleSystem.enableEmission = true;
		FX.particleSystem.Play();
		if ((bool)NormalMesh.GetComponent<SkinnedMeshRenderer>())
		{
			NormalMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = BrokenMesh;
			return;
		}
		NormalMesh.GetComponent<MeshFilter>().sharedMesh = BrokenMesh;
		FX.particleSystem.enableEmission = true;
		FX.particleSystem.Play();
		base.animation.Play();
		if ((bool)GetComponent<AudioSource>())
		{
			base.audio.Play();
		}
	}
}
