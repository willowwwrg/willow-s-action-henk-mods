using UnityEngine;

public class CameraLag : MonoBehaviour
{
	public Transform attach;

	public Transform target;

	public float time = 1f;

	public Vector3 pos;

	private Vector3 spd = Vector3.zero;

	private void Start()
	{
		if ((bool)attach)
		{
			pos = attach.position;
		}
	}

	private void LateUpdate()
	{
		if ((bool)attach)
		{
			Vector3 position = attach.position;
			pos = Vector3.SmoothDamp(pos, position, ref spd, time);
			base.transform.position = pos;
			if ((bool)target)
			{
				base.transform.rotation = Quaternion.LookRotation(target.position - pos);
			}
		}
	}
}
