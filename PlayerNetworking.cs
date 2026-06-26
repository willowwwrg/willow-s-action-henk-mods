using System.Collections;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

public class PlayerNetworking : Photon.MonoBehaviour
{
	private RaycastCollider playerCollider;

	private PlatformerPhysics physics;

	private PlayerWaypointManager playerWaypoints;

	private PlayerGraphics graphics;

	private PlayerManager playerManager;

	private Vector2 lastPosition;

	private Vector2 lastVelocity;

	private double lastTimestamp;

	private bool isBroadcasting;

	private float walkInput;

	private bool jumpInput;

	private bool slideInput;

	private bool abilityInput;

	private Vector3 prevPos = Vector3.zero;

	private float lastWalkInput;

	private float lastTauntTime;

	private float minTimeBetweenTaunts = 2f;

	private bool levelNotReadyToEnablePlayer;

	public bool onPodium;

	private void Awake()
	{
		playerCollider = GetComponent<RaycastCollider>();
		playerWaypoints = GetComponent<PlayerWaypointManager>();
		physics = GetComponent<PlatformerPhysics>();
		graphics = GetComponent<PlayerGraphics>();
		playerManager = Singleton<PlayerManager>.SP;
		if ((bool)base.photonView && !base.photonView.isMine && !Object.FindObjectOfType<GameSpline>())
		{
			physics.enabled = false;
			graphics.enabled = false;
			playerWaypoints.enabled = false;
			playerCollider.enabled = false;
			levelNotReadyToEnablePlayer = true;
			UnityEngine.MonoBehaviour.print("Disabling player");
		}
	}

	private void Start()
	{
		if (base.photonView.isMine)
		{
			PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable
			{
				["character"] = (int)Singleton<CharacterSelect>.SP.GetSelectedCharacter(),
				["skin"] = Singleton<CharacterSelect>.SP.GetSelectedSkin()
			});
			return;
		}
		Debug.Log("spawned player from someone else");
		Singleton<PlayerManager>.SP.AddNetworkPlayer(base.gameObject);
		Object.Destroy(GetComponent<PlayerAudio>());
		Object.Destroy(GetComponent<ReplayRecorder>());
		GetComponent<GrapplingHook>().enabled = false;
		GetComponent<PlatformerController>().isExternalControlled = true;
		GetComponent<PlayerGraphics>().hasGhostGraphics = true;
		StartCoroutine(GetSkin());
		Singleton<PlayerManager>.SP.SpawnNameLabel(base.gameObject, string.Empty);
		graphics.ghostNameLabel.SetGhostName(base.photonView.owner.name);
		graphics.ghostNameLabel.ToggleLabel(state: true);
	}

	private void OnDestroy()
	{
		if (playerManager != null)
		{
			playerManager.RemovePlayer(base.gameObject);
		}
	}

	private void OnDisable()
	{
		ResetKnownInput();
	}

	private IEnumerator GetSkin()
	{
		PhotonPlayer owner = base.photonView.owner;
		while (owner.customProperties["character"] == null)
		{
			yield return new WaitForEndOfFrame();
		}
		CharacterSelect.Characters character = (CharacterSelect.Characters)(int)owner.customProperties["character"];
		int skinNum = (int)owner.customProperties["skin"];
		graphics.SetModel(character, skinNum);
	}

	public void SubmitScore(float finishTime)
	{
		if (!base.photonView.isMine)
		{
			Debug.LogError("Calling submit score on a player that is not local!");
		}
		ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.player.customProperties;
		if (customProperties["score"] == null || finishTime < (float)customProperties["score"])
		{
			customProperties["score"] = finishTime;
			PhotonNetwork.SetPlayerCustomProperties(customProperties);
		}
	}

	public void ResetKnownInput()
	{
		walkInput = 0f;
		jumpInput = false;
		slideInput = false;
		abilityInput = false;
	}

	[RPC]
	public void FloatInput(byte input, float inputValue, Vector2 curvePos, Vector2 curveVel, PhotonMessageInfo info)
	{
		if (input == 87)
		{
			Debug.DrawRay(base.transform.position, Vector3.up, Color.yellow, 10f);
			physics.input.walkInput = inputValue;
		}
		ResolveDelay(info.timestamp, curvePos, curveVel);
	}

	[RPC]
	public void BoolInput(byte input, bool inputValue, Vector2 curvePos, Vector2 curveVel, PhotonMessageInfo info)
	{
		if (levelNotReadyToEnablePlayer || !base.enabled)
		{
			return;
		}
		switch (input)
		{
		case 74:
			Debug.DrawRay(base.transform.position, Vector3.up, Color.blue, 10f);
			physics.input.jumpInput = inputValue;
			break;
		case 83:
			Debug.DrawRay(base.transform.position, Vector3.up, Color.green, 10f);
			physics.input.slideInput = inputValue;
			break;
		case 65:
			Debug.DrawRay(base.transform.position, Vector3.up, Color.cyan, 10f);
			if (inputValue && !GetComponent<GrapplingHook>().enabled)
			{
				GetComponent<GrapplingHook>().enabled = true;
			}
			physics.input.abilityInput = inputValue;
			break;
		}
		ResolveDelay(info.timestamp, curvePos, curveVel);
	}

	[RPC]
	public void ResetInput(byte input, PhotonMessageInfo info)
	{
		if (!levelNotReadyToEnablePlayer && base.enabled)
		{
			switch (input)
			{
			case 83:
				Debug.DrawRay(base.transform.position, Vector3.up, Color.magenta, 10f);
				Singleton<PlayerManager>.SP.ResetPlayer(base.gameObject, hard: false);
				break;
			case 72:
				Debug.DrawRay(base.transform.position, Vector3.up, Color.red, 10f);
				Singleton<PlayerManager>.SP.ResetPlayer(base.gameObject, hard: true);
				break;
			}
		}
	}

	private void OnRespawn()
	{
		if (base.photonView.isMine && base.enabled)
		{
			base.photonView.RPC("ResetInput", PhotonTargets.Others, (byte)83);
		}
	}

	private void OnReset()
	{
		if (base.photonView.isMine && base.enabled)
		{
			ResetKnownInput();
			base.photonView.RPC("ResetInput", PhotonTargets.Others, (byte)72);
		}
	}

	private void ResolveDelay(double timestamp, Vector2 originalCurvePos, Vector2 originalCurveVel)
	{
		if (!levelNotReadyToEnablePlayer && base.enabled)
		{
			_ = PhotonNetwork.time;
			Vector3 position = graphics.rotatingChild.position;
			Vector3 position2 = base.transform.position;
			playerWaypoints.SetOffset(originalCurvePos, alsoSetPos: true);
			playerCollider.velocity = physics.GetRightFlat() * originalCurveVel.x + Vector3.up * originalCurveVel.y;
			float magnitude = (position2 - base.transform.position).magnitude;
			Debug.DrawLine(position2, base.transform.position, Color.red, 10f);
			prevPos = Vector3.zero;
			if (magnitude < 15f)
			{
				graphics.SetRotatorAtWorldPos(position);
			}
			else
			{
				graphics.SetRotatorAtWorldPos(base.transform.position);
			}
		}
	}

	private void FixedUpdate()
	{
		if (onPodium)
		{
			return;
		}
		if (base.photonView.isMine)
		{
			Vector2 offset2D = playerWaypoints.GetOffset2D();
			Vector2 vector = new Vector2(physics.GetHorizontalSpeed(), physics.GetVerticalSpeed());
			if (physics.input.walkInput != walkInput && CanSendWalkFloat(physics.input.walkInput))
			{
				walkInput = physics.input.walkInput;
				lastWalkInput = Time.time;
				base.photonView.RPC("FloatInput", PhotonTargets.Others, (byte)87, physics.input.walkInput, offset2D, vector);
			}
			if (physics.input.jumpInput != jumpInput)
			{
				jumpInput = physics.input.jumpInput;
				base.photonView.RPC("BoolInput", PhotonTargets.Others, (byte)74, physics.input.jumpInput, offset2D, vector);
			}
			if (physics.input.slideInput != slideInput)
			{
				slideInput = physics.input.slideInput;
				base.photonView.RPC("BoolInput", PhotonTargets.Others, (byte)83, physics.input.slideInput, offset2D, vector);
			}
			if (physics.input.abilityInput != abilityInput)
			{
				abilityInput = physics.input.abilityInput;
				if (!abilityInput || GetComponent<GrapplingHook>().enabled)
				{
					base.photonView.RPC("BoolInput", PhotonTargets.Others, (byte)65, physics.input.abilityInput, offset2D, vector);
				}
			}
		}
		else
		{
			if (prevPos != Vector3.zero)
			{
				Debug.DrawLine(prevPos, base.transform.position, Color.white, 10f);
			}
			if (levelNotReadyToEnablePlayer)
			{
				GameSpline gameSpline = Object.FindObjectOfType<GameSpline>();
				if ((bool)gameSpline)
				{
					playerWaypoints.splineToFollow = gameSpline;
					physics.enabled = true;
					graphics.enabled = true;
					playerWaypoints.enabled = true;
					playerCollider.enabled = true;
					levelNotReadyToEnablePlayer = false;
					Singleton<PlayerManager>.SP.ResetPlayer(base.gameObject, hard: true);
					StartCoroutine(GetSkin());
					UnityEngine.MonoBehaviour.print("Enabling player");
				}
			}
		}
		prevPos = base.transform.position;
	}

	private bool CanSendWalkFloat(float input)
	{
		if (input == 1f || Mathf.Approximately(input, 0f) || input == -1f)
		{
			return true;
		}
		if (Time.time - lastWalkInput > 1f)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		float num = Time.time - lastTauntTime;
		if (Singleton<InputManager>.SP.CheckAction(InputAction.Taunt) && num > minTimeBetweenTaunts && base.photonView.isMine)
		{
			lastTauntTime = Time.time;
			base.photonView.RPC("Taunt", PhotonTargets.All);
		}
	}

	[RPC]
	private void Taunt(PhotonMessageInfo info)
	{
		if (onPodium)
		{
			Singleton<AudioManager>.SP.PlayCharacterTaunt(base.gameObject, 1f);
			GetComponent<PlayerGraphics>().DoTaunt();
		}
		else if (!levelNotReadyToEnablePlayer && !(Singleton<PlayerManager>.SP.GetPlayer() == null))
		{
			float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, base.transform.position);
			float volume = Mathf.InverseLerp(30f, 10f, value);
			Singleton<AudioManager>.SP.PlayCharacterTaunt(base.gameObject, volume);
			GetComponent<PlayerGraphics>().DoTaunt();
		}
	}
}
