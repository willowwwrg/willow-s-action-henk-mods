using UnityEngine;

public class ControllerKeyboardObjectSwap : MonoBehaviour
{
	public GameObject controllerObj;

	public GameObject keyboardObj;

	private GameObject activeObj;

	private void Start()
	{
		controllerObj.SetActive(value: false);
		keyboardObj.SetActive(value: true);
		activeObj = keyboardObj;
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Keyboard && activeObj != keyboardObj)
		{
			controllerObj.SetActive(value: false);
			keyboardObj.SetActive(value: true);
			activeObj = keyboardObj;
		}
		else if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Joystick && activeObj != controllerObj)
		{
			controllerObj.SetActive(value: true);
			keyboardObj.SetActive(value: false);
			activeObj = controllerObj;
		}
	}
}
