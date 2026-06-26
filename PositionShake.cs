using UnityEngine;

public class PositionShake : MonoBehaviour
{
	public float shakeAmount;

	public bool isControlledExternally;

	private Vector3 startPos;

	private void Start()
	{
		startPos = base.transform.localPosition;
	}

	private void LateUpdate()
	{
		if (!isControlledExternally)
		{
			base.transform.localPosition = startPos;
		}
		base.transform.Translate(Random.value * shakeAmount, Random.value * shakeAmount, Random.value * shakeAmount);
	}

	private void FixedUpdate()
	{
	}
}
