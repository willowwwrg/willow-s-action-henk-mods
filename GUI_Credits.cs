using System.Collections;
using UnityEngine;

public class GUI_Credits : MonoBehaviour
{
	private bool startedScrolling;

	public float scrollSpeedPerSecond = 75f;

	public float scrollStartDelay = 1f;

	public int maxY = 1500;

	public Transform creditsObj;

	private void TransitionCompleted()
	{
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: true);
		Singleton<PermaGUI>.SP.ToggleBackForwardsButtons(stateBack: true, stateForwards: false, string.Empty, string.Empty);
		Singleton<PermaGUI>.SP.BGPanelSetTitles("Credits", string.Empty);
		Singleton<PermaGUI>.SP.ToggleBGPanelLeft(state: false, string.Empty);
		creditsObj.localPosition = new Vector3(0f, -650f, 0f);
		StartCoroutine(ScrollStartDelay());
	}

	private void PrevWindow()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
		AudioController.Play("ButtonBackwards");
		Singleton<PermaGUI>.SP.ToggleBGPanel(state: false);
	}

	private IEnumerator ScrollStartDelay()
	{
		startedScrolling = false;
		yield return new WaitForSeconds(scrollStartDelay);
		startedScrolling = true;
	}

	private void FixedUpdate()
	{
		if (startedScrolling)
		{
			creditsObj.localPosition = new Vector3(creditsObj.localPosition.x, creditsObj.localPosition.y + Time.fixedDeltaTime * scrollSpeedPerSecond, creditsObj.localPosition.z);
		}
		if (creditsObj.localPosition.y > (float)maxY)
		{
			startedScrolling = false;
		}
	}
}
