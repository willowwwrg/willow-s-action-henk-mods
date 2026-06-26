using UnityEngine;

[ExecuteInEditMode]
public class MegaWalkLoft : MonoBehaviour
{
	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public float alpha;

	public float crossalpha;

	public float delay;

	public float offset;

	public float tangent = 0.01f;

	public Vector3 rotate = Vector3.zero;

	public MegaWalkMode mode;

	public float distance;

	public bool lateupdate = true;

	public bool animate;

	public float speed;

	public float upright;

	public Vector3 uprot = Vector3.zero;

	public bool initrot = true;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2785");
	}

	private void Update()
	{
		if (!lateupdate)
		{
			DoUpdate();
		}
	}

	private void LateUpdate()
	{
		if (lateupdate)
		{
			DoUpdate();
		}
	}

	private void DoUpdate()
	{
		if ((bool)surfaceLoft && surfaceLayer >= 0)
		{
			MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
			if (animate)
			{
				distance += speed * Time.deltaTime;
				distance = Mathf.Repeat(distance, megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length);
				alpha = distance / megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			}
			Vector3 p = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;
			if (mode == MegaWalkMode.Distance)
			{
				alpha = distance / megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			}
			else
			{
				distance = alpha * megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			}
			Vector3 posAndFrame = megaLoftLayerSimple.GetPosAndFrame(surfaceLoft, crossalpha, alpha, tangent, out p, out up, out right, out fwd);
			posAndFrame += up * offset;
			Quaternion quaternion = Quaternion.LookRotation(fwd, up);
			Quaternion to = Quaternion.Euler(uprot);
			quaternion = Quaternion.Lerp(quaternion, to, upright);
			Quaternion quaternion2 = Quaternion.Euler(rotate);
			if (!initrot && delay != 0f)
			{
				quaternion = Quaternion.Slerp(base.transform.rotation, quaternion * quaternion2, Time.deltaTime * delay);
			}
			else
			{
				initrot = false;
				quaternion *= quaternion2;
			}
			base.transform.rotation = quaternion;
			base.transform.position = megaLoftLayerSimple.transform.TransformPoint(posAndFrame);
		}
	}
}
