using UnityEngine;

public class Billboard : MonoBehaviour
{
	public enum Axis
	{
		up,
		down,
		left,
		right,
		forward,
		back
	}

	private Camera referenceCamera;

	public bool reverseFace;

	public Axis axis;

	public Vector3 GetAxis(Axis refAxis)
	{
		return refAxis switch
		{
			Axis.down => Vector3.down, 
			Axis.forward => Vector3.forward, 
			Axis.back => Vector3.back, 
			Axis.left => Vector3.left, 
			Axis.right => Vector3.right, 
			_ => Vector3.up, 
		};
	}

	private void Awake()
	{
		if (!referenceCamera)
		{
			referenceCamera = Camera.main;
		}
	}

	private void FixedUpdate()
	{
		Vector3 worldPosition = base.transform.position + referenceCamera.transform.rotation * ((!reverseFace) ? Vector3.back : Vector3.forward);
		Vector3 worldUp = referenceCamera.transform.rotation * GetAxis(axis);
		base.transform.LookAt(worldPosition, worldUp);
	}
}
