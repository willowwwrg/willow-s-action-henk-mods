using UnityEngine;

public class AccelEffect : MonoBehaviour
{
	public Vector3 acceleration = new Vector3(0f, 0f, 1f);

	public float friction = 1f;

	private Vector3 vel;

	private bool started;

	private Vector3 startPos;

	public void Start()
	{
		startPos = base.transform.position;
	}

	public void Update()
	{
		if (started)
		{
			vel += acceleration * Time.deltaTime;
			vel -= friction * vel * Time.deltaTime;
			base.transform.Translate(vel * Time.deltaTime, Space.Self);
		}
	}

	private void CheckpointTrigger()
	{
		started = true;
	}

	private void CheckpointReset()
	{
		started = false;
		base.transform.position = startPos;
		vel = Vector3.zero;
	}
}
