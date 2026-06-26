using UnityEngine;

public class Rotation : MonoBehaviour
{
	[SerializeField]
	private float m_angle = 5f;

	[SerializeField]
	private bool m_useLocalYAxis = true;

	[SerializeField]
	private Vector3 m_axis = Vector3.up;

	private void Update()
	{
		if (m_useLocalYAxis)
		{
			base.transform.rotation *= Quaternion.AngleAxis(m_angle * Time.deltaTime, base.transform.up);
		}
		else
		{
			base.transform.rotation *= Quaternion.AngleAxis(m_angle * Time.deltaTime, m_axis);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (m_useLocalYAxis)
		{
			Gizmos.DrawRay(base.transform.position, base.transform.up * 10f);
		}
		else
		{
			Gizmos.DrawRay(base.transform.position, m_axis * 10f);
		}
	}
}
