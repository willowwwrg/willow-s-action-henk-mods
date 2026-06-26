using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorGUISelectableObj : MonoBehaviour
{
	public int catNum;

	private List<int> category;

	public int objectNum = -1;

	public GameObject tooltip;

	private void Start()
	{
		category = Singleton<LevelEditorManager>.SP.categories[catNum];
	}

	private void Update()
	{
	}

	public void OnMouseOver()
	{
		foreach (InputObject levelEditorSelectableObject in Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameLevelEditor>().levelEditorSelectableObjects)
		{
			if (levelEditorSelectableObject.GetComponent<LevelEditorGUISelectableObj>() == this)
			{
				Highlight(state: true);
			}
			else
			{
				levelEditorSelectableObject.GetComponent<LevelEditorGUISelectableObj>().Highlight(state: false);
			}
		}
	}

	public void OnMouseDown()
	{
		GameObject gameObject = null;
		if (objectNum == -1)
		{
			gameObject = Singleton<EditorCursor>.SP.SpawnCategoryObject(category);
			Singleton<EditorCursor>.SP.UpdatePrevNextBlockButtons();
		}
		else
		{
			gameObject = Singleton<EditorCursor>.SP.SpawnNonCategoryObject(objectNum);
		}
		if ((bool)gameObject)
		{
			Highlight(state: false);
			StartCoroutine(CloseToyboxEOF());
		}
	}

	private IEnumerator CloseToyboxEOF()
	{
		yield return new WaitForEndOfFrame();
		Singleton<EditorCursor>.SP.ToggleToybox(state: false);
	}

	public void OnMouseExit()
	{
		Highlight(state: false);
	}

	public void Highlight(bool state)
	{
		if (!tooltip.activeInHierarchy && state)
		{
			AudioController.Play("lvledit_rollover");
		}
		tooltip.SetActive(state);
		RotateTransformBehaviour componentInChildren = GetComponentInChildren<RotateTransformBehaviour>();
		componentInChildren.enabled = state;
		if (!state)
		{
			componentInChildren.transform.rotation = Quaternion.Euler(Vector3.zero);
		}
	}
}
