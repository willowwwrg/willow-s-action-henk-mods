using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAtDay : MonoBehaviour
{
	public TOD_Sky sky;

	public float fadeTime = 1f;

	private float lerpTime;

	private AudioSource audioComponent;

	private float audioVolume;

	protected void OnEnable()
	{
		if (!sky)
		{
			Debug.LogError("Sky instance reference not set. Disabling script.");
			base.enabled = false;
		}
		audioComponent = base.audio;
		audioVolume = audioComponent.volume;
	}

	protected void Update()
	{
		int num = (sky.IsDay ? 1 : (-1));
		lerpTime = Mathf.Clamp01(lerpTime + (float)num * Time.deltaTime / fadeTime);
		audioComponent.volume = Mathf.Lerp(0f, audioVolume, lerpTime);
	}
}
