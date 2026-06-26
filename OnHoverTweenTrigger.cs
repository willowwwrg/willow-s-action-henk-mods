using System.Collections.Generic;
using UnityEngine;

public class OnHoverTweenTrigger : MonoBehaviour
{
	public List<UITweener> tweensToTrigger = new List<UITweener>();

	public void OnHover(bool toggle)
	{
		if (tweensToTrigger.Count < 1)
		{
			return;
		}
		if (toggle)
		{
			for (int i = 0; i < tweensToTrigger.Count; i++)
			{
				tweensToTrigger[i].style = UITweener.Style.PingPong;
				tweensToTrigger[i].PlayForward();
			}
		}
		else
		{
			for (int j = 0; j < tweensToTrigger.Count; j++)
			{
				tweensToTrigger[j].style = UITweener.Style.Once;
				tweensToTrigger[j].PlayReverse();
			}
		}
	}
}
