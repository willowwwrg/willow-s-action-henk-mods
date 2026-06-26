using System.Collections;
using UnityEngine;

public class TentacleTrigger : MonoBehaviour
{
	private Vector3 startPos;

	private Vector3 targetPos;

	private bool movingUp;

	private void Awake()
	{
		startPos = base.transform.localPosition;
		startPos.y -= 35f;
		targetPos = base.transform.localPosition;
		base.transform.localPosition = startPos;
	}

	public void OnTriggerEnter()
	{
		StartCoroutine(MoveRoutine());
		base.animation.Play();
	}

	private void Update()
	{
		if (base.animation.isPlaying)
		{
			if (movingUp)
			{
				base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, targetPos, Time.deltaTime * 1.6f);
			}
			else
			{
				base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, startPos, Time.deltaTime * 1.6f);
			}
		}
	}

	private IEnumerator MoveRoutine()
	{
		if (!base.animation.isPlaying)
		{
			movingUp = true;
			yield return new WaitForSeconds(1.6f);
			movingUp = false;
		}
		else
		{
			yield return new WaitForSeconds(0.1f);
		}
	}
}
