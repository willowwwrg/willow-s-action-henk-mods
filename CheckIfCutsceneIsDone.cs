using System.Collections;
using UnityEngine;
using WellFired;

public class CheckIfCutsceneIsDone : MonoBehaviour
{
	private USSequencer sequence;

	private State_Cutscene state;

	private bool fading;

	private void Start()
	{
		sequence = Object.FindObjectOfType<USSequencer>();
		state = Object.FindObjectOfType<State_Cutscene>();
	}

	private void Update()
	{
		if (state != null && sequence != null && sequence.IsComplete && !fading)
		{
			fading = true;
			StartCoroutine(FadeAndExit());
		}
	}

	private IEnumerator FadeAndExit()
	{
		Singleton<PermaGUI>.SP.FadeInOrOut(1.5f, fadeIn: false);
		yield return new WaitForSeconds(1.6f);
		if ((bool)state)
		{
			state.EndOfCutscene();
		}
	}
}
