using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
	[HideInInspector]
	public string targetString = string.Empty;

	public Transform target;

	private void OnTriggerEnter(Collider other)
	{
		if (!target && targetString != string.Empty)
		{
			GameObject gameObject = GameObject.Find(targetString);
			if ((bool)gameObject)
			{
				target = gameObject.transform;
			}
		}
		if ((bool)target)
		{
			target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnDrawGizmos()
	{
		if ((bool)target)
		{
			Debug.DrawLine(base.transform.position, target.position, Color.white);
		}
	}
}
