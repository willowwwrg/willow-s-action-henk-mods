using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
	private void LateUpdate()
	{
		if ((bool)Singleton<PlayerManager>.SP.GetPlayer())
		{
			base.transform.LookAt(Singleton<PlayerManager>.SP.GetPlayer().GetComponent<PlayerGraphics>().physicsInterpolator);
		}
	}
}
