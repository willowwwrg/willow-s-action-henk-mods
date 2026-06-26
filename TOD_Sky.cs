using System;
using UnityEngine;

[ExecuteInEditMode]
public class TOD_Sky : MonoBehaviour
{
	public enum ColorSpaceDetection
	{
		Auto,
		Linear,
		Gamma
	}

	public enum CloudQualityType
	{
		Fastest,
		Density,
		Bumped
	}

	public enum MeshQualityType
	{
		Low,
		Medium,
		High
	}

	private const float pi = (float)Math.PI;

	private const float pi2 = 9.869605f;

	private const float pi3 = 31.006279f;

	private const float pi4 = 97.4091f;

	private Vector2 opticalDepth;

	private Vector3 oneOverBeta;

	private Vector3 betaRayleigh;

	private Vector3 betaRayleighTheta;

	private Vector3 betaMie;

	private Vector3 betaMieTheta;

	private Vector2 betaMiePhase;

	private Vector3 betaNight;

	public ColorSpaceDetection UnityColorSpace;

	public CloudQualityType CloudQuality = CloudQualityType.Bumped;

	public MeshQualityType MeshQuality = MeshQualityType.High;

	public TOD_CycleParameters Cycle;

	public TOD_AtmosphereParameters Atmosphere;

	public TOD_DayParameters Day;

	public TOD_NightParameters Night;

	public TOD_LightParameters Light;

	public TOD_StarParameters Stars;

	public TOD_CloudParameters Clouds;

	public TOD_WorldParameters World;

	internal TOD_Components Components { get; private set; }

	internal bool IsDay => LerpValue > 0f;

	internal bool IsNight => LerpValue == 0f;

	internal float Radius => Components.DomeTransform.localScale.x;

	internal float Gamma
	{
		get
		{
			if ((UnityColorSpace == ColorSpaceDetection.Auto && QualitySettings.activeColorSpace == ColorSpace.Linear) || UnityColorSpace == ColorSpaceDetection.Linear)
			{
				return 1f;
			}
			return 2.2f;
		}
	}

	internal float OneOverGamma
	{
		get
		{
			if ((UnityColorSpace == ColorSpaceDetection.Auto && QualitySettings.activeColorSpace == ColorSpace.Linear) || UnityColorSpace == ColorSpaceDetection.Linear)
			{
				return 1f;
			}
			return 0.45454544f;
		}
	}

	internal float LerpValue { get; private set; }

	internal float SunZenith { get; private set; }

	internal float MoonZenith { get; private set; }

	internal float LightZenith => Mathf.Min(SunZenith, MoonZenith);

	internal float LightIntensity => Components.LightSource.intensity;

	internal Vector3 MoonDirection => Components.MoonTransform.forward;

	internal Vector3 SunDirection => Components.SunTransform.forward;

	internal Vector3 LightDirection => Vector3.Lerp(MoonDirection, SunDirection, LerpValue * LerpValue);

	internal Color LightColor => Components.LightSource.color;

	internal Color SunColor { get; private set; }

	internal Color MoonColor { get; private set; }

	internal Color MoonHaloColor { get; private set; }

	internal Color CloudColor { get; private set; }

	internal Color AdditiveColor { get; private set; }

	internal Color AmbientColor
	{
		get
		{
			if (World.SetAmbientLight)
			{
				return RenderSettings.ambientLight;
			}
			return SampleAmbientColor();
		}
	}

	internal Color FogColor
	{
		get
		{
			if (World.SetFogColor)
			{
				return RenderSettings.fogColor;
			}
			return SampleFogColor();
		}
	}

	internal Vector3 OrbitalToUnity(float radius, float theta, float phi)
	{
		float num = Mathf.Sin(theta);
		float num2 = Mathf.Cos(theta);
		float num3 = Mathf.Sin(phi);
		float num4 = Mathf.Cos(phi);
		return new Vector3
		{
			z = radius * num * num4,
			y = radius * num2,
			x = radius * num * num3
		};
	}

	internal Color SampleAtmosphere(Vector3 direction)
	{
		direction = Components.DomeTransform.InverseTransformDirection(direction);
		float horizonOffset = World.HorizonOffset;
		float p = Atmosphere.Contrast * 0.45454544f;
		float haziness = Atmosphere.Haziness;
		float fogginess = Atmosphere.Fogginess;
		Color sunColor = SunColor;
		Color moonColor = MoonColor;
		Color cloudColor = CloudColor;
		Color additiveColor = AdditiveColor;
		Vector3 rhs = Components.DomeTransform.InverseTransformDirection(SunDirection);
		Vector3 vector = opticalDepth;
		Vector3 vector2 = oneOverBeta;
		Vector3 vector3 = betaRayleigh;
		Vector3 vector4 = betaRayleighTheta;
		Vector3 vector5 = betaMie;
		Vector3 vector6 = betaMieTheta;
		Vector3 vector7 = betaMiePhase;
		Vector3 vector8 = betaNight;
		Color black = Color.black;
		float num = Mathf.Max(0f, Vector3.Dot(-direction, rhs));
		float num2 = Mathf.Pow(Mathf.Clamp(direction.y + horizonOffset, 0.001f, 1f), haziness);
		float num3 = (1f - num2) * 190000f;
		float num4 = num3 + num2 * (vector.x - num3);
		float num5 = num3 + num2 * (vector.y - num3);
		float num6 = 1f + num * num;
		Vector3 vector9 = vector3 * num4 + vector5 * num5;
		Vector3 vector10 = vector4 + vector6 / Mathf.Pow(vector7.x - vector7.y * num, 1.5f);
		float r = sunColor.r;
		float g = sunColor.g;
		float b = sunColor.b;
		float r2 = moonColor.r;
		float g2 = moonColor.g;
		float b2 = moonColor.b;
		float num7 = Mathf.Exp(0f - vector9.x);
		float num8 = Mathf.Exp(0f - vector9.y);
		float num9 = Mathf.Exp(0f - vector9.z);
		float num10 = num6 * vector10.x * vector2.x;
		float num11 = num6 * vector10.y * vector2.y;
		float num12 = num6 * vector10.z * vector2.z;
		float x = vector8.x;
		float y = vector8.y;
		float z = vector8.z;
		black.r = (1f - num7) * (r * num10 + r2 * x);
		black.g = (1f - num8) * (g * num11 + g2 * y);
		black.b = (1f - num9) * (b * num12 + b2 * z);
		black.a = 10f * Max3(black.r, black.g, black.b);
		black += additiveColor;
		black.r = Mathf.Lerp(black.r, cloudColor.r, fogginess);
		black.g = Mathf.Lerp(black.g, cloudColor.g, fogginess);
		black.b = Mathf.Lerp(black.b, cloudColor.b, fogginess);
		black.a += fogginess;
		black.a = Mathf.Clamp01(black.a);
		return PowRGB(black, p);
	}

	private void SetupScattering()
	{
		float num = 0.001f + Atmosphere.RayleighMultiplier * Atmosphere.ScatteringColor.r;
		float num2 = 0.001f + Atmosphere.RayleighMultiplier * Atmosphere.ScatteringColor.g;
		float num3 = 0.001f + Atmosphere.RayleighMultiplier * Atmosphere.ScatteringColor.b;
		betaRayleigh.x = 5.8E-06f * num;
		betaRayleigh.y = 1.35E-05f * num2;
		betaRayleigh.z = 3.31E-05f * num3;
		betaRayleighTheta.x = 0.000116f * num * (3f / (16f * (float)Math.PI));
		betaRayleighTheta.y = 0.00027f * num2 * (3f / (16f * (float)Math.PI));
		betaRayleighTheta.z = 0.00066200003f * num3 * (3f / (16f * (float)Math.PI));
		opticalDepth.x = 8000f * Mathf.Exp((0f - World.ViewerHeight) * 50000f / 8000f);
		float num4 = 0.001f + Atmosphere.MieMultiplier * Atmosphere.ScatteringColor.r;
		float num5 = 0.001f + Atmosphere.MieMultiplier * Atmosphere.ScatteringColor.g;
		float num6 = 0.001f + Atmosphere.MieMultiplier * Atmosphere.ScatteringColor.b;
		float directionality = Atmosphere.Directionality;
		float num7 = 3f / (4f * (float)Math.PI) * (1f - directionality * directionality) / (2f + directionality * directionality);
		betaMie.x = 2E-06f * num4;
		betaMie.y = 2E-06f * num5;
		betaMie.z = 2E-06f * num6;
		betaMieTheta.x = 4E-05f * num4 * num7;
		betaMieTheta.y = 4E-05f * num5 * num7;
		betaMieTheta.z = 4E-05f * num6 * num7;
		betaMiePhase.x = 1f + directionality * directionality;
		betaMiePhase.y = 2f * directionality;
		opticalDepth.y = 1200f * Mathf.Exp((0f - World.ViewerHeight) * 50000f / 1200f);
		oneOverBeta = Inverse(betaMie + betaRayleigh);
		betaNight = Vector3.Scale(betaRayleighTheta + betaMieTheta / Mathf.Pow(betaMiePhase.x, 1.5f), oneOverBeta);
	}

	private void SetupSunAndMoon()
	{
		float f = (float)Math.PI / 180f * Cycle.Latitude;
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float longitude = Cycle.Longitude;
		float num3 = 367 * Cycle.Year - 7 * (Cycle.Year + (Cycle.Month + 9) / 12) / 4 + 275 * Cycle.Month / 9 + Cycle.Day - 730530;
		float num4 = Cycle.Hour - Cycle.UTC;
		float num5 = 23.4393f - 3.563E-07f * num3;
		float f2 = (float)Math.PI / 180f * num5;
		float num6 = Mathf.Sin(f2);
		float num7 = Mathf.Cos(f2);
		float num8 = 282.9404f + 4.70935E-05f * num3;
		float num9 = 0.016709f - 1.151E-09f * num3;
		float num10 = 356.047f + 0.98560023f * num3;
		float f3 = (float)Math.PI / 180f * num10;
		float num11 = Mathf.Sin(f3);
		float num12 = Mathf.Cos(f3);
		float num13 = num10 + num9 * 57.29578f * num11 * (1f + num9 * num12);
		float f4 = (float)Math.PI / 180f * num13;
		float num14 = Mathf.Sin(f4);
		float num15 = Mathf.Cos(f4) - num9;
		float num16 = num14 * Mathf.Sqrt(1f - num9 * num9);
		float num17 = 57.29578f * Mathf.Atan2(num16, num15);
		float num18 = Mathf.Sqrt(num15 * num15 + num16 * num16);
		float num19 = num17 + num8;
		float f5 = (float)Math.PI / 180f * num19;
		float num20 = Mathf.Sin(f5);
		float num21 = Mathf.Cos(f5);
		float num22 = num18 * num21;
		float num23 = num18 * num20;
		float num24 = num22;
		float num25 = num23 * num7;
		float y = num23 * num6;
		float num26 = Mathf.Atan2(num25, num24);
		float num27 = 57.29578f * num26;
		float f6 = Mathf.Atan2(y, Mathf.Sqrt(num24 * num24 + num25 * num25));
		float num28 = Mathf.Sin(f6);
		float num29 = Mathf.Cos(f6);
		float num30 = num17 + num8 + 180f + num4 * 15f + longitude - num27;
		float f7 = (float)Math.PI / 180f * num30;
		float num31 = Mathf.Sin(f7);
		float num32 = Mathf.Cos(f7) * num29;
		float num33 = num31 * num29;
		float num34 = num28;
		float num35 = num32 * num - num34 * num2;
		float num36 = num33;
		float y2 = num32 * num2 + num34 * num;
		float num37 = Mathf.Atan2(num36, num35) + (float)Math.PI;
		float num38 = Mathf.Atan2(y2, Mathf.Sqrt(num35 * num35 + num36 * num36));
		float num39 = (float)Math.PI / 2f - num38;
		float phi = num37;
		Vector3 eulerAngles = Components.CameraTransform.rotation.eulerAngles;
		float z = 2f * Time.time + Mathf.Abs(eulerAngles.x) + Mathf.Abs(eulerAngles.y) + Mathf.Abs(eulerAngles.z);
		Vector3 position = Components.DomeTransform.position + Components.DomeTransform.rotation * OrbitalToUnity(Radius, num39, phi);
		Components.SunTransform.position = position;
		Components.SunTransform.LookAt(Components.DomeTransform.position);
		Components.SunTransform.Rotate(new Vector3(0f, 0f, z), Space.Self);
		Vector3 position2 = Components.DomeTransform.position + Components.DomeTransform.rotation * OrbitalToUnity(Radius, num39 + (float)Math.PI, phi);
		Components.MoonTransform.position = position2;
		Components.MoonTransform.LookAt(Components.DomeTransform.position);
		float num40 = 4f * Mathf.Tan((float)Math.PI / 360f * Day.SunMeshSize);
		float num41 = 2f * num40;
		Vector3 localScale = new Vector3(num41, num41, num41);
		Components.SunTransform.localScale = localScale;
		float num42 = 2f * Mathf.Tan((float)Math.PI / 360f * Night.MoonMeshSize);
		float num43 = 2f * num42;
		Vector3 localScale2 = new Vector3(num43, num43, num43);
		Components.MoonTransform.localScale = localScale2;
		SunZenith = 57.29578f * num39;
		MoonZenith = Mathf.PingPong(SunZenith + 180f, 180f);
		bool flag = Components.SunTransform.localPosition.y > -0.5f;
		bool flag2 = Components.MoonTransform.localPosition.y > -0.1f;
		bool flag3 = SampleAtmosphere(Vector3.up).a < 0.99f;
		bool flag4 = Clouds.Density > 0f;
		Components.SunRenderer.enabled = flag;
		Components.MoonRenderer.enabled = flag2;
		Components.SpaceRenderer.enabled = flag3;
		Components.CloudRenderer.enabled = flag4;
		SetupLightColor(num39);
		SetupLightIntensity(num38);
	}

	private void SetupLightColor(float theta)
	{
		float num = Mathf.Cos(Mathf.Pow(theta / ((float)Math.PI * 2f), 2f - Light.Falloff) * 2f * (float)Math.PI);
		float num2 = Mathf.Sqrt(501264f * num * num + 1416f + 1f) - 708f * num;
		float r = Day.SunLightColor.r;
		float g = Day.SunLightColor.g;
		float b = Day.SunLightColor.b;
		float a = Components.LightSource.intensity / Mathf.Max(Day.SunLightIntensity, Night.MoonLightIntensity);
		r *= Mathf.Exp(-0.008735f * Mathf.Pow(0.68f, -4.08f * num2));
		g *= Mathf.Exp(-0.008735f * Mathf.Pow(0.55f, -4.08f * num2));
		b *= Mathf.Exp(-0.008735f * Mathf.Pow(0.44f, -4.08f * num2));
		LerpValue = Max3(r, g, b);
		Color moonLightColor = Night.MoonLightColor;
		Color b2 = Color.Lerp(Day.SunLightColor, new Color(r, g, b, a), Light.Coloring);
		Color color = Color.Lerp(moonLightColor, b2, Max3(b2.r, b2.g, b2.b));
		Color color2 = new Color(color.r, color.g, color.b, a);
		Components.LightSource.color = color2;
		SunColor = Atmosphere.Brightness * Day.SkyMultiplier * Mathf.Lerp(1f, 0.1f, Mathf.Sqrt(SunZenith / 90f) - 0.25f) * Color.Lerp(Day.SunLightColor * LerpValue, new Color(r, g, b, a), Light.SkyColoring);
		SunColor = new Color(SunColor.r, SunColor.g, SunColor.b, LerpValue);
		MoonColor = (1f - LerpValue) * 0.5f * Atmosphere.Brightness * Night.SkyMultiplier * Night.MoonLightColor;
		MoonColor = new Color(MoonColor.r, MoonColor.g, MoonColor.b, 1f - LerpValue);
		MoonHaloColor = (1f - LerpValue) * (1f - Mathf.Abs(Cycle.MoonPhase)) * Atmosphere.Brightness * Night.SkyMultiplier * Night.MoonHaloColor;
		float num3 = Mathf.Lerp(Night.CloudMultiplier, Day.CloudMultiplier, LerpValue);
		CloudColor = num3 * 1.25f * Clouds.Brightness * Color.Lerp(Color.white * num3, Color.Lerp(MoonColor, SunColor, LerpValue), Light.CloudColoring);
		CloudColor = new Color(CloudColor.r, CloudColor.g, CloudColor.b, num3);
		Color additiveColor = Color.Lerp(Night.AdditiveColor, Day.AdditiveColor, LerpValue);
		additiveColor.a = Max3(additiveColor.r, additiveColor.g, additiveColor.b);
		AdditiveColor = additiveColor;
	}

	private void SetupLightIntensity(float altitude)
	{
		float intensity;
		float shadowStrength;
		Vector3 position;
		Quaternion rotation;
		if (LerpValue > 0.2f)
		{
			float t = LerpValue / 0.2f - 1f;
			intensity = Mathf.Lerp(0f, Day.SunLightIntensity, t);
			shadowStrength = Day.ShadowStrength;
			position = Components.SunTransform.position;
			rotation = Components.SunTransform.rotation;
		}
		else
		{
			float t2 = 1f - LerpValue / 0.2f;
			float num = 1f - Mathf.Abs(Cycle.MoonPhase);
			intensity = Mathf.Lerp(0f, Night.MoonLightIntensity * num, t2);
			shadowStrength = Night.ShadowStrength;
			position = Components.MoonTransform.position;
			rotation = Components.MoonTransform.rotation;
		}
		LightShadows shadows = ((Components.LightSource.shadowStrength != 0f) ? LightShadows.Soft : LightShadows.None);
		Components.LightSource.intensity = intensity;
		Components.LightSource.shadowStrength = shadowStrength;
		Components.LightTransform.position = position;
		Components.LightTransform.rotation = rotation;
		Components.LightSource.shadows = shadows;
	}

	private Color SampleFogColor()
	{
		Vector3 forward = Components.CameraTransform.forward;
		Vector3 direction = Vector3.Lerp(new Vector3(forward.x, 0f, forward.z), Vector3.up, World.FogColorBias);
		Color color = SampleAtmosphere(direction);
		return new Color(color.a * color.r, color.a * color.g, color.a * color.b, 1f);
	}

	private Color SampleAmbientColor()
	{
		float num = Mathf.Lerp(Night.AmbientIntensity, Day.AmbientIntensity, LerpValue);
		Color lightColor = LightColor;
		return new Color(lightColor.r * num, lightColor.g * num, lightColor.b * num, 1f);
	}

	private Color PowRGB(Color c, float p)
	{
		return new Color(Mathf.Pow(c.r, p), Mathf.Pow(c.g, p), Mathf.Pow(c.b, p), c.a);
	}

	private Color PowRGBA(Color c, float p)
	{
		return new Color(Mathf.Pow(c.r, p), Mathf.Pow(c.g, p), Mathf.Pow(c.b, p), Mathf.Pow(c.a, p));
	}

	private float Max3(float a, float b, float c)
	{
		if (!(a >= b) || !(a >= c))
		{
			if (b >= c)
			{
				return b;
			}
			return c;
		}
		return a;
	}

	private Vector3 Inverse(Vector3 v)
	{
		return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
	}

	private void SetupQualitySettings()
	{
		TOD_Resources resources = Components.Resources;
		Material material = null;
		Material material2 = null;
		switch (CloudQuality)
		{
		case CloudQualityType.Fastest:
			material = resources.CloudMaterialFastest;
			material2 = resources.ShadowMaterialFastest;
			break;
		case CloudQualityType.Density:
			material = resources.CloudMaterialDensity;
			material2 = resources.ShadowMaterialDensity;
			break;
		case CloudQualityType.Bumped:
			material = resources.CloudMaterialBumped;
			material2 = resources.ShadowMaterialBumped;
			break;
		default:
			Debug.LogError("Unknown cloud quality.");
			break;
		}
		Mesh mesh = null;
		Mesh mesh2 = null;
		Mesh mesh3 = null;
		Mesh mesh4 = null;
		Mesh mesh5 = null;
		switch (MeshQuality)
		{
		case MeshQualityType.Low:
			mesh = resources.IcosphereLow;
			mesh2 = resources.IcosphereLow;
			mesh3 = resources.HalfIcosphereLow;
			mesh4 = resources.Quad;
			mesh5 = resources.SphereLow;
			break;
		case MeshQualityType.Medium:
			mesh = resources.IcosphereMedium;
			mesh2 = resources.IcosphereMedium;
			mesh3 = resources.HalfIcosphereMedium;
			mesh4 = resources.Quad;
			mesh5 = resources.SphereMedium;
			break;
		case MeshQualityType.High:
			mesh = resources.IcosphereHigh;
			mesh2 = resources.IcosphereHigh;
			mesh3 = resources.HalfIcosphereHigh;
			mesh4 = resources.Quad;
			mesh5 = resources.SphereHigh;
			break;
		default:
			Debug.LogError("Unknown mesh quality.");
			break;
		}
		if (!Components.SpaceShader || Components.SpaceShader.name != resources.SpaceMaterial.name)
		{
			TOD_Components components = Components;
			Material spaceMaterial = resources.SpaceMaterial;
			Components.SpaceRenderer.sharedMaterial = spaceMaterial;
			components.SpaceShader = spaceMaterial;
		}
		if (!Components.AtmosphereShader || Components.AtmosphereShader.name != resources.AtmosphereMaterial.name)
		{
			TOD_Components components2 = Components;
			Material atmosphereMaterial = resources.AtmosphereMaterial;
			Components.AtmosphereRenderer.sharedMaterial = atmosphereMaterial;
			components2.AtmosphereShader = atmosphereMaterial;
		}
		if (!Components.CloudShader || Components.CloudShader.name != material.name)
		{
			TOD_Components components3 = Components;
			Material material3 = material;
			Components.CloudRenderer.sharedMaterial = material3;
			components3.CloudShader = material3;
		}
		if (!Components.ShadowShader || Components.ShadowShader.name != material2.name)
		{
			TOD_Components components4 = Components;
			Material material4 = material2;
			Components.ShadowProjector.material = material4;
			components4.ShadowShader = material4;
		}
		if (!Components.SunShader || Components.SunShader.name != resources.SunMaterial.name)
		{
			TOD_Components components5 = Components;
			Material sunMaterial = resources.SunMaterial;
			Components.SunRenderer.sharedMaterial = sunMaterial;
			components5.SunShader = sunMaterial;
		}
		if (!Components.MoonShader || Components.MoonShader.name != resources.MoonMaterial.name)
		{
			TOD_Components components6 = Components;
			Material moonMaterial = resources.MoonMaterial;
			Components.MoonRenderer.sharedMaterial = moonMaterial;
			components6.MoonShader = moonMaterial;
		}
		if (Components.SpaceMeshFilter.sharedMesh != mesh)
		{
			Components.SpaceMeshFilter.mesh = mesh;
		}
		if (Components.AtmosphereMeshFilter.sharedMesh != mesh2)
		{
			Components.AtmosphereMeshFilter.mesh = mesh2;
		}
		if (Components.CloudMeshFilter.sharedMesh != mesh3)
		{
			Components.CloudMeshFilter.mesh = mesh3;
		}
		if (Components.SunMeshFilter.sharedMesh != mesh4)
		{
			Components.SunMeshFilter.mesh = mesh4;
		}
		if (Components.MoonMeshFilter.sharedMesh != mesh5)
		{
			Components.MoonMeshFilter.mesh = mesh5;
		}
	}

	protected void OnEnable()
	{
		Components = GetComponent<TOD_Components>();
		if (!Components)
		{
			Debug.LogError("TOD_Components not found. Disabling script.");
			base.enabled = false;
		}
	}

	protected void Update()
	{
		Cycle.CheckRange();
		SetupQualitySettings();
		SetupSunAndMoon();
		SetupScattering();
		if (World.SetFogColor)
		{
			RenderSettings.fogColor = SampleFogColor();
		}
		if (World.SetAmbientLight)
		{
			RenderSettings.ambientLight = SampleAmbientColor();
		}
		Vector4 vector = Components.Animation.CloudUV + Components.Animation.OffsetUV;
		Shader.SetGlobalFloat("TOD_Gamma", Gamma);
		Shader.SetGlobalFloat("TOD_OneOverGamma", OneOverGamma);
		Shader.SetGlobalColor("TOD_LightColor", LightColor);
		Shader.SetGlobalColor("TOD_CloudColor", CloudColor);
		Shader.SetGlobalColor("TOD_SunColor", SunColor);
		Shader.SetGlobalColor("TOD_MoonColor", MoonColor);
		Shader.SetGlobalColor("TOD_AdditiveColor", AdditiveColor);
		Shader.SetGlobalColor("TOD_MoonHaloColor", MoonHaloColor);
		Shader.SetGlobalVector("TOD_SunDirection", SunDirection);
		Shader.SetGlobalVector("TOD_MoonDirection", MoonDirection);
		Shader.SetGlobalVector("TOD_LightDirection", LightDirection);
		Shader.SetGlobalVector("TOD_LocalSunDirection", Components.DomeTransform.InverseTransformDirection(SunDirection));
		Shader.SetGlobalVector("TOD_LocalMoonDirection", Components.DomeTransform.InverseTransformDirection(MoonDirection));
		Shader.SetGlobalVector("TOD_LocalLightDirection", Components.DomeTransform.InverseTransformDirection(LightDirection));
		if (Components.AtmosphereShader != null)
		{
			Components.AtmosphereShader.SetFloat("_Contrast", Atmosphere.Contrast * OneOverGamma);
			Components.AtmosphereShader.SetFloat("_Haziness", Atmosphere.Haziness);
			Components.AtmosphereShader.SetFloat("_Fogginess", Atmosphere.Fogginess);
			Components.AtmosphereShader.SetFloat("_Horizon", World.HorizonOffset);
			Components.AtmosphereShader.SetVector("_OpticalDepth", opticalDepth);
			Components.AtmosphereShader.SetVector("_OneOverBeta", oneOverBeta);
			Components.AtmosphereShader.SetVector("_BetaRayleigh", betaRayleigh);
			Components.AtmosphereShader.SetVector("_BetaRayleighTheta", betaRayleighTheta);
			Components.AtmosphereShader.SetVector("_BetaMie", betaMie);
			Components.AtmosphereShader.SetVector("_BetaMieTheta", betaMieTheta);
			Components.AtmosphereShader.SetVector("_BetaMiePhase", betaMiePhase);
			Components.AtmosphereShader.SetVector("_BetaNight", betaNight);
		}
		if (Components.CloudShader != null)
		{
			float value = (1f - Atmosphere.Fogginess) * LerpValue;
			float value2 = (1f - Atmosphere.Fogginess) * 0.6f * (1f - Mathf.Abs(Cycle.MoonPhase));
			Components.CloudShader.SetFloat("_SunGlow", value);
			Components.CloudShader.SetFloat("_MoonGlow", value2);
			Components.CloudShader.SetFloat("_CloudDensity", Clouds.Density);
			Components.CloudShader.SetFloat("_CloudSharpness", Clouds.Sharpness);
			Components.CloudShader.SetFloat("_CloudScale1", Clouds.Scale1);
			Components.CloudShader.SetFloat("_CloudScale2", Clouds.Scale2);
			Components.CloudShader.SetVector("_CloudUV", vector);
		}
		if (Components.SpaceShader != null)
		{
			Components.SpaceShader.mainTextureScale = new Vector2(Stars.Tiling, Stars.Tiling);
			Components.SpaceShader.SetFloat("_Subtract", 1f - Mathf.Pow(Stars.Density, 0.1f));
		}
		if (Components.SunShader != null)
		{
			Components.SunShader.SetColor("_Color", Day.SunMeshColor * LerpValue * (1f - Atmosphere.Fogginess));
		}
		if (Components.MoonShader != null)
		{
			Components.MoonShader.SetColor("_Color", Night.MoonMeshColor);
			Components.MoonShader.SetFloat("_Phase", Cycle.MoonPhase);
		}
		if (Components.ShadowShader != null)
		{
			float value3 = Clouds.ShadowStrength * Mathf.Clamp01(1f - LightZenith / 90f);
			Components.ShadowShader.SetFloat("_Alpha", value3);
			Components.ShadowShader.SetFloat("_CloudDensity", Clouds.Density);
			Components.ShadowShader.SetFloat("_CloudSharpness", Clouds.Sharpness);
			Components.ShadowShader.SetFloat("_CloudScale1", Clouds.Scale1);
			Components.ShadowShader.SetFloat("_CloudScale2", Clouds.Scale2);
			Components.ShadowShader.SetVector("_CloudUV", vector);
		}
		if (Components.ShadowProjector != null)
		{
			bool flag = Clouds.ShadowStrength != 0f && Components.ShadowShader != null;
			float farClipPlane = Radius * 2f;
			float radius = Radius;
			Components.ShadowProjector.enabled = flag;
			Components.ShadowProjector.farClipPlane = farClipPlane;
			Components.ShadowProjector.orthographicSize = radius;
		}
	}
}
