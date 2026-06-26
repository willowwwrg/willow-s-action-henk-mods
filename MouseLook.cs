using UnityEngine;

public class MouseLook : MonoBehaviour
{
	private float lookX;

	private float lookY;

	public float lookSpeed = 40f;

	public float lookSmooth = 0.1f;

	public float lookXMax = 20f;

	public float lookYMax = 20f;

	private void Update()
	{
		lookX += Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
		lookY += Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
		lookX = Mathf.Clamp(lookX, 0f - lookXMax, lookXMax);
		lookY = Mathf.Clamp(lookY, 0f - lookYMax, lookYMax);
		Quaternion to = Quaternion.Euler(0f - lookY, lookX, 0f);
		base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, to, lookSmooth);
	}
}
