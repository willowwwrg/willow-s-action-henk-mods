using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	public float waitTime;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(waitTime);
		Object.Destroy(base.gameObject);
	}
}
