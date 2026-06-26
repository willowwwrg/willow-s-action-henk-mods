using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExplodeOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		StartExplosion();
	}

	private void StartExplosion()
	{
		BroadcastMessage("Explode");
		Object.Destroy(base.gameObject);
	}
}
