using System.Collections;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
	public float objectResetTimer = 2f;

	public void OnTriggerEnter()
	{
		base.animation.Play();
		StartCoroutine(ObjectTimeOut());
	}

	private IEnumerator ObjectTimeOut()
	{
		yield return new WaitForSeconds(objectResetTimer);
		base.gameObject.SampleAnimation(base.animation.clip, 0f);
	}

	public void TriggerAnimation()
	{
		GetComponent<Animation>().Play();
	}
}
