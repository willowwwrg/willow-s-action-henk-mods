using UnityEngine;

public struct ActionCreateBlock : EditorAction
{
	public Vector3 splineOffset;

	public Vector3 rotation;

	public Vector3 scale;

	public int blockID;

	public GameObject block;

	public Transform parentTransform;

	public void Undo()
	{
		if ((bool)block)
		{
			Object.Destroy(block);
			return;
		}
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			SplineFollow component = gameObject.GetComponent<SplineFollow>();
			if ((bool)component && component.splineOffset == splineOffset && Singleton<LevelEditorFileWriter>.SP.GetBuildingBlockID(gameObject) == blockID)
			{
				Object.Destroy(gameObject);
				break;
			}
		}
	}

	public void AddToHistoryStack(GameObject obj)
	{
		splineOffset = obj.GetComponent<SplineFollow>().splineOffset;
		rotation = obj.transform.rotation.eulerAngles;
		scale = obj.transform.localScale;
		block = obj;
		blockID = Singleton<LevelEditorFileWriter>.SP.GetBuildingBlockID(obj);
		parentTransform = obj.GetComponent<SelectedItem>().originalParent;
		Singleton<LevelEditorHistory>.SP.AddAction(this);
	}
}
