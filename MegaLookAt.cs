using UnityEngine;

public class MegaLookAt : MonoBehaviour
{
	public Transform target;

	private void LateUpdate()
	{
		if ((bool)target)
		{
			base.transform.LookAt(target);
		}
	}
}
