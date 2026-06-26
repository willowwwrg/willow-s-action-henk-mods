using UnityEngine;

public class SimplePathControl : MonoBehaviour
{
	private CameraPathBezierAnimator animator;

	private void Start()
	{
		animator = GetComponent<CameraPathBezierAnimator>();
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (animator.isPlaying)
			{
				animator.Pause();
			}
			else
			{
				animator.Play();
			}
		}
		if (Input.GetMouseButtonUp(1))
		{
			animator.Seek(0f);
			if (!animator.isPlaying)
			{
				animator.Play();
			}
		}
	}
}
