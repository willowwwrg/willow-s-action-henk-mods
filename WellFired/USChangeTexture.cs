using UnityEngine;

namespace WellFired;

[USequencerEvent("Render/Change Objects Texture")]
[USequencerEventHideDuration]
[USequencerFriendlyName("Change Texture")]
public class USChangeTexture : USEventBase
{
	public Texture newTexture;

	private Texture previousTexture;

	public override void FireEvent()
	{
		if ((bool)base.AffectedObject)
		{
			if (!newTexture)
			{
				Debug.LogWarning("you've not given a texture to the USChangeTexture Event", this);
			}
			else if (!Application.isPlaying && Application.isEditor)
			{
				previousTexture = base.AffectedObject.renderer.sharedMaterial.mainTexture;
				base.AffectedObject.renderer.sharedMaterial.mainTexture = newTexture;
			}
			else
			{
				previousTexture = base.AffectedObject.renderer.material.mainTexture;
				base.AffectedObject.renderer.material.mainTexture = newTexture;
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
		if ((bool)base.AffectedObject && (bool)previousTexture)
		{
			if (!Application.isPlaying && Application.isEditor)
			{
				base.AffectedObject.renderer.sharedMaterial.mainTexture = previousTexture;
			}
			else
			{
				base.AffectedObject.renderer.material.mainTexture = previousTexture;
			}
			previousTexture = null;
		}
	}
}
