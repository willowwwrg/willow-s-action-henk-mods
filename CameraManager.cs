using UnityEngine;

[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{
	[HideInInspector]
	public GameCamera currentCamera;

	[HideInInspector]
	public Vector3 cameraCurrentPos = Vector3.zero;

	[HideInInspector]
	public Vector3 cameraCurrentRot = Vector3.zero;

	[HideInInspector]
	public float cameraCurrentFOV;

	public GUIText cammode;

	public static Vector3 MouseDelta = Vector3.zero;

	private Vector3 cameraPos = Vector3.zero;

	private Vector3 cameraRot = Vector3.zero;

	private float cameraFOV;

	private GameCamera[] cameras;

	private bool Interactive = true;

	private void Start()
	{
		if (Application.isPlaying)
		{
			Screen.lockCursor = Interactive;
		}
		cameraPos = base.transform.position;
		cameraCurrentPos = cameraPos;
		cameraRot = base.transform.eulerAngles;
		cameraCurrentRot = cameraRot;
		cameraFOV = Camera.main.fieldOfView;
		cameraCurrentFOV = cameraFOV;
		Input.ResetInputAxes();
		cameras = GetComponents<GameCamera>();
		for (int i = 0; i < cameras.Length; i++)
		{
			cameraPos = base.transform.position;
			cameraCurrentPos = cameraPos;
			cameraRot = base.transform.eulerAngles;
			cameraCurrentRot = cameraRot;
			cameras[i].cameraPos = base.transform.position;
			cameras[i].cameraCurrentPos = cameraPos;
			cameras[i].cameraRot = base.transform.eulerAngles;
			cameras[i].cameraCurrentRot = cameraRot;
			cameras[i].cameraCurrentFOV = cameras[i].cameraFOV;
			cameras[i].Init(this);
		}
		if (cameras.Length != 0)
		{
			currentCamera = cameras[0];
		}
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Interactive = !Interactive;
			Screen.lockCursor = Interactive;
		}
		if (cameras.Length == 0)
		{
			return;
		}
		if (Interactive)
		{
			MouseDelta.x = Input.GetAxis("Mouse X");
			MouseDelta.y = Input.GetAxis("Mouse Y");
			MouseDelta.z = Input.GetAxis("Mouse ScrollWheel");
		}
		else
		{
			MouseDelta = Vector3.zero;
		}
		if ((bool)currentCamera)
		{
			currentCamera.CameraUpdate(this);
			currentCamera.UpdateFOV();
			cameraCurrentFOV = currentCamera.GetFOV();
			cameraCurrentPos = currentCamera.GetCamPos();
			cameraCurrentRot = currentCamera.GetCamRot();
			Vector3 position = cameraCurrentPos;
			Vector3 eulerAngles = cameraCurrentRot;
			if (position.y < 0.1f)
			{
				position.y = 0.1f;
			}
			if (Application.isPlaying)
			{
				base.transform.position = position;
				base.transform.eulerAngles = eulerAngles;
				base.camera.fieldOfView = cameraCurrentFOV;
			}
		}
	}
}
