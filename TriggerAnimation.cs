using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
	public List<Animation> anims = new List<Animation>();

	public void OnReset()
	{
		foreach (Animation anim in anims)
		{
			anim.Stop();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!Singleton<PlayerManager>.SP.IsLocalPlayer(other.transform.root.gameObject))
		{
			return;
		}
		foreach (Animation anim in anims)
		{
			anim.Play();
		}
	}
}
