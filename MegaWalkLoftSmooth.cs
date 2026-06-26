using UnityEngine;

[ExecuteInEditMode]
public class MegaWalkLoftSmooth : MonoBehaviour
{
	public MegaShapeLoft surfaceLoft;

	public int surfaceLayer = -1;

	public float alpha;

	public float crossalpha;

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
		if ((bool)surfaceLoft && surfaceLayer >= 0 && surfaceLayer < surfaceLoft.Layers.Length)
		{
			MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
			if (animate)
			{
				distance += speed * Time.deltaTime;
				distance = Mathf.Repeat(distance, megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length);
				alpha = distance / megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			}
			if (mode == MegaWalkMode.Distance)
			{
				alpha = distance / megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			}
			else
			{
				distance = alpha * megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			}
			Vector3 vector = megaLoftLayerSimple.transform.TransformPoint(megaLoftLayerSimple.SampleSplines(surfaceLoft, crossalpha, alpha));
			Vector3 vector2 = megaLoftLayerSimple.transform.TransformPoint(megaLoftLayerSimple.SampleSplines(surfaceLoft, crossalpha + tangent, alpha));
			Vector3 normalized = (megaLoftLayerSimple.transform.TransformPoint(megaLoftLayerSimple.SampleSplines(surfaceLoft, crossalpha, alpha + tangent)) - vector).normalized;
			Vector3 normalized2 = (vector2 - vector).normalized;
			Vector3 vector3 = Vector3.Cross(normalized, normalized2);
			vector += vector3 * offset;
			Quaternion quaternion = Quaternion.LookRotation(normalized, vector3);
			Quaternion to = Quaternion.Euler(uprot);
			quaternion = Quaternion.Lerp(quaternion, to, upright);
			Quaternion quaternion2 = Quaternion.Euler(rotate);
			quaternion *= quaternion2;
			base.transform.rotation = quaternion;
			base.transform.position = vector;
		}
	}

	public Vector3 GetPos(float a, float ca)
	{
		if ((bool)surfaceLoft && surfaceLayer >= 0 && surfaceLayer < surfaceLoft.Layers.Length)
		{
			return ((MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer]).SampleSplines(surfaceLoft, ca, a);
		}
		return Vector3.zero;
	}

	public Vector3 GetPosDist(float dist, float ca)
	{
		if ((bool)surfaceLoft && surfaceLayer >= 0 && surfaceLayer < surfaceLoft.Layers.Length)
		{
			MegaLoftLayerSimple megaLoftLayerSimple = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
			float pa = dist / megaLoftLayerSimple.layerPath.splines[megaLoftLayerSimple.curve].length;
			return megaLoftLayerSimple.SampleSplines(surfaceLoft, ca, pa);
		}
		return Vector3.zero;
	}
}
