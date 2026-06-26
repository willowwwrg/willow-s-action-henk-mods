using UnityEngine;

public class OnSelectMainMenuItem : MonoBehaviour
{
	public Vector3 highlighterPos;

	public bool selectOnAwake;

	private void Awake()
	{
		if (selectOnAwake)
		{
			Singleton<MainMenuHighlighter>.SP.SetTitle(string.Empty);
			Singleton<MainMenuHighlighter>.SP.SetTargetPosition(highlighterPos, hard: true);
		}
	}

	public void Select()
	{
		Singleton<MainMenuHighlighter>.SP.SetTitle(string.Empty);
		Singleton<MainMenuHighlighter>.SP.SetTargetPosition(highlighterPos);
	}
}
