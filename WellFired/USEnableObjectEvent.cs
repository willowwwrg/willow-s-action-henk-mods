namespace WellFired;

[USequencerEvent("Object/Toggle Object")]
[USequencerFriendlyName("Toggle Object")]
[USequencerEventHideDuration]
public class USEnableObjectEvent : USEventBase
{
	public bool enable;

	private bool prevEnable;

	public override void FireEvent()
	{
		prevEnable = base.AffectedObject.activeSelf;
		base.AffectedObject.SetActive(enable);
	}

	public override void ProcessEvent(float deltaTime)
	{
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		if ((bool)base.AffectedObject)
		{
			base.AffectedObject.SetActive(prevEnable);
		}
	}
}
