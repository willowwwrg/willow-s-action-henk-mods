using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathBezier : MonoBehaviour
{
	public enum viewmodes
	{
		usercontrolled,
		target,
		mouselook,
		followpath,
		reverseFollowpath
	}

	public viewmodes mode;

	public Transform target;

	public Color lineColour = Color.white;

	public bool loop;

	[SerializeField]
	private CameraPathBezierControlPoint[] _controlPoints = new CameraPathBezierControlPoint[0];

	[SerializeField]
	private float _storedTotalArcLength;

	[SerializeField]
	private float[] _storedArcLengths;

	[SerializeField]
	private float[] _storedArcLengthsFull;

	[SerializeField]
	private int storedArcLengthArraySize = 750;

	private Quaternion lastRotation = Quaternion.identity;

	private int overRotate;

	public int numberOfControlPoints
	{
		get
		{
			if (controlPoints != null)
			{
				return controlPoints.Length;
			}
			return 0;
		}
	}

	public int numberOfCurves
	{
		get
		{
			if (!loop)
			{
				return _controlPoints.Length - 1;
			}
			return _controlPoints.Length;
		}
	}

	public CameraPathBezierControlPoint[] controlPoints => _controlPoints;

	public float storedTotalArcLength => _storedTotalArcLength;

	public float[] storedArcLengths => _storedArcLengths;

	private int numberOfUseablePoints
	{
		get
		{
			if (!loop)
			{
				return _controlPoints.Length - 1;
			}
			return _controlPoints.Length;
		}
	}

	public void RecalculateStoredValues()
	{
		if (_controlPoints.Length >= 2)
		{
			float num = ((numberOfCurves >= 1) ? (1f / (float)numberOfCurves) : 1f);
			_storedArcLengths = new float[numberOfCurves];
			for (int i = 0; i < numberOfCurves; i++)
			{
				_storedArcLengths[i] = 0f;
			}
			float num2 = 1f / (float)storedArcLengthArraySize;
			float num3 = 0f;
			_storedArcLengthsFull = new float[storedArcLengthArraySize];
			_storedArcLengthsFull[0] = 0f;
			for (int j = 0; j < storedArcLengthArraySize - 1; j++)
			{
				float num4 = num2 * (float)(j + 1);
				float t = num2 * (float)(j + 1) + num2;
				Vector3 pathPosition = GetPathPosition(num4);
				Vector3 pathPosition2 = GetPathPosition(t);
				float num5 = Vector3.Distance(pathPosition, pathPosition2);
				int num6 = Mathf.FloorToInt(num4 / num);
				num3 += num5;
				_storedArcLengths[num6] += num5;
				_storedArcLengthsFull[j + 1] = num3;
			}
			_storedTotalArcLength = num3;
		}
	}

	public void Awake()
	{
	}

	public void ResetPath()
	{
		while (_controlPoints.Length != 0)
		{
			DeletePoint(0, destroy: true);
		}
		RecalculateStoredValues();
	}

	public void DeletePoint(CameraPathBezierControlPoint deletePoint, bool destroy)
	{
		List<CameraPathBezierControlPoint> list = new List<CameraPathBezierControlPoint>(_controlPoints);
		list.Remove(deletePoint);
		_controlPoints = list.ToArray();
		if (destroy)
		{
			UnityEngine.Object.DestroyImmediate(deletePoint.gameObject);
		}
		RecalculateStoredValues();
	}

	public void DeletePoint(int index, bool destroy)
	{
		List<CameraPathBezierControlPoint> list = new List<CameraPathBezierControlPoint>(_controlPoints);
		CameraPathBezierControlPoint cameraPathBezierControlPoint = list[index];
		list.RemoveAt(index);
		_controlPoints = new CameraPathBezierControlPoint[list.Count];
		_controlPoints = list.ToArray();
		if (destroy)
		{
			UnityEngine.Object.DestroyImmediate(cameraPathBezierControlPoint.gameObject);
		}
		RecalculateStoredValues();
	}

	public void AddNewPoint()
	{
		AddNewPoint(_controlPoints.Length);
	}

	public void AddNewPoint(int index)
	{
		GameObject gameObject = new GameObject("Control Point");
		gameObject.transform.parent = base.transform;
		if (numberOfControlPoints == 0)
		{
			gameObject.transform.localPosition = Vector3.zero;
		}
		else if (numberOfControlPoints == 1)
		{
			gameObject.transform.localPosition = _controlPoints[0].transform.rotation * Vector3.forward * 5f;
		}
		else if (index < numberOfControlPoints)
		{
			CameraPathBezierControlPoint obj = _controlPoints[Mathf.Clamp(index - 1, 0, numberOfUseablePoints)];
			CameraPathBezierControlPoint cameraPathBezierControlPoint = _controlPoints[Mathf.Clamp(index, 0, numberOfUseablePoints)];
			Vector3 position = obj.transform.position;
			Vector3 worldControlPoint = obj.worldControlPoint;
			Vector3 worldControlPoint2 = cameraPathBezierControlPoint.worldControlPoint;
			Vector3 position2 = cameraPathBezierControlPoint.transform.position;
			gameObject.transform.position = CalculateBezierPoint(0.5f, position, worldControlPoint, worldControlPoint2, position2);
			Quaternion rotation = _controlPoints[Mathf.Clamp(index - 2, 0, numberOfUseablePoints)].transform.rotation;
			Quaternion rotation2 = _controlPoints[Mathf.Clamp(index - 1, 0, numberOfUseablePoints)].transform.rotation;
			Quaternion rotation3 = _controlPoints[Mathf.Clamp(index, 0, numberOfUseablePoints)].transform.rotation;
			Quaternion rotation4 = _controlPoints[Mathf.Clamp(index + 1, 0, numberOfUseablePoints)].transform.rotation;
			gameObject.transform.rotation = CalculateCubicRotation(rotation, rotation3, rotation4, rotation2, 0.5f);
		}
		else
		{
			Vector3 normalized = (GetPathPosition(1f) - GetPathPosition(0.95f)).normalized;
			gameObject.transform.position = _controlPoints[index - 1].transform.position + normalized * 5f;
			gameObject.transform.rotation = _controlPoints[index - 1].transform.rotation;
		}
		CameraPathBezierControlPoint cameraPathBezierControlPoint2 = gameObject.AddComponent<CameraPathBezierControlPoint>();
		cameraPathBezierControlPoint2.name = "bezier control point " + index;
		cameraPathBezierControlPoint2.bezier = this;
		List<CameraPathBezierControlPoint> list = new List<CameraPathBezierControlPoint>(_controlPoints);
		list.Insert(index, cameraPathBezierControlPoint2);
		_controlPoints = list.ToArray();
		RecalculateStoredValues();
	}

	public float GetNormalisedT(float t)
	{
		if (t == 0f)
		{
			return 0f;
		}
		float num = t * _storedTotalArcLength;
		int num2 = 0;
		int num3 = storedArcLengthArraySize;
		int num4 = 0;
		while (num2 < num3)
		{
			num4 = num2 + (num3 - num2) / 2;
			if (_storedArcLengthsFull[num4] < num)
			{
				num2 = num4 + 1;
			}
			else
			{
				num3 = num4;
			}
		}
		if (_storedArcLengthsFull[num4] > num && num4 > 0)
		{
			num4--;
		}
		float num5 = _storedArcLengthsFull[num4];
		float result = (float)num4 / (float)storedArcLengthArraySize;
		if (num5 == num)
		{
			return result;
		}
		return ((float)num4 + (num - num5) / (_storedArcLengthsFull[num4 + 1] - num5)) / (float)storedArcLengthArraySize;
	}

	public int GetPointNumber(float t)
	{
		float num = 1f / (float)numberOfUseablePoints;
		return Mathf.Clamp(Mathf.FloorToInt(t / num), 0, _controlPoints.Length - 1);
	}

	public Vector3 GetPathPosition(float t)
	{
		float num = 1f / (float)numberOfUseablePoints;
		int num2 = Mathf.FloorToInt(t / num);
		float t2 = Mathf.Clamp01((t - (float)num2 * num) * (float)numberOfUseablePoints);
		CameraPathBezierControlPoint point = GetPoint(num2);
		CameraPathBezierControlPoint point2 = GetPoint(num2 + 1);
		return CalculateBezierPoint(t2, point.transform.position, point.worldControlPoint, point2.reverseWorldControlPoint, point2.transform.position);
	}

	public Quaternion GetPathRotation(float t)
	{
		float num = 1f / (float)numberOfUseablePoints;
		int num2 = Mathf.FloorToInt(t / num);
		float t2 = Mathf.Clamp01((t - (float)num2 * num) * (float)numberOfUseablePoints);
		Quaternion rotation = GetPoint(num2).transform.rotation;
		Quaternion rotation2 = GetPoint(num2 + 1).transform.rotation;
		Quaternion rotation3 = GetPoint(num2 - 1).transform.rotation;
		Quaternion rotation4 = GetPoint(num2 + 2).transform.rotation;
		Quaternion quaternion = CalculateCubicRotation(rotation, rotation3, rotation4, rotation2, t2);
		if (lastRotation != Quaternion.identity)
		{
			if (Quaternion.Angle(quaternion, lastRotation) > 90f && overRotate > 5)
			{
				quaternion = lastRotation;
				overRotate++;
			}
			else
			{
				overRotate = 0;
			}
		}
		lastRotation = quaternion;
		return quaternion;
	}

	public float GetPathFOV(float t)
	{
		float num = 1f / (float)numberOfUseablePoints;
		int num2 = Mathf.FloorToInt(t / num);
		float val = Mathf.Clamp01((t - (float)num2 * num) * (float)numberOfUseablePoints);
		float fOV = GetPoint(num2).FOV;
		float fOV2 = GetPoint(num2 + 1).FOV;
		return Mathf.Lerp(fOV, fOV2, CalculateHermite(val));
	}

	public float GetPathTilt(float t)
	{
		float num = 1f / (float)numberOfUseablePoints;
		int num2 = Mathf.FloorToInt(t / num);
		float val = Mathf.Clamp01((t - (float)num2 * num) * (float)numberOfUseablePoints);
		float tilt = GetPoint(num2).tilt;
		float tilt2 = GetPoint(num2 + 1).tilt;
		return Mathf.Lerp(tilt, tilt2, CalculateHermite(val));
	}

	public float GetPathPercentageAtPoint(CameraPathBezierControlPoint point)
	{
		return (float)new List<CameraPathBezierControlPoint>(_controlPoints).IndexOf(point) / (float)numberOfCurves;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = lineColour;
		if (numberOfUseablePoints >= 1)
		{
			for (float num = 0f; num <= 1f; num += 0.015f)
			{
				Gizmos.DrawLine(GetPathPosition(num), GetPathPosition(num + 0.013f));
			}
		}
	}

	private void OnEnable()
	{
		RecalculateStoredValues();
	}

	private void Start()
	{
		RecalculateStoredValues();
	}

	private CameraPathBezierControlPoint GetPoint(int index)
	{
		if (_controlPoints.Length == 0)
		{
			return null;
		}
		if (!loop)
		{
			return _controlPoints[Mathf.Clamp(index, 0, numberOfUseablePoints)];
		}
		if (index >= numberOfControlPoints)
		{
			index -= numberOfControlPoints;
		}
		if (index < 0)
		{
			index += numberOfControlPoints;
		}
		return _controlPoints[index];
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = t * t;
		float num2 = num * t;
		float num3 = 1f - t;
		float num4 = num3 * num3;
		return num4 * num3 * p0 + 3f * num4 * t * p1 + 3f * num3 * num * p2 + num2 * p3;
	}

	private Quaternion CalculateCubicRotation(Quaternion p, Quaternion a, Quaternion b, Quaternion q, float t)
	{
		Quaternion p2 = SquadTangent(a, p, q);
		Quaternion q2 = SquadTangent(p, q, b);
		float t2 = 2f * t * (1f - t);
		return Slerp(Slerp(p, q, t), Slerp(p2, q2, t), t2);
	}

	private float CalculateHermite(float val)
	{
		return val * val * (3f - 2f * val);
	}

	private Quaternion SquadTangent(Quaternion before, Quaternion center, Quaternion after)
	{
		Quaternion quaternion = lnDif(center, before);
		Quaternion quaternion2 = lnDif(center, after);
		Quaternion identity = Quaternion.identity;
		for (int i = 0; i < 4; i++)
		{
			identity[i] = -0.25f * (quaternion[i] + quaternion2[i]);
		}
		return center * exp(identity);
	}

	private Quaternion lnDif(Quaternion a, Quaternion b)
	{
		Quaternion q = Quaternion.Inverse(a) * b;
		Normalize(q);
		return log(q);
	}

	private Quaternion Normalize(Quaternion q)
	{
		float num = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
		if (num > 0f)
		{
			q.x /= num;
			q.y /= num;
			q.z /= num;
			q.w /= num;
		}
		else
		{
			q.x = 0f;
			q.y = 0f;
			q.z = 0f;
			q.w = 1f;
		}
		return q;
	}

	private Quaternion exp(Quaternion q)
	{
		float num = Mathf.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]);
		if ((double)num < 1E-06)
		{
			return new Quaternion(q[0], q[1], q[2], Mathf.Cos(num));
		}
		float num2 = Mathf.Sin(num) / num;
		return new Quaternion(q[0] * num2, q[1] * num2, q[2] * num2, Mathf.Cos(num));
	}

	private Quaternion log(Quaternion q)
	{
		float num = Mathf.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]);
		if ((double)num < 1E-06)
		{
			return new Quaternion(q[0], q[1], q[2], 0f);
		}
		float num2 = Mathf.Acos(q[3]) / num;
		return new Quaternion(q[0] * num2, q[1] * num2, q[2] * num2, 0f);
	}

	private Quaternion Slerp(Quaternion p, Quaternion q, float t)
	{
		float num = Quaternion.Dot(p, q);
		Quaternion result = default(Quaternion);
		if (1f + num > 1E-05f)
		{
			float num5;
			float num6;
			if (1f - num > 1E-05f)
			{
				float num2 = Mathf.Acos(num);
				float num3 = Mathf.Sin(num2);
				float num4 = Mathf.Sign(num3) * 1f / num3;
				num5 = Mathf.Sin((1f - t) * num2) * num4;
				num6 = Mathf.Sin(t * num2) * num4;
			}
			else
			{
				num5 = 1f - t;
				num6 = t;
			}
			result.x = num5 * p.x + num6 * q.x;
			result.y = num5 * p.y + num6 * q.y;
			result.z = num5 * p.z + num6 * q.z;
			result.w = num5 * p.w + num6 * q.w;
		}
		else
		{
			float num7 = Mathf.Sin((1f - t) * (float)Math.PI * 0.5f);
			float num8 = Mathf.Sin(t * (float)Math.PI * 0.5f);
			result.x = num7 * p.x - num8 * p.y;
			result.y = num7 * p.y + num8 * p.x;
			result.z = num7 * p.z - num8 * p.w;
			result.w = p.z;
		}
		return result;
	}

	private Quaternion Nlerp(Quaternion p, Quaternion q, float t)
	{
		float num = 1f - t;
		Quaternion quaternion = new Quaternion
		{
			x = num * p.x + t * q.x,
			y = num * p.y + t * q.y,
			z = num * p.z + t * q.z,
			w = num * p.w + t * q.w
		};
		Normalize(quaternion);
		return quaternion;
	}

	private Quaternion GetQuatConjugate(Quaternion q)
	{
		return new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, q.w);
	}
}
