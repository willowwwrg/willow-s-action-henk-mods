using System;
using UnityEngine;

public class EditorCameraFollow : Singleton<EditorCameraFollow>
{
	public float accel = 160f;

	public float accelFast = 400f;

	public float friction = 6f;

	public float mouseAccel = 1f;

	private Vector3 velocity;

	private SplineFollow spline;

	private Vector3 prevMousePos;

	private void Awake()
	{
		spline = GetComponent<SplineFollow>();
	}

	private void FixedUpdate()
	{
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.D))
		{
			zero += Vector3.right;
		}
		if (Input.GetKey(KeyCode.A))
		{
			zero += Vector3.left;
		}
		if (Input.GetKey(KeyCode.W))
		{
			zero += Vector3.up;
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero += Vector3.down;
		}
		float num = accel;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			num = accelFast;
		}
		velocity -= velocity * friction * Time.deltaTime;
		velocity += num * zero * Time.deltaTime;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 vector = mousePosition - prevMousePos;
		vector.z = 0f;
		if (Input.GetMouseButton(1))
		{
			float baseDistance = Camera.main.GetComponent<PlatformerCamera>().baseDistance;
			float num2 = Mathf.Tan(Camera.main.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * baseDistance * 2f / (float)Screen.width;
			velocity -= vector * mouseAccel * num2;
		}
		prevMousePos = mousePosition;
		spline.splineOffset += velocity * Time.deltaTime;
		spline.ForceOneUpdate();
	}
}
