using UnityEngine;

public class ActivatingDemo : MonoBehaviour
{
	public GameObject nextObj;

	public float _time = 5f;

	private void Start()
	{
		Invoke("ShowNext", _time);
	}

	private void ShowNext()
	{
		nextObj.SetActive(value: true);
		base.gameObject.SetActive(value: false);
	}

	private void FixedUpdate()
	{
	}
}
