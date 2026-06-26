using System.Collections;
using Henk;
using UnityEngine;

public class GUI_InGameMultiplayer : GUI_Base
{
	public UILabel timeLabel;

	public UILabel checkpointLabel;

	public GameObject multiplayerGUI;

	private void TransitionCompleted()
	{
		InitializeScreen();
		multiplayerGUI.SetActive(value: true);
		checkpointLabel.text = string.Empty;
		Singleton<PermaGUI>.SP.ToggleMultiplayerMutatorText(Singleton<MutatorManager>.SP.mutatorActive, "Mutator: " + Singleton<MutatorManager>.SP.GetActiveMutatorString());
	}

	private void Update()
	{
		timeLabel.text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Singleton<Stopwatch>.SP.GetCurrentTime());
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Start))
		{
			Singleton<InGameMenu>.SP.ToggleMenu(!Singleton<InGameMenu>.SP.menuEnabled);
		}
	}

	public void ShowCheckpointTime(float timeDiff)
	{
		StopCoroutine("ShowCPTime");
		StartCoroutine("ShowCPTime", timeDiff);
	}

	private IEnumerator ShowCPTime(float timeDiff)
	{
		bool flag = false;
		if (timeDiff > 0f)
		{
			flag = true;
		}
		string text = Singleton<HighscoreManager>.SP.ConvertTimeToString(Mathf.Abs(timeDiff));
		if (!flag)
		{
			checkpointLabel.color = Color.red;
			checkpointLabel.text = "+ " + text.ToString();
		}
		else
		{
			checkpointLabel.color = Color.green;
			checkpointLabel.text = "- " + text.ToString();
		}
		yield return new WaitForSeconds(1.5f);
		checkpointLabel.text = string.Empty;
	}

	private void Button_Quit()
	{
		Singleton<InGameMenu>.SP.ToggleMenu(toggle: false);
		multiplayerGUI.SetActive(value: false);
		Singleton<PermaGUI>.SP.ToggleChatBox(state: false);
		Singleton<MultiManager>.SP.Disconnect();
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
	}

	private void Button_Resume()
	{
		Singleton<InGameMenu>.SP.ToggleMenu(!Singleton<InGameMenu>.SP.menuEnabled);
	}
}
