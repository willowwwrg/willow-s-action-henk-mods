using UnityEngine;

public class UVAnimate : MonoBehaviour
{
	public Vector2 speed = Vector2.zero;

	private Vector2 offset = Vector2.zero;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		offset += speed * Time.fixedDeltaTime;
		base.renderer.material.SetTextureOffset("_MainTex", offset);
	}
}
