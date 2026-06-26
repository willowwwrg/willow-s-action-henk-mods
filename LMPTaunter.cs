using UnityEngine;

public class LMPTaunter : MonoBehaviour
{
	private float lastTauntTime;

	private void Update()
	{
		if (Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_LMP_Pregame && Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_LMP_Ingame && Singleton<GUIManager>.SP.GetCurrentScreenName() != GUIManager.GUIScreens.GUIScreen_LMP_Endgame)
		{
			return;
		}
		int localPlayerNumber = GetComponent<PlatformerController>().localPlayerNumber;
		if (localPlayerNumber != -1)
		{
			float num = Time.time - lastTauntTime;
			if (Singleton<ControllerInput>.SP.GetKeyDown(localPlayerNumber, "RB") && num > 2f)
			{
				lastTauntTime = Time.time;
				Singleton<AudioManager>.SP.PlayCharacterTaunt2D(base.gameObject);
				GetComponent<PlayerGraphics>().DoTaunt();
			}
		}
	}
}
