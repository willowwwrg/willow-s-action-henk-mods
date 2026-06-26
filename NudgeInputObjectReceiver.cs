using System.Collections.Generic;
using UnityEngine;

public class NudgeInputObjectReceiver : MonoBehaviour
{
	public List<UITweener> onSelectTweens = new List<UITweener>();

	private bool selected;

	public void Select()
	{
		selected = true;
		foreach (UITweener onSelectTween in onSelectTweens)
		{
			onSelectTween.Play(forward: true);
		}
	}

	public void Deselect()
	{
		selected = false;
		foreach (UITweener onSelectTween in onSelectTweens)
		{
			onSelectTween.ResetToBeginning();
		}
	}

	private void Update()
	{
		_ = selected;
	}
}
