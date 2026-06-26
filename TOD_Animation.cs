using System;
using UnityEngine;

public class TOD_Animation : MonoBehaviour
{
	public float WindDegrees;

	public float WindSpeed = 3f;

	internal Vector4 CloudUV { get; set; }

	internal Vector4 OffsetUV
	{
		get
		{
			Vector3 position = base.transform.position;
			Vector3 lossyScale = base.transform.lossyScale;
			Vector3 direction = new Vector3(position.x / lossyScale.x, 0f, position.z / lossyScale.z);
			direction = -base.transform.TransformDirection(direction);
			return new Vector4(direction.x, direction.z, direction.x, direction.z);
		}
	}

	protected void Update()
	{
		Vector2 vector = new Vector2(Mathf.Cos((float)Math.PI / 180f * (WindDegrees + 15f)), Mathf.Sin((float)Math.PI / 180f * (WindDegrees + 15f)));
		Vector2 vector2 = new Vector2(Mathf.Cos((float)Math.PI / 180f * (WindDegrees - 15f)), Mathf.Sin((float)Math.PI / 180f * (WindDegrees - 15f)));
		Vector4 vector3 = WindSpeed / 100f * new Vector4(vector.x, vector.y, vector2.x, vector2.y);
		CloudUV += Time.deltaTime * vector3;
	}
}
