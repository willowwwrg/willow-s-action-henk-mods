using UnityEngine;

public class SkySwab : MonoBehaviour
{
	public Sky targetSky;

	private Vector3 scale = new Vector3(1f, 1.01f, 1f);

	private Vector3 bigScale = new Vector3(1.2f, 1.21f, 1.2f);

	private Vector3 littleScale = new Vector3(0.75f, 0.76f, 0.75f);

	private Quaternion baseRot = Quaternion.identity;

	private void Start()
	{
		baseRot = base.transform.localRotation;
		scale = littleScale;
	}

	private void OnMouseDown()
	{
		if ((bool)targetSky)
		{
			targetSky.Apply();
		}
	}

	private void OnMouseOver()
	{
		scale = Vector3.one;
	}

	private void OnMouseExit()
	{
		scale = littleScale;
	}

	private void Update()
	{
		if (Sky.activeSky == targetSky)
		{
			base.transform.Rotate(0f, 200f * Time.deltaTime, 0f);
			base.transform.localScale = bigScale;
		}
		else
		{
			base.transform.localRotation = baseRot;
			base.transform.localScale = scale;
		}
	}
}
