using UnityEngine;

public class PlayMovie : MonoBehaviour
{
	private MovieTexture tex;

	public int mat;

	public bool playOnStart;

	private bool started;

	public void Start()
	{
		if (playOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		tex = (MovieTexture)base.renderer.materials[mat].mainTexture;
		tex.loop = false;
		tex.Stop();
		tex.Play();
		started = true;
		if ((bool)base.audio)
		{
			if ((bool)tex.audioClip)
			{
				base.audio.clip = tex.audioClip;
			}
			float volume = (float)PlayerPrefs.GetInt("musicVolume_i", 10) / 10f;
			base.audio.volume = volume;
			base.audio.Play();
		}
	}

	public void Stop()
	{
		if ((bool)tex && tex.isPlaying)
		{
			tex.Stop();
			base.renderer.enabled = false;
		}
	}

	public bool IsDone()
	{
		if (!tex.isPlaying)
		{
			return started;
		}
		return false;
	}

	private void Update()
	{
	}
}
