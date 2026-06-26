using System.Collections.Generic;
using UnityEngine;

public class LevelEditorManager : Singleton<LevelEditorManager>
{
	public List<int> buildingBlockHistory = new List<int>();

	public List<GameObject> buildingBlocks;

	public List<GameObject> buildingBlocksLowPoly;

	public GameObject plasticPillar;

	public GameObject woodPillar;

	public GameObject startLine;

	public GameObject startLineDouble;

	public GameObject checkpoint;

	public GameObject plasticCheckpoint;

	public GameObject finishLine;

	public GameObject finishLineDouble;

	public List<int> cat_WoodBlocks;

	public List<int> cat_WoodRamps;

	public List<int> cat_PlasticStraights;

	public List<int> cat_SmallCurves;

	public List<int> cat_SmallCurvesTop;

	public List<int> cat_BigCurves;

	public List<int> cat_BigCurvesTop;

	public List<int> cat_PlasticRamps;

	public List<List<int>> categories;

	public bool isValidating;

	private void Awake()
	{
		categories = new List<List<int>>();
		categories.Add(cat_WoodBlocks);
		categories.Add(cat_WoodRamps);
		categories.Add(cat_PlasticStraights);
		categories.Add(cat_SmallCurves);
		categories.Add(cat_SmallCurvesTop);
		categories.Add(cat_BigCurves);
		categories.Add(cat_BigCurvesTop);
		categories.Add(cat_PlasticRamps);
	}

	public void StartValidation()
	{
		isValidating = true;
	}

	public void StopValidating(bool successful)
	{
	}

	private void Update()
	{
		if (Application.isEditor && Input.GetKeyDown(KeyCode.F5))
		{
			Singleton<LevelEditorFileWriter>.SP.levelName = Singleton<LevelBatchManager>.SP.GetLevelFromCode(Application.loadedLevel).levelName;
			Singleton<LevelEditorFileWriter>.SP.ExportLevel(string.Empty);
		}
	}
}
