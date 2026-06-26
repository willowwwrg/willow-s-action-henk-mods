using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelFileLoader : Singleton<LevelFileLoader>
{
	public bool levelLoaded;

	public List<GameObject> buildingBlocks;

	[HideInInspector]
	public GameObject parentLevelBlocks;

	[HideInInspector]
	public GameObject parentEntities;

	[HideInInspector]
	public GameObject parentLevelSupport;

	[HideInInspector]
	public GameObject parentLevelCurve;

	[HideInInspector]
	public GameObject parentCheckpoints;

	[HideInInspector]
	public GameObject parentLMPCheckpoints;

	private Material[] plasticMaterials;

	private int totalVertPlastic;

	private List<CombineInstance> allPlasticCurves = new List<CombineInstance>();

	private Material[] woodMaterials;

	private int totalVertWood;

	private List<CombineInstance> allWood = new List<CombineInstance>();

	private Material[] pillarMaterials;

	private int totalVertPillar;

	private List<CombineInstance> allPillars = new List<CombineInstance>();

	private bool combineMeshes = true;

	private void Awake()
	{
		buildingBlocks = Singleton<LevelEditorManager>.SP.buildingBlocks;
		parentLevelBlocks = null;
		parentEntities = null;
		parentLevelSupport = null;
		parentLevelCurve = null;
		parentCheckpoints = null;
		parentLMPCheckpoints = null;
	}

	private void InitParentObjects()
	{
		parentLevelBlocks = GameObject.Find("LevelBlocks");
		if (parentLevelBlocks == null)
		{
			parentLevelBlocks = new GameObject();
			parentLevelBlocks.name = "LevelBlocks";
		}
		parentLevelSupport = GameObject.Find("LevelSupport");
		if (parentLevelSupport == null)
		{
			parentLevelSupport = new GameObject();
			parentLevelSupport.name = "LevelSupport";
		}
		parentEntities = GameObject.Find("Entities");
		if (parentEntities == null)
		{
			parentEntities = new GameObject();
			parentEntities.name = "Entities";
		}
		parentLevelCurve = GameObject.Find("LevelCurve");
		if (parentLevelCurve == null)
		{
			parentLevelCurve = new GameObject();
			parentLevelCurve.name = "LevelCurve";
			parentLevelCurve.AddComponent<GameSpline>();
		}
		parentCheckpoints = GameObject.Find("StartFinishCheckpoints");
		if (parentCheckpoints == null)
		{
			parentCheckpoints = new GameObject();
			parentCheckpoints.name = "StartFinishCheckpoints";
			parentCheckpoints.AddComponent<CheckpointManager>();
		}
		parentLMPCheckpoints = GameObject.Find("LMPCheckpoints");
		if (parentLMPCheckpoints == null)
		{
			parentLMPCheckpoints = new GameObject();
			parentLMPCheckpoints.name = "LMPCheckpoints";
		}
	}

	public bool LoadUGCLevelFile(Level levelObj)
	{
		string data = string.Empty;
		string text;
		if (levelObj.levelName == "NewLevel")
		{
			int levelStyle = (int)levelObj.levelStyle;
			text = "Levels_Exported/LEVEL-999-" + levelStyle;
			TextAsset textAsset = (TextAsset)Resources.Load(text);
			if (textAsset == null)
			{
				Debug.LogError("Trying to read nonexisting level file: " + text);
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOLEVELFILE", "PERMA") + text);
				return false;
			}
			data = textAsset.text;
		}
		else
		{
			text = ((levelObj.levelType != LevelType.Workshop) ? (Application.dataPath + "/Resources/../../CustomLevels/" + levelObj.guid.ToString() + "/" + levelObj.guid.ToString() + ".txt") : (levelObj.workshopFolderName + "/" + levelObj.guid.ToString() + ".txt"));
			if (!File.Exists(text))
			{
				Debug.LogError("Trying to read nonexisting level file: " + text);
				Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOLEVELFILE", "PERMA") + " " + text);
				return false;
			}
			using StreamReader streamReader = new StreamReader(text);
			data = streamReader.ReadToEnd();
		}
		return ReadLevelFile(data, text);
	}

	public bool Load()
	{
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelCode == -1)
		{
			return LoadUGCLevelFile(Singleton<LevelBatchManager>.SP.currentLevel);
		}
		string text = "Levels_Exported/LEVEL-" + Singleton<LevelBatchManager>.SP.currentLevel.levelCode;
		TextAsset textAsset = (TextAsset)Resources.Load(text);
		if (textAsset == null)
		{
			Debug.LogError("Trying to read nonexisting level file: " + text);
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_NOLEVELFILE", "PERMA") + " " + text);
			return false;
		}
		return ReadLevelFile(textAsset.text, text);
	}

	private bool ReadLevelFile(string data, string fileToLoad)
	{
		Debug.Log("Loading level " + fileToLoad);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		using (StringReader stringReader = new StringReader(data))
		{
			string empty = string.Empty;
			empty = stringReader.ReadLine();
			if (empty != "LE" && Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
			{
				Singleton<LevelEditorFileWriter>.SP.levelName = empty;
				Singleton<Workshop>.SP.CheckIfCurrentLevelExistsInWorkshop();
			}
			empty = stringReader.ReadLine();
			empty = stringReader.ReadLine();
			if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
			{
				if (Singleton<LevelBatchManager>.SP.currentLevel.levelName == "NewLevel")
				{
					Singleton<Foreman>.SP.StylizeLevel(Singleton<LevelBatchManager>.SP.currentLevel.levelStyle);
				}
				else
				{
					Singleton<Foreman>.SP.StylizeLevel((LevelStyle)HenkUtils.IntParse(empty));
				}
			}
			else
			{
				Singleton<Foreman>.SP.StylizeLevel((LevelStyle)HenkUtils.IntParse(empty));
			}
			InitParentObjects();
			allPlasticCurves.Clear();
			allWood.Clear();
			allPillars.Clear();
			totalVertWood = 0;
			totalVertPlastic = 0;
			totalVertPillar = 0;
			woodMaterials = null;
			plasticMaterials = null;
			pillarMaterials = null;
			empty = stringReader.ReadLine();
			int num = 0;
			if (empty != "C")
			{
				num = HenkUtils.IntParse(empty);
				if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop)
				{
					Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelVersion = num;
				}
				empty = stringReader.ReadLine();
			}
			do
			{
				empty = stringReader.ReadLine();
				if (empty == null || empty == string.Empty)
				{
					Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_READINGFILE", "PERMA") + fileToLoad);
					return false;
				}
				if (empty == "B")
				{
					break;
				}
				string[] array = empty.Split(',');
				Vector2 vector = new Vector2(HenkUtils.FloatParse(array[0]), HenkUtils.FloatParse(array[1]));
				GameObject obj = new GameObject();
				obj.name = "CV";
				obj.AddComponent<GameSplineCV>();
				obj.transform.parent = parentLevelCurve.transform;
				obj.transform.localPosition = vector;
			}
			while (empty != null && empty != string.Empty);
			parentLevelCurve.GetComponent<GameSpline>().RebuildSpline();
			bool flag = false;
			bool flag2 = false;
			do
			{
				empty = stringReader.ReadLine();
				if (empty == null || empty == string.Empty)
				{
					Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_READINGFILE", "PERMA") + fileToLoad);
					return false;
				}
				switch (empty)
				{
				case "S":
					flag = true;
					goto IL_069a;
				case "E":
					flag2 = true;
					goto IL_069a;
				default:
				{
					string[] array2 = empty.Split(',');
					int blockID = HenkUtils.IntParse(array2[0]);
					string text = array2[1];
					Vector3 offset = default(Vector3);
					bool flag3 = true;
					if (text != "M")
					{
						offset = new Vector3(HenkUtils.IntParse(array2[2]), HenkUtils.IntParse(array2[3]), HenkUtils.IntParse(array2[4]));
						flag3 = array2[5] == "1";
					}
					GameObject gameObject;
					switch (text)
					{
					case "D":
					{
						Vector2 scale4 = new Vector2(HenkUtils.FloatParse(array2[6]), HenkUtils.FloatParse(array2[7]));
						float rotation = HenkUtils.IntParse(array2[8]);
						gameObject = CreateSplineObjectDeform(blockID, offset, scale4, rotation);
						if (parentLevelBlocks != null)
						{
							gameObject.transform.parent = parentLevelBlocks.transform;
						}
						if (flag2)
						{
							gameObject.transform.parent = parentEntities.transform;
						}
						if (flag)
						{
							gameObject.transform.parent = parentLevelSupport.transform;
						}
						break;
					}
					case "F":
					{
						Vector3 scale2 = new Vector3(HenkUtils.FloatParse(array2[6]), HenkUtils.FloatParse(array2[7]), HenkUtils.FloatParse(array2[8]));
						Vector3 rot2 = new Vector3(HenkUtils.FloatParse(array2[9]), 0f, HenkUtils.FloatParse(array2[10]));
						gameObject = CreateSplineObjectFollow(blockID, offset, scale2, rot2);
						if (parentLevelBlocks != null)
						{
							gameObject.transform.parent = parentLevelBlocks.transform;
						}
						if (flag2)
						{
							gameObject.transform.parent = parentEntities.transform;
							if (array2.Length > 11 && gameObject.GetComponent<SimpleTrigger>() != null)
							{
								gameObject.GetComponent<SimpleTrigger>().targetString = array2[11];
							}
						}
						if (flag)
						{
							gameObject.transform.parent = parentLevelSupport.transform;
						}
						break;
					}
					case "R":
					{
						Vector3 scale3 = new Vector3(HenkUtils.FloatParse(array2[6]), HenkUtils.FloatParse(array2[7]), HenkUtils.FloatParse(array2[8]));
						Vector3 rot3 = new Vector3(HenkUtils.FloatParse(array2[9]), HenkUtils.FloatParse(array2[11]), HenkUtils.FloatParse(array2[10]));
						gameObject = CreateSplineObjectFollowNoRotate(blockID, offset, scale3, rot3);
						if (parentLevelBlocks != null)
						{
							gameObject.transform.parent = parentLevelBlocks.transform;
						}
						if (flag2)
						{
							gameObject.transform.parent = parentEntities.transform;
						}
						if (flag)
						{
							gameObject.transform.parent = parentLevelSupport.transform;
						}
						break;
					}
					case "M":
					{
						Vector3 pos = new Vector3(HenkUtils.FloatParse(array2[2]), HenkUtils.FloatParse(array2[3]), HenkUtils.FloatParse(array2[4]));
						Vector3 rot = new Vector3(HenkUtils.FloatParse(array2[5]), HenkUtils.FloatParse(array2[6]), HenkUtils.FloatParse(array2[7]));
						Vector3 scale = new Vector3(HenkUtils.FloatParse(array2[8]), HenkUtils.FloatParse(array2[9]), HenkUtils.FloatParse(array2[10]));
						gameObject = CreateMiscObject(blockID, pos, rot, scale);
						if (flag2)
						{
							gameObject.transform.parent = parentEntities.transform;
						}
						break;
					}
					default:
						Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_READINGFILE", "PERMA") + fileToLoad);
						return false;
					}
					if ((bool)gameObject.renderer)
					{
						gameObject.renderer.enabled = flag3;
					}
					CheckCombine(gameObject);
					if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
					{
						gameObject.AddComponent<SelectedItem>();
					}
					goto IL_069a;
				}
				case "SP":
					break;
					IL_069a:
					if (empty == null)
					{
						break;
					}
					continue;
				}
				break;
			}
			while (empty != string.Empty);
			do
			{
				empty = stringReader.ReadLine();
				if (empty == null || empty == string.Empty || empty == "CP")
				{
					break;
				}
				string[] array3 = empty.Split(',');
				string text2 = array3[0];
				Vector3 offset2 = new Vector3(HenkUtils.IntParse(array3[1]), HenkUtils.IntParse(array3[2]), HenkUtils.IntParse(array3[3]));
				GameObject gameObject2 = null;
				if (text2 == "P")
				{
					bool doublePillar = HenkUtils.BoolParse(array3[4]);
					int skipObjectCount = HenkUtils.IntParse(array3[5]);
					float x = HenkUtils.FloatParse(array3[6]);
					float y = HenkUtils.FloatParse(array3[7]);
					bool mirrored = HenkUtils.BoolParse(array3[8]);
					bool bottom = true;
					if (array3.Length > 9)
					{
						bottom = HenkUtils.BoolParse(array3[9]);
					}
					gameObject2 = CreatePlasticPillar(offset2, doublePillar, new Vector2(x, y), skipObjectCount, mirrored, bottom);
					if (parentLevelBlocks != null)
					{
						gameObject2.transform.parent = parentLevelSupport.transform;
					}
				}
				else if (text2 == "W")
				{
					gameObject2 = CreateWoodPillar(offset2);
					if (parentLevelBlocks != null)
					{
						gameObject2.transform.parent = parentLevelSupport.transform;
					}
				}
				CheckCombine(gameObject2);
			}
			while (empty != null && empty != string.Empty);
			bool flag4 = false;
			List<Checkpoint> list = new List<Checkpoint>();
			do
			{
				empty = stringReader.ReadLine();
				if (empty == null || empty == string.Empty)
				{
					break;
				}
				if (empty.StartsWith("PFID"))
				{
					Singleton<LevelBatchManager>.SP.currentLevel.SetPublishedFileID(empty);
					break;
				}
				if (empty == "LMP_CP")
				{
					flag4 = true;
					break;
				}
				string[] array4 = empty.Split(',');
				string text3 = array4[0];
				Vector3 offset3 = new Vector3(HenkUtils.IntParse(array4[1]), HenkUtils.IntParse(array4[2]), HenkUtils.IntParse(array4[3]));
				GameObject gameObject3;
				switch (text3)
				{
				case "S":
					gameObject3 = CreateCheckpoint(offset3, plastic: false, start: true);
					parentCheckpoints.GetComponent<CheckpointManager>().Startline = gameObject3.GetComponent<Checkpoint>();
					break;
				case "F":
					gameObject3 = CreateCheckpoint(offset3, plastic: false, start: false, finish: true);
					if (array4.Length > 4)
					{
						gameObject3.transform.localScale = new Vector3(HenkUtils.FloatParse(array4[4]), 1f, 1f);
					}
					parentCheckpoints.GetComponent<CheckpointManager>().Finishline = gameObject3.GetComponent<Checkpoint>();
					break;
				case "C":
				case "C1":
				{
					bool plastic = text3 == "C1";
					gameObject3 = CreateCheckpoint(offset3, plastic);
					if (array4.Length > 5)
					{
						gameObject3.transform.localScale = new Vector3(HenkUtils.FloatParse(array4[5]), 1f, 1f);
					}
					int num2 = HenkUtils.IntParse(array4[4]);
					BoxCollider component = gameObject3.GetComponent<BoxCollider>();
					component.size = new Vector3(component.size.x, num2, component.size.z);
					component.center = new Vector3(component.center.x, num2 / 2, component.center.z);
					list.Add(gameObject3.GetComponent<Checkpoint>());
					break;
				}
				default:
					Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_READINGFILE", "PERMA") + fileToLoad);
					return false;
				}
				if (parentCheckpoints != null)
				{
					gameObject3.transform.parent = parentCheckpoints.transform;
				}
				if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor)
				{
					gameObject3.AddComponent<SelectedItem>();
				}
			}
			while (empty != null && empty != string.Empty);
			parentCheckpoints.GetComponent<CheckpointManager>().Checkpoints = list.ToArray();
			Singleton<CheckpointManager>.SP.Initialize();
			if (flag4)
			{
				GameObject gameObject4 = null;
				int num3 = 0;
				do
				{
					empty = stringReader.ReadLine();
					if (empty == null || empty == string.Empty)
					{
						break;
					}
					string[] array5 = empty.Split(',');
					Vector3 offset4 = new Vector3(HenkUtils.IntParse(array5[0]), HenkUtils.IntParse(array5[1]), -1f);
					GameObject gameObject5 = CreateLMPCheckpoint(offset4, num3);
					if (gameObject4 != null)
					{
						gameObject4.GetComponent<LMPCheckpoint>().next = gameObject5.GetComponent<LMPCheckpoint>();
					}
					gameObject4 = gameObject5;
					num3++;
				}
				while (empty != null && empty != string.Empty);
			}
		}
		levelLoaded = true;
		CombineMeshes();
		float loadTime = Time.realtimeSinceStartup - realtimeSinceStartup;
		Singleton<GAManager>.SP.LevelLoadTime(Singleton<LevelBatchManager>.SP.GetCurrentLevel(), loadTime);
		Debug.Log("Level loaded in " + loadTime);
		return true;
	}

	private GameObject CreateCheckpoint(Vector3 offset, bool plastic = false, bool start = false, bool finish = false)
	{
		GameObject gameObject;
		if (start)
		{
			gameObject = ((Singleton<LevelBatchManager>.SP.currentLevel.levelType != LevelType.Challenge) ? (Object.Instantiate(Singleton<LevelEditorManager>.SP.startLine) as GameObject) : (Object.Instantiate(Singleton<LevelEditorManager>.SP.startLineDouble) as GameObject));
			gameObject.name = Singleton<LevelEditorManager>.SP.startLine.name;
		}
		else if (finish)
		{
			gameObject = ((Singleton<LevelBatchManager>.SP.currentLevel.levelType != LevelType.Challenge) ? (Object.Instantiate(Singleton<LevelEditorManager>.SP.finishLine) as GameObject) : (Object.Instantiate(Singleton<LevelEditorManager>.SP.finishLineDouble) as GameObject));
			gameObject.name = Singleton<LevelEditorManager>.SP.finishLine.name;
		}
		else
		{
			gameObject = ((!plastic) ? (Object.Instantiate(Singleton<LevelEditorManager>.SP.checkpoint) as GameObject) : (Object.Instantiate(Singleton<LevelEditorManager>.SP.plasticCheckpoint) as GameObject));
			gameObject.name = Singleton<LevelEditorManager>.SP.checkpoint.name;
		}
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(gameObject);
		SplineFollow component = gameObject.GetComponent<SplineFollow>();
		component.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		component.splineOffset = offset;
		component.createdAtRuntime = true;
		component.Update();
		component.createdAtRuntime = false;
		return gameObject;
	}

	private GameObject CreateLMPCheckpoint(Vector3 offset, int number)
	{
		GameObject gameObject = new GameObject("lmpcheckpoint" + number);
		SplineFollow splineFollow = gameObject.AddComponent<SplineFollow>();
		splineFollow.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		splineFollow.splineOffset = offset;
		splineFollow.ForceOneUpdate();
		gameObject.AddComponent<LMPCheckpoint>().firstCheckpoint = number == 0;
		if (parentLMPCheckpoints != null)
		{
			gameObject.transform.parent = parentLMPCheckpoints.transform;
		}
		return gameObject;
	}

	private GameObject CreateSplineObjectFollow(int blockID, Vector3 offset, Vector3 scale, Vector3 rot)
	{
		GameObject gameObject = Object.Instantiate(buildingBlocks[blockID]) as GameObject;
		gameObject.name = buildingBlocks[blockID].name;
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(gameObject);
		SplineFollow splineFollow = gameObject.GetComponent<SplineFollow>();
		if (!splineFollow)
		{
			splineFollow = gameObject.AddComponent<SplineFollow>();
		}
		splineFollow.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		splineFollow.splineOffset = offset;
		splineFollow.transform.localScale = scale;
		splineFollow.transform.rotation = Quaternion.Euler(rot);
		splineFollow.ForceOneUpdate();
		return gameObject;
	}

	private GameObject CreateMiscObject(int blockID, Vector3 pos, Vector3 rot, Vector3 scale)
	{
		GameObject obj = Object.Instantiate(buildingBlocks[blockID]) as GameObject;
		obj.name = buildingBlocks[blockID].name;
		obj.transform.localPosition = pos;
		obj.transform.rotation = Quaternion.Euler(rot);
		obj.transform.localScale = scale;
		return obj;
	}

	private GameObject CreateSplineObjectFollowNoRotate(int blockID, Vector3 offset, Vector3 scale, Vector3 rot)
	{
		GameObject obj = Object.Instantiate(buildingBlocks[blockID]) as GameObject;
		obj.name = buildingBlocks[blockID].name;
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(obj);
		SplineFollowNoRotate splineFollowNoRotate = obj.AddComponent<SplineFollowNoRotate>();
		splineFollowNoRotate.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		splineFollowNoRotate.splineOffset = offset;
		splineFollowNoRotate.transform.localScale = scale;
		splineFollowNoRotate.transform.rotation = Quaternion.Euler(rot);
		splineFollowNoRotate.createdAtRuntime = true;
		splineFollowNoRotate.Update();
		splineFollowNoRotate.createdAtRuntime = false;
		return obj;
	}

	private GameObject CreateSplineObjectDeform(int blockID, Vector3 offset, Vector2 scale, float rotation)
	{
		if (Singleton<LevelEditorManager>.SP.buildingBlocksLowPoly.Count > blockID)
		{
			_ = (bool)Singleton<LevelEditorManager>.SP.buildingBlocksLowPoly[blockID];
		}
		GameObject gameObject = buildingBlocks[blockID];
		GameObject obj = Object.Instantiate(gameObject) as GameObject;
		obj.name = gameObject.name;
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(obj);
		SplineDeform splineDeform = obj.AddComponent<SplineDeform>();
		splineDeform.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		splineDeform.splineOffset = offset;
		splineDeform.xScale = scale.x;
		splineDeform.yScale = scale.y;
		splineDeform.rotation = rotation;
		splineDeform.originalMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		if (gameObject.GetComponent<MeshCollider>() == null)
		{
			splineDeform.originalCollider = gameObject.GetComponent<MeshFilter>().sharedMesh;
		}
		else
		{
			splineDeform.originalCollider = gameObject.GetComponent<MeshCollider>().sharedMesh;
		}
		splineDeform.createdAtRuntime = true;
		splineDeform.Update();
		splineDeform.createdAtRuntime = false;
		return obj;
	}

	private GameObject CreateWoodPillar(Vector3 offset)
	{
		GameObject obj = Object.Instantiate(Singleton<LevelEditorManager>.SP.woodPillar) as GameObject;
		obj.name = Singleton<LevelEditorManager>.SP.woodPillar.name;
		HenkUtils.InitializeMaterialsAccordingToEnvironmentStyle(obj);
		WoodSupport component = obj.GetComponent<WoodSupport>();
		component.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		component.splineOffset = offset;
		component.createdAtRuntime = true;
		component.UpdateSupport();
		component.createdAtRuntime = false;
		return obj;
	}

	private GameObject CreatePlasticPillar(Vector3 offset, bool doublePillar, Vector2 manualOffset, int skipObjectCount, bool mirrored, bool bottom)
	{
		GameObject obj = Object.Instantiate(Singleton<LevelEditorManager>.SP.plasticPillar) as GameObject;
		obj.name = Singleton<LevelEditorManager>.SP.plasticPillar.name;
		PlasticSupport component = obj.GetComponent<PlasticSupport>();
		component.spline = (GameSpline)Object.FindObjectOfType(typeof(GameSpline));
		component.splineOffset = offset;
		component.manualHeightOffset = manualOffset.x;
		component.manualRotationtOffset = manualOffset.y;
		component.mirrored = mirrored;
		component.skipObjects = skipObjectCount;
		component.doublePillar = doublePillar;
		component.noBottom = !bottom;
		component.createdAtRuntime = true;
		component.UpdateSupport();
		component.createdAtRuntime = false;
		return obj;
	}

	private void CheckCombine(GameObject newBlock)
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LevelEditor || !combineMeshes || newBlock == null || newBlock.renderer == null)
		{
			return;
		}
		bool num = newBlock.renderer.sharedMaterials[0].name.StartsWith("curve_blocks");
		bool flag = newBlock.renderer.sharedMaterials[0].name.StartsWith("wood_dif");
		bool flag2 = newBlock.GetComponent<PlasticSupport>() != null && !newBlock.GetComponent<PlasticSupport>().doublePillar;
		if (num && newBlock.renderer.enabled)
		{
			totalVertPlastic += newBlock.GetComponent<MeshFilter>().mesh.vertexCount;
			if (totalVertPlastic < 65000)
			{
				CombineInstance item = new CombineInstance
				{
					mesh = newBlock.GetComponent<MeshFilter>().mesh,
					transform = newBlock.transform.localToWorldMatrix
				};
				allPlasticCurves.Add(item);
				newBlock.renderer.enabled = false;
				if (plasticMaterials == null)
				{
					plasticMaterials = newBlock.renderer.sharedMaterials;
				}
			}
		}
		if (flag && newBlock.renderer.enabled)
		{
			totalVertWood += newBlock.GetComponent<MeshFilter>().mesh.vertexCount;
			if (totalVertWood < 65000)
			{
				CombineInstance item2 = new CombineInstance
				{
					mesh = newBlock.GetComponent<MeshFilter>().mesh,
					transform = newBlock.transform.localToWorldMatrix
				};
				allWood.Add(item2);
				newBlock.renderer.enabled = false;
				if (woodMaterials == null)
				{
					woodMaterials = newBlock.renderer.sharedMaterials;
				}
			}
		}
		if (!flag2 || !newBlock.renderer.enabled)
		{
			return;
		}
		totalVertPillar += newBlock.GetComponent<MeshFilter>().mesh.vertexCount;
		if (totalVertPillar < 65000)
		{
			CombineInstance item3 = new CombineInstance
			{
				mesh = newBlock.GetComponent<MeshFilter>().mesh,
				transform = newBlock.transform.localToWorldMatrix
			};
			allPillars.Add(item3);
			newBlock.renderer.enabled = false;
			if (pillarMaterials == null)
			{
				pillarMaterials = newBlock.renderer.sharedMaterials;
			}
		}
	}

	private void CombineMeshes()
	{
		if (Singleton<GamestateManager>.SP.GetCurrentGameMode() != GameMode.LevelEditor && combineMeshes)
		{
			MonoBehaviour.print("Total wood: " + totalVertWood + " Total plastic: " + totalVertPlastic);
			if (allPlasticCurves.Count != 0)
			{
				GameObject obj = new GameObject("combined plastic");
				obj.AddComponent<MeshFilter>();
				obj.AddComponent<MeshRenderer>();
				obj.GetComponent<MeshFilter>().mesh = new Mesh();
				obj.GetComponent<MeshFilter>().mesh.CombineMeshes(allPlasticCurves.ToArray());
				obj.renderer.sharedMaterials = plasticMaterials;
				obj.transform.parent = parentLevelBlocks.transform;
			}
			if (allWood.Count != 0)
			{
				GameObject obj2 = new GameObject("combined wood");
				obj2.AddComponent<MeshFilter>();
				obj2.AddComponent<MeshRenderer>();
				obj2.GetComponent<MeshFilter>().mesh = new Mesh();
				obj2.GetComponent<MeshFilter>().mesh.CombineMeshes(allWood.ToArray());
				obj2.renderer.sharedMaterials = woodMaterials;
				obj2.transform.parent = parentLevelBlocks.transform;
			}
			if (allPillars.Count != 0)
			{
				GameObject obj3 = new GameObject("combined pillars");
				obj3.AddComponent<MeshFilter>();
				obj3.AddComponent<MeshRenderer>();
				obj3.GetComponent<MeshFilter>().mesh = new Mesh();
				obj3.GetComponent<MeshFilter>().mesh.CombineMeshes(allPillars.ToArray());
				obj3.renderer.sharedMaterials = pillarMaterials;
				obj3.transform.parent = parentLevelSupport.transform;
			}
		}
	}
}
