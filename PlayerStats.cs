using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	public const float unitsToCentimeters = 1.7f;

	private PlatformerController controller;

	private RaycastCollider playerCollider;

	private PlatformerPhysics physics;

	[HideInInspector]
	public float unitsMoved;

	[HideInInspector]
	public float unitsRunning;

	[HideInInspector]
	public float unitsSliding;

	[HideInInspector]
	public float secondsOfAirtime;

	private void Awake()
	{
		controller = GetComponent<PlatformerController>();
		playerCollider = GetComponent<RaycastCollider>();
		physics = GetComponent<PlatformerPhysics>();
		ResetStats();
	}

	private void FixedUpdate()
	{
		if (controller.isExternalControlled)
		{
			return;
		}
		float num = playerCollider.velocity.magnitude * Time.fixedDeltaTime;
		unitsMoved += num;
		if (physics.onGround)
		{
			if (physics.sliding)
			{
				unitsSliding += num;
			}
			else
			{
				unitsRunning += num;
			}
		}
		else
		{
			secondsOfAirtime += Time.fixedDeltaTime;
		}
	}

	public void ResetStats()
	{
		StartCoroutine(ResetStatsDelayed());
	}

	private IEnumerator ResetStatsDelayed()
	{
		yield return new WaitForSeconds(0.1f);
		unitsMoved = 0f;
		unitsRunning = 0f;
		unitsSliding = 0f;
		secondsOfAirtime = 0f;
	}
}
