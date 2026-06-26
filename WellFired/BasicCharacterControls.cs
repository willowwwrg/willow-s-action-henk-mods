using UnityEngine;

namespace WellFired;

public class BasicCharacterControls : MonoBehaviour
{
	public USSequencer cutscene;

	public float strength = 10f;

	private void Update()
	{
		if (!cutscene || !cutscene.IsPlaying)
		{
			float num = strength * Time.deltaTime;
			if (Input.GetKey(KeyCode.W))
			{
				base.rigidbody.AddRelativeForce(0f - num, 0f, 0f);
			}
			if (Input.GetKey(KeyCode.S))
			{
				base.rigidbody.AddRelativeForce(num, 0f, 0f);
			}
			if (Input.GetKey(KeyCode.A))
			{
				base.rigidbody.AddRelativeForce(0f, 0f, 0f - num);
			}
			if (Input.GetKey(KeyCode.D))
			{
				base.rigidbody.AddRelativeForce(0f, 0f, num);
			}
		}
	}
}
