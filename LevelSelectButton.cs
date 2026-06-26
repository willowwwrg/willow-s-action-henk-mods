using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
	public List<GameObject> buttons;

	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			GetComponent<UIButtonMessage>().target.SendMessage("HoverOverLevelSelectButton", base.gameObject);
		}
	}
}
