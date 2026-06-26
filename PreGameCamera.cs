using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreGameCamera : MonoBehaviour
{
	private GameSpline levelSpline;

	private float cameraFieldOfView = 65f;

	private List<SnapshotFrame> snapshots = new List<SnapshotFrame>();

	private int currentSnapShot = -1;

	private float snapShotTime = -10f;

	private Vector3 rotSpeed = Vector3.zero;

	private Vector3 velocity = Vector3.zero;

	private bool bonusMode;

	private bool challengeMode;

	private int challengeShot;

	private Vector3 averageCoinPosition = Vector3.zero;

	private Vector3 averageCoinDirection = Vector3.zero;

	private Vector3 minCoinPos = Vector3.zero;

	private Vector3 maxCoinPos = Vector3.zero;

	public void Initialize()
	{
		bonusMode = false;
		challengeMode = false;
		levelSpline = UnityEngine.Object.FindObjectOfType<GameSpline>();
		snapshots.Clear();
		List<Checkpoint> list = new List<Checkpoint>();
		list.Add(Singleton<CheckpointManager>.SP.Startline);
		if (Singleton<CheckpointManager>.SP.Checkpoints != null)
		{
			list.AddRange(Singleton<CheckpointManager>.SP.Checkpoints);
		}
		foreach (Checkpoint item in list)
		{
			SnapshotFrame snapshotFrame = new SnapshotFrame();
			if (item == Singleton<CheckpointManager>.SP.Startline)
			{
				snapshotFrame.position = item.transform.position + item.transform.TransformDirection(item.extraOffsetToSpawnPosition);
			}
			else
			{
				snapshotFrame.position = item.transform.position + item.transform.TransformDirection(new Vector3(-1f, item.extraOffsetToSpawnPosition.y, item.extraOffsetToSpawnPosition.z));
			}
			snapshotFrame.waypoint = (int)item.GetComponent<SplineFollow>().splineOffset.x + 2;
			snapshotFrame.velocity = Vector3.zero;
			snapshots.Add(snapshotFrame);
		}
		TextAsset textAsset = (TextAsset)Resources.Load("MedalReplays/lvl" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + "_Rainbow");
		if (textAsset != null)
		{
			HenkUtils.GetReplaySnapshots(textAsset.text, snapshots);
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus)
		{
			bonusMode = true;
			Vector3 vector = new Vector3(-10000f, -10000f, -10000f);
			Vector3 vector2 = new Vector3(10000f, 10000f, 10000f);
			Collectable[] array = UnityEngine.Object.FindObjectsOfType<Collectable>();
			foreach (Collectable collectable in array)
			{
				vector = Vector3.Max(vector, collectable.GetComponent<SplineFollow>().splineOffset);
				vector2 = Vector3.Min(vector2, collectable.GetComponent<SplineFollow>().splineOffset);
			}
			Vector3 splineOffset = (vector + vector2) * 0.5f;
			minCoinPos = levelSpline.DeformVertex(Vector3.zero, vector2);
			maxCoinPos = levelSpline.DeformVertex(Vector3.zero, vector);
			averageCoinPosition = levelSpline.DeformVertex(Vector3.zero, splineOffset) + Vector3.up * 5f;
			averageCoinDirection = Vector3.Slerp(levelSpline.GetNormal3D((int)vector2.x), levelSpline.GetNormal3D((int)vector.x), 0.5f);
			SnapshotFrame snapshotFrame2 = new SnapshotFrame();
			snapshotFrame2.position = averageCoinPosition;
			snapshotFrame2.waypoint = 0;
			snapshotFrame2.velocity = Vector3.zero;
			snapshots.Add(snapshotFrame2);
		}
		else if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop)
		{
			BoostPad[] array2 = UnityEngine.Object.FindObjectsOfType<BoostPad>();
			foreach (BoostPad boostPad in array2)
			{
				SnapshotFrame snapshotFrame3 = new SnapshotFrame();
				snapshotFrame3.position = boostPad.transform.position + boostPad.transform.TransformDirection(new Vector3(5f, 2f, -1f));
				snapshotFrame3.waypoint = (int)boostPad.GetComponent<SplineFollow>().splineOffset.x + 5;
				snapshotFrame3.velocity = Vector3.zero;
				snapshots.Add(snapshotFrame3);
			}
			JumpPad[] array3 = UnityEngine.Object.FindObjectsOfType<JumpPad>();
			foreach (JumpPad jumpPad in array3)
			{
				SnapshotFrame snapshotFrame4 = new SnapshotFrame();
				snapshotFrame4.position = jumpPad.transform.position + jumpPad.transform.TransformDirection(new Vector3(1f, 2f, -1f));
				snapshotFrame4.waypoint = (int)jumpPad.GetComponent<SplineFollow>().splineOffset.x + 1;
				snapshotFrame4.velocity = Vector3.zero;
				snapshots.Add(snapshotFrame4);
			}
		}
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
		{
			challengeMode = true;
			snapshots.RemoveRange(1, snapshots.Count - 1);
		}
	}

	private int GetRandomSnapShot()
	{
		int num = currentSnapShot;
		for (int i = 0; i < 10; i++)
		{
			if (num != currentSnapShot)
			{
				break;
			}
			num = UnityEngine.Random.Range(0, snapshots.Count);
		}
		return num;
	}

	private void SetNextSnapshot()
	{
		if (snapshots.Count == 0)
		{
			return;
		}
		snapShotTime = 0f;
		if (currentSnapShot == -1)
		{
			currentSnapShot = 0;
		}
		else if (bonusMode)
		{
			currentSnapShot = 1;
		}
		else if (!challengeMode)
		{
			currentSnapShot = GetRandomSnapShot();
		}
		if (bonusMode && currentSnapShot != 0)
		{
			Vector3 vector = averageCoinDirection;
			base.transform.position = averageCoinPosition + vector * 20f;
			rotSpeed = Vector3.zero;
			velocity = vector * 7f;
			Vector3 worldPosition = base.transform.position - vector;
			base.transform.LookAt(worldPosition);
			return;
		}
		SnapshotFrame snapshotFrame = snapshots[currentSnapShot];
		SnapshotFrame snapshotFrame2 = snapshots[currentSnapShot];
		if (currentSnapShot < snapshots.Count - 1)
		{
			snapshotFrame2 = snapshots[currentSnapShot + 1];
		}
		Vector3 normal3D = levelSpline.GetNormal3D(snapshotFrame.waypoint);
		Vector3 edge3D = levelSpline.GetEdge3D(snapshotFrame.waypoint);
		if (!challengeMode)
		{
			float f = snapshotFrame2.waypoint - snapshotFrame.waypoint;
			float num = snapshotFrame2.position.y - snapshotFrame.position.y;
			float num2 = Mathf.Sign(num - 1f);
			float num3 = 4f;
			float num4 = 3f;
			float num5 = 1f;
			if (currentSnapShot == 0)
			{
				f = -1f;
				num = 0f;
				num2 = -1f;
				num3 = 1.6f;
				num4 = 3f;
				num5 = 1f;
			}
			base.transform.position = snapshotFrame.position + normal3D * num3 + Vector3.up * num4 * num2 + edge3D * 3.5f * (0f - Mathf.Sign(f));
			Vector3 worldPosition2 = base.transform.position + edge3D * Mathf.Sign(f) - normal3D * 0.5f;
			base.transform.LookAt(worldPosition2);
			velocity = normal3D * (num5 + Mathf.Abs(num * 0.05f));
			velocity.y = (0f - num2) * 1.75f;
			rotSpeed.y = (6f + Mathf.Abs(num * 0.05f)) * (0f - Mathf.Sign(f));
			rotSpeed.x = (0f - num) * 0.4f;
			rotSpeed.z = 1f;
			return;
		}
		if (challengeShot == 0)
		{
			base.transform.position = snapshotFrame.position - normal3D * 2f + edge3D * 1.5f + Vector3.down * 0.8f;
			base.transform.LookAt(base.transform.position - edge3D + Vector3.up * 0.2f);
			velocity = Vector3.up * 1.1f;
			rotSpeed = Vector3.zero;
			rotSpeed.x = 10f;
		}
		if (challengeShot == 1)
		{
			base.transform.position = snapshotFrame.position + normal3D * 0.75f + edge3D * 1.5f + Vector3.up * 1.25f;
			base.transform.LookAt(base.transform.position - edge3D);
			velocity = -normal3D * 1.25f;
			rotSpeed = Vector3.zero;
		}
		if (challengeShot == 2)
		{
			base.transform.position = snapshotFrame.position + normal3D * 1.5f + Vector3.up * -3f + edge3D * 3.5f;
			base.transform.LookAt(base.transform.position - edge3D - normal3D * 0.5f);
			velocity = normal3D * 1f + Vector3.up * 1.75f;
			rotSpeed = new Vector3(6f, 5f, 1f);
		}
		if (challengeShot == 3)
		{
			base.transform.position = snapshotFrame.position - normal3D + Vector3.down * 0.9f - edge3D * 3f;
			base.transform.LookAt(base.transform.position + edge3D + Vector3.up * 0.25f);
			velocity = Vector3.up * 1.5f;
			rotSpeed = Vector3.zero;
			rotSpeed.x = 10f;
		}
		challengeShot++;
		challengeShot %= 4;
	}

	private void Update()
	{
		snapShotTime += Time.deltaTime;
		float num = 4f;
		float num2 = 3f / num;
		if (bonusMode && currentSnapShot != 0)
		{
			Vector3 vector = base.camera.WorldToViewportPoint(minCoinPos);
			Vector3 vector2 = base.camera.WorldToViewportPoint(maxCoinPos);
			bool num3 = vector.z > 0f && vector2.z > 0f;
			bool flag = vector.x > 0f && vector.y > 0f && vector.x < 1f && vector.y < 0.9f;
			bool flag2 = vector2.x > 0f && vector2.y > 0f && vector2.x < 1f && vector2.y < 0.9f;
			if (num3 && flag && flag2)
			{
				velocity -= velocity * 0.5f * Time.deltaTime;
			}
		}
		else if (snapShotTime - Time.deltaTime < num - 1.2f && snapShotTime > num - 1.2f)
		{
			Singleton<PermaGUI>.SP.FadeInOrOut(0.95f, fadeIn: false);
		}
		else if (snapShotTime > num)
		{
			SetNextSnapshot();
			Singleton<PermaGUI>.SP.FadeInOrOut(1f, fadeIn: true);
		}
		base.transform.position += velocity * Time.deltaTime * num2;
		base.transform.Rotate(rotSpeed * Time.deltaTime * num2);
	}

	public void StartCamera()
	{
		base.camera.fieldOfView = cameraFieldOfView;
		challengeShot = 0;
		currentSnapShot = -1;
		SetNextSnapshot();
		if (bonusMode || challengeMode)
		{
			StartCoroutine("PlayBonusCharacterIntro");
		}
	}

	public void StopCamera()
	{
		StopCoroutine("PlayBonusCharacterIntro");
	}

	private IEnumerator PlayBonusCharacterIntro()
	{
		yield return new WaitForSeconds(0.7f);
		GameObject gameObject = ((!bonusMode) ? Singleton<PlayerManager>.SP.GetGhost() : Singleton<PlayerManager>.SP.GetPlayer());
		Singleton<AudioManager>.SP.PlayCharacterIntro(gameObject.GetComponent<PlayerGraphics>().currentCharacter, gameObject.GetComponent<PlayerGraphics>().currentSkinNum);
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < snapshots.Count - 1; i++)
		{
			SnapshotFrame snapshotFrame = snapshots[i];
			SnapshotFrame snapshotFrame2 = snapshots[i + 1];
			float t = Mathf.InverseLerp(20f, 60f, snapshotFrame.velocity.magnitude);
			Gizmos.color = Color.Lerp(Color.green, Color.red, t);
			Gizmos.DrawLine(snapshotFrame.position, snapshotFrame2.position);
		}
	}

	public float GetCameraHorizontalFOV()
	{
		return Mathf.Atan(Mathf.Tan(base.camera.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * base.camera.aspect) * 2f * 57.29578f;
	}
}
