using UnityEngine;

[ExecuteInEditMode]
public class CameraPathPoint : MonoBehaviour
{
	public enum PositionModes
	{
		Free,
		FixedToPoint,
		FixedToPercent
	}

	public PositionModes positionModes;

	public string givenName = string.Empty;

	public string customName = string.Empty;

	public string fullName = string.Empty;

	[SerializeField]
	protected float _percent;

	[SerializeField]
	protected float _animationPercentage;

	public CameraPathControlPoint point;

	public int index;

	public CameraPathControlPoint cpointA;

	public CameraPathControlPoint cpointB;

	public float curvePercentage;

	public Vector3 worldPosition;

	public bool lockPoint;

	public float percent
	{
		get
		{
			return positionModes switch
			{
				PositionModes.Free => _percent, 
				PositionModes.FixedToPercent => _percent, 
				PositionModes.FixedToPoint => point.percentage, 
				_ => _percent, 
			};
		}
		set
		{
			_percent = value;
		}
	}

	public float rawPercent => _percent;

	public float animationPercentage
	{
		get
		{
			return positionModes switch
			{
				PositionModes.Free => _animationPercentage, 
				PositionModes.FixedToPercent => _animationPercentage, 
				PositionModes.FixedToPoint => point.normalisedPercentage, 
				_ => _percent, 
			};
		}
		set
		{
			_animationPercentage = value;
		}
	}

	public string displayName
	{
		get
		{
			if (customName != string.Empty)
			{
				return customName;
			}
			return givenName;
		}
	}

	private void OnEnable()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}
}
