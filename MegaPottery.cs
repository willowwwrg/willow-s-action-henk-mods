using UnityEngine;

public class MegaPottery : MonoBehaviour
{
	public float speed;

	public float angle;

	public GameObject backdrop;

	private void Update()
	{
		angle += speed * Time.deltaTime;
		angle = Mathf.Repeat(angle, 360f);
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.y = angle;
		base.transform.eulerAngles = eulerAngles;
		_ = (bool)backdrop;
	}
}
