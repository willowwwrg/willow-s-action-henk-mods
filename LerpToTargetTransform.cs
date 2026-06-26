using UnityEngine;

public class LerpToTargetTransform : MonoBehaviour
{
	public Vector3 target;

	public Vector3 initialPos;

	public float lerpFactor;

	private void Awake()
	{
		initialPos = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z);
		target = initialPos;
	}

	private void Update()
	{
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, target, Time.deltaTime * lerpFactor);
	}

	public void DisplaceFromOrigin(Vector3 displacement, bool hard = false)
	{
		target = initialPos + displacement;
		if (hard)
		{
			base.transform.localPosition = target;
		}
	}

	public void SetTarget(Vector3 targetPos, bool hard = false)
	{
		target = targetPos;
		if (hard)
		{
			base.transform.localPosition = target;
		}
	}

	public void Default(bool hard = false)
	{
		target = initialPos;
		if (hard)
		{
			base.transform.localPosition = target;
		}
	}
}
