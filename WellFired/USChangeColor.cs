using UnityEngine;

namespace WellFired;

[USequencerEvent("Render/Change Objects Color")]
[USequencerEventHideDuration]
[USequencerFriendlyName("Change Color")]
public class USChangeColor : USEventBase
{
	public Color newColor;

	private Color previousColor;

	public override void FireEvent()
	{
		if ((bool)base.AffectedObject)
		{
			if (!Application.isPlaying && Application.isEditor)
			{
				previousColor = base.AffectedObject.renderer.sharedMaterial.color;
				base.AffectedObject.renderer.sharedMaterial.color = newColor;
			}
			else
			{
				previousColor = base.AffectedObject.renderer.material.color;
				base.AffectedObject.renderer.material.color = newColor;
			}
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
			if (!Application.isPlaying && Application.isEditor)
			{
				base.AffectedObject.renderer.sharedMaterial.color = previousColor;
			}
			else
			{
				base.AffectedObject.renderer.material.color = previousColor;
			}
		}
	}
}
