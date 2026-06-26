using UnityEngine;

public class Killzone_Trigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		PlatformerController component = other.transform.root.GetComponent<PlatformerController>();
		if ((bool)component && !component.isExternalControlled)
		{
			if (Singleton<PlayerManager>.SP.IsLocalPlayer(component.gameObject))
			{
				Singleton<AudioManager>.SP.PlayCharacterLavaDeath(component.gameObject);
			}
			Singleton<PlayerManager>.SP.ResetPlayer(component.gameObject, hard: false);
		}
	}
}
