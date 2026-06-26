using System.Collections.Generic;
using UnityEngine;

public class AdditionalSkinAudio : MonoBehaviour
{
	public List<AdditionalAudioStruct> additionalAudio;

	public void Play(SfxEvents audioEvent)
	{
		if (additionalAudio.Count <= 0)
		{
			return;
		}
		foreach (AdditionalAudioStruct item in additionalAudio)
		{
			if (item.trigger == audioEvent && !AudioController.IsPlaying(item.audioString))
			{
				AudioController.Play(item.audioString);
			}
		}
	}
}
