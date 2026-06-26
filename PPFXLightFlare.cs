using System.Collections;
using UnityEngine;

public class PPFXLightFlare : MonoBehaviour
{
	public Light li;

	public float fadeTime;

	private float t;

	private void Start()
	{
		StartCoroutine(Fade());
	}

	private IEnumerator Fade()
	{
		t = 0f;
		while (t < fadeTime)
		{
			t += Time.deltaTime;
			li.intensity = Mathf.Lerp(1f, 0f, t / fadeTime);
			yield return null;
		}
	}
}
