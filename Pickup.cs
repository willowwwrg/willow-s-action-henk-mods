using System.Collections;
using UnityEngine;

public class Pickup : MonoBehaviour
{
	public pickupType type;

	public Transform rotatingModel;

	public GameObject groundAura;

	public GameObject pickUpEffect;

	private float startY;

	private bool visible = true;

	public float respawnTime = 1.5f;

	public float heightBobbing = 0.25f;

	public float heightBobbingSpeed = 2f;

	public float rotationWobbleSpeed = 2f;

	public Vector3 rotationWobble = Vector3.zero;

	public void OnReset()
	{
		StopCoroutine("WaitForRespawn");
		Enable();
	}

	private void Start()
	{
		startY = rotatingModel.localPosition.y;
	}

	private void Update()
	{
		Vector3 localPosition = rotatingModel.localPosition;
		localPosition.y = startY + Mathf.Sin(Time.time * heightBobbingSpeed) * heightBobbing;
		rotatingModel.localPosition = localPosition;
		float num = Mathf.Sin(Time.time * rotationWobbleSpeed);
		Vector3 vector = rotationWobble * num;
		rotatingModel.localEulerAngles = vector + Vector3.up * 180f;
	}

	private void OnTriggerEnter(Collider other)
	{
		Transform root = other.transform.root;
		if (!root.GetComponent<PlatformerController>())
		{
			return;
		}
		if (Singleton<PlayerManager>.SP.IsLocalPlayer(root.gameObject) && !root.GetComponent<GrapplingHook>().enabled)
		{
			if (root.GetComponent<PlatformerController>().localPlayerNumber == -1)
			{
				Disable();
			}
			else if ((bool)pickUpEffect)
			{
				ParticleSystem[] componentsInChildren = pickUpEffect.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Play();
				}
			}
			AudioController.Play("pickup");
			if (!root.GetComponent<PlatformerController>().isExternalControlled)
			{
				Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Hook);
			}
		}
		root.GetComponent<GrapplingHook>().enabled = true;
	}

	private IEnumerator WaitForRespawn()
	{
		yield return new WaitForSeconds(respawnTime);
		Enable();
	}

	private void Disable()
	{
		visible = false;
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		if ((bool)pickUpEffect)
		{
			ParticleSystem[] componentsInChildren2 = pickUpEffect.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].Play();
			}
		}
		SetAura(enable: false);
	}

	private void Enable()
	{
		visible = true;
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		SetAura(enable: true);
	}

	private void SetAura(bool enable)
	{
		if ((bool)groundAura)
		{
			ParticleSystem[] componentsInChildren = groundAura.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enableEmission = enable;
			}
		}
	}
}
