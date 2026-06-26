using UnityEngine;

public class GUI_PostGameEditor : MonoBehaviour
{
	public GameObject uploadingPanel;

	private void TransitionCompleted()
	{
		uploadingPanel.SetActive(Singleton<Workshop>.SP.validating);
	}

	private void Update()
	{
	}
}
