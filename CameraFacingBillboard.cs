using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.LookAt(Camera.main.transform, base.transform.up);
	}
}
