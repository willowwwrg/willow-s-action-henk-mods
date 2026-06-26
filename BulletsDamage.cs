using UnityEngine;

public class BulletsDamage : MonoBehaviour
{
	[Range(1f, 100f)]
	public int Damage = 34;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision other)
	{
		Object.Destroy(base.gameObject);
	}
}
