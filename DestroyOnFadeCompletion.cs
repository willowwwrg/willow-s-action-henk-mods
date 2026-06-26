using UnityEngine;

public class DestroyOnFadeCompletion : MonoBehaviour
{
	private void FadeCompleted()
	{
		Object.Destroy(base.gameObject);
	}
}
