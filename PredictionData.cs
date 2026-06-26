using UnityEngine;

public class PredictionData
{
	public Vector3 position;

	public Vector3 velocity;

	public bool isEvent;

	public PredictionData(Vector3 pos, Vector3 vel)
	{
		position = pos;
		velocity = vel;
		isEvent = false;
	}
}
