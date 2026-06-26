using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Send Message")]
[USequencerEvent("Signal/Send Message")]
[USequencerEventHideDuration]
public class USSendMessageEvent : USEventBase
{
	public GameObject receiver;

	public string action = "OnSignal";

	public override void FireEvent()
	{
		if (Application.isPlaying)
		{
			if ((bool)receiver)
			{
				receiver.SendMessage(action);
				return;
			}
			Debug.LogWarning("No receiver of signal \"" + action + "\" on object " + receiver.name + " (" + receiver.GetType().Name + ")", receiver);
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
	}
}
