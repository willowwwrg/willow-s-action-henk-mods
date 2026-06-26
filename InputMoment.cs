using System;
using UnityEngine;

[Serializable]
public class InputMoment
{
	public float walkInput;

	public bool jumpInput;

	public bool slideInput;

	public int triggerTime;

	public float triggerXPos = -1000f;

	public float triggerXPosVariation = 1f;

	[HideInInspector]
	public bool triggered;

	[HideInInspector]
	public bool releasedJump;

	public float triggerChance = 0.5f;

	[HideInInspector]
	public float origXPos;
}
