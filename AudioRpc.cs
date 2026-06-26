using Photon;
using UnityEngine;

public class AudioRpc : Photon.MonoBehaviour
{
	public AudioClip marco;

	public AudioClip polo;

	[RPC]
	private void Marco()
	{
		if (base.enabled)
		{
			Debug.Log("Marco");
			base.audio.clip = marco;
			base.audio.Play();
		}
	}

	[RPC]
	private void Polo()
	{
		if (base.enabled)
		{
			Debug.Log("Polo");
			base.audio.clip = polo;
			base.audio.Play();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		base.enabled = focus;
	}
}
