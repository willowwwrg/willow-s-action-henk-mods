using Henk;
using UnityEngine;

public class State_InGameMultiplayer : GameState
{
	public Collectable[] allCollectibles;

	private int prevCollectibles;

	public override void OnActivate()
	{
		Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlatformerController>().GiveControl();
		Singleton<Transitions>.SP.TransitionTo(GUIManager.GUIScreens.GUIScreen_InGameMultiplayer, "none");
		Singleton<Stopwatch>.SP.ResetTimer();
		Singleton<Stopwatch>.SP.StartTimer();
		Time.timeScale = 1f;
		UpdateCollectibles();
	}

	public override void OnDeactivate()
	{
		Singleton<Stopwatch>.SP.StopTimer();
	}

	public override void OnUpdate()
	{
		if (!Singleton<InGameMenu>.SP.menuEnabled)
		{
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Retry))
			{
				AudioController.Play("Reset");
				Singleton<GamestateManager>.SP.SetState(typeof(State_PreGameMultiplayer));
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.ResetCheckpoint))
			{
				AudioController.Play("Reset");
				Singleton<PlayerManager>.SP.ResetPlayer(Singleton<PlayerManager>.SP.GetPlayer(), hard: false);
				Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerGraphics>().ParticleEvent(SfxEvents.ControlEnabled);
			}
		}
	}

	public void UpdateCollectibles()
	{
		allCollectibles = Object.FindObjectsOfType<Collectable>();
		prevCollectibles = 0;
	}

	public void FixedUpdate()
	{
		int num = 0;
		Collectable[] array = allCollectibles;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].isPickedUp)
			{
				num++;
			}
		}
		if (prevCollectibles != num && num == 0)
		{
			Singleton<CheckpointManager>.SP.Finish(Singleton<Stopwatch>.SP.GetCurrentTime());
		}
		prevCollectibles = num;
	}
}
