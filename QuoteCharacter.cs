using UnityEngine;

public class QuoteCharacter : MonoBehaviour
{
	public float quoteDuration = 0.1f;

	private float targetY = 2f;

	private float timeAlive;

	private void Update()
	{
		timeAlive += Time.deltaTime;
		Vector3 to = new Vector3(base.transform.localPosition.x, targetY, base.transform.localPosition.z);
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, to, Time.deltaTime * 10f);
		if (timeAlive > quoteDuration)
		{
			targetY = -0.2f;
		}
		if (timeAlive > quoteDuration + 0.5f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
