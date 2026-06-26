using Rewired;
using UnityEngine;
using XInputDotNetPure;

public class PlatformerController : MonoBehaviour
{
	private PlatformerPhysics player;

	private RaycastCollider playerCollider;

	private bool hasControl;

	public bool isExternalControlled;

	private float rumble;

	private float lastRumble;

	private float rumbleFriction = 6f;

	private float rumbleSlide = 4f;

	private float rumbleMultiplier = 0.01f;

	private int lastGamePadUsed;

	private bool enableRumble = true;

	private int controllerUpdateFrequency = 10;

	private int controllerUpdateCounter;

	public int localPlayerNumber = -1;

	private void Awake()
	{
		hasControl = true;
		player = GetComponent<PlatformerPhysics>();
		playerCollider = GetComponent<RaycastCollider>();
		enableRumble = PlayerPrefs.GetInt("Options_EnableRumble", 1) == 1;
		if (GetComponent<LMPTaunter>() == null)
		{
			base.gameObject.AddComponent<LMPTaunter>();
		}
	}

	private void OnReset()
	{
		ResetInput();
	}

	private void ResetInput()
	{
		player.input.slideInput = false;
		player.input.jumpInput = false;
		player.input.walkInput = 0f;
		player.input.verticalInput = 0f;
		player.input.abilityInput = false;
	}

	private void FixedUpdate()
	{
		if (!player || isExternalControlled)
		{
			return;
		}
		if (hasControl)
		{
			if (localPlayerNumber != -1)
			{
				int num = localPlayerNumber;
				player.input.slideInput = Singleton<ControllerInput>.SP.GetKey(num, "LT");
				player.input.jumpInput = Singleton<ControllerInput>.SP.GetKey(num, "A");
				player.input.abilityInput = Singleton<ControllerInput>.SP.GetKey(num, "X");
				player.input.walkInput = Mathf.Clamp(ReInput.players.GetPlayer(num).GetAxisRaw("triggerHorizontal") * 2.5f, -1f, 1f);
			}
			else
			{
				player.input.slideInput = Singleton<InputManager>.SP.CheckActionContinuous(InputAction.Slide) || Singleton<InputManager>.SP.CheckTriggerContinuous(InputAction.Slide) > 0.1f;
				player.input.jumpInput = Singleton<InputManager>.SP.CheckActionContinuous(InputAction.Jump);
				player.input.abilityInput = Singleton<InputManager>.SP.CheckActionContinuous(InputAction.Ability);
				player.input.walkInput = Singleton<InputManager>.SP.GetJoystickHorizontalTriggerClamped();
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					player.input.walkInput -= 1f;
				}
				if (Input.GetKey(KeyCode.RightArrow))
				{
					player.input.walkInput += 1f;
				}
				player.input.walkInput = Mathf.Clamp(player.input.walkInput, -1f, 1f);
				player.input.verticalInput = 0f;
			}
			player.input.walkInput = HenkUtils.CapFloat(player.input.walkInput, 100);
			player.input.verticalInput = HenkUtils.CapFloat(player.input.verticalInput, 100);
			if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.InvertedControls || Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.OppositeCamera)
			{
				player.input.walkInput = 0f - player.input.walkInput;
			}
			if (Singleton<MutatorManager>.SP.GetActiveMutator() == Mutator.InvertedControls)
			{
				bool slideInput = player.input.slideInput;
				player.input.slideInput = player.input.jumpInput;
				player.input.jumpInput = slideInput;
			}
		}
		if (!HenkUtils.is64Bit() && enableRumble && localPlayerNumber == -1)
		{
			controllerUpdateCounter++;
			controllerUpdateCounter %= controllerUpdateFrequency;
			if (controllerUpdateCounter == 0)
			{
				UpdateLastGamePad();
			}
			rumble -= rumble * rumbleFriction * Time.fixedDeltaTime;
			if (player.onGround && player.sliding && playerCollider.velocity.magnitude > 20f && Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Joystick)
			{
				rumble += playerCollider.velocity.magnitude * rumbleSlide * Time.fixedDeltaTime;
			}
			float num2 = rumble * rumbleMultiplier;
			if (num2 < 0.1f)
			{
				num2 = 0f;
			}
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			if (num2 != lastRumble)
			{
				GamePad.SetVibration((PlayerIndex)lastGamePadUsed, num2, num2);
			}
			lastRumble = num2;
		}
	}

	public void UpdateLastGamePad()
	{
		for (int num = 3; num >= 0; num--)
		{
			GamePadState state = GamePad.GetState((PlayerIndex)num);
			if (state.Buttons.A == ButtonState.Pressed || state.ThumbSticks.Left.X != 0f)
			{
				if (lastGamePadUsed != num)
				{
					RemoveRumble();
				}
				lastGamePadUsed = num;
			}
		}
	}

	public void GiveControl()
	{
		hasControl = true;
	}

	public void RemoveControl()
	{
		hasControl = false;
		ResetInput();
	}

	public bool HasControl()
	{
		return hasControl;
	}

	private void OnDestroy()
	{
		RemoveRumble();
	}

	public static void RemoveRumble()
	{
		if (!HenkUtils.is64Bit())
		{
			for (int num = 3; num >= 0; num--)
			{
				GamePad.SetVibration((PlayerIndex)num, 0f, 0f);
			}
		}
	}
}
