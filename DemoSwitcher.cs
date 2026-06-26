using UnityEngine;

public class DemoSwitcher : MonoBehaviour
{
	public Transform cameraTransform;

	public Transform[] camPosTransfroms;

	public Transform target;

	public float speed = 3f;

	public int index;

	private void Start()
	{
		base.transform.position = camPosTransfroms[0].position;
		base.transform.rotation = camPosTransfroms[0].rotation;
		target = camPosTransfroms[0];
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 100f, 30f), "Last"))
		{
			index--;
			if (index < 0)
			{
				index = camPosTransfroms.Length - 1;
			}
			target = camPosTransfroms[index];
		}
		if (GUI.Button(new Rect(Screen.width - 110, 10f, 100f, 30f), "Next"))
		{
			index++;
			index %= camPosTransfroms.Length;
			target = camPosTransfroms[index];
		}
	}

	private void Update()
	{
		base.transform.position = Vector3.Slerp(base.transform.position, target.position, Time.deltaTime * speed);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, target.rotation, Time.deltaTime * speed);
	}
}
