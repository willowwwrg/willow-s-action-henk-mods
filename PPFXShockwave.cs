using System.Collections;
using UnityEngine;

public class PPFXShockwave : MonoBehaviour
{
	public enum Axis
	{
		up,
		down,
		left,
		right,
		forward,
		back
	}

	private Camera referenceCamera;

	public bool reverseFace;

	public Axis axis;

	private float duration;

	public float scale;

	public bool loop;

	public bool lookAt;

	public Vector3 GetAxis(Axis refAxis)
	{
		return refAxis switch
		{
			Axis.down => Vector3.down, 
			Axis.forward => Vector3.forward, 
			Axis.back => Vector3.back, 
			Axis.left => Vector3.left, 
			Axis.right => Vector3.right, 
			_ => Vector3.up, 
		};
	}

	private void Awake()
	{
		if (!referenceCamera)
		{
			referenceCamera = Camera.main;
		}
		StartCoroutine(StartAnimation());
	}

	private void Update()
	{
		if (lookAt)
		{
			Vector3 worldPosition = base.transform.position + referenceCamera.transform.rotation * ((!reverseFace) ? Vector3.back : Vector3.forward);
			Vector3 worldUp = referenceCamera.transform.rotation * GetAxis(axis);
			base.transform.LookAt(worldPosition, worldUp);
		}
	}

	private IEnumerator StartAnimation()
	{
		base.transform.localScale = new Vector3(0f, 0f, 0f);
		base.transform.renderer.material.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
		StartCoroutine(Animate());
		yield return null;
	}

	private IEnumerator Animate()
	{
		float t = 0f;
		while (t < 1f)
		{
			base.transform.localScale = new Vector3(Mathf.Lerp(0f, scale, t / 0.5f), Mathf.Lerp(0f, scale, t / 0.5f), Mathf.Lerp(0f, scale, t / 0.5f));
			base.transform.renderer.material.SetColor("_TintColor", Color.Lerp(new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0f), t / 0.5f));
			t += Time.deltaTime;
			yield return null;
		}
	}
}
