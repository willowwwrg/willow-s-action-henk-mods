using System.Collections;
using UnityEngine;

public class FlickeringUISprite : MonoBehaviour
{
	public float flickerSpeed = 1f;

	private void Start()
	{
		GetComponent<UISprite>().enabled = false;
	}

	private void Update()
	{
		if (GetComponent<UISprite>().enabled)
		{
			StartCoroutine(FlickerRoutine(state: false));
		}
		else
		{
			StartCoroutine(FlickerRoutine(state: true));
		}
	}

	private IEnumerator FlickerRoutine(bool state)
	{
		yield return new WaitForSeconds(flickerSpeed);
		GetComponent<UISprite>().enabled = state;
	}
}
