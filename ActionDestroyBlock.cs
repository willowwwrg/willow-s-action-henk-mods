using UnityEngine;

public struct ActionDestroyBlock : EditorAction
{
	public Vector3 splineOffset;

	public Vector3 rotation;

	public Vector3 scale;

	public int blockID;

	public GameObject block;

	public Transform parentTransform;

	public void Undo()
	{
		block = Singleton<EditorCursor>.SP.SpawnBlock(blockID, select: false);
		block.transform.parent = parentTransform;
		block.transform.localEulerAngles = rotation;
		block.transform.localScale = scale;
		block.GetComponent<SplineFollow>().splineOffset = splineOffset;
		block.GetComponent<SplineFollow>().ForceOneUpdate();
	}

	public void AddToHistoryStack(GameObject obj)
	{
		splineOffset = obj.GetComponent<SplineFollow>().splineOffset;
		rotation = obj.transform.rotation.eulerAngles;
		scale = obj.transform.localScale;
		blockID = Singleton<LevelEditorFileWriter>.SP.GetBuildingBlockID(obj);
		parentTransform = obj.GetComponent<SelectedItem>().originalParent;
		Singleton<LevelEditorHistory>.SP.AddAction(this);
	}
}
