using UnityEngine;

public class Wobbler : MonoBehaviour
{
	private float startY;

	public bool useLocalHeight;

	public float heightBobbing = 0.25f;

	public float heightBobbingSpeed = 2f;

	public float rotationWobbleSpeed = 2f;

	public Vector3 rotationWobble = Vector3.zero;

	private Vector3 startAngles;

	private float startOffset;

	private void Start()
	{
		startY = base.transform.localPosition.y;
		if (!useLocalHeight)
		{
			startY = base.transform.position.y;
		}
		startAngles = base.transform.localEulerAngles;
		startOffset = Random.value * 10f;
	}

	private void Update()
	{
		Vector3 vector = base.transform.position;
		if (useLocalHeight)
		{
			vector = base.transform.localPosition;
		}
		vector.y = startY + Singleton<SineLookUp>.SP.GetSin(Time.time * heightBobbingSpeed + startOffset) * heightBobbing;
		if (useLocalHeight)
		{
			base.transform.localPosition = vector;
		}
		else
		{
			base.transform.position = vector;
		}
		if (rotationWobble != Vector3.zero)
		{
			float sin = Singleton<SineLookUp>.SP.GetSin(Time.time * rotationWobbleSpeed + startOffset);
			Vector3 vector2 = rotationWobble * sin;
			base.transform.localEulerAngles = vector2 + startAngles;
		}
	}
}
