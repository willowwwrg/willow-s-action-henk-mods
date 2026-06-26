using System.Collections;
using UnityEngine;

[AddComponentMenu("AFS/Touch Bending/Player Listener")]
public class touchBendingPlayerListener : MonoBehaviour
{
	public float maxSpeed = 8f;

	public float Player_DampSpeed = 0.75f;

	private Transform myTransform;

	private Vector3 Player_Position;

	private Vector3 Player_OldPosition;

	public float Player_Speed;

	private float Player_NewSpeed;

	public Vector3 Player_Direction;

	private void Awake()
	{
		myTransform = base.transform;
	}

	private void Start()
	{
		Player_Position = base.transform.position;
		Player_OldPosition = Player_Position;
	}

	private void Update()
	{
		StartCoroutine(AfsPlayerDataUpdate());
	}

	private IEnumerator AfsPlayerDataUpdate()
	{
		yield return new WaitForEndOfFrame();
		Player_Position = myTransform.position;
		Player_NewSpeed = (Player_Position - Player_OldPosition).magnitude / Time.deltaTime / maxSpeed;
		float num = 1f - Mathf.Exp(-20f * Time.deltaTime);
		float num2 = 0.25f * num;
		num *= 0.125f;
		if (Player_NewSpeed < Player_Speed)
		{
			Player_Speed = Mathf.Lerp(Player_Speed, Player_NewSpeed, num * Player_DampSpeed);
		}
		else
		{
			Player_Speed = Mathf.Lerp(Player_Speed, Player_NewSpeed, num2 * Player_DampSpeed);
		}
		if (Player_Position != Player_OldPosition)
		{
			Player_Direction = Vector3.Normalize(Player_Position - Player_OldPosition);
		}
		Player_OldPosition = Player_Position;
	}
}
