using UnityEngine;

public class PPFXRandomPosition : MonoBehaviour
{
	public Vector2 amplitude;

	public Vector2 turbulence;

	public bool xAxis;

	public bool yAxis;

	private Transform target;

	private float amp;

	private float turb;

	private void Start()
	{
		target = base.transform;
		amp = Random.Range(amplitude.x, amplitude.y);
		turb = Random.Range(turbulence.x, turbulence.y);
	}

	private void Update()
	{
		if (xAxis)
		{
			base.transform.Translate(Vector3.right * Mathf.Sin(turb * Time.time) * amp);
		}
		if (yAxis)
		{
			base.transform.Translate(Vector3.up * Mathf.Sin(turb * Time.time) * amp);
		}
		base.transform.LookAt(target.transform.position);
	}
}
