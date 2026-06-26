using UnityEngine;

public class ScaryTrigger : MonoBehaviour
{
	public bool isExitTrigger;

	private bool effectEnabled;

	private float targetFogDensity;

	private Color initialAmbient;

	private void Start()
	{
		initialAmbient = RenderSettings.ambientLight;
		RenderSettings.fogDensity = 0f;
		RenderSettings.fog = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject gameObject = other.transform.root.gameObject;
		if (Singleton<PlayerManager>.SP.IsLocalPlayer(gameObject) && !gameObject.GetComponent<PlatformerController>().isExternalControlled && gameObject.GetComponent<PlatformerController>().localPlayerNumber == -1)
		{
			Camera.main.GetComponent<CameraEffectsManager>().SetFogColor(new Color(0.1f, 0.1f, 0.1f));
			Camera.main.GetComponent<CameraEffectsManager>().SetTargetFogAmount(0.075f);
			AudioController.Play("spooky");
		}
	}

	private void OnTriggerExit(Collider other)
	{
		GameObject gameObject = other.transform.root.gameObject;
		if (Singleton<PlayerManager>.SP.IsLocalPlayer(gameObject) && !gameObject.GetComponent<PlatformerController>().isExternalControlled && gameObject.GetComponent<PlatformerController>().localPlayerNumber == -1)
		{
			Camera.main.GetComponent<CameraEffectsManager>().SetTargetFogAmount(0f);
			Singleton<AudioManager>.SP.PlayCharacterLaugh(gameObject);
		}
	}
}
