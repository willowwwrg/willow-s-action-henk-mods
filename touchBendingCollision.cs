using UnityEngine;

[AddComponentMenu("AFS/Touch Bending/Collision")]
public class touchBendingCollision : MonoBehaviour
{
	public Material simpleBendingMaterial;

	public Material touchBendingMaterial;

	public float stiffness = 10f;

	public float disturbance = 0.3f;

	public float duration = 5f;

	private Transform myTransform;

	private Renderer myRenderer;

	private Matrix4x4 myMatrix;

	private Vector3 axis;

	private Vector3 axis1;

	private bool touched;

	private bool doubletouched;

	private bool left;

	private bool finished = true;

	private bool left1;

	private bool finished1 = true;

	private float intialTouchForce;

	private float touchBending;

	private float targetTouchBending;

	private float easingControl;

	private float intialTouchForce1;

	private float touchBending1;

	private float targetTouchBending1;

	private float easingControl1;

	private int Player_ID;

	private touchBendingPlayerListener PlayerVars;

	private Vector3 Player_Direction;

	private float Player_Speed;

	private int Player1_ID;

	private touchBendingPlayerListener PlayerVars1;

	private Vector3 Player_Direction1;

	private float Player_Speed1;

	private float timer;

	private float timer1;

	private float lerptime;

	private void Awake()
	{
		myTransform = base.transform;
		myRenderer = base.renderer;
	}

	private void Start()
	{
		myRenderer.sharedMaterial = simpleBendingMaterial;
	}

	private void OnTriggerEnter(Collider other)
	{
		touchBendingPlayerListener component = other.GetComponent<touchBendingPlayerListener>();
		if (!(component != null) || !component.enabled)
		{
			return;
		}
		if (!touched)
		{
			Player_ID = other.GetInstanceID();
			Object.Destroy(myRenderer.material);
			PlayerVars = component;
			Player_Direction = PlayerVars.Player_Direction;
			Player_Speed = PlayerVars.Player_Speed;
			intialTouchForce = Player_Speed;
			myRenderer.material = touchBendingMaterial;
			myRenderer.material.SetVector("_TouchBendingPosition", new Vector4(0f, 0f, 0f, 0f));
			axis = myTransform.InverseTransformDirection(Player_Direction);
			axis = Quaternion.Euler(0f, 90f, 0f) * axis;
			timer = 0f;
			touched = true;
			left = false;
			targetTouchBending = 1f;
			touchBending = targetTouchBending;
			finished = false;
			return;
		}
		if (doubletouched)
		{
			SwapTouchBending();
		}
		Player1_ID = other.GetInstanceID();
		PlayerVars1 = component;
		Player_Direction1 = PlayerVars1.Player_Direction;
		Player_Speed1 = PlayerVars1.Player_Speed;
		intialTouchForce1 = Player_Speed1;
		axis1 = myTransform.InverseTransformDirection(Player_Direction1);
		axis1 = Quaternion.Euler(0f, 90f, 0f) * axis1;
		timer1 = 0f;
		left1 = false;
		targetTouchBending1 = 1f;
		touchBending1 = targetTouchBending1;
		finished1 = false;
		lerptime = duration - timer;
		doubletouched = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (Player_ID != Player1_ID)
		{
			if (other.GetInstanceID() == Player_ID)
			{
				left = true;
				targetTouchBending = 0f;
			}
			else
			{
				left1 = true;
				targetTouchBending1 = 0f;
			}
		}
		else
		{
			left = true;
			targetTouchBending = 0f;
			left1 = true;
			targetTouchBending1 = 0f;
		}
	}

	private void Update()
	{
		if (!touched)
		{
			return;
		}
		Player_Speed = PlayerVars.Player_Speed;
		touchBending = Mathf.Lerp(touchBending, targetTouchBending, timer / duration);
		easingControl = Bounce(timer);
		if (!doubletouched)
		{
			if (finished && targetTouchBending == 0f)
			{
				ResetTouchBending();
				return;
			}
			Quaternion q = Quaternion.Euler(axis * (intialTouchForce * stiffness) * easingControl);
			myMatrix.SetTRS(Vector3.zero, q, new Vector3(1f, 1f, 1f));
			myRenderer.material.SetMatrix("_RotMatrix", myMatrix);
			myRenderer.material.SetVector("_TouchBendingForce", new Vector4(Player_Direction.x, Player_Direction.y, Player_Direction.z, Player_Speed * easingControl * disturbance));
			if (left)
			{
				timer += Time.deltaTime;
			}
			else
			{
				timer += Time.deltaTime * Player_Speed;
			}
			return;
		}
		if (finished && targetTouchBending == 0f)
		{
			SwapTouchBending();
			doubletouched = false;
			Player_Speed = PlayerVars.Player_Speed;
			touchBending = Mathf.Lerp(touchBending, targetTouchBending, timer / duration);
			easingControl = Bounce(timer);
			if (finished && targetTouchBending == 0f)
			{
				ResetTouchBending();
				return;
			}
			Quaternion q2 = Quaternion.Euler(axis * (intialTouchForce * stiffness) * easingControl);
			myMatrix.SetTRS(Vector3.zero, q2, new Vector3(1f, 1f, 1f));
			myRenderer.material.SetMatrix("_RotMatrix", myMatrix);
			myRenderer.material.SetVector("_TouchBendingForce", new Vector4(Player_Direction.x, Player_Direction.y, Player_Direction.z, Player_Speed * easingControl * disturbance));
			if (left)
			{
				timer += Time.deltaTime;
			}
			else
			{
				timer += Time.deltaTime * Player_Speed;
			}
			return;
		}
		Player_Speed1 = PlayerVars1.Player_Speed;
		touchBending1 = Mathf.Lerp(touchBending1, targetTouchBending1, timer1 / duration);
		easingControl1 = Bounce1(timer1);
		if (finished1 && targetTouchBending1 == 0f)
		{
			doubletouched = false;
			return;
		}
		Quaternion q3 = Quaternion.Euler(axis * (intialTouchForce * stiffness) * easingControl);
		Quaternion quaternion = Quaternion.Euler(axis1 * (intialTouchForce1 * stiffness) * easingControl1);
		q3 *= quaternion;
		myMatrix.SetTRS(Vector3.zero, q3, new Vector3(1f, 1f, 1f));
		myRenderer.material.SetMatrix("_RotMatrix", myMatrix);
		myRenderer.material.SetVector("_TouchBendingForce", Vector4.Lerp(new Vector4(Player_Direction.x, Player_Direction.y, Player_Direction.z, Player_Speed * easingControl * disturbance), new Vector4(Player_Direction1.x, Player_Direction1.y, Player_Direction1.z, Player_Speed1 * easingControl1 * disturbance), timer1 / (lerptime + 0.0001f) * 8f));
		if (left)
		{
			timer += Time.deltaTime;
		}
		else
		{
			timer += Time.deltaTime * Player_Speed;
		}
		if (left1)
		{
			timer1 += Time.deltaTime;
		}
		else
		{
			timer1 += Time.deltaTime * Player_Speed1;
		}
	}

	public float Bounce(float x)
	{
		if (x / duration >= 1f)
		{
			if (easingControl == 0f && left)
			{
				finished = true;
			}
			return targetTouchBending;
		}
		return Mathf.Lerp(Mathf.Sin(x * 10f / duration) / (x + 1.25f) * 8f, touchBending, Mathf.Sqrt(x / duration));
	}

	public float Bounce1(float x)
	{
		if (x / duration >= 1f)
		{
			if (easingControl1 == 0f && left1)
			{
				finished1 = true;
			}
			return targetTouchBending1;
		}
		return Mathf.Lerp(Mathf.Sin(x * 10f / duration) / (x + 1.25f) * 8f, touchBending1, Mathf.Sqrt(x / duration));
	}

	public void SwapTouchBending()
	{
		Player_ID = Player1_ID;
		PlayerVars = PlayerVars1;
		Player_Direction = Player_Direction1;
		Player_Speed = Player_Speed1;
		intialTouchForce = intialTouchForce1;
		touchBending = touchBending1;
		targetTouchBending = targetTouchBending1;
		easingControl = easingControl1;
		left = left1;
		finished = finished1;
		axis = axis1;
		timer = timer1;
	}

	public void ResetTouchBending()
	{
		Object.DestroyImmediate(myRenderer.material);
		myRenderer.sharedMaterial = simpleBendingMaterial;
		touched = false;
		doubletouched = false;
	}
}
