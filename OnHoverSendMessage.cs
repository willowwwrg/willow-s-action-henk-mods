using UnityEngine;

public class OnHoverSendMessage : MonoBehaviour
{
	public GameObject target;

	public void OnHover(bool isOver)
	{
		target.SendMessage("OnHover", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}
}
