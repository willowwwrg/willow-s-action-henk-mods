using System.Collections;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
	public float selfDestructTimer = 1f;

	private void Awake()
	{
		StartCoroutine(DestructTimer());
	}

	private IEnumerator DestructTimer()
	{
		yield return new WaitForSeconds(selfDestructTimer);
		Object.Destroy(base.gameObject);
	}
}
