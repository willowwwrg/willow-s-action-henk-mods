using System.Collections;
using UnityEngine;

public class PPFXPhysicForce : MonoBehaviour
{
	public float radius = 10f;

	public float force = 10f;

	public float delay = 0.2f;

	private Collider[] colliders;

	private void Start()
	{
		colliders = Physics.OverlapSphere(base.transform.position, radius);
		StartCoroutine(Explode());
	}

	private IEnumerator Explode()
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].GetComponent<Rigidbody>() != null)
			{
				colliders[i].rigidbody.AddExplosionForce(force, base.transform.position, radius, new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), Random.Range(-3, 3)), ForceMode.Impulse);
			}
		}
		yield return null;
	}
}
