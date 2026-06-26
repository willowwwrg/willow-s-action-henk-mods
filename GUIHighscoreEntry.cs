using UnityEngine;

public class GUIHighscoreEntry : MonoBehaviour
{
	public HighscoreEntry highscoreEntry;

	public string originalSprite;

	public string selectedSprite;

	public UIToggle checkbox;

	public UISprite envelope;

	public UISprite exclamation;

	public GameObject toggleCheckboxButton;

	private void Awake()
	{
		if ((bool)toggleCheckboxButton)
		{
			toggleCheckboxButton.SetActive(value: false);
		}
		originalSprite = GetComponent<UISprite>().spriteName;
	}

	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			GetComponent<UISprite>().spriteName = selectedSprite;
			if ((bool)toggleCheckboxButton && checkbox.gameObject.activeSelf)
			{
				toggleCheckboxButton.SetActive(value: true);
			}
		}
		else
		{
			GetComponent<UISprite>().spriteName = originalSprite;
			if ((bool)toggleCheckboxButton)
			{
				toggleCheckboxButton.SetActive(value: false);
			}
		}
	}

	private void Update()
	{
		if ((bool)toggleCheckboxButton && !checkbox.gameObject.activeSelf)
		{
			toggleCheckboxButton.SetActive(value: false);
		}
	}

	public void Reset()
	{
		GetComponent<UISprite>().spriteName = originalSprite;
		if ((bool)toggleCheckboxButton)
		{
			toggleCheckboxButton.SetActive(value: false);
		}
	}
}
