using UnityEngine;

public class MegaMovePlayer : MonoBehaviour
{
	public float distance;

	public float speed = 1f;

	public MegaShapeFollow follow;

	private void Start()
	{
		if (!follow)
		{
			follow = GetComponent<MegaShapeFollow>();
		}
		if ((bool)follow)
		{
			follow.mode = MegaFollowMode.Distance;
		}
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			distance += speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S))
		{
			distance -= speed * Time.deltaTime;
		}
		if ((bool)follow)
		{
			follow.distance = distance;
		}
	}
}
