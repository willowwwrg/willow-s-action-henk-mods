using UnityEngine;

public class SpawnAndAttachPrefab : MonoBehaviour
{
	public GameObject prefabToSpawnAndAttach;

	private void Start()
	{
		GameObject obj = (GameObject)Object.Instantiate(prefabToSpawnAndAttach);
		obj.transform.parent = base.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localEulerAngles = Vector3.zero;
		obj.transform.localScale = Vector3.one;
	}
}
