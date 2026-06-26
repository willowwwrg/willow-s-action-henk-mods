using UnityEngine;

public class Freeflight : MonoBehaviour
{
	private enum ControlAxis
	{
		MouseXandY,
		MouseX,
		MouseY
	}

	private ControlAxis controlAxis;

	[SerializeField]
	private float moveSpeed = 3f;

	[SerializeField]
	private float sensitivityX = 3f;

	[SerializeField]
	private float sensitivityY = 3f;

	[SerializeField]
	private float minimumX = -360f;

	[SerializeField]
	private float maximumX = 360f;

	[SerializeField]
	private float minimumY = -90f;

	[SerializeField]
	private float maximumY = 90f;

	private float rotationX;

	private float rotationY;

	private void Update()
	{
		if (Input.GetAxis("Vertical") != 0f)
		{
			float axis = Input.GetAxis("Vertical");
			base.transform.localPosition += base.transform.localRotation * new Vector3(0f, 0f, axis) * moveSpeed * Time.deltaTime;
		}
		if (Input.GetAxis("Horizontal") != 0f)
		{
			float axis2 = Input.GetAxis("Horizontal");
			base.transform.localPosition += base.transform.localRotation * new Vector3(axis2, 0f, 0f) * moveSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.E))
		{
			base.transform.localPosition += Vector3.up * moveSpeed * Time.deltaTime * 0.6f;
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			base.transform.localPosition += Vector3.down * moveSpeed * Time.deltaTime * 0.6f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			moveSpeed = Mathf.Clamp(moveSpeed + Input.GetAxis("Mouse ScrollWheel") * 2f, 0.01f, 10f);
		}
		if (controlAxis == ControlAxis.MouseXandY)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			Adjust360andClamp();
			KeyLookAround();
			KeyLookUp();
		}
		else if (controlAxis == ControlAxis.MouseX)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			Adjust360andClamp();
			KeyLookAround();
			KeyLookUp();
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			Adjust360andClamp();
			KeyLookAround();
			KeyLookUp();
		}
	}

	private void KeyLookAround()
	{
		Adjust360andClamp();
		base.transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
	}

	private void KeyLookUp()
	{
		Adjust360andClamp();
		base.transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
	}

	private void Adjust360andClamp()
	{
		if (rotationX < -360f)
		{
			rotationX += 360f;
		}
		else if (rotationX > 360f)
		{
			rotationX -= 360f;
		}
		if (rotationY < -360f)
		{
			rotationY += 360f;
		}
		else if (rotationY > 360f)
		{
			rotationY -= 360f;
		}
		rotationX = Mathf.Clamp(rotationX, minimumX, maximumX);
		rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
	}

	private void Start()
	{
		if ((bool)base.rigidbody)
		{
			base.rigidbody.freezeRotation = true;
		}
	}
}
