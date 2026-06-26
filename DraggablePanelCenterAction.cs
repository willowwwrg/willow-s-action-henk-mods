using System.Collections;
using UnityEngine;

public class DraggablePanelCenterAction : MonoBehaviour
{
	public UIScrollView dragPanel;

	public UISprite selectionSprite;

	public GUI_LevelSelectCampaign guiScriptLevelSelect;

	public GUI_Multiplayer guiScriptMultiplayer;

	private void OnSelectItem(bool delayed)
	{
		Vector3 vector = dragPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(base.transform.position);
		if (Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_LevelSelectCampaign)) || Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_Multiplayer)))
		{
			if (!(guiScriptLevelSelect != null))
			{
				Debug.LogError("Something went wrong when selecting a list item.");
				return;
			}
			int num = 0;
			int num2 = 0;
			if (num2 != -1 && num > 0)
			{
				switch (num2)
				{
				case 0:
					vector.y -= 185f;
					break;
				case 1:
					vector.y -= 85f;
					break;
				case 2:
					vector.y -= -15f;
					break;
				default:
					if (num2 == num - 1)
					{
						vector.y -= -195f;
					}
					else if (num2 == num - 2)
					{
						vector.y -= -95f;
					}
					else if (num2 == num - 3)
					{
						vector.y -= 5f;
					}
					else
					{
						vector.y -= 5f;
					}
					break;
				}
			}
			else
			{
				vector.y -= -5f;
			}
		}
		else
		{
			vector.y -= -5f;
		}
		if (delayed)
		{
			StartCoroutine(DelayedSelect(vector));
		}
		else
		{
			SpringPanel.Begin(dragPanel.gameObject, -vector, 6f);
			ToggleSelectionSprite(state: true);
		}
		Singleton<GetRoot>.SP.Get().BroadcastMessage("SelectMe", this, SendMessageOptions.DontRequireReceiver);
	}

	private void OnClick()
	{
		OnSelectItem(delayed: false);
	}

	private IEnumerator DelayedSelect(Vector3 newPos)
	{
		yield return new WaitForSeconds(0.2f);
		SpringPanel.Begin(dragPanel.gameObject, -newPos, 4f);
		ToggleSelectionSprite(state: true);
	}

	public void ToggleSelectionSprite(bool state)
	{
		if (guiScriptLevelSelect == null && guiScriptMultiplayer == null)
		{
			return;
		}
		if (state)
		{
			DraggablePanelCenterAction[] componentsInChildren = base.transform.parent.GetComponentsInChildren<DraggablePanelCenterAction>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ToggleSelectionSprite(state: false);
			}
			if (guiScriptLevelSelect != null)
			{
				guiScriptLevelSelect.SelectedLevelButton = base.gameObject;
			}
		}
		selectionSprite.enabled = state;
	}
}
