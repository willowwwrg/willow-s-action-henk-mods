using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerWaypointManager : MonoBehaviour
{
	private RaycastCollider playerCollider;

	public GameSpline splineToFollow;

	public int startingOffset = -1;

	public bool looping;

	private int wayPointNumber;

	[NonSerialized]
	public float offsetFromCurve = 1f;

	private Checkpoint MostRecentCheckpoint;

	private List<float> CheckpointTimes = new List<float>();

	private LMPCheckpoint firstLMPCheckpoint;

	private LMPCheckpoint currentLMPCheckpoint;

	private float positionAlongTrack;

	public void Awake()
	{
		playerCollider = GetComponent<RaycastCollider>();
		wayPointNumber = startingOffset;
		if (splineToFollow == null)
		{
			GameObject gameObject = GameObject.Find("LevelCurve");
			if ((bool)gameObject)
			{
				splineToFollow = gameObject.GetComponent<GameSpline>();
			}
			else
			{
				base.enabled = false;
			}
		}
		LMPCheckpoint[] array = UnityEngine.Object.FindObjectsOfType<LMPCheckpoint>();
		foreach (LMPCheckpoint lMPCheckpoint in array)
		{
			if (lMPCheckpoint.firstCheckpoint)
			{
				firstLMPCheckpoint = lMPCheckpoint;
				currentLMPCheckpoint = lMPCheckpoint;
				break;
			}
		}
	}

	public void ResetCheckpointData()
	{
		CheckpointTimes.Clear();
		MostRecentCheckpoint = Singleton<CheckpointManager>.SP.Startline;
	}

	public void Update()
	{
		if (!Application.isPlaying && wayPointNumber >= 0 && startingOffset >= 0 && !(splineToFollow == null))
		{
			Vector3 point3D = splineToFollow.GetPoint3D(startingOffset);
			Vector3 normal3D = splineToFollow.GetNormal3D(startingOffset);
			Vector3 vector = new Vector3(point3D.x, base.transform.position.y, point3D.z) + normal3D * offsetFromCurve;
			if (base.transform.position != vector)
			{
				base.transform.position = vector;
			}
		}
	}

	public void OnReset()
	{
		if ((bool)firstLMPCheckpoint)
		{
			currentLMPCheckpoint = firstLMPCheckpoint;
			positionAlongTrack = 0f;
		}
		wayPointNumber = startingOffset;
		ResetCheckpointData();
	}

	public void CopyLMPData(GameObject otherPlayer)
	{
		positionAlongTrack = otherPlayer.GetComponent<PlayerWaypointManager>().GetPositionAlongTrack();
		currentLMPCheckpoint = otherPlayer.GetComponent<PlayerWaypointManager>().GetCurrentLMPCheckpoint();
	}

	public void CopyLMPData(Checkpoint otherCheckpoint)
	{
		positionAlongTrack = otherCheckpoint.positionAlongTrackWhenPassed;
		currentLMPCheckpoint = otherCheckpoint.LMPCheckpointWhenPassed;
	}

	public void SetOffset(float offset, bool alsoSetPos = false, bool safe = false)
	{
		SetOffset(new Vector2(offset, base.transform.position.y), alsoSetPos, safe);
	}

	public void SetOffset(Vector2 offset, bool alsoSetPos = false, bool safe = false)
	{
		if (splineToFollow == null)
		{
			return;
		}
		float f = offset.x / splineToFollow.segmentDistance;
		wayPointNumber = Mathf.FloorToInt(f);
		if (alsoSetPos)
		{
			Vector3 point3D = splineToFollow.GetPoint3D(wayPointNumber);
			Vector3 edge3D = splineToFollow.GetEdge3D(wayPointNumber);
			Vector3 normal3D = splineToFollow.GetNormal3D(wayPointNumber);
			float num = offset.x - Mathf.Floor(offset.x);
			Vector3 vector = point3D + edge3D * num + normal3D * offsetFromCurve;
			Vector3 vector2 = new Vector3(vector.x, offset.y, vector.z);
			if (!safe)
			{
				base.transform.position = vector2;
			}
			else
			{
				playerCollider.Move(vector2 - base.transform.position);
			}
			RotateToWaypoint(snap: true);
		}
	}

	public float GetOffset()
	{
		if (splineToFollow == null)
		{
			return wayPointNumber;
		}
		Vector3 point3D = splineToFollow.GetPoint3D(wayPointNumber);
		Vector3 edge3D = splineToFollow.GetEdge3D(wayPointNumber);
		float num = Vector3.Dot(base.transform.position - point3D, edge3D);
		return (float)wayPointNumber + num;
	}

	public Vector2 GetOffset2D()
	{
		return new Vector2(GetOffset(), base.transform.position.y);
	}

	public int GetWaypoint()
	{
		return wayPointNumber;
	}

	public Vector3 GetSideVectorSmooth()
	{
		if (splineToFollow == null)
		{
			return base.transform.forward;
		}
		float offset = GetOffset();
		float t = offset - Mathf.Floor(offset);
		Vector3 vector = -splineToFollow.GetNormal3D(Mathf.FloorToInt(offset));
		Vector3 to = -splineToFollow.GetNormal3D(Mathf.CeilToInt(offset));
		return Vector3.Lerp(vector, to, t);
	}

	public void FixedUpdate()
	{
		if (splineToFollow == null)
		{
			splineToFollow = UnityEngine.Object.FindObjectOfType<GameSpline>();
		}
		if (splineToFollow == null)
		{
			return;
		}
		bool flag = false;
		int num = 0;
		while (!flag && num < 100)
		{
			Vector3 point3D = splineToFollow.GetPoint3D(wayPointNumber);
			Vector3 edge3D = splineToFollow.GetEdge3D(wayPointNumber);
			Vector3 normal3D = splineToFollow.GetNormal3D(wayPointNumber);
			float magnitude = edge3D.magnitude;
			Vector3 vector = edge3D / magnitude;
			Vector3 vector2 = base.transform.position - offsetFromCurve * normal3D;
			vector2.y = 0f;
			Vector3 lhs = vector2 - point3D;
			float num2 = Vector3.Dot(lhs, vector);
			float magnitude2 = lhs.magnitude;
			Vector3 vector3 = point3D + vector * magnitude2 * Mathf.Sign(num2);
			base.transform.position = new Vector3(vector3.x, base.transform.position.y, vector3.z);
			if (num2 < 0f)
			{
				if (wayPointNumber > 0)
				{
					wayPointNumber--;
				}
				else if (looping)
				{
					wayPointNumber = splineToFollow.distanceResampledPolyline.pointCount;
				}
				else
				{
					flag = true;
				}
			}
			else if (magnitude2 > magnitude)
			{
				if (wayPointNumber < splineToFollow.distanceResampledPolyline.pointCount)
				{
					wayPointNumber++;
				}
				else if (looping)
				{
					wayPointNumber = 0;
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			Vector3 normal3D2 = splineToFollow.GetNormal3D(wayPointNumber);
			base.transform.position += normal3D2 * offsetFromCurve;
			num++;
		}
		if (num == 100 && !Application.isEditor)
		{
			Debug.LogWarning("player waypoint while loop had over 100 tries, should not happen! level= " + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelName);
		}
		RotateToWaypoint(snap: true);
		Vector3 velocity = playerCollider.velocity;
		float num3 = Vector3.Dot(playerCollider.velocity, base.transform.forward);
		Vector3 velocity2 = playerCollider.velocity - num3 * base.transform.forward;
		playerCollider.velocity = velocity2;
		if (velocity2.magnitude != 0f && num3 != 0f)
		{
			float num4 = velocity.magnitude / velocity2.magnitude;
			playerCollider.velocity *= num4;
		}
		if (!currentLMPCheckpoint)
		{
			positionAlongTrack = GetOffset();
		}
		else if ((bool)currentLMPCheckpoint.next)
		{
			int num5 = Mathf.FloorToInt(positionAlongTrack);
			Vector2 splineOffset2D = currentLMPCheckpoint.GetComponent<SplineFollow>().GetSplineOffset2D();
			Vector2 splineOffset2D2 = currentLMPCheckpoint.next.GetComponent<SplineFollow>().GetSplineOffset2D();
			Vector2 offset2D = GetOffset2D();
			Vector2 vector4 = splineOffset2D2 - splineOffset2D;
			float magnitude3 = vector4.magnitude;
			Vector2 lhs2 = vector4 / magnitude3;
			Vector2 rhs = offset2D - splineOffset2D;
			float num6 = Vector2.Dot(lhs2, rhs) / magnitude3;
			if (num6 > 1f)
			{
				positionAlongTrack = (float)num5 + 1f;
				currentLMPCheckpoint = currentLMPCheckpoint.next;
			}
			else if (num6 > 0f)
			{
				positionAlongTrack = (float)num5 + num6;
			}
			else
			{
				positionAlongTrack = num5;
			}
		}
	}

	public void RotateToWaypoint(bool snap)
	{
		if (!(splineToFollow == null))
		{
			Vector3 edge3D = splineToFollow.GetEdge3D(wayPointNumber);
			float num = Vector3.Angle(Vector3.right, edge3D);
			if (edge3D.z > 0f)
			{
				num = 0f - num;
			}
			if (!snap)
			{
				base.transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(base.transform.localEulerAngles.y, num, 6f * Time.fixedDeltaTime), 0f);
			}
			else
			{
				base.transform.localEulerAngles = new Vector3(0f, num, 0f);
			}
		}
	}

	public Vector3 GetDirection()
	{
		if (splineToFollow == null)
		{
			return Vector3.forward;
		}
		return splineToFollow.GetEdge3D(wayPointNumber);
	}

	public List<float> GetCheckpointTimes()
	{
		return CheckpointTimes;
	}

	public Checkpoint GetMostRecentCheckpoint()
	{
		return MostRecentCheckpoint;
	}

	public void SetMostRecentCheckpoint(Checkpoint checkpoint, float timeOfPassing)
	{
		CheckpointTimes.Add(timeOfPassing);
		MostRecentCheckpoint = checkpoint;
	}

	public float GetPositionAlongTrack()
	{
		return positionAlongTrack;
	}

	public LMPCheckpoint GetCurrentLMPCheckpoint()
	{
		return currentLMPCheckpoint;
	}
}
