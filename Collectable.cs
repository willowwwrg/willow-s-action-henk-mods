using UnityEngine;

public class Collectable : MonoBehaviour
{
	[HideInInspector]
	public bool isPickedUp;

	private GameObject pickupParticle;

	private void Start()
	{
		pickupParticle = Resources.Load("Particles/CoinParticle") as GameObject;
	}

	public void OnReset()
	{
		isPickedUp = false;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Transform root = other.transform.root;
		if (Singleton<PlayerManager>.SP.IsLocalPlayer(root.gameObject) && !isPickedUp)
		{
			isPickedUp = true;
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			(Object.Instantiate(pickupParticle) as GameObject).transform.position = base.transform.position;
			AudioController.Play("PickupCoin");
			// Record bonus split
			Singleton<BonusSplitManager>.SP.RecordLocalCoin(
				gameObject.GetInstanceID(),
				Singleton<Henk.Stopwatch>.SP.GetCurrentTime());
		}
		else if (Singleton<PlayerManager>.SP.IsGhost(root.gameObject) && !isPickedUp)
		{
			// Record ghost coin time for split comparison
			Singleton<BonusSplitManager>.SP.RecordGhostCoin(
				gameObject.GetInstanceID(),
				Singleton<Henk.Stopwatch>.SP.GetCurrentTime());
		}
	}
}
