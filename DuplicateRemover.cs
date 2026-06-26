using UnityEngine;

public class DuplicateRemover : MonoBehaviour
{
	private void Start()
	{
		if (GameObject.FindGameObjectsWithTag(base.gameObject.tag).Length > 1)
		{
			Debug.Log(base.name + ": Duplicate object with tag: \"" + base.gameObject.tag + "\" found and eliminated.");
			Object.Destroy(base.gameObject);
			Singleton<ActionHenk>.SP.HardResetGame("Duplicate root.");
		}
	}
}
