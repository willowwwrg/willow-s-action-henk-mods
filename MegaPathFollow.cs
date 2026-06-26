using UnityEngine;

[ExecuteInEditMode]
public class MegaPathFollow : MonoBehaviour
{
	public float tangentDist = 0.01f;

	public float alpha;

	public float speed;

	public bool rot;

	public float time;

	public float ctime;

	public int curve;

	public MegaShape target;

	public float distance;

	public bool animate;

	public bool UseDistance = true;

	public bool addtwist;

	public Vector3 offset = Vector3.zero;

	public Vector3 rotate = Vector3.zero;

	public MegaRepeatMode loopmode;

	public void SetPos(float a)
	{
		if (!(target != null))
		{
			return;
		}
		float twist = 0f;
		switch (loopmode)
		{
		case MegaRepeatMode.Clamp:
			a = Mathf.Clamp01(a);
			break;
		case MegaRepeatMode.Loop:
			a = Mathf.Repeat(a, 1f);
			break;
		case MegaRepeatMode.PingPong:
			a = Mathf.PingPong(a, 1f);
			break;
		}
		Vector3 zero = Vector3.zero;
		Vector3 vector = target.InterpCurve3D(curve, a, target.normalizedInterp, ref twist);
		if (rot)
		{
			float num = tangentDist / target.GetCurveLength(curve);
			Vector3 vector2 = target.InterpCurve3D(curve, a + num, target.normalizedInterp);
			Vector3 euler = rotate;
			if (addtwist)
			{
				euler.z += twist;
			}
			Quaternion quaternion = Quaternion.Euler(euler);
			Vector3 forward = vector2 - vector;
			Vector3 vector3 = Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one).MultiplyVector(Vector3.up);
			Vector3 vector4 = Vector3.Cross(forward.normalized, vector3);
			zero = Vector3.zero;
			zero += vector4 * offset.x;
			zero += vector3 * offset.y;
			Quaternion quaternion2 = Quaternion.LookRotation(forward);
			base.transform.rotation = target.transform.rotation * quaternion2 * quaternion;
		}
		base.transform.position = target.transform.TransformPoint(vector - zero);
	}

	public void SetPosFomDist(float dist)
	{
		if (!(target != null))
		{
			return;
		}
		float num = dist / target.GetCurveLength(curve);
		float twist = 0f;
		switch (loopmode)
		{
		case MegaRepeatMode.Clamp:
			num = Mathf.Clamp01(num);
			break;
		case MegaRepeatMode.Loop:
			num = Mathf.Repeat(num, 1f);
			break;
		case MegaRepeatMode.PingPong:
			num = Mathf.PingPong(num, 1f);
			break;
		}
		Vector3 zero = Vector3.zero;
		Vector3 vector = target.InterpCurve3D(curve, num, target.normalizedInterp, ref twist);
		if (rot)
		{
			float num2 = tangentDist / target.GetCurveLength(curve);
			Vector3 vector2 = target.InterpCurve3D(curve, num + num2, target.normalizedInterp);
			Vector3 euler = rotate;
			if (addtwist)
			{
				euler.z += twist;
			}
			Quaternion quaternion = Quaternion.Euler(euler);
			Vector3 forward = vector2 - vector;
			Vector3 vector3 = Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one).MultiplyVector(Vector3.up);
			Vector3 vector4 = Vector3.Cross(forward.normalized, vector3);
			zero = Vector3.zero;
			zero += vector4 * offset.x;
			zero += vector3 * offset.y;
			Quaternion quaternion2 = Quaternion.LookRotation(forward);
			base.transform.rotation = target.transform.rotation * quaternion2 * quaternion;
		}
		base.transform.position = target.transform.TransformPoint(vector - zero);
	}

	public void Start()
	{
		ctime = 0f;
		curve = 0;
	}

	private void Update()
	{
		if (animate)
		{
			if (UseDistance)
			{
				distance += speed * Time.deltaTime;
			}
			else if (time > 0f)
			{
				ctime += Time.deltaTime;
				if (ctime > time)
				{
					ctime = 0f;
				}
				alpha = ctime / time * 100f;
			}
			else if (speed != 0f)
			{
				alpha += speed * Time.deltaTime;
				if (alpha > 100f)
				{
					alpha = 0f;
				}
				else if (alpha < 0f)
				{
					alpha = 100f;
				}
			}
		}
		if (UseDistance)
		{
			SetPosFomDist(distance);
		}
		else
		{
			SetPos(alpha * 0.01f);
		}
	}
}
