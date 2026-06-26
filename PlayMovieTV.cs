using UnityEngine;

public class PlayMovieTV : MonoBehaviour
{
	public Material sourceVideo;

	private MovieTexture tex;

	public int mat;

	public int onlyActiveInLevel = -1;

	public bool activeInMainMenu = true;

	public void Start()
	{
		if (onlyActiveInLevel == -1 || Singleton<LevelBatchManager>.SP.GetCurrentLevel() == onlyActiveInLevel || (!HenkUtils.IsInALevel() && activeInMainMenu))
		{
			base.renderer.materials[mat].mainTexture = sourceVideo.mainTexture;
			base.renderer.materials[mat].color = Color.white;
			tex = (MovieTexture)base.renderer.materials[mat].mainTexture;
			tex.loop = true;
			tex.Play();
			if ((bool)base.audio)
			{
				base.audio.Play();
			}
		}
	}

	private void Update()
	{
	}
}
