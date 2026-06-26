using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerFriendlyName("Toggle Mesh Renderer")]
[USequencerEvent("Render/Toggle Mesh Renderer")]
public class USMeshRenderDisable : USEventBase
{
	public bool enable;

	private bool previousEnable;

	public override void FireEvent()
	{
		if ((bool)base.AffectedObject)
		{
			MeshRenderer component = base.AffectedObject.GetComponent<MeshRenderer>();
			if (!component)
			{
				Debug.LogWarning("You didn't add a Mesh Renderer to the Affected Object", base.AffectedObject);
				return;
			}
			previousEnable = component.enabled;
			component.enabled = enable;
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
	}

	public override void EndEvent()
	{
		UndoEvent();
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		if ((bool)base.AffectedObject)
		{
			MeshRenderer component = base.AffectedObject.GetComponent<MeshRenderer>();
			if (!component)
			{
				Debug.LogWarning("You didn't add a Mesh Renderer to the Affected Object", base.AffectedObject);
			}
			else
			{
				component.enabled = previousEnable;
			}
		}
	}
}
