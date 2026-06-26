using UnityEngine;

[ExecuteInEditMode]
public class GameSpline : MonoBehaviour
{
	private int simplePolyLineSegments = 100;

	[HideInInspector]
	public float segmentDistance = 1f;

	private Spline spline;

	public PolyLine distanceResampledPolyline;

	public bool catmullRom;

	protected void OnEnable()
	{
		if (base.transform.childCount != 0)
		{
			RebuildSpline();
		}
	}

	public void RebuildSpline()
	{
		Vector2[] array = new Vector2[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			child.name = "CV" + i.ToString("D2");
			ref Vector2 reference = ref array[i];
			reference = child.localPosition;
		}
		spline = new Spline(array);
		spline.catmullRom = catmullRom;
		distanceResampledPolyline = spline.Resampled(segmentDistance, simplePolyLineSegments);
		Object[] array2 = Object.FindObjectsOfType(typeof(SplineObject));
		for (int j = 0; j < array2.Length; j++)
		{
			((SplineObject)array2[j]).prevSplineOffset.x = -1000f;
		}
	}

	protected void Update()
	{
		if (base.transform.localEulerAngles != new Vector3(90f, 0f, 0f))
		{
			base.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
		}
	}

	protected void OnDrawGizmos()
	{
		spline.DrawAsGizmo(base.transform.localToWorldMatrix);
		Gizmos.color = Color.green;
		distanceResampledPolyline.DrawAsGizmo(base.transform.localToWorldMatrix);
		Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (i % 2 == 1)
			{
				Gizmos.DrawLine(base.transform.GetChild(i).position, base.transform.GetChild(i - 1).position);
			}
		}
	}

	public Vector3 DeformVertex(Vector3 baseVertex, Vector3 splineOffset)
	{
		float num = baseVertex.x + splineOffset.x;
		int num2 = Mathf.FloorToInt(num / segmentDistance);
		Vector3 point3D = GetPoint3D(num2);
		Vector3 point3D2 = GetPoint3D(num2 + 1);
		float t = num / segmentDistance - (float)num2;
		Vector3 vector = Vector3.Lerp(point3D, point3D2, t);
		Vector3 normal3D = GetNormal3D(num2);
		return vector - (baseVertex.z + splineOffset.z) * normal3D + (baseVertex.y + splineOffset.y) * Vector3.up;
	}

	public Vector3 DeformDirection(Vector3 direction, float curveOffset)
	{
		int edgeId = Mathf.FloorToInt(curveOffset / segmentDistance);
		Vector3 edge3D = GetEdge3D(edgeId);
		return Quaternion.FromToRotation(base.transform.right, edge3D) * direction;
	}

	public Vector3 GetPoint3D(int pointId)
	{
		Vector2 vector = distanceResampledPolyline.Point(pointId);
		return new Vector3(vector.x, 0f, vector.y);
	}

	public Vector3 GetEdge3D(int edgeId)
	{
		Vector2 vector = distanceResampledPolyline.Edge(edgeId);
		return new Vector3(vector.x, 0f, vector.y);
	}

	public Vector3 GetNormal3D(int normalId)
	{
		Vector2 vector = distanceResampledPolyline.Normal(normalId);
		return new Vector3(vector.x, 0f, vector.y);
	}

	public Vector3 GetNormal3DSmooth(float normalOffset)
	{
		float t = normalOffset - Mathf.Floor(normalOffset);
		Vector3 normal3D = GetNormal3D(Mathf.FloorToInt(normalOffset));
		Vector3 normal3D2 = GetNormal3D(Mathf.CeilToInt(normalOffset));
		return Vector3.Lerp(normal3D, normal3D2, t);
	}
}
