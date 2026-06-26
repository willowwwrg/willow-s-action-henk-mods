using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class HenkUtils : MonoBehaviour
{
	private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static bool is64Bit()
	{
		return IntPtr.Size == 8;
	}

	public static T EnumParse<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
	}

	public static bool BoolParse(string boolString)
	{
		if (boolString == "1")
		{
			return true;
		}
		return false;
	}

	public static int IntParse(string intString)
	{
		return int.Parse(intString, CultureInfo.InvariantCulture);
	}

	public static float FloatParse(string floatString)
	{
		return float.Parse(floatString, CultureInfo.InvariantCulture);
	}

	public static float CapFloat(float inputFloat, int multiplier)
	{
		return Mathf.Round(inputFloat * (float)multiplier) / (float)multiplier;
	}

	public static float GetHighscoreFromFile(int levelNum, string medal = "")
	{
		string text = string.Empty;
		if (medal == string.Empty)
		{
			Level levelFromCode = Singleton<LevelBatchManager>.SP.GetLevelFromCode(levelNum);
			string text2 = levelFromCode.levelCode + "-" + levelFromCode.levelName;
			string path = Application.dataPath + "/Resources/../../PBreplays/replay" + text2 + ".txt";
			if (File.Exists(path))
			{
				using StreamReader streamReader = new StreamReader(path);
				text = streamReader.ReadToEnd();
			}
		}
		else
		{
			TextAsset textAsset = (TextAsset)Resources.Load("MedalReplays/lvl" + levelNum + "_" + medal);
			if (textAsset != null)
			{
				text = textAsset.text;
			}
		}
		if (text == string.Empty)
		{
			return 0f;
		}
		using (StringReader stringReader = new StringReader(text))
		{
			stringReader.ReadLine();
			if (IntParse(stringReader.ReadLine()) >= 5)
			{
				return FloatParse(stringReader.ReadLine());
			}
		}
		return 0f;
	}

	public static string ConvertSecondsToTimeString(float seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
	}

	public static void ChangeLevelNameInFile(Level levelObj, string newLevelName)
	{
		string path = string.Concat(Application.dataPath + "/Resources/../../CustomLevels/" + levelObj.guid.ToString() + "/", levelObj.guid.ToString(), ".txt");
		string[] array = File.ReadAllLines(path);
		string text = string.Empty;
		if (array[0] != null)
		{
			array[0] = newLevelName;
		}
		for (int i = 0; i < array.Length; i++)
		{
			text = text + array[i] + "\n";
		}
		File.WriteAllText(path, text);
	}

	public static void GetReplaySnapshots(string fileData, List<SnapshotFrame> framesToFill)
	{
		framesToFill.Clear();
		bool flag = false;
		using StringReader stringReader = new StringReader(fileData);
		string text = stringReader.ReadLine();
		do
		{
			text = stringReader.ReadLine();
			if (text == null || !(text != string.Empty))
			{
				continue;
			}
			if (!flag)
			{
				if (text == "snaps")
				{
					flag = true;
				}
				continue;
			}
			string[] array = text.Split(',');
			SnapshotFrame snapshotFrame = new SnapshotFrame();
			snapshotFrame.position = new Vector3(FloatParse(array[0]), FloatParse(array[1]), FloatParse(array[2]));
			snapshotFrame.velocity = new Vector3(FloatParse(array[3]), FloatParse(array[4]), FloatParse(array[5]));
			snapshotFrame.waypoint = IntParse(array[6]);
			framesToFill.Add(snapshotFrame);
		}
		while (text != null && text != string.Empty);
	}

	public static void ToggleGUICameras(bool state)
	{
		Camera[] componentsInChildren = Singleton<GetRoot>.SP.Get().GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = state;
		}
	}

	public static Transform FindTransformInHierarchy(Transform parent, string name)
	{
		if (parent.name == name)
		{
			return parent;
		}
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	public static void SetLayerRecursively(GameObject obj, string newLayer)
	{
		if (null == obj)
		{
			return;
		}
		obj.layer = LayerMask.NameToLayer(newLayer);
		foreach (Transform item in obj.transform)
		{
			if (!(null == item))
			{
				SetLayerRecursively(item.gameObject, newLayer);
			}
		}
	}

	public static void InitializeMaterialsAccordingToEnvironmentStyle(GameObject obj)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			Camera.main.GetComponent<CameraEffectsManager>().UpdateMaterialForStyle(renderer);
		}
	}

	public static float RoundFloat(float input)
	{
		if (Mathf.Abs(Mathf.Round(input) - input) < 0.0001f)
		{
			input = Mathf.Round(input);
		}
		return input;
	}

	public static bool IsInALevel()
	{
		string loadedLevelName = Application.loadedLevelName;
		if (loadedLevelName != "MenuScene" && loadedLevelName != "LoadingScene" && loadedLevelName != "SplashScene" && !loadedLevelName.Contains("Cutscene") && loadedLevelName != "EmptyScene")
		{
			return true;
		}
		if (Singleton<LevelBatchManager>.SP.currentLevel != null && Singleton<LevelBatchManager>.SP.currentLevel.isSceneLess)
		{
			return true;
		}
		return false;
	}

	public static void BackToMenu()
	{
		if (Singleton<LevelBatchManager>.SP.currentLevel != null || Singleton<Foreman>.SP.currentEnvironmentStyle != LevelStyle.KidsRoom_Menu)
		{
			Singleton<LevelBatchManager>.SP.currentLevel = null;
			Singleton<InputManager>.SP.inputEnabled = false;
			Singleton<PermaGUI>.SP.FadeInOrOut(0f, fadeIn: false, 0f, fadeOverGUI: true);
			KillLevelObjects();
			Singleton<HenkSWNotifications>.SP.FetchAllSessions();
			Singleton<Foreman>.SP.BackToMainMenu();
			Singleton<MutatorManager>.SP.mutatorActive = false;
			ToggleMenuSceneItems(state: true);
			Singleton<PermaGUI>.SP.FadeInOrOut(0.65f, fadeIn: true, 0.05f, fadeOverGUI: true);
		}
	}

	public static void ForceGUIUpdate()
	{
		Singleton<GetRoot>.SP.Get().GetComponent<UIPanel>().SetDirty();
	}

	public static void KillLevelObjects()
	{
		GameObject gameObject = GameObject.Find("instantiatedMainCam");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		foreach (PlatformerCamera playerCamera in Singleton<LocalMultiManager>.SP.playerCameras)
		{
			UnityEngine.Object.Destroy(playerCamera.gameObject);
		}
		Singleton<LocalMultiManager>.SP.playerCameras.Clear();
		GameObject[] localPlayers = Singleton<PlayerManager>.SP.GetLocalPlayers();
		GameObject[] array = localPlayers;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2.GetComponent<PlayerNetworking>().enabled && gameObject2.GetComponent<PhotonView>().isMine)
			{
				PhotonNetwork.Destroy(gameObject2);
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		localPlayers = Singleton<PlayerManager>.SP.GetAllGhosts();
		for (int j = 0; j < localPlayers.Length; j++)
		{
			UnityEngine.Object.Destroy(localPlayers[j]);
		}
		string[] array2 = new string[6] { "LevelBlocks", "LevelSupport", "Entities", "LevelCurve", "StartFinishCheckpoints", "LMPCheckpoints" };
		for (int k = 0; k < array2.Length; k++)
		{
			GameObject gameObject3 = GameObject.Find(array2[k]);
			if (gameObject3 != null)
			{
				UnityEngine.Object.Destroy(gameObject3);
			}
		}
	}

	public static void ToggleMenuSceneItems(bool state)
	{
		GameObject gameObject = GameObject.Find("menuSceneSpecifics");
		if ((bool)gameObject)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				gameObject.transform.GetChild(i).gameObject.SetActive(state);
			}
		}
	}

	public static void EnablePreGameCamera()
	{
		if (IsInALevel())
		{
			Camera.main.GetComponent<PlatformerCamera>().enabled = false;
			Camera.main.GetComponent<PreGameCamera>().enabled = true;
			Camera.main.GetComponent<PreGameCamera>().StartCamera();
			if ((bool)Camera.main.GetComponent<DepthOfFieldScatter>())
			{
				Camera.main.GetComponent<DepthOfFieldScatter>().enabled = false;
			}
		}
	}

	public static void DisablePreGameCamera()
	{
		if (IsInALevel())
		{
			Camera.main.GetComponent<PlatformerCamera>().enabled = true;
			Camera.main.GetComponent<PlatformerCamera>().Start();
			Camera.main.GetComponent<PreGameCamera>().StopCamera();
			Camera.main.GetComponent<PreGameCamera>().enabled = false;
			Camera.main.GetComponent<PlatformerCamera>().SetCameraState(CameraState.Default);
			Camera.main.SendMessage("OnReset", SendMessageOptions.DontRequireReceiver);
			Singleton<PermaGUI>.SP.CancelFade();
			Camera.main.GetComponent<CameraEffectsManager>().UpdateComponents();
		}
	}

	public static void ResetLevel()
	{
		EffectsTrigger[] array = UnityEngine.Object.FindObjectsOfType<EffectsTrigger>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnReset();
		}
		TutorialPlane[] array2 = UnityEngine.Object.FindObjectsOfType<TutorialPlane>();
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].OnReset();
		}
		Pickup[] array3 = UnityEngine.Object.FindObjectsOfType<Pickup>();
		for (int k = 0; k < array3.Length; k++)
		{
			array3[k].OnReset();
		}
		TriggerAnimation[] array4 = UnityEngine.Object.FindObjectsOfType<TriggerAnimation>();
		for (int l = 0; l < array4.Length; l++)
		{
			array4[l].OnReset();
		}
		Collectable[] array5 = UnityEngine.Object.FindObjectsOfType<Collectable>();
		for (int m = 0; m < array5.Length; m++)
		{
			array5[m].OnReset();
		}
		Time.timeScale = 1f;
	}

	public static bool ExecuteEvery(float seconds)
	{
		return Mathf.Repeat(Time.time + Time.deltaTime, seconds) < Mathf.Repeat(Time.time, seconds);
	}

	public static LevelStyle GetLevelStyleFromLevelFile(FileInfo a_file)
	{
		string s = string.Empty;
		string path = a_file.DirectoryName + "/" + a_file.Name;
		string intString = "1";
		if (!File.Exists(path))
		{
			return LevelStyle.KidsRoom_Day;
		}
		using (StreamReader streamReader = new StreamReader(path))
		{
			s = streamReader.ReadToEnd();
		}
		using (StringReader stringReader = new StringReader(s))
		{
			stringReader.ReadLine();
			stringReader.ReadLine();
			intString = stringReader.ReadLine();
		}
		return (LevelStyle)IntParse(intString);
	}

	public static ulong GetPFIDFromWorkshopFile(string guid)
	{
		string[] array = File.ReadAllLines(string.Concat(Application.dataPath + "/Resources/../../CustomLevels/" + guid + "/", guid, ".txt"));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("PFID"))
			{
				return Convert.ToUInt64(array[i].Substring(4));
			}
		}
		return 0uL;
	}

	public static void WritePFIDInWorkshopFile(ulong id, string guid)
	{
		string path = string.Concat(Application.dataPath + "/Resources/../../CustomLevels/" + guid + "/", guid, ".txt");
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("PFID"))
			{
				return;
			}
		}
		string text = string.Empty;
		for (int j = 0; j < array.Length; j++)
		{
			text = text + array[j] + "\n";
		}
		text = text + "PFID" + id + "\n";
		File.WriteAllText(path, text);
	}

	public static void GetNameAndGUIDFromLevelFile(out string a_levelName, out string a_guid, FileInfo a_file)
	{
		string s = string.Empty;
		string path = a_file.DirectoryName + "/" + a_file.Name;
		if (!File.Exists(path))
		{
			a_levelName = string.Empty;
			a_guid = string.Empty;
			return;
		}
		using (StreamReader streamReader = new StreamReader(path))
		{
			s = streamReader.ReadToEnd();
		}
		using StringReader stringReader = new StringReader(s);
		a_levelName = stringReader.ReadLine();
		a_guid = stringReader.ReadLine();
	}

	public static void GetLevelVersionFromLevelFile(out int a_versionNum, FileInfo a_file)
	{
		string s = string.Empty;
		string path = a_file.DirectoryName + "/" + a_file.Name;
		if (!File.Exists(path))
		{
			a_versionNum = 0;
			return;
		}
		using (StreamReader streamReader = new StreamReader(path))
		{
			s = streamReader.ReadToEnd();
		}
		using StringReader stringReader = new StringReader(s);
		stringReader.ReadLine();
		stringReader.ReadLine();
		stringReader.ReadLine();
		string text = stringReader.ReadLine();
		if (text == "C")
		{
			a_versionNum = -1;
		}
		else
		{
			a_versionNum = IntParse(text);
		}
	}

	public static long GetUnixTimestamp()
	{
		return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
	}

	public static bool IsOutside()
	{
		LevelStyle levelStyle = Camera.main.GetComponent<CameraEffectsManager>().levelStyle;
		if (levelStyle == LevelStyle.Island_Water || levelStyle == LevelStyle.Island_Beach || levelStyle == LevelStyle.Island_Jungle || levelStyle == LevelStyle.Island_Night || levelStyle == LevelStyle.City || levelStyle == LevelStyle.Credits_Space || levelStyle == LevelStyle.Winter)
		{
			return true;
		}
		return false;
	}

	public static string GetCredits()
	{
		return (Resources.Load("credits") as TextAsset).text;
	}

	public static bool IsHalloween()
	{
		DateTime now = DateTime.Now;
		if ((now.Month == 10 && now.Day >= 29) || (now.Month == 11 && now.Day <= 14))
		{
			return true;
		}
		return false;
	}

	public static bool IsDev(ulong steamID)
	{
		if (steamID != 76561197984172790L && steamID != 76561198131175064L && steamID != 76561198040533058L)
		{
			_ = 76561197990269809L;
		}
		return true;
	}
}
