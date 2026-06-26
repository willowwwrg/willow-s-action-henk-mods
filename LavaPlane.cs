using UnityEngine;

public class LavaPlane : MonoBehaviour
{
	private float currentAlpha;

	private float targetAlpha;

	private AudioObject lavaAmbientSound;

	private float timeActive;

	public void Start()
	{
		if (!HenkUtils.IsInALevel())
		{
			base.enabled = false;
			base.renderer.enabled = false;
		}
		else
		{
			lavaAmbientSound = AudioController.Play("loop_lavaplane", base.transform, 0f);
		}
	}

	public void OnReset()
	{
	}

	private void FixedUpdate()
	{
		if (!Singleton<PlayerManager>.SP.GetPlayer())
		{
			if (base.renderer.enabled)
			{
				base.renderer.enabled = false;
			}
			return;
		}
		bool flag = targetAlpha == 1f;
		targetAlpha = 0f;
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		foreach (GameObject gameObject in localPlayers)
		{
			if (!gameObject.activeSelf)
			{
				continue;
			}
			Vector3 position = gameObject.transform.position;
			float num = 17f;
			if (position.y < num)
			{
				int layerMask = -16385;
				if ((Physics.Raycast(position, Vector3.down, out var hitInfo, num + 3f, layerMask) || position.y < 0.1f) && hitInfo.point.y < 0.1f)
				{
					targetAlpha = 1f;
					Camera.main.GetComponent<CameraEffectsManager>().SetFogColor(new Color(0.5f, 0f, 0f));
					Camera.main.GetComponent<CameraEffectsManager>().SetTargetFogAmount(0.01f);
					break;
				}
			}
		}
		if (flag && targetAlpha == 0f)
		{
			Camera.main.GetComponent<CameraEffectsManager>().SetTargetFogAmount(0f);
		}
		currentAlpha -= (currentAlpha - targetAlpha) * 7f * Time.fixedDeltaTime;
		Color color = base.renderer.material.color;
		color.a = currentAlpha;
		base.renderer.material.color = color;
		if ((bool)lavaAmbientSound)
		{
			lavaAmbientSound.volume = Mathf.Lerp(lavaAmbientSound.volume, currentAlpha, Time.deltaTime * 5f);
		}
		if (currentAlpha < 0.005f && base.renderer.enabled)
		{
			base.renderer.enabled = false;
		}
		if (currentAlpha > 0.005f && !base.renderer.enabled)
		{
			base.renderer.enabled = true;
		}
		timeActive += Time.deltaTime;
		if (timeActive < 2f)
		{
			currentAlpha = 0.05f;
			base.renderer.enabled = true;
		}
	}
}
