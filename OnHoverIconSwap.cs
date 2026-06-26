using UnityEngine;

public class OnHoverIconSwap : MonoBehaviour
{
	public UISprite hoverSprite;

	public UISprite originalSprite;

	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			hoverSprite.enabled = true;
			originalSprite.enabled = false;
		}
		else
		{
			hoverSprite.enabled = false;
			originalSprite.enabled = true;
		}
	}

	public void OnDisable()
	{
		hoverSprite.enabled = false;
		originalSprite.enabled = true;
	}
}
