using UnityEngine;

public class MetalBarrel : MonoBehaviour
{
	public GameObject[] Chunks;

	[Range(0f, 30f)]
	public float BurningTime = 3f;

	[Range(1f, 100f)]
	public int Health = 100;

	public float ExplosionForce = 200f;

	public float ChunksRotation = 20f;

	public bool BreakByClick = true;

	public bool DestroyAfterTime = true;

	public float time = 5f;

	public GameObject ExpLight;

	public GameObject FireFX;

	private void Start()
	{
		FireFX.SetActive(value: false);
		if ((bool)GetComponent<AudioSource>())
		{
			base.audio.pitch = Random.Range(1f, 1.3f);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if ((bool)other.gameObject.GetComponent<BulletsDamage>())
		{
			Health -= other.gameObject.GetComponent<BulletsDamage>().Damage;
			if (Health <= 0 && !FireFX.activeInHierarchy)
			{
				FireFX.SetActive(value: true);
				Invoke("Crushing", BurningTime);
			}
		}
		else if (other.gameObject.tag == "BarrelChunk" && !FireFX.activeInHierarchy)
		{
			Health = 0;
			FireFX.SetActive(value: true);
			Invoke("Crushing", BurningTime);
		}
	}

	private void FixedUpdate()
	{
		if ((bool)ExpLight && ExpLight.light.intensity > 0f && !base.renderer.enabled)
		{
			ExpLight.light.intensity -= 0.3f;
		}
	}

	private void OnMouseDown()
	{
		if (BreakByClick)
		{
			Health = 0;
			FireFX.SetActive(value: true);
			Invoke("Crushing", BurningTime);
			BreakByClick = false;
		}
	}

	private void Crushing()
	{
		FireFX.SetActive(value: false);
		base.renderer.enabled = false;
		base.collider.enabled = false;
		base.rigidbody.isKinematic = true;
		GameObject[] chunks = Chunks;
		foreach (GameObject obj in chunks)
		{
			obj.SetActive(value: true);
			obj.rigidbody.AddRelativeForce(Vector3.forward * (0f - ExplosionForce));
			obj.rigidbody.AddRelativeTorque(Vector3.forward * (0f - ChunksRotation) * Random.Range(-5f, 5f));
			obj.rigidbody.AddRelativeTorque(Vector3.right * (0f - ChunksRotation) * Random.Range(-5f, 5f));
		}
		if (DestroyAfterTime)
		{
			Invoke("DestructObject", time);
		}
	}

	private void DestructObject()
	{
		Object.Destroy(base.gameObject);
	}
}
