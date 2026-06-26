using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
	public bool abilityEnabled;

	public Transform hookPrefab;

	private Transform hookModel;

	private HookContainer hookContainer;

	private Vector3 suctionPos;

	private RaycastCollider playerCollider;

	private PlatformerPhysics physics;

	private PlayerGraphics graphics;

	private Vector3 hookPoint;

	private float hookDistance;

	private float stiffness = 75f;

	private float damping = 0.2f;

	private float minDistance = 2.5f;

	private float maxDistance = 50f;

	private float hookAccel = 50f;

	private float hookFriction = 5f;

	private float hookVel;

	private float movementAccel = 25f;

	private float horSpeedForAimDirection = 10f;

	private float keepRotationSpeedFactor = 0.4f;

	private float cooldown;

	private Vector3 hookDir = Vector3.up;

	private Vector3 sideDir = Vector3.right;

	private Vector3 hitNormal;

	private bool forcedStop;

	[NonSerialized]
	public float hookDirection = 1f;

	private float hookDirectionVel = 1f;

	private float hookDirectionAccel = 35f;

	private float hookDirectionFriction = 30f;

	private bool noAccelAbove90 = true;

	private float forceUpInput;

	private float speedBoostOnHit;

	private float startHookVel = -15f;

	private float lerpToIdealVelocity = 0.5f;

	private bool ghostGraphics;

	private void OnReset()
	{
		if (base.enabled)
		{
			base.enabled = false;
		}
		OnRespawn();
	}

	private void OnRespawn()
	{
		abilityEnabled = false;
		suctionPos = Vector3.forward * 0.3f;
		cooldown = 0f;
		hookDirection = 1f;
		hookDirectionVel = 1f;
	}

	private void Awake()
	{
		playerCollider = GetComponent<RaycastCollider>();
		physics = GetComponent<PlatformerPhysics>();
		graphics = GetComponent<PlayerGraphics>();
	}

	private void OnEnable()
	{
		hookModel = (Transform)UnityEngine.Object.Instantiate(hookPrefab);
		hookContainer = hookModel.GetComponent<HookContainer>();
		if (!graphics.hasGhostGraphics)
		{
			SetNonTransparentShaders();
		}
	}

	private void OnDisable()
	{
		if ((bool)hookModel)
		{
			UnityEngine.Object.Destroy(hookModel.gameObject);
		}
	}

	public Vector3 GetShootDir(bool visual = true)
	{
		Vector3 up = Vector3.up;
		if (visual)
		{
			up += hookDirection * base.transform.right * 0.8f;
		}
		else
		{
			up += hookDirectionVel * base.transform.right * 0.8f;
		}
		up.Normalize();
		return up;
	}

	public Vector3 CheckTarget([Optional] Vector3 startPos)
	{
		if (startPos == default(Vector3))
		{
			startPos = base.transform.position;
		}
		Vector3 result = Vector3.zero;
		int layerMask = -36869;
		if (CanShoot() && Physics.Raycast(startPos, GetShootDir(), out var hitInfo, maxDistance, layerMask))
		{
			result = hitInfo.point;
		}
		return result;
	}

	public bool CanShoot()
	{
		if (physics.IsOnWall())
		{
			bool flag = physics.GetGroundNormalX() < 0f;
			if ((flag && hookDirectionVel > 0f) || (!flag && hookDirectionVel < 0f))
			{
				return false;
			}
		}
		return true;
	}

	public void EnableAbility()
	{
		if (forcedStop || !CanShoot())
		{
			return;
		}
		Vector3 shootDir = GetShootDir(visual: false);
		Vector3 position = base.transform.position;
		int layerMask = -36869;
		if (Physics.Raycast(position, shootDir, out var hitInfo, maxDistance, layerMask) && !abilityEnabled && base.enabled && !(cooldown > 0f))
		{
			abilityEnabled = true;
			cooldown = 0.33f;
			hookVel = 0f;
			hookPoint = hitInfo.point;
			hookDistance = (hookPoint - position).magnitude;
			hitNormal = hitInfo.normal;
			hookDir = (hookPoint - position) / hookDistance;
			Vector3 vector = Vector3.Cross(hookDir, base.transform.forward);
			if (Vector3.Dot(vector, playerCollider.velocity) < 0f)
			{
				vector = -vector;
			}
			Vector3 to = vector * playerCollider.velocity.magnitude;
			playerCollider.velocity = Vector3.Lerp(playerCollider.velocity, to, lerpToIdealVelocity);
			hookDistance -= 3f;
			hookDistance = Mathf.Clamp(hookDistance, minDistance, maxDistance);
			hookVel = startHookVel;
			if ((bool)hookContainer.suctionModel)
			{
				suctionPos = hookContainer.suctionModel.position;
			}
			if (!GetComponent<PlayerGraphics>().hasGhostGraphics)
			{
				hookContainer.TriggerShootHook();
			}
		}
	}

	public void DisableAbility(bool forced = false)
	{
		if (abilityEnabled && (cooldown < 0.2f || forced))
		{
			abilityEnabled = false;
			if ((bool)hookContainer.suctionModel)
			{
				suctionPos = hookContainer.suctionModel.localPosition;
			}
			if (!GetComponent<PlayerGraphics>().hasGhostGraphics)
			{
				hookContainer.TriggerHookRelease();
			}
			cooldown = 0.2f;
			forcedStop = forced;
		}
		if (forcedStop && !forced)
		{
			forcedStop = false;
		}
	}

	private void Update()
	{
		hookDirection += hookDirectionVel * hookDirectionAccel * Time.deltaTime;
		hookDirection = Mathf.Clamp(hookDirection, -1f, 1f);
		Vector3 vector = Vector3.forward * 0.3f;
		if (IsEnabled())
		{
			vector = hookPoint + hitNormal * 0.22f;
		}
		suctionPos -= (suctionPos - vector) * 20f * Time.deltaTime;
	}

	private void FixedUpdate()
	{
		if (cooldown > 0f)
		{
			float num = cooldown;
			cooldown -= Time.fixedDeltaTime;
			if (num >= 0.2f && cooldown < 0.2f && abilityEnabled && !graphics.hasGhostGraphics)
			{
				hookContainer.TriggerSuctionHitWall();
			}
		}
		if (physics.input.walkInput < 0f)
		{
			hookDirectionVel = -1f;
		}
		if (physics.input.walkInput > 0f)
		{
			hookDirectionVel = 1f;
		}
		if (abilityEnabled)
		{
			float num2 = maxDistance;
			float verticalInput = physics.input.verticalInput;
			float walkInput = physics.input.walkInput;
			verticalInput = Mathf.Round(verticalInput);
			verticalInput = forceUpInput;
			walkInput = Mathf.Round(walkInput);
			Vector3 vector = hookPoint - base.transform.position;
			num2 = vector.magnitude;
			hookDir = vector / num2;
			hookVel -= hookVel * hookFriction * Time.fixedDeltaTime;
			hookVel += (0f - verticalInput) * hookAccel * Time.fixedDeltaTime;
			float rotationSpeed = GetRotationSpeed();
			hookDistance += hookVel * Time.fixedDeltaTime;
			hookDistance = Mathf.Clamp(hookDistance, minDistance, maxDistance);
			float num3 = GetRotationSpeed() / rotationSpeed;
			if (num3 < 1f)
			{
				float num4 = 1f - num3;
				num4 *= 1f - keepRotationSpeedFactor;
				playerCollider.velocity *= 1f - num4;
			}
			sideDir = Vector3.Cross(hookDir, base.transform.forward);
			if (hookDir.y > 0f || !noAccelAbove90)
			{
				playerCollider.velocity += walkInput * movementAccel * sideDir * Time.fixedDeltaTime;
			}
			float num5 = num2 - hookDistance;
			float num6 = Vector3.Dot(playerCollider.velocity, hookDir) / Time.fixedDeltaTime;
			float num7 = num5 * stiffness - num6 * damping;
			playerCollider.velocity += num7 * hookDir * Time.fixedDeltaTime;
			Debug.DrawLine(base.transform.position, hookPoint, Color.green);
			Debug.DrawRay(base.transform.position, sideDir * walkInput * 5f, Color.cyan);
			Debug.DrawRay(base.transform.position + sideDir * walkInput * 5f, hookDir * verticalInput * 5f, Color.cyan);
			if (!graphics.hasGhostGraphics)
			{
				hookContainer.UpdateSwing(playerCollider.velocity.magnitude);
				float num8 = playerCollider.velocity.magnitude * Time.fixedDeltaTime * 0.1f;
				playerCollider.gameObject.BroadcastMessage("AddSpeedLines", num8, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (!graphics.hasGhostGraphics)
		{
			hookContainer.UpdateSwing(0f);
		}
	}

	private void LateUpdate()
	{
		Transform rightWrist = graphics.GetRightWrist();
		if ((bool)rightWrist)
		{
			hookModel.transform.position = rightWrist.position;
			hookModel.transform.rotation = rightWrist.rotation * Quaternion.Euler(0f, 90f, 0f);
			if (IsEnabled())
			{
				hookContainer.suctionModel.LookAt(hookContainer.suctionModel.position - hitNormal);
				hookContainer.suctionModel.transform.position = suctionPos;
			}
			else
			{
				hookContainer.suctionModel.transform.localPosition = suctionPos;
				hookContainer.suctionModel.transform.localEulerAngles = Vector3.zero;
			}
			float num = Vector3.Distance(hookContainer.ropeModel.position, hookContainer.suctionModel.position);
			hookContainer.ropeModel.localScale = new Vector3(2f, 7f, num * 25f);
			hookContainer.ropeModel.LookAt(hookContainer.suctionModel);
		}
	}

	public void HasCollision(RaycastCollisionInfo collision)
	{
		if (!IsEnabled())
		{
			return;
		}
		if (Vector3.Angle(collision.normal, hookDir) < 45f && collision.normal.y > 0.3f)
		{
			if (hookDistance > 5f)
			{
				hookDistance = (hookPoint - base.transform.position).magnitude - 3f;
				if (hookDistance < minDistance)
				{
					hookDistance = minDistance;
				}
				playerCollider.velocity = collision.velocityBeforeImpact;
			}
		}
		else
		{
			DisableAbility(forced: true);
		}
	}

	public bool IsEnabled()
	{
		if (base.enabled)
		{
			return abilityEnabled;
		}
		return false;
	}

	public Vector3 GetHookPoint()
	{
		return hookPoint;
	}

	public Vector3 GetHookDir()
	{
		return hookDir;
	}

	public Vector3 GetSideDir()
	{
		return sideDir;
	}

	public float GetRotationSpeed()
	{
		return (float)Math.PI * 2f * hookDistance / Vector3.Dot(playerCollider.velocity, sideDir);
	}

	public void SetNonTransparentShaders()
	{
		if (!hookContainer)
		{
			return;
		}
		Renderer[] componentsInChildren = hookContainer.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material material in materials)
			{
				if (material.shader.name.Contains("Marmoset/Transparent/"))
				{
					string text = "Marmoset/" + material.shader.name.Substring(21);
					material.shader = Shader.Find(text);
				}
			}
		}
	}

	public void SetHookAlpha(float hookAlpha)
	{
		if (!hookContainer)
		{
			return;
		}
		Renderer[] componentsInChildren = hookContainer.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.GetType() != typeof(SkinnedMeshRenderer) && renderer.GetType() != typeof(MeshRenderer))
			{
				continue;
			}
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				if (renderer.materials[j].HasProperty("_Color"))
				{
					if (hookAlpha == 0f)
					{
						renderer.enabled = false;
						continue;
					}
					renderer.enabled = true;
					renderer.materials[j].color = new Color(renderer.materials[j].color.r, renderer.materials[j].color.g, renderer.materials[j].color.b, hookAlpha);
				}
			}
		}
	}
}
