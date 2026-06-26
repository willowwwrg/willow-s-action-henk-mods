using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerFriendlyName("Toggle Component")]
[USequencerEvent("Object/Toggle Component")]
public class USEnableComponentEvent : USEventBase
{
	public bool enableComponent;

	private bool prevEnable;

	[HideInInspector]
	[SerializeField]
	private string componentName;

	public string ComponentName
	{
		get
		{
			return componentName;
		}
		set
		{
			componentName = value;
		}
	}

	public override void FireEvent()
	{
		Behaviour behaviour = base.AffectedObject.GetComponent(ComponentName) as Behaviour;
		if ((bool)behaviour)
		{
			prevEnable = behaviour.enabled;
			behaviour.enabled = enableComponent;
		}
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
			Behaviour behaviour = base.AffectedObject.GetComponent(ComponentName) as Behaviour;
			if ((bool)behaviour)
			{
				behaviour.enabled = prevEnable;
			}
		}
	}
}
