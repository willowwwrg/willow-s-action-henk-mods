using UnityEngine;

public class CameraPathBezierAnimator : MonoBehaviour
{
	public enum modes
	{
		once,
		loop,
		reverse,
		reverseLoop,
		pingPong
	}

	public delegate void AnimationStartedEventHandler();

	public delegate void AnimationPausedEventHandler();

	public delegate void AnimationStoppedEventHandler();

	public delegate void AnimationFinishedEventHandler();

	public delegate void AnimationLoopedEventHandler();

	public delegate void AnimationPingPongEventHandler();

	public delegate void AnimationPointReachedEventHandler();

	public delegate void AnimationPointReachedWithNumberEventHandler(int pointNumber);

	public CameraPathBezier _bezier;

	public bool playOnStart = true;

	public Transform animationTarget;

	public bool isCamera = true;

	private bool playing;

	public modes mode;

	private float pingPongDirection = 1f;

	public bool normalised = true;

	public float editorTime;

	public float pathTime = 10f;

	private float _percentage;

	private float usePercentage;

	private int atPointNumber;

	public float sensitivity = 5f;

	public float minX = -90f;

	public float maxX = 90f;

	private float rotationX;

	private float rotationY;

	public bool showPreview = true;

	public bool showScenePreview = true;

	public bool animateSceneObjectInEditor;

	public CameraPathBezierAnimator nextAnimation;

	public float pathSpeed
	{
		get
		{
			return bezier.storedTotalArcLength / pathTime;
		}
		set
		{
			pathTime = bezier.storedTotalArcLength / Mathf.Max(value, 1E-06f);
		}
	}

	public bool isPlaying => playing;

	public float percentage => _percentage;

	public bool pingPongGoingForward => pingPongDirection == 1f;

	public CameraPathBezier bezier
	{
		get
		{
			if (!_bezier)
			{
				_bezier = GetComponent<CameraPathBezier>();
			}
			return _bezier;
		}
	}

	private bool isReversed
	{
		get
		{
			if (mode != modes.reverse && mode != modes.reverseLoop)
			{
				return pingPongDirection < 0f;
			}
			return true;
		}
	}

	public event AnimationStartedEventHandler AnimationStarted;

	public event AnimationPausedEventHandler AnimationPaused;

	public event AnimationStoppedEventHandler AnimationStopped;

	public event AnimationFinishedEventHandler AnimationFinished;

	public event AnimationLoopedEventHandler AnimationLooped;

	public event AnimationPingPongEventHandler AnimationPingPong;

	public event AnimationPointReachedEventHandler AnimationPointReached;

	public event AnimationPointReachedWithNumberEventHandler AnimationPointReachedWithNumber;

	public void Play()
	{
		playing = true;
		if (!isReversed)
		{
			if (_percentage == 0f && this.AnimationStarted != null)
			{
				this.AnimationStarted();
			}
		}
		else if (_percentage == 1f && this.AnimationStarted != null)
		{
			this.AnimationStarted();
		}
	}

	public void Stop()
	{
		playing = false;
		CancelInvoke("Play");
		_percentage = 0f;
		if (this.AnimationStopped != null)
		{
			this.AnimationStopped();
		}
	}

	public void Pause()
	{
		playing = false;
		CancelInvoke("Play");
		if (this.AnimationPaused != null)
		{
			this.AnimationPaused();
		}
	}

	public void Seek(float value)
	{
		_percentage = Mathf.Clamp01(value);
		UpdateAnimationTime(advance: false);
		bool flag = playing;
		playing = true;
		UpdateAnimation();
		playing = flag;
	}

	public void Reverse()
	{
		switch (mode)
		{
		case modes.once:
			mode = modes.reverse;
			break;
		case modes.reverse:
			mode = modes.once;
			break;
		case modes.pingPong:
			pingPongDirection = ((pingPongDirection == -1f) ? 1 : (-1));
			break;
		case modes.loop:
			mode = modes.reverseLoop;
			break;
		case modes.reverseLoop:
			mode = modes.loop;
			break;
		}
	}

	public float RecalculatePercentage(float percentage)
	{
		if (bezier.numberOfControlPoints == 0)
		{
			return percentage;
		}
		float normalisedT = bezier.GetNormalisedT(percentage);
		int numberOfCurves = bezier.numberOfCurves;
		float num = 1f / (float)numberOfCurves;
		int num2 = Mathf.FloorToInt(normalisedT / num);
		float time = Mathf.Clamp01((normalisedT - (float)num2 * num) * (float)numberOfCurves);
		if (bezier.controlPoints[num2]._curve != null)
		{
			return bezier.controlPoints[num2]._curve.Evaluate(time) / (float)numberOfCurves + (float)num2 * num;
		}
		return percentage;
	}

	private void Awake()
	{
		if (Camera.allCameras.Length == 0)
		{
			Debug.LogWarning("Warning: There are no cameras in the scene");
			isCamera = false;
		}
		else if (isCamera && !animationTarget.GetComponent<Camera>())
		{
			Debug.LogWarning("Warning: Do not set animation to 'isCamera' when not using a camera");
			isCamera = false;
		}
		if (!isReversed)
		{
			_percentage = 0f;
			atPointNumber = -1;
		}
		else
		{
			_percentage = 1f;
			atPointNumber = bezier.numberOfControlPoints - 1;
		}
		Vector3 eulerAngles = bezier.GetPathRotation(0f).eulerAngles;
		rotationX = eulerAngles.y;
		rotationY = eulerAngles.x;
	}

	private void Start()
	{
		if (playOnStart)
		{
			Play();
		}
	}

	private void Update()
	{
		if (!isCamera)
		{
			if (playing)
			{
				UpdateAnimationTime();
				UpdateAnimation();
				UpdatePointReached();
			}
			else if (nextAnimation != null && _percentage >= 1f)
			{
				nextAnimation.Play();
				nextAnimation = null;
			}
		}
	}

	private void LateUpdate()
	{
		if (isCamera)
		{
			if (playing)
			{
				UpdateAnimationTime();
				UpdateAnimation();
				UpdatePointReached();
			}
			else if (nextAnimation != null && _percentage >= 1f)
			{
				nextAnimation.Play();
				nextAnimation = null;
			}
		}
	}

	private void UpdateAnimation()
	{
		if (animationTarget == null)
		{
			Debug.LogError("There is no aniamtion target specified in the Camera Path Bezier Animator component. Nothing to animate.\nYou can find this component in the main camera path component.");
			Stop();
		}
		else
		{
			if (!playing)
			{
				return;
			}
			animationTarget.position = bezier.GetPathPosition(usePercentage);
			if (isCamera)
			{
				animationTarget.camera.fieldOfView = bezier.GetPathFOV(usePercentage);
			}
			switch (bezier.mode)
			{
			case CameraPathBezier.viewmodes.usercontrolled:
				animationTarget.rotation = bezier.GetPathRotation(usePercentage);
				break;
			case CameraPathBezier.viewmodes.target:
				animationTarget.LookAt(bezier.target.transform.position);
				break;
			case CameraPathBezier.viewmodes.followpath:
			{
				Vector3 pathPosition3;
				Vector3 pathPosition4;
				if (!bezier.loop)
				{
					pathPosition3 = bezier.GetPathPosition(Mathf.Clamp01(usePercentage - 0.05f));
					pathPosition4 = bezier.GetPathPosition(Mathf.Clamp01(usePercentage + 0.05f));
				}
				else
				{
					float num3 = usePercentage - 0.05f;
					if (num3 < 0f)
					{
						num3 += 1f;
					}
					float num4 = usePercentage + 0.05f;
					if (num4 > 1f)
					{
						num4 += -1f;
					}
					pathPosition3 = bezier.GetPathPosition(num3);
					pathPosition4 = bezier.GetPathPosition(num4);
				}
				animationTarget.LookAt(animationTarget.position + (pathPosition4 - pathPosition3));
				animationTarget.eulerAngles += base.transform.forward * (0f - bezier.GetPathTilt(usePercentage));
				break;
			}
			case CameraPathBezier.viewmodes.reverseFollowpath:
			{
				Vector3 pathPosition;
				Vector3 pathPosition2;
				if (!bezier.loop)
				{
					pathPosition = bezier.GetPathPosition(Mathf.Clamp01(usePercentage - 0.05f));
					pathPosition2 = bezier.GetPathPosition(Mathf.Clamp01(usePercentage + 0.05f));
				}
				else
				{
					float num = usePercentage - 0.05f;
					if (num < 0f)
					{
						num += 1f;
					}
					float num2 = usePercentage + 0.05f;
					if (num2 > 1f)
					{
						num2 += -1f;
					}
					pathPosition = bezier.GetPathPosition(num);
					pathPosition2 = bezier.GetPathPosition(num2);
				}
				animationTarget.LookAt(animationTarget.position + (pathPosition - pathPosition2));
				break;
			}
			case CameraPathBezier.viewmodes.mouselook:
				animationTarget.rotation = GetMouseLook();
				break;
			}
		}
	}

	private void UpdatePointReached()
	{
		int pointNumber = bezier.GetPointNumber(usePercentage);
		if (pointNumber != atPointNumber)
		{
			CameraPathBezierControlPoint cameraPathBezierControlPoint = (isReversed ? bezier.controlPoints[atPointNumber] : bezier.controlPoints[pointNumber]);
			if (this.AnimationPointReached != null)
			{
				this.AnimationPointReached();
			}
			if (!isReversed)
			{
				if (this.AnimationPointReachedWithNumber != null)
				{
					this.AnimationPointReachedWithNumber(pointNumber);
				}
				else if (this.AnimationPointReachedWithNumber != null)
				{
					this.AnimationPointReachedWithNumber(atPointNumber);
				}
			}
			switch (cameraPathBezierControlPoint.delayMode)
			{
			case CameraPathBezierControlPoint.DELAY_MODES.indefinite:
				Pause();
				break;
			case CameraPathBezierControlPoint.DELAY_MODES.timed:
				Pause();
				Invoke("Play", cameraPathBezierControlPoint.delayTime);
				break;
			}
		}
		atPointNumber = pointNumber;
	}

	private void UpdateAnimationTime()
	{
		UpdateAnimationTime(advance: true);
	}

	private void UpdateAnimationTime(bool advance)
	{
		if (advance)
		{
			switch (mode)
			{
			case modes.once:
				if (_percentage >= 1f)
				{
					playing = false;
					if (this.AnimationPointReached != null)
					{
						this.AnimationPointReached();
					}
					if (this.AnimationPointReachedWithNumber != null)
					{
						this.AnimationPointReachedWithNumber(bezier.numberOfControlPoints - 1);
					}
					if (this.AnimationFinished != null)
					{
						this.AnimationFinished();
					}
				}
				else
				{
					_percentage += Time.deltaTime * (1f / pathTime);
				}
				break;
			case modes.loop:
				if (_percentage >= 1f)
				{
					_percentage = 0f;
					if (this.AnimationPointReached != null)
					{
						this.AnimationPointReached();
					}
					if (this.AnimationPointReachedWithNumber != null)
					{
						this.AnimationPointReachedWithNumber(bezier.numberOfControlPoints - 1);
					}
					if (this.AnimationLooped != null)
					{
						this.AnimationLooped();
					}
				}
				_percentage += Time.deltaTime * (1f / pathTime);
				break;
			case modes.reverseLoop:
				if (_percentage <= 0f)
				{
					_percentage = 1f;
					if (this.AnimationPointReached != null)
					{
						this.AnimationPointReached();
					}
					if (this.AnimationPointReachedWithNumber != null)
					{
						this.AnimationPointReachedWithNumber(0);
					}
					if (this.AnimationLooped != null)
					{
						this.AnimationLooped();
					}
				}
				_percentage += (0f - Time.deltaTime) * (1f / pathTime);
				break;
			case modes.reverse:
				if (_percentage <= 0f)
				{
					playing = false;
					if (this.AnimationPointReached != null)
					{
						this.AnimationPointReached();
					}
					if (this.AnimationPointReachedWithNumber != null)
					{
						this.AnimationPointReachedWithNumber(0);
					}
					if (this.AnimationFinished != null)
					{
						this.AnimationFinished();
					}
				}
				else
				{
					_percentage += (0f - Time.deltaTime) * (1f / pathTime);
				}
				break;
			case modes.pingPong:
				_percentage += Time.deltaTime * (1f / pathTime) * pingPongDirection;
				if (_percentage >= 1f)
				{
					_percentage = 0.99f;
					pingPongDirection = -1f;
					if (this.AnimationPointReached != null)
					{
						this.AnimationPointReached();
					}
					if (this.AnimationPointReachedWithNumber != null)
					{
						this.AnimationPointReachedWithNumber(bezier.numberOfControlPoints - 1);
					}
					if (this.AnimationPingPong != null)
					{
						this.AnimationPingPong();
					}
				}
				if (_percentage <= 0f)
				{
					_percentage = 0.01f;
					pingPongDirection = 1f;
					if (this.AnimationPointReached != null)
					{
						this.AnimationPointReached();
					}
					if (this.AnimationPointReachedWithNumber != null)
					{
						this.AnimationPointReachedWithNumber(0);
					}
					if (this.AnimationPingPong != null)
					{
						this.AnimationPingPong();
					}
				}
				break;
			}
		}
		_percentage = Mathf.Clamp01(_percentage);
		usePercentage = ((!normalised) ? _percentage : RecalculatePercentage(_percentage));
	}

	private Quaternion GetMouseLook()
	{
		if (animationTarget == null)
		{
			return Quaternion.identity;
		}
		rotationX += Input.GetAxis("Mouse X") * sensitivity;
		rotationY += (0f - Input.GetAxis("Mouse Y")) * sensitivity;
		rotationY = Mathf.Clamp(rotationY, minX, maxX);
		return Quaternion.Euler(new Vector3(rotationY, rotationX, 0f));
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, 0f - max, 0f - min);
	}
}
