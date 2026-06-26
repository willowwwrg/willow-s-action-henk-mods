using UnityEngine;

public class LateScale : MonoBehaviour
{
	public float scale = 1f;

	private void LateUpdate()
	{
		base.transform.localScale = Vector3.one * scale;
	}
}
