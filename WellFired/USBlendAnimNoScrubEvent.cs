using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Blend Animation No Scrub (Legacy)")]
[USequencerEventHideDuration]
[USequencerEvent("Animation (Legacy)/Blend Animation No Scrub")]
public class USBlendAnimNoScrubEvent : USEventBase
{
	public AnimationClip blendedAnimation;

	public void Update()
	{
		if (base.Duration < 0f)
		{
			base.Duration = blendedAnimation.length;
		}
	}

	public override void FireEvent()
	{
		Animation component = base.AffectedObject.GetComponent<Animation>();
		if (!component)
		{
			Debug.Log("Attempting to play an animation on a GameObject without an Animation Component from USPlayAnimEvent.FireEvent");
			return;
		}
		component[blendedAnimation.name].wrapMode = WrapMode.Once;
		component[blendedAnimation.name].layer = 1;
	}

	public override void ProcessEvent(float deltaTime)
	{
		base.animation.CrossFade(blendedAnimation.name);
	}

	public override void StopEvent()
	{
		if ((bool)base.AffectedObject)
		{
			Animation component = base.AffectedObject.GetComponent<Animation>();
			if ((bool)component)
			{
				component.Stop();
			}
		}
	}
}
