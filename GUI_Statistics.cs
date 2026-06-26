using UnityEngine;

public class GUI_Statistics : MonoBehaviour
{
	public void Button_Back()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
	}
}
