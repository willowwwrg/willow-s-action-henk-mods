using System;
using UnityEngine;

[ExecuteInEditMode]
public class MegaTankWheels : MonoBehaviour
{
	public float radius = 1f;

	public MegaTracks track;

	private Vector3 localrot = Vector3.zero;

	public float offang;

	public MegaAxis axis = MegaAxis.Z;

	private void Start()
	{
		localrot = base.transform.localRotation.eulerAngles;
	}

	private void Update()
	{
		if ((bool)track && (bool)track.shape)
		{
			float t = track.shape.splines[track.curve].length * track.start * 0.01f / ((float)Math.PI * 2f * radius) * (float)Math.PI * 2f * 57.29578f;
			t = Mathf.Repeat(t, 360f);
			Vector3 euler = localrot;
			int index2;
			int index = (index2 = (int)axis);
			float num = euler[index2];
			euler[index] = num + (t + offang);
			base.transform.localRotation = Quaternion.Euler(euler);
		}
	}
}
