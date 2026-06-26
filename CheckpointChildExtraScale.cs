using UnityEngine;

public class CheckpointChildExtraScale : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.parent.localScale.x < 0f)
		{
			base.transform.localScale = new Vector3(0f - base.transform.localScale.x, base.transform.localScale.y, 0f - base.transform.localScale.z);
			base.transform.Rotate(0f, 180f, 0f);
		}
	}
}
