using UnityEngine;

[ExecuteInEditMode]
public class CameraPathOrientationList : CameraPathPointList
{
	public enum Interpolation
	{
		None,
		Linear,
		SmoothStep,
		Hermite,
		Cubic
	}

	public Interpolation interpolation = Interpolation.Cubic;

	public new CameraPathOrientation this[int index] => (CameraPathOrientation)base[index];

	private void OnEnable()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	public override void Init(CameraPath _cameraPath)
	{
		if (!initialised)
		{
			pointTypeName = "Orientation";
			base.Init(_cameraPath);
			cameraPath.PathPointAddedEvent += AddOrientation;
			initialised = true;
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		cameraPath.PathPointAddedEvent -= AddOrientation;
		initialised = false;
	}

	public void AddOrientation(CameraPathControlPoint atPoint)
	{
		CameraPathOrientation cameraPathOrientation = base.gameObject.AddComponent<CameraPathOrientation>();
		if (atPoint.forwardControlPoint != Vector3.zero)
		{
			cameraPathOrientation.rotation = Quaternion.LookRotation(atPoint.forwardControlPoint);
		}
		else
		{
			cameraPathOrientation.rotation = Quaternion.LookRotation(cameraPath.GetPathDirection(atPoint.percentage));
		}
		cameraPathOrientation.hideFlags = HideFlags.HideInInspector;
		AddPoint(cameraPathOrientation, atPoint);
		RecalculatePoints();
	}

	public CameraPathOrientation AddOrientation(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, Quaternion rotation)
	{
		CameraPathOrientation cameraPathOrientation = base.gameObject.AddComponent<CameraPathOrientation>();
		cameraPathOrientation.rotation = rotation;
		cameraPathOrientation.hideFlags = HideFlags.HideInInspector;
		AddPoint(cameraPathOrientation, curvePointA, curvePointB, curvePercetage);
		RecalculatePoints();
		return cameraPathOrientation;
	}

	public void RemovePoint(CameraPathOrientation orientation)
	{
		RemovePoint((CameraPathPoint)orientation);
		RecalculatePoints();
	}

	public Quaternion GetOrientation(float percentage)
	{
		if (base.realNumberOfPoints < 2)
		{
			if (base.realNumberOfPoints == 1)
			{
				return this[0].rotation;
			}
			return Quaternion.identity;
		}
		if (float.IsNaN(percentage))
		{
			percentage = 0f;
		}
		percentage = Mathf.Clamp(percentage, 0f, 1f);
		Quaternion identity = Quaternion.identity;
		identity = interpolation switch
		{
			Interpolation.Cubic => CubicInterpolation(percentage), 
			Interpolation.Hermite => CubicInterpolation(percentage), 
			Interpolation.SmoothStep => SmootStepInterpolation(percentage), 
			Interpolation.Linear => LinearInterpolation(percentage), 
			Interpolation.None => ((CameraPathOrientation)GetPoint(GetNextPointIndex(percentage))).rotation, 
			_ => Quaternion.LookRotation(Vector3.forward), 
		};
		if (float.IsNaN(identity.x))
		{
			return Quaternion.identity;
		}
		return identity;
	}

	private Quaternion LinearInterpolation(float percentage)
	{
		int lastPointIndex = GetLastPointIndex(percentage);
		CameraPathOrientation obj = (CameraPathOrientation)GetPoint(lastPointIndex);
		CameraPathOrientation cameraPathOrientation = (CameraPathOrientation)GetPoint(lastPointIndex + 1);
		float percent = obj.percent;
		float num = cameraPathOrientation.percent;
		if (percent > num)
		{
			num += 1f;
		}
		float num2 = num - percent;
		return Quaternion.Lerp(t: (percentage - percent) / num2, from: obj.rotation, to: cameraPathOrientation.rotation);
	}

	private Quaternion SmootStepInterpolation(float percentage)
	{
		int lastPointIndex = GetLastPointIndex(percentage);
		CameraPathOrientation obj = (CameraPathOrientation)GetPoint(lastPointIndex);
		CameraPathOrientation cameraPathOrientation = (CameraPathOrientation)GetPoint(lastPointIndex + 1);
		float percent = obj.percent;
		float num = cameraPathOrientation.percent;
		if (percent > num)
		{
			num += 1f;
		}
		float num2 = num - percent;
		return Quaternion.Lerp(t: CPMath.SmoothStep((percentage - percent) / num2), from: obj.rotation, to: cameraPathOrientation.rotation);
	}

	private Quaternion CubicInterpolation(float percentage)
	{
		int lastPointIndex = GetLastPointIndex(percentage);
		CameraPathOrientation cameraPathOrientation = (CameraPathOrientation)GetPoint(lastPointIndex);
		CameraPathOrientation cameraPathOrientation2 = (CameraPathOrientation)GetPoint(lastPointIndex + 1);
		CameraPathOrientation cameraPathOrientation3 = (CameraPathOrientation)GetPoint(lastPointIndex - 1);
		CameraPathOrientation cameraPathOrientation4 = (CameraPathOrientation)GetPoint(lastPointIndex + 2);
		float percent = cameraPathOrientation.percent;
		float num = cameraPathOrientation2.percent;
		if (percent > num)
		{
			num += 1f;
		}
		float num2 = num - percent;
		float t = (percentage - percent) / num2;
		Quaternion result = CPMath.CalculateCubic(cameraPathOrientation.rotation, cameraPathOrientation3.rotation, cameraPathOrientation4.rotation, cameraPathOrientation2.rotation, t);
		if (float.IsNaN(result.x))
		{
			Debug.Log(percentage + " " + cameraPathOrientation.fullName + " " + cameraPathOrientation2.fullName + " " + cameraPathOrientation3.fullName + " " + cameraPathOrientation4.fullName);
		}
		return result;
	}

	protected override void RecalculatePoints()
	{
		base.RecalculatePoints();
		for (int i = 0; i < base.realNumberOfPoints; i++)
		{
			CameraPathOrientation cameraPathOrientation = this[i];
			if (cameraPathOrientation.lookAt != null)
			{
				cameraPathOrientation.rotation = Quaternion.LookRotation(cameraPathOrientation.lookAt.transform.position - cameraPathOrientation.worldPosition);
			}
		}
	}
}
