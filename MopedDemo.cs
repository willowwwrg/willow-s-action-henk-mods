using UnityEngine;

public class MopedDemo : MonoBehaviour
{
	public Sky[] skies;

	private bool spinning = true;

	private bool background = true;

	private int currentSky;

	private bool showDiffuse = true;

	private bool showSpecular = true;

	private bool wireframe;

	private float exposure = 1f;

	private float glow = 0.5f;

	public Renderer[] meshes;

	public Renderer[] glowMeshes;

	public Renderer[] specGlowMeshes;

	private Texture[] diffTextures;

	private Texture[] specTextures;

	private Texture[] glowTextures;

	private Texture2D greyTex;

	private Texture2D blackTex;

	public Texture2D helpTex;

	private Color helpColor = new Color(1f, 1f, 1f, 0f);

	public bool showGUI = true;

	private bool firstFrame = true;

	private Vector3 angularVel = new Vector3(0f, 6f, 0f);

	private void Start()
	{
		firstFrame = true;
		setDiffuse(yes: true);
		setSpecular(yes: true);
		currentSky = 0;
		for (int num = skies.Length - 1; num >= 0; num--)
		{
			setSky(num);
		}
		setBackground(background);
		greyTex = new Texture2D(16, 16);
		Color color = new Color(0.85f, 0.85f, 0.85f, 1f);
		Color[] pixels = greyTex.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = color;
		}
		greyTex.SetPixels(pixels);
		greyTex.Apply(updateMipmaps: true);
		blackTex = new Texture2D(16, 16);
		pixels = blackTex.GetPixels();
		Color color2 = new Color(0f, 0f, 0f, 0f);
		for (int j = 0; j < pixels.Length; j++)
		{
			pixels[j] = color2;
		}
		blackTex.SetPixels(pixels);
		blackTex.Apply(updateMipmaps: true);
		if (meshes != null)
		{
			diffTextures = new Texture[meshes.Length];
			specTextures = new Texture[meshes.Length];
			glowTextures = new Texture[meshes.Length];
			for (int k = 0; k < meshes.Length; k++)
			{
				if (meshes[k].material.HasProperty("_MainTex"))
				{
					diffTextures[k] = meshes[k].material.GetTexture("_MainTex");
				}
				if (meshes[k].material.HasProperty("_SpecTex"))
				{
					specTextures[k] = meshes[k].material.GetTexture("_SpecTex");
				}
				if (meshes[k].material.HasProperty("_Illum"))
				{
					glowTextures[k] = meshes[k].material.GetTexture("_Illum");
				}
			}
		}
		setWireframe(yes: false);
		setGlow(glow);
		setExposures(1f);
	}

	private void setDiffuse(bool yes)
	{
		showDiffuse = yes;
		for (int i = 0; i < skies.Length; i++)
		{
			if ((bool)skies[i])
			{
				skies[i].diffIntensity = ((!yes) ? 0f : 1f);
			}
		}
		if ((bool)Sky.activeSky)
		{
			Sky.activeSky.Apply();
		}
	}

	private void setSpecular(bool yes)
	{
		showSpecular = yes;
		for (int i = 0; i < skies.Length; i++)
		{
			if ((bool)skies[i])
			{
				skies[i].specIntensity = ((!yes) ? 0f : 1f);
			}
		}
		if ((bool)Sky.activeSky)
		{
			Sky.activeSky.Apply();
		}
	}

	private void setExposures(float val)
	{
		exposure = val;
		for (int i = 0; i < skies.Length; i++)
		{
			if ((bool)skies[i])
			{
				skies[i].camExposure = val;
			}
		}
		Sky.activeSky.Apply();
	}

	private void setSky(int index)
	{
		currentSky = index;
		skies[currentSky].Apply();
	}

	private void setBackground(bool yes)
	{
		background = yes;
		for (int i = 0; i < skies.Length; i++)
		{
			if ((bool)skies[i])
			{
				skies[i].showSkybox = yes;
			}
		}
		Sky.activeSky.Apply();
	}

	private void setWireframe(bool yes)
	{
	}

	private void setGlow(float val)
	{
		glow = val;
		if (glowMeshes != null)
		{
			for (int i = 0; i < glowMeshes.Length; i++)
			{
				Material material = glowMeshes[i].material;
				if (material.HasProperty("_GlowStrength"))
				{
					material.SetFloat("_GlowStrength", 12f * glow);
				}
			}
		}
		if (specGlowMeshes == null)
		{
			return;
		}
		for (int j = 0; j < specGlowMeshes.Length; j++)
		{
			Material material2 = specGlowMeshes[j].material;
			if (material2.HasProperty("_SpecInt"))
			{
				material2.SetFloat("_SpecInt", glow);
			}
		}
	}

	private void Update()
	{
		if (firstFrame)
		{
			firstFrame = false;
			setSky(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			setExposures(0.25f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			setExposures(0.5f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			setExposures(0.75f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			setExposures(1f);
		}
		if (skies.Length != 0)
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				setSky((currentSky + 1) % skies.Length);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				setSky((currentSky + skies.Length - 1) % skies.Length);
			}
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			setBackground(!background);
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			spinning = !spinning;
		}
		if (!Input.multiTouchEnabled || Input.touchCount != 2)
		{
			return;
		}
		float x = Input.GetTouch(0).position.x;
		float x2 = Input.GetTouch(1).position.x;
		float num = Input.GetTouch(0).deltaPosition.x;
		float num2 = Input.GetTouch(1).deltaPosition.x;
		if (num != 0f && num2 != 0f)
		{
			if (x > x2)
			{
				float num3 = num;
				num = num2;
				num2 = num3;
			}
			float num4 = num2 - num;
			setExposures(Mathf.Clamp(exposure + num4 * 0.0025f, 0.01f, 10f));
		}
	}

	private void FixedUpdate()
	{
		if (spinning)
		{
			Sky.activeSky.transform.Rotate(angularVel * Time.fixedDeltaTime);
		}
	}

	private void OnGUI()
	{
		Rect pixelRect = base.camera.pixelRect;
		pixelRect.y = base.camera.pixelRect.height * 0.87f;
		pixelRect.height = base.camera.pixelRect.height * 0.06f;
		if (Input.mousePosition.y < base.camera.pixelHeight * 0.13f)
		{
			helpColor.a = Mathf.Min(1f, helpColor.a + 0.1f);
		}
		else
		{
			helpColor.a = Mathf.Min(1f, 0.9f * helpColor.a);
		}
		drawHelp(pixelRect.width - (float)helpTex.width, pixelRect.y - (float)helpTex.height - 10f);
		GUI.color = Color.white;
		if (showGUI)
		{
			Rect rect = pixelRect;
			rect.x = 10f;
			rect.y += 3f;
			rect.height = 20f;
			rect.width = 100f;
			bool num = showDiffuse;
			showDiffuse = GUI.Toggle(rect, showDiffuse, "Diffuse IBL");
			if (num != showDiffuse)
			{
				setDiffuse(showDiffuse);
			}
			rect.y += 15f;
			bool num2 = showSpecular;
			showSpecular = GUI.Toggle(rect, showSpecular, "Specular IBL");
			if (num2 != showSpecular)
			{
				setSpecular(showSpecular);
			}
			rect.x += rect.width;
			rect.y -= 15f;
			bool num3 = background;
			rect.x += rect.width;
			background = GUI.Toggle(rect, background, "Skybox");
			if (num3 != background)
			{
				setBackground(background);
			}
			rect.y += 15f;
			spinning = GUI.Toggle(rect, spinning, "Spinning");
			rect.x += rect.width;
			rect.y -= 15f;
			bool num4 = wireframe;
			wireframe = GUI.Toggle(rect, wireframe, "Wireframe");
			if (num4 != wireframe)
			{
				setWireframe(wireframe);
			}
			Rect position = rect;
			position.x = 15f;
			position.y = pixelRect.yMax - 10f;
			position.height = 20f;
			position.width = pixelRect.width * 0.28f;
			GUI.Label(position, "Exposure: " + Mathf.CeilToInt(exposure * 100f) + "%");
			position.y += 18f;
			float value = Mathf.Sqrt(exposure);
			value = GUI.HorizontalSlider(position, value, 0f, 2f);
			exposure = value * value;
			setExposures(exposure);
			position.x = base.camera.pixelRect.width - position.width - 15f;
			position.y -= 18f;
			GUI.Label(position, "Moped Lights");
			position.y += 18f;
			float num5 = glow * glow;
			float num6 = num5;
			num5 = GUI.HorizontalSlider(position, num5, 0f, 1f);
			if (num5 != num6)
			{
				setGlow(Mathf.Sqrt(num5));
			}
		}
	}

	private void drawHelp(float x, float y)
	{
		if ((bool)helpTex)
		{
			Rect position = new Rect(x, y, helpTex.width, helpTex.height);
			GUI.color = helpColor;
			GUI.DrawTexture(position, helpTex);
		}
	}
}
