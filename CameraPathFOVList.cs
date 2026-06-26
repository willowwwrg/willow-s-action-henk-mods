using UnityEngine;

[ExecuteInEditMode]
public class CameraPathFOVList : CameraPathPointList
{
	public enum ProjectionType
	{
		FOV,
		Orthographic
	}

	public enum Interpolation
	{
		None,
		Linear,
		SmoothStep
	}

	private const float DEFAULT_FOV = 60f;

	private const float DEFAULT_SIZE = 5f;

	public Interpolation interpolation = Interpolation.SmoothStep;

	public bool listEnabled;

	public new CameraPathFOV this[int index] => (CameraPathFOV)base[index];

	private float defaultFOV
	{
		get
		{
			if ((bool)Camera.current)
			{
				return Camera.current.fieldOfView;
			}
			Camera[] allCameras = Camera.allCameras;
			if (allCameras.Length != 0)
			{
				return allCameras[0].fieldOfView;
			}
			return 60f;
		}
	}

	private float defaultSize
	{
		get
		{
			if ((bool)Camera.current)
			{
				return Camera.current.orthographicSize;
			}
			Camera[] allCameras = Camera.allCameras;
			if (allCameras.Length != 0)
			{
				return allCameras[0].orthographicSize;
			}
			return 5f;
		}
	}

	private void OnEnable()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	public override void Init(CameraPath _cameraPath)
	{
		if (!initialised)
		{
			base.Init(_cameraPath);
			cameraPath.PathPointAddedEvent += AddFOV;
			pointTypeName = "FOV";
			initialised = true;
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		cameraPath.PathPointAddedEvent -= AddFOV;
		initialised = false;
	}

	public void AddFOV(CameraPathControlPoint atPoint)
	{
		CameraPathFOV cameraPathFOV = base.gameObject.AddComponent<CameraPathFOV>();
		cameraPathFOV.FOV = defaultFOV;
		cameraPathFOV.Size = defaultSize;
		cameraPathFOV.hideFlags = HideFlags.HideInInspector;
		AddPoint(cameraPathFOV, atPoint);
		RecalculatePoints();
	}

	public CameraPathFOV AddFOV(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, float fov, float size)
	{
		CameraPathFOV cameraPathFOV = base.gameObject.AddComponent<CameraPathFOV>();
		cameraPathFOV.hideFlags = HideFlags.HideInInspector;
		cameraPathFOV.FOV = fov;
		cameraPathFOV.Size = size;
		cameraPathFOV.Size = defaultSize;
		AddPoint(cameraPathFOV, curvePointA, curvePointB, curvePercetage);
		RecalculatePoints();
		return cameraPathFOV;
	}

	public float GetValue(float percentage, ProjectionType type)
	{
		if (base.realNumberOfPoints < 2)
		{
			if (type == ProjectionType.FOV)
			{
				if (base.realNumberOfPoints == 1)
				{
					return this[0].FOV;
				}
				return defaultFOV;
			}
			if (base.realNumberOfPoints == 1)
			{
				return this[0].Size;
			}
			return defaultSize;
		}
		percentage = Mathf.Clamp(percentage, 0f, 1f);
		return interpolation switch
		{
			Interpolation.SmoothStep => SmoothStepInterpolation(percentage, type), 
			Interpolation.Linear => LinearInterpolation(percentage, type), 
			_ => LinearInterpolation(percentage, type), 
		};
	}

	private float LinearInterpolation(float percentage, ProjectionType projectionType)
	{
		int lastPointIndex = GetLastPointIndex(percentage);
		CameraPathFOV cameraPathFOV = (CameraPathFOV)GetPoint(lastPointIndex);
		CameraPathFOV cameraPathFOV2 = (CameraPathFOV)GetPoint(lastPointIndex + 1);
		float percent = cameraPathFOV.percent;
		float num = cameraPathFOV2.percent;
		if (percent > num)
		{
			num += 1f;
		}
		float num2 = num - percent;
		float t = (percentage - percent) / num2;
		float num3 = ((projectionType != ProjectionType.FOV) ? cameraPathFOV.Size : cameraPathFOV.FOV);
		float to = ((projectionType != ProjectionType.FOV) ? cameraPathFOV2.Size : cameraPathFOV2.FOV);
		return Mathf.Lerp(num3, to, t);
	}

	private float SmoothStepInterpolation(float percentage, ProjectionType projectionType)
	{
		int lastPointIndex = GetLastPointIndex(percentage);
		CameraPathFOV cameraPathFOV = (CameraPathFOV)GetPoint(lastPointIndex);
		CameraPathFOV cameraPathFOV2 = (CameraPathFOV)GetPoint(lastPointIndex + 1);
		float percent = cameraPathFOV.percent;
		float num = cameraPathFOV2.percent;
		if (percent > num)
		{
			num += 1f;
		}
		float num2 = num - percent;
		float val = (percentage - percent) / num2;
		float num3 = ((projectionType != ProjectionType.FOV) ? cameraPathFOV.Size : cameraPathFOV.FOV);
		float to = ((projectionType != ProjectionType.FOV) ? cameraPathFOV2.Size : cameraPathFOV2.FOV);
		return Mathf.Lerp(num3, to, CPMath.SmoothStep(val));
	}
}
