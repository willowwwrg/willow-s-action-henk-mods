using UnityEngine;

public class TutorialPlane : MonoBehaviour
{
	private bool started;

	private float startVelocity = -78f;

	private float targetVelocity = -35f;

	private float velocity;

	public float extraDist = 5f;

	private Vector3 startPos = Vector3.zero;

	private void Start()
	{
		base.transform.GetChild(0).animation["Take 001"].speed = 3f;
		startPos = base.transform.position;
	}

	public void OnReset()
	{
		if (startPos != Vector3.zero)
		{
			started = false;
			base.transform.position = startPos;
		}
	}

	private void Update()
	{
		if (started)
		{
			Vector3 lhs = base.transform.position - Camera.main.transform.position;
			Vector3 vector = Camera.main.transform.forward * Vector3.Dot(lhs, Camera.main.transform.forward);
			Vector3 vector2 = Camera.main.transform.position + vector;
			Debug.DrawLine(Camera.main.transform.position, vector2);
			float num = Vector3.Dot(vector2 - base.transform.position, base.transform.forward) + extraDist;
			if (num < 0f)
			{
				Vector3 vector3 = base.transform.position + base.transform.forward * num;
				Debug.DrawLine(base.transform.position, vector3);
				base.transform.position -= (base.transform.position - vector3) * 2.5f * Time.deltaTime;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Singleton<PlayerManager>.SP.IsLocalPlayer(other.transform.root.gameObject))
		{
			started = true;
			velocity = startVelocity;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position - base.transform.forward * 500f);
	}
}
