using UnityEngine;

namespace WellFired;

[USequencerEventHideDuration]
[USequencerFriendlyName("Display Image")]
[USequencerEvent("Fullscreen/Display Image")]
public class USDisplayImageEvent : USEventBase
{
	public UILayer uiLayer;

	public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f), new Keyframe(3f, 1f), new Keyframe(4f, 0f));

	public Texture2D displayImage;

	public UIPosition displayPosition;

	public UIPosition anchorPosition;

	private float currentCurveSampleTime;

	public override void FireEvent()
	{
		if (!displayImage)
		{
			Debug.LogWarning("Trying to use a DisplayImage Event, but you didn't give it an image to display", this);
		}
	}

	public override void ProcessEvent(float deltaTime)
	{
		currentCurveSampleTime = deltaTime;
	}

	public override void EndEvent()
	{
		float b = fadeCurve.Evaluate(fadeCurve.keys[fadeCurve.length - 1].time);
		b = Mathf.Min(Mathf.Max(0f, b), 1f);
	}

	public override void StopEvent()
	{
		UndoEvent();
	}

	public override void UndoEvent()
	{
		currentCurveSampleTime = 0f;
	}

	private void OnGUI()
	{
		if (!base.Sequence.IsPlaying)
		{
			return;
		}
		float num = 0f;
		Keyframe[] keys = fadeCurve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			if (keyframe.time > num)
			{
				num = keyframe.time;
			}
		}
		base.Duration = num;
		float b = fadeCurve.Evaluate(currentCurveSampleTime);
		b = Mathf.Min(Mathf.Max(0f, b), 1f);
		if ((bool)displayImage)
		{
			Rect position = new Rect((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, displayImage.width, displayImage.height);
			switch (displayPosition)
			{
			case UIPosition.TopLeft:
				position.x = 0f;
				position.y = 0f;
				break;
			case UIPosition.TopRight:
				position.x = Screen.width;
				position.y = 0f;
				break;
			case UIPosition.BottomLeft:
				position.x = 0f;
				position.y = Screen.height;
				break;
			case UIPosition.BottomRight:
				position.x = Screen.width;
				position.y = Screen.height;
				break;
			}
			switch (anchorPosition)
			{
			case UIPosition.Center:
				position.x -= (float)displayImage.width * 0.5f;
				position.y -= (float)displayImage.height * 0.5f;
				break;
			case UIPosition.TopRight:
				position.x -= displayImage.width;
				break;
			case UIPosition.BottomLeft:
				position.y -= displayImage.height;
				break;
			case UIPosition.BottomRight:
				position.x -= displayImage.width;
				position.y -= displayImage.height;
				break;
			}
			GUI.depth = (int)uiLayer;
			Color color = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, b);
			GUI.DrawTexture(position, displayImage);
			GUI.color = color;
		}
	}
}
