using UnityEngine;

public class ReplayName : MonoBehaviour
{
	private Vector3 prevTargetPosition;

	private Vector3 targetPosition;

	private GameObject targetObject;

	private Vector3 ghostPos;

	public UILabel label;

	private bool visible = true;

	private float baseAlpha = 1f;

	private float yOffset = -2.25f;

	private float yOffsetLMP = 2.5f;

	private float distForDefaultScale = 20f;

	private float distForDefaultScaleLMP = 40f;

	private float scalingSpeed = 0.65f;

	private float distForFullAlpha = 25f;

	private float distForNoAlpha = 80f;

	private void Awake()
	{
		label.text = string.Empty;
	}

	private void FixedUpdate()
	{
		prevTargetPosition = targetPosition;
		targetPosition = targetObject.transform.position;
	}

	private void Update()
	{
		bool flag = targetObject.GetComponent<PlatformerController>().localPlayerNumber != -1;
		if (label.enabled && !targetObject.activeSelf)
		{
			label.enabled = false;
		}
		if (!label.enabled && targetObject.activeSelf)
		{
			label.enabled = true;
		}
		if (label.text == string.Empty || targetObject == null || !label.enabled)
		{
			return;
		}
		Vector3 vector = Vector3.Lerp(prevTargetPosition, targetPosition, Singleton<PhysicsTimeManager>.SP.interpolationValue);
		if ((bool)targetObject.GetComponent<PlayerGraphics>())
		{
			PlayerGraphics component = targetObject.GetComponent<PlayerGraphics>();
			Vector3 vector2 = component.physicsInterpolator.TransformDirection(component.rotatingChild.localPosition);
			vector += vector2;
		}
		Vector3 vector3 = Vector3.up * yOffset;
		if (flag)
		{
			vector3 = Vector3.up * yOffsetLMP;
		}
		ghostPos = Camera.main.WorldToViewportPoint(vector + vector3);
		if (flag && !Singleton<LocalMultiManager>.SP.IsPlayerAlive(targetObject))
		{
			float num = 0.03f;
			ghostPos.x = Mathf.Clamp(ghostPos.x, num, 1f - num);
			ghostPos.y = Mathf.Clamp(ghostPos.y, num, 1f - num);
		}
		float z = ghostPos.z;
		if (!flag)
		{
			float num2 = Mathf.InverseLerp(distForNoAlpha, distForFullAlpha, z);
			label.alpha = baseAlpha * num2;
		}
		if (z > distForDefaultScale)
		{
			float num3 = distForDefaultScale / z;
			float scale = 1f - (1f - num3) * scalingSpeed;
			UpdateScale(scale);
		}
		else
		{
			UpdateScale(1f);
		}
		if (flag)
		{
			if (!Singleton<LocalMultiManager>.SP.IsPlayerAlive(targetObject))
			{
				if (Mathf.Repeat(Time.time, 0.2f) > 0.1f)
				{
					ToggleOutline(toggleOn: true);
				}
				else
				{
					ToggleOutline(toggleOn: false);
				}
				int localPlayerNumber = targetObject.GetComponent<PlatformerController>().localPlayerNumber;
				GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
				foreach (GameObject gameObject in localPlayers)
				{
					if (gameObject.GetComponent<PlatformerController>().localPlayerNumber > localPlayerNumber && !Singleton<LocalMultiManager>.SP.IsPlayerAlive(gameObject))
					{
						ReplayName ghostNameLabel = gameObject.GetComponent<PlayerGraphics>().ghostNameLabel;
						float num4 = Mathf.Abs(base.transform.localPosition.x - ghostNameLabel.transform.localPosition.x);
						float num5 = 30f;
						if (num4 < num5)
						{
							ghostPos.y += (num5 - num4) / 1080f;
						}
					}
				}
			}
			else
			{
				ToggleOutline(toggleOn: false);
			}
		}
		if (ghostPos.z < 0f)
		{
			label.enabled = false;
		}
		else
		{
			label.enabled = true;
		}
		float num6 = (float)Screen.width / (float)Screen.height / 1.7777778f;
		base.transform.localPosition = new Vector3(ghostPos.x * 1920f * num6, ghostPos.y * 1080f, 0f);
	}

	public void ToggleLabel(bool state)
	{
		label.enabled = state;
		base.enabled = state;
	}

	public void UpdateAlpha(float percentage)
	{
		baseAlpha = percentage;
	}

	public void UpdateScale(float scale)
	{
		base.transform.localScale = Vector3.one * scale;
	}

	public void SetGhostName(string name)
	{
		label.text = name;
	}

	public void SetTargetObject(GameObject target)
	{
		targetObject = target;
		if (targetObject.GetComponent<PlatformerController>().localPlayerNumber != -1)
		{
			label.fontSize = 30;
			label.fontStyle = FontStyle.Bold;
			distForDefaultScale = distForDefaultScaleLMP;
		}
	}

	public void ToggleOutline(bool toggleOn)
	{
		if (toggleOn)
		{
			label.effectStyle = UILabel.Effect.Outline;
			label.effectColor = Color.white;
			label.effectDistance = new Vector2(1f, 1f);
		}
		else
		{
			label.effectStyle = UILabel.Effect.None;
		}
	}

	public void SetYOffset(float newYOffset = -2.25f)
	{
		yOffset = newYOffset;
	}
}
