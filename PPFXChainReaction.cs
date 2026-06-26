using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPFXChainReaction : MonoBehaviour
{
	public List<GameObject> objects = new List<GameObject>();

	public List<float> cloneTime = new List<float>();

	public List<Vector3> clonePosition = new List<Vector3>();

	public bool destroyLastPrefab;

	private GameObject container;

	private float duration;

	private bool isLooping;

	private int currentIndex;

	private void Start()
	{
		StartCoroutine(CloneIndex(currentIndex));
	}

	private IEnumerator CloneIndex(int _inx)
	{
		yield return new WaitForSeconds(cloneTime[_inx]);
		if (destroyLastPrefab)
		{
			Object.Destroy(container);
		}
		container = Object.Instantiate(objects[_inx], base.transform.position + clonePosition[_inx], objects[_inx].transform.rotation) as GameObject;
		container.transform.parent = base.transform;
		ParticleSystem component = container.GetComponent<ParticleSystem>();
		if (component != null)
		{
			duration = component.duration;
			isLooping = component.loop;
			StartCoroutine(Wait(cloneTime[_inx]));
		}
		yield return null;
	}

	private IEnumerator Wait(float _time)
	{
		yield return new WaitForSeconds(_time);
		currentIndex++;
		if (currentIndex < objects.Count)
		{
			StartCoroutine(CloneIndex(currentIndex));
			yield break;
		}
		yield return new WaitForSeconds(duration);
		if (!isLooping)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
