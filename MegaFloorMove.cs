using UnityEngine;

[ExecuteInEditMode]
public class MegaFloorMove : MonoBehaviour
{
	public float speed;

	public float amp = 1f;

	public float time;

	public float ypos;

	public Vector3 pos = Vector3.zero;

	private void Start()
	{
		pos = base.transform.position;
	}

	private void Update()
	{
		time += Time.deltaTime * speed;
		Vector3 position = pos;
		position.y = ypos + Mathf.Sin(time) * amp;
		base.transform.position = position;
	}
}
