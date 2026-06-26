using UnityEngine;

public class LoopingAudio : MonoBehaviour
{
	public string audioString = string.Empty;

	public bool is3D;

	public bool playInPreGame;

	private bool isPlaying;

	public void Play(bool pregame)
	{
		if ((!pregame || playInPreGame) && !isPlaying)
		{
			isPlaying = true;
			if (is3D)
			{
				AudioController.Play(audioString, base.transform);
			}
			else
			{
				AudioController.Play(audioString);
			}
		}
	}
}
