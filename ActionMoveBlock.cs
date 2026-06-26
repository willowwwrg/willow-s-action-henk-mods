using UnityEngine;

public struct ActionMoveBlock : EditorAction
{
	public Vector3 splineOffset;

	public Vector3 rotation;

	public Vector3 scale;

	public GameObject block;

	public void Undo()
	{
	}

	public void AddToHistoryStack(GameObject obj)
	{
	}
}
