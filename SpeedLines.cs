using UnityEngine;

public class SpeedLines : MonoBehaviour
{
	public Vector3 extraRotation = Vector3.zero;

	private RaycastCollider playerCollider;

	public float speedLinesDecay = 5f;

	private float speedLinesVal;

	private void Start()
	{
		if (!base.transform.root.GetComponent<PlatformerPhysics>())
		{
			Debug.LogError("This class must be attached to a child of the player!");
		}
		playerCollider = base.transform.root.GetComponent<RaycastCollider>();
	}

	private void FixedUpdate()
	{
		Vector3 vector = playerCollider.velocity;
		if (vector == Vector3.zero)
		{
			vector = Vector3.forward;
		}
		Quaternion to = Quaternion.LookRotation(vector) * Quaternion.Euler(extraRotation);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, to, 10f * Time.fixedDeltaTime);
		speedLinesVal -= speedLinesVal * speedLinesDecay * Time.fixedDeltaTime;
		if (playerCollider.velocity.magnitude < 20f)
		{
			speedLinesVal -= speedLinesVal * speedLinesDecay * Time.fixedDeltaTime;
		}
		Color startColor = base.particleSystem.startColor;
		startColor.a = Mathf.Clamp01(speedLinesVal);
		base.particleSystem.startColor = startColor;
	}

	public void AddSpeedLines(float amount)
	{
		if (speedLinesVal < 0.1f)
		{
			base.transform.rotation = Quaternion.LookRotation(playerCollider.velocity) * Quaternion.Euler(extraRotation);
		}
		speedLinesVal += amount;
	}
}
