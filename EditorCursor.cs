using System.Collections.Generic;
using UnityEngine;

public class EditorCursor : Singleton<EditorCursor>
{
	private enum CursorMode
	{
		Select,
		Move,
		Rotate,
		SelectInGui
	}

	private SplineFollow follow;

	public List<GameObject> selection = new List<GameObject>();

	public List<int> currentCategory;

	public List<GameObject> softSelection = new List<GameObject>();

	public bool toyboxOpen;

	public InputObject selectedGUIItem;

	private CursorMode cursorMode;

	public List<SelectedItem> hoverTargets = new List<SelectedItem>();

	public List<SelectedItem> hoverTargetsLastFrame = new List<SelectedItem>();

	public GameObject visualsObject;

	public Transform cursor;

	public Transform selectionContainer;

	private float targetRotation;

	private Vector3 selectionOffset = Vector3.zero;

	private float storedRotation;

	private bool inMultiSelect;

	private Vector3 multiSelectStart = Vector3.zero;

	private GameObject multiSelectVisuals;

	public bool menuVisible;

	private void Awake()
	{
		follow = GetComponent<SplineFollow>();
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(base.gameObject);
	}

	public void Init()
	{
		selection.Clear();
		SetCursorMode(CursorMode.Select);
	}

	private void FixedUpdate()
	{
		UpdateCursorPositionMouse();
		for (int num = hoverTargets.Count - 1; num >= 0; num--)
		{
			SelectedItem selectedItem = hoverTargets[num];
			if (!hoverTargetsLastFrame.Contains(selectedItem))
			{
				selectedItem.RevertMaterialChanges();
				hoverTargets.Remove(selectedItem);
				if (hoverTargets.Count == 0)
				{
					foreach (GameObject item in selection)
					{
						if ((bool)item.GetComponent<SelectedItem>())
						{
							item.GetComponent<SelectedItem>().MakeGreen();
						}
					}
				}
			}
		}
		hoverTargetsLastFrame.Clear();
	}

	private void Update()
	{
		if (!Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_InGameLevelEditor)))
		{
			return;
		}
		float z = Mathf.LerpAngle(selectionContainer.localEulerAngles.z, targetRotation, 15f * Time.deltaTime);
		selectionContainer.localEulerAngles = new Vector3(0f, 0f, z);
		cursor.localEulerAngles = new Vector3(0f, 0f, z);
		if (menuVisible)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.E) && selection.Count == 1 && currentCategory != null)
		{
			SpawnNextInCategory(forwards: true);
			AudioController.Play("lvledit_scrollright");
		}
		if (Input.GetKeyDown(KeyCode.Q) && selection.Count == 1 && currentCategory != null)
		{
			SpawnNextInCategory(forwards: false);
			AudioController.Play("lvledit_scrollleft");
		}
		if (Input.GetKeyDown(KeyCode.C) && selection.Count > 0)
		{
			RotateSelection(clockwise: true);
		}
		if (Input.GetKeyDown(KeyCode.Z) && selection.Count > 0)
		{
			RotateSelection(clockwise: false);
		}
		if (Input.GetKeyDown(KeyCode.X) && selection.Count > 0)
		{
			MirrorSelection(horizontal: true);
		}
		if (Input.GetKeyDown(KeyCode.Delete))
		{
			if (selection.Count > 0)
			{
				AudioController.Play("lvledit_trash");
			}
			if (cursorMode == CursorMode.Move)
			{
				ClearSelection(destroy: true);
				SetCursorMode(CursorMode.Select);
			}
		}
		if (!toyboxOpen)
		{
			UpdateMultiSelect();
			Input.GetKeyDown(KeyCode.F9);
			if (Input.GetKeyDown(KeyCode.Mouse0) && cursorMode == CursorMode.Move)
			{
				if (toyboxOpen)
				{
					return;
				}
				if (selection.Count == 0 || hoverTargets.Count != 0)
				{
					if (hoverTargets.Count > 0)
					{
						AudioController.Play("lvledit_invalid");
					}
					return;
				}
				AudioController.Play("lvledit_drop");
				if (Input.GetKey(KeyCode.LeftShift))
				{
					ClearSelection(destroy: false, clone: true);
				}
				else
				{
					ClearSelection();
				}
				if (selection.Count == 0)
				{
					SetCursorMode(CursorMode.Select);
				}
			}
			if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
			{
				if (selection.Count > 0)
				{
					AudioController.Play("lvledit_trash");
				}
				ClearSelection(destroy: true);
				SetCursorMode(CursorMode.Select);
			}
		}
		else if (Singleton<InputManager>.SP.CheckAction(InputAction.Cancel))
		{
			ToggleToybox(state: false, resetCursorState: true);
			ClearSelection(destroy: true);
			SetCursorMode(CursorMode.Select);
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			ToggleToybox(!toyboxOpen, resetCursorState: true);
		}
	}

	private void ProcessHotkeys()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[0]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[1]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[2]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[3]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[7]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[4]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[5]);
			ToggleToybox(state: false, resetCursorState: true);
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			SpawnCategoryObject(Singleton<LevelEditorManager>.SP.categories[6]);
			ToggleToybox(state: false, resetCursorState: true);
		}
	}

	public void UpdateCursorPositionMouse()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float num = Vector3.Dot(base.transform.position - base.transform.forward * 2f - ray.origin, base.transform.forward);
		float num2 = Vector3.Dot(ray.direction, base.transform.forward);
		Vector3 lhs = ray.origin + num / num2 * ray.direction - base.transform.position;
		float x = Vector3.Dot(lhs, base.transform.right);
		float y = Vector3.Dot(lhs, base.transform.up);
		follow.splineOffset += new Vector3(x, y, 0f);
		follow.ForceOneUpdate();
		float x2 = follow.splineOffset.x - Mathf.Round(follow.splineOffset.x);
		float y2 = follow.splineOffset.y - Mathf.Round(follow.splineOffset.y);
		Vector3 vector = new Vector3(x2, y2, 0f);
		Vector3 vector2 = selectionOffset;
		vector2.x *= selectionContainer.localScale.x;
		vector2.y *= selectionContainer.localScale.y;
		vector2 = selectionContainer.localRotation * vector2;
		selectionContainer.transform.localPosition = vector2 - vector;
		selectionContainer.transform.localPosition = new Vector3(selectionContainer.transform.localPosition.x, selectionContainer.transform.localPosition.y, -0.1f);
	}

	private void UpdateMultiSelect()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0) && cursorMode == CursorMode.Select && !inMultiSelect)
		{
			inMultiSelect = true;
			multiSelectStart = follow.splineOffset;
			multiSelectVisuals = (GameObject)Object.Instantiate(Resources.Load("LevelEditor/MultiSelect"));
			multiSelectVisuals.transform.position = base.transform.position - base.transform.forward * 2.1f;
			multiSelectVisuals.transform.localRotation = base.transform.localRotation;
		}
		if (inMultiSelect)
		{
			Vector3 vector = follow.splineOffset - multiSelectStart;
			BoxCollider component = GetComponent<BoxCollider>();
			component.center = new Vector3((0f - vector.x) * 0.5f, (0f - vector.y) * 0.5f, -1f);
			component.size = new Vector3(vector.x, vector.y, 2f);
			if ((bool)multiSelectVisuals)
			{
				multiSelectVisuals.transform.localScale = new Vector3(vector.x, vector.y, 0.1f);
			}
		}
		if ((Input.GetKeyUp(KeyCode.Mouse0) || cursorMode != CursorMode.Select) && inMultiSelect)
		{
			SelectWhatsUnderCursor();
			inMultiSelect = false;
			BoxCollider component2 = GetComponent<BoxCollider>();
			component2.center = new Vector3(0f, 0f, -1f);
			component2.size = new Vector3(0.01f, 0.01f, 2f);
			Object.Destroy(multiSelectVisuals);
			if (Input.GetKey(KeyCode.LeftShift))
			{
				ClearSelection(destroy: false, clone: true);
			}
		}
	}

	public bool DoesStartLineExist()
	{
		bool result = false;
		for (int i = 0; i < Singleton<LevelFileLoader>.SP.parentLevelBlocks.transform.childCount; i++)
		{
			if (Singleton<LevelFileLoader>.SP.parentLevelBlocks.transform.GetChild(i).name == "StartLine")
			{
				result = true;
			}
		}
		for (int j = 0; j < Singleton<LevelFileLoader>.SP.parentCheckpoints.transform.childCount; j++)
		{
			if (Singleton<LevelFileLoader>.SP.parentCheckpoints.transform.GetChild(j).name == "StartLine")
			{
				result = true;
			}
		}
		return result;
	}

	public void ToggleToybox(bool state, bool resetCursorState = false)
	{
		if (state)
		{
			AudioController.Play("lvledit_trayopen");
		}
		else
		{
			AudioController.Play("lvledit_trayclose");
		}
		toyboxOpen = state;
		Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameLevelEditor>().ToggleToybox(state);
		if (state)
		{
			if (cursorMode != CursorMode.SelectInGui)
			{
				SetCursorMode(CursorMode.SelectInGui);
			}
			return;
		}
		Singleton<InputManager>.SP.Deselect();
		if (resetCursorState)
		{
			SetCursorMode(CursorMode.Move);
		}
	}

	public bool SaveLevel(string name)
	{
		return Singleton<LevelEditorFileWriter>.SP.ExportLevel("levelName", editorLevel: true);
	}

	public bool IsObjectUniqueAndUsed(int id)
	{
		string text = Singleton<LevelEditorManager>.SP.buildingBlocks[id].name;
		if (GameObject.Find(text) != null)
		{
			switch (text)
			{
			case "Finishline":
				return true;
			case "StartLine":
				return true;
			case "Pickup_Hook":
				return true;
			case "Checkpoint":
				return true;
			}
		}
		return false;
	}

	private void SelectWhatsUnderCursor()
	{
		foreach (SelectedItem hoverTarget in hoverTargets)
		{
			SelectBlock(hoverTarget);
		}
		_ = hoverTargets.Count;
		if (hoverTargets.Count != 0)
		{
			SetCursorMode(CursorMode.Move);
			hoverTargets.Clear();
			AudioController.Play("lvledit_select");
		}
	}

	public GameObject SpawnBlock(int id, bool select = true)
	{
		ClearSelection(destroy: true);
		if (IsObjectUniqueAndUsed(id) && id != 76)
		{
			SetCursorMode(CursorMode.Select);
			return null;
		}
		Singleton<LevelEditorManager>.SP.buildingBlockHistory.Add(id);
		SnapToTargetRotation();
		GameObject gameObject = Object.Instantiate(Singleton<LevelEditorManager>.SP.buildingBlocks[id]) as GameObject;
		gameObject.name = Singleton<LevelEditorManager>.SP.buildingBlocks[id].name;
		gameObject.transform.position = base.transform.position;
		gameObject.transform.rotation = selectionContainer.rotation;
		gameObject.transform.localScale = selectionContainer.localScale;
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(gameObject);
		BackToOrignalPosition();
		SplineFollow splineFollow = gameObject.GetComponent<SplineFollow>();
		if (!splineFollow)
		{
			splineFollow = gameObject.AddComponent<SplineFollow>();
		}
		splineFollow.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		splineFollow.splineOffset = follow.splineOffset;
		splineFollow.ForceOneUpdate();
		SelectedItem selectedItem = gameObject.AddComponent<SelectedItem>();
		selectedItem.SwitchToEditMode();
		if (select)
		{
			selectedItem.newlyCreated = true;
			selection.Add(gameObject);
			selectedItem.MakeGreen();
		}
		SetCursorMode(CursorMode.Move);
		return gameObject;
	}

	public GameObject SpawnCategoryObject(List<int> cat, int blockNum = 0)
	{
		currentCategory = cat;
		return SpawnBlock(currentCategory[blockNum]);
	}

	public GameObject SpawnNonCategoryObject(int objectNum)
	{
		currentCategory = null;
		return SpawnBlock(objectNum);
	}

	private GameObject SpawnNextInCategory(bool forwards)
	{
		if (selection.Count == 1)
		{
			for (int i = 0; i < currentCategory.Count; i++)
			{
				if (!(selection[0].name == Singleton<LevelEditorManager>.SP.buildingBlocks[currentCategory[i]].name))
				{
					continue;
				}
				int num = 0;
				if (forwards)
				{
					num = i + 1;
					if (num > currentCategory.Count - 1)
					{
						num = 0;
					}
				}
				else
				{
					num = i - 1;
					if (num < 0)
					{
						num = currentCategory.Count - 1;
					}
				}
				SpawnCategoryObject(currentCategory, num);
				break;
			}
			UpdatePrevNextBlockButtons();
		}
		return null;
	}

	public int GetPrevNextBlockID(bool forwards)
	{
		if (selection.Count == 1)
		{
			for (int i = 0; i < currentCategory.Count; i++)
			{
				if (!(selection[0].name == Singleton<LevelEditorManager>.SP.buildingBlocks[currentCategory[i]].name))
				{
					continue;
				}
				int num = 0;
				if (forwards)
				{
					num = i + 1;
					if (num > currentCategory.Count - 1)
					{
						num = 0;
					}
				}
				else
				{
					num = i - 1;
					if (num < 0)
					{
						num = currentCategory.Count - 1;
					}
				}
				return currentCategory[num];
			}
		}
		return -1;
	}

	public void UpdatePrevNextBlockButtons(bool show = true)
	{
		if (selection.Count != 0)
		{
			SpawnPreviewBlock(next: true);
			SpawnPreviewBlock(next: false);
			TogglePrevNextWindow(state: true);
		}
	}

	public void TogglePrevNextWindow(bool state)
	{
		Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameLevelEditor>().prevNextObj.SetActive(state);
	}

	private void SpawnPreviewBlock(bool next)
	{
		GUI_InGameLevelEditor component = Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameLevelEditor>();
		GameObject gameObject = ((!next) ? component.prevItemAnchor : component.nextItemAnchor);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			Object.Destroy(gameObject.transform.GetChild(i).gameObject);
		}
		GameObject gameObject2 = Object.Instantiate(Singleton<LevelEditorManager>.SP.buildingBlocks[GetPrevNextBlockID(next)]) as GameObject;
		gameObject2.layer = 11;
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject2.transform.localPosition = Vector3.zero;
		Vector3[] vertices = gameObject2.GetComponent<MeshCollider>().sharedMesh.vertices;
		float a = 10000f;
		float num = -10000f;
		float a2 = 10000f;
		float num2 = -10000f;
		for (int j = 0; j < vertices.Length; j++)
		{
			a = Mathf.Min(a, vertices[j].x);
			num = Mathf.Max(num, vertices[j].x);
			a2 = Mathf.Min(a2, vertices[j].y);
			num2 = Mathf.Max(num2, vertices[j].y);
		}
		gameObject2.transform.localPosition = new Vector3(gameObject2.transform.localPosition.x - num / 2f, gameObject2.transform.localPosition.y - num2 / 2f, gameObject2.transform.localPosition.z);
		gameObject.transform.localScale = new Vector3(7f, 7f, 7f);
	}

	public void GoToIngame()
	{
		Singleton<CheckpointManager>.SP.LevelEditorUpdate();
		ClearSelection(destroy: true);
		SetCursorMode(CursorMode.Select);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		GetComponent<BoxCollider>().enabled = false;
		base.enabled = false;
		Singleton<AudioManager>.SP.PlayIngameTheme();
	}

	public void GoToEditorMode()
	{
		SetCursorMode(CursorMode.Select);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		GetComponent<BoxCollider>().enabled = true;
		base.enabled = true;
	}

	private void RotateSelection(bool clockwise)
	{
		if (clockwise)
		{
			AudioController.Play("lvledit_rotatecw");
		}
		else
		{
			AudioController.Play("lvledit_rotateccw");
		}
		float num = 90f;
		if (!clockwise)
		{
			num = -90f;
		}
		targetRotation -= num;
		if (targetRotation < 0f)
		{
			targetRotation += 360f;
		}
		else if (targetRotation > 360f)
		{
			targetRotation -= 360f;
		}
		targetRotation = Mathf.Round(targetRotation);
	}

	private void MirrorSelection(bool horizontal)
	{
		AudioController.Play("lvledit_mirror");
		if (Mathf.Abs(Mathf.DeltaAngle(selectionContainer.localEulerAngles.z, 90f)) < 45f || Mathf.Abs(Mathf.DeltaAngle(selectionContainer.localEulerAngles.z, 270f)) < 45f)
		{
			horizontal = !horizontal;
		}
		if (horizontal)
		{
			selectionContainer.localScale = new Vector3(0f - selectionContainer.localScale.x, 1f, 1f);
		}
		else
		{
			selectionContainer.localScale = new Vector3(1f, 0f - selectionContainer.localScale.y, 1f);
		}
		cursor.localScale = selectionContainer.localScale;
	}

	private void Select(GameObject obj)
	{
		if (!(obj == null))
		{
			selection.Add(obj);
		}
	}

	private void SetCursorMode(CursorMode mode)
	{
		cursorMode = mode;
		switch (mode)
		{
		case CursorMode.Move:
		{
			if (selection.Count == 0)
			{
				cursorMode = CursorMode.Select;
				break;
			}
			base.collider.enabled = false;
			SnapToTargetRotation();
			selectionContainer.transform.localPosition = Vector3.zero;
			for (int i = 0; i < selection.Count; i++)
			{
				if (!(selection[i] == null))
				{
					selection[i].transform.parent = selectionContainer;
				}
			}
			float x = follow.splineOffset.x - Mathf.Round(follow.splineOffset.x);
			float y = follow.splineOffset.y - Mathf.Round(follow.splineOffset.y);
			selectionOffset = new Vector3(x, y);
			selectionOffset = Quaternion.Inverse(selectionContainer.localRotation) * selectionOffset;
			selectionOffset.x *= selectionContainer.localScale.x;
			selectionOffset.y *= selectionContainer.localScale.y;
			BackToOrignalPosition();
			RefreshRigidBody();
			break;
		}
		case CursorMode.Select:
			ClearSelection();
			base.collider.enabled = true;
			targetRotation = 0f;
			selectionContainer.localScale = Vector3.one;
			cursor.localScale = Vector3.one;
			break;
		case CursorMode.SelectInGui:
			if (selectedGUIItem == null)
			{
				selectedGUIItem = Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_InGameLevelEditor>().levelEditorSelectableObjects[0];
			}
			Singleton<InputManager>.SP.Select(selectedGUIItem, delayedTillEndOfFrame: false, playSound: false);
			break;
		case CursorMode.Rotate:
			break;
		}
	}

	private void SelectBlock(SelectedItem item)
	{
		item.originalParent = item.transform.root;
		item.pickedUp = true;
		item.MakeGreen();
		selection.Add(item.gameObject);
	}

	private void ClearSelection(bool destroy = false, bool clone = false)
	{
		SnapToTargetRotation();
		List<GameObject> list = selection;
		if (clone)
		{
			for (int i = 0; i < selection.Count; i++)
			{
				if (IsObjectUniqueAndUsed(Singleton<LevelEditorFileWriter>.SP.GetBuildingBlockID(selection[i])))
				{
					clone = false;
					Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_CANTCLONE", "PERMA"));
					SetCursorMode(CursorMode.Select);
				}
			}
		}
		if (clone)
		{
			List<GameObject> list2 = new List<GameObject>();
			foreach (GameObject item in selection)
			{
				item.GetComponent<SelectedItem>().RevertMaterialChanges();
				GameObject gameObject = (GameObject)Object.Instantiate(item, item.transform.position, item.transform.rotation);
				gameObject.transform.parent = item.transform.parent;
				gameObject.name = item.name;
				gameObject.transform.localScale = item.transform.localScale;
				list2.Add(gameObject);
				item.GetComponent<SelectedItem>().MakeGreen();
			}
			list = list2;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == null)
			{
				continue;
			}
			GameObject gameObject2 = list[j];
			gameObject2.transform.parent = Singleton<LevelFileLoader>.SP.parentLevelBlocks.transform;
			if (destroy)
			{
				SelectedItem component = gameObject2.GetComponent<SelectedItem>();
				if (component != null)
				{
					gameObject2.SendMessage("ObjectDestroyed", SendMessageOptions.DontRequireReceiver);
					if (component.pickedUp)
					{
						default(ActionDestroyBlock).AddToHistoryStack(gameObject2);
						Object.Destroy(gameObject2);
					}
					else
					{
						Object.Destroy(gameObject2);
					}
				}
				else
				{
					Debug.LogError("Error: No SelectedItem class on this object: " + gameObject2.name);
				}
				continue;
			}
			Vector3 lhs = gameObject2.transform.position - base.transform.position;
			float x = Vector3.Dot(lhs, base.transform.right);
			float y = Vector3.Dot(lhs, base.transform.up);
			gameObject2.GetComponent<SplineFollow>().splineOffset = follow.splineOffset + new Vector3(x, y, 0f);
			gameObject2.GetComponent<SplineFollow>().ForceOneUpdate();
			gameObject2.SendMessage("ObjectPlaced", SendMessageOptions.DontRequireReceiver);
			SelectedItem component2 = gameObject2.GetComponent<SelectedItem>();
			if (component2 != null)
			{
				if (component2.newlyCreated)
				{
					default(ActionCreateBlock).AddToHistoryStack(gameObject2);
				}
				else
				{
					default(ActionMoveBlock).AddToHistoryStack(gameObject2);
				}
				component2.DeselectItem();
			}
			else
			{
				Debug.LogError("Error: No SelectedItem class on this object: " + gameObject2.name);
			}
		}
		if (!clone)
		{
			TogglePrevNextWindow(state: false);
		}
		BackToOrignalPosition();
		list.Clear();
		RefreshRigidBody();
	}

	private void OnTriggerStay(Collider other)
	{
		SelectedItem component = other.transform.GetComponent<SelectedItem>();
		if (!component && (bool)other.transform.parent)
		{
			component = other.transform.parent.GetComponent<SelectedItem>();
		}
		if (!component)
		{
			return;
		}
		if (!hoverTargets.Contains(component))
		{
			hoverTargets.Add(component);
			foreach (GameObject item in selection)
			{
				if ((bool)item.GetComponent<SelectedItem>())
				{
					item.GetComponent<SelectedItem>().MakeRed();
				}
			}
			if (cursorMode == CursorMode.Select)
			{
				component.MakeHovered();
			}
		}
		hoverTargetsLastFrame.Add(component);
	}

	private void SnapToTargetRotation()
	{
		storedRotation = selectionContainer.localEulerAngles.z;
		selectionContainer.localEulerAngles = new Vector3(0f, 0f, targetRotation);
	}

	private void BackToOrignalPosition()
	{
		selectionContainer.localEulerAngles = new Vector3(0f, 0f, storedRotation);
	}

	public void RefreshRigidBody()
	{
		base.transform.parent = Singleton<LevelFileLoader>.SP.parentLevelBlocks.transform;
		base.transform.parent = null;
	}
}
