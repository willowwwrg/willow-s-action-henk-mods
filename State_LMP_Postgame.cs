using UnityEngine;

public class State_LMP_Postgame : GameState
{
	public GUI_LMP_Postgame guiScript;

	private bool timing;

	public float countdown;

	public override void OnActivate()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP_Postgame, "none");
		timing = false;
		countdown = 0f;
		guiScript.stateObj = this;
		if (Singleton<LocalMultiManager>.SP.GetAttemptsLeft() <= 1)
		{
			Singleton<AudioManager>.SP.PlayPostgame();
		}
		if (!AudioController.IsPlaying("Finish") && !AudioController.IsPlaying("Finish_improved"))
		{
			AudioController.Play("Finish");
		}
		if (Singleton<LocalMultiManager>.SP.IsGameOver())
		{
			CountdownTimer(3.9f);
		}
		else
		{
			CountdownTimer(10f);
		}
	}

	public override void OnDeactivate()
	{
	}

	public override void OnUpdate()
	{
		if (!timing)
		{
			return;
		}
		countdown -= Time.deltaTime;
		if (countdown <= 0f)
		{
			if (Singleton<LocalMultiManager>.SP.IsGameOver())
			{
				GoToEndgame();
			}
			else
			{
				Singleton<LocalMultiManager>.SP.RetryOrNextLevel();
			}
			timing = false;
		}
		if (countdown < 3.1f && countdown > 2.9f && !AudioController.IsPlaying("mul_begin"))
		{
			AudioController.Play("mul_begin");
		}
	}

	private void CountdownTimer(float time)
	{
		if (!timing)
		{
			timing = true;
			countdown = time;
		}
	}

	public void GoToEndgame()
	{
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_LMP_Endgame, "none");
	}
}
