using UnityEngine;

public class LerpToTargetRotation : MonoBehaviour
{
	public Vector3 currentRotation;

	private Vector3 targetRotation;

	public Vector3 from;

	public Vector3 to;

	public float lerpFactor = 2.5f;

	private void Start()
	{
		targetRotation = from;
		currentRotation = base.transform.localRotation.eulerAngles;
	}

	private void Update()
	{
		currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * lerpFactor);
		base.transform.localRotation = Quaternion.Euler(currentRotation);
	}

	public void NewGUIScreen()
	{
		ToggleFromTo();
	}

	private void ToggleFromTo()
	{
		if (targetRotation == from)
		{
			targetRotation = to;
		}
		else
		{
			targetRotation = from;
		}
	}
}
