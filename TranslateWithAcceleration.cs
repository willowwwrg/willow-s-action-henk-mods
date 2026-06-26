using UnityEngine;

public class TranslateWithAcceleration : MonoBehaviour
{
	public Vector3 acceleration = new Vector3(0f, 0f, 1f);

	public float friction = 1f;

	private Vector3 vel;

	public void Update()
	{
		vel += acceleration * Time.deltaTime;
		vel -= friction * vel * Time.deltaTime;
		base.transform.Translate(vel * Time.deltaTime, Space.Self);
	}
}
