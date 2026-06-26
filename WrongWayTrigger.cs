using UnityEngine;

public class WrongWayTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		PlatformerController component = other.transform.root.GetComponent<PlatformerController>();
		if ((bool)component && !component.isExternalControlled)
		{
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.Barrier);
		}
	}
}
