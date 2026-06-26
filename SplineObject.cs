using System;
using UnityEngine;

[ExecuteInEditMode]
public class SplineObject : MonoBehaviour
{
	public GameSpline spline;

	public Vector3 splineOffset;

	[HideInInspector]
	public bool firstFrame;

	[HideInInspector]
	public bool createdAtRuntime;

	private bool secondFrame;

	[HideInInspector]
	public Vector3 handlePosition;

	[HideInInspector]
	public Vector3 handleDirection = Vector3.right;

	[HideInInspector]
	public Vector3 handleNormal = Vector3.forward;

	private Vector3 prevHandlePosition;

	[NonSerialized]
	public Vector3 prevSplineOffset;

	private void Start()
	{
		firstFrame = true;
	}

	public virtual void Update()
	{
		FindSpline();
		if (!Application.isPlaying && !(spline == null))
		{
			ApplyHandleOffset();
			SnapCoordinates();
			RecalcHandlePosition();
			ClearFirstFrame();
			ClearTransform();
			prevSplineOffset = splineOffset;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(handlePosition, 0.1f);
	}

	public void FindSpline()
	{
		if (!Application.isPlaying && spline == null)
		{
			MonoBehaviour.print("Attaching a curve to " + base.gameObject.name);
			spline = (GameSpline)UnityEngine.Object.FindObjectOfType(typeof(GameSpline));
		}
	}

	public void ClearTransform()
	{
		if (base.transform.localPosition != Vector3.zero)
		{
			base.transform.localPosition = Vector3.zero;
		}
		if (base.transform.localEulerAngles != Vector3.zero)
		{
			base.transform.localEulerAngles = Vector3.zero;
		}
		if (base.transform.localScale != Vector3.one)
		{
			base.transform.localScale = Vector3.one;
		}
	}

	public void ClearFirstFrame()
	{
		if (firstFrame)
		{
			firstFrame = false;
		}
	}

	public void SnapCoordinates()
	{
		splineOffset.x = Mathf.Round(splineOffset.x);
		splineOffset.y = Mathf.Round(splineOffset.y);
		splineOffset.z = Mathf.Round(splineOffset.z);
		splineOffset.x = Mathf.Max(splineOffset.x, 0f);
	}

	public void ClampCoordinates()
	{
		if (Application.isPlaying && Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
		{
			float min = 0f;
			float max = spline.distanceResampledPolyline.edgeCount;
			float min2 = 0f;
			if (HenkUtils.IsOutside())
			{
				min2 = 30f;
			}
			float max2 = 131f;
			if (HenkUtils.IsOutside())
			{
				max2 = 400f;
			}
			splineOffset.x = Mathf.Clamp(splineOffset.x, min, max);
			splineOffset.y = Mathf.Clamp(splineOffset.y, min2, max2);
		}
	}

	public void ApplyHandleOffset()
	{
		if (!firstFrame && splineOffset == prevSplineOffset)
		{
			Vector3 zero = Vector3.zero;
			zero.x = Vector3.Dot(handlePosition - prevHandlePosition, handleDirection);
			zero.y = handlePosition.y - prevHandlePosition.y;
			zero.z = Vector3.Dot(handlePosition - prevHandlePosition, handleNormal);
			if (Mathf.Abs(zero.x) > 1E-05f)
			{
				zero.z = 0f;
			}
			splineOffset += zero;
		}
	}

	public void RecalcHandlePosition()
	{
		handlePosition = spline.DeformVertex(Vector3.zero, splineOffset);
		handleDirection = spline.DeformDirection(Vector3.right, splineOffset.x).normalized;
		handleNormal = spline.DeformDirection(Vector3.forward, splineOffset.x).normalized;
		prevHandlePosition = handlePosition;
	}

	public Vector3 GetRaycastPosition()
	{
		Vector3 vector = spline.DeformVertex(Vector3.zero, splineOffset);
		Vector3 normalized = spline.DeformDirection(Vector3.right, splineOffset.x).normalized;
		Vector3 normalized2 = spline.DeformDirection(Vector3.forward, splineOffset.x).normalized;
		return vector + normalized2 * -0.5f + normalized * -0.5f;
	}

	public Vector2 GetSplineOffset2D()
	{
		return new Vector2(splineOffset.x, splineOffset.y);
	}
}
