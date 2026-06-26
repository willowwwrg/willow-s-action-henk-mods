using UnityEngine;

public class ControllerKeyboardLabel : MonoBehaviour
{
	public string controllerText = string.Empty;

	public string keyboardText = string.Empty;

	private string activeText;

	private void Update()
	{
		if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Keyboard && activeText != keyboardText)
		{
			SetLabel(keyboardText);
		}
		else if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Joystick && activeText != controllerText)
		{
			SetLabel(controllerText);
		}
	}

	private void SetLabel(string text)
	{
		activeText = text;
		GetComponent<UILabel>().text = activeText;
	}
}
