using UnityEngine;

public class MB_ExampleMover : MonoBehaviour
{
	public int axis;

	private void Update()
	{
		Vector3 position = new Vector3(5f, 5f, 5f);
		int index2;
		int index = (index2 = axis);
		float num = position[index2];
		position[index] = num * Mathf.Sin(Time.time);
		base.transform.position = position;
	}
}
