using System.Collections;
using UnityEngine;

public class SlomoTrigger : MonoBehaviour
{
	public float timeScale = 1f;

	public float duration = 1f;

	public CameraState cameraState;

	private float targetTimeScale = 1f;

	private bool go;

	public void OnReset()
	{
		go = false;
		StopCoroutine("End");
		Singleton<GetRoot>.SP.Get().BroadcastMessage("SlomoOffHard", SendMessageOptions.DontRequireReceiver);
		targetTimeScale = timeScale;
		if ((bool)Camera.main.GetComponent<AudioLowPassFilter>())
		{
			Camera.main.GetComponent<AudioLowPassFilter>().enabled = false;
		}
		Time.timeScale = 1f;
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject gameObject = other.transform.root.gameObject;
		if (!go && targetTimeScale != 1f && Singleton<PlayerManager>.SP.IsLocalPlayer(gameObject) && !gameObject.GetComponent<PlatformerController>().isExternalControlled)
		{
			go = true;
			AudioController.Play("SlowDown");
			Singleton<GetRoot>.SP.Get().BroadcastMessage("SlomoOn", SendMessageOptions.DontRequireReceiver);
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(cameraState);
			if ((bool)Camera.main.GetComponent<AudioLowPassFilter>())
			{
				Camera.main.GetComponent<AudioLowPassFilter>().enabled = true;
			}
			StartCoroutine("End");
		}
	}

	private IEnumerator End()
	{
		yield return new WaitForSeconds(duration);
		targetTimeScale = 1f;
		go = true;
		AudioController.Play("SpeedUp");
		Singleton<GetRoot>.SP.Get().BroadcastMessage("SlomoOff", SendMessageOptions.DontRequireReceiver);
		Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
	}

	private void FixedUpdate()
	{
		if (!go)
		{
			return;
		}
		Time.timeScale -= (Time.timeScale - targetTimeScale) * 10f * Time.fixedDeltaTime;
		if (Mathf.Abs(Time.timeScale - targetTimeScale) < 0.1f)
		{
			Time.timeScale = targetTimeScale;
			go = false;
			if (targetTimeScale == 1f && (bool)Camera.main.GetComponent<AudioLowPassFilter>())
			{
				Camera.main.GetComponent<AudioLowPassFilter>().enabled = false;
			}
		}
	}
}
