using UnityEngine;

public class CheckIfCutsceneMovieIsDone : MonoBehaviour
{
	private PlayMovie moviePlayer;

	private State_Cutscene state;

	private float delay = 0.25f;

	private void Start()
	{
		moviePlayer = GetComponent<PlayMovie>();
		state = Object.FindObjectOfType<State_Cutscene>();
	}

	private void Update()
	{
		if ((bool)state && moviePlayer.IsDone())
		{
			delay -= Time.deltaTime;
			if (delay < 0f)
			{
				state.EndOfCutscene();
			}
		}
	}
}
