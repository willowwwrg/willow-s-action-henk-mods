using UnityEngine;

public class AnchorResolutionScale : MonoBehaviour
{
	private void Update()
	{
		float num = Mathf.Lerp(1f, 1.2f, Mathf.InverseLerp(1.25f, 1.78f, (float)Screen.width / (float)Screen.height));
		base.transform.localScale = new Vector3(num, num, num);
	}
}
