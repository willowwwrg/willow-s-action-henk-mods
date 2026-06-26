using UnityEngine;

namespace WellFired;

[USequencerFriendlyName("Play Animation (Legacy)")]
[USequencerEvent("Animation (Legacy)/Play Animation")]
public class USPlayAnimEvent : USEventBase
{
	public AnimationClip animationClip;

	public WrapMode wrapMode;

	public float playbackSpeed = 1f;

	public void Update()
	{
		if (wrapMode != WrapMode.Loop && (bool)animationClip)
		{
			base.Duration = animationClip.length / playbackSpeed;
		}
	}

	public override void FireEvent()
	{
		if (!animationClip)
		{
			Debug.Log("Attempting to play an animation on a GameObject but you haven't given the event an animation clip from USPlayAnimEvent::FireEvent");
			return;
		}
		Animation component = base.AffectedObject.GetComponent<Animation>();
		if (!component)
		{
			Debug.Log("Attempting to play an animation on a GameObject without an Animation Component from USPlayAnimEvent.FireEvent");
			return;
		}
		component.wrapMode = wrapMode;
		component.Play(animationClip.name);
		AnimationState animationState = component[animationClip.name];
		if ((bool)animationState)
		{
			animationState.speed = playbackSpeed;
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
		Animation animation = base.AffectedObject.GetComponent<Animation>();
		if ((bool)animationClip)
		{
			if (!animation)
			{
				Debug.LogError(string.Concat("Trying to play an animation : ", animationClip.name, " but : ", base.AffectedObject, " doesn't have an animation component, we will add one, this time, though you should add it manually"));
				animation = base.AffectedObject.AddComponent<Animation>();
			}
			if (animation[animationClip.name] == null)
			{
				Debug.LogError("Trying to play an animation : " + animationClip.name + " but it isn't in the animation list. I will add it, this time, though you should add it manually.");
				animation.AddClip(animationClip, animationClip.name);
			}
			AnimationState animationState = animation[animationClip.name];
			if (!animation.IsPlaying(animationClip.name))
			{
				animation.wrapMode = wrapMode;
				animation.Play(animationClip.name);
			}
			animationState.speed = playbackSpeed;
			animationState.time = deltaTime * playbackSpeed;
			animationState.enabled = true;
			animation.Sample();
			animationState.enabled = false;
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
