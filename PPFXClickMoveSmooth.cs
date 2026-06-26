using System.Collections;
using UnityEngine;

public class PPFXClickMoveSmooth : MonoBehaviour
{
	public float speed = 5f;

	public string tagName = "plane";

	private float radius = 2f;

	private float dist;

	private bool anim;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo) && hitInfo.collider.tag == tagName)
		{
			dist = Vector3.Distance(base.transform.position, hitInfo.point);
			if (!anim)
			{
				anim = true;
				StartCoroutine(Move(hitInfo.point));
			}
		}
	}

	private IEnumerator Move(Vector3 _newPos)
	{
		_newPos = new Vector3(_newPos.x, 0f, _newPos.z);
		while (dist > radius)
		{
			float smoothTime = speed * Time.deltaTime;
			Vector3 currentVelocity = Vector3.zero;
			base.transform.position = Vector3.SmoothDamp(base.transform.position, _newPos, ref currentVelocity, smoothTime);
			dist = Vector3.Distance(base.transform.position, _newPos);
			yield return null;
		}
		anim = false;
	}
}
