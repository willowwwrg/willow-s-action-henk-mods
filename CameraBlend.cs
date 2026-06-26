using UnityEngine;

public class CameraBlend : MonoBehaviour
{
	public Transform[] cameras;

	public float blendFactor;

	public float cameraShake;

	public float blendMoveSpeed = 2f;

	private float blendSpeedLerp;

	private float targetBlend;

	private void Start()
	{
		if (cameras.Length == 0)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			targetBlend -= 1f;
			blendSpeedLerp = 0f;
		}
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			blendSpeedLerp = 0f;
			targetBlend += 1f;
		}
		blendSpeedLerp = Mathf.Lerp(blendSpeedLerp, 1f, blendMoveSpeed * Time.deltaTime);
		blendFactor = Mathf.Lerp(blendFactor, targetBlend, blendMoveSpeed * blendSpeedLerp * Time.deltaTime);
		if (blendFactor >= (float)(cameras.Length - 1))
		{
			blendFactor = (float)cameras.Length - 1.0001f;
			targetBlend = blendFactor;
		}
		if (blendFactor < 0f)
		{
			blendFactor = 0f;
			targetBlend = 0f;
		}
		int num = Mathf.FloorToInt(blendFactor);
		float t = blendFactor - Mathf.Floor(blendFactor);
		Transform transform = cameras[num];
		Transform transform2 = cameras[num + 1];
		base.transform.rotation = Quaternion.Slerp(transform.rotation, transform2.rotation, t);
		base.transform.position = Vector3.Lerp(transform.position, transform2.position, t);
		base.transform.Rotate(Random.value * cameraShake, Random.value * cameraShake, Random.value * cameraShake);
	}
}
