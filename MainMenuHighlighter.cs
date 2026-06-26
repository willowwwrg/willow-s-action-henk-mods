using UnityEngine;

public class MainMenuHighlighter : Singleton<MainMenuHighlighter>
{
	private Vector3 targetPos = Vector3.zero;

	public float lerpFactor = 5.5f;

	public Vector3 titlePosition;

	private void Update()
	{
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, targetPos, Time.deltaTime * lerpFactor);
	}

	public void SetTargetPosition(Vector3 pos, bool hard = false)
	{
		ToggleOnOff(state: true);
		targetPos = pos;
		if (hard)
		{
			base.transform.localPosition = pos;
		}
	}

	public void ToggleOnOff(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void SetTitle(string title)
	{
		base.transform.GetComponentInChildren<UILabel>().text = title;
	}

	public void SetAsTitle(string titleText = "")
	{
		SetTargetPosition(titlePosition, hard: true);
		if (titleText != string.Empty)
		{
			SetTitle(titleText);
		}
		ToggleOnOff(state: true);
	}
}
