using System.Collections;
using UnityEngine;

public class GA_GameObjectManager : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void RunCoroutine(IEnumerator routine)
	{
		StartCoroutine(routine);
	}

	public void OnApplicationQuit()
	{
		if (GA.SettingsGA.DelayQuitToSendData && !GA_Queue.QUITONSUBMIT)
		{
			GA_Queue.QUITONSUBMIT = true;
			GA.API.Design.NewEvent("GA:ExitGame");
			GA_Queue.SubmitQueue();
		}
	}
}
