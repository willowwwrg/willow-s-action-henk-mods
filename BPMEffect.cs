using UnityEngine;

public class BPMEffect : MonoBehaviour
{
	public BPMEffectStyle effect;

	public bool[] measures;

	public bool[] songInfo = new bool[20];

	private Transform[] allChildren;

	private int prevPart = -1;

	private int prevMeasure = -1;

	private float baseTint;

	private float targetTint = 1f;

	private Vector3 baseScale = Vector3.one;

	public Vector3 scaleAmount = new Vector3(1.2f, 1.2f, 1.5f);

	private Vector3 targetScale = Vector3.one;

	public string additionalAudioEffect = string.Empty;

	private bool enabledThisMeasure;

	private void Start()
	{
		if (!IsPowerOfTwo(measures.Length))
		{
			Debug.LogError("BPM Effect Measures needs to be 0, 2, 4, 8, 16 or 32");
			base.enabled = false;
		}
		allChildren = new Transform[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (base.transform.GetChild(i).gameObject.activeSelf)
			{
				allChildren[i] = base.transform.GetChild(i);
			}
		}
		if (effect == BPMEffectStyle.ToggleTint)
		{
			baseTint = base.renderer.material.GetColor("_TintColor").a;
		}
		else if (effect == BPMEffectStyle.ScaleBurst)
		{
			baseScale = base.transform.localScale;
		}
	}

	private bool IsPowerOfTwo(int x)
	{
		return (x & (x - 1)) == 0;
	}

	private void Update()
	{
		if (!Camera.main)
		{
			return;
		}
		float num = Singleton<AudioManager>.SP.GetCurrentMusicTime() - 0.45f;
		if (num < 0f)
		{
			num = 0f;
		}
		float num2 = 132f;
		float num3 = 60f / num2 * 8f;
		int num4 = Mathf.FloorToInt(num / num3);
		if (num4 != prevMeasure)
		{
			int num5 = num4 % songInfo.Length;
			enabledThisMeasure = songInfo[num5];
			if (!enabledThisMeasure)
			{
				TriggerEffect(onOrOff: false);
			}
			else if (measures.Length == 0)
			{
				TriggerEffect(onOrOff: true);
			}
			prevMeasure = num4;
		}
		if (measures.Length != 0 && enabledThisMeasure)
		{
			float num6 = num3 / (float)measures.Length;
			int num7 = Mathf.FloorToInt(Mathf.Repeat(num, num3) / num6);
			if (num7 != prevPart)
			{
				prevPart = num7;
				if (prevPart >= 0 && prevPart < measures.Length)
				{
					TriggerEffect(measures[prevPart]);
				}
			}
		}
		if (effect == BPMEffectStyle.ToggleTint)
		{
			Color color = base.renderer.material.GetColor("_TintColor");
			float a = color.a;
			float to = targetTint * baseTint;
			float num8 = (color.a = Mathf.Lerp(a, to, Time.deltaTime * 22f));
			base.renderer.material.SetColor("_TintColor", color);
			if (num8 < 0.01f && base.renderer.enabled)
			{
				base.renderer.enabled = false;
			}
			else if (!base.renderer.enabled)
			{
				base.renderer.enabled = true;
			}
		}
		else if (effect == BPMEffectStyle.ScaleBurst)
		{
			Vector3 localScale = base.transform.localScale;
			localScale = Vector3.Lerp(localScale, targetScale, Time.deltaTime * 10f);
			base.transform.localScale = localScale;
		}
	}

	private void TriggerEffect(bool onOrOff)
	{
		if (effect == BPMEffectStyle.SetChildrenActive)
		{
			Transform[] array = allChildren;
			foreach (Transform transform in array)
			{
				if ((bool)transform)
				{
					transform.gameObject.SetActive(onOrOff);
				}
			}
		}
		else if (effect == BPMEffectStyle.ToggleChildrenRenderers)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = onOrOff;
			}
		}
		else if (effect == BPMEffectStyle.ParticleBurst && onOrOff)
		{
			ParticleSystem[] componentsInChildren2 = GetComponentsInChildren<ParticleSystem>();
			for (int k = 0; k < componentsInChildren2.Length; k++)
			{
				componentsInChildren2[k].Play();
			}
			if (additionalAudioEffect != string.Empty)
			{
				AudioController.Play(additionalAudioEffect, base.transform);
			}
		}
		else if (effect == BPMEffectStyle.ToggleTint)
		{
			targetTint = (onOrOff ? 1 : 0);
		}
		else if (effect == BPMEffectStyle.ScaleBurst)
		{
			targetScale = ((!onOrOff) ? Vector3.one : scaleAmount);
		}
	}
}
