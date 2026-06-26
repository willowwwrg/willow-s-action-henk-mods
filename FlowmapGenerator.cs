using System.Collections.Generic;
using Flowmap;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Flowmaps/Generator")]
public class FlowmapGenerator : MonoBehaviour
{
	public static SimulationPath SimulationPath;

	private static int _threadCount = 1;

	[SerializeField]
	private List<FlowSimulationField> fields = new List<FlowSimulationField>();

	public bool gpuAcceleration;

	public bool autoAddChildFields = true;

	public int maxThreadCount = 1;

	[SerializeField]
	private Vector2 dimensions = Vector2.one;

	private Vector3 cachedPosition;

	public int outputFileFormat;

	private FlowSimulator flowSimulator;

	private FlowHeightmap heightmap;

	public static LayerMask GpuRenderLayer => LayerMask.NameToLayer("Default");

	public static bool SupportsGPUPath
	{
		get
		{
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) || !SystemInfo.supportsRenderTextures)
			{
				return false;
			}
			return true;
		}
	}

	public static int ThreadCount
	{
		get
		{
			return _threadCount;
		}
		set
		{
			_threadCount = value;
		}
	}

	public static RenderTextureFormat GetSingleChannelRTFormat
	{
		get
		{
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
			{
				return RenderTextureFormat.RFloat;
			}
			return RenderTextureFormat.ARGBHalf;
		}
	}

	public static RenderTextureFormat GetTwoChannelRTFormat
	{
		get
		{
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
			{
				return RenderTextureFormat.RGFloat;
			}
			return RenderTextureFormat.ARGBHalf;
		}
	}

	public static RenderTextureFormat GetFourChannelRTFormat
	{
		get
		{
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
			{
				return RenderTextureFormat.ARGBFloat;
			}
			return RenderTextureFormat.ARGBHalf;
		}
	}

	public FlowSimulationField[] Fields
	{
		get
		{
			CleanNullFields();
			return fields.ToArray();
		}
	}

	public Vector2 Dimensions
	{
		get
		{
			return dimensions;
		}
		set
		{
			dimensions = value;
		}
	}

	public Vector3 Position => cachedPosition;

	public FlowSimulator FlowSimulator
	{
		get
		{
			if (!flowSimulator)
			{
				flowSimulator = GetComponent<FlowSimulator>();
			}
			return flowSimulator;
		}
	}

	public FlowHeightmap Heightmap
	{
		get
		{
			if (!heightmap)
			{
				heightmap = GetComponent<FlowHeightmap>();
			}
			return heightmap;
		}
	}

	public SimulationPath GetSimulationPath()
	{
		if (gpuAcceleration && SupportsGPUPath)
		{
			return SimulationPath.GPU;
		}
		return SimulationPath.CPU;
	}

	private void Awake()
	{
		base.transform.rotation = Quaternion.identity;
		cachedPosition = base.transform.position;
		UpdateThreadCount();
	}

	private void Start()
	{
		UpdateSimulationPath();
		if ((bool)FlowSimulator)
		{
			FlowSimulator.Init();
			if (FlowSimulator.simulateOnPlay && Application.isPlaying)
			{
				FlowSimulator.StartSimulating();
			}
		}
	}

	public void UpdateSimulationPath()
	{
		SimulationPath = GetSimulationPath();
	}

	public void UpdateThreadCount()
	{
		_threadCount = maxThreadCount;
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
		cachedPosition = base.transform.position;
		if (autoAddChildFields)
		{
			FlowSimulationField[] componentsInChildren = GetComponentsInChildren<FlowSimulationField>();
			foreach (FlowSimulationField field in componentsInChildren)
			{
				AddSimulationField(field);
			}
		}
	}

	public void CleanNullFields()
	{
		fields.RemoveAll((FlowSimulationField i) => i == null);
	}

	public void AddSimulationField(FlowSimulationField field)
	{
		if (!fields.Contains(field))
		{
			fields.Add(field);
		}
	}

	public void ClearAllFields()
	{
		fields.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(base.transform.position, new Vector3(Dimensions.x, 0f, Dimensions.y));
	}
}
