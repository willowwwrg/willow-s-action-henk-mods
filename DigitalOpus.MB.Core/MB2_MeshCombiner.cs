using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[Serializable]
public class MB2_MeshCombiner
{
	[Serializable]
	private class MB_DynamicGameObject : IComparable<MB_DynamicGameObject>
	{
		public int instanceID;

		public string name;

		public int vertIdx;

		public int numVerts;

		public int bonesIdx;

		public int numBones;

		public int lightmapIndex = -1;

		public Vector4 lightmapTilingOffset = new Vector4(1f, 1f, 0f, 0f);

		public bool show = true;

		public int[] submeshTriIdxs;

		public int[] submeshNumTris;

		public int[] targetSubmeshIdxs;

		public Rect[] uvRects;

		public Rect[] obUVRects;

		public int[][] _submeshTris;

		public bool _beingDeleted;

		public int _triangleIdxAdjustment;

		public int CompareTo(MB_DynamicGameObject b)
		{
			return vertIdx - b.vertIdx;
		}
	}

	public delegate void GenerateUV2Delegate(Mesh m);

	[NonSerialized]
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	[NonSerialized]
	public MB2_ValidationLevel validationLevel = MB2_ValidationLevel.robust;

	[SerializeField]
	private bool _doNorm = true;

	[SerializeField]
	private bool _doTan = true;

	[SerializeField]
	private bool _doCol = true;

	[SerializeField]
	private bool _doUV = true;

	[SerializeField]
	private bool _doUV1 = true;

	[SerializeField]
	private int lightmapIndex = -1;

	[SerializeField]
	private List<GameObject> objectsInCombinedMesh = new List<GameObject>();

	[SerializeField]
	private List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = new List<MB_DynamicGameObject>();

	private Dictionary<int, MB_DynamicGameObject> _instance2combined_map = new Dictionary<int, MB_DynamicGameObject>();

	[SerializeField]
	private Vector3[] verts = new Vector3[0];

	[SerializeField]
	private Vector3[] normals = new Vector3[0];

	[SerializeField]
	private Vector4[] tangents = new Vector4[0];

	[SerializeField]
	private Vector2[] uvs = new Vector2[0];

	[SerializeField]
	private Vector2[] uv1s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv2s = new Vector2[0];

	[SerializeField]
	private Color[] colors = new Color[0];

	[SerializeField]
	private Matrix4x4[] bindPoses = new Matrix4x4[0];

	[SerializeField]
	private Transform[] bones = new Transform[0];

	[SerializeField]
	private Mesh _mesh;

	private int[][] submeshTris = new int[0][];

	private BoneWeight[] boneWeights = new BoneWeight[0];

	private GameObject[] empty = new GameObject[0];

	private int[] emptyIDs = new int[0];

	[SerializeField]
	private string __name;

	[SerializeField]
	private MB2_TextureBakeResults __textureBakeResults;

	[SerializeField]
	private Renderer __targetRenderer;

	[SerializeField]
	private MB_RenderType __renderType;

	[SerializeField]
	private MB2_OutputOptions __outputOption;

	[SerializeField]
	private MB2_LightmapOptions __lightmapOption;

	[SerializeField]
	private bool __doNorm;

	[SerializeField]
	private bool __doTan;

	[SerializeField]
	private bool __doCol;

	[SerializeField]
	private bool __doUV;

	[SerializeField]
	private bool __doUV1;

	private Vector2 _HALF_UV = new Vector2(0.5f, 0.5f);

	public static bool EVAL_VERSION => false;

	public string name
	{
		get
		{
			return __name;
		}
		set
		{
			__name = value;
		}
	}

	public MB2_TextureBakeResults textureBakeResults
	{
		get
		{
			return __textureBakeResults;
		}
		set
		{
			if (objectsInCombinedMesh.Count > 0 && __textureBakeResults != value && __textureBakeResults != null && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("If material bake result is changed then objects currently in combined mesh may be invalid.");
			}
			__textureBakeResults = value;
		}
	}

	public Renderer targetRenderer
	{
		get
		{
			return __targetRenderer;
		}
		set
		{
			__targetRenderer = value;
		}
	}

	public MB_RenderType renderType
	{
		get
		{
			return __renderType;
		}
		set
		{
			if (value == MB_RenderType.skinnedMeshRenderer && __renderType == MB_RenderType.meshRenderer && boneWeights.Length != verts.Length)
			{
				Debug.LogError("Can't set the render type to SkinnedMeshRenderer without clearing the mesh first. Try deleteing the CombinedMesh scene object.");
			}
			__renderType = value;
		}
	}

	public MB2_OutputOptions outputOption
	{
		get
		{
			return __outputOption;
		}
		set
		{
			__outputOption = value;
		}
	}

	public MB2_LightmapOptions lightmapOption
	{
		get
		{
			return __lightmapOption;
		}
		set
		{
			if (objectsInCombinedMesh.Count > 0 && __lightmapOption != value && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Can't change lightmap option once objects are in the combined mesh.");
			}
			__lightmapOption = value;
		}
	}

	public bool doNorm
	{
		get
		{
			return __doNorm;
		}
		set
		{
			__doNorm = value;
		}
	}

	public bool doTan
	{
		get
		{
			return __doTan;
		}
		set
		{
			__doTan = value;
		}
	}

	public bool doCol
	{
		get
		{
			return __doCol;
		}
		set
		{
			__doCol = value;
		}
	}

	public bool doUV
	{
		get
		{
			return __doUV;
		}
		set
		{
			__doUV = value;
		}
	}

	public bool doUV1
	{
		get
		{
			return __doUV1;
		}
		set
		{
			__doUV1 = value;
		}
	}

	private MB_DynamicGameObject instance2Combined_MapGet(int gameObjectID)
	{
		return _instance2combined_map[gameObjectID];
	}

	private void instance2Combined_MapAdd(int gameObjectID, MB_DynamicGameObject dgo)
	{
		_instance2combined_map.Add(gameObjectID, dgo);
	}

	private void instance2Combined_MapRemove(int gameObjectID)
	{
		_instance2combined_map.Remove(gameObjectID);
	}

	private bool instance2Combined_MapTryGetValue(int gameObjectID, out MB_DynamicGameObject dgo)
	{
		return _instance2combined_map.TryGetValue(gameObjectID, out dgo);
	}

	private int instance2Combined_MapCount()
	{
		return _instance2combined_map.Count;
	}

	private void instance2Combined_MapClear()
	{
		_instance2combined_map.Clear();
	}

	private bool instance2Combined_MapContainsKey(int gameObjectID)
	{
		return _instance2combined_map.ContainsKey(gameObjectID);
	}

	public bool doUV2()
	{
		if (lightmapOption != MB2_LightmapOptions.copy_UV2_unchanged)
		{
			return lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping;
		}
		return true;
	}

	public int GetNumObjectsInCombined()
	{
		return objectsInCombinedMesh.Count;
	}

	public List<GameObject> GetObjectsInCombined()
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(objectsInCombinedMesh);
		return list;
	}

	public Mesh GetMesh()
	{
		if (_mesh == null)
		{
			_mesh = new Mesh();
		}
		return _mesh;
	}

	public Transform[] GetBones()
	{
		return bones;
	}

	public int GetLightmapIndex()
	{
		if (lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout || lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
		{
			return lightmapIndex;
		}
		return -1;
	}

	public int GetNumVerticesFor(GameObject go)
	{
		return GetNumVerticesFor(go.GetInstanceID());
	}

	public int GetNumVerticesFor(int instanceID)
	{
		if (instance2Combined_MapTryGetValue(instanceID, out var dgo))
		{
			return dgo.numVerts;
		}
		return -1;
	}

	private void _initialize()
	{
		if (objectsInCombinedMesh.Count == 0)
		{
			lightmapIndex = -1;
		}
		if (_mesh == null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("_initialize Creating new Mesh");
			}
			_mesh = new Mesh();
		}
		if (instance2Combined_MapCount() != objectsInCombinedMesh.Count)
		{
			instance2Combined_MapClear();
			for (int i = 0; i < objectsInCombinedMesh.Count; i++)
			{
				instance2Combined_MapAdd(objectsInCombinedMesh[i].GetInstanceID(), mbDynamicObjectsInCombinedMesh[i]);
			}
			boneWeights = _mesh.boneWeights;
			submeshTris = new int[_mesh.subMeshCount][];
			for (int j = 0; j < submeshTris.Length; j++)
			{
				submeshTris[j] = _mesh.GetTriangles(j);
			}
		}
	}

	private bool _collectMaterialTriangles(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map)
	{
		int num = m.subMeshCount;
		if (sharedMaterials.Length < num)
		{
			num = sharedMaterials.Length;
		}
		dgo._submeshTris = new int[num][];
		dgo.targetSubmeshIdxs = new int[num];
		for (int i = 0; i < num; i++)
		{
			if (textureBakeResults.doMultiMaterial)
			{
				if (!sourceMats2submeshIdx_map.Contains(sharedMaterials[i]))
				{
					Debug.LogError("Object " + dgo.name + " has a material that was not found in the result materials maping. " + sharedMaterials[i]);
					return false;
				}
				dgo.targetSubmeshIdxs[i] = (int)sourceMats2submeshIdx_map[sharedMaterials[i]];
			}
			else
			{
				dgo.targetSubmeshIdxs[i] = 0;
			}
			dgo._submeshTris[i] = m.GetTriangles(i);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Collecting triangles for: " + dgo.name + " submesh:" + i + " maps to submesh:" + dgo.targetSubmeshIdxs[i] + " added:" + dgo._submeshTris[i].Length, LOG_LEVEL);
			}
		}
		return true;
	}

	private bool _collectOutOfBoundsUVRects2(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map)
	{
		if (textureBakeResults == null)
		{
			Debug.LogError("Need to bake textures into combined material");
			return false;
		}
		int num = m.subMeshCount;
		if (sharedMaterials.Length < num)
		{
			num = sharedMaterials.Length;
		}
		dgo.obUVRects = new Rect[num];
		for (int i = 0; i < dgo.obUVRects.Length; i++)
		{
			ref Rect reference = ref dgo.obUVRects[i];
			reference = new Rect(0f, 0f, 1f, 1f);
		}
		for (int j = 0; j < num; j++)
		{
			Rect uvBounds = default(Rect);
			MB_Utility.hasOutOfBoundsUVs(m, ref uvBounds, j);
			dgo.obUVRects[j] = uvBounds;
		}
		return true;
	}

	private bool _validateTextureBakeResults()
	{
		if (textureBakeResults == null)
		{
			Debug.LogError("Material Bake Results is null. Can't combine meshes.");
			return false;
		}
		if (textureBakeResults.materials == null || textureBakeResults.materials.Length == 0)
		{
			Debug.LogError("Material Bake Results has no materials in material to uvRect map. Try baking materials. Can't combine meshes.");
			return false;
		}
		if (textureBakeResults.doMultiMaterial)
		{
			if (textureBakeResults.resultMaterials == null || textureBakeResults.resultMaterials.Length == 0)
			{
				Debug.LogError("Material Bake Results has no result materials. Try baking materials. Can't combine meshes.");
				return false;
			}
		}
		else if (textureBakeResults.resultMaterial == null)
		{
			Debug.LogError("Material Bake Results has no result material. Try baking materials. Can't combine meshes.");
			return false;
		}
		return true;
	}

	private bool _validateMeshFlags()
	{
		if (objectsInCombinedMesh.Count > 0 && ((!_doNorm && doNorm) || (!_doTan && doTan) || (!_doCol && doCol) || (!_doUV && doUV) || (!_doUV1 && doUV1)))
		{
			Debug.LogError("The channels have changed. There are already objects in the combined mesh that were added with a different set of channels.");
			return false;
		}
		_doNorm = doNorm;
		_doTan = doTan;
		_doCol = doCol;
		_doUV = doUV;
		_doUV1 = doUV1;
		return true;
	}

	private bool getIsGameObjectActive(GameObject g)
	{
		return g.activeInHierarchy;
	}

	private bool _showHide(GameObject[] goToShow, GameObject[] goToHide)
	{
		if (goToShow == null)
		{
			goToShow = empty;
		}
		if (goToHide == null)
		{
			goToHide = empty;
		}
		_initialize();
		for (int i = 0; i < goToHide.Length; i++)
		{
			if (!instance2Combined_MapContainsKey(goToHide[i].GetInstanceID()))
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning(string.Concat("Trying to hide an object ", goToHide[i], " that is not in combined mesh"));
				}
				return false;
			}
		}
		for (int j = 0; j < goToShow.Length; j++)
		{
			if (!instance2Combined_MapContainsKey(goToShow[j].GetInstanceID()))
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning(string.Concat("Trying to show an object ", goToShow[j], " that is not in combined mesh"));
				}
				return false;
			}
		}
		for (int k = 0; k < goToHide.Length; k++)
		{
			_instance2combined_map[goToHide[k].GetInstanceID()].show = false;
		}
		for (int l = 0; l < goToShow.Length; l++)
		{
			_instance2combined_map[goToShow[l].GetInstanceID()].show = true;
		}
		return true;
	}

	private bool _addToCombined(GameObject[] goToAdd, int[] goToDelete, bool disableRendererInSource)
	{
		if (!_validateTextureBakeResults())
		{
			return false;
		}
		if (!_validateMeshFlags())
		{
			return false;
		}
		if (outputOption != MB2_OutputOptions.bakeMeshAssetsInPlace && renderType == MB_RenderType.skinnedMeshRenderer && (targetRenderer == null || !(__targetRenderer is SkinnedMeshRenderer)))
		{
			Debug.LogError("Target renderer must be set and must be a SkinnedMeshRenderer");
			return false;
		}
		GameObject[] _goToAdd;
		if (goToAdd == null)
		{
			_goToAdd = empty;
		}
		else
		{
			_goToAdd = (GameObject[])goToAdd.Clone();
		}
		int[] array = ((goToDelete != null) ? ((int[])goToDelete.Clone()) : emptyIDs);
		if (_mesh == null)
		{
			DestroyMesh();
		}
		Dictionary<Material, Rect> mat2RectMap = textureBakeResults.GetMat2RectMap();
		_initialize();
		int num = 1;
		if (textureBakeResults.doMultiMaterial)
		{
			num = textureBakeResults.resultMaterials.Length;
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.LogDebug("==== Calling _addToCombined objs adding:" + _goToAdd.Length + " objs deleting:" + array.Length + " fixOutOfBounds:" + textureBakeResults.fixOutOfBoundsUVs + " doMultiMaterial:" + textureBakeResults.doMultiMaterial + " disableRenderersInSource:" + disableRendererInSource, LOG_LEVEL);
		}
		OrderedDictionary orderedDictionary = null;
		if (textureBakeResults.doMultiMaterial)
		{
			orderedDictionary = new OrderedDictionary();
			for (int i = 0; i < num; i++)
			{
				MB_MultiMaterial mB_MultiMaterial = textureBakeResults.resultMaterials[i];
				for (int j = 0; j < mB_MultiMaterial.sourceMaterials.Count; j++)
				{
					if (mB_MultiMaterial.sourceMaterials[j] == null)
					{
						Debug.LogError("Found null material in source materials for combined mesh materials " + i);
						return false;
					}
					if (!orderedDictionary.Contains(mB_MultiMaterial.sourceMaterials[j]))
					{
						orderedDictionary.Add(mB_MultiMaterial.sourceMaterials[j], i);
					}
				}
			}
		}
		if (submeshTris.Length != num)
		{
			submeshTris = new int[num][];
			for (int k = 0; k < submeshTris.Length; k++)
			{
				submeshTris[k] = new int[0];
			}
		}
		int num2 = 0;
		int num3 = 0;
		int[] array2 = new int[num];
		for (int l = 0; l < array.Length; l++)
		{
			if (instance2Combined_MapTryGetValue(array[l], out var dgo))
			{
				num2 += dgo.numVerts;
				if (renderType == MB_RenderType.skinnedMeshRenderer)
				{
					num3 += dgo.numBones;
				}
				for (int m = 0; m < dgo.submeshNumTris.Length; m++)
				{
					array2[m] += dgo.submeshNumTris[m];
				}
			}
			else if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Trying to delete an object that is not in combined mesh");
			}
		}
		List<MB_DynamicGameObject> list = new List<MB_DynamicGameObject>();
		int num4 = 0;
		int num5 = 0;
		int[] array3 = new int[num];
		int n;
		for (n = 0; n < _goToAdd.Length; n++)
		{
			if (!instance2Combined_MapContainsKey(_goToAdd[n].GetInstanceID()) || Array.FindIndex(array, (int o) => o == _goToAdd[n].GetInstanceID()) != -1)
			{
				MB_DynamicGameObject mB_DynamicGameObject = new MB_DynamicGameObject();
				GameObject gameObject = _goToAdd[n];
				Material[] gOMaterials = MB_Utility.GetGOMaterials(gameObject);
				if (gOMaterials == null)
				{
					Debug.LogError("Object " + gameObject.name + " does not have a Renderer");
					_goToAdd[n] = null;
					return false;
				}
				Mesh mesh = MB_Utility.GetMesh(gameObject);
				if (mesh == null)
				{
					Debug.LogError("Object " + gameObject.name + " MeshFilter or SkinedMeshRenderer had no mesh");
					_goToAdd[n] = null;
					return false;
				}
				Rect[] array4 = new Rect[gOMaterials.Length];
				for (int num6 = 0; num6 < gOMaterials.Length; num6++)
				{
					if (!mat2RectMap.TryGetValue(gOMaterials[num6], out array4[num6]))
					{
						Debug.LogError(string.Concat("Object ", gameObject.name, " has an unknown material ", gOMaterials[num6], ". Try baking textures"));
						_goToAdd[n] = null;
					}
				}
				if (!(_goToAdd[n] != null))
				{
					continue;
				}
				list.Add(mB_DynamicGameObject);
				mB_DynamicGameObject.name = $"{_goToAdd[n].ToString()} {_goToAdd[n].GetInstanceID()}";
				mB_DynamicGameObject.instanceID = _goToAdd[n].GetInstanceID();
				mB_DynamicGameObject.uvRects = array4;
				mB_DynamicGameObject.numVerts = mesh.vertexCount;
				Renderer renderer = MB_Utility.GetRenderer(gameObject);
				mB_DynamicGameObject.numBones = _getNumBones(renderer);
				if (lightmapIndex == -1)
				{
					lightmapIndex = renderer.lightmapIndex;
				}
				if (lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
				{
					if (lightmapIndex != renderer.lightmapIndex && LOG_LEVEL >= MB2_LogLevel.warn)
					{
						Debug.LogWarning("Object " + gameObject.name + " has a different lightmap index. Lightmapping will not work.");
					}
					if (!getIsGameObjectActive(gameObject) && LOG_LEVEL >= MB2_LogLevel.warn)
					{
						Debug.LogWarning("Object " + gameObject.name + " is inactive. Can only get lightmap index of active objects.");
					}
					if (renderer.lightmapIndex == -1 && LOG_LEVEL >= MB2_LogLevel.warn)
					{
						Debug.LogWarning("Object " + gameObject.name + " does not have an index to a lightmap.");
					}
				}
				mB_DynamicGameObject.lightmapIndex = renderer.lightmapIndex;
				mB_DynamicGameObject.lightmapTilingOffset = renderer.lightmapTilingOffset;
				if (!_collectMaterialTriangles(mesh, mB_DynamicGameObject, gOMaterials, orderedDictionary))
				{
					return false;
				}
				mB_DynamicGameObject.submeshNumTris = new int[num];
				mB_DynamicGameObject.submeshTriIdxs = new int[num];
				if (textureBakeResults.fixOutOfBoundsUVs && !_collectOutOfBoundsUVRects2(mesh, mB_DynamicGameObject, gOMaterials, orderedDictionary))
				{
					return false;
				}
				num4 += mB_DynamicGameObject.numVerts;
				if (renderType == MB_RenderType.skinnedMeshRenderer)
				{
					num5 += mB_DynamicGameObject.numBones;
				}
				for (int num7 = 0; num7 < mB_DynamicGameObject._submeshTris.Length; num7++)
				{
					array3[mB_DynamicGameObject.targetSubmeshIdxs[num7]] += mB_DynamicGameObject._submeshTris[num7].Length;
				}
			}
			else
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					Debug.LogWarning("Object " + _goToAdd[n].name + " has already been added");
				}
				_goToAdd[n] = null;
			}
		}
		for (int num8 = 0; num8 < _goToAdd.Length; num8++)
		{
			if (_goToAdd[num8] != null && disableRendererInSource)
			{
				MB_Utility.DisableRendererInSource(_goToAdd[num8]);
			}
		}
		int num9 = verts.Length + num4 - num2;
		int num10 = bindPoses.Length + num5 - num3;
		int[] array5 = new int[num];
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.LogDebug("Verts adding:" + num4 + " deleting:" + num2 + " submeshes:" + array5.Length + " bones:" + num10, LOG_LEVEL);
		}
		for (int num11 = 0; num11 < array5.Length; num11++)
		{
			array5[num11] = submeshTris[num11].Length + array3[num11] - array2[num11];
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("    submesh :" + num11 + " already contains:" + submeshTris[num11].Length + " tris to be Added:" + array3[num11] + " tris to be Deleted:" + array2[num11]);
			}
		}
		if (num9 > 65534)
		{
			Debug.LogError("Cannot add objects. Resulting mesh will have more than 64k vertices. Try using a Multi-MeshBaker component. This will split the combined mesh into several meshes. You don't have to re-configure the MB2_TextureBaker. Just remove the MB2_MeshBaker component and add a MB2_MultiMeshBaker component.");
			return false;
		}
		Vector3[] destinationArray = null;
		Vector4[] destinationArray2 = null;
		Vector2[] destinationArray3 = null;
		Vector2[] destinationArray4 = null;
		Vector2[] destinationArray5 = null;
		Color[] destinationArray6 = null;
		Vector3[] destinationArray7 = new Vector3[num9];
		if (_doNorm)
		{
			destinationArray = new Vector3[num9];
		}
		if (_doTan)
		{
			destinationArray2 = new Vector4[num9];
		}
		if (_doUV)
		{
			destinationArray3 = new Vector2[num9];
		}
		if (_doUV1)
		{
			destinationArray4 = new Vector2[num9];
		}
		if (lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged || lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
		{
			destinationArray5 = new Vector2[num9];
		}
		if (_doCol)
		{
			destinationArray6 = new Color[num9];
		}
		BoneWeight[] destinationArray8 = new BoneWeight[num9];
		Matrix4x4[] array6 = new Matrix4x4[num10];
		Transform[] destinationArray9 = new Transform[num10];
		int[][] array7 = null;
		array7 = new int[num][];
		for (int num12 = 0; num12 < array7.Length; num12++)
		{
			array7[num12] = new int[array5[num12]];
		}
		for (int num13 = 0; num13 < array.Length; num13++)
		{
			MB_DynamicGameObject dgo2 = null;
			if (instance2Combined_MapTryGetValue(array[num13], out dgo2))
			{
				dgo2._beingDeleted = true;
			}
		}
		mbDynamicObjectsInCombinedMesh.Sort();
		int num14 = 0;
		int num15 = 0;
		int[] array8 = new int[num];
		int num16 = 0;
		int num17 = 0;
		for (int num18 = 0; num18 < mbDynamicObjectsInCombinedMesh.Count; num18++)
		{
			MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[num18];
			if (!mB_DynamicGameObject2._beingDeleted)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Copying obj in combined arrays idx:" + num18, LOG_LEVEL);
				}
				Array.Copy(verts, mB_DynamicGameObject2.vertIdx, destinationArray7, num14, mB_DynamicGameObject2.numVerts);
				if (_doNorm)
				{
					Array.Copy(normals, mB_DynamicGameObject2.vertIdx, destinationArray, num14, mB_DynamicGameObject2.numVerts);
				}
				if (_doTan)
				{
					Array.Copy(tangents, mB_DynamicGameObject2.vertIdx, destinationArray2, num14, mB_DynamicGameObject2.numVerts);
				}
				if (_doUV)
				{
					Array.Copy(uvs, mB_DynamicGameObject2.vertIdx, destinationArray3, num14, mB_DynamicGameObject2.numVerts);
				}
				if (_doUV1)
				{
					Array.Copy(uv1s, mB_DynamicGameObject2.vertIdx, destinationArray4, num14, mB_DynamicGameObject2.numVerts);
				}
				if (doUV2())
				{
					Array.Copy(uv2s, mB_DynamicGameObject2.vertIdx, destinationArray5, num14, mB_DynamicGameObject2.numVerts);
				}
				if (_doCol)
				{
					Array.Copy(colors, mB_DynamicGameObject2.vertIdx, destinationArray6, num14, mB_DynamicGameObject2.numVerts);
				}
				if (renderType == MB_RenderType.skinnedMeshRenderer)
				{
					for (int num19 = mB_DynamicGameObject2.vertIdx; num19 < mB_DynamicGameObject2.vertIdx + mB_DynamicGameObject2.numVerts; num19++)
					{
						boneWeights[num19].boneIndex0 = boneWeights[num19].boneIndex0 - num17;
						boneWeights[num19].boneIndex1 = boneWeights[num19].boneIndex1 - num17;
						boneWeights[num19].boneIndex2 = boneWeights[num19].boneIndex2 - num17;
						boneWeights[num19].boneIndex3 = boneWeights[num19].boneIndex3 - num17;
					}
					Array.Copy(boneWeights, mB_DynamicGameObject2.vertIdx, destinationArray8, num14, mB_DynamicGameObject2.numVerts);
				}
				Array.Copy(bindPoses, mB_DynamicGameObject2.bonesIdx, array6, num15, mB_DynamicGameObject2.numBones);
				Array.Copy(bones, mB_DynamicGameObject2.bonesIdx, destinationArray9, num15, mB_DynamicGameObject2.numBones);
				for (int num20 = 0; num20 < num; num20++)
				{
					int[] array9 = submeshTris[num20];
					int num21 = mB_DynamicGameObject2.submeshTriIdxs[num20];
					int num22 = mB_DynamicGameObject2.submeshNumTris[num20];
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("    Adjusting submesh triangles submesh:" + num20 + " startIdx:" + num21 + " num:" + num22, LOG_LEVEL);
					}
					for (int num23 = num21; num23 < num21 + num22; num23++)
					{
						array9[num23] -= num16;
					}
					Array.Copy(array9, num21, array7[num20], array8[num20], num22);
				}
				mB_DynamicGameObject2.bonesIdx = num15;
				mB_DynamicGameObject2.vertIdx = num14;
				for (int num24 = 0; num24 < array8.Length; num24++)
				{
					mB_DynamicGameObject2.submeshTriIdxs[num24] = array8[num24];
					array8[num24] += mB_DynamicGameObject2.submeshNumTris[num24];
				}
				num15 += mB_DynamicGameObject2.numBones;
				num14 += mB_DynamicGameObject2.numVerts;
			}
			else
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Not copying obj: " + num18, LOG_LEVEL);
				}
				num16 += mB_DynamicGameObject2.numVerts;
				num17 += mB_DynamicGameObject2.numBones;
			}
		}
		for (int num25 = mbDynamicObjectsInCombinedMesh.Count - 1; num25 >= 0; num25--)
		{
			if (mbDynamicObjectsInCombinedMesh[num25]._beingDeleted)
			{
				instance2Combined_MapRemove(mbDynamicObjectsInCombinedMesh[num25].instanceID);
				objectsInCombinedMesh.RemoveAt(num25);
				mbDynamicObjectsInCombinedMesh.RemoveAt(num25);
			}
		}
		verts = destinationArray7;
		if (_doNorm)
		{
			normals = destinationArray;
		}
		if (_doTan)
		{
			tangents = destinationArray2;
		}
		if (_doUV)
		{
			uvs = destinationArray3;
		}
		if (_doUV1)
		{
			uv1s = destinationArray4;
		}
		if (doUV2())
		{
			uv2s = destinationArray5;
		}
		if (_doCol)
		{
			colors = destinationArray6;
		}
		if (renderType == MB_RenderType.skinnedMeshRenderer)
		{
			boneWeights = destinationArray8;
		}
		bindPoses = array6;
		bones = destinationArray9;
		submeshTris = array7;
		for (int num26 = 0; num26 < list.Count; num26++)
		{
			MB_DynamicGameObject mB_DynamicGameObject3 = list[num26];
			GameObject gameObject2 = _goToAdd[num26];
			int num27 = num14;
			Mesh mesh2 = MB_Utility.GetMesh(gameObject2);
			Matrix4x4 localToWorldMatrix = gameObject2.transform.localToWorldMatrix;
			Quaternion rotation = gameObject2.transform.rotation;
			destinationArray7 = mesh2.vertices;
			Vector3[] array10 = null;
			Vector4[] array11 = null;
			if (_doNorm)
			{
				array10 = _getMeshNormals(mesh2);
			}
			if (_doTan)
			{
				array11 = _getMeshTangents(mesh2);
			}
			if (renderType != MB_RenderType.skinnedMeshRenderer)
			{
				for (int num28 = 0; num28 < destinationArray7.Length; num28++)
				{
					ref Vector3 reference = ref destinationArray7[num28];
					reference = localToWorldMatrix.MultiplyPoint(destinationArray7[num28]);
					if (_doNorm)
					{
						ref Vector3 reference2 = ref array10[num28];
						reference2 = rotation * array10[num28];
					}
					if (_doTan)
					{
						float w = array11[num28].w;
						ref Vector4 reference3 = ref array11[num28];
						reference3 = rotation * array11[num28];
						array11[num28].w = w;
					}
				}
			}
			if (_doNorm)
			{
				array10.CopyTo(normals, num27);
			}
			if (_doTan)
			{
				array11.CopyTo(tangents, num27);
			}
			destinationArray7.CopyTo(verts, num27);
			int subMeshCount = mesh2.subMeshCount;
			if (mB_DynamicGameObject3.uvRects.Length < subMeshCount)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + mB_DynamicGameObject3.name + " has more submeshes than materials");
				}
				subMeshCount = mB_DynamicGameObject3.uvRects.Length;
			}
			else if (mB_DynamicGameObject3.uvRects.Length > subMeshCount && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Mesh " + mB_DynamicGameObject3.name + " has fewer submeshes than materials");
			}
			if (_doUV)
			{
				_copyAndAdjustUVsFromMesh(mB_DynamicGameObject3, mesh2, num27);
			}
			if (doUV2())
			{
				_copyAndAdjustUV2FromMesh(mB_DynamicGameObject3, mesh2, num27);
			}
			if (_doUV1)
			{
				destinationArray4 = _getMeshUV1s(mesh2);
				destinationArray4.CopyTo(uv1s, num27);
			}
			if (_doCol)
			{
				destinationArray6 = _getMeshColors(mesh2);
				destinationArray6.CopyTo(colors, num27);
			}
			if (renderType == MB_RenderType.skinnedMeshRenderer)
			{
				num15 += _copyBonesBindPosesAndBoneWeightsFromMesh(gameObject2, mB_DynamicGameObject3, num27, num15);
			}
			for (int num29 = 0; num29 < array8.Length; num29++)
			{
				mB_DynamicGameObject3.submeshTriIdxs[num29] = array8[num29];
			}
			for (int num30 = 0; num30 < mB_DynamicGameObject3._submeshTris.Length; num30++)
			{
				int[] array12 = mB_DynamicGameObject3._submeshTris[num30];
				for (int num31 = 0; num31 < array12.Length; num31++)
				{
					array12[num31] += num27;
				}
				int num32 = mB_DynamicGameObject3.targetSubmeshIdxs[num30];
				array12.CopyTo(submeshTris[num32], array8[num32]);
				mB_DynamicGameObject3.submeshNumTris[num32] += array12.Length;
				array8[num32] += array12.Length;
			}
			mB_DynamicGameObject3.vertIdx = num14;
			instance2Combined_MapAdd(gameObject2.GetInstanceID(), mB_DynamicGameObject3);
			objectsInCombinedMesh.Add(gameObject2);
			mbDynamicObjectsInCombinedMesh.Add(mB_DynamicGameObject3);
			num14 += destinationArray7.Length;
			for (int num33 = 0; num33 < mB_DynamicGameObject3._submeshTris.Length; num33++)
			{
				mB_DynamicGameObject3._submeshTris[num33] = null;
			}
			mB_DynamicGameObject3._submeshTris = null;
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Added to combined:" + mB_DynamicGameObject3.name + " verts:" + destinationArray7.Length + " bindPoses:" + array6.Length, LOG_LEVEL);
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.LogDebug("===== _addToCombined completed. Verts in buffer: " + verts.Length, LOG_LEVEL);
		}
		return true;
	}

	private void _copyAndAdjustUVsFromMesh(MB_DynamicGameObject dgo, Mesh mesh, int vertsIdx)
	{
		Vector2[] array = _getMeshUVs(mesh);
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = -1;
		}
		bool flag = false;
		Rect rect = default(Rect);
		for (int j = 0; j < dgo.targetSubmeshIdxs.Length; j++)
		{
			int[] obj = ((dgo._submeshTris == null) ? mesh.GetTriangles(j) : dgo._submeshTris[j]);
			Rect rect2 = dgo.uvRects[j];
			if (textureBakeResults.fixOutOfBoundsUVs)
			{
				rect = dgo.obUVRects[j];
			}
			int[] array3 = obj;
			foreach (int num in array3)
			{
				if (array2[num] == -1)
				{
					array2[num] = j;
					if (textureBakeResults.fixOutOfBoundsUVs)
					{
						array[num].x = array[num].x / rect.width - rect.x / rect.width;
						array[num].y = array[num].y / rect.height - rect.y / rect.height;
					}
					array[num].x = rect2.x + array[num].x * rect2.width;
					array[num].y = rect2.y + array[num].y * rect2.height;
				}
				if (array2[num] != j)
				{
					flag = true;
				}
			}
		}
		if (flag && LOG_LEVEL >= MB2_LogLevel.warn)
		{
			Debug.LogWarning(dgo.name + "has submeshes which share verticies. Adjusted uvs may not map correctly in combined atlas.");
		}
		array.CopyTo(uvs, vertsIdx);
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			Debug.Log("_copyAndAdjustUVsFromMesh copied " + array.Length);
		}
	}

	private void _copyAndAdjustUV2FromMesh(MB_DynamicGameObject dgo, Mesh mesh, int vertsIdx)
	{
		Vector2[] array = _getMeshUV2s(mesh);
		if (lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
		{
			Vector4 lightmapTilingOffset = dgo.lightmapTilingOffset;
			Vector2 vector = new Vector2(lightmapTilingOffset.x, lightmapTilingOffset.y);
			Vector2 vector2 = new Vector2(lightmapTilingOffset.z, lightmapTilingOffset.w);
			Vector2 vector3 = default(Vector2);
			for (int i = 0; i < array.Length; i++)
			{
				vector3.x = vector.x * array[i].x;
				vector3.y = vector.y * array[i].y;
				ref Vector2 reference = ref array[i];
				reference = vector2 + vector3;
			}
		}
		array.CopyTo(uv2s, vertsIdx);
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			Debug.Log("_copyAndAdjustUV2FromMesh copied " + array.Length);
		}
	}

	private int _copyBonesBindPosesAndBoneWeightsFromMesh(GameObject go, MB_DynamicGameObject dgo, int vertsIdx, int bonesIdx)
	{
		Renderer renderer = MB_Utility.GetRenderer(go);
		_getBones(renderer).CopyTo(bones, bonesIdx);
		Matrix4x4[] array = _getBindPoses(renderer);
		array.CopyTo(bindPoses, bonesIdx);
		BoneWeight[] array2 = _getBoneWeights(renderer, dgo.numVerts);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].boneIndex0 = array2[i].boneIndex0 + bonesIdx;
			array2[i].boneIndex1 = array2[i].boneIndex1 + bonesIdx;
			array2[i].boneIndex2 = array2[i].boneIndex2 + bonesIdx;
			array2[i].boneIndex3 = array2[i].boneIndex3 + bonesIdx;
		}
		array2.CopyTo(boneWeights, vertsIdx);
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			Debug.Log("_copyBonesBindPosesAndBoneWeightsFromMesh copied " + array2.Length);
		}
		dgo.bonesIdx = bonesIdx;
		return array.Length;
	}

	private Color[] _getMeshColors(Mesh m)
	{
		Color[] array = m.colors;
		if (array.Length == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no colors. Generating"));
			}
			if (_doCol && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning(string.Concat("Mesh ", m, " didn't have colors. Generating an array of white colors"));
			}
			array = new Color[m.vertexCount];
			for (int i = 0; i < array.Length; i++)
			{
				ref Color reference = ref array[i];
				reference = Color.white;
			}
		}
		return array;
	}

	private Vector3[] _getMeshNormals(Mesh m)
	{
		Vector3[] array = m.normals;
		if (array.Length == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no normals. Generating"));
			}
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning(string.Concat("Mesh ", m, " didn't have normals. Generating normals."));
			}
			Mesh obj = (Mesh)UnityEngine.Object.Instantiate(m);
			obj.RecalculateNormals();
			array = obj.normals;
			MB_Utility.Destroy(obj);
		}
		return array;
	}

	private Vector4[] _getMeshTangents(Mesh m)
	{
		Vector4[] array = m.tangents;
		if (array.Length == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no tangents. Generating"));
			}
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning(string.Concat("Mesh ", m, " didn't have tangents. Generating tangents."));
			}
			Vector3[] vertices = m.vertices;
			Vector2[] array2 = _getMeshUVs(m);
			Vector3[] array3 = _getMeshNormals(m);
			array = new Vector4[m.vertexCount];
			for (int i = 0; i < m.subMeshCount; i++)
			{
				int[] triangles = m.GetTriangles(i);
				_generateTangents(triangles, vertices, array2, array3, array);
			}
		}
		return array;
	}

	private Vector2[] _getMeshUVs(Mesh m)
	{
		Vector2[] array = m.uv;
		if (array.Length == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no uvs. Generating"));
			}
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning(string.Concat("Mesh ", m, " didn't have uvs. Generating uvs."));
			}
			array = new Vector2[m.vertexCount];
			for (int i = 0; i < array.Length; i++)
			{
				ref Vector2 reference = ref array[i];
				reference = _HALF_UV;
			}
		}
		return array;
	}

	private Vector2[] _getMeshUV1s(Mesh m)
	{
		Vector2[] array = m.uv1;
		if (array.Length == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no uv1s. Generating"));
			}
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning(string.Concat("Mesh ", m, " didn't have uv1s. Generating uv1s."));
			}
			array = new Vector2[m.vertexCount];
			for (int i = 0; i < array.Length; i++)
			{
				ref Vector2 reference = ref array[i];
				reference = _HALF_UV;
			}
		}
		return array;
	}

	private Vector2[] _getMeshUV2s(Mesh m)
	{
		Vector2[] array = m.uv2;
		if (array.Length == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug(string.Concat("Mesh ", m, " has no uv2s. Generating"));
			}
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning(string.Concat("Mesh ", m, " didn't have uv2s. Lightmapping option was set to ", lightmapOption, " Generating uv2s."));
			}
			array = new Vector2[m.vertexCount];
			for (int i = 0; i < array.Length; i++)
			{
				ref Vector2 reference = ref array[i];
				reference = _HALF_UV;
			}
		}
		return array;
	}

	public void UpdateSkinnedMeshApproximateBounds()
	{
		UpdateSkinnedMeshApproximateBoundsFromBounds();
	}

	public void UpdateSkinnedMeshApproximateBoundsFromBones()
	{
		if (outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBounds when output type is bakeMeshAssetsInPlace");
			}
		}
		else if (bones.Length == 0)
		{
			if (verts.Length != 0 && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("No bones in SkinnedMeshRenderer. Could not UpdateSkinnedMeshApproximateBounds.");
			}
		}
		else if (targetRenderer == null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Target Renderer is not set. No point in calling UpdateSkinnedMeshApproximateBounds.");
			}
		}
		else if (!__targetRenderer.GetType().Equals(typeof(SkinnedMeshRenderer)))
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Target Renderer is not a SkinnedMeshRenderer. No point in calling UpdateSkinnedMeshApproximateBounds.");
			}
		}
		else
		{
			UpdateSkinnedMeshApproximateBoundsFromBonesStatic(bones, (SkinnedMeshRenderer)targetRenderer);
		}
	}

	public static void UpdateSkinnedMeshApproximateBoundsFromBonesStatic(Transform[] bs, SkinnedMeshRenderer smr)
	{
		Vector3 position = bs[0].position;
		Vector3 position2 = bs[0].position;
		for (int i = 1; i < bs.Length; i++)
		{
			Vector3 position3 = bs[i].position;
			if (position3.x < position2.x)
			{
				position2.x = position3.x;
			}
			if (position3.y < position2.y)
			{
				position2.y = position3.y;
			}
			if (position3.z < position2.z)
			{
				position2.z = position3.z;
			}
			if (position3.x > position.x)
			{
				position.x = position3.x;
			}
			if (position3.y > position.y)
			{
				position.y = position3.y;
			}
			if (position3.z > position.z)
			{
				position.z = position3.z;
			}
		}
		Vector3 vector = (position + position2) / 2f;
		Vector3 vector2 = position - position2;
		Matrix4x4 worldToLocalMatrix = smr.worldToLocalMatrix;
		Bounds localBounds = new Bounds(worldToLocalMatrix * vector, worldToLocalMatrix * vector2);
		smr.localBounds = localBounds;
	}

	public void UpdateSkinnedMeshApproximateBoundsFromBounds()
	{
		if (outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBoundsFromBounds when output type is bakeMeshAssetsInPlace");
			}
		}
		else if (verts.Length == 0 || objectsInCombinedMesh.Count == 0)
		{
			if (verts.Length != 0 && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Nothing in SkinnedMeshRenderer. Could not UpdateSkinnedMeshApproximateBoundsFromBounds.");
			}
		}
		else if (targetRenderer == null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Target Renderer is not set. No point in calling UpdateSkinnedMeshApproximateBoundsFromBounds.");
			}
		}
		else if (!__targetRenderer.GetType().Equals(typeof(SkinnedMeshRenderer)))
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Target Renderer is not a SkinnedMeshRenderer. No point in calling UpdateSkinnedMeshApproximateBoundsFromBounds.");
			}
		}
		else
		{
			UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(objectsInCombinedMesh, (SkinnedMeshRenderer)targetRenderer);
		}
	}

	public static void UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(List<GameObject> objectsInCombined, SkinnedMeshRenderer smr)
	{
		Bounds b = default(Bounds);
		Bounds bounds = default(Bounds);
		if (MB_Utility.GetBounds(objectsInCombined[0], out b))
		{
			bounds = b;
			for (int i = 1; i < objectsInCombined.Count; i++)
			{
				if (MB_Utility.GetBounds(objectsInCombined[i], out b))
				{
					bounds.Encapsulate(b);
					continue;
				}
				Debug.LogError("Could not get bounds. Not updating skinned mesh bounds");
				return;
			}
			smr.localBounds = bounds;
		}
		else
		{
			Debug.LogError("Could not get bounds. Not updating skinned mesh bounds");
		}
	}

	private int _getNumBones(Renderer r)
	{
		if (renderType == MB_RenderType.skinnedMeshRenderer)
		{
			if (r is SkinnedMeshRenderer)
			{
				return ((SkinnedMeshRenderer)r).bones.Length;
			}
			if (r is MeshRenderer)
			{
				return 1;
			}
			Debug.LogError("Could not _getNumBones. Object does not have a renderer");
			return 0;
		}
		return 0;
	}

	private Transform[] _getBones(Renderer r)
	{
		if (r is SkinnedMeshRenderer)
		{
			return ((SkinnedMeshRenderer)r).bones;
		}
		if (r is MeshRenderer)
		{
			return new Transform[1] { r.transform };
		}
		Debug.LogError("Could not getBones. Object does not have a renderer");
		return null;
	}

	private Matrix4x4[] _getBindPoses(Renderer r)
	{
		if (r is SkinnedMeshRenderer)
		{
			return ((SkinnedMeshRenderer)r).sharedMesh.bindposes;
		}
		if (r is MeshRenderer)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			return new Matrix4x4[1] { identity };
		}
		Debug.LogError("Could not _getBindPoses. Object does not have a renderer");
		return null;
	}

	private BoneWeight[] _getBoneWeights(Renderer r, int numVerts)
	{
		if (r is SkinnedMeshRenderer)
		{
			return ((SkinnedMeshRenderer)r).sharedMesh.boneWeights;
		}
		if (r is MeshRenderer)
		{
			BoneWeight boneWeight = default(BoneWeight);
			int num = (boneWeight.boneIndex3 = 0);
			int num3 = num;
			num = (boneWeight.boneIndex2 = num3);
			num3 = num;
			num = (boneWeight.boneIndex1 = num3);
			num3 = num;
			boneWeight.boneIndex0 = num3;
			boneWeight.weight0 = 1f;
			float num6 = (boneWeight.weight3 = 0f);
			float num8 = num6;
			num6 = (boneWeight.weight2 = num8);
			num8 = num6;
			boneWeight.weight1 = num8;
			BoneWeight[] array = new BoneWeight[numVerts];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = boneWeight;
			}
			return array;
		}
		Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
		return null;
	}

	public void Apply(GenerateUV2Delegate uv2GenerationMethod = null)
	{
		bool flag = false;
		if (renderType == MB_RenderType.skinnedMeshRenderer)
		{
			flag = true;
		}
		Apply(triangles: true, vertices: true, _doNorm, _doTan, _doUV, _doCol, _doUV1, doUV2(), flag, uv2GenerationMethod);
	}

	public void ApplyShowHide()
	{
		if (_mesh != null)
		{
			_mesh.Clear(keepVertexLayout: true);
			_mesh.vertices = verts;
			int[][] submeshTrisWithShowHideApplied = GetSubmeshTrisWithShowHideApplied();
			if (textureBakeResults.doMultiMaterial)
			{
				_mesh.subMeshCount = submeshTrisWithShowHideApplied.Length;
				for (int i = 0; i < submeshTrisWithShowHideApplied.Length; i++)
				{
					_mesh.SetTriangles(submeshTrisWithShowHideApplied[i], i);
				}
			}
			else
			{
				_mesh.triangles = submeshTrisWithShowHideApplied[0];
			}
		}
		else
		{
			Debug.LogError("Need to add objects to this meshbaker before calling ApplyShowHide");
		}
	}

	public void Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool colors, bool uv1, bool uv2, bool bones = false, GenerateUV2Delegate uv2GenerationMethod = null)
	{
		if (_mesh != null)
		{
			if (triangles || _mesh.vertexCount != verts.Length)
			{
				if (triangles && !vertices && !normals && !tangents && !uvs && !colors && !uv1 && !uv2 && !bones)
				{
					_mesh.Clear(keepVertexLayout: true);
				}
				else
				{
					_mesh.Clear(keepVertexLayout: false);
				}
			}
			if (vertices)
			{
				_mesh.vertices = verts;
			}
			if (triangles)
			{
				int[][] submeshTrisWithShowHideApplied = GetSubmeshTrisWithShowHideApplied();
				if (textureBakeResults.doMultiMaterial)
				{
					_mesh.subMeshCount = submeshTrisWithShowHideApplied.Length;
					for (int i = 0; i < submeshTrisWithShowHideApplied.Length; i++)
					{
						_mesh.SetTriangles(submeshTrisWithShowHideApplied[i], i);
					}
				}
				else
				{
					_mesh.triangles = submeshTrisWithShowHideApplied[0];
				}
			}
			if (normals)
			{
				if (_doNorm)
				{
					_mesh.normals = this.normals;
				}
				else
				{
					Debug.LogError("normal flag was set in Apply but MeshBaker didn't generate normals");
				}
			}
			if (tangents)
			{
				if (_doTan)
				{
					_mesh.tangents = this.tangents;
				}
				else
				{
					Debug.LogError("tangent flag was set in Apply but MeshBaker didn't generate tangents");
				}
			}
			if (uvs)
			{
				if (_doUV)
				{
					_mesh.uv = this.uvs;
				}
				else
				{
					Debug.LogError("uv flag was set in Apply but MeshBaker didn't generate uvs");
				}
			}
			if (colors)
			{
				if (_doCol)
				{
					_mesh.colors = this.colors;
				}
				else
				{
					Debug.LogError("color flag was set in Apply but MeshBaker didn't generate colors");
				}
			}
			if (uv1)
			{
				if (_doUV1)
				{
					_mesh.uv1 = uv1s;
				}
				else
				{
					Debug.LogError("uv1 flag was set in Apply but MeshBaker didn't generate uv1s");
				}
			}
			if (uv2)
			{
				if (doUV2())
				{
					_mesh.uv2 = uv2s;
				}
				else
				{
					Debug.LogError("uv2 flag was set in Apply but lightmapping option was set to " + lightmapOption);
				}
			}
			bool flag = false;
			if (renderType != MB_RenderType.skinnedMeshRenderer && lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
			{
				if (uv2GenerationMethod != null)
				{
					uv2GenerationMethod(_mesh);
				}
				else
				{
					Debug.LogError("No GenerateUV2Delegate method was supplied. UV2 cannot be generated.");
				}
				flag = true;
			}
			else if (renderType == MB_RenderType.skinnedMeshRenderer && lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("UV2 cannot be generated for SkinnedMeshRenderer objects.");
			}
			if (renderType != MB_RenderType.skinnedMeshRenderer && lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout && !flag)
			{
				Debug.LogError("Failed to generate new UV2 layout. Only works in editor.");
			}
			if (renderType == MB_RenderType.skinnedMeshRenderer)
			{
				if (verts.Length == 0)
				{
					targetRenderer.enabled = false;
				}
				else
				{
					targetRenderer.enabled = true;
				}
				bool updateWhenOffscreen = ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen;
				((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen = true;
				((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen = updateWhenOffscreen;
			}
			if (bones)
			{
				_mesh.bindposes = bindPoses;
				_mesh.boneWeights = boneWeights;
			}
			if (triangles || vertices)
			{
				_mesh.RecalculateBounds();
			}
		}
		else
		{
			Debug.LogError("Need to add objects to this meshbaker before calling Apply or ApplyAll");
		}
	}

	public int[][] GetSubmeshTrisWithShowHideApplied()
	{
		bool flag = false;
		for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
		{
			if (!mbDynamicObjectsInCombinedMesh[i].show)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			int[] array = new int[submeshTris.Length];
			int[][] array2 = new int[submeshTris.Length][];
			for (int j = 0; j < mbDynamicObjectsInCombinedMesh.Count; j++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[j];
				if (mB_DynamicGameObject.show)
				{
					for (int k = 0; k < mB_DynamicGameObject.submeshNumTris.Length; k++)
					{
						array[k] += mB_DynamicGameObject.submeshNumTris[k];
					}
				}
			}
			for (int l = 0; l < array2.Length; l++)
			{
				array2[l] = new int[array[l]];
			}
			int[] array3 = new int[array2.Length];
			for (int m = 0; m < mbDynamicObjectsInCombinedMesh.Count; m++)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[m];
				if (!mB_DynamicGameObject2.show)
				{
					continue;
				}
				for (int n = 0; n < submeshTris.Length; n++)
				{
					int[] array4 = submeshTris[n];
					int num = mB_DynamicGameObject2.submeshTriIdxs[n];
					int num2 = num + mB_DynamicGameObject2.submeshNumTris[n];
					for (int num3 = num; num3 < num2; num3++)
					{
						array2[n][array3[n]] = array4[num3];
						array3[n]++;
					}
				}
			}
			return array2;
		}
		return submeshTris;
	}

	public void UpdateGameObjects(GameObject[] gos, bool recalcBounds = true, bool updateVertices = true, bool updateNormals = true, bool updateTangents = true, bool updateUV = false, bool updateUV1 = false, bool updateUV2 = false, bool updateColors = false, bool updateSkinningInfo = false)
	{
		_updateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV1, updateUV2, updateColors, updateSkinningInfo);
	}

	private void _updateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV1, bool updateUV2, bool updateColors, bool updateSkinningInfo)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("UpdateGameObjects called on " + gos.Length + " objects.");
		}
		_initialize();
		for (int i = 0; i < gos.Length; i++)
		{
			_updateGameObject(gos[i], updateVertices, updateNormals, updateTangents, updateUV, updateUV1, updateUV2, updateColors, updateSkinningInfo);
		}
		if (recalcBounds)
		{
			_mesh.RecalculateBounds();
		}
	}

	private void _updateGameObject(GameObject go, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV1, bool updateUV2, bool updateColors, bool updateSkinningInfo)
	{
		MB_DynamicGameObject dgo = null;
		if (!instance2Combined_MapTryGetValue(go.GetInstanceID(), out dgo))
		{
			Debug.LogError("Object " + go.name + " has not been added");
			return;
		}
		Mesh mesh = MB_Utility.GetMesh(go);
		if (dgo.numVerts != mesh.vertexCount)
		{
			Debug.LogError("Object " + go.name + " source mesh has been modified since being added");
			return;
		}
		if (_doUV && updateUV)
		{
			_copyAndAdjustUVsFromMesh(dgo, mesh, dgo.vertIdx);
		}
		if (doUV2() && updateUV2)
		{
			_copyAndAdjustUV2FromMesh(dgo, mesh, dgo.vertIdx);
		}
		if (renderType == MB_RenderType.skinnedMeshRenderer && updateSkinningInfo)
		{
			_copyBonesBindPosesAndBoneWeightsFromMesh(go, dgo, dgo.vertIdx, dgo.bonesIdx);
		}
		if (updateVertices)
		{
			Matrix4x4 localToWorldMatrix = go.transform.localToWorldMatrix;
			Vector3[] vertices = mesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				ref Vector3 reference = ref verts[dgo.vertIdx + i];
				reference = localToWorldMatrix.MultiplyPoint3x4(vertices[i]);
			}
		}
		if (_doNorm && updateNormals)
		{
			Quaternion rotation = go.transform.rotation;
			Vector3[] array = _getMeshNormals(mesh);
			for (int j = 0; j < array.Length; j++)
			{
				ref Vector3 reference2 = ref normals[dgo.vertIdx + j];
				reference2 = rotation * array[j];
			}
		}
		if (_doTan && updateTangents)
		{
			Quaternion rotation2 = go.transform.rotation;
			Vector4[] array2 = _getMeshTangents(mesh);
			for (int k = 0; k < array2.Length; k++)
			{
				float w = array2[k].w;
				int num = dgo.vertIdx + k;
				ref Vector4 reference3 = ref tangents[num];
				reference3 = rotation2 * array2[k];
				tangents[num].w = w;
			}
		}
		if (_doCol && updateColors)
		{
			Color[] array3 = _getMeshColors(mesh);
			for (int l = 0; l < array3.Length; l++)
			{
				ref Color reference4 = ref colors[dgo.vertIdx + l];
				reference4 = array3[l];
			}
		}
		if (_doUV1 && updateUV1)
		{
			Vector2[] array4 = _getMeshUV1s(mesh);
			for (int m = 0; m < array4.Length; m++)
			{
				ref Vector2 reference5 = ref uv1s[dgo.vertIdx + m];
				reference5 = array4[m];
			}
		}
	}

	public bool ShowHideGameObjects(GameObject[] toShow, GameObject[] toHide)
	{
		return _showHide(toShow, toHide);
	}

	public Mesh AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs)
	{
		return AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource: true, textureBakeResults.fixOutOfBoundsUVs);
	}

	public Mesh AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		if (textureBakeResults == null)
		{
			Debug.LogError("Need to set textureBakeResults");
			return null;
		}
		return AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource, textureBakeResults.fixOutOfBoundsUVs);
	}

	public Mesh AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource, bool fixOutOfBoundUVs)
	{
		int[] array = null;
		if (deleteGOs != null)
		{
			array = new int[deleteGOs.Length];
			for (int i = 0; i < deleteGOs.Length; i++)
			{
				if (deleteGOs[i] == null)
				{
					Debug.LogError("The " + i + "th object on the list of objects to delete is 'Null'");
				}
				else
				{
					array[i] = deleteGOs[i].GetInstanceID();
				}
			}
		}
		return AddDeleteGameObjectsByID(gos, array, disableRendererInSource, fixOutOfBoundUVs);
	}

	public Mesh AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource, bool fixOutOfBoundUVs)
	{
		if (validationLevel > MB2_ValidationLevel.none)
		{
			if (gos != null)
			{
				for (int i = 0; i < gos.Length; i++)
				{
					if (gos[i] == null)
					{
						Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
						return null;
					}
					if (validationLevel < MB2_ValidationLevel.robust)
					{
						continue;
					}
					for (int j = i + 1; j < gos.Length; j++)
					{
						if (gos[i] == gos[j])
						{
							Debug.LogError(string.Concat("GameObject ", gos[i], "appears twice in list of game objects to add"));
							return null;
						}
					}
				}
			}
			if (deleteGOinstanceIDs != null && validationLevel >= MB2_ValidationLevel.robust)
			{
				for (int k = 0; k < deleteGOinstanceIDs.Length; k++)
				{
					for (int l = k + 1; l < deleteGOinstanceIDs.Length; l++)
					{
						if (deleteGOinstanceIDs[k] == deleteGOinstanceIDs[l])
						{
							Debug.LogError("GameObject " + deleteGOinstanceIDs[k] + "appears twice in list of game objects to delete");
							return null;
						}
					}
				}
			}
		}
		if (!_addToCombined(gos, deleteGOinstanceIDs, disableRendererInSource))
		{
			Debug.LogError("Failed to add/delete objects to combined mesh");
			return null;
		}
		if (targetRenderer != null && outputOption != MB2_OutputOptions.bakeMeshAssetsInPlace)
		{
			if (renderType == MB_RenderType.skinnedMeshRenderer)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Setting bones in renderer");
				}
				((SkinnedMeshRenderer)targetRenderer).bones = bones;
				UpdateSkinnedMeshApproximateBoundsFromBounds();
			}
			targetRenderer.lightmapIndex = GetLightmapIndex();
		}
		return _mesh;
	}

	public bool CombinedMeshContains(GameObject go)
	{
		return objectsInCombinedMesh.Contains(go);
	}

	private void _clearArrays()
	{
		verts = new Vector3[0];
		normals = new Vector3[0];
		tangents = new Vector4[0];
		uvs = new Vector2[0];
		uv1s = new Vector2[0];
		uv2s = new Vector2[0];
		colors = new Color[0];
		bones = new Transform[0];
		bindPoses = new Matrix4x4[0];
		boneWeights = new BoneWeight[0];
		submeshTris = new int[0][];
		mbDynamicObjectsInCombinedMesh.Clear();
		objectsInCombinedMesh.Clear();
		instance2Combined_MapClear();
	}

	public void ClearMesh()
	{
		if (_mesh != null)
		{
			_mesh.Clear(keepVertexLayout: false);
		}
		else
		{
			_mesh = new Mesh();
		}
		_clearArrays();
	}

	public void DestroyMesh()
	{
		if (_mesh != null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Destroying Mesh");
			}
			MB_Utility.Destroy(_mesh);
		}
		_mesh = new Mesh();
		_clearArrays();
	}

	public void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
	{
		if (_mesh != null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Destroying Mesh");
			}
			editorMethods.Destroy(_mesh);
		}
		_mesh = new Mesh();
		_clearArrays();
	}

	private void _generateTangents(int[] triangles, Vector3[] verts, Vector2[] uvs, Vector3[] normals, Vector4[] outTangents)
	{
		int num = triangles.Length;
		int num2 = verts.Length;
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		for (int i = 0; i < num; i += 3)
		{
			int num3 = triangles[i];
			int num4 = triangles[i + 1];
			int num5 = triangles[i + 2];
			Vector3 vector = verts[num3];
			Vector3 vector2 = verts[num4];
			Vector3 vector3 = verts[num5];
			Vector2 vector4 = uvs[num3];
			Vector2 vector5 = uvs[num4];
			Vector2 vector6 = uvs[num5];
			float num6 = vector2.x - vector.x;
			float num7 = vector3.x - vector.x;
			float num8 = vector2.y - vector.y;
			float num9 = vector3.y - vector.y;
			float num10 = vector2.z - vector.z;
			float num11 = vector3.z - vector.z;
			float num12 = vector5.x - vector4.x;
			float num13 = vector6.x - vector4.x;
			float num14 = vector5.y - vector4.y;
			float num15 = vector6.y - vector4.y;
			float num16 = num12 * num15 - num13 * num14;
			if (num16 == 0f)
			{
				Debug.LogError("Could not compute tangents. All UVs need to form a valid triangles in UV space. If any UV triangles are collapsed, tangents cannot be generated.");
				return;
			}
			float num17 = 1f / num16;
			Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num17, (num15 * num8 - num14 * num9) * num17, (num15 * num10 - num14 * num11) * num17);
			Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num17, (num12 * num9 - num13 * num8) * num17, (num12 * num11 - num13 * num10) * num17);
			array[num3] += vector7;
			array[num4] += vector7;
			array[num5] += vector7;
			array2[num3] += vector8;
			array2[num4] += vector8;
			array2[num5] += vector8;
		}
		for (int j = 0; j < num2; j++)
		{
			Vector3 vector9 = normals[j];
			Vector3 vector10 = array[j];
			Vector3 normalized = (vector10 - vector9 * Vector3.Dot(vector9, vector10)).normalized;
			ref Vector4 reference = ref outTangents[j];
			reference = new Vector4(normalized.x, normalized.y, normalized.z);
			outTangents[j].w = ((!(Vector3.Dot(Vector3.Cross(vector9, vector10), array2[j]) < 0f)) ? 1f : (-1f));
		}
	}

	public GameObject buildSceneMeshObject(GameObject root, Mesh m, bool createNewChild = false)
	{
		MeshFilter meshFilter = null;
		MeshRenderer meshRenderer = null;
		SkinnedMeshRenderer skinnedMeshRenderer = null;
		Transform transform = null;
		if (!createNewChild)
		{
			foreach (Transform item in root.transform)
			{
				if (item.name.EndsWith("-mesh"))
				{
					transform = item;
					break;
				}
			}
		}
		GameObject gameObject = ((!(transform == null)) ? transform.gameObject : new GameObject(name + "-mesh"));
		if (renderType == MB_RenderType.skinnedMeshRenderer)
		{
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			if (component != null)
			{
				MB_Utility.Destroy(component);
			}
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			if (component2 != null)
			{
				MB_Utility.Destroy(component2);
			}
			skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (skinnedMeshRenderer == null)
			{
				skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
			}
		}
		else
		{
			SkinnedMeshRenderer component3 = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component3 != null)
			{
				MB_Utility.Destroy(component3);
			}
			meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
		}
		if (textureBakeResults.doMultiMaterial)
		{
			Material[] array = new Material[textureBakeResults.resultMaterials.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = textureBakeResults.resultMaterials[i].combinedMaterial;
			}
			if (renderType == MB_RenderType.skinnedMeshRenderer)
			{
				skinnedMeshRenderer.sharedMaterial = null;
				skinnedMeshRenderer.sharedMaterials = array;
				skinnedMeshRenderer.bones = GetBones();
				skinnedMeshRenderer.updateWhenOffscreen = true;
				skinnedMeshRenderer.updateWhenOffscreen = false;
			}
			else
			{
				meshRenderer.sharedMaterial = null;
				meshRenderer.sharedMaterials = array;
			}
		}
		else if (renderType == MB_RenderType.skinnedMeshRenderer)
		{
			skinnedMeshRenderer.sharedMaterials = new Material[1] { textureBakeResults.resultMaterial };
			skinnedMeshRenderer.sharedMaterial = textureBakeResults.resultMaterial;
			skinnedMeshRenderer.bones = GetBones();
		}
		else
		{
			meshRenderer.sharedMaterials = new Material[1] { textureBakeResults.resultMaterial };
			meshRenderer.sharedMaterial = textureBakeResults.resultMaterial;
		}
		if (renderType == MB_RenderType.skinnedMeshRenderer)
		{
			skinnedMeshRenderer.sharedMesh = m;
			skinnedMeshRenderer.lightmapIndex = GetLightmapIndex();
		}
		else
		{
			meshFilter.sharedMesh = m;
			meshRenderer.lightmapIndex = GetLightmapIndex();
		}
		if (lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping || lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
		{
			gameObject.isStatic = true;
		}
		List<GameObject> objectsInCombined = GetObjectsInCombined();
		if (objectsInCombined.Count > 0 && objectsInCombined[0] != null)
		{
			bool flag = true;
			bool flag2 = true;
			string tag = objectsInCombined[0].tag;
			int layer = objectsInCombined[0].layer;
			for (int j = 0; j < objectsInCombined.Count; j++)
			{
				if (objectsInCombined[j] != null)
				{
					if (!objectsInCombined[j].tag.Equals(tag))
					{
						flag = false;
					}
					if (objectsInCombined[j].layer != layer)
					{
						flag2 = false;
					}
				}
			}
			if (flag)
			{
				root.tag = tag;
				gameObject.tag = tag;
			}
			if (flag2)
			{
				root.layer = layer;
				gameObject.layer = layer;
			}
		}
		gameObject.transform.parent = root.transform;
		return gameObject;
	}
}
