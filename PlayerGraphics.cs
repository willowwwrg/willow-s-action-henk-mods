using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGraphics : MonoBehaviour
{
	public Transform physicsInterpolator;

	private Vector3 prevPos = Vector3.zero;

	private Vector3 nextPos = Vector3.zero;

	public Transform rotatingChild;

	public CharacterSelect.Characters currentCharacter = CharacterSelect.Characters.Henk;

	public int currentSkinNum;

	private int curModel;

	public GameObject animatedModel;

	public float rotationAccelAir = 500f;

	public float rotationFrictionAir = 3f;

	public float turnAroundSpeed = 5f;

	[HideInInspector]
	public float rotationSpeed;

	private Animator animator;

	private RaycastCollider playerCollider;

	private PlatformerController controller;

	private PlatformerPhysics physics;

	private GrapplingHook grapplingHook;

	private ReplayController replayController;

	private CharacterModel cachedCharacterModel;

	private float tiltRotation;

	private float yRotation;

	private float targetDirection = 90f;

	public GameObject particles;

	public GameObject baseParticles;

	public bool hasGhostGraphics;

	private bool ghostInitialized;

	private float minGhostRange = 0.7f;

	private float maxGhostRange = 4f;

	private float minGhostAlpha;

	private float maxGhostAlpha = 0.8f;

	private bool forceOnGround = true;

	public GameObject directionIndicator;

	public GameObject hitInfoIndicator;

	public GameObject tauntIndicator;

	public ReplayName ghostNameLabel;

	public bool spawningModel;

	private Vector3 rotatorTargetLocalPos = Vector3.zero;

	private float rotatorPosSmoothing = 2.5f;

	private bool taunting;

	private bool isDead;

	private void Awake()
	{
		physics = GetComponent<PlatformerPhysics>();
		playerCollider = GetComponent<RaycastCollider>();
		controller = GetComponent<PlatformerController>();
		grapplingHook = GetComponent<GrapplingHook>();
		replayController = GetComponent<ReplayController>();
		ghostInitialized = false;
		taunting = false;
	}

	private void Start()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Replay)
		{
			return;
		}
		if (GetComponent<PlatformerController>().isExternalControlled)
		{
			hasGhostGraphics = true;
			Object.Destroy(directionIndicator);
			Object.Destroy(hitInfoIndicator);
			return;
		}
		if (controller.localPlayerNumber != -1)
		{
			if ((bool)tauntIndicator)
			{
				tauntIndicator.transform.localPosition = new Vector3(0f, 3.2f, 0f);
			}
			CharSkin selectedCharacter = Singleton<LocalMultiManager>.SP.playerInfo[controller.localPlayerNumber].selectedCharacter;
			SetModel(selectedCharacter.character, selectedCharacter.skinNum);
			return;
		}
		Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
		if (currentLevelObj.levelType == LevelType.Bonus)
		{
			SetModel(currentLevelObj.challenger, currentLevelObj.challengerSkinNum);
		}
		else if (currentLevelObj.publishedFiledID == 635322454)
		{
			SetModel(CharacterSelect.Characters.Kentony, 2);
		}
		else
		{
			SetModel(Singleton<CharacterSelect>.SP.GetSelectedCharacter(), Singleton<CharacterSelect>.SP.GetSelectedSkin());
		}
	}

	private void OnDestroy()
	{
		StopCoroutine("TauntRoutine");
		if (ghostNameLabel != null)
		{
			Object.Destroy(ghostNameLabel.gameObject);
		}
	}

	public void OnReset()
	{
		OnRespawn();
	}

	public void OnRespawn()
	{
		rotationSpeed = 0f;
		tiltRotation = 0f;
		targetDirection = 90f;
		yRotation = targetDirection;
		forceOnGround = true;
		if ((bool)animator)
		{
			animator.SetTrigger("Reset");
		}
		StopCoroutine("TauntRoutine");
		tauntIndicator.SetActive(value: false);
		taunting = false;
		SetPlayerAlive();
	}

	public SelectableCharacter GetCharacterScript()
	{
		return animatedModel.GetComponent<SelectableCharacter>();
	}

	public void DefaultToHenkModel()
	{
		SetModel(CharacterSelect.Characters.Henk);
	}

	private IEnumerator SetModelRoutine(CharSkin charSkin)
	{
		spawningModel = true;
		CharacterSelect.Characters character = charSkin.character;
		int skinNum = charSkin.skinNum;
		if (animatedModel != null)
		{
			Object.Destroy(animatedModel);
		}
		currentCharacter = character;
		currentSkinNum = skinNum;
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: true);
		ResourceRequest resourceRequest = Resources.LoadAsync(string.Concat("CharacterModels/", character, "_", skinNum));
		while (!resourceRequest.isDone)
		{
			yield return new WaitForEndOfFrame();
		}
		GameObject gameObject = resourceRequest.asset as GameObject;
		Singleton<PermaGUI>.SP.ToggleLoadingSprite(state: false);
		if (gameObject == null)
		{
			gameObject = Resources.Load(string.Concat("CharacterModels/", character, "_", 0)) as GameObject;
			Debug.LogError("Error while loading character: " + character.ToString() + "-" + skinNum);
		}
		animatedModel = Object.Instantiate(gameObject) as GameObject;
		animatedModel.transform.parent = rotatingChild;
		animatedModel.transform.localPosition = new Vector3(0f, animatedModel.GetComponent<CharacterModel>().modelYOffset, 0f);
		animatedModel.transform.localRotation = Quaternion.identity;
		rotatingChild.localEulerAngles = new Vector3(0f, animatedModel.GetComponent<CharacterModel>().rotationOffset, 0f);
		LMPHenkOverride();
		bool flag = Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.FirstPerson || Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.Invisible;
		if (HenkUtils.IsInALevel() && !flag && (bool)baseParticles)
		{
			InitParticlesWithOverrides();
			InitParticles();
		}
		InitShadersAndMaterials();
		if (flag)
		{
			SkinnedMeshRenderer[] componentsInChildren = animatedModel.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			MeshRenderer[] componentsInChildren2 = animatedModel.GetComponentsInChildren<MeshRenderer>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
		}
		if (GetComponent<PlatformerController>().isExternalControlled && Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge)
		{
			SetRotatorTargetLocalPos(new Vector3(0f, 0f, 2f));
		}
		animator = animatedModel.GetComponent<Animator>();
		cachedCharacterModel = animatedModel.GetComponent<CharacterModel>();
		if (hasGhostGraphics)
		{
			ghostInitialized = true;
		}
		spawningModel = false;
		yield return 0;
	}

	public void LMPHenkOverride()
	{
		if (GetComponent<PlatformerController>().localPlayerNumber == -1 || currentCharacter != CharacterSelect.Characters.Henk || currentSkinNum != 0)
		{
			return;
		}
		SkinnedMeshRenderer[] componentsInChildren = animatedModel.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (skinnedMeshRenderer.material.mainTexture.name == "henk_body_d")
			{
				skinnedMeshRenderer.material.mainTexture = Object.FindObjectOfType<State_LMP>().henkOverrideTextures[GetComponent<PlatformerController>().localPlayerNumber];
			}
		}
	}

	public void SetModel(CharacterSelect.Characters character, int skinNum = 0)
	{
		CharSkin charSkin = new CharSkin
		{
			character = character,
			skinNum = skinNum
		};
		StopCoroutine("SetModelRoutine");
		StartCoroutine("SetModelRoutine", charSkin);
	}

	private void InitParticlesWithOverrides()
	{
		if (particles != null)
		{
			Object.DestroyImmediate(particles);
		}
		particles = Object.Instantiate(baseParticles) as GameObject;
		particles.transform.parent = rotatingChild;
		particles.transform.localPosition = Vector3.zero;
		particles.transform.localRotation = Quaternion.Euler(Vector3.zero);
		PlayerParticleEffects component = particles.GetComponent<PlayerParticleEffects>();
		CharacterModel component2 = animatedModel.GetComponent<CharacterModel>();
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LocalMultiplayer)
		{
			if (component2.particleOverrides.Count > 0)
			{
				foreach (ParticleOverrideStruct particleOverride in component2.particleOverrides)
				{
					for (int i = 0; i < component.particleEffects.Count; i++)
					{
						if (particleOverride.effect != component.particleEffects[i].effect)
						{
							continue;
						}
						component.particleEffects[i].gameObject.SetActive(value: false);
						foreach (ParticleOverrideStruct particleOverride2 in component2.particleOverrides)
						{
							if (particleOverride2.effect == component.particleEffects[i].effect)
							{
								GameObject gameObject = Object.Instantiate(particleOverride2.overrideObject) as GameObject;
								gameObject.transform.parent = particles.transform;
								gameObject.transform.localPosition = particleOverride2.overrideObject.transform.localPosition;
								gameObject.transform.localRotation = particleOverride2.overrideObject.transform.localRotation;
								particleOverride2.overrideObject = gameObject;
								if (!gameObject.GetComponent<ParticleEffect>())
								{
									gameObject.AddComponent<ParticleEffect>().effect = particleOverride.effect;
								}
							}
						}
					}
				}
			}
			foreach (ParticleOverrideStruct particleOverride3 in component2.particleOverrides)
			{
				if (particleOverride3.effect == ParticleEffects.AddExtraParticle)
				{
					GameObject gameObject2 = Object.Instantiate(particleOverride3.overrideObject) as GameObject;
					gameObject2.name = "extraParticles";
					gameObject2.transform.parent = particles.transform;
					gameObject2.transform.localPosition = particleOverride3.overrideObject.transform.localPosition;
					gameObject2.transform.localRotation = particleOverride3.overrideObject.transform.localRotation;
					particleOverride3.overrideObject = gameObject2;
				}
			}
			AdditionalSkinParticles component3 = component2.GetComponent<AdditionalSkinParticles>();
			if (component3 != null)
			{
				foreach (AdditionalParticlesStruct additionalParticle in component3.additionalParticles)
				{
					GameObject gameObject3 = Object.Instantiate(additionalParticle.particleEffect) as GameObject;
					gameObject3.name = "extraEventParticles";
					gameObject3.transform.parent = particles.transform;
					gameObject3.transform.localPosition = additionalParticle.particleEffect.transform.localPosition;
					gameObject3.transform.localRotation = additionalParticle.particleEffect.transform.localRotation;
					additionalParticle.particleEffect = gameObject3;
				}
			}
		}
		particles.gameObject.SetActive(value: true);
	}

	public void InitShadersAndMaterials()
	{
		if (currentSkinNum != 8 && currentSkinNum != 9)
		{
			MeshRenderer[] componentsInChildren = animatedModel.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				Camera.main.GetComponent<CameraEffectsManager>().UpdateMaterialForStyle(meshRenderer, isCharacter: true);
			}
			SkinnedMeshRenderer[] componentsInChildren2 = animatedModel.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
			{
				Camera.main.GetComponent<CameraEffectsManager>().UpdateMaterialForStyle(skinnedMeshRenderer, isCharacter: true);
			}
		}
		if (GetComponent<PlatformerController>().isExternalControlled)
		{
			bool isChallengerOnChallengeLevel = replayController != null
				&& replayController.GetReplayType() == GhostType.Challenger
				&& Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge;
			bool skipTransparency = isChallengerOnChallengeLevel || 
				(!DevCommands.IsChallengeGhostTransparent() && Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge);
			if (!skipTransparency)
				MakeTransparent();
		}
		Renderer[] componentsInChildren3 = animatedModel.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren3)
		{
			MaterialBaseValues component = renderer.GetComponent<MaterialBaseValues>();
			if (!component)
			{
				continue;
			}
			component.baseValues = new BaseValues[renderer.materials.Length];
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				component.baseValues[j] = new BaseValues();
				if (renderer.materials[j].HasProperty("_SpecInt"))
				{
					component.baseValues[j].specInt = renderer.materials[j].GetFloat("_SpecInt");
				}
				if (renderer.materials[j].HasProperty("_GlowStrength"))
				{
					component.baseValues[j].glowStrength = renderer.materials[j].GetFloat("_GlowStrength");
				}
			}
		}
		if (GetComponent<PlatformerController>().localPlayerNumber == -1 || !HenkUtils.IsInALevel())
		{
			return;
		}
		Color color = NGUIText.ParseColor24(Singleton<LocalMultiManager>.SP.playerBrightColorCodes[GetComponent<PlatformerController>().localPlayerNumber], 1);
		componentsInChildren3 = animatedModel.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer2 in componentsInChildren3)
		{
			Transform transform = (Transform)Object.Instantiate(renderer2.transform);
			transform.parent = renderer2.transform;
			transform.transform.localPosition = Vector3.zero;
			transform.transform.localEulerAngles = Vector3.zero;
			transform.transform.localScale = Vector3.one;
			List<Material> list = new List<Material>();
			int num = renderer2.materials.Length;
			for (int k = 0; k < num; k++)
			{
				Material material = Object.Instantiate(Resources.Load("OutlineShaderMaterial", typeof(Material))) as Material;
				material.SetColor("_OutlineColor", color);
				list.Add(material);
			}
			transform.renderer.materials = list.ToArray();
		}
	}

	public void MakeTransparent()
	{
		Renderer[] componentsInChildren = animatedModel.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				string text = material.shader.name;
				if (text.StartsWith("Marmoset"))
				{
					if (text.Split('/').Length - 1 == 1)
					{
						string text2 = "Marmoset/Transparent/" + material.shader.name.Substring(9);
						material.shader = Shader.Find(text2);
					}
					if (text.StartsWith("Marmoset/Self-Illumin"))
					{
						material.shader = Shader.Find("Marmoset/Transparent/Simple Glass/Bumped Specular Glow IBL");
					}
				}
				if ((material.HasProperty("_SpecInt") || material.HasProperty("_GlowStrength")) && !renderer.gameObject.GetComponent<MaterialBaseValues>())
				{
					renderer.gameObject.AddComponent<MaterialBaseValues>();
				}
			}
		}
	}

	private void UpdateGhostAlpha(float alphaOverride = -1f)
	{
		float value = 10000f;
		float num = alphaOverride;
		if (num < 0f)
		{
			if ((bool)Singleton<PlayerManager>.SP.GetPlayer())
			{
				value = (Singleton<PlayerManager>.SP.GetPlayer().transform.position - base.transform.position).magnitude;
			}
			float t = Mathf.InverseLerp(minGhostRange, maxGhostRange, value);
			num = Mathf.Lerp(minGhostAlpha, maxGhostAlpha, t);
		}
		if ((bool)ghostNameLabel)
		{
			ghostNameLabel.UpdateAlpha(num);
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.GetType() != typeof(SkinnedMeshRenderer) && renderer.GetType() != typeof(MeshRenderer))
			{
				continue;
			}
			Material[] mats = renderer.materials;
			MaterialBaseValues mbv = renderer.GetComponent<MaterialBaseValues>();
			for (int j = 0; j < mats.Length; j++)
			{
				if (mats[j].HasProperty("_Color"))
				{
					if (num == 0f)
					{
						renderer.enabled = false;
					}
					else
					{
						renderer.enabled = true;
						Color c = mats[j].color;
						mats[j].color = new Color(c.r, c.g, c.b, num);
					}
				}
				if ((bool)mbv && mbv.baseValues != null && mbv.baseValues[j] != null)
				{
					if (mbv.baseValues[j].specInt != 0f && mats[j].HasProperty("_SpecInt"))
					{
						mats[j].SetFloat("_SpecInt", mbv.baseValues[j].specInt * num);
					}
					if (mbv.baseValues[j].glowStrength != 0f && mats[j].HasProperty("_GlowStrength"))
					{
						mats[j].SetFloat("_GlowStrength", mbv.baseValues[j].glowStrength * num);
					}
				}
			}
		}
		if ((bool)grapplingHook && grapplingHook.enabled)
		{
			grapplingHook.SetHookAlpha(num);
		}
	}

	public void NextSkin()
	{
		if (!hasGhostGraphics)
		{
			CharacterSelect.Characters selectedCharacter = Singleton<CharacterSelect>.SP.GetSelectedCharacter();
			currentSkinNum++;
			if (Resources.Load(string.Concat("CharacterModels/", selectedCharacter, "_", currentSkinNum)) as GameObject == null)
			{
				currentSkinNum = 0;
			}
			SetModel(selectedCharacter, currentSkinNum);
		}
	}

	private void FixedUpdate()
	{
		prevPos = nextPos;
		nextPos = base.transform.position;
	}

	private void Update()
	{
		if ((!ghostInitialized && hasGhostGraphics) || !animator)
		{
			return;
		}
		InterpolatePhysics();
		if (hasGhostGraphics)
		{
			bool isChallenger = replayController != null && replayController.GetReplayType() == GhostType.Challenger;
			bool onChallengeLevel = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Challenge;
			if (!onChallengeLevel || (DevCommands.IsChallengeGhostTransparent() && !isChallenger))
				UpdateGhostAlpha();
		}
		bool flag = physics.onGround;
		float value = playerCollider.velocity.magnitude;
		if (forceOnGround)
		{
			if (flag)
			{
				forceOnGround = false;
			}
			else
			{
				flag = true;
				value = 0f;
			}
		}
		animator.SetFloat("Velocity", value);
		animator.SetBool("OnGround", flag);
		animator.SetBool("Jump", physics.IsInAJump());
		bool flag2 = physics.IsOnWall() && !physics.hasWallGrip && physics.onGround;
		animator.SetBool("OnWall", flag2);
		bool flag3 = (bool)grapplingHook && grapplingHook.IsEnabled();
		animator.SetBool("GrapplingHook", flag3);
		animator.SetBool("Sliding", physics.sliding && !flag3);
		if (flag3)
		{
			float num = Vector3.Dot(playerCollider.velocity, grapplingHook.GetSideDir());
			if (targetDirection < 0f)
			{
				num = 0f - num;
			}
			animator.SetFloat("Velocity", num);
		}
		if (directionIndicator != null)
		{
			bool flag4 = controller.localPlayerNumber != -1;
			MeshRenderer componentInChildren = directionIndicator.GetComponentInChildren<MeshRenderer>();
			Color color = componentInChildren.material.GetColor("_TintColor");
			Color color2 = Color.green;
			if (flag4)
			{
				color2 = NGUIText.ParseColor24(Singleton<LocalMultiManager>.SP.playerBrightColorCodes[controller.localPlayerNumber], 1);
			}
			color2.a = 0f;
			if (hitInfoIndicator != null)
			{
				hitInfoIndicator.renderer.enabled = false;
			}
			if (grapplingHook != null && grapplingHook.enabled && !grapplingHook.IsEnabled() && controller.HasControl())
			{
				directionIndicator.transform.LookAt(physicsInterpolator.position + grapplingHook.GetShootDir());
				componentInChildren.enabled = true;
				Vector3 vector = grapplingHook.CheckTarget(physicsInterpolator.position);
				if (vector == Vector3.zero)
				{
					if (!flag4)
					{
						color2 = Color.red;
					}
				}
				else if (hitInfoIndicator != null)
				{
					hitInfoIndicator.transform.position = vector;
					hitInfoIndicator.renderer.enabled = true;
					if (flag4)
					{
						hitInfoIndicator.renderer.material.SetColor("_TintColor", new Color(color2.r, color2.g, color2.b, 1f));
					}
				}
				color2.a = 0.3f;
			}
			color -= (color - color2) * 20f * Time.deltaTime;
			componentInChildren.material.SetColor("_TintColor", color);
		}
		float z = 0f;
		if (flag2 && !physics.sliding)
		{
			z = cachedCharacterModel.onWallZOffset;
		}
		float num2 = cachedCharacterModel.modelYOffset;
		if (!flag && !physics.sliding)
		{
			num2 += cachedCharacterModel.extraOffsetForJump;
		}
		Transform transform = animatedModel.transform;
		transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, num2, z), 10f * Time.deltaTime);
		if ((bool)rotatingChild)
		{
			ApplyRotation();
			if (!flag3)
			{
				if (!physics.IsOnWall())
				{
					if (physics.GetHorizontalSpeed() < -0.1f)
					{
						targetDirection = -90f;
					}
					else if (physics.GetHorizontalSpeed() > 0.1f)
					{
						targetDirection = 90f;
					}
					if (physics.onGround && physics.groundNormal.y < 0f)
					{
						targetDirection = 0f - targetDirection;
					}
				}
				else
				{
					if (physics.GetVerticalSpeed() < 0f)
					{
						targetDirection = -90f;
					}
					else
					{
						targetDirection = 90f;
					}
					bool flag5 = physics.GetGroundNormalX() < 0f;
					if (!flag5)
					{
						targetDirection = 0f - targetDirection;
					}
					if (!physics.hasWallGrip && !physics.sliding)
					{
						if (!flag5)
						{
							targetDirection = -90f;
						}
						else
						{
							targetDirection = 90f;
						}
					}
				}
			}
			yRotation -= (yRotation - targetDirection) * turnAroundSpeed * Time.deltaTime;
		}
		rotatingChild.localRotation = Quaternion.AngleAxis(tiltRotation, Vector3.forward) * Quaternion.AngleAxis(180f - yRotation, Vector3.up);
		rotatingChild.localPosition -= (rotatingChild.localPosition - rotatorTargetLocalPos) * rotatorPosSmoothing * Time.deltaTime;
	}

	private void InterpolatePhysics()
	{
		physicsInterpolator.transform.rotation = Quaternion.LookRotation(GetComponent<PlayerWaypointManager>().GetSideVectorSmooth());
		physicsInterpolator.position = Vector3.Lerp(prevPos, nextPos, Singleton<PhysicsTimeManager>.SP.interpolationValue);
	}

	private void ApplyRotation()
	{
		Vector3 vector = physics.groundNormal;
		bool flag = (bool)grapplingHook && grapplingHook.IsEnabled();
		if (!physics.onGround && !flag)
		{
			if (!(Mathf.Abs(Vector3.Dot(playerCollider.velocity, physics.GetRightFlat())) > 0.001f))
			{
				return;
			}
			vector = ((!(playerCollider.predictedNormal.y > 0.1f)) ? Vector3.up : playerCollider.predictedNormal);
			float num = Vector3.Angle(vector, GetUp());
			float num2 = RotationSign(vector);
			float num3 = rotationAccelAir;
			if (Mathf.Sign(rotationSpeed) == Mathf.Sign(num2))
			{
				float num4 = 0f - num3;
				if (predictMotionLeft(Mathf.Abs(rotationSpeed), num4, rotationFrictionAir) > num)
				{
					num3 = num4;
				}
			}
			rotationSpeed -= rotationSpeed * rotationFrictionAir * Time.deltaTime;
			rotationSpeed += num2 * num3 * Time.deltaTime;
			tiltRotation += rotationSpeed * Time.deltaTime;
		}
		else
		{
			if (physics.IsOnWall() && !physics.hasWallGrip && !physics.sliding)
			{
				vector = Vector3.up;
			}
			if (flag)
			{
				vector = grapplingHook.GetHookDir();
			}
			float num5 = Vector3.Angle(vector, GetUp());
			float num6 = RotationSign(vector);
			float num7 = 0f;
			if ((bool)animatedModel)
			{
				num7 = cachedCharacterModel.groundRotationSpeed;
			}
			tiltRotation += num5 * num6 * num7 * Time.deltaTime;
			rotationSpeed = 0f;
		}
	}

	private void LateUpdate()
	{
		if ((ghostInitialized || !hasGhostGraphics) && (bool)animatedModel)
		{
			animatedModel.transform.localRotation = Quaternion.Euler(0f, cachedCharacterModel.rotationOffset, 0f);
		}
	}

	private void OnEventLand(float impactAmount)
	{
		if ((ghostInitialized || !hasGhostGraphics) && (bool)animator)
		{
			animator.SetFloat("ImpactVel", impactAmount);
		}
	}

	private void OnEventJump(bool wallJump)
	{
		if ((ghostInitialized || !hasGhostGraphics) && (bool)animator)
		{
			animator.SetTrigger("Jump");
			float num = 0f - Mathf.Sign(physics.GetGroundNormalX());
			bool flag = !cachedCharacterModel.noFastRunningFlips;
			float flipSpeed = cachedCharacterModel.flipSpeed;
			float velocityBeforeFlip = cachedCharacterModel.velocityBeforeFlip;
			if (physics.IsOnWall())
			{
				rotationSpeed += 1300f * num;
			}
			else if (physics.groundNormal.y < 0.5f)
			{
				rotationSpeed += 800f * num;
			}
			else if (playerCollider.velocity.magnitude >= velocityBeforeFlip && flag)
			{
				rotationSpeed += Mathf.Sign(physics.GetHorizontalSpeed()) * (0f - flipSpeed);
			}
		}
	}

	public float RotationSign(Vector3 targetUpVector)
	{
		float result = 1f;
		if (Vector3.Dot(Vector3.Cross(GetUp(), targetUpVector), base.transform.forward) < 0f)
		{
			result = -1f;
		}
		return result;
	}

	public void OnCharacterSelected()
	{
		animator.SetBool("Selected", value: true);
	}

	public float predictMotionLeft(float startVel, float accel, float friction)
	{
		if (accel > 0f || friction < 0f || (accel == 0f && friction == 0f))
		{
			return float.PositiveInfinity;
		}
		float num = startVel;
		float num2 = 0f;
		int num3 = 0;
		while (num > 0f && num3 < 1000)
		{
			num += accel * Time.fixedDeltaTime;
			num -= num * friction * Time.fixedDeltaTime;
			num2 += num * Time.fixedDeltaTime;
			num3++;
		}
		return num2;
	}

	public Vector3 GetUp()
	{
		return rotatingChild.up;
	}

	public Transform GetRightWrist()
	{
		if ((bool)animatedModel && (bool)animatedModel.GetComponent<CharacterModel>())
		{
			return animatedModel.GetComponent<CharacterModel>().rightWristBone;
		}
		return null;
	}

	private void InitParticles()
	{
		ParticleSystem[] componentsInChildren;
		if (hasGhostGraphics)
		{
			componentsInChildren = particles.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				bool num = particleSystem.gameObject.name == "LongTrail" || particleSystem.gameObject.name.StartsWith("Direction Burst");
				bool flag = Singleton<PlayerManager>.SP.IsNetworkedPlayer(base.gameObject) || Singleton<IRCManager>.SP.allSpawnedGhosts.Contains(base.gameObject);
				if (!num || flag)
				{
					particleSystem.gameObject.SetActive(value: false);
				}
			}
		}
		foreach (ParticleOverrideStruct particleOverride in animatedModel.GetComponent<CharacterModel>().particleOverrides)
		{
			if (particleOverride.showOnGhost)
			{
				particleOverride.overrideObject.SetActive(value: true);
			}
		}
		if (controller.localPlayerNumber == -1)
		{
			return;
		}
		float num2 = Singleton<PlayerManager>.SP.GetLocalPlayers().Length;
		componentsInChildren = particles.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem2 in componentsInChildren)
		{
			float num3 = 1.2f - num2 * 0.2f;
			Color startColor = particleSystem2.startColor;
			startColor.a *= num3;
			particleSystem2.startColor = startColor;
			if ((bool)particleSystem2.GetComponent<ParticleInterpolate>())
			{
				Color lerpTowardsColor = particleSystem2.GetComponent<ParticleInterpolate>().lerpTowardsColor;
				lerpTowardsColor.a *= num3;
				particleSystem2.GetComponent<ParticleInterpolate>().lerpTowardsColor = lerpTowardsColor;
			}
			if ((bool)particleSystem2.GetComponent<ParticleEffect>() && particleSystem2.GetComponent<ParticleEffect>().effect == ParticleEffects.BasicTrail)
			{
				Color color = NGUIText.ParseColor24(Singleton<LocalMultiManager>.SP.playerBrightColorCodes[GetComponent<PlatformerController>().localPlayerNumber], 1);
				startColor = particleSystem2.startColor;
				startColor.r = color.r;
				startColor.g = color.g;
				startColor.b = color.b;
				particleSystem2.startColor = startColor;
				if ((bool)particleSystem2.GetComponent<ParticleInterpolate>())
				{
					Color lerpTowardsColor2 = particleSystem2.GetComponent<ParticleInterpolate>().lerpTowardsColor;
					lerpTowardsColor2.r = color.r;
					lerpTowardsColor2.g = color.g;
					lerpTowardsColor2.b = color.b;
					particleSystem2.GetComponent<ParticleInterpolate>().lerpTowardsColor = lerpTowardsColor2;
				}
			}
		}
	}

	public void ToggleParticles(bool toggle)
	{
		if (!(particles == null))
		{
			particles.gameObject.SetActive(toggle);
		}
	}

	public void SetGhostSkin(string medalReplay, CharacterSelect.Characters replayCharacter, int replaySkin)
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Replay)
		{
			Level currentLevelObj = Singleton<LevelBatchManager>.SP.GetCurrentLevelObj();
			if (currentLevelObj.levelType == LevelType.Bonus)
			{
				SetModel(currentLevelObj.challenger, currentLevelObj.challengerSkinNum);
			}
			else
			{
				SetModel(replayCharacter, replaySkin);
			}
		}
		else
		{
			if (!hasGhostGraphics)
			{
				return;
			}
			if (medalReplay == string.Empty)
			{
				SetModel(replayCharacter, replaySkin);
				if ((bool)ghostNameLabel)
				{
					ghostNameLabel.SetGhostName(GetComponent<ReplayController>().playerName);
				}
				return;
			}
			_ = Color.white;
			switch (medalReplay)
			{
			case "Bronze":
				replayCharacter = CharacterSelect.Characters.Betsy;
				replaySkin = 9;
				ghostNameLabel.SetGhostName(Language.Get("BRONZEMEDAL", "GENERIC"));
				break;
			case "Silver":
				replayCharacter = CharacterSelect.Characters.Afronaut;
				replaySkin = 9;
				ghostNameLabel.SetGhostName(Language.Get("SILVERMEDAL", "GENERIC"));
				break;
			case "Gold":
				replayCharacter = CharacterSelect.Characters.Kentony;
				replaySkin = 9;
				ghostNameLabel.SetGhostName(Language.Get("GOLDMEDAL", "GENERIC"));
				break;
			case "Rainbow":
				replayCharacter = CharacterSelect.Characters.Henk;
				replaySkin = 8;
				ghostNameLabel.SetGhostName(Language.Get("RAINBOWMEDAL", "GENERIC"));
				break;
			case "Challenge":
				ghostNameLabel.SetGhostName(string.Empty);
				break;
			}
			SetModel(replayCharacter, replaySkin);
		}
	}

	public void SetRotatorTargetLocalPos(Vector3 localPos)
	{
		rotatorTargetLocalPos = localPos;
		rotatingChild.localPosition = rotatorTargetLocalPos;
	}

	public void SetRotatorAtWorldPos(Vector3 worldPos)
	{
		physicsInterpolator.transform.localPosition = Vector3.zero;
		SnapPhysicsInterpolator();
		rotatingChild.position = worldPos;
	}

	public void SnapPhysicsInterpolator()
	{
		physicsInterpolator.transform.localPosition = Vector3.zero;
		physicsInterpolator.transform.localRotation = Quaternion.identity;
		prevPos = base.transform.position;
		nextPos = base.transform.position;
	}

	public void SetDirection(float targetDir)
	{
		targetDirection = targetDir;
		yRotation = targetDirection;
	}

	public void ParticleEvent(SfxEvents sfxEvent)
	{
		if ((bool)animatedModel)
		{
			AdditionalSkinParticles component = animatedModel.GetComponent<AdditionalSkinParticles>();
			if (component != null)
			{
				component.Play(sfxEvent);
			}
			AdditionalSkinAudio component2 = animatedModel.GetComponent<AdditionalSkinAudio>();
			if (component2 != null)
			{
				component2.Play(sfxEvent);
			}
		}
	}

	public void GoToPodium(Transform podiumSpot)
	{
		if (hasGhostGraphics)
		{
			UpdateGhostAlpha(1f);
		}
		OnReset();
		rotatingChild.localPosition = Vector3.zero;
		rotatingChild.localEulerAngles = Vector3.zero;
		SnapPhysicsInterpolator();
		if (directionIndicator != null)
		{
			directionIndicator.SetActive(value: false);
		}
		if (hitInfoIndicator != null)
		{
			hitInfoIndicator.SetActive(value: false);
		}
		animatedModel.transform.localPosition = new Vector3(0f, animatedModel.GetComponent<CharacterModel>().modelYOffset, 0f);
		ResetAnimations();
		ToggleParticles(toggle: false);
		base.transform.position = podiumSpot.position;
		base.transform.rotation = podiumSpot.rotation;
		ToggleParticles(toggle: true);
		if ((bool)ghostNameLabel)
		{
			ghostNameLabel.SetGhostName(string.Empty);
		}
		if ((bool)GetComponent<PlayerNetworking>())
		{
			GetComponent<PlayerNetworking>().onPodium = true;
		}
		if (GetComponent<PlatformerController>().isExternalControlled)
		{
			GetComponent<PlatformerController>().isExternalControlled = false;
			SetModel(currentCharacter, currentSkinNum);
		}
		tauntIndicator.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		tauntIndicator.transform.localPosition = new Vector3(0f, 2.2f, 0f);
	}

	public void DoTaunt()
	{
		if (!taunting)
		{
			StartCoroutine("TauntRoutine");
		}
	}

	private IEnumerator TauntRoutine()
	{
		taunting = true;
		tauntIndicator.SetActive(value: true);
		yield return new WaitForSeconds(0.85f);
		tauntIndicator.SetActive(value: false);
		yield return new WaitForSeconds(1.1f);
		taunting = false;
	}

	public void ResetAnimations()
	{
		if ((bool)animator)
		{
			animator.SetFloat("Velocity", 0f);
			animator.SetBool("OnGround", value: true);
			animator.SetBool("Jump", value: false);
			animator.SetBool("OnWall", value: false);
			animator.SetBool("GrapplingHook", value: false);
			animator.SetBool("Sliding", value: false);
			animator.SetTrigger("Reset");
		}
	}

	public bool IsAlive()
	{
		return !isDead;
	}

	public void KillPlayer()
	{
		if (isDead)
		{
			return;
		}
		isDead = true;
		if (base.gameObject.activeSelf)
		{
			rotatingChild.localPosition = Vector3.zero;
			rotatingChild.localRotation = Quaternion.Euler(0f, 180f - yRotation, 0f);
			SnapPhysicsInterpolator();
			if (hitInfoIndicator != null)
			{
				hitInfoIndicator.renderer.enabled = false;
			}
			if (directionIndicator != null)
			{
				directionIndicator.GetComponentInChildren<MeshRenderer>().enabled = false;
			}
			animatedModel.transform.localPosition = new Vector3(0f, animatedModel.GetComponent<CharacterModel>().modelYOffset, 0f);
			ResetAnimations();
			MakeTransparent();
			UpdateGhostAlpha(0.8f);
		}
	}

	public void SetPlayerAlive()
	{
		if (isDead)
		{
			isDead = false;
			SetModel(currentCharacter, currentSkinNum);
		}
	}

	public float GetTiltRotation()
	{
		return tiltRotation;
	}
}
