using UnityEngine;

public class PPFXClickMove : MonoBehaviour
{
	public float speed = 5f;

	public string tagName = "plane";

	private Vector3 pos = new Vector3(0f, 0f, 0f);

	private void Start()
	{
		pos = base.transform.position;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo) && hitInfo.collider.tag == tagName)
		{
			pos = hitInfo.point;
		}
		float maxDistanceDelta = speed * Time.deltaTime;
		base.transform.position = Vector3.MoveTowards(base.transform.position, pos, maxDistanceDelta);
	}
}
