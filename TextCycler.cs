using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class TextCycler : MonoBehaviour
{
	public List<string> strings;

	public float swapsPerSecond = 1f;

	private UILabel label;

	private int stringCounter;

	private int frameCounter;

	private float triggerFrame;

	private void Awake()
	{
		if (strings.Count >= 2)
		{
			triggerFrame = 80f / swapsPerSecond;
			frameCounter = 0;
			stringCounter = 0;
			label = GetComponent<UILabel>();
			label.text = strings[stringCounter];
		}
	}

	private void FixedUpdate()
	{
		if (strings.Count < 2)
		{
			return;
		}
		frameCounter++;
		if ((float)frameCounter > triggerFrame)
		{
			frameCounter = 0;
			stringCounter++;
			if (stringCounter > strings.Count - 1)
			{
				stringCounter = 0;
			}
			label.text = strings[stringCounter];
		}
	}
}
