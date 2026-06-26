using System.Collections;
using UnityEngine;

public class TriggerImpulse : MonoBehaviour
{
	private Vector3 initialPos;

	private Quaternion initialRot;

	private bool initialized;

	public float delayTime;

	public float randomDelayTime;

	public Vector3 impulseDirection1;

	public Vector3 impulseDirection2;

	public Vector3 impulseOffset;

	public float impulseStrength = 25f;

	public float randomImpulseStrength;

	private void Awake()
	{
		initialPos = base.transform.localPosition;
		initialRot = base.transform.localRotation;
		initialized = true;
	}

	private void FixedUpdate()
	{
		if (base.transform.position.y < -100f)
		{
			base.rigidbody.isKinematic = true;
		}
	}

	private void CheckpointTrigger()
	{
		StartCoroutine("Delay");
	}

	private void CheckpointReset()
	{
		if (initialized)
		{
			StopCoroutine("Delay");
			base.rigidbody.isKinematic = true;
			base.transform.localPosition = initialPos;
			base.transform.localRotation = initialRot;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(base.transform.position + impulseOffset, 0.1f);
		Gizmos.DrawLine(base.transform.position + impulseOffset, base.transform.position + impulseOffset + base.transform.TransformDirection(impulseDirection1));
		Gizmos.DrawLine(base.transform.position + impulseOffset, base.transform.position + impulseOffset + base.transform.TransformDirection(impulseDirection2));
	}

	private IEnumerator Delay()
	{
		float num = Random.value * randomDelayTime;
		yield return new WaitForSeconds(delayTime + num);
		Vector3 vector = new Vector3(Random.Range(impulseDirection1.x, impulseDirection2.x), Random.Range(impulseDirection1.y, impulseDirection2.y), Random.Range(impulseDirection1.z, impulseDirection2.z));
		base.rigidbody.isKinematic = false;
		float num2 = Random.value * randomImpulseStrength;
		base.rigidbody.AddForceAtPosition(base.transform.TransformDirection(vector.normalized) * (impulseStrength + num2), base.transform.position + impulseOffset, ForceMode.Impulse);
	}
}
