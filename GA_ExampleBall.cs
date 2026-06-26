using UnityEngine;

public class GA_ExampleBall : MonoBehaviour
{
	public float Speed = 1f;

	private void Start()
	{
		base.rigidbody.velocity = new Vector3(Random.value * 0.2f - 0.1f, -1f, 0f) * Speed;
	}

	private void Update()
	{
		base.rigidbody.AddForce(Vector3.down * 0.0001f);
		base.rigidbody.velocity = base.rigidbody.velocity.normalized * Speed;
	}
}
