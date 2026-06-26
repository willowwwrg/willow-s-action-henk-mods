using UnityEngine;

public class destruction : MonoBehaviour
{
	public GameObject[] Chunks;

	public GameObject[] HidingObjs;

	[Range(1f, 100f)]
	public int Health = 100;

	public float ExplosionForce = 200f;

	public float ChunksRotation = 20f;

	public float strength = 5f;

	public bool BreakByClick;

	public bool DestroyAftertime = true;

	public float time = 15f;

	public GameObject FX;

	private void Start()
	{
		if ((bool)GetComponent<AudioSource>())
		{
			base.audio.pitch = Random.Range(0.7f, 1.1f);
		}
		if (HidingObjs.Length != 0)
		{
			GameObject[] hidingObjs = HidingObjs;
			for (int i = 0; i < hidingObjs.Length; i++)
			{
				hidingObjs[i].SetActive(value: true);
			}
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if ((bool)other.gameObject.GetComponent<BulletsDamage>())
		{
			Health -= other.gameObject.GetComponent<BulletsDamage>().Damage;
			if (Health <= 0)
			{
				Crushing();
			}
		}
		else if (other.relativeVelocity.magnitude > strength)
		{
			Crushing();
		}
	}

	private void OnMouseDown()
	{
		if (BreakByClick)
		{
			Crushing();
			BreakByClick = false;
		}
	}

	private void Crushing()
	{
		GameObject[] hidingObjs;
		if (HidingObjs.Length != 0)
		{
			hidingObjs = HidingObjs;
			for (int i = 0; i < hidingObjs.Length; i++)
			{
				hidingObjs[i].SetActive(value: false);
			}
		}
		if ((bool)FX)
		{
			FX.SetActive(value: true);
		}
		if ((bool)GetComponent<AudioSource>())
		{
			base.audio.Play();
		}
		base.renderer.enabled = false;
		base.collider.enabled = false;
		base.rigidbody.isKinematic = true;
		hidingObjs = Chunks;
		GameObject[] array = hidingObjs;
		foreach (GameObject obj in array)
		{
			obj.SetActive(value: true);
			obj.rigidbody.AddRelativeForce(Vector3.forward * (0f - ExplosionForce));
			obj.rigidbody.AddRelativeTorque(Vector3.forward * (0f - ChunksRotation) * Random.Range(-5f, 5f));
			obj.rigidbody.AddRelativeTorque(Vector3.right * (0f - ChunksRotation) * Random.Range(-5f, 5f));
		}
		if (DestroyAftertime)
		{
			Invoke("DestructObject", time);
		}
	}

	private void DestructObject()
	{
		Object.Destroy(base.gameObject);
	}
}
