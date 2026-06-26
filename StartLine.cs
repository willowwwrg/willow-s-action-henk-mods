using System.Collections;
using UnityEngine;

public class StartLine : MonoBehaviour
{
	public Transform animatedModel;

	public Renderer lights;

	public GameObject gunShootEffect;

	private bool countingDown;

	private void Start()
	{
		animatedModel.animation["Take 001"].speed = 1f;
	}

	private void Update()
	{
	}

	public void DoCountDown()
	{
		StartCoroutine(CountDown());
	}

	public void ResetLights()
	{
		animatedModel.animation.Play("Take 001");
		animatedModel.animation["Take 001"].time = 0f;
		animatedModel.animation.Sample();
		animatedModel.animation.Stop("Take 001");
		for (int i = 0; i < 9; i++)
		{
			if (i == 3 || i == 5 || i == 8)
			{
				lights.materials[i].SetColor("_GlowColor", new Color(1f, 0f, 0f));
			}
			if (i == 4 || i == 6 || i == 7)
			{
				lights.materials[i].SetColor("_SpecColor", new Color(1f, 0f, 0f));
			}
		}
	}

	private IEnumerator CountDown()
	{
		if (countingDown)
		{
			yield break;
		}
		countingDown = true;
		float countDownTime = State_PreGame.GetCountDownTime();
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LocalMultiplayer)
		{
			countDownTime = State_LMP_Pregame.GetCountDownTime();
		}
		ResetLights();
		yield return new WaitForSeconds(countDownTime);
		for (int i = 0; i < 9; i++)
		{
			if (i == 3 || i == 5 || i == 8)
			{
				lights.materials[i].SetColor("_GlowColor", new Color(0f, 1f, 0f));
			}
			if (i == 4 || i == 6 || i == 7)
			{
				lights.materials[i].SetColor("_SpecColor", new Color(0f, 1f, 0f));
			}
		}
		StartCoroutine(PlayEffects());
		animatedModel.animation.Play("Take 001");
		countingDown = false;
	}

	private IEnumerator PlayEffects()
	{
		yield return new WaitForSeconds(0.1f);
		if ((bool)gunShootEffect)
		{
			ParticleSystem[] componentsInChildren = gunShootEffect.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
			AudioController.Play("StartlineGunshot", base.transform);
		}
	}
}
