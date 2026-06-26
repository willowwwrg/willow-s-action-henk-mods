using System.Collections;
using UnityEngine;

public class PPFXMeteor : MonoBehaviour
{
	private Vector3 groundPos = new Vector3(0f, 0f, 0f);

	public Vector3 spawnPosOffset = new Vector3(0f, 0f, 0f);

	public float speed = 10f;

	public GameObject detonationPrefab;

	public bool destroyOnHit;

	public bool setRateToNull;

	private float dist;

	private float radius = 2f;

	private ParticleSystem[] psystems;

	private void Start()
	{
		groundPos = base.transform.position;
		base.transform.position = base.transform.position + spawnPosOffset;
		dist = Vector3.Distance(base.transform.position, groundPos);
		StartCoroutine(Move());
	}

	private IEnumerator Move()
	{
		psystems = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = psystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].emissionRate *= speed / 10f;
		}
		while (dist > radius)
		{
			float maxDistanceDelta = speed * Time.deltaTime;
			base.transform.position = Vector3.MoveTowards(base.transform.position, groundPos, maxDistanceDelta);
			dist = Vector3.Distance(base.transform.position, groundPos);
			yield return null;
		}
		if (destroyOnHit)
		{
			Object.Destroy(base.gameObject);
		}
		else if (setRateToNull)
		{
			ParticleSystem[] array2 = psystems;
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].emissionRate = 0f;
			}
			GetComponent<PPFXAutodestruct>().DestroyPSystem(base.gameObject);
		}
		else
		{
			ParticleSystem[] array3 = psystems;
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k].emissionRate /= speed / 10f;
			}
		}
		if (detonationPrefab != null)
		{
			Object.Instantiate(detonationPrefab, base.transform.position, detonationPrefab.transform.rotation);
		}
	}
}
