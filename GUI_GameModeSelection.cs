using UnityEngine;

public class GUI_GameModeSelection : MonoBehaviour
{
	public void Button_Back()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
	}
}
