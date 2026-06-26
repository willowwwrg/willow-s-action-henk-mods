using UnityEngine;

[ExecuteInEditMode]
public class CameraPathBezierControlPoint : MonoBehaviour
{
	public enum animationEase
	{
		flat,
		easein,
		easeout,
		easeinout
	}

	public enum DELAY_MODES
	{
		none,
		timed,
		indefinite
	}

	public Vector3 controlPoint = Vector3.zero;

	public CameraPathBezier bezier;

	[SerializeField]
	private animationEase _ease;

	public AnimationCurve _curve;

	public float FOV = 60f;

	public DELAY_MODES delayMode;

	public float delayTime;

	private float directionLineLength = 1f;

	private float focusBoxLength = 0.25f;

	[SerializeField]
	private float _tilt;

	public bool useLongestRotation;

	[SerializeField]
	public Vector3 storedPosition = Vector3.zero;

	public Vector3 worldControlPoint
	{
		get
		{
			return bezier.transform.rotation * controlPoint + base.transform.position;
		}
		set
		{
			Vector3 vector = value - base.transform.position;
			vector = Quaternion.Inverse(bezier.transform.rotation) * vector;
			controlPoint = vector;
		}
	}

	public Vector3 reverseWorldControlPoint
	{
		get
		{
			return bezier.transform.rotation * -controlPoint + base.transform.position;
		}
		set
		{
			Vector3 vector = -value - base.transform.position;
			vector = Quaternion.Inverse(bezier.transform.rotation) * vector;
			controlPoint = vector;
		}
	}

	public bool isLastPoint
	{
		get
		{
			if (bezier == null || bezier.controlPoints == null)
			{
				return true;
			}
			return this == bezier.controlPoints[bezier.numberOfControlPoints - 1];
		}
	}

	public AnimationCurve curve => _curve;

	public animationEase ease
	{
		get
		{
			return _ease;
		}
		set
		{
			_ease = value;
			SetAnimationCurve();
		}
	}

	public float tilt
	{
		get
		{
			return _tilt;
		}
		set
		{
			_tilt = value;
			if (_tilt < -180f)
			{
				_tilt += 360f;
			}
			if (_tilt > 180f)
			{
				_tilt += -360f;
			}
		}
	}

	public Vector3 GetPathDirection()
	{
		float pathPercentageAtPoint = bezier.GetPathPercentageAtPoint(this);
		float t = Mathf.Clamp01(pathPercentageAtPoint - 0.05f);
		float t2 = Mathf.Clamp01(pathPercentageAtPoint + 0.05f);
		Vector3 pathPosition = bezier.GetPathPosition(t);
		return bezier.GetPathPosition(t2) - pathPosition;
	}

	public void SetRotationToCurve()
	{
		base.transform.LookAt(base.transform.position + GetPathDirection());
	}

	private void OnEnable()
	{
		SetAnimationCurve();
	}

	private void Update()
	{
		CheckPosition();
	}

	private void OnDestroy()
	{
		if (bezier != null)
		{
			bezier.DeletePoint(this, destroy: false);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "pathpoint");
		switch (bezier.mode)
		{
		case CameraPathBezier.viewmodes.usercontrolled:
		{
			Vector3 vector = base.transform.TransformDirection(new Vector3(0f - focusBoxLength, 0f - focusBoxLength, 1f) * directionLineLength) + base.transform.position;
			Vector3 vector2 = base.transform.TransformDirection(new Vector3(focusBoxLength, 0f - focusBoxLength, 1f) * directionLineLength) + base.transform.position;
			Vector3 vector3 = base.transform.TransformDirection(new Vector3(0f - focusBoxLength, focusBoxLength, 1f) * directionLineLength) + base.transform.position;
			Vector3 vector4 = base.transform.TransformDirection(new Vector3(focusBoxLength, focusBoxLength, 1f) * directionLineLength) + base.transform.position;
			Gizmos.color = new Color(0f, 0f, 1f, 0.6f);
			Gizmos.DrawLine(base.transform.position, vector);
			Gizmos.DrawLine(base.transform.position, vector2);
			Gizmos.DrawLine(base.transform.position, vector3);
			Gizmos.DrawLine(base.transform.position, vector4);
			Gizmos.color = new Color(0f, 0f, 1f, 0.4f);
			Gizmos.DrawLine(vector3, vector);
			Gizmos.DrawLine(vector, vector2);
			Gizmos.DrawLine(vector2, vector4);
			Gizmos.DrawLine(vector4, vector3);
			break;
		}
		case CameraPathBezier.viewmodes.target:
		{
			Transform target = bezier.target;
			if (target != null)
			{
				Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
				Gizmos.DrawRay(base.transform.position, (target.position - base.transform.position) * 0.9f);
			}
			break;
		}
		case CameraPathBezier.viewmodes.mouselook:
		case CameraPathBezier.viewmodes.followpath:
		case CameraPathBezier.viewmodes.reverseFollowpath:
			break;
		}
	}

	private void SetAnimationCurve()
	{
		switch (_ease)
		{
		case animationEase.flat:
			_curve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
			break;
		case animationEase.easein:
			_curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 1f, 1f));
			break;
		case animationEase.easeout:
			_curve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 0f, 0f));
			break;
		case animationEase.easeinout:
			_curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));
			break;
		}
	}

	public void CheckPosition()
	{
		if (base.transform.position != storedPosition)
		{
			bezier.RecalculateStoredValues();
			storedPosition = base.transform.position;
		}
	}
}
