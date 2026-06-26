using UnityEngine;

public class InputObject : MonoBehaviour
{
	public InputObject selectOnUp;

	public InputObject selectOnDown;

	public InputObject selectOnLeft;

	public InputObject selectOnRight;

	public void ClickMe()
	{
		UIButtonMessage[] components = GetComponents<UIButtonMessage>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].ClickMe();
		}
	}

	public void OnHover(object val)
	{
		if (base.transform.GetComponent<UIButtonMessageArguments>() != null)
		{
			if (base.transform.GetComponent<UIButtonMessageArguments>().GetGameobjects().Count > 0 && (bool)val)
			{
				base.transform.root.BroadcastMessage("LevelSelectItemSelected", base.transform.GetComponent<UIButtonMessageArguments>().GetGameobjects()[0], SendMessageOptions.DontRequireReceiver);
			}
			if (base.transform.GetComponent<UIButtonMessageArguments>().GetStrings().Count > 0 && (bool)val)
			{
				base.transform.root.BroadcastMessage("LobbyItemSelected", SendMessageOptions.DontRequireReceiver);
			}
			if (base.transform.GetComponent<UIButtonMessageArguments>().GetInts().Count > 0 && (bool)val)
			{
				base.transform.root.BroadcastMessage("SceneSelectItemSelected", (LevelStyle)base.transform.GetComponent<UIButtonMessageArguments>().GetInts()[0], SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
