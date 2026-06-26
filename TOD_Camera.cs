using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Main Script")]
public class TOD_Camera : MonoBehaviour
{
	public TOD_Sky sky;

	public bool DomePosToCamera = true;

	public bool DomeScaleToFarClip;

	public float DomeScaleFactor = 0.95f;

	protected void OnPreCull()
	{
		if ((bool)sky)
		{
			if (DomeScaleToFarClip)
			{
				float num = DomeScaleFactor * base.camera.farClipPlane;
				sky.transform.localScale = new Vector3(num, num, num);
			}
			if (DomePosToCamera)
			{
				sky.transform.position = base.transform.position;
			}
		}
	}
}
