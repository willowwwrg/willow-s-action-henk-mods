using UnityEngine;

[ExecuteInEditMode]
public class MegaCharacterFollow : MonoBehaviour
{
	public MegaShape path;

	public bool rot;

	public Vector3 rotate = Vector3.zero;

	private void Start()
	{
		float alpha = 0f;
		Vector3 tangent = Vector3.zero;
		int kn = 0;
		Vector3 position = base.transform.position;
		Vector3 position2 = path.FindNearestPointWorld(position, 5, ref kn, ref tangent, ref alpha);
		base.rigidbody.MovePosition(position2);
	}

	private void LateUpdate()
	{
		if ((bool)path)
		{
			Vector3 position = base.transform.position;
			float alpha = 0f;
			Vector3 tangent = Vector3.zero;
			int kn = 0;
			Vector3 vector = path.FindNearestPointWorld(position, 5, ref kn, ref tangent, ref alpha);
			if (rot)
			{
				Vector3 vector2 = path.splines[0].InterpCurve3D(alpha + 0.001f, type: true, ref kn);
				Quaternion quaternion = Quaternion.Euler(rotate);
				Quaternion quaternion2 = Quaternion.LookRotation(vector2 - vector);
				base.transform.rotation = path.transform.rotation * quaternion2 * quaternion;
			}
			vector.y = position.y;
			base.transform.position = vector;
		}
	}
}
