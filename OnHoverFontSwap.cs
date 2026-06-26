using UnityEngine;

public class OnHoverFontSwap : MonoBehaviour
{
	public UIFont hoverFont;

	private UIFont oldFont;

	private void Awake()
	{
		oldFont = GetComponentInChildren<UILabel>().bitmapFont;
	}

	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			GetComponentInChildren<UILabel>().bitmapFont = hoverFont;
		}
		else
		{
			GetComponentInChildren<UILabel>().bitmapFont = oldFont;
		}
	}
}
