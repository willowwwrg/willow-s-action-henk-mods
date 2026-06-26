using System.Collections.Generic;

public class LevelEditorHistory : Singleton<LevelEditorHistory>
{
	public List<EditorAction> history = new List<EditorAction>();

	public int undoIterator = -1;

	public void AddAction(EditorAction action)
	{
		if (undoIterator != history.Count - 1)
		{
			history.RemoveRange(undoIterator + 1, history.Count - 1 - undoIterator);
		}
		history.Add(action);
		undoIterator = history.Count - 1;
	}

	public void ClearList()
	{
		history.Clear();
	}

	public void UndoMostRecent()
	{
		if (history.Count >= 1 && undoIterator >= 0)
		{
			history[undoIterator].Undo();
			undoIterator--;
		}
	}
}
