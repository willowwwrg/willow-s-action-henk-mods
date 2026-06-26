using System.Collections;
using UnityEngine;

public class PPFXCountdownDestruct : MonoBehaviour
{
	public float time;

	private void Start()
	{
		StartCoroutine(StartCountdown());
	}

	private IEnumerator StartCountdown()
	{
		yield return new WaitForSeconds(time);
		Object.Destroy(base.gameObject);
	}
}
