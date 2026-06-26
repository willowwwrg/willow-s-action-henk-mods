using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
	private RaycastCollider playerCollider;

	private PlayerGraphics graphics;

	private GameObject lavaSmokeParticles;

	private GameObject waterDeathParticle;

	private GameObject sandDeathParticle;

	private GameObject jungleDeathParticle;

	private GameObject cityDeathParticle;

	private GameObject snowDeathParticle;

	private int liquidLayer = 8192;

	private int solidLayer = 16384;

	[HideInInspector]
	public bool isDying;

	private DeathVolume volumeWeHit;

	private float deathTimeLeft;

	private GameObject spawnedBird;

	private GameObject spawnedShark;

	private void Start()
	{
		lavaSmokeParticles = Resources.Load("Particles/LavaDeathParticles") as GameObject;
		waterDeathParticle = Resources.Load("Particles/Water_Death") as GameObject;
		sandDeathParticle = Resources.Load("Particles/Sand_Death") as GameObject;
		jungleDeathParticle = Resources.Load("Particles/Jungle_Death") as GameObject;
		cityDeathParticle = Resources.Load("Particles/City_Death") as GameObject;
		snowDeathParticle = Resources.Load("Particles/Snow_Death") as GameObject;
		playerCollider = GetComponent<RaycastCollider>();
		graphics = GetComponent<PlayerGraphics>();
	}

	private void OnRespawn()
	{
		OnReset();
	}

	private void OnReset()
	{
		isDying = false;
		volumeWeHit = null;
		deathTimeLeft = 0f;
		if (spawnedBird != null)
		{
			Object.Destroy(spawnedBird);
		}
		if (spawnedShark != null)
		{
			Object.Destroy(spawnedShark);
		}
	}

	private void FixedUpdate()
	{
		float num = 50f;
		int layerMask = liquidLayer | solidLayer;
		if (Physics.Raycast(base.transform.position + Vector3.up * num, Vector3.down, out var hitInfo, 1000f, layerMask))
		{
			float num2 = hitInfo.distance - num - playerCollider.radius;
			if (!isDying && num2 < 0.05f)
			{
				isDying = true;
				deathTimeLeft = 1f;
				if ((bool)hitInfo.transform.GetComponent<DeathVolume>())
				{
					volumeWeHit = hitInfo.transform.GetComponent<DeathVolume>();
					deathTimeLeft = TimeItTakesToDie(volumeWeHit.deathVolumeType);
					FirstHit(volumeWeHit.deathVolumeType);
				}
			}
			if (isDying && num2 > 0.05f && Singleton<PlayerManager>.SP.IsNetworkedPlayer(base.gameObject))
			{
				isDying = false;
			}
			if (Singleton<PlayerManager>.SP.IsLocalPlayer(base.gameObject))
			{
				float num3 = 1f;
				if (HenkUtils.IsOutside())
				{
					num3 = 5f;
				}
				Camera.main.GetComponent<PlatformerCamera>().yPosOfDeathVolume = hitInfo.point.y + num3;
				if ((bool)Camera.main.GetComponent<PlatformerCameraMultiplayer>())
				{
					float num4 = hitInfo.point.y + num3;
					if (num4 > Camera.main.GetComponent<PlatformerCameraMultiplayer>().yPosOfDeathVolume)
					{
						Camera.main.GetComponent<PlatformerCameraMultiplayer>().yPosOfDeathVolume = num4;
					}
				}
			}
			Debug.DrawLine(base.transform.position, hitInfo.point, Color.red);
		}
		if (!isDying)
		{
			return;
		}
		if ((bool)volumeWeHit)
		{
			UpdateDeath(volumeWeHit.deathVolumeType);
		}
		deathTimeLeft -= Time.deltaTime;
		if (deathTimeLeft <= 0f && !GetComponent<PlatformerController>().isExternalControlled)
		{
			if (GetComponent<PlatformerController>().localPlayerNumber == -1)
			{
				Singleton<PlayerManager>.SP.ResetPlayer(base.gameObject, hard: false);
				return;
			}
			Singleton<LocalMultiManager>.SP.KillPlayer(base.gameObject);
			OnRespawn();
		}
	}

	public void FirstHit(DeathVolumeType deathType)
	{
		if (!Singleton<PlayerManager>.SP.IsLocalPlayer(base.gameObject))
		{
			return;
		}
		graphics.ParticleEvent(SfxEvents.LavaDeath);
		Singleton<AudioManager>.SP.PlayCharacterDeath(base.gameObject);
		Singleton<TheNSA>.SP.PokeTheNSA(NotificationType.PlayerDeath, string.Empty);
		BroadcastMessage("OnEventLava", SendMessageOptions.DontRequireReceiver);
		switch (deathType)
		{
		case DeathVolumeType.Lava:
			Singleton<AchievementsManager>.SP.UnlockAchievement(Achievement.FloorIsLava);
			AudioController.Play("Lava_hit");
			AudioController.Play("Lava_bubbles", 1f, 0.5f);
			Singleton<AudioManager>.SP.PlayCharacterLavaDeath(base.gameObject);
			if (!GetComponent<PlatformerController>().isExternalControlled)
			{
				Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Lava);
			}
			(Object.Instantiate(lavaSmokeParticles) as GameObject).transform.position = GetComponent<PlayerGraphics>().animatedModel.transform.position;
			break;
		case DeathVolumeType.Water:
			AudioController.Play("waterdeath");
			spawnedShark = Object.Instantiate(waterDeathParticle) as GameObject;
			spawnedShark.transform.position = GetComponent<PlayerGraphics>().animatedModel.transform.position;
			spawnedShark.transform.parent = base.transform;
			break;
		case DeathVolumeType.Beach:
			AudioController.Play("sanddeath");
			(Object.Instantiate(sandDeathParticle) as GameObject).transform.position = GetComponent<PlayerGraphics>().animatedModel.transform.position;
			break;
		case DeathVolumeType.Snow:
			AudioController.Play("sanddeath");
			(Object.Instantiate(snowDeathParticle) as GameObject).transform.position = GetComponent<PlayerGraphics>().animatedModel.transform.position;
			break;
		case DeathVolumeType.Jungle:
			AudioController.Play("chutedeath");
			spawnedBird = Object.Instantiate(jungleDeathParticle) as GameObject;
			spawnedBird.transform.parent = base.transform;
			spawnedBird.transform.localPosition = new Vector3(0f, -1.5f, 0f);
			spawnedBird.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			break;
		case DeathVolumeType.City:
			AudioController.Play("citydeath");
			(Object.Instantiate(cityDeathParticle) as GameObject).transform.position = GetComponent<PlayerGraphics>().animatedModel.transform.position;
			break;
		case DeathVolumeType.Rocks:
		case DeathVolumeType.Streets:
			break;
		}
	}

	public void UpdateDeath(DeathVolumeType deathType)
	{
		float num = 0f;
		if (deathType == DeathVolumeType.Lava)
		{
			num = 20f;
		}
		if (deathType == DeathVolumeType.Water)
		{
			num = 15f;
		}
		if (deathType == DeathVolumeType.Beach || deathType == DeathVolumeType.Snow)
		{
			num = 45f;
		}
		if (deathType == DeathVolumeType.City)
		{
			num = 25f;
		}
		if (deathType == DeathVolumeType.Jungle)
		{
			num = 8f;
		}
		playerCollider.velocity -= playerCollider.velocity * num * Time.fixedDeltaTime;
		if ((bool)GetComponent<GrapplingHook>() && GetComponent<GrapplingHook>().IsEnabled())
		{
			GetComponent<GrapplingHook>().DisableAbility(forced: true);
		}
	}

	public float TimeItTakesToDie(DeathVolumeType deathType)
	{
		return deathType switch
		{
			DeathVolumeType.Lava => 0.75f, 
			DeathVolumeType.Water => 0.75f, 
			DeathVolumeType.Beach => 0.75f, 
			DeathVolumeType.Snow => 0.75f, 
			DeathVolumeType.Jungle => 1.5f, 
			DeathVolumeType.Rocks => 0f, 
			DeathVolumeType.Streets => 0f, 
			DeathVolumeType.City => 0.75f, 
			_ => 1f, 
		};
	}
}
