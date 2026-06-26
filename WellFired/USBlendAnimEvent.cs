using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Blend Animation (Legacy)")]
[USequencerEvent("Animation (Legacy)/Blend Animation")]
public class USBlendAnimEvent : USEventBase
{
	public AnimationClip animationClipSource;

	public AnimationClip animationClipDest;

	public float blendPoint = 1f;

	public void Update()
	{
		if (base.Duration < 0f)
		{
			base.Duration = 2f;
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
		component.wrapMode = WrapMode.Loop;
		component.Play(animationClipSource.name);
	}

	public override void ProcessEvent(float deltaTime)
	{
		Animation animation = base.AffectedObject.GetComponent<Animation>();
		if (!animation)
		{
			Debug.LogError(string.Concat("Trying to play an animation : ", animationClipSource.name, " but : ", base.AffectedObject, " doesn't have an animation component, we will add one, this time, though you should add it manually"));
			animation = base.AffectedObject.AddComponent<Animation>();
		}
		if (animation[animationClipSource.name] == null)
		{
			Debug.LogError("Trying to play an animation : " + animationClipSource.name + " but it isn't in the animation list. I will add it, this time, though you should add it manually.");
			animation.AddClip(animationClipSource, animationClipSource.name);
		}
		if (animation[animationClipDest.name] == null)
		{
			Debug.LogError("Trying to play an animation : " + animationClipDest.name + " but it isn't in the animation list. I will add it, this time, though you should add it manually.");
			animation.AddClip(animationClipDest, animationClipDest.name);
		}
		if (deltaTime < blendPoint)
		{
			animation.CrossFade(animationClipSource.name);
		}
		else
		{
			animation.CrossFade(animationClipDest.name);
		}
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
