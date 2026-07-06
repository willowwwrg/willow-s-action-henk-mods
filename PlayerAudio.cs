using System.Collections;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
	private PlatformerPhysics physics;

	private RaycastCollider playerCollider;

	private PlatformerController controller;

	private PlayerGraphics playerGraphics;

	private AudioObject slideAudio;

	private AudioObject slideFire;

	private AudioObject wallSlide;

	private AudioObject skateRoll;

	private void Awake()
	{
		physics = GetComponent<PlatformerPhysics>();
		playerCollider = GetComponent<RaycastCollider>();
		controller = GetComponent<PlatformerController>();
		playerGraphics = GetComponent<PlayerGraphics>();
	}

	private void Update()
	{
		if (!physics.isJumping)
		{
			AudioController.Stop("Jump", 0.1f);
			AudioController.Stop("WallJump", 0.1f);
		}
		float num = 1f;
		if (Singleton<PlayerManager>.SP.IsGhost(base.gameObject))
		{
			float value = Vector3.Distance(Singleton<PlayerManager>.SP.GetPlayer().transform.position, base.transform.position);
			num = Mathf.InverseLerp(30f, 10f, value);
		}
		bool flag = physics.IsOnWall() && !physics.hasWallGrip;
		slideAudio = PlayLoopingSound(slideAudio, physics.sliding && physics.onGround, "Slide", 10f, 60f, 1.5f, 0.3f, 0f, 30f);
		if ((bool)slideAudio)
		{
			slideAudio.volume *= num;
		}
		slideFire = PlayLoopingSound(slideFire, physics.sliding && physics.onGround, "SlideFire", 45f, 80f, 1f, 1.5f, 20f, 50f);
		if ((bool)slideFire)
		{
			slideFire.volume *= num;
		}
		wallSlide = PlayLoopingSound(wallSlide, flag, "WallSlide", 10f, 40f, 0.5f, 0.1f, 0f, 30f);
		if ((bool)wallSlide)
		{
			wallSlide.volume *= num;
		}
		if (!controller.HasControl())
		{
			if ((bool)slideAudio)
			{
				slideAudio.volume = 0f;
			}
			if ((bool)slideFire)
			{
				slideFire.volume = 0f;
			}
			if ((bool)wallSlide)
			{
				wallSlide.volume = 0f;
			}
		}
		if (playerGraphics.currentCharacter == CharacterSelect.Characters.Cedar)
		{
			skateRoll = PlayLoopingSound(skateRoll, physics.onGround && !flag, "Cedar_skateroll", 10f, 80f, 1f, 1.4f, 0f, 50f, 0.35f);
			if ((bool)skateRoll)
			{
				skateRoll.volume *= num;
			}
		}
	}

	public void Mute()
	{
		OnDestroy();
	}

	private void OnDestroy()
	{
		if ((bool)slideAudio)
		{
			slideAudio.Stop();
		}
		if ((bool)slideFire)
		{
			slideFire.Stop();
		}
		if ((bool)wallSlide)
		{
			wallSlide.Stop();
		}
		if ((bool)skateRoll)
		{
			skateRoll.Stop();
		}
	}

	public void OnReset()
	{
	}

	private AudioObject PlayLoopingSound(AudioObject audioObject, bool playTrigger, string soundToPlay, float pitchLerpMinSpeed, float pitchLerpMaxSpeed, float minPitch, float maxPitch, float volumeLerpMinSpeed, float volumeLerpMaxSpeed, float volumeMult = 1f)
	{
		AudioObject audioObject2 = audioObject;
		if (playTrigger)
		{
			if (audioObject2 == null || !audioObject2.IsPlaying())
			{
				audioObject2 = AudioController.Play(soundToPlay);
			}
			if ((bool)playerCollider && (bool)audioObject2)
			{
				float t = Mathf.InverseLerp(pitchLerpMinSpeed, pitchLerpMaxSpeed, playerCollider.velocity.magnitude);
				audioObject2.pitch = Mathf.Lerp(minPitch, maxPitch, t);
				t = Mathf.InverseLerp(volumeLerpMinSpeed, volumeLerpMaxSpeed, playerCollider.velocity.magnitude);
				audioObject2.volumeItem = t * volumeMult;
			}
		}
		else if (audioObject2 != null)
		{
			audioObject2.Stop(0.1f);
		}
		return audioObject2;
	}

	private void OnEventJump(bool wallJump)
	{
		if (playerGraphics.currentCharacter == CharacterSelect.Characters.Cedar)
		{
			AudioController.Play("Cedar_ollie", base.transform);
		}
		else if (playerGraphics.currentCharacter == CharacterSelect.Characters.Henk && playerGraphics.currentSkinNum == 11)
		{
			if (!wallJump)
				AudioController.Play("hariojump", base.transform);
			else
				AudioController.Play("hariowalljump", base.transform);
		}
		else if (!wallJump)
			AudioController.Play("Jump", base.transform);
		else
			AudioController.Play("WallJump", base.transform);
		Singleton<AudioManager>.SP.PlayCharacterJump(base.gameObject);
	}

	private void OnEventLand(float impactAmount)
	{
		float volume = Mathf.InverseLerp(0f, 25f, impactAmount);
		if (playerGraphics.currentCharacter == CharacterSelect.Characters.Cedar)
			AudioController.Play("Cedar_ollieland", base.transform, volume);
		else
			AudioController.Play("char_land", base.transform, volume);
	}

	private void OnEventWall(float impactAmount)
	{
		float volume = Mathf.InverseLerp(5f, 40f, impactAmount);
		AudioController.Play("char_bump", base.transform, volume);
	}

	private IEnumerator LandingFootstepSequence()
	{
		AudioController.Play("Footsteps", base.transform);
		yield return new WaitForSeconds(0.1f);
		AudioController.Play("Footsteps", base.transform);
	}
}
