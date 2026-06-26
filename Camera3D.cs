public class Camera3D : Singleton<Camera3D>
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ToggleCamera(bool state)
	{
		base.camera.enabled = state;
	}
}
