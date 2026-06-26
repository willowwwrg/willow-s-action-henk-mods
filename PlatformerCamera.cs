using System;
using UnityEngine;

public class PlatformerCamera : MonoBehaviour
{
	public Transform target;

	private Transform targetBackup;

	[HideInInspector]
	public Transform levelEditorCursorTransform;

	private float zoomInterpolation = 0.1f;

	private float targetZoomInterpolation = 0.1f;

	private PlatformerPhysics physics;

	private RaycastCollider playerCollider;

	private PlayerWaypointManager waypointManager;

	[HideInInspector]
	private CameraState curCameraState;

	[HideInInspector]
	public float yPosOfDeathVolume = -1000f;

	public float baseDistance = 13f;

	public float extraDistAtMaxSpeed = 15f;

	public float extraHeight = 3f;

	public float extraHeightLookBack;

	public float posSmoothing = 5f;

	public float rotSmoothing = 5f;

	private float speedToStartDistanceShift = 20f;

	private float maxSpeedForDistanceShift = 70f;

	private float distanceSmoothing = 2f;

	public bool dollyZoom;

	public float horizontalAdd = 4f;

	public float horizontalAccel = 0.1f;

	public float horizontalLookBack;

	private float direction = 1f;

	public float verticalAccel = 0.3f;

	public float verticalFriction = 3f;

	private float directionV;

	private float maxVerticalAccel = 45f;

	public bool cameraShake = true;

	public float camShakeTest;

	public float camShakeBase = 0.005f;

	public float camShakeSliding = 0.015f;

	public float camShakeImpact = 0.0125f;

	private float camShake;

	private float camShakeFriction = 3f;

	public bool cameraShakePosition;

	private float camShakePositionMultiply = 0.25f;

	public float curveLookAhead = 0.35f;

	public bool isInside = true;

	private float viewWidth;

	private float distance;

	private Quaternion camShakeAdd = Quaternion.identity;

	private Vector3 camShakePositionAdd = Vector3.zero;

	private bool staticFollow;

	private Vector3 previousPhysicsPos = Vector3.zero;

	private Vector3 physicsPosition = Vector3.zero;

	private Quaternion previousPhysicsRotation = Quaternion.identity;

	private Quaternion physicsRotation = Quaternion.identity;

	private UpdateMode updateMode;

	[HideInInspector]
	public bool hasCameraStateInput;

	private float targetTimeScale = 1f;

	public void Start()
	{
		if (!target)
		{
			FindTarget();
		}
	}

	private void OnEventLand(float impactAmount)
	{
		if (base.gameObject == target)
		{
			camShake += camShakeImpact * impactAmount;
		}
	}

	public void FindTarget()
	{
		if ((bool)Singleton<PlayerManager>.SP.GetPlayer())
		{
			SetTarget(Singleton<PlayerManager>.SP.GetPlayer().transform);
		}
	}

	public void SetTarget(Transform targetToSet)
	{
		target = targetToSet;
		targetBackup = target;
		if ((bool)GetComponent<DepthOfFieldScatter>())
		{
			GetComponent<DepthOfFieldScatter>().SetTarget(target);
		}
		physics = target.GetComponent<PlatformerPhysics>();
		playerCollider = target.GetComponent<RaycastCollider>();
		waypointManager = target.GetComponent<PlayerWaypointManager>();
	}

	public void ToggleExtraControls(bool enable)
	{
		hasCameraStateInput = enable;
		targetTimeScale = 1f;
	}

	private void Update()
	{
		if (curCameraState == CameraState.LevelEditor)
		{
			LevelEditorCameraControls();
		}
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer && Input.GetKey(KeyCode.LeftShift))
		{
			for (int i = 0; i < 9; i++)
			{
				if (Input.GetKeyDown((KeyCode)(49 + i)))
				{
					GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
					if (allPlayers.Length > i)
					{
						SetTarget(allPlayers[i].transform);
						RespawnNoCamTypeReset();
					}
				}
			}
		}
		if (hasCameraStateInput)
		{
			if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.None)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					SetCameraState(CameraState.SuperZoom);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					SetCameraState(CameraState.StaticFollow);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					SetCameraState(CameraState.FlyBy);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					SetCameraState(CameraState.Zoom);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					SetCameraState(CameraState.FrogView);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha6))
				{
					SetCameraState(CameraState.SideView);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha7))
				{
					SetCameraState(CameraState.Default);
					SnapCamera();
				}
				if (Input.GetKeyDown(KeyCode.Alpha8))
				{
					SetCameraState(CameraState.CloseUpFront);
					SnapCamera();
				}
			}
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				targetTimeScale = 0.1f;
			}
			else
			{
				targetTimeScale = 1f;
			}
			Time.timeScale -= (Time.timeScale - targetTimeScale) * 7f * Time.deltaTime;
		}
		if (updateMode == UpdateMode.Update && (bool)target)
		{
			base.transform.position = Vector3.Lerp(previousPhysicsPos, physicsPosition, Singleton<PhysicsTimeManager>.SP.interpolationValue);
			base.transform.rotation = Quaternion.Slerp(previousPhysicsRotation, physicsRotation, Singleton<PhysicsTimeManager>.SP.interpolationValue);
		}
	}

	private void LevelEditorCameraControls()
	{
		if (!Singleton<EditorCursor>.SP.toyboxOpen)
		{
			float num = 0f - Input.GetAxisRaw("Mouse ScrollWheel");
			targetZoomInterpolation += num * 0.5f;
			targetZoomInterpolation = Mathf.Clamp01(targetZoomInterpolation);
			zoomInterpolation = Mathf.Lerp(zoomInterpolation, targetZoomInterpolation, Time.deltaTime * 15f);
			baseDistance = Mathf.Lerp(10f, 100f, zoomInterpolation);
		}
	}

	private void FixedUpdate()
	{
		previousPhysicsPos = physicsPosition;
		previousPhysicsRotation = physicsRotation;
		UpdateFuction();
		if (updateMode == UpdateMode.FixedUpdate && (bool)target)
		{
			base.transform.position = physicsPosition;
			base.transform.rotation = physicsRotation;
		}
	}

	private void UpdateFuction()
	{
		if (curCameraState == CameraState.LevelEditor && target == targetBackup)
		{
			if (!levelEditorCursorTransform)
			{
				levelEditorCursorTransform = Singleton<EditorCameraFollow>.SP.transform;
			}
			target = levelEditorCursorTransform;
		}
		else if (curCameraState != CameraState.LevelEditor && target != targetBackup)
		{
			target = targetBackup;
		}
		if ((bool)target)
		{
			Vector3 velocity = playerCollider.velocity;
			float num = Mathf.InverseLerp(speedToStartDistanceShift, maxSpeedForDistanceShift, velocity.magnitude) * extraDistAtMaxSpeed;
			float num2 = baseDistance + num;
			distance -= (distance - num2) * distanceSmoothing * Time.deltaTime;
			float horizontalSpeed = physics.GetHorizontalSpeed();
			float num3 = Mathf.Sign(horizontalSpeed);
			direction -= (direction - num3) * Time.deltaTime * Mathf.Abs(horizontalSpeed) * horizontalAccel;
			if (!physics.onGround)
			{
				float num4 = (playerCollider.predictedPosition.y - target.position.y) * verticalAccel;
				if (num4 < 0f - maxVerticalAccel)
				{
					num4 = 0f - maxVerticalAccel;
				}
				directionV -= num4 * Time.deltaTime;
			}
			directionV -= Mathf.Sign(directionV) * directionV * directionV * verticalFriction * Time.deltaTime;
			Vector3 vector = directionV * -Vector3.up;
			Vector3 vector2 = target.forward;
			Vector3 vector3 = target.right;
			if ((bool)waypointManager)
			{
				waypointManager.GetOffset();
				_ = curveLookAhead;
				vector2 = waypointManager.GetSideVectorSmooth();
				vector3 = new Vector3(vector2.z, 0f, 0f - vector2.x);
			}
			float num5 = extraHeight;
			if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.FirstPerson && physics.sliding)
			{
				num5 = extraHeight * 0.8f;
			}
			Vector3 vector4 = target.position + vector3 * direction * horizontalAdd + num5 * target.up + vector;
			Vector3 vector5 = vector4 - distance * vector2;
			if (!HenkUtils.IsOutside())
			{
				vector5.y = Mathf.Clamp(vector5.y, 1f, 125f);
			}
			if (vector5.y < yPosOfDeathVolume)
			{
				vector5.y = yPosOfDeathVolume;
			}
			if (cameraShakePosition)
			{
				physicsPosition -= camShakePositionAdd;
				if (physics.onGround && physics.sliding)
				{
					camShake += playerCollider.velocity.magnitude * camShakeSliding * camShakePositionMultiply * Time.deltaTime;
				}
				else
				{
					camShake += playerCollider.velocity.magnitude * camShakeBase * camShakePositionMultiply * Time.deltaTime;
				}
				camShake += camShakeTest * camShakePositionMultiply * Time.deltaTime;
				camShakePositionAdd = UnityEngine.Random.onUnitSphere * camShake;
				camShakePositionAdd -= Vector3.Dot(camShakePositionAdd, base.transform.forward) * base.transform.forward;
				camShake -= camShake * camShakeFriction * Time.deltaTime;
			}
			Vector3 vector6 = (physicsPosition - vector5) * posSmoothing * Time.deltaTime;
			if (staticFollow)
			{
				vector6.x = 0f;
				vector6.z = 0f;
			}
			physicsPosition -= vector6;
			if (cameraShakePosition)
			{
				physicsPosition += camShakePositionAdd;
			}
			Vector3 vector7 = vector4 + vector - vector3 * direction * horizontalLookBack + extraHeightLookBack * Vector3.down;
			Quaternion to = Quaternion.LookRotation(vector7 - physicsPosition);
			if (cameraShake && !cameraShakePosition)
			{
				physicsRotation *= Quaternion.Inverse(camShakeAdd);
				if (physics.onGround && physics.sliding)
				{
					camShake += playerCollider.velocity.magnitude * camShakeSliding * Time.deltaTime;
				}
				else
				{
					camShake += playerCollider.velocity.magnitude * camShakeBase * Time.deltaTime;
				}
				camShake += camShakeTest * Time.deltaTime;
				camShakeAdd = Quaternion.Euler(UnityEngine.Random.onUnitSphere * camShake);
				camShake -= camShake * camShakeFriction * Time.deltaTime;
			}
			physicsRotation = Quaternion.Slerp(physicsRotation, to, rotSmoothing * Time.deltaTime);
			if (cameraShake && !cameraShakePosition)
			{
				physicsRotation *= camShakeAdd;
			}
			if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.RotatingCamera && (bool)target.GetComponent<PlayerGraphics>())
			{
				Vector3 eulerAngles = physicsRotation.eulerAngles;
				eulerAngles.z = target.GetComponent<PlayerGraphics>().GetTiltRotation();
				physicsRotation.eulerAngles = eulerAngles;
			}
			if (viewWidth != 0f && dollyZoom && (bool)base.camera)
			{
				base.camera.fieldOfView = Mathf.Atan(viewWidth / (distance * 2f)) * 2f * 57.29578f;
			}
			if (staticFollow && (physicsPosition - target.position).magnitude > 50f)
			{
				SnapCamera();
			}
			Debug.DrawLine(physicsPosition, vector5, Color.green);
			Debug.DrawLine(physicsPosition, vector7, Color.green);
			Debug.DrawLine(vector4, vector5, Color.green);
			Debug.DrawLine(vector4, target.position, Color.green);
		}
		yPosOfDeathVolume = -1000f;
	}

	public void OnRespawn()
	{
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGame)) || Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGameMultiplayer)))
		{
			SetCameraState(CameraState.Default);
			if ((bool)target.GetComponent<GrapplingHook>() && target.GetComponent<GrapplingHook>().enabled)
			{
				SetCameraState(CameraState.Hook);
			}
		}
		distance = baseDistance;
		direction = 1f;
		directionV = 0f;
		camShake = 0f;
		SnapCamera();
	}

	public void RespawnNoCamTypeReset()
	{
		distance = baseDistance;
		direction = 1f;
		directionV = 0f;
		camShake = 0f;
		SnapCamera();
	}

	public void OnReset()
	{
		OnRespawn();
	}

	public void RecalcViewWidth()
	{
		if ((bool)base.camera)
		{
			viewWidth = Mathf.Tan(base.camera.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * baseDistance * 2f;
		}
	}

	public CameraState GetCurrentCameraState()
	{
		return curCameraState;
	}

	public void SetCameraState(CameraState state)
	{
		if ((state == CameraState.Default || state == CameraState.Hook) && Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Bonus)
		{
			state = CameraState.Bonus;
		}
		baseDistance = 13f;
		extraDistAtMaxSpeed = 15f;
		extraHeight = 3f;
		extraHeightLookBack = 0f;
		posSmoothing = 5f;
		rotSmoothing = 5f;
		distanceSmoothing = 2f;
		horizontalAdd = 4f;
		horizontalAccel = 0.1f;
		horizontalLookBack = 0f;
		verticalAccel = 0.85f;
		verticalFriction = 1f;
		staticFollow = false;
		SetCameraFov(70f);
		curCameraState = state;
		cameraShake = true;
		dollyZoom = false;
		updateMode = UpdateMode.Update;
		switch (state)
		{
		case CameraState.Hook:
			baseDistance = 23f;
			verticalAccel = 0.05f;
			extraDistAtMaxSpeed = 20f;
			break;
		case CameraState.Bonus:
			baseDistance = 23f;
			verticalAccel = 0.05f;
			extraDistAtMaxSpeed = 15f;
			cameraShake = false;
			break;
		case CameraState.Zoom:
			baseDistance = 5f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = 1.5f;
			distanceSmoothing = 3.5f;
			horizontalAdd = 3f;
			break;
		case CameraState.FlyBy:
			baseDistance = 27f;
			extraHeight = 25f;
			extraHeightLookBack = 21f;
			posSmoothing = 1f;
			rotSmoothing = 4f;
			SetCameraFov(40f);
			cameraShake = false;
			dollyZoom = true;
			RecalcViewWidth();
			break;
		case CameraState.SuperZoom:
			baseDistance = 2f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = 0.3f;
			posSmoothing = 40f;
			rotSmoothing = 20f;
			horizontalAdd = 3f;
			horizontalLookBack = 2f;
			verticalAccel = 0f;
			SetCameraFov(90f);
			break;
		case CameraState.StaticFollow:
			rotSmoothing = 10f;
			posSmoothing = 0.33f;
			horizontalAdd = 30f;
			horizontalLookBack = 25f;
			staticFollow = true;
			verticalAccel = 0f;
			break;
		case CameraState.Lava:
			baseDistance = 5f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = 4.5f;
			distanceSmoothing = 2.5f;
			horizontalAdd = 0f;
			posSmoothing = 2f;
			rotSmoothing = 2f;
			verticalAccel = 0f;
			break;
		case CameraState.LevelEditor:
			baseDistance = 14f;
			extraHeight = 1f;
			verticalAccel = 0.05f;
			rotSmoothing = 12f;
			posSmoothing = 12f;
			distanceSmoothing = 10f;
			horizontalAdd = 0f;
			updateMode = UpdateMode.FixedUpdate;
			break;
		case CameraState.FrogView:
			baseDistance = 2.3f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = -0.3f;
			extraHeightLookBack = -0.7f;
			posSmoothing = 10f;
			rotSmoothing = 10f;
			horizontalAdd = 0.5f;
			verticalAccel = 0f;
			SetCameraFov(90f);
			break;
		case CameraState.SideView:
			baseDistance = 50f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = 4f;
			posSmoothing = 10f;
			rotSmoothing = 10f;
			horizontalAdd = 3f;
			verticalAccel = 0.5f;
			SetCameraFov(30f);
			break;
		case CameraState.CloseUpFront:
			baseDistance = 2.25f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = 1.55f;
			extraHeightLookBack = 0.95f;
			posSmoothing = 40f;
			rotSmoothing = 100f;
			distanceSmoothing = 3.5f;
			horizontalAdd = 2.38f;
			horizontalLookBack = 2.1f;
			verticalAccel = 0f;
			break;
		}
		if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.BlindMode)
		{
			horizontalAdd = 1f;
			verticalAccel = 0f;
			posSmoothing = 15f;
			rotSmoothing = 15f;
		}
		else if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.FirstPerson)
		{
			baseDistance = 0.01f;
			extraDistAtMaxSpeed = 0f;
			extraHeight = 1.75f;
			posSmoothing = 25f;
			rotSmoothing = 25f;
			horizontalAdd = 0f;
			horizontalLookBack = -2f;
			horizontalAccel = 0.075f;
			staticFollow = false;
		}
		else if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.OppositeCamera)
		{
			baseDistance = 0f - baseDistance;
			extraDistAtMaxSpeed = 0f - extraDistAtMaxSpeed;
		}
		else if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.RotatingCamera)
		{
			baseDistance = 13f;
			if (state == CameraState.Hook)
			{
				baseDistance = 23f;
			}
			extraDistAtMaxSpeed = 0f;
			extraHeight = 2f;
			posSmoothing = 10f;
			rotSmoothing = 10f;
			horizontalAdd = 0f;
			verticalAccel = 0f;
		}
	}

	public void SetCameraFov(float fov)
	{
		if ((bool)base.camera)
		{
			base.camera.fieldOfView = fov;
		}
	}

	public void SnapCamera()
	{
		float num = posSmoothing;
		float num2 = distanceSmoothing;
		float num3 = rotSmoothing;
		bool flag = staticFollow;
		posSmoothing = 1f / Time.deltaTime;
		distanceSmoothing = 1f / Time.deltaTime;
		rotSmoothing = 1f / Time.deltaTime;
		staticFollow = false;
		UpdateFuction();
		posSmoothing = num;
		distanceSmoothing = num2;
		rotSmoothing = num3;
		staticFollow = flag;
	}
}
