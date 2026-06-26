using UnityEngine;

public class RainbowMedalVisibility : MonoBehaviour
{
	public void CheckVisibility()
	{
		if (Singleton<LevelBatchManager>.SP.RainbowMedalCount() > 0)
		{
			base.renderer.enabled = true;
		}
		else
		{
			base.renderer.enabled = false;
		}
	}
}
