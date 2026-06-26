using UnityEngine;

public class ExternalControlExample : MonoBehaviour
{
	[SerializeField]
	private CameraPathBezierAnimator pathAnimatorA;

	[SerializeField]
	private CameraPathBezierAnimator pathAnimatorB;

	private CameraPathBezierAnimator pathAnimator;

	private void Start()
	{
		if (!(pathAnimatorA == null))
		{
			pathAnimator = pathAnimatorA;
		}
	}

	private void OnGUI()
	{
		if (pathAnimator == null)
		{
			return;
		}
		if (GUILayout.Button("START"))
		{
			pathAnimator.Play();
		}
		if (GUILayout.Button("PAUSE"))
		{
			pathAnimator.Pause();
		}
		if (GUILayout.Button("STOP"))
		{
			pathAnimator.Stop();
		}
		if (GUILayout.Button("SWITCH"))
		{
			pathAnimator.Stop();
			if (pathAnimator == pathAnimatorA)
			{
				pathAnimator = pathAnimatorB;
			}
			else
			{
				pathAnimator = pathAnimatorA;
			}
			pathAnimator.Play();
		}
		if (GUILayout.Button("JUMP"))
		{
			pathAnimator.Stop();
			pathAnimator.Seek(0.75f);
			pathAnimator.Play();
		}
		if (!pathAnimator.isPlaying && GUILayout.Button("REPLAY"))
		{
			if (pathAnimator.mode != CameraPathBezierAnimator.modes.reverse)
			{
				pathAnimator.Seek(0f);
			}
			else
			{
				pathAnimator.Seek(1f);
			}
			pathAnimator.Play();
		}
		GUILayout.Space(10f);
		GUILayout.Label("ANIMATION MODE");
		GUILayout.Label("current:" + pathAnimator.mode);
		if (GUILayout.Button("FORWARD"))
		{
			pathAnimator.mode = CameraPathBezierAnimator.modes.once;
		}
		if (GUILayout.Button("REVERSE"))
		{
			pathAnimator.mode = CameraPathBezierAnimator.modes.reverse;
		}
		if (GUILayout.Button("LOOP"))
		{
			pathAnimator.mode = CameraPathBezierAnimator.modes.loop;
		}
		CameraPathBezier bezier = pathAnimator.bezier;
		GUILayout.Space(10f);
		GUILayout.Label("CAMERA MODE");
		GUILayout.Label("current:" + bezier.mode);
		if (GUILayout.Button("USER CONTROLLED"))
		{
			bezier.mode = CameraPathBezier.viewmodes.usercontrolled;
		}
		if (GUILayout.Button("MOUSE LOOK"))
		{
			bezier.mode = CameraPathBezier.viewmodes.mouselook;
		}
		if (GUILayout.Button("FOLLOW PATH"))
		{
			bezier.mode = CameraPathBezier.viewmodes.followpath;
		}
		if (GUILayout.Button("REVERSE FOLLOW PATH"))
		{
			bezier.mode = CameraPathBezier.viewmodes.reverseFollowpath;
		}
	}
}
