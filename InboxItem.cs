using UnityEngine;

public class InboxItem : MonoBehaviour
{
	private string originalSprite;

	public string selectedSprite;

	public UILabel fromLabel;

	public UILabel subjLabel;

	public UISprite unreadSprite;

	public InboxMessage inboxMessage;

	private void Awake()
	{
		originalSprite = GetComponent<UISprite>().spriteName;
	}

	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			GetComponent<UISprite>().spriteName = selectedSprite;
		}
		else
		{
			GetComponent<UISprite>().spriteName = originalSprite;
		}
	}

	public void Reset()
	{
		GetComponent<UISprite>().spriteName = originalSprite;
	}
}
