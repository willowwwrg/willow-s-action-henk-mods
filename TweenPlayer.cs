using UnityEngine;

public class TweenPlayer : MonoBehaviour
{
	public GameObject parent;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ResetTweens()
	{
		if (!(parent == null))
		{
			TweenPosition[] componentsInChildren = parent.GetComponentsInChildren<TweenPosition>();
			foreach (TweenPosition obj in componentsInChildren)
			{
				obj.ResetToBeginning();
				obj.enabled = false;
			}
			TweenRotation[] componentsInChildren2 = parent.GetComponentsInChildren<TweenRotation>();
			foreach (TweenRotation obj2 in componentsInChildren2)
			{
				obj2.ResetToBeginning();
				obj2.enabled = false;
			}
			TweenAlpha[] componentsInChildren3 = parent.GetComponentsInChildren<TweenAlpha>();
			foreach (TweenAlpha obj3 in componentsInChildren3)
			{
				obj3.ResetToBeginning();
				obj3.enabled = false;
			}
		}
	}

	public void PlayTweens()
	{
		if (!(parent == null))
		{
			ResetTweens();
			TweenPosition[] componentsInChildren = parent.GetComponentsInChildren<TweenPosition>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			TweenRotation[] componentsInChildren2 = parent.GetComponentsInChildren<TweenRotation>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].enabled = true;
			}
			TweenAlpha[] componentsInChildren3 = parent.GetComponentsInChildren<TweenAlpha>();
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].enabled = true;
			}
		}
	}
}
