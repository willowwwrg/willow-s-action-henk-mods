using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClickOrTapToExplode : MonoBehaviour
{
	private void OnMouseDown()
	{
		StartExplosion();
	}

	private void Update()
	{
		Touch[] touches = Input.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			if (touch.phase == TouchPhase.Began && Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out var hitInfo) && !(hitInfo.collider != base.collider))
			{
				StartExplosion();
				break;
			}
		}
	}

	private void StartExplosion()
	{
		BroadcastMessage("Explode");
		Object.Destroy(base.gameObject);
	}
}
