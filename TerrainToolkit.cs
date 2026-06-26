using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Terrain/Terrain Toolkit")]
public class TerrainToolkit : MonoBehaviour
{
	public class PeakDistance : IComparable
	{
		public int id;

		public float dist;

		public int CompareTo(object obj)
		{
			PeakDistance peakDistance = (PeakDistance)obj;
			int num = dist.CompareTo(peakDistance.dist);
			if (num == 0)
			{
				num = dist.CompareTo(peakDistance.dist);
			}
			return num;
		}
	}

	public struct Peak
	{
		public Vector2 peakPoint;

		public float peakHeight;
	}

	public class voronoiPresetData
	{
		public string presetName;

		public VoronoiType voronoiType;

		public int voronoiCells;

		public float voronoiFeatures;

		public float voronoiScale;

		public float voronoiBlend;

		public voronoiPresetData(string pn, VoronoiType vt, int c, float vf, float vs, float vb)
		{
			presetName = pn;
			voronoiType = vt;
			voronoiCells = c;
			voronoiFeatures = vf;
			voronoiScale = vs;
			voronoiBlend = vb;
		}
	}

	public class fractalPresetData
	{
		public string presetName;

		public float diamondSquareDelta;

		public float diamondSquareBlend;

		public fractalPresetData(string pn, float dsd, float dsb)
		{
			presetName = pn;
			diamondSquareDelta = dsd;
			diamondSquareBlend = dsb;
		}
	}

	public class perlinPresetData
	{
		public string presetName;

		public int perlinFrequency;

		public float perlinAmplitude;

		public int perlinOctaves;

		public float perlinBlend;

		public perlinPresetData(string pn, int pf, float pa, int po, float pb)
		{
			presetName = pn;
			perlinFrequency = pf;
			perlinAmplitude = pa;
			perlinOctaves = po;
			perlinBlend = pb;
		}
	}

	public class thermalErosionPresetData
	{
		public string presetName;

		public int thermalIterations;

		public float thermalMinSlope;

		public float thermalFalloff;

		public thermalErosionPresetData(string pn, int ti, float tms, float tba)
		{
			presetName = pn;
			thermalIterations = ti;
			thermalMinSlope = tms;
			thermalFalloff = tba;
		}
	}

	public class fastHydraulicErosionPresetData
	{
		public string presetName;

		public int hydraulicIterations;

		public float hydraulicMaxSlope;

		public float hydraulicFalloff;

		public fastHydraulicErosionPresetData(string pn, int hi, float hms, float hba)
		{
			presetName = pn;
			hydraulicIterations = hi;
			hydraulicMaxSlope = hms;
			hydraulicFalloff = hba;
		}
	}

	public class fullHydraulicErosionPresetData
	{
		public string presetName;

		public int hydraulicIterations;

		public float hydraulicRainfall;

		public float hydraulicEvaporation;

		public float hydraulicSedimentSolubility;

		public float hydraulicSedimentSaturation;

		public fullHydraulicErosionPresetData(string pn, int hi, float hr, float he, float hso, float hsa)
		{
			presetName = pn;
			hydraulicIterations = hi;
			hydraulicRainfall = hr;
			hydraulicEvaporation = he;
			hydraulicSedimentSolubility = hso;
			hydraulicSedimentSaturation = hsa;
		}
	}

	public class velocityHydraulicErosionPresetData
	{
		public string presetName;

		public int hydraulicIterations;

		public float hydraulicVelocityRainfall;

		public float hydraulicVelocityEvaporation;

		public float hydraulicVelocitySedimentSolubility;

		public float hydraulicVelocitySedimentSaturation;

		public float hydraulicVelocity;

		public float hydraulicMomentum;

		public float hydraulicEntropy;

		public float hydraulicDowncutting;

		public velocityHydraulicErosionPresetData(string pn, int hi, float hvr, float hve, float hso, float hsa, float hv, float hm, float he, float hd)
		{
			presetName = pn;
			hydraulicIterations = hi;
			hydraulicVelocityRainfall = hvr;
			hydraulicVelocityEvaporation = hve;
			hydraulicVelocitySedimentSolubility = hso;
			hydraulicVelocitySedimentSaturation = hsa;
			hydraulicVelocity = hv;
			hydraulicMomentum = hm;
			hydraulicEntropy = he;
			hydraulicDowncutting = hd;
		}
	}

	public class tidalErosionPresetData
	{
		public string presetName;

		public int tidalIterations;

		public float tidalRangeAmount;

		public float tidalCliffLimit;

		public tidalErosionPresetData(string pn, int ti, float tra, float tcl)
		{
			presetName = pn;
			tidalIterations = ti;
			tidalRangeAmount = tra;
			tidalCliffLimit = tcl;
		}
	}

	public class windErosionPresetData
	{
		public string presetName;

		public int windIterations;

		public float windDirection;

		public float windForce;

		public float windLift;

		public float windGravity;

		public float windCapacity;

		public float windEntropy;

		public float windSmoothing;

		public windErosionPresetData(string pn, int wi, float wd, float wf, float wl, float wg, float wc, float we, float ws)
		{
			presetName = pn;
			windIterations = wi;
			windDirection = wd;
			windForce = wf;
			windLift = wl;
			windGravity = wg;
			windCapacity = wc;
			windEntropy = we;
			windSmoothing = ws;
		}
	}

	public enum ToolMode
	{
		Create,
		Erode,
		Texture
	}

	public enum ErosionMode
	{
		Filter,
		Brush
	}

	public enum ErosionType
	{
		Thermal,
		Hydraulic,
		Tidal,
		Wind,
		Glacial
	}

	public enum HydraulicType
	{
		Fast,
		Full,
		Velocity
	}

	public enum Neighbourhood
	{
		Moore,
		VonNeumann
	}

	public enum GeneratorType
	{
		Voronoi,
		DiamondSquare,
		Perlin,
		Smooth,
		Normalise
	}

	public enum VoronoiType
	{
		Linear,
		Sine,
		Tangent
	}

	public enum FeatureType
	{
		Mountains,
		Hills,
		Plateaus
	}

	public class PerlinNoise2D
	{
		private double[,] noiseValues;

		private float amplitude = 1f;

		private int frequency = 1;

		public float Amplitude => amplitude;

		public int Frequency => frequency;

		public PerlinNoise2D(int freq, float _amp)
		{
			System.Random random = new System.Random(Environment.TickCount);
			noiseValues = new double[freq, freq];
			amplitude = _amp;
			frequency = freq;
			for (int i = 0; i < freq; i++)
			{
				for (int j = 0; j < freq; j++)
				{
					noiseValues[i, j] = random.NextDouble();
				}
			}
		}

		public double getInterpolatedPoint(int _xa, int _xb, int _ya, int _yb, double Px, double Py)
		{
			double pa = interpolate(noiseValues[_xa % Frequency, _ya % frequency], noiseValues[_xb % Frequency, _ya % frequency], Px);
			double pb = interpolate(noiseValues[_xa % Frequency, _yb % frequency], noiseValues[_xb % Frequency, _yb % frequency], Px);
			return interpolate(pa, pb, Py);
		}

		private double interpolate(double Pa, double Pb, double Px)
		{
			double num = Px * 3.1415927410125732;
			double num2 = (double)(1f - Mathf.Cos((float)num)) * 0.5;
			return Pa * (1.0 - num2) + Pb * num2;
		}
	}

	public delegate void ErosionProgressDelegate(string titleString, string displayString, int iteration, int nIterations, float percentComplete);

	public delegate void TextureProgressDelegate(string titleString, string displayString, float percentComplete);

	public delegate void GeneratorProgressDelegate(string titleString, string displayString, float percentComplete);

	public GUISkin guiSkin;

	public Texture2D createIcon;

	public Texture2D erodeIcon;

	public Texture2D textureIcon;

	public Texture2D mooreIcon;

	public Texture2D vonNeumannIcon;

	public Texture2D mountainsIcon;

	public Texture2D hillsIcon;

	public Texture2D plateausIcon;

	public Texture2D defaultTexture;

	public int toolModeInt;

	private ErosionMode erosionMode;

	private ErosionType erosionType;

	public int erosionTypeInt;

	private GeneratorType generatorType;

	public int generatorTypeInt;

	public bool isBrushOn;

	public bool isBrushHidden;

	public bool isBrushPainting;

	public Vector3 brushPosition;

	public float brushSize = 50f;

	public float brushOpacity = 1f;

	public float brushSoftness = 0.5f;

	public int neighbourhoodInt;

	private Neighbourhood neighbourhood;

	public bool useDifferenceMaps = true;

	public int thermalIterations = 25;

	public float thermalMinSlope = 1f;

	public float thermalFalloff = 0.5f;

	public int hydraulicTypeInt;

	public HydraulicType hydraulicType;

	public int hydraulicIterations = 25;

	public float hydraulicMaxSlope = 60f;

	public float hydraulicFalloff = 0.5f;

	public float hydraulicRainfall = 0.01f;

	public float hydraulicEvaporation = 0.5f;

	public float hydraulicSedimentSolubility = 0.01f;

	public float hydraulicSedimentSaturation = 0.1f;

	public float hydraulicVelocityRainfall = 0.01f;

	public float hydraulicVelocityEvaporation = 0.5f;

	public float hydraulicVelocitySedimentSolubility = 0.01f;

	public float hydraulicVelocitySedimentSaturation = 0.1f;

	public float hydraulicVelocity = 20f;

	public float hydraulicMomentum = 1f;

	public float hydraulicEntropy;

	public float hydraulicDowncutting = 0.1f;

	public int tidalIterations = 25;

	public float tidalSeaLevel = 50f;

	public float tidalRangeAmount = 5f;

	public float tidalCliffLimit = 60f;

	public int windIterations = 25;

	public float windDirection;

	public float windForce = 0.5f;

	public float windLift = 0.01f;

	public float windGravity = 0.5f;

	public float windCapacity = 0.01f;

	public float windEntropy = 0.1f;

	public float windSmoothing = 0.25f;

	public SplatPrototype[] splatPrototypes;

	public Texture2D tempTexture;

	public float slopeBlendMinAngle = 60f;

	public float slopeBlendMaxAngle = 75f;

	public List<float> heightBlendPoints;

	public string[] gradientStyles;

	public int voronoiTypeInt;

	public VoronoiType voronoiType;

	public int voronoiCells = 16;

	public float voronoiFeatures = 1f;

	public float voronoiScale = 1f;

	public float voronoiBlend = 1f;

	public float diamondSquareDelta = 0.5f;

	public float diamondSquareBlend = 1f;

	public int perlinFrequency = 4;

	public float perlinAmplitude = 1f;

	public int perlinOctaves = 8;

	public float perlinBlend = 1f;

	public float smoothBlend = 1f;

	public int smoothIterations;

	public float normaliseMin;

	public float normaliseMax = 1f;

	public float normaliseBlend = 1f;

	[NonSerialized]
	public bool presetsInitialised;

	[NonSerialized]
	public int voronoiPresetId;

	[NonSerialized]
	public int fractalPresetId;

	[NonSerialized]
	public int perlinPresetId;

	[NonSerialized]
	public int thermalErosionPresetId;

	[NonSerialized]
	public int fastHydraulicErosionPresetId;

	[NonSerialized]
	public int fullHydraulicErosionPresetId;

	[NonSerialized]
	public int velocityHydraulicErosionPresetId;

	[NonSerialized]
	public int tidalErosionPresetId;

	[NonSerialized]
	public int windErosionPresetId;

	public ArrayList voronoiPresets = new ArrayList();

	public ArrayList fractalPresets = new ArrayList();

	public ArrayList perlinPresets = new ArrayList();

	public ArrayList thermalErosionPresets = new ArrayList();

	public ArrayList fastHydraulicErosionPresets = new ArrayList();

	public ArrayList fullHydraulicErosionPresets = new ArrayList();

	public ArrayList velocityHydraulicErosionPresets = new ArrayList();

	public ArrayList tidalErosionPresets = new ArrayList();

	public ArrayList windErosionPresets = new ArrayList();

	public void addPresets()
	{
		presetsInitialised = true;
		voronoiPresets = new ArrayList();
		fractalPresets = new ArrayList();
		perlinPresets = new ArrayList();
		thermalErosionPresets = new ArrayList();
		fastHydraulicErosionPresets = new ArrayList();
		fullHydraulicErosionPresets = new ArrayList();
		velocityHydraulicErosionPresets = new ArrayList();
		tidalErosionPresets = new ArrayList();
		windErosionPresets = new ArrayList();
		voronoiPresets.Add(new voronoiPresetData("Scattered Peaks", VoronoiType.Linear, 16, 8f, 0.5f, 1f));
		voronoiPresets.Add(new voronoiPresetData("Rolling Hills", VoronoiType.Sine, 8, 8f, 0f, 1f));
		voronoiPresets.Add(new voronoiPresetData("Jagged Mountains", VoronoiType.Linear, 32, 32f, 0.5f, 1f));
		fractalPresets.Add(new fractalPresetData("Rolling Plains", 0.4f, 1f));
		fractalPresets.Add(new fractalPresetData("Rough Mountains", 0.5f, 1f));
		fractalPresets.Add(new fractalPresetData("Add Noise", 0.75f, 0.05f));
		perlinPresets.Add(new perlinPresetData("Rough Plains", 2, 0.5f, 9, 1f));
		perlinPresets.Add(new perlinPresetData("Rolling Hills", 5, 0.75f, 3, 1f));
		perlinPresets.Add(new perlinPresetData("Rocky Mountains", 4, 1f, 8, 1f));
		perlinPresets.Add(new perlinPresetData("Hellish Landscape", 11, 1f, 7, 1f));
		perlinPresets.Add(new perlinPresetData("Add Noise", 10, 1f, 8, 0.2f));
		thermalErosionPresets.Add(new thermalErosionPresetData("Gradual, Weak Erosion", 25, 7.5f, 0.5f));
		thermalErosionPresets.Add(new thermalErosionPresetData("Fast, Harsh Erosion", 25, 2.5f, 0.1f));
		thermalErosionPresets.Add(new thermalErosionPresetData("Thermal Erosion Brush", 25, 0.1f, 0f));
		fastHydraulicErosionPresets.Add(new fastHydraulicErosionPresetData("Rainswept Earth", 25, 70f, 1f));
		fastHydraulicErosionPresets.Add(new fastHydraulicErosionPresetData("Terraced Slopes", 25, 30f, 0.4f));
		fastHydraulicErosionPresets.Add(new fastHydraulicErosionPresetData("Hydraulic Erosion Brush", 25, 85f, 1f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Low Rainfall, Hard Rock", 25, 0.01f, 0.5f, 0.01f, 0.1f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Low Rainfall, Soft Earth", 25, 0.01f, 0.5f, 0.06f, 0.15f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Heavy Rainfall, Hard Rock", 25, 0.02f, 0.5f, 0.01f, 0.1f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Heavy Rainfall, Soft Earth", 25, 0.02f, 0.5f, 0.06f, 0.15f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Low Rainfall, Hard Rock", 25, 0.01f, 0.5f, 0.01f, 0.1f, 1f, 1f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Low Rainfall, Soft Earth", 25, 0.01f, 0.5f, 0.06f, 0.15f, 1.2f, 2.8f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Heavy Rainfall, Hard Rock", 25, 0.02f, 0.5f, 0.01f, 0.1f, 1.1f, 2.2f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Heavy Rainfall, Soft Earth", 25, 0.02f, 0.5f, 0.06f, 0.15f, 1.2f, 2.4f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Carved Stone", 25, 0.01f, 0.5f, 0.01f, 0.1f, 2f, 1.25f, 0.05f, 0.35f));
		tidalErosionPresets.Add(new tidalErosionPresetData("Low Tidal Range, Calm Waves", 25, 5f, 65f));
		tidalErosionPresets.Add(new tidalErosionPresetData("Low Tidal Range, Strong Waves", 25, 5f, 35f));
		tidalErosionPresets.Add(new tidalErosionPresetData("High Tidal Range, Calm Water", 25, 15f, 55f));
		tidalErosionPresets.Add(new tidalErosionPresetData("High Tidal Range, Strong Waves", 25, 15f, 25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Northerly)", 25, 180f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Southerly)", 25, 0f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Easterly)", 25, 270f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Westerly)", 25, 90f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
	}

	public void setVoronoiPreset(voronoiPresetData preset)
	{
		generatorTypeInt = 0;
		generatorType = GeneratorType.Voronoi;
		voronoiTypeInt = (int)preset.voronoiType;
		voronoiType = preset.voronoiType;
		voronoiCells = preset.voronoiCells;
		voronoiFeatures = preset.voronoiFeatures;
		voronoiScale = preset.voronoiScale;
		voronoiBlend = preset.voronoiBlend;
	}

	public void setFractalPreset(fractalPresetData preset)
	{
		generatorTypeInt = 1;
		generatorType = GeneratorType.DiamondSquare;
		diamondSquareDelta = preset.diamondSquareDelta;
		diamondSquareBlend = preset.diamondSquareBlend;
	}

	public void setPerlinPreset(perlinPresetData preset)
	{
		generatorTypeInt = 2;
		generatorType = GeneratorType.Perlin;
		perlinFrequency = preset.perlinFrequency;
		perlinAmplitude = preset.perlinAmplitude;
		perlinOctaves = preset.perlinOctaves;
		perlinBlend = preset.perlinBlend;
	}

	public void setThermalErosionPreset(thermalErosionPresetData preset)
	{
		erosionTypeInt = 0;
		erosionType = ErosionType.Thermal;
		thermalIterations = preset.thermalIterations;
		thermalMinSlope = preset.thermalMinSlope;
		thermalFalloff = preset.thermalFalloff;
	}

	public void setFastHydraulicErosionPreset(fastHydraulicErosionPresetData preset)
	{
		erosionTypeInt = 1;
		erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 0;
		hydraulicType = HydraulicType.Fast;
		hydraulicIterations = preset.hydraulicIterations;
		hydraulicMaxSlope = preset.hydraulicMaxSlope;
		hydraulicFalloff = preset.hydraulicFalloff;
	}

	public void setFullHydraulicErosionPreset(fullHydraulicErosionPresetData preset)
	{
		erosionTypeInt = 1;
		erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 1;
		hydraulicType = HydraulicType.Full;
		hydraulicIterations = preset.hydraulicIterations;
		hydraulicRainfall = preset.hydraulicRainfall;
		hydraulicEvaporation = preset.hydraulicEvaporation;
		hydraulicSedimentSolubility = preset.hydraulicSedimentSolubility;
		hydraulicSedimentSaturation = preset.hydraulicSedimentSaturation;
	}

	public void setVelocityHydraulicErosionPreset(velocityHydraulicErosionPresetData preset)
	{
		erosionTypeInt = 1;
		erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 2;
		hydraulicType = HydraulicType.Velocity;
		hydraulicIterations = preset.hydraulicIterations;
		hydraulicVelocityRainfall = preset.hydraulicVelocityRainfall;
		hydraulicVelocityEvaporation = preset.hydraulicVelocityEvaporation;
		hydraulicVelocitySedimentSolubility = preset.hydraulicVelocitySedimentSolubility;
		hydraulicVelocitySedimentSaturation = preset.hydraulicVelocitySedimentSaturation;
		hydraulicVelocity = preset.hydraulicVelocity;
		hydraulicMomentum = preset.hydraulicMomentum;
		hydraulicEntropy = preset.hydraulicEntropy;
		hydraulicDowncutting = preset.hydraulicDowncutting;
	}

	public void setTidalErosionPreset(tidalErosionPresetData preset)
	{
		erosionTypeInt = 2;
		erosionType = ErosionType.Tidal;
		tidalIterations = preset.tidalIterations;
		tidalRangeAmount = preset.tidalRangeAmount;
		tidalCliffLimit = preset.tidalCliffLimit;
	}

	public void setWindErosionPreset(windErosionPresetData preset)
	{
		erosionTypeInt = 3;
		erosionType = ErosionType.Wind;
		windIterations = preset.windIterations;
		windDirection = preset.windDirection;
		windForce = preset.windForce;
		windLift = preset.windLift;
		windGravity = preset.windGravity;
		windCapacity = preset.windCapacity;
		windEntropy = preset.windEntropy;
		windSmoothing = preset.windSmoothing;
	}

	public void Update()
	{
		if (isBrushOn && (toolModeInt != 1 || erosionTypeInt > 2 || (erosionTypeInt == 1 && hydraulicTypeInt > 0)))
		{
			isBrushOn = false;
		}
	}

	public void OnDrawGizmos()
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			return;
		}
		if (isBrushOn && !isBrushHidden)
		{
			if (isBrushPainting)
			{
				Gizmos.color = Color.red;
			}
			else
			{
				Gizmos.color = Color.white;
			}
			float num = brushSize / 4f;
			Gizmos.DrawLine(brushPosition + new Vector3(0f - num, 0f, 0f), brushPosition + new Vector3(num, 0f, 0f));
			Gizmos.DrawLine(brushPosition + new Vector3(0f, 0f - num, 0f), brushPosition + new Vector3(0f, num, 0f));
			Gizmos.DrawLine(brushPosition + new Vector3(0f, 0f, 0f - num), brushPosition + new Vector3(0f, 0f, num));
			Gizmos.DrawWireCube(brushPosition, new Vector3(brushSize, 0f, brushSize));
			Gizmos.DrawWireSphere(brushPosition, brushSize / 2f);
		}
		Vector3 size = terrain.terrainData.size;
		if (toolModeInt == 1 && erosionTypeInt == 2)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(new Vector3(base.transform.position.x + size.x / 2f, tidalSeaLevel, base.transform.position.z + size.z / 2f), new Vector3(size.x, 0f, size.z));
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(new Vector3(base.transform.position.x + size.x / 2f, tidalSeaLevel, base.transform.position.z + size.z / 2f), new Vector3(size.x, tidalRangeAmount * 2f, size.z));
		}
		if (toolModeInt == 1 && erosionTypeInt == 3)
		{
			Gizmos.color = Color.blue;
			Vector3 vector = Quaternion.Euler(0f, windDirection, 0f) * Vector3.forward;
			Vector3 vector2 = new Vector3(base.transform.position.x + size.x / 2f, base.transform.position.y + size.y, base.transform.position.z + size.z / 2f);
			Vector3 vector3 = vector2 + vector * (size.x / 4f);
			Vector3 vector4 = vector2 + vector * (size.x / 6f);
			Gizmos.DrawLine(vector2, vector3);
			Gizmos.DrawLine(vector3, vector4 + new Vector3(0f, size.x / 16f, 0f));
			Gizmos.DrawLine(vector3, vector4 - new Vector3(0f, size.x / 16f, 0f));
		}
	}

	public void paint()
	{
		convertIntVarsToEnums();
		erodeTerrainWithBrush();
	}

	private void erodeTerrainWithBrush()
	{
		erosionMode = ErosionMode.Brush;
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		try
		{
			TerrainData terrainData = terrain.terrainData;
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapHeight = terrainData.heightmapHeight;
			Vector3 size = terrainData.size;
			int num3 = (int)Mathf.Floor((float)heightmapWidth / size.x * brushSize);
			int num4 = (int)Mathf.Floor((float)heightmapHeight / size.z * brushSize);
			Vector3 vector = base.transform.InverseTransformPoint(brushPosition);
			num = (int)Mathf.Round(vector.x / size.x * (float)heightmapWidth - (float)(num3 / 2));
			num2 = (int)Mathf.Round(vector.z / size.z * (float)heightmapHeight - (float)(num4 / 2));
			if (num < 0)
			{
				num3 += num;
				num = 0;
			}
			if (num2 < 0)
			{
				num4 += num2;
				num2 = 0;
			}
			if (num + num3 > heightmapWidth)
			{
				num3 = heightmapWidth - num;
			}
			if (num2 + num4 > heightmapHeight)
			{
				num4 = heightmapHeight - num2;
			}
			float[,] heights = terrainData.GetHeights(num, num2, num3, num4);
			num3 = heights.GetLength(1);
			num4 = heights.GetLength(0);
			float[,] heightMap = (float[,])heights.Clone();
			ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
			heightMap = fastErosion(heightMap, new Vector2(num3, num4), 1, erosionProgressDelegate);
			float num5 = (float)num3 / 2f;
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < num4; j++)
				{
					float num6 = heights[j, i];
					float num7 = heightMap[j, i];
					float num8 = Vector2.Distance(new Vector2(j, i), new Vector2(num5, num5));
					float num9 = 1f - (num8 - (num5 - num5 * brushSoftness)) / (num5 * brushSoftness);
					if (num9 < 0f)
					{
						num9 = 0f;
					}
					else if (num9 > 1f)
					{
						num9 = 1f;
					}
					num9 *= brushOpacity;
					float num10 = num7 * num9 + num6 * (1f - num9);
					heights[j, i] = num10;
				}
			}
			terrainData.SetHeights(num, num2, heights);
		}
		catch (Exception ex)
		{
			Debug.LogError("A brush error occurred: " + ex);
		}
	}

	public void erodeAllTerrain(ErosionProgressDelegate erosionProgressDelegate)
	{
		erosionMode = ErosionMode.Filter;
		convertIntVarsToEnums();
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			return;
		}
		try
		{
			TerrainData terrainData = terrain.terrainData;
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapHeight = terrainData.heightmapHeight;
			float[,] array = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
			switch (erosionType)
			{
			default:
				return;
			case ErosionType.Thermal:
			{
				int iterations4 = thermalIterations;
				array = fastErosion(array, new Vector2(heightmapWidth, heightmapHeight), iterations4, erosionProgressDelegate);
				break;
			}
			case ErosionType.Hydraulic:
			{
				int iterations3 = hydraulicIterations;
				switch (hydraulicType)
				{
				case HydraulicType.Fast:
					array = fastErosion(array, new Vector2(heightmapWidth, heightmapHeight), iterations3, erosionProgressDelegate);
					break;
				case HydraulicType.Full:
					array = fullHydraulicErosion(array, new Vector2(heightmapWidth, heightmapHeight), iterations3, erosionProgressDelegate);
					break;
				case HydraulicType.Velocity:
					array = velocityHydraulicErosion(array, new Vector2(heightmapWidth, heightmapHeight), iterations3, erosionProgressDelegate);
					break;
				}
				break;
			}
			case ErosionType.Tidal:
			{
				Vector3 size = terrainData.size;
				if (tidalSeaLevel >= base.transform.position.y && tidalSeaLevel <= base.transform.position.y + size.y)
				{
					int iterations2 = tidalIterations;
					array = fastErosion(array, new Vector2(heightmapWidth, heightmapHeight), iterations2, erosionProgressDelegate);
				}
				else
				{
					Debug.LogError("Sea level does not intersect terrain object. Erosion operation failed.");
				}
				break;
			}
			case ErosionType.Wind:
			{
				int iterations = windIterations;
				array = windErosion(array, new Vector2(heightmapWidth, heightmapHeight), iterations, erosionProgressDelegate);
				break;
			}
			}
			terrainData.SetHeights(0, 0, array);
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred: " + ex);
		}
	}

	private float[,] fastErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate)
	{
		int num = (int)arraySize.y;
		int num2 = (int)arraySize.x;
		float[,] array = new float[num, num2];
		Vector3 size = ((Terrain)GetComponent(typeof(Terrain))).terrainData.size;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = 0f;
		switch (erosionType)
		{
		case ErosionType.Thermal:
		{
			num3 = size.x / (float)num * Mathf.Tan(thermalMinSlope * ((float)Math.PI / 180f)) / size.y;
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			if (thermalFalloff == 1f)
			{
				thermalFalloff = 0.999f;
			}
			float num13 = thermalMinSlope + (90f - thermalMinSlope) * thermalFalloff;
			num4 = size.x / (float)num * Mathf.Tan(num13 * ((float)Math.PI / 180f)) / size.y;
			if (num4 > 1f)
			{
				num4 = 1f;
			}
			break;
		}
		case ErosionType.Hydraulic:
		{
			num6 = size.x / (float)num * Mathf.Tan(hydraulicMaxSlope * ((float)Math.PI / 180f)) / size.y;
			if (hydraulicFalloff == 0f)
			{
				hydraulicFalloff = 0.001f;
			}
			float num12 = hydraulicMaxSlope * (1f - hydraulicFalloff);
			num5 = size.x / (float)num * Mathf.Tan(num12 * ((float)Math.PI / 180f)) / size.y;
			break;
		}
		case ErosionType.Tidal:
			num7 = (tidalSeaLevel - base.transform.position.y) / (base.transform.position.y + size.y);
			num8 = (tidalSeaLevel - base.transform.position.y - tidalRangeAmount) / (base.transform.position.y + size.y);
			num9 = (tidalSeaLevel - base.transform.position.y + tidalRangeAmount) / (base.transform.position.y + size.y);
			num10 = num9 - num7;
			num11 = size.x / (float)num * Mathf.Tan(tidalCliffLimit * ((float)Math.PI / 180f)) / size.y;
			break;
		default:
			return heightMap;
		}
		for (int i = 0; i < iterations; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int num14;
				int num15;
				int num16;
				if (j == 0)
				{
					num14 = 2;
					num15 = 0;
					num16 = 0;
				}
				else if (j == num2 - 1)
				{
					num14 = 2;
					num15 = -1;
					num16 = 1;
				}
				else
				{
					num14 = 3;
					num15 = -1;
					num16 = 1;
				}
				for (int k = 0; k < num; k++)
				{
					int num17;
					int num18;
					int num19;
					if (k == 0)
					{
						num17 = 2;
						num18 = 0;
						num19 = 0;
					}
					else if (k == num - 1)
					{
						num17 = 2;
						num18 = -1;
						num19 = 1;
					}
					else
					{
						num17 = 3;
						num18 = -1;
						num19 = 1;
					}
					float num20 = 1f;
					float num21 = 0f;
					float num22 = 0f;
					float num23 = heightMap[k + num19 + num18, j + num16 + num15];
					float num24 = num23;
					int num25 = 0;
					for (int l = 0; l < num14; l++)
					{
						for (int m = 0; m < num17; m++)
						{
							if ((m == num19 && l == num16) || (neighbourhood != Neighbourhood.Moore && (neighbourhood != Neighbourhood.VonNeumann || (m != num19 && l != num16))))
							{
								continue;
							}
							float num26 = heightMap[k + m + num18, j + l + num15];
							num24 += num26;
							float num27 = num23 - num26;
							if (num27 > 0f)
							{
								num22 += num27;
								if (num27 < num20)
								{
									num20 = num27;
								}
								if (num27 > num21)
								{
									num21 = num27;
								}
							}
							num25++;
						}
					}
					float num28 = num22 / (float)num25;
					bool flag = false;
					switch (erosionType)
					{
					case ErosionType.Thermal:
						if (num28 >= num3)
						{
							flag = true;
						}
						break;
					case ErosionType.Hydraulic:
						if (num28 > 0f && num28 <= num6)
						{
							flag = true;
						}
						break;
					case ErosionType.Tidal:
						if (num28 > 0f && num28 <= num11 && num23 < num9 && num23 > num8)
						{
							flag = true;
						}
						break;
					default:
						return heightMap;
					}
					if (!flag)
					{
						continue;
					}
					float num31;
					if (erosionType == ErosionType.Tidal)
					{
						float num29 = num24 / (float)(num25 + 1);
						float num30 = Mathf.Abs(num7 - num23);
						num31 = num30 / num10;
						float num32 = num23 * num31 + num29 * (1f - num31);
						float num33 = Mathf.Pow(num30, 3f);
						heightMap[k + num19 + num18, j + num16 + num15] = num7 * num33 + num32 * (1f - num33);
						continue;
					}
					if (erosionType == ErosionType.Thermal)
					{
						if (num28 > num4)
						{
							num31 = 1f;
						}
						else
						{
							float num34 = num4 - num3;
							num31 = (num28 - num3) / num34;
						}
					}
					else if (num28 < num5)
					{
						num31 = 1f;
					}
					else
					{
						float num35 = num6 - num5;
						num31 = 1f - (num28 - num5) / num35;
					}
					float num36 = num20 / 2f * num31;
					float num37 = heightMap[k + num19 + num18, j + num16 + num15];
					if (erosionMode == ErosionMode.Filter || (erosionMode == ErosionMode.Brush && useDifferenceMaps))
					{
						float num38 = array[k + num19 + num18, j + num16 + num15] - num36;
						array[k + num19 + num18, j + num16 + num15] = num38;
					}
					else
					{
						float num39 = num37 - num36;
						if (num39 < 0f)
						{
							num39 = 0f;
						}
						heightMap[k + num19 + num18, j + num16 + num15] = num39;
					}
					for (int n = 0; n < num14; n++)
					{
						for (int num40 = 0; num40 < num17; num40++)
						{
							if ((num40 == num19 && n == num16) || (neighbourhood != Neighbourhood.Moore && (neighbourhood != Neighbourhood.VonNeumann || (num40 != num19 && n != num16))))
							{
								continue;
							}
							float num41 = heightMap[k + num40 + num18, j + n + num15];
							float num42 = num37 - num41;
							if (!(num42 > 0f))
							{
								continue;
							}
							float num43 = num36 * (num42 / num22);
							if (erosionMode == ErosionMode.Filter || (erosionMode == ErosionMode.Brush && useDifferenceMaps))
							{
								float num44 = array[k + num40 + num18, j + n + num15] + num43;
								array[k + num40 + num18, j + n + num15] = num44;
								continue;
							}
							num41 += num43;
							if (num41 < 0f)
							{
								num41 = 0f;
							}
							heightMap[k + num40 + num18, j + n + num15] = num41;
						}
					}
				}
			}
			if ((erosionMode == ErosionMode.Filter || (erosionMode == ErosionMode.Brush && useDifferenceMaps)) && erosionType != ErosionType.Tidal)
			{
				for (int num45 = 0; num45 < num2; num45++)
				{
					for (int num46 = 0; num46 < num; num46++)
					{
						float num47 = heightMap[num46, num45] + array[num46, num45];
						if (num47 > 1f)
						{
							num47 = 1f;
						}
						else if (num47 < 0f)
						{
							num47 = 0f;
						}
						heightMap[num46, num45] = num47;
						array[num46, num45] = 0f;
					}
				}
			}
			if (erosionMode == ErosionMode.Filter)
			{
				string empty = string.Empty;
				string empty2 = string.Empty;
				switch (erosionType)
				{
				case ErosionType.Thermal:
					empty = "Applying Thermal Erosion";
					empty2 = "Applying thermal erosion.";
					break;
				case ErosionType.Hydraulic:
					empty = "Applying Hydraulic Erosion";
					empty2 = "Applying hydraulic erosion.";
					break;
				case ErosionType.Tidal:
					empty = "Applying Tidal Erosion";
					empty2 = "Applying tidal erosion.";
					break;
				default:
					return heightMap;
				}
				float percentComplete = (float)i / (float)iterations;
				erosionProgressDelegate(empty, empty2, i, iterations, percentComplete);
			}
		}
		return heightMap;
	}

	private float[,] velocityHydraulicErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float[,] array = new float[num, num2];
		float[,] array2 = new float[num, num2];
		float[,] array3 = new float[num, num2];
		float[,] array4 = new float[num, num2];
		float[,] array5 = new float[num, num2];
		float[,] array6 = new float[num, num2];
		float[,] array7 = new float[num, num2];
		float[,] array8 = new float[num, num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array3[j, i] = 0f;
				array4[j, i] = 0f;
				array5[j, i] = 0f;
				array6[j, i] = 0f;
				array7[j, i] = 0f;
				array8[j, i] = 0f;
			}
		}
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				float num3 = heightMap[l, k];
				array[l, k] = num3;
			}
		}
		for (int m = 0; m < num2; m++)
		{
			int num4;
			int num5;
			int num6;
			if (m == 0)
			{
				num4 = 2;
				num5 = 0;
				num6 = 0;
			}
			else if (m == num2 - 1)
			{
				num4 = 2;
				num5 = -1;
				num6 = 1;
			}
			else
			{
				num4 = 3;
				num5 = -1;
				num6 = 1;
			}
			for (int n = 0; n < num; n++)
			{
				int num7;
				int num8;
				int num9;
				if (n == 0)
				{
					num7 = 2;
					num8 = 0;
					num9 = 0;
				}
				else if (n == num - 1)
				{
					num7 = 2;
					num8 = -1;
					num9 = 1;
				}
				else
				{
					num7 = 3;
					num8 = -1;
					num9 = 1;
				}
				float num10 = 0f;
				float num11 = heightMap[n + num9 + num8, m + num6 + num5];
				int num12 = 0;
				for (int num13 = 0; num13 < num4; num13++)
				{
					for (int num14 = 0; num14 < num7; num14++)
					{
						if ((num14 != num9 || num13 != num6) && (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (num14 == num9 || num13 == num6))))
						{
							float num15 = heightMap[n + num14 + num8, m + num13 + num5];
							float num16 = Mathf.Abs(num11 - num15);
							num10 += num16;
							num12++;
						}
					}
				}
				float num17 = num10 / (float)num12;
				array2[n + num9 + num8, m + num6 + num5] = num17;
			}
		}
		for (int num18 = 0; num18 < iterations; num18++)
		{
			for (int num19 = 0; num19 < num2; num19++)
			{
				for (int num20 = 0; num20 < num; num20++)
				{
					float num21 = array3[num20, num19] + array[num20, num19] * hydraulicVelocityRainfall;
					if (num21 > 1f)
					{
						num21 = 1f;
					}
					array3[num20, num19] = num21;
				}
			}
			for (int num22 = 0; num22 < num2; num22++)
			{
				for (int num23 = 0; num23 < num; num23++)
				{
					float num24 = array7[num23, num22];
					float num25 = array3[num23, num22] * hydraulicVelocitySedimentSaturation;
					if (num24 < num25)
					{
						float num26 = array3[num23, num22] * array5[num23, num22] * hydraulicVelocitySedimentSolubility;
						if (num24 + num26 > num25)
						{
							num26 = num25 - num24;
						}
						float num27 = heightMap[num23, num22];
						if (num26 > num27)
						{
							num26 = num27;
						}
						array7[num23, num22] = num24 + num26;
						heightMap[num23, num22] = num27 - num26;
					}
				}
			}
			for (int num28 = 0; num28 < num2; num28++)
			{
				int num29;
				int num30;
				int num31;
				if (num28 == 0)
				{
					num29 = 2;
					num30 = 0;
					num31 = 0;
				}
				else if (num28 == num2 - 1)
				{
					num29 = 2;
					num30 = -1;
					num31 = 1;
				}
				else
				{
					num29 = 3;
					num30 = -1;
					num31 = 1;
				}
				for (int num32 = 0; num32 < num; num32++)
				{
					int num33;
					int num34;
					int num35;
					if (num32 == 0)
					{
						num33 = 2;
						num34 = 0;
						num35 = 0;
					}
					else if (num32 == num - 1)
					{
						num33 = 2;
						num34 = -1;
						num35 = 1;
					}
					else
					{
						num33 = 3;
						num34 = -1;
						num35 = 1;
					}
					float num36 = 0f;
					float num37 = heightMap[num32, num28];
					float num38 = num37;
					float num39 = array3[num32, num28];
					int num40 = 0;
					for (int num41 = 0; num41 < num29; num41++)
					{
						for (int num42 = 0; num42 < num33; num42++)
						{
							if ((num42 != num35 || num41 != num31) && (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (num42 == num35 || num41 == num31))))
							{
								float num43 = heightMap[num32 + num42 + num34, num28 + num41 + num30];
								float num44 = array3[num32 + num42 + num34, num28 + num41 + num30];
								float num45 = num37 + num39 - (num43 + num44);
								if (num45 > 0f)
								{
									num36 += num45;
									num38 += num37 + num39;
									num40++;
								}
							}
						}
					}
					float num46 = array5[num32, num28];
					float num47 = array2[num32, num28];
					float num48 = array7[num32, num28];
					float num49 = num46 + hydraulicVelocity * num47;
					float num50 = num38 / (float)(num40 + 1);
					float num51 = num37 + num39 - num50;
					float num52 = Mathf.Min(num39, num51 * (1f + num46));
					float num53 = array4[num32, num28] - num52;
					array4[num32, num28] = num53;
					float num54 = num49 * (num52 / num39);
					float num55 = num48 * (num52 / num39);
					for (int num56 = 0; num56 < num29; num56++)
					{
						for (int num57 = 0; num57 < num33; num57++)
						{
							if ((num57 != num35 || num56 != num31) && (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (num57 == num35 || num56 == num31))))
							{
								float num58 = heightMap[num32 + num57 + num34, num28 + num56 + num30];
								float num59 = array3[num32 + num57 + num34, num28 + num56 + num30];
								float num60 = num37 + num39 - (num58 + num59);
								if (num60 > 0f)
								{
									float num61 = array4[num32 + num57 + num34, num28 + num56 + num30];
									float num62 = num52 * (num60 / num36);
									float num63 = num61 + num62;
									array4[num32 + num57 + num34, num28 + num56 + num30] = num63;
									float num64 = array6[num32 + num57 + num34, num28 + num56 + num30];
									float num65 = num54 * hydraulicMomentum * (num60 / num36);
									float num66 = num64 + num65;
									array6[num32 + num57 + num34, num28 + num56 + num30] = num66;
									float num67 = array8[num32 + num57 + num34, num28 + num56 + num30];
									float num68 = num55 * hydraulicMomentum * (num60 / num36);
									float num69 = num67 + num68;
									array8[num32 + num57 + num34, num28 + num56 + num30] = num69;
								}
							}
						}
					}
					float num70 = array6[num32, num28];
					array6[num32, num28] = num70 - num54;
				}
			}
			for (int num71 = 0; num71 < num2; num71++)
			{
				for (int num72 = 0; num72 < num; num72++)
				{
					float num73 = array5[num72, num71] + array6[num72, num71];
					num73 *= 1f - hydraulicEntropy;
					if (num73 > 1f)
					{
						num73 = 1f;
					}
					else if (num73 < 0f)
					{
						num73 = 0f;
					}
					array5[num72, num71] = num73;
					array6[num72, num71] = 0f;
				}
			}
			for (int num74 = 0; num74 < num2; num74++)
			{
				for (int num75 = 0; num75 < num; num75++)
				{
					float num76 = array3[num75, num74] + array4[num75, num74];
					float num77 = num76 * hydraulicVelocityEvaporation;
					num76 -= num77;
					if (num76 > 1f)
					{
						num76 = 1f;
					}
					else if (num76 < 0f)
					{
						num76 = 0f;
					}
					array3[num75, num74] = num76;
					array4[num75, num74] = 0f;
				}
			}
			for (int num78 = 0; num78 < num2; num78++)
			{
				for (int num79 = 0; num79 < num; num79++)
				{
					float num80 = array7[num79, num78] + array8[num79, num78];
					if (num80 > 1f)
					{
						num80 = 1f;
					}
					else if (num80 < 0f)
					{
						num80 = 0f;
					}
					array7[num79, num78] = num80;
					array8[num79, num78] = 0f;
				}
			}
			for (int num81 = 0; num81 < num2; num81++)
			{
				for (int num82 = 0; num82 < num; num82++)
				{
					float num83 = array3[num82, num81] * hydraulicVelocitySedimentSaturation;
					float num84 = array7[num82, num81];
					if (num84 > num83)
					{
						float num85 = num84 - num83;
						array7[num82, num81] = num83;
						float num86 = heightMap[num82, num81];
						heightMap[num82, num81] = num86 + num85;
					}
				}
			}
			for (int num87 = 0; num87 < num2; num87++)
			{
				for (int num88 = 0; num88 < num; num88++)
				{
					float num89 = array3[num88, num87];
					float num90 = heightMap[num88, num87];
					float num91 = 1f - Mathf.Abs(0.5f - num90) * 2f;
					float num92 = hydraulicDowncutting * num89 * num91;
					num90 -= num92;
					heightMap[num88, num87] = num90;
				}
			}
			float percentComplete = (float)num18 / (float)iterations;
			erosionProgressDelegate("Applying Hydraulic Erosion", "Applying hydraulic erosion.", num18, iterations, percentComplete);
		}
		return heightMap;
	}

	private float[,] fullHydraulicErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float[,] array = new float[num, num2];
		float[,] array2 = new float[num, num2];
		float[,] array3 = new float[num, num2];
		float[,] array4 = new float[num, num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[j, i] = 0f;
				array2[j, i] = 0f;
				array3[j, i] = 0f;
				array4[j, i] = 0f;
			}
		}
		for (int k = 0; k < iterations; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				for (int m = 0; m < num; m++)
				{
					float num3 = array[m, l] + hydraulicRainfall;
					if (num3 > 1f)
					{
						num3 = 1f;
					}
					array[m, l] = num3;
				}
			}
			for (int n = 0; n < num2; n++)
			{
				for (int num4 = 0; num4 < num; num4++)
				{
					float num5 = array3[num4, n];
					float num6 = array[num4, n] * hydraulicSedimentSaturation;
					if (num5 < num6)
					{
						float num7 = array[num4, n] * hydraulicSedimentSolubility;
						if (num5 + num7 > num6)
						{
							num7 = num6 - num5;
						}
						float num8 = heightMap[num4, n];
						if (num7 > num8)
						{
							num7 = num8;
						}
						array3[num4, n] = num5 + num7;
						heightMap[num4, n] = num8 - num7;
					}
				}
			}
			for (int num9 = 0; num9 < num2; num9++)
			{
				int num10;
				int num11;
				int num12;
				if (num9 == 0)
				{
					num10 = 2;
					num11 = 0;
					num12 = 0;
				}
				else if (num9 == num2 - 1)
				{
					num10 = 2;
					num11 = -1;
					num12 = 1;
				}
				else
				{
					num10 = 3;
					num11 = -1;
					num12 = 1;
				}
				for (int num13 = 0; num13 < num; num13++)
				{
					int num14;
					int num15;
					int num16;
					if (num13 == 0)
					{
						num14 = 2;
						num15 = 0;
						num16 = 0;
					}
					else if (num13 == num - 1)
					{
						num14 = 2;
						num15 = -1;
						num16 = 1;
					}
					else
					{
						num14 = 3;
						num15 = -1;
						num16 = 1;
					}
					float num17 = 0f;
					float num18 = 0f;
					float num19 = heightMap[num13 + num16 + num15, num9 + num12 + num11];
					float num20 = array[num13 + num16 + num15, num9 + num12 + num11];
					float num21 = num19;
					int num22 = 0;
					for (int num23 = 0; num23 < num10; num23++)
					{
						for (int num24 = 0; num24 < num14; num24++)
						{
							if ((num24 == num16 && num23 == num12) || (neighbourhood != Neighbourhood.Moore && (neighbourhood != Neighbourhood.VonNeumann || (num24 != num16 && num23 != num12))))
							{
								continue;
							}
							float num25 = heightMap[num13 + num24 + num15, num9 + num23 + num11];
							float num26 = array[num13 + num24 + num15, num9 + num23 + num11];
							float num27 = num19 + num20 - (num25 + num26);
							if (num27 > 0f)
							{
								num17 += num27;
								num21 += num25 + num26;
								num22++;
								if (num27 > num18)
								{
									num27 = num18;
								}
							}
						}
					}
					float num28 = num21 / (float)(num22 + 1);
					float b = num19 + num20 - num28;
					float num29 = Mathf.Min(num20, b);
					float num30 = array2[num13 + num16 + num15, num9 + num12 + num11] - num29;
					array2[num13 + num16 + num15, num9 + num12 + num11] = num30;
					float num31 = array3[num13 + num16 + num15, num9 + num12 + num11] * (num29 / num20);
					float num32 = array4[num13 + num16 + num15, num9 + num12 + num11] - num31;
					array4[num13 + num16 + num15, num9 + num12 + num11] = num32;
					for (int num33 = 0; num33 < num10; num33++)
					{
						for (int num34 = 0; num34 < num14; num34++)
						{
							if ((num34 != num16 || num33 != num12) && (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (num34 == num16 || num33 == num12))))
							{
								float num35 = heightMap[num13 + num34 + num15, num9 + num33 + num11];
								float num36 = array[num13 + num34 + num15, num9 + num33 + num11];
								float num37 = num19 + num20 - (num35 + num36);
								if (num37 > 0f)
								{
									float num38 = array2[num13 + num34 + num15, num9 + num33 + num11];
									float num39 = num29 * (num37 / num17);
									float num40 = num38 + num39;
									array2[num13 + num34 + num15, num9 + num33 + num11] = num40;
									float num41 = array4[num13 + num34 + num15, num9 + num33 + num11];
									float num42 = num31 * (num37 / num17);
									float num43 = num41 + num42;
									array4[num13 + num34 + num15, num9 + num33 + num11] = num43;
								}
							}
						}
					}
				}
			}
			for (int num44 = 0; num44 < num2; num44++)
			{
				for (int num45 = 0; num45 < num; num45++)
				{
					float num46 = array[num45, num44] + array2[num45, num44];
					float num47 = num46 * hydraulicEvaporation;
					num46 -= num47;
					if (num46 < 0f)
					{
						num46 = 0f;
					}
					array[num45, num44] = num46;
					array2[num45, num44] = 0f;
				}
			}
			for (int num48 = 0; num48 < num2; num48++)
			{
				for (int num49 = 0; num49 < num; num49++)
				{
					float num50 = array3[num49, num48] + array4[num49, num48];
					if (num50 > 1f)
					{
						num50 = 1f;
					}
					else if (num50 < 0f)
					{
						num50 = 0f;
					}
					array3[num49, num48] = num50;
					array4[num49, num48] = 0f;
				}
			}
			for (int num51 = 0; num51 < num2; num51++)
			{
				for (int num52 = 0; num52 < num; num52++)
				{
					float num53 = array[num52, num51] * hydraulicSedimentSaturation;
					float num54 = array3[num52, num51];
					if (num54 > num53)
					{
						float num55 = num54 - num53;
						array3[num52, num51] = num53;
						float num56 = heightMap[num52, num51];
						heightMap[num52, num51] = num56 + num55;
					}
				}
			}
			float percentComplete = (float)k / (float)iterations;
			erosionProgressDelegate("Applying Hydraulic Erosion", "Applying hydraulic erosion.", k, iterations, percentComplete);
		}
		return heightMap;
	}

	private float[,] windErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate)
	{
		TerrainData terrainData = ((Terrain)GetComponent(typeof(Terrain))).terrainData;
		Vector3 to = Quaternion.Euler(0f, windDirection + 180f, 0f) * Vector3.forward;
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float[,] array = new float[num, num2];
		float[,] array2 = new float[num, num2];
		float[,] array3 = new float[num, num2];
		float[,] array4 = new float[num, num2];
		float[,] array5 = new float[num, num2];
		float[,] array6 = new float[num, num2];
		float[,] array7 = new float[num, num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[j, i] = 0f;
				array2[j, i] = 0f;
				array3[j, i] = 0f;
				array4[j, i] = 0f;
				array5[j, i] = 0f;
				array6[j, i] = 0f;
				array7[j, i] = 0f;
			}
		}
		for (int k = 0; k < iterations; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				for (int m = 0; m < num; m++)
				{
					_ = array3[m, l];
					float num3 = heightMap[m, l];
					float num4 = array5[m, l];
					float num5 = num4 * windGravity;
					array5[m, l] = num4 - num5;
					heightMap[m, l] = num3 + num5;
				}
			}
			for (int n = 0; n < num2; n++)
			{
				for (int num6 = 0; num6 < num; num6++)
				{
					float num7 = heightMap[num6, n];
					Vector3 interpolatedNormal = terrainData.GetInterpolatedNormal((float)num6 / (float)num, (float)n / (float)num2);
					float num8 = (Vector3.Angle(interpolatedNormal, to) - 90f) / 90f;
					if (num8 < 0f)
					{
						num8 = 0f;
					}
					array[num6, n] = num8 * num7;
					float num9 = 1f - Mathf.Abs(Vector3.Angle(interpolatedNormal, to) - 90f) / 90f;
					array2[num6, n] = num9 * num7;
					float num10 = num9 * num7 * windForce;
					float num11 = array3[num6, n];
					float num12 = (array3[num6, n] = num11 + num10);
					float num13 = array5[num6, n];
					float num14 = windLift * num12;
					if (num13 + num14 > windCapacity)
					{
						num14 = windCapacity - num13;
					}
					array5[num6, n] = num13 + num14;
					heightMap[num6, n] = num7 - num14;
				}
			}
			for (int num15 = 0; num15 < num2; num15++)
			{
				int num16;
				int num17;
				int num18;
				if (num15 == 0)
				{
					num16 = 2;
					num17 = 0;
					num18 = 0;
				}
				else if (num15 == num2 - 1)
				{
					num16 = 2;
					num17 = -1;
					num18 = 1;
				}
				else
				{
					num16 = 3;
					num17 = -1;
					num18 = 1;
				}
				for (int num19 = 0; num19 < num; num19++)
				{
					int num20;
					int num21;
					int num22;
					if (num19 == 0)
					{
						num20 = 2;
						num21 = 0;
						num22 = 0;
					}
					else if (num19 == num - 1)
					{
						num20 = 2;
						num21 = -1;
						num22 = 1;
					}
					else
					{
						num20 = 3;
						num21 = -1;
						num22 = 1;
					}
					float num23 = array2[num19, num15];
					float num24 = array[num19, num15];
					float num25 = array5[num19, num15];
					for (int num26 = 0; num26 < num16; num26++)
					{
						for (int num27 = 0; num27 < num20; num27++)
						{
							if ((num27 != num22 || num26 != num18) && (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (num27 == num22 || num26 == num18))))
							{
								Vector3 vector = new Vector3(num27 + num21, 0f, -1 * (num26 + num17));
								float num28 = (90f - Vector3.Angle(vector, to)) / 90f;
								if (num28 < 0f)
								{
									num28 = 0f;
								}
								float num29 = array4[num19 + num27 + num21, num15 + num26 + num17];
								float num30 = num28 * (num23 - num24) * 0.1f;
								if (num30 < 0f)
								{
									num30 = 0f;
								}
								float num31 = num29 + num30;
								array4[num19 + num27 + num21, num15 + num26 + num17] = num31;
								float num32 = array4[num19, num15] - num30;
								array4[num19, num15] = num32;
								float num33 = array6[num19 + num27 + num21, num15 + num26 + num17];
								float num34 = num25 * num30;
								float num35 = num33 + num34;
								array6[num19 + num27 + num21, num15 + num26 + num17] = num35;
								float num36 = array6[num19, num15] - num34;
								array6[num19, num15] = num36;
							}
						}
					}
				}
			}
			for (int num37 = 0; num37 < num2; num37++)
			{
				for (int num38 = 0; num38 < num; num38++)
				{
					float num39 = array5[num38, num37] + array6[num38, num37];
					if (num39 > 1f)
					{
						num39 = 1f;
					}
					else if (num39 < 0f)
					{
						num39 = 0f;
					}
					array5[num38, num37] = num39;
					array6[num38, num37] = 0f;
				}
			}
			for (int num40 = 0; num40 < num2; num40++)
			{
				for (int num41 = 0; num41 < num; num41++)
				{
					float num42 = array3[num41, num40] + array4[num41, num40];
					num42 *= 1f - windEntropy;
					if (num42 > 1f)
					{
						num42 = 1f;
					}
					else if (num42 < 0f)
					{
						num42 = 0f;
					}
					array3[num41, num40] = num42;
					array4[num41, num40] = 0f;
				}
			}
			smoothIterations = 1;
			smoothBlend = 0.25f;
			float[,] heightMap2 = (float[,])heightMap.Clone();
			GeneratorProgressDelegate generatorProgressDelegate = dummyGeneratorProgress;
			heightMap2 = smooth(heightMap2, arraySize, generatorProgressDelegate);
			for (int num43 = 0; num43 < num2; num43++)
			{
				for (int num44 = 0; num44 < num; num44++)
				{
					float num45 = heightMap[num44, num43];
					float num46 = heightMap2[num44, num43];
					float num47 = array[num44, num43] * windSmoothing;
					float num48 = num46 * num47 + num45 * (1f - num47);
					heightMap[num44, num43] = num48;
				}
			}
			float percentComplete = (float)k / (float)iterations;
			erosionProgressDelegate("Applying Wind Erosion", "Applying wind erosion.", k, iterations, percentComplete);
		}
		return heightMap;
	}

	public void textureTerrain(TextureProgressDelegate textureProgressDelegate)
	{
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			return;
		}
		TerrainData terrainData = terrain.terrainData;
		splatPrototypes = terrainData.splatPrototypes;
		int num = splatPrototypes.Length;
		if (num < 2)
		{
			Debug.LogError("Error: You must assign at least 2 textures.");
			return;
		}
		textureProgressDelegate("Procedural Terrain Texture", "Generating height and slope maps. Please wait.", 0.1f);
		int num2 = terrainData.heightmapWidth - 1;
		int num3 = terrainData.heightmapHeight - 1;
		float[,] array = new float[num2, num3];
		float[,] array2 = new float[num2, num3];
		terrainData.alphamapResolution = num2;
		float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, num2, num2);
		Vector3 size = terrainData.size;
		float num4 = size.x / (float)num2 * Mathf.Tan(slopeBlendMinAngle * ((float)Math.PI / 180f)) / size.y;
		float num5 = size.x / (float)num2 * Mathf.Tan(slopeBlendMaxAngle * ((float)Math.PI / 180f)) / size.y;
		try
		{
			float num6 = 0f;
			float[,] heights = terrainData.GetHeights(0, 0, num2, num3);
			for (int i = 0; i < num3; i++)
			{
				int num7;
				int num8;
				int num9;
				if (i == 0)
				{
					num7 = 2;
					num8 = 0;
					num9 = 0;
				}
				else if (i == num3 - 1)
				{
					num7 = 2;
					num8 = -1;
					num9 = 1;
				}
				else
				{
					num7 = 3;
					num8 = -1;
					num9 = 1;
				}
				for (int j = 0; j < num2; j++)
				{
					int num10;
					int num11;
					int num12;
					if (j == 0)
					{
						num10 = 2;
						num11 = 0;
						num12 = 0;
					}
					else if (j == num2 - 1)
					{
						num10 = 2;
						num11 = -1;
						num12 = 1;
					}
					else
					{
						num10 = 3;
						num11 = -1;
						num12 = 1;
					}
					float num13 = heights[j + num12 + num11, i + num9 + num8];
					if (num13 > num6)
					{
						num6 = num13;
					}
					array[j, i] = num13;
					float num14 = 0f;
					float num15 = num10 * num7 - 1;
					for (int k = 0; k < num7; k++)
					{
						for (int l = 0; l < num10; l++)
						{
							if (l != num12 || k != num9)
							{
								float num16 = Mathf.Abs(num13 - heights[j + l + num11, i + k + num8]);
								num14 += num16;
							}
						}
					}
					float num17 = num14 / num15;
					array2[j, i] = num17;
				}
			}
			for (int m = 0; m < num3; m++)
			{
				for (int n = 0; n < num2; n++)
				{
					float num18 = array2[n, m];
					if (num18 < num4)
					{
						num18 = 0f;
					}
					else if (num18 < num5)
					{
						num18 = (num18 - num4) / (num5 - num4);
					}
					else if (num18 > num5)
					{
						num18 = 1f;
					}
					array2[n, m] = num18;
					alphamaps[n, m, 0] = num18;
				}
			}
			for (int num19 = 1; num19 < num; num19++)
			{
				for (int num20 = 0; num20 < num3; num20++)
				{
					for (int num21 = 0; num21 < num2; num21++)
					{
						float num22 = 0f;
						float num23 = 0f;
						float num24 = 1f;
						float num25 = 1f;
						float num26 = 0f;
						if (num19 > 1)
						{
							num22 = heightBlendPoints[num19 * 2 - 4];
							num23 = heightBlendPoints[num19 * 2 - 3];
						}
						if (num19 < num - 1)
						{
							num24 = heightBlendPoints[num19 * 2 - 2];
							num25 = heightBlendPoints[num19 * 2 - 1];
						}
						float num27 = array[num21, num20];
						if (num27 >= num23 && num27 <= num24)
						{
							num26 = 1f;
						}
						else if (num27 >= num22 && num27 < num23)
						{
							num26 = (num27 - num22) / (num23 - num22);
						}
						else if (num27 > num24 && num27 <= num25)
						{
							num26 = 1f - (num27 - num24) / (num25 - num24);
						}
						float num28 = array2[num21, num20];
						num26 -= num28;
						if (num26 < 0f)
						{
							num26 = 0f;
						}
						alphamaps[num21, num20, num19] = num26;
					}
				}
			}
			textureProgressDelegate("Procedural Terrain Texture", "Generating splat map. Please wait.", 0.9f);
			terrainData.SetAlphamaps(0, 0, alphamaps);
			array = null;
			array2 = null;
			alphamaps = null;
		}
		catch (Exception ex)
		{
			array = null;
			array2 = null;
			alphamaps = null;
			Debug.LogError("An error occurred: " + ex);
		}
	}

	public void addSplatPrototype(Texture2D tex, int index)
	{
		SplatPrototype[] array = new SplatPrototype[index + 1];
		for (int i = 0; i <= index; i++)
		{
			array[i] = new SplatPrototype();
			if (i == index)
			{
				array[i].texture = tex;
				array[i].tileSize = new Vector2(15f, 15f);
			}
			else
			{
				array[i].texture = splatPrototypes[i].texture;
				array[i].tileSize = splatPrototypes[i].tileSize;
			}
		}
		splatPrototypes = array;
		if (index + 1 > 2)
		{
			addBlendPoints();
		}
	}

	public void deleteSplatPrototype(Texture2D tex, int index)
	{
		int num = 0;
		num = splatPrototypes.Length;
		SplatPrototype[] array = new SplatPrototype[num - 1];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (i != index)
			{
				array[num2] = new SplatPrototype();
				array[num2].texture = splatPrototypes[i].texture;
				array[num2].tileSize = splatPrototypes[i].tileSize;
				num2++;
			}
		}
		splatPrototypes = array;
		if (num - 1 > 1)
		{
			deleteBlendPoints();
		}
	}

	public void deleteAllSplatPrototypes()
	{
		SplatPrototype[] array = new SplatPrototype[0];
		splatPrototypes = array;
	}

	public void addBlendPoints()
	{
		float num = 0f;
		if (heightBlendPoints.Count > 0)
		{
			num = heightBlendPoints[heightBlendPoints.Count - 1];
		}
		float item = num + (1f - num) * 0.33f;
		heightBlendPoints.Add(item);
		item = num + (1f - num) * 0.66f;
		heightBlendPoints.Add(item);
	}

	public void deleteBlendPoints()
	{
		if (heightBlendPoints.Count > 0)
		{
			heightBlendPoints.RemoveAt(heightBlendPoints.Count - 1);
		}
		if (heightBlendPoints.Count > 0)
		{
			heightBlendPoints.RemoveAt(heightBlendPoints.Count - 1);
		}
	}

	public void deleteAllBlendPoints()
	{
		heightBlendPoints = new List<float>();
	}

	public void generateTerrain(GeneratorProgressDelegate generatorProgressDelegate)
	{
		convertIntVarsToEnums();
		Terrain terrain = (Terrain)GetComponent(typeof(Terrain));
		if (terrain == null)
		{
			return;
		}
		TerrainData terrainData = terrain.terrainData;
		int heightmapWidth = terrainData.heightmapWidth;
		int heightmapHeight = terrainData.heightmapHeight;
		float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
		float[,] heightMap = (float[,])heights.Clone();
		switch (generatorType)
		{
		default:
			return;
		case GeneratorType.Voronoi:
			heightMap = generateVoronoi(heightMap, new Vector2(heightmapWidth, heightmapHeight), generatorProgressDelegate);
			break;
		case GeneratorType.DiamondSquare:
			heightMap = generateDiamondSquare(heightMap, new Vector2(heightmapWidth, heightmapHeight), generatorProgressDelegate);
			break;
		case GeneratorType.Perlin:
			heightMap = generatePerlin(heightMap, new Vector2(heightmapWidth, heightmapHeight), generatorProgressDelegate);
			break;
		case GeneratorType.Smooth:
			heightMap = smooth(heightMap, new Vector2(heightmapWidth, heightmapHeight), generatorProgressDelegate);
			break;
		case GeneratorType.Normalise:
			heightMap = normalise(heightMap, new Vector2(heightmapWidth, heightmapHeight), generatorProgressDelegate);
			break;
		}
		for (int i = 0; i < heightmapHeight; i++)
		{
			for (int j = 0; j < heightmapWidth; j++)
			{
				float num = heights[j, i];
				float num2 = heightMap[j, i];
				float num3 = 0f;
				switch (generatorType)
				{
				case GeneratorType.Voronoi:
					num3 = num2 * voronoiBlend + num * (1f - voronoiBlend);
					break;
				case GeneratorType.DiamondSquare:
					num3 = num2 * diamondSquareBlend + num * (1f - diamondSquareBlend);
					break;
				case GeneratorType.Perlin:
					num3 = num2 * perlinBlend + num * (1f - perlinBlend);
					break;
				case GeneratorType.Smooth:
					num3 = num2 * smoothBlend + num * (1f - smoothBlend);
					break;
				case GeneratorType.Normalise:
					num3 = num2 * normaliseBlend + num * (1f - normaliseBlend);
					break;
				}
				heights[j, i] = num3;
			}
		}
		terrainData.SetHeights(0, 0, heights);
	}

	private float[,] generateVoronoi(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < voronoiCells; i++)
		{
			Peak peak = default(Peak);
			int num3 = (int)Mathf.Floor(UnityEngine.Random.value * (float)num);
			int num4 = (int)Mathf.Floor(UnityEngine.Random.value * (float)num2);
			float peakHeight = UnityEngine.Random.value;
			if (UnityEngine.Random.value > voronoiFeatures)
			{
				peakHeight = 0f;
			}
			peak.peakPoint = new Vector2(num3, num4);
			peak.peakHeight = peakHeight;
			arrayList.Add(peak);
		}
		float num5 = 0f;
		for (int j = 0; j < num2; j++)
		{
			for (int k = 0; k < num; k++)
			{
				ArrayList arrayList2 = new ArrayList();
				for (int l = 0; l < voronoiCells; l++)
				{
					float dist = Vector2.Distance(((Peak)arrayList[l]).peakPoint, new Vector2(k, j));
					PeakDistance peakDistance = new PeakDistance();
					peakDistance.id = l;
					peakDistance.dist = dist;
					arrayList2.Add(peakDistance);
				}
				arrayList2.Sort();
				PeakDistance peakDistance2 = (PeakDistance)arrayList2[0];
				PeakDistance obj = (PeakDistance)arrayList2[1];
				int id = peakDistance2.id;
				float dist2 = peakDistance2.dist;
				float dist3 = obj.dist;
				float num6 = Mathf.Abs(dist2 - dist3) / ((float)(num + num2) / Mathf.Sqrt(voronoiCells));
				float peakHeight2 = ((Peak)arrayList[id]).peakHeight;
				float num7 = peakHeight2 - Mathf.Abs(dist2 / dist3) * peakHeight2;
				switch (voronoiType)
				{
				case VoronoiType.Sine:
				{
					float f2 = num7 * (float)Math.PI - (float)Math.PI / 2f;
					num7 = 0.5f + Mathf.Sin(f2) / 2f;
					break;
				}
				case VoronoiType.Tangent:
				{
					float f = num7 * (float)Math.PI / 2f;
					num7 = 0.5f + Mathf.Tan(f) / 2f;
					break;
				}
				}
				num7 = num7 * num6 * voronoiScale + num7 * (1f - voronoiScale);
				if (num7 < 0f)
				{
					num7 = 0f;
				}
				else if (num7 > 1f)
				{
					num7 = 1f;
				}
				heightMap[k, j] = num7;
				if (num7 > num5)
				{
					num5 = num7;
				}
			}
			float num8 = j * num2;
			float num9 = num * num2;
			float percentComplete = num8 / num9;
			generatorProgressDelegate("Voronoi Generator", "Generating height map. Please wait.", percentComplete);
		}
		for (int m = 0; m < num2; m++)
		{
			for (int n = 0; n < num; n++)
			{
				float num10 = heightMap[n, m] * (1f / num5);
				heightMap[n, m] = num10;
			}
		}
		return heightMap;
	}

	private float[,] generateDiamondSquare(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float num3 = 1f;
		int num4 = num - 1;
		heightMap[0, 0] = 0.5f;
		heightMap[num - 1, 0] = 0.5f;
		heightMap[0, num2 - 1] = 0.5f;
		heightMap[num - 1, num2 - 1] = 0.5f;
		generatorProgressDelegate("Fractal Generator", "Generating height map. Please wait.", 0f);
		while (num4 > 1)
		{
			for (int i = 0; i < num - 1; i += num4)
			{
				for (int j = 0; j < num2 - 1; j += num4)
				{
					int tx = i + (num4 >> 1);
					int ty = j + (num4 >> 1);
					dsCalculateHeight(points: new Vector2[4]
					{
						new Vector2(i, j),
						new Vector2(i + num4, j),
						new Vector2(i, j + num4),
						new Vector2(i + num4, j + num4)
					}, heightMap: heightMap, arraySize: arraySize, Tx: tx, Ty: ty, heightRange: num3);
				}
			}
			for (int k = 0; k < num - 1; k += num4)
			{
				for (int l = 0; l < num2 - 1; l += num4)
				{
					int num5 = num4 >> 1;
					int num6 = k + num5;
					int num7 = l;
					int num8 = k;
					int num9 = l + num5;
					Vector2[] points = new Vector2[4]
					{
						new Vector2(num6 - num5, num7),
						new Vector2(num6, num7 - num5),
						new Vector2(num6 + num5, num7),
						new Vector2(num6, num7 + num5)
					};
					Vector2[] points2 = new Vector2[4]
					{
						new Vector2(num8 - num5, num9),
						new Vector2(num8, num9 - num5),
						new Vector2(num8 + num5, num9),
						new Vector2(num8, num9 + num5)
					};
					dsCalculateHeight(heightMap, arraySize, num6, num7, points, num3);
					dsCalculateHeight(heightMap, arraySize, num8, num9, points2, num3);
				}
			}
			num3 *= diamondSquareDelta;
			num4 >>= 1;
		}
		generatorProgressDelegate("Fractal Generator", "Generating height map. Please wait.", 1f);
		return heightMap;
	}

	private void dsCalculateHeight(float[,] heightMap, Vector2 arraySize, int Tx, int Ty, Vector2[] points, float heightRange)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float num3 = 0f;
		for (int i = 0; i < 4; i++)
		{
			if (points[i].x < 0f)
			{
				points[i].x += num - 1;
			}
			else if (points[i].x > (float)num)
			{
				points[i].x -= num - 1;
			}
			else if (points[i].y < 0f)
			{
				points[i].y += num2 - 1;
			}
			else if (points[i].y > (float)num2)
			{
				points[i].y -= num2 - 1;
			}
			num3 += heightMap[(int)points[i].x, (int)points[i].y] / 4f;
		}
		num3 += UnityEngine.Random.value * heightRange - heightRange / 2f;
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		else if (num3 > 1f)
		{
			num3 = 1f;
		}
		heightMap[Tx, Ty] = num3;
		if (Tx == 0)
		{
			heightMap[num - 1, Ty] = num3;
		}
		else if (Tx == num - 1)
		{
			heightMap[0, Ty] = num3;
		}
		else if (Ty == 0)
		{
			heightMap[Tx, num2 - 1] = num3;
		}
		else if (Ty == num2 - 1)
		{
			heightMap[Tx, 0] = num3;
		}
	}

	private float[,] generatePerlin(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				heightMap[j, i] = 0f;
			}
		}
		PerlinNoise2D[] array = new PerlinNoise2D[perlinOctaves];
		int num3 = perlinFrequency;
		float num4 = 1f;
		for (int k = 0; k < perlinOctaves; k++)
		{
			array[k] = new PerlinNoise2D(num3, num4);
			num3 *= 2;
			num4 /= 2f;
		}
		for (int l = 0; l < perlinOctaves; l++)
		{
			double num5 = (float)num / (float)array[l].Frequency;
			double num6 = (float)num2 / (float)array[l].Frequency;
			for (int m = 0; m < num; m++)
			{
				for (int n = 0; n < num2; n++)
				{
					int num7 = (int)((double)m / num5);
					int xb = num7 + 1;
					int num8 = (int)((double)n / num6);
					int yb = num8 + 1;
					double interpolatedPoint = array[l].getInterpolatedPoint(num7, xb, num8, yb, (double)m / num5 - (double)num7, (double)n / num6 - (double)num8);
					heightMap[m, n] += (float)(interpolatedPoint * (double)array[l].Amplitude);
				}
			}
			float percentComplete = (l + 1) / perlinOctaves;
			generatorProgressDelegate("Perlin Generator", "Generating height map. Please wait.", percentComplete);
		}
		GeneratorProgressDelegate generatorProgressDelegate2 = dummyGeneratorProgress;
		float num9 = normaliseMin;
		float num10 = normaliseMax;
		float num11 = normaliseBlend;
		normaliseMin = 0f;
		normaliseMax = 1f;
		normaliseBlend = 1f;
		heightMap = normalise(heightMap, arraySize, generatorProgressDelegate2);
		normaliseMin = num9;
		normaliseMax = num10;
		normaliseBlend = num11;
		for (int num12 = 0; num12 < num; num12++)
		{
			for (int num13 = 0; num13 < num2; num13++)
			{
				heightMap[num12, num13] *= perlinAmplitude;
			}
		}
		for (int num14 = 0; num14 < perlinOctaves; num14++)
		{
			array[num14] = null;
		}
		array = null;
		return heightMap;
	}

	private float[,] smooth(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		for (int i = 0; i < smoothIterations; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int num3;
				int num4;
				int num5;
				if (j == 0)
				{
					num3 = 2;
					num4 = 0;
					num5 = 0;
				}
				else if (j == num2 - 1)
				{
					num3 = 2;
					num4 = -1;
					num5 = 1;
				}
				else
				{
					num3 = 3;
					num4 = -1;
					num5 = 1;
				}
				for (int k = 0; k < num; k++)
				{
					int num6;
					int num7;
					int num8;
					if (k == 0)
					{
						num6 = 2;
						num7 = 0;
						num8 = 0;
					}
					else if (k == num - 1)
					{
						num6 = 2;
						num7 = -1;
						num8 = 1;
					}
					else
					{
						num6 = 3;
						num7 = -1;
						num8 = 1;
					}
					float num9 = 0f;
					int num10 = 0;
					for (int l = 0; l < num3; l++)
					{
						for (int m = 0; m < num6; m++)
						{
							if (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (m == num8 || l == num5)))
							{
								float num11 = heightMap[k + m + num7, j + l + num4];
								num9 += num11;
								num10++;
							}
						}
					}
					float num12 = num9 / (float)num10;
					heightMap[k + num8 + num7, j + num5 + num4] = num12;
				}
			}
			float percentComplete = (i + 1) / smoothIterations;
			generatorProgressDelegate("Smoothing Filter", "Smoothing height map. Please wait.", percentComplete);
		}
		return heightMap;
	}

	private float[,] normalise(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate)
	{
		int num = (int)arraySize.x;
		int num2 = (int)arraySize.y;
		float num3 = 0f;
		float num4 = 1f;
		generatorProgressDelegate("Normalise Filter", "Normalising height map. Please wait.", 0f);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num5 = heightMap[j, i];
				if (num5 < num4)
				{
					num4 = num5;
				}
				else if (num5 > num3)
				{
					num3 = num5;
				}
			}
		}
		generatorProgressDelegate("Normalise Filter", "Normalising height map. Please wait.", 0.5f);
		float num6 = num3 - num4;
		float num7 = normaliseMax - normaliseMin;
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				float num8 = (heightMap[l, k] - num4) / num6 * num7;
				heightMap[l, k] = normaliseMin + num8;
			}
		}
		generatorProgressDelegate("Normalise Filter", "Normalising height map. Please wait.", 1f);
		return heightMap;
	}

	public void FastThermalErosion(int iterations, float minSlope, float blendAmount)
	{
		erosionTypeInt = 0;
		erosionType = ErosionType.Thermal;
		thermalIterations = iterations;
		thermalMinSlope = minSlope;
		thermalFalloff = blendAmount;
		neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
		erodeAllTerrain(erosionProgressDelegate);
	}

	public void FastHydraulicErosion(int iterations, float maxSlope, float blendAmount)
	{
		erosionTypeInt = 1;
		erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 0;
		hydraulicType = HydraulicType.Fast;
		hydraulicIterations = iterations;
		hydraulicMaxSlope = maxSlope;
		hydraulicFalloff = blendAmount;
		neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
		erodeAllTerrain(erosionProgressDelegate);
	}

	public void FullHydraulicErosion(int iterations, float rainfall, float evaporation, float solubility, float saturation)
	{
		erosionTypeInt = 1;
		erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 1;
		hydraulicType = HydraulicType.Full;
		hydraulicIterations = iterations;
		hydraulicRainfall = rainfall;
		hydraulicEvaporation = evaporation;
		hydraulicSedimentSolubility = solubility;
		hydraulicSedimentSaturation = saturation;
		neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
		erodeAllTerrain(erosionProgressDelegate);
	}

	public void VelocityHydraulicErosion(int iterations, float rainfall, float evaporation, float solubility, float saturation, float velocity, float momentum, float entropy, float downcutting)
	{
		erosionTypeInt = 1;
		erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 2;
		hydraulicType = HydraulicType.Velocity;
		hydraulicIterations = iterations;
		hydraulicVelocityRainfall = rainfall;
		hydraulicVelocityEvaporation = evaporation;
		hydraulicVelocitySedimentSolubility = solubility;
		hydraulicVelocitySedimentSaturation = saturation;
		hydraulicVelocity = velocity;
		hydraulicMomentum = momentum;
		hydraulicEntropy = entropy;
		hydraulicDowncutting = downcutting;
		neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
		erodeAllTerrain(erosionProgressDelegate);
	}

	public void TidalErosion(int iterations, float seaLevel, float tidalRange, float cliffLimit)
	{
		erosionTypeInt = 2;
		erosionType = ErosionType.Tidal;
		tidalIterations = iterations;
		tidalSeaLevel = seaLevel;
		tidalRangeAmount = tidalRange;
		tidalCliffLimit = cliffLimit;
		neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
		erodeAllTerrain(erosionProgressDelegate);
	}

	public void WindErosion(int iterations, float direction, float force, float lift, float gravity, float capacity, float entropy, float smoothing)
	{
		erosionTypeInt = 3;
		erosionType = ErosionType.Wind;
		windIterations = iterations;
		windDirection = direction;
		windForce = force;
		windLift = lift;
		windGravity = gravity;
		windCapacity = capacity;
		windEntropy = entropy;
		windSmoothing = smoothing;
		neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = dummyErosionProgress;
		erodeAllTerrain(erosionProgressDelegate);
	}

	public void TextureTerrain(float[] slopeStops, float[] heightStops, Texture2D[] textures)
	{
		if (slopeStops.Length != 2)
		{
			Debug.LogError("Error: slopeStops must have 2 values");
			return;
		}
		if (heightStops.Length > 8)
		{
			Debug.LogError("Error: heightStops must have no more than 8 values");
			return;
		}
		if (heightStops.Length % 2 != 0)
		{
			Debug.LogError("Error: heightStops must have an even number of values");
			return;
		}
		int num = textures.Length;
		int num2 = heightStops.Length / 2 + 2;
		if (num != num2)
		{
			Debug.LogError("Error: heightStops contains an incorrect number of values");
			return;
		}
		float[] array = slopeStops;
		foreach (float num3 in array)
		{
			if (num3 < 0f || num3 > 90f)
			{
				Debug.LogError("Error: The value of all slopeStops must be in the range 0.0 to 90.0");
				return;
			}
		}
		array = heightStops;
		foreach (float num4 in array)
		{
			if (num4 < 0f || num4 > 1f)
			{
				Debug.LogError("Error: The value of all heightStops must be in the range 0.0 to 1.0");
				return;
			}
		}
		TerrainData terrainData = ((Terrain)GetComponent(typeof(Terrain))).terrainData;
		splatPrototypes = terrainData.splatPrototypes;
		deleteAllSplatPrototypes();
		int num5 = 0;
		foreach (Texture2D tex in textures)
		{
			addSplatPrototype(tex, num5);
			num5++;
		}
		slopeBlendMinAngle = slopeStops[0];
		slopeBlendMaxAngle = slopeStops[1];
		num5 = 0;
		array = heightStops;
		foreach (float value in array)
		{
			heightBlendPoints[num5] = value;
			num5++;
		}
		terrainData.splatPrototypes = splatPrototypes;
		TextureProgressDelegate textureProgressDelegate = dummyTextureProgress;
		textureTerrain(textureProgressDelegate);
	}

	public void VoronoiGenerator(FeatureType featureType, int cells, float features, float scale, float blend)
	{
		generatorTypeInt = 0;
		generatorType = GeneratorType.Voronoi;
		switch (featureType)
		{
		case FeatureType.Mountains:
			voronoiTypeInt = 0;
			voronoiType = VoronoiType.Linear;
			break;
		case FeatureType.Hills:
			voronoiTypeInt = 1;
			voronoiType = VoronoiType.Sine;
			break;
		case FeatureType.Plateaus:
			voronoiTypeInt = 2;
			voronoiType = VoronoiType.Tangent;
			break;
		}
		voronoiCells = cells;
		voronoiFeatures = features;
		voronoiScale = scale;
		voronoiBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = dummyGeneratorProgress;
		generateTerrain(generatorProgressDelegate);
	}

	public void FractalGenerator(float fractalDelta, float blend)
	{
		generatorTypeInt = 1;
		generatorType = GeneratorType.DiamondSquare;
		diamondSquareDelta = fractalDelta;
		diamondSquareBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = dummyGeneratorProgress;
		generateTerrain(generatorProgressDelegate);
	}

	public void PerlinGenerator(int frequency, float amplitude, int octaves, float blend)
	{
		generatorTypeInt = 2;
		generatorType = GeneratorType.Perlin;
		perlinFrequency = frequency;
		perlinAmplitude = amplitude;
		perlinOctaves = octaves;
		perlinBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = dummyGeneratorProgress;
		generateTerrain(generatorProgressDelegate);
	}

	public void SmoothTerrain(int iterations, float blend)
	{
		generatorTypeInt = 3;
		generatorType = GeneratorType.Smooth;
		smoothIterations = iterations;
		smoothBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = dummyGeneratorProgress;
		generateTerrain(generatorProgressDelegate);
	}

	public void NormaliseTerrain(float minHeight, float maxHeight, float blend)
	{
		generatorTypeInt = 4;
		generatorType = GeneratorType.Normalise;
		normaliseMin = minHeight;
		normaliseMax = maxHeight;
		normaliseBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = dummyGeneratorProgress;
		generateTerrain(generatorProgressDelegate);
	}

	public void NormalizeTerrain(float minHeight, float maxHeight, float blend)
	{
		NormaliseTerrain(minHeight, maxHeight, blend);
	}

	private void convertIntVarsToEnums()
	{
		switch (erosionTypeInt)
		{
		case 0:
			erosionType = ErosionType.Thermal;
			break;
		case 1:
			erosionType = ErosionType.Hydraulic;
			break;
		case 2:
			erosionType = ErosionType.Tidal;
			break;
		case 3:
			erosionType = ErosionType.Wind;
			break;
		case 4:
			erosionType = ErosionType.Glacial;
			break;
		}
		switch (hydraulicTypeInt)
		{
		case 0:
			hydraulicType = HydraulicType.Fast;
			break;
		case 1:
			hydraulicType = HydraulicType.Full;
			break;
		case 2:
			hydraulicType = HydraulicType.Velocity;
			break;
		}
		switch (generatorTypeInt)
		{
		case 0:
			generatorType = GeneratorType.Voronoi;
			break;
		case 1:
			generatorType = GeneratorType.DiamondSquare;
			break;
		case 2:
			generatorType = GeneratorType.Perlin;
			break;
		case 3:
			generatorType = GeneratorType.Smooth;
			break;
		case 4:
			generatorType = GeneratorType.Normalise;
			break;
		}
		switch (voronoiTypeInt)
		{
		case 0:
			voronoiType = VoronoiType.Linear;
			break;
		case 1:
			voronoiType = VoronoiType.Sine;
			break;
		case 2:
			voronoiType = VoronoiType.Tangent;
			break;
		}
		switch (neighbourhoodInt)
		{
		case 0:
			neighbourhood = Neighbourhood.Moore;
			break;
		case 1:
			neighbourhood = Neighbourhood.VonNeumann;
			break;
		}
	}

	public void dummyErosionProgress(string titleString, string displayString, int iteration, int nIterations, float percentComplete)
	{
	}

	public void dummyTextureProgress(string titleString, string displayString, float percentComplete)
	{
	}

	public void dummyGeneratorProgress(string titleString, string displayString, float percentComplete)
	{
	}
}
