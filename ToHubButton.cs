using UnityEngine;

public class ToHubButton : MonoBehaviour
{
	public Texture2D ButtonTexture;

	private Rect ButtonRect;

	private void Start()
	{
		if (ButtonTexture == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		int num = ButtonTexture.width + 4;
		int num2 = ButtonTexture.height + 4;
		ButtonRect = new Rect(Screen.width - num, Screen.height - num2, num, num2);
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void OnGUI()
	{
		if (Application.loadedLevel != 0 && GUI.Button(ButtonRect, ButtonTexture))
		{
			PhotonNetwork.Disconnect();
			Application.LoadLevel(0);
		}
	}
}
