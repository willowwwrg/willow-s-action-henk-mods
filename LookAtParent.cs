using UnityEngine;

public class LookAtParent : MonoBehaviour
{
	private void Update()
	{
		base.transform.LookAt(base.transform.parent);
	}
}
