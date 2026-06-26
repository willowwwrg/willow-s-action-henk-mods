using UnityEngine;

public class ShootDemo : MonoBehaviour
{
	public GameObject BulletPrefab01;

	public GameObject BulletPrefab02;

	public GameObject BulletPrefab03;

	private GameObject SelectedBullet;

	private void Start()
	{
		SelectedBullet = BulletPrefab01;
		Screen.showCursor = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SelectedBullet = BulletPrefab01;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SelectedBullet = BulletPrefab02;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			SelectedBullet = BulletPrefab03;
		}
		if (Input.GetMouseButtonDown(0))
		{
			GameObject gameObject = Object.Instantiate(SelectedBullet, Camera.main.transform.position, Camera.main.transform.rotation) as GameObject;
			if (SelectedBullet == BulletPrefab01)
			{
				gameObject.rigidbody.AddForce(gameObject.transform.forward * 70f);
			}
			if (SelectedBullet == BulletPrefab02)
			{
				gameObject.rigidbody.AddForce(gameObject.transform.forward * 140f);
			}
			if (SelectedBullet == BulletPrefab03)
			{
				gameObject.rigidbody.AddForce(gameObject.transform.forward * 210f);
			}
		}
	}

	private void FixedUpdate()
	{
	}
}
