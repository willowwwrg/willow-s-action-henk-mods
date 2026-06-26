using System.Collections.Generic;
using UnityEngine;

public class CharacterModel : MonoBehaviour
{
	public Transform rightWristBone;

	public float modelYOffset;

	public float extraOffsetForJump;

	public float onWallZOffset = 0.35f;

	public float rotationOffset;

	public bool noFastRunningFlips;

	public float velocityBeforeFlip = 35f;

	public float flipSpeed = 1300f;

	public float groundRotationSpeed = 13f;

	public List<ParticleOverrideStruct> particleOverrides;

	private void FootStep()
	{
		if ((bool)base.transform.root.GetComponent<PlayerAudio>() && base.transform.root.GetComponent<PlayerAudio>().enabled)
		{
			AudioController.Play("Footsteps", base.transform);
		}
	}
}
