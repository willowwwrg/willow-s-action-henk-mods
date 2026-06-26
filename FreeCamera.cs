using UnityEngine;

public class FreeCamera : GameCamera
{
	public KeyCode ForwardKey = KeyCode.W;

	public KeyCode BackwardKey = KeyCode.S;

	public KeyCode LeftKey = KeyCode.A;

	public KeyCode RightKey = KeyCode.D;

	public KeyCode UpKey = KeyCode.E;

	public KeyCode DownKey = KeyCode.C;

	public Vector3 freeSpeed = Vector3.one;

	public float XSpeed = 1f;

	public float YSpeed = 1f;

	public float ZSpeed = 1f;

	public float Tau = 0.18f;

	public float Critical = 1f;

	public float cameraEaseTime = 1f;

	private float vx;

	private float vy;

	private float fovvel;

	private Vector3 cwvel = Vector3.zero;

	public Collider bounds;

	public GameObject doftarget;

	public Vector3 dofpos;

	public float doftime = 1f;

	public Vector3 dofvel = Vector3.zero;

	public Vector3 currentdofpos = Vector3.zero;

	public float dofspeed = 1f;

	public override string CameraName()
	{
		return "Free";
	}

	public override Vector3 GetTargetPos()
	{
		return Quaternion.Euler(cameraCurrentRot) * new Vector3(0f, 0f, 1f) + cameraCurrentPos;
	}

	public override void Rollem(GameCamera cam)
	{
		cameraPos = (cameraCurrentPos = cam.GetCamPos());
		cameraRot = (cameraCurrentRot = cam.GetCamRot());
		cameraFOV = (cameraCurrentFOV = cam.GetFOV());
	}

	public override void CameraUpdate(CameraManager camman)
	{
		float num = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
		Vector3 zero = Vector3.zero;
		float num2 = cameraCurrentFOV / 40f;
		float mx = CameraManager.MouseDelta.x * num2;
		float my = CameraManager.MouseDelta.y * num2;
		LimitMouse(ref mx, ref my);
		if (Input.GetKey(ForwardKey) || Input.GetMouseButton(0))
		{
			zero.z = freeSpeed.z;
		}
		if (Input.GetKey(BackwardKey) || Input.GetMouseButton(1))
		{
			zero.z = 0f - freeSpeed.z;
		}
		if (Input.GetKey(LeftKey))
		{
			zero.x = 0f - freeSpeed.x;
		}
		if (Input.GetKey(RightKey))
		{
			zero.x = freeSpeed.x;
		}
		if (Input.GetKey(UpKey))
		{
			zero.y = freeSpeed.y;
		}
		if (Input.GetKey(DownKey))
		{
			zero.y = 0f - freeSpeed.y;
		}
		cameraPos += base.transform.TransformDirection(zero * num);
		float trg = cameraCurrentRot.x - my * XSpeed;
		float trg2 = cameraCurrentRot.y + mx * YSpeed;
		Vector3 prev = cameraCurrentPos;
		cameraCurrentPos = Vector3.SmoothDamp(cameraCurrentPos, cameraPos, ref cwvel, cameraEaseTime);
		CameraHit(cameraCurrentPos, prev);
		cameraCurrentRot.x = MegaEase.SpringDamp(cameraCurrentRot.x, trg, ref vx, Time.deltaTime, Tau, Critical);
		cameraCurrentRot.y = MegaEase.SpringDamp(cameraCurrentRot.y, trg2, ref vy, Time.deltaTime, Tau, Critical);
		cameraFOV += (0f - CameraManager.MouseDelta.z) * ZSpeed;
		cameraCurrentFOV = Mathf.SmoothDamp(cameraCurrentFOV, cameraFOV, ref fovvel, 0.25f);
	}

	private Vector3 CameraHit(Vector3 pos, Vector3 prev)
	{
		if ((bool)doftarget)
		{
			Ray ray = new Ray
			{
				origin = Camera.main.transform.position,
				direction = Camera.main.transform.forward
			};
			RaycastHit[] array = Physics.RaycastAll(ray.origin, ray.direction);
			if (array.Length != 0)
			{
				float distance = array[0].distance;
				int num = 0;
				for (int i = 1; i < array.Length; i++)
				{
					if (array[i].distance < distance)
					{
						num = i;
						distance = array[i].distance;
					}
				}
				dofpos = array[num].point;
				if (distance < 0.4f)
				{
					pos = ray.GetPoint(array[num].distance - 0.4f);
					cameraPos = pos;
					cwvel = Vector3.zero;
					cameraCurrentPos = pos;
				}
			}
			currentdofpos = Vector3.SmoothDamp(currentdofpos, dofpos, ref dofvel, doftime);
			doftarget.transform.position = currentdofpos;
		}
		return pos;
	}

	public override void UpdateFOV()
	{
	}

	private void OnGUI()
	{
		float num = 1f / Time.smoothDeltaTime;
		GUI.Label(new Rect(0f, 0f, 100f, 32f), num.ToString("0.0"));
	}
}
