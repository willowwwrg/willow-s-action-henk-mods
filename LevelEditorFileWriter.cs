using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class LevelEditorFileWriter : Singleton<LevelEditorFileWriter>
{
	[HideInInspector]
	public GameObject parentLevelBlocks;

	[HideInInspector]
	public GameObject parentLevelBlocks2;

	[HideInInspector]
	public GameObject parentLevelEntities;

	[HideInInspector]
	public GameObject parentLevelSupport;

	[HideInInspector]
	public GameObject parentLevelCurve;

	public string levelName = string.Empty;

	private void Awake()
	{
		parentLevelBlocks = null;
		parentLevelBlocks2 = null;
		parentLevelEntities = null;
		parentLevelSupport = null;
		parentLevelCurve = null;
	}

	private void Update()
	{
	}

	public bool ExportLevel(string name = "levelName", bool editorLevel = false)
	{
		name = ((!(levelName != string.Empty)) ? "Unknown level name" : levelName);
		bool flag = false;
		parentLevelBlocks = GameObject.Find("LevelBlocks");
		if (parentLevelBlocks == null)
		{
			Debug.LogError("Couldn't find find the level blocks in the scene.");
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_SAVINGLEVEL", "PERMA"));
			return false;
		}
		parentLevelBlocks2 = GameObject.Find("LevelBlocks_2");
		parentLevelEntities = GameObject.Find("Entities");
		parentLevelSupport = GameObject.Find("LevelSupport");
		parentLevelCurve = GameObject.Find("LevelCurve");
		if (parentLevelCurve == null)
		{
			Debug.LogError("Couldn't find find the level curve in the scene.");
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_SAVINGLEVEL", "PERMA"));
			return false;
		}
		string text = name + "\n";
		text = ((!(Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().guid == string.Empty)) ? (text + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().guid.ToString() + "\n") : (text + "null\n"));
		string text2 = text;
		int levelStyle = (int)Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelStyle;
		text = text2 + levelStyle + "\n";
		if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelType == LevelType.Workshop)
		{
			if (Singleton<Workshop>.SP.validating)
			{
				Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelVersion++;
			}
			text = text + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().levelVersion + "\n";
		}
		text += "C\n";
		for (int i = 0; i < parentLevelCurve.transform.childCount; i++)
		{
			GameObject gameObject = parentLevelCurve.transform.GetChild(i).gameObject;
			text = text + gameObject.transform.localPosition.x.ToString(CultureInfo.InvariantCulture) + ",";
			text = text + gameObject.transform.localPosition.y.ToString(CultureInfo.InvariantCulture) + "\n";
		}
		text += "B\n";
		for (int j = 0; j < parentLevelBlocks.transform.childCount; j++)
		{
			GameObject splineObj = parentLevelBlocks.transform.GetChild(j).gameObject;
			text = WriteSplineObjectInToLevelFile(splineObj, text);
			if (text == string.Empty)
			{
				flag = true;
			}
			text += "\n";
		}
		if ((bool)parentLevelBlocks2)
		{
			for (int k = 0; k < parentLevelBlocks2.transform.childCount; k++)
			{
				GameObject splineObj2 = parentLevelBlocks2.transform.GetChild(k).gameObject;
				text = WriteSplineObjectInToLevelFile(splineObj2, text);
				if (text == string.Empty)
				{
					flag = true;
				}
				text += "\n";
			}
		}
		text += "E\n";
		if ((bool)parentLevelEntities)
		{
			for (int l = 0; l < parentLevelEntities.transform.childCount; l++)
			{
				GameObject gameObject2 = parentLevelEntities.transform.GetChild(l).gameObject;
				if (gameObject2.activeSelf)
				{
					text = ((!(gameObject2.GetComponent<SplineFollow>() == null)) ? WriteSplineObjectInToLevelFile(gameObject2, text) : WriteMiscObjectInToLevelFile(gameObject2, text));
					if (text == string.Empty)
					{
						flag = true;
					}
					if (gameObject2.GetComponent<SimpleTrigger>() != null)
					{
						text = text + "," + gameObject2.GetComponent<SimpleTrigger>().target.name;
					}
					text += "\n";
				}
			}
		}
		text += "S\n";
		List<GameObject> list = new List<GameObject>();
		if ((bool)parentLevelSupport)
		{
			SplineFollow[] componentsInChildren = parentLevelSupport.GetComponentsInChildren<SplineFollow>();
			foreach (SplineFollow splineFollow in componentsInChildren)
			{
				if (splineFollow.GetType() == typeof(SplineFollow))
				{
					list.Add(splineFollow.gameObject);
				}
			}
			SplineDeform[] componentsInChildren2 = parentLevelSupport.GetComponentsInChildren<SplineDeform>();
			foreach (SplineDeform splineDeform in componentsInChildren2)
			{
				if (splineDeform.GetType() == typeof(SplineDeform))
				{
					list.Add(splineDeform.gameObject);
				}
			}
			SplineFollowNoRotate[] componentsInChildren3 = parentLevelSupport.GetComponentsInChildren<SplineFollowNoRotate>();
			foreach (SplineFollowNoRotate splineFollowNoRotate in componentsInChildren3)
			{
				if (splineFollowNoRotate.GetType() == typeof(SplineFollowNoRotate))
				{
					list.Add(splineFollowNoRotate.gameObject);
				}
			}
			foreach (GameObject item in list)
			{
				text = WriteSplineObjectInToLevelFile(item, text);
				if (text == string.Empty)
				{
					flag = true;
				}
				text += "\n";
			}
			text += "SP\n";
			PlasticSupport[] componentsInChildren4 = parentLevelSupport.GetComponentsInChildren<PlasticSupport>();
			foreach (PlasticSupport plasticSupport in componentsInChildren4)
			{
				text = WritePlasticSupportInToLevelFile(plasticSupport, text);
				if (text == string.Empty)
				{
					flag = true;
				}
				text += "\n";
			}
			WoodSupport[] componentsInChildren5 = parentLevelSupport.GetComponentsInChildren<WoodSupport>();
			foreach (WoodSupport woodSupport in componentsInChildren5)
			{
				text = WriteWoodSupportInToLevelFile(woodSupport, text);
				if (text == string.Empty)
				{
					flag = true;
				}
				text += "\n";
			}
		}
		else
		{
			text += "SP\n";
		}
		text += "CP\n";
		Vector3 splineOffset = Singleton<CheckpointManager>.SP.Startline.GetComponent<SplineFollow>().splineOffset;
		text += "S,";
		text = text + Mathf.RoundToInt(splineOffset.x) + ",";
		text = text + Mathf.RoundToInt(splineOffset.y) + ",";
		text = text + Mathf.RoundToInt(splineOffset.z) + "\n";
		if ((bool)Singleton<CheckpointManager>.SP.Finishline)
		{
			splineOffset = Singleton<CheckpointManager>.SP.Finishline.GetComponent<SplineFollow>().splineOffset;
			text += "F,";
			text = text + Mathf.RoundToInt(splineOffset.x) + ",";
			text = text + Mathf.RoundToInt(splineOffset.y) + ",";
			text = text + Mathf.RoundToInt(splineOffset.z) + ",";
			text = text + Singleton<CheckpointManager>.SP.Finishline.transform.localScale.x + "\n";
		}
		for (int n = 0; n < Singleton<CheckpointManager>.SP.Checkpoints.Length; n++)
		{
			splineOffset = Singleton<CheckpointManager>.SP.Checkpoints[n].GetComponent<SplineFollow>().splineOffset;
			text = ((!Singleton<CheckpointManager>.SP.Checkpoints[n].name.Contains("Plastic")) ? (text + "C,") : (text + "C1,"));
			text = text + Mathf.RoundToInt(splineOffset.x) + ",";
			text = text + Mathf.RoundToInt(splineOffset.y) + ",";
			text = text + Mathf.RoundToInt(splineOffset.z) + ",";
			text += (int)Singleton<CheckpointManager>.SP.Checkpoints[n].GetComponent<BoxCollider>().size.y;
			text = text + "," + Singleton<CheckpointManager>.SP.Checkpoints[n].transform.localScale.x + "\n";
		}
		LMPCheckpoint lMPCheckpoint = null;
		LMPCheckpoint[] array = Object.FindObjectsOfType<LMPCheckpoint>();
		foreach (LMPCheckpoint lMPCheckpoint2 in array)
		{
			if (lMPCheckpoint2.firstCheckpoint)
			{
				lMPCheckpoint = lMPCheckpoint2;
				break;
			}
		}
		if (lMPCheckpoint != null)
		{
			text += "LMP_CP\n";
			LMPCheckpoint lMPCheckpoint3 = lMPCheckpoint;
			do
			{
				Vector3 splineOffset2 = lMPCheckpoint3.GetComponent<SplineFollow>().splineOffset;
				text = text + Mathf.RoundToInt(splineOffset2.x) + ",";
				text = text + Mathf.RoundToInt(splineOffset2.y) + "\n";
				if ((bool)lMPCheckpoint3.next)
				{
					lMPCheckpoint3 = lMPCheckpoint3.next;
				}
			}
			while (lMPCheckpoint3.next != null);
			Vector3 splineOffset3 = lMPCheckpoint3.GetComponent<SplineFollow>().splineOffset;
			text = text + Mathf.RoundToInt(splineOffset3.x) + ",";
			text = text + Mathf.RoundToInt(splineOffset3.y) + "\n";
		}
		if (flag)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("ERROR_SAVINGLEVEL", "PERMA"));
			return false;
		}
		if (!Singleton<Workshop>.SP.validating)
		{
			Singleton<PermaGUI>.SP.ErrorPopup(Language.Get("LEVELSAVED", "LEVELEDITOR").Replace("{X}", levelName));
		}
		string text3;
		string path;
		if (!editorLevel)
		{
			text3 = Application.dataPath + "/Resources/Levels_Exported/";
			path = text3 + "LEVEL-" + Singleton<LevelBatchManager>.SP.GetCurrentLevel() + ".txt";
		}
		else
		{
			text3 = Application.dataPath + "/Resources/../../CustomLevels/" + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().guid.ToString() + "/";
			path = text3 + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().guid.ToString() + ".txt";
			if (Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().publishedFiledID != 0L)
			{
				text = text + "PFID" + Singleton<LevelBatchManager>.SP.GetCurrentLevelObj().publishedFiledID + "\n";
			}
		}
		if (!Directory.Exists(text3))
		{
			Directory.CreateDirectory(text3);
		}
		StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.Write(text);
		streamWriter.Close();
		return true;
	}

	private string WriteWoodSupportInToLevelFile(WoodSupport woodSupport, string levelString)
	{
		levelString += "W,";
		Vector3 splineOffset = woodSupport.splineOffset;
		levelString = levelString + Mathf.RoundToInt(splineOffset.x) + ",";
		levelString = levelString + Mathf.RoundToInt(splineOffset.y) + ",";
		levelString += Mathf.RoundToInt(splineOffset.z);
		return levelString;
	}

	private string WritePlasticSupportInToLevelFile(PlasticSupport plasticSupport, string levelString)
	{
		levelString += "P,";
		Vector3 splineOffset = plasticSupport.splineOffset;
		levelString = levelString + Mathf.RoundToInt(splineOffset.x) + ",";
		levelString = levelString + Mathf.RoundToInt(splineOffset.y) + ",";
		levelString = levelString + Mathf.RoundToInt(splineOffset.z) + ",";
		levelString = ((!plasticSupport.doublePillar) ? (levelString + "0,") : (levelString + "1,"));
		levelString = levelString + plasticSupport.skipObjects + ",";
		levelString = levelString + plasticSupport.manualHeightOffset + ",";
		levelString = levelString + plasticSupport.manualRotationtOffset + ",";
		levelString = ((!plasticSupport.mirrored) ? (levelString + "0,") : (levelString + "1,"));
		levelString = ((!plasticSupport.noBottom) ? (levelString + "1") : (levelString + "0"));
		return levelString;
	}

	private string WriteMiscObjectInToLevelFile(GameObject obj, string levelString)
	{
		bool flag = false;
		int buildingBlockID = GetBuildingBlockID(obj);
		if (buildingBlockID == -1)
		{
			flag = true;
		}
		levelString = levelString + buildingBlockID + ",";
		levelString += "M,";
		Vector3 vector = default(Vector3);
		vector.x = obj.transform.localPosition.x;
		vector.y = obj.transform.localPosition.y;
		vector.z = obj.transform.localPosition.z;
		vector.x = HenkUtils.RoundFloat(vector.x);
		vector.y = HenkUtils.RoundFloat(vector.y);
		vector.z = HenkUtils.RoundFloat(vector.z);
		levelString = levelString + vector.x.ToString(CultureInfo.InvariantCulture) + ",";
		levelString = levelString + vector.y.ToString(CultureInfo.InvariantCulture) + ",";
		levelString = levelString + vector.z.ToString(CultureInfo.InvariantCulture) + ",";
		Vector3 vector2 = default(Vector3);
		vector2.x = obj.transform.rotation.eulerAngles.x;
		vector2.y = obj.transform.rotation.eulerAngles.y;
		vector2.z = obj.transform.rotation.eulerAngles.z;
		vector2.x = HenkUtils.RoundFloat(vector2.x);
		vector2.y = HenkUtils.RoundFloat(vector2.y);
		vector2.z = HenkUtils.RoundFloat(vector2.z);
		levelString = levelString + vector2.x.ToString(CultureInfo.InvariantCulture) + ",";
		levelString = levelString + vector2.y.ToString(CultureInfo.InvariantCulture) + ",";
		levelString = levelString + vector2.z.ToString(CultureInfo.InvariantCulture) + ",";
		Vector3 vector3 = default(Vector3);
		vector3.x = obj.transform.localScale.x;
		vector3.y = obj.transform.localScale.y;
		vector3.z = obj.transform.localScale.z;
		vector3.x = HenkUtils.RoundFloat(vector3.x);
		vector3.y = HenkUtils.RoundFloat(vector3.y);
		vector3.z = HenkUtils.RoundFloat(vector3.z);
		levelString = levelString + vector3.x.ToString(CultureInfo.InvariantCulture) + ",";
		levelString = levelString + vector3.y.ToString(CultureInfo.InvariantCulture) + ",";
		levelString += vector3.z.ToString(CultureInfo.InvariantCulture);
		if (flag)
		{
			return string.Empty;
		}
		return levelString;
	}

	private string WriteSplineObjectInToLevelFile(GameObject splineObj, string levelString)
	{
		bool flag = false;
		int buildingBlockID = GetBuildingBlockID(splineObj);
		if (buildingBlockID == -1)
		{
			flag = true;
		}
		levelString = levelString + buildingBlockID + ",";
		char splineType = GetSplineType(splineObj);
		if (splineType == '-')
		{
			flag = true;
		}
		levelString = levelString + splineType + ",";
		Vector3 splineOffset = splineObj.GetComponent<SplineObject>().splineOffset;
		levelString = levelString + Mathf.RoundToInt(splineOffset.x) + ",";
		levelString = levelString + Mathf.RoundToInt(splineOffset.y) + ",";
		levelString = levelString + Mathf.RoundToInt(splineOffset.z) + ",";
		bool flag2 = true;
		if ((bool)splineObj.renderer && !splineObj.renderer.enabled)
		{
			flag2 = false;
		}
		levelString += ((!flag2) ? "0," : "1,");
		Vector3 vector = new Vector3(0f, 0f, 0f);
		float num = 0f;
		if (splineType == 'D')
		{
			vector.x = splineObj.GetComponent<SplineDeform>().xScale;
			vector.y = splineObj.GetComponent<SplineDeform>().yScale;
			num = splineObj.GetComponent<SplineDeform>().rotation;
			levelString = levelString + vector.x.ToString(CultureInfo.InvariantCulture) + ",";
			levelString = levelString + vector.y.ToString(CultureInfo.InvariantCulture) + ",";
			levelString += num.ToString(CultureInfo.InvariantCulture);
		}
		else
		{
			vector.x = splineObj.transform.localScale.x;
			vector.y = splineObj.transform.localScale.y;
			vector.z = splineObj.transform.localScale.z;
			vector.x = HenkUtils.RoundFloat(vector.x);
			vector.y = HenkUtils.RoundFloat(vector.y);
			vector.z = HenkUtils.RoundFloat(vector.z);
			levelString = levelString + vector.x.ToString(CultureInfo.InvariantCulture) + ",";
			levelString = levelString + vector.y.ToString(CultureInfo.InvariantCulture) + ",";
			levelString = levelString + vector.z.ToString(CultureInfo.InvariantCulture) + ",";
			Vector3 vector2 = default(Vector3);
			vector2.x = splineObj.transform.rotation.eulerAngles.x;
			vector2.z = splineObj.transform.rotation.eulerAngles.z;
			vector2.x = HenkUtils.RoundFloat(vector2.x);
			vector2.z = HenkUtils.RoundFloat(vector2.z);
			levelString = levelString + vector2.x.ToString(CultureInfo.InvariantCulture) + ",";
			levelString += vector2.z.ToString(CultureInfo.InvariantCulture);
			if (splineType == 'R')
			{
				vector2.y = splineObj.transform.rotation.eulerAngles.y;
				levelString = levelString + "," + vector2.y;
			}
		}
		if (flag)
		{
			return string.Empty;
		}
		return levelString;
	}

	public int GetBuildingBlockID(GameObject levelBlock)
	{
		int buildingBlockIDFromName = GetBuildingBlockIDFromName(levelBlock.name);
		if (buildingBlockIDFromName == -1)
		{
			Debug.LogError("Error trying to save level: No building block ID for block with name: " + levelBlock.name);
		}
		return buildingBlockIDFromName;
	}

	private int GetBuildingBlockIDFromName(string blockName)
	{
		int result = -1;
		for (int i = 0; i < Singleton<LevelEditorManager>.SP.buildingBlocks.Count; i++)
		{
			if ((bool)Singleton<LevelEditorManager>.SP.buildingBlocks[i] && Singleton<LevelEditorManager>.SP.buildingBlocks[i].name == blockName)
			{
				result = i;
			}
		}
		return result;
	}

	private char GetSplineType(GameObject levelBlock)
	{
		if (levelBlock.GetComponent<SplineFollow>() != null)
		{
			return 'F';
		}
		if (levelBlock.GetComponent<SplineDeform>() != null)
		{
			return 'D';
		}
		if (levelBlock.GetComponent<SplineFollowNoRotate>() != null)
		{
			return 'R';
		}
		Debug.LogError("Error trying to save level: No Follow, Deform or FollowNoRotate scripts attached on: " + levelBlock.name);
		return '-';
	}
}
