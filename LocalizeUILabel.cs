using UnityEngine;

public class LocalizeUILabel : MonoBehaviour
{
	private UILabel label;

	public string LocalizationString = string.Empty;

	public string SheetName = string.Empty;

	public bool hasControllerVariant;

	private bool playingWithController = true;

	private void Awake()
	{
		label = GetComponent<UILabel>();
		UpdateLabelOnce();
	}

	public void SetLabelInfo(string localizationString, string sheetName)
	{
		LocalizationString = localizationString;
		SheetName = sheetName;
		UpdateLabelOnce();
	}

	public void UpdateLabelOnce()
	{
		if (LocalizationString == string.Empty || SheetName == string.Empty)
		{
			return;
		}
		if (hasControllerVariant && playingWithController)
		{
			label.text = Language.Get(LocalizationString + "_C", SheetName);
			return;
		}
		if (!label)
		{
			label = GetComponent<UILabel>();
		}
		label.text = Language.Get(LocalizationString, SheetName);
	}

	private void FixedUpdate()
	{
		if (hasControllerVariant)
		{
			if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Keyboard && playingWithController)
			{
				playingWithController = false;
				UpdateLabelOnce();
			}
			else if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Joystick && !playingWithController)
			{
				playingWithController = true;
				UpdateLabelOnce();
			}
		}
	}
}
