using UnityEngine;

public class PPFXSpawnOnClick : MonoBehaviour
{
	public GameObject inst;

	public string tagName;

	private GameObject container;

	private void Start()
	{
		container = new GameObject();
		container.name = "_Container";
	}

	private void Update()
	{
		if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && Physics.Raycast((Application.platform != RuntimePlatform.Android) ? Camera.main.ScreenPointToRay(Input.mousePosition) : Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out var hitInfo) && GUIUtility.hotControl == 0 && hitInfo.collider.tag == tagName && inst != null)
		{
			GameObject gameObject = Object.Instantiate(position: new Vector3(hitInfo.point.x, 0f, hitInfo.point.z), original: inst, rotation: inst.transform.rotation) as GameObject;
			if (container != null)
			{
				gameObject.transform.parent = container.transform;
				return;
			}
			container = new GameObject();
			container.name = "_Container";
			gameObject.transform.parent = container.transform;
		}
	}
}
