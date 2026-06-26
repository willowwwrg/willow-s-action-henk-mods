using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
	private List<GameObject> prevFramePlayers = new List<GameObject>();

	private GameObject lavaSmokeParticles;

	private void Awake()
	{
		lavaSmokeParticles = Resources.Load("LavaDeathParticles") as GameObject;
	}

	private void FixedUpdate()
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] allPlayers = Singleton<PlayerManager>.SP.GetAllPlayers();
		foreach (GameObject gameObject in allPlayers)
		{
			RaycastCollider component = gameObject.GetComponent<RaycastCollider>();
			float radius = component.radius;
			if (!(gameObject.transform.position.y < base.transform.position.y + radius))
			{
				continue;
			}
			list.Add(gameObject);
			if (!prevFramePlayers.Contains(gameObject))
			{
				(Object.Instantiate(lavaSmokeParticles) as GameObject).transform.position = gameObject.GetComponent<PlayerGraphics>().animatedModel.transform.position;
				if (Singleton<PlayerManager>.SP.IsLocalPlayer(gameObject))
				{
					AdditionalSkinParticles componentInChildren = Singleton<PlayerManager>.SP.GetPlayer().GetComponentInChildren<AdditionalSkinParticles>();
					if (componentInChildren != null)
					{
						componentInChildren.Play(SfxEvents.LavaDeath);
					}
					Singleton<AudioManager>.SP.PlayCharacterLavaDeath(gameObject);
					Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.PlayerDeath, string.Empty);
					gameObject.BroadcastMessage("OnEventLava", SendMessageOptions.DontRequireReceiver);
				}
			}
			if ((bool)gameObject.GetComponent<GrapplingHook>() && gameObject.GetComponent<GrapplingHook>().IsEnabled())
			{
				gameObject.GetComponent<GrapplingHook>().DisableAbility(forced: true);
			}
			component.velocity -= component.velocity * 20f * Time.fixedDeltaTime;
			if (gameObject.GetComponent<PlatformerController>().isExternalControlled)
			{
				continue;
			}
			if (gameObject.transform.position.y < base.transform.position.y + radius - 0.2f)
			{
				Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Lava);
			}
			if (gameObject.transform.position.y < base.transform.position.y - 2.4f)
			{
				Singleton<PlayerManager>.SP.ResetPlayer(gameObject, hard: false);
				AdditionalSkinParticles componentInChildren2 = Singleton<PlayerManager>.SP.GetPlayer().GetComponentInChildren<AdditionalSkinParticles>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.Play(SfxEvents.ControlEnabled);
				}
			}
		}
		prevFramePlayers.Clear();
		prevFramePlayers.AddRange(list);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(base.transform.position, new Vector3(500f, 0f, 500f));
	}
}
