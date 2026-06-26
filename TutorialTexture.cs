using UnityEngine;

public class TutorialTexture : MonoBehaviour
{
	public Texture2D keyboardTexture;

	public Texture2D controllerTexture;

	public int materialNum;

	private Texture2D currentTex;

	private void Start()
	{
	}

	private void Update()
	{
		if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Keyboard && currentTex != keyboardTexture)
		{
			base.renderer.materials[materialNum].mainTexture = keyboardTexture;
			currentTex = keyboardTexture;
		}
		else if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Joystick && currentTex != controllerTexture)
		{
			base.renderer.materials[materialNum].mainTexture = controllerTexture;
			currentTex = controllerTexture;
		}
	}
}
