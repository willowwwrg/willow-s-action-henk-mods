using UnityEngine;

public class GUISelectableItem : MonoBehaviour
{
	public void OnSelectItem()
	{
		GUI_LevelEditorPlayLevel component = Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_LevelEditorPlayLevel>();
		if (component != null)
		{
			component.SetSelectedItem(GetComponent<InputObject>());
		}
	}
}
