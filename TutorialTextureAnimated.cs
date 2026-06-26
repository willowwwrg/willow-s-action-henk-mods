using UnityEngine;

public class TutorialTextureAnimated : MonoBehaviour
{
	public Texture2D keyboardTexture;

	public Texture2D controllerTexture;

	public Texture2D keyboardTexture2;

	public Texture2D controllerTexture2;

	public int materialNum;

	public float swapTime = 0.5f;

	private Texture2D currentTex;

	private void Start()
	{
	}

	private void Update()
	{
		bool flag = Mathf.FloorToInt(Time.time / swapTime) % 2 == 0;
		if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Keyboard)
		{
			Texture2D texture2D = ((!flag) ? keyboardTexture : keyboardTexture2);
			if (currentTex != texture2D)
			{
				base.renderer.materials[materialNum].mainTexture = texture2D;
				currentTex = texture2D;
			}
		}
		else if (Singleton<InputManager>.SP.GetInputType() == InputManager.InputType.Joystick)
		{
			Texture2D texture2D2 = ((!flag) ? controllerTexture : controllerTexture2);
			if (currentTex != texture2D2)
			{
				base.renderer.materials[materialNum].mainTexture = texture2D2;
				currentTex = texture2D2;
			}
		}
	}
}
