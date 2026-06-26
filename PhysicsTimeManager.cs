using UnityEngine;

public class PhysicsTimeManager : Singleton<PhysicsTimeManager>
{
	private float timeAccumulator;

	private int nextUpdateAccumulatorShouldSurpass;

	public float interpolationValue;

	private void FixedUpdate()
	{
		nextUpdateAccumulatorShouldSurpass++;
	}

	private void Update()
	{
		timeAccumulator += Time.deltaTime;
		if ((int)(timeAccumulator / Time.fixedDeltaTime) < nextUpdateAccumulatorShouldSurpass)
		{
			timeAccumulator = 0f;
		}
		timeAccumulator = Mathf.Repeat(timeAccumulator, Time.fixedDeltaTime);
		nextUpdateAccumulatorShouldSurpass = 0;
		interpolationValue = timeAccumulator / Time.fixedDeltaTime;
	}
}
