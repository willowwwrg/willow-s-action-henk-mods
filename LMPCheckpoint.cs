using UnityEngine;

public class LMPCheckpoint : MonoBehaviour
{
	public bool firstCheckpoint;

	public LMPCheckpoint next;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (firstCheckpoint)
		{
			Gizmos.color = Color.green;
		}
		Gizmos.DrawSphere(base.transform.position, 0.5f);
		Gizmos.color = Color.white;
		if (next != null)
		{
			Gizmos.DrawLine(base.transform.position, next.transform.position);
		}
	}
}
