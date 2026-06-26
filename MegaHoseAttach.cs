using UnityEngine;

[ExecuteInEditMode]
public class MegaHoseAttach : MonoBehaviour
{
	public float alpha;

	public MegaHose hose;

	public Vector3 offset = Vector3.zero;

	public Vector3 rotate = Vector3.zero;

	public bool doLateUpdate = true;

	public bool rot = true;

	private void Update()
	{
		if (!doLateUpdate)
		{
			PositionObject();
		}
	}

	private void LateUpdate()
	{
		if (doLateUpdate)
		{
			PositionObject();
		}
	}

	private void PositionObject()
	{
		if ((bool)hose)
		{
			Vector3 position = hose.GetPosition(alpha);
			if (rot)
			{
				Quaternion rotation = Quaternion.LookRotation(hose.GetPosition(alpha + 0.001f) - position) * Quaternion.Euler(rotate);
				base.transform.rotation = rotation;
			}
			base.transform.position = position + base.transform.TransformDirection(offset);
		}
	}
}
