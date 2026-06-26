using UnityEngine;

public class RotateMeBasedOnParentScale : MonoBehaviour
{
	private void Start()
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.y = Mathf.Sign(base.transform.parent.localScale.x) * 90f;
		base.transform.localEulerAngles = localEulerAngles;
	}
}
