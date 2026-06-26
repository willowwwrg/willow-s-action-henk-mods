using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
	public string keyName;

	private UIPopupList mList;

	private UIToggle mCheck;

	private string key
	{
		get
		{
			if (string.IsNullOrEmpty(keyName))
			{
				return "NGUI State: " + base.name;
			}
			return keyName;
		}
	}

	private void Awake()
	{
		mList = GetComponent<UIPopupList>();
		mCheck = GetComponent<UIToggle>();
	}

	private void OnEnable()
	{
		if (mList != null)
		{
			EventDelegate.Add(mList.onChange, SaveSelection);
		}
		if (mCheck != null)
		{
			EventDelegate.Add(mCheck.onChange, SaveState);
		}
		if (mList != null)
		{
			string value = PlayerPrefs.GetString(key);
			if (!string.IsNullOrEmpty(value))
			{
				mList.value = value;
			}
			return;
		}
		if (mCheck != null)
		{
			mCheck.value = PlayerPrefs.GetInt(key, 1) != 0;
			return;
		}
		string text = PlayerPrefs.GetString(key);
		UIToggle[] componentsInChildren = GetComponentsInChildren<UIToggle>(includeInactive: true);
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			UIToggle obj = componentsInChildren[i];
			obj.value = obj.name == text;
		}
	}

	private void OnDisable()
	{
		if (mCheck != null)
		{
			EventDelegate.Remove(mCheck.onChange, SaveState);
		}
		if (mList != null)
		{
			EventDelegate.Remove(mList.onChange, SaveSelection);
		}
		if (!(mCheck == null) || !(mList == null))
		{
			return;
		}
		UIToggle[] componentsInChildren = GetComponentsInChildren<UIToggle>(includeInactive: true);
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			UIToggle uIToggle = componentsInChildren[i];
			if (uIToggle.value)
			{
				PlayerPrefs.SetString(key, uIToggle.name);
				break;
			}
		}
	}

	public void SaveSelection()
	{
		PlayerPrefs.SetString(key, UIPopupList.current.value);
	}

	public void SaveState()
	{
		PlayerPrefs.SetInt(key, UIToggle.current.value ? 1 : 0);
	}
}
