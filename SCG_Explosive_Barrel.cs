using UnityEngine;

public class SCG_Explosive_Barrel : MonoBehaviour
{
	public AudioClip explodeSound;

	public ParticleSystem explodeParticles;

	public GameObject barrelGibs;

	public float gibLifeTime = 3f;

	public float explosionForce = 40f;

	public GameObject[] lodMeshes;

	private AudioSource aSource;

	private float timer;

	private GameObject gibs;

	private bool moveDown;

	private bool isExploded;

	private void Start()
	{
		aSource = base.gameObject.AddComponent<AudioSource>();
		aSource.clip = explodeSound;
		aSource.playOnAwake = false;
		aSource.loop = false;
	}

	private void ExplodeBarrel()
	{
		if (isExploded)
		{
			return;
		}
		if (explodeSound != null)
		{
			aSource.Play();
		}
		else
		{
			Debug.Log("No sound connected to Explosive barrel. Silent explosion!");
		}
		if (barrelGibs != null)
		{
			gibs = Object.Instantiate(barrelGibs, base.transform.position, Quaternion.identity) as GameObject;
			Transform transform = gibs.transform;
			transform.parent = base.transform;
			transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).rigidbody.AddExplosionForce(explosionForce, base.transform.position, 1f);
			}
			isExploded = true;
			Invoke("DestroyRigidbody", gibLifeTime);
		}
		else
		{
			Debug.Log("No gibs setup. Please add one!");
		}
		if (explodeParticles != null)
		{
			ParticleSystem obj = Object.Instantiate(explodeParticles, base.gameObject.transform.position, Quaternion.identity) as ParticleSystem;
			obj.Emit(100);
			obj.transform.parent = base.transform;
		}
		else
		{
			Debug.Log("Particle effects for Explosive Barrel not found");
		}
		base.gameObject.renderer.enabled = false;
		if (lodMeshes.Length != 0)
		{
			for (int j = 0; j < lodMeshes.Length; j++)
			{
				lodMeshes[j].renderer.enabled = false;
			}
		}
		else
		{
			Debug.Log("Lod meshes not set up. This may cause graphical errors, if object has lods.");
		}
	}

	private void DestroyRigidbody()
	{
		Transform transform = gibs.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Object.Destroy(transform.GetChild(i).rigidbody);
		}
		moveDown = true;
	}

	private void Update()
	{
		if (moveDown)
		{
			gibs.transform.Translate(Vector3.down * 0.005f);
			if (gibs.transform.localPosition.y < -0.3f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			ExplodeBarrel();
		}
	}
}
