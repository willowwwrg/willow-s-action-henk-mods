using UnityEngine;

public class GUI_LevelSelectMultiplayer : MonoBehaviour
{
	public void Button_Back()
	{
		Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
	}
}
