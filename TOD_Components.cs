using UnityEngine;

[ExecuteInEditMode]
public class TOD_Components : MonoBehaviour
{
	public GameObject Sun;

	public GameObject Moon;

	public GameObject Atmosphere;

	public GameObject Clouds;

	public GameObject Space;

	public GameObject Light;

	public GameObject Projector;

	internal Transform DomeTransform;

	internal Transform SunTransform;

	internal Transform MoonTransform;

	internal Transform CameraTransform;

	internal Transform LightTransform;

	internal Renderer SpaceRenderer;

	internal Renderer AtmosphereRenderer;

	internal Renderer CloudRenderer;

	internal Renderer SunRenderer;

	internal Renderer MoonRenderer;

	internal MeshFilter SpaceMeshFilter;

	internal MeshFilter AtmosphereMeshFilter;

	internal MeshFilter CloudMeshFilter;

	internal MeshFilter SunMeshFilter;

	internal MeshFilter MoonMeshFilter;

	internal Material SpaceShader;

	internal Material AtmosphereShader;

	internal Material CloudShader;

	internal Material SunShader;

	internal Material MoonShader;

	internal Material ShadowShader;

	internal Light LightSource;

	internal Projector ShadowProjector;

	internal TOD_Sky Sky;

	internal TOD_Animation Animation;

	internal TOD_Time Time;

	internal TOD_Weather Weather;

	internal TOD_Resources Resources;

	protected void OnEnable()
	{
		DomeTransform = base.transform;
		CameraTransform = ((!(Camera.main != null)) ? DomeTransform : Camera.main.transform);
		Sky = GetComponent<TOD_Sky>();
		Animation = GetComponent<TOD_Animation>();
		Time = GetComponent<TOD_Time>();
		Weather = GetComponent<TOD_Weather>();
		Resources = GetComponent<TOD_Resources>();
		if ((bool)Space)
		{
			SpaceRenderer = Space.renderer;
			SpaceShader = SpaceRenderer.sharedMaterial;
			SpaceMeshFilter = Space.GetComponent<MeshFilter>();
			if ((bool)Atmosphere)
			{
				AtmosphereRenderer = Atmosphere.renderer;
				AtmosphereShader = AtmosphereRenderer.sharedMaterial;
				AtmosphereMeshFilter = Atmosphere.GetComponent<MeshFilter>();
				if ((bool)Clouds)
				{
					CloudRenderer = Clouds.renderer;
					CloudShader = CloudRenderer.sharedMaterial;
					CloudMeshFilter = Clouds.GetComponent<MeshFilter>();
					if ((bool)Projector)
					{
						ShadowProjector = Projector.GetComponent<Projector>();
						ShadowShader = ShadowProjector.material;
						if ((bool)Light)
						{
							LightTransform = Light.transform;
							LightSource = Light.light;
							if ((bool)Sun)
							{
								SunTransform = Sun.transform;
								SunRenderer = Sun.renderer;
								SunShader = SunRenderer.sharedMaterial;
								SunMeshFilter = Sun.GetComponent<MeshFilter>();
								if ((bool)Moon)
								{
									MoonTransform = Moon.transform;
									MoonRenderer = Moon.renderer;
									MoonShader = MoonRenderer.sharedMaterial;
									MoonMeshFilter = Moon.GetComponent<MeshFilter>();
								}
								else
								{
									Debug.LogError("Moon reference not set. Disabling script.");
									base.enabled = false;
								}
							}
							else
							{
								Debug.LogError("Sun reference not set. Disabling script.");
								base.enabled = false;
							}
						}
						else
						{
							Debug.LogError("Light reference not set. Disabling script.");
							base.enabled = false;
						}
					}
					else
					{
						Debug.LogError("Projector reference not set. Disabling script.");
						base.enabled = false;
					}
				}
				else
				{
					Debug.LogError("Clouds reference not set. Disabling script.");
					base.enabled = false;
				}
			}
			else
			{
				Debug.LogError("Atmosphere reference not set. Disabling script.");
				base.enabled = false;
			}
		}
		else
		{
			Debug.LogError("Space reference not set. Disabling script.");
			base.enabled = false;
		}
	}
}
