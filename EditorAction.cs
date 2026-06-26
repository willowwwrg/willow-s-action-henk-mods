using UnityEngine;

public interface EditorAction
{
	void Undo();

	void AddToHistoryStack(GameObject obj);
}
