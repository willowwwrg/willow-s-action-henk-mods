using UnityEngine;

public class GameCamera : MonoBehaviour
{
	public KeyCode RollemKey = KeyCode.F1;

	public Vector3 cameraPos = Vector3.zero;

	public Vector3 cameraCurrentPos = Vector3.zero;

	public Vector3 cameraRot = Vector3.zero;

	public Vector3 cameraCurrentRot = Vector3.zero;

	public Vector3 posShakeScale = Vector3.zero;

	public Vector3 rotShakeScale = Vector3.zero;

	public float posShakeFreq = 1f;

	public float rotShakeFreq = 1f;

	public float cameraFOV;

	public float cameraCurrentFOV;

	public MegaEaseType transease = MegaEaseType.InOutQuint;

	public float swaptime = 2f;

	public TransitionType transType;

	public float OutTime = 1f;

	public float InTime = 1f;

	public float minFOV = 15f;

	public float maxFOV = 120f;

	public bool FlatZoom;

	public float zoomwidth = 1f;

	public float MaxFOV = 65f;

	public float MinFOV = 5f;

	public AnimationCurve easeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public Vector3 maxMouseSpeed = Vector2.one;

	public AnimationCurve mouseSpeedXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve mouseSpeedYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public virtual string CameraName()
	{
		return "Default";
	}

	public virtual bool CheckModeKey()
	{
		return Input.GetKeyDown(RollemKey);
	}

	public virtual void Cut()
	{
	}

	public virtual void Rollem(GameCamera camera)
	{
	}

	public virtual void Init(CameraManager man)
	{
	}

	public virtual bool Valid()
	{
		return true;
	}

	public virtual void CameraUpdate(CameraManager man)
	{
	}

	public virtual Vector3 GetCamPos()
	{
		return cameraCurrentPos;
	}

	public virtual Vector3 GetCamRot()
	{
		return cameraCurrentRot;
	}

	public virtual float GetFOV()
	{
		return cameraCurrentFOV;
	}

	public virtual MegaEaseType GetEaseType()
	{
		return transease;
	}

	public virtual float GetSwitchTime()
	{
		return swaptime;
	}

	public virtual Vector3 GetTargetPos()
	{
		return Vector3.zero;
	}

	public void LimitMouse(ref float mx, ref float my)
	{
		mx = Mathf.Clamp(mx, 0f - maxMouseSpeed.x, maxMouseSpeed.x);
		my = Mathf.Clamp(my, 0f - maxMouseSpeed.y, maxMouseSpeed.y);
		mx = mouseSpeedXCurve.Evaluate(Mathf.Abs(mx) / maxMouseSpeed.x) * Mathf.Sign(mx) * maxMouseSpeed.x;
		my = mouseSpeedYCurve.Evaluate(Mathf.Abs(my) / maxMouseSpeed.y) * Mathf.Sign(my) * maxMouseSpeed.y;
	}

	public virtual void UpdateFOV()
	{
		if (FlatZoom)
		{
			if (Input.GetKey(KeyCode.Equals))
			{
				zoomwidth += Time.deltaTime * 1f;
			}
			if (Input.GetKey(KeyCode.Minus))
			{
				zoomwidth -= Time.deltaTime * 1f;
			}
			zoomwidth = Mathf.Clamp(zoomwidth, 0.1f, 10f);
			float x = Vector3.Distance(cameraCurrentPos, GetTargetPos());
			cameraCurrentFOV = 114.59156f * Mathf.Atan2(zoomwidth * 0.5f, x);
		}
		else
		{
			if (Input.GetKey(KeyCode.Equals))
			{
				cameraFOV += Time.deltaTime * 10f;
			}
			if (Input.GetKey(KeyCode.Minus))
			{
				cameraFOV -= Time.deltaTime * 10f;
			}
		}
		cameraFOV = Mathf.Clamp(cameraFOV, MinFOV, MaxFOV);
	}
}
