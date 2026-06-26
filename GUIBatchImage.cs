using System.Collections;
using UnityEngine;

public class GUIBatchImage : MonoBehaviour
{
	public int batchNum;

	public GameObject hoverItems;

	public UILabel batchLabel;

	public GameObject bgBar;

	private string originalSprite;

	private void Awake()
	{
		originalSprite = "batch_" + (batchNum + 1) + "_selected";
		InitImage();
	}

	public void InitImage()
	{
		GetComponent<UISprite>().spriteName = originalSprite;
		LevelBatch batchFromNum = Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum);
		if (batchFromNum != null)
		{
			if (!batchFromNum.IsUnlocked())
			{
				SetLockImage(state: true);
			}
			else if (batchFromNum.playUnlockAnim)
			{
				StartCoroutine(PlayUnlockAnimRoutine(batchFromNum));
			}
			else
			{
				SetLockImage(state: false);
			}
			SetBatchName(batchFromNum.batchName);
		}
		else
		{
			SetLockImage(state: true);
			SetBatchName(string.Empty);
		}
	}

	private IEnumerator PlayUnlockAnimRoutine(LevelBatch batch)
	{
		batch.playUnlockAnim = false;
		HenkUtils.FindTransformInHierarchy(base.transform, "lock").GetComponent<PositionShake>().enabled = true;
		AudioController.Play("Finish_improved");
		yield return new WaitForSeconds(1f);
		HenkUtils.FindTransformInHierarchy(base.transform, "lock").GetComponent<PositionShake>().enabled = false;
		TweenAlpha[] componentsInChildren = HenkUtils.FindTransformInHierarchy(base.transform, "lock").GetComponentsInChildren<TweenAlpha>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play(forward: true);
		}
		HenkUtils.FindTransformInHierarchy(base.transform, "lock").GetComponentInChildren<TweenPosition>().Play(forward: true);
		yield return new WaitForSeconds(1.2f);
		SetLockImage(state: false);
	}

	private void SetLockImage(bool state)
	{
		if (base.gameObject.activeInHierarchy)
		{
			UISprite[] componentsInChildren = HenkUtils.FindTransformInHierarchy(base.transform, "lock").GetComponentsInChildren<UISprite>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = state;
			}
			if (state)
			{
				GetComponent<UISprite>().spriteName = "batch_" + (batchNum + 1) + "_unlocked";
			}
			else
			{
				GetComponent<UISprite>().spriteName = originalSprite;
			}
		}
	}

	private void SetBatchName(string batchName)
	{
		if (Singleton<LevelBatchManager>.SP.GetBatchFromNum(batchNum).IsUnlocked())
		{
			batchLabel.text = batchName;
		}
		else
		{
			batchLabel.text = "??????";
		}
	}

	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			GetComponent<UIButtonMessage>().target.SendMessage("HoverOverBatchButton", base.gameObject);
			hoverItems.SetActive(value: true);
		}
		else
		{
			hoverItems.SetActive(value: false);
		}
	}
}
