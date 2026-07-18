using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCommands : MonoBehaviour
{
	private static bool commandsEnabled;

	private int enableSequence;

	private float enableTimer;

	private float dtCountStartTime;

	private float dtCountStartTime2;

	private float smoothDTUpdateInterval = 1f;

	private float smoothDTUpdateInterval2 = 10f;

	private int dtCounter;

	private int dtCounter2;

	private float totalDT;

	private float totalDT2;

	private float smoothDT;

	private float smoothDT2;

	private UILabel label;

	private UILabel fpsLabel;

	private Camera GUICam;

	private Camera GUICam3D;

	private bool performanceMode;

	private PerformanceGroups performanceGroups;

	private bool skyboxEnabled = true;

	private Material originalSkybox;

	private static readonly Dictionary<string, string> friendlyNames = new Dictionary<string, string>
	{
		{ "EnvironmentStyleContainer", "Background" },
		{ "LevelBlocks", "Level Blocks" },
		{ "LevelBlocks_2", "Level Blocks 2" },
		{ "LevelSupport", "Level Support" },
		{ "StartFinishCheckpoints", "Start/Finish" },
		{ "Entities", "Entities" },
		{ "_GeneratedPerformanceGroups", "Background" }
	};

	private static readonly string[] splitModeLabels = new string[] { "Off", "Every coin", "Every 2 coins", "Every 3 coins", "Every 4 coins", "Every 5 coins" };

	private const string FullGameModeKey = "FullGameMode";

	public static bool IsFullGameMode()
	{
		return PlayerPrefs.GetInt(FullGameModeKey, 0) == 1;
	}

	private void ToggleFullGameMode()
	{
		PlayerPrefs.SetInt(FullGameModeKey, IsFullGameMode() ? 0 : 1);
		PlayerPrefs.Save();
	}

	private void ToggleSkybox()
	{
		skyboxEnabled = !skyboxEnabled;
		if (Camera.main != null)
		{
			if (!skyboxEnabled)
			{
				originalSkybox = RenderSettings.skybox;
				Camera.main.clearFlags = CameraClearFlags.Color;
				Camera.main.backgroundColor = Color.black;
				RenderSettings.skybox = null;
			}
			else
			{
				Camera.main.clearFlags = CameraClearFlags.Skybox;
				RenderSettings.skybox = originalSkybox;
			}
		}
	}

	private void Start()
	{
		label = GameObject.Find("DevCommandsLabel").GetComponent<UILabel>();
		fpsLabel = GameObject.Find("fpsLabel").GetComponent<UILabel>();
		FindGUICam();
		performanceGroups = Object.FindObjectOfType<PerformanceGroups>();
		UpdateText();
		label.enabled = false;
		fpsLabel.enabled = false;
	}

	private void FindGUICam()
	{
		GUICam = Object.FindObjectOfType<UICamera>().camera;
		GUICam3D = GameObject.Find("3DCamera").camera;
	}

	private void OnLevelWasLoaded()
	{
		performanceGroups = Object.FindObjectOfType<PerformanceGroups>();
	}

	private void LevelFileLoaded()
	{
		System.GC.Collect();
		skyboxEnabled = true;
		originalSkybox = RenderSettings.skybox;
		StartCoroutine(GeneratePerformanceGroupsDelayed());
	}

	private void LevelFileLoadedWorkshop()
	{
		LevelFileLoaded();
	}

	private IEnumerator GeneratePerformanceGroupsDelayed()
	{
		yield return new WaitForSeconds(2f);
		PerformanceGroups existing = Object.FindObjectOfType<PerformanceGroups>();
		if (existing != null && existing.gameObject.name == "_GeneratedPerformanceGroups")
		{
			Object.Destroy(existing.gameObject);
		}
		GeneratePerformanceGroups();
	}

	private void GeneratePerformanceGroups()
	{
		GameObject gameObject = new GameObject("_GeneratedPerformanceGroups");
		performanceGroups = gameObject.AddComponent<PerformanceGroups>();
		string[] obj = new string[5] { "LevelBlocks", "LevelBlocks_2", "LevelSupport", "StartFinishCheckpoints", "EnvironmentStyleContainer" };
		List<PerformanceGroup> list = new List<PerformanceGroup>();
		string[] array = obj;
		foreach (string text in array)
		{
			GameObject gameObject2 = GameObject.Find(text);
			if (gameObject2 == null)
			{
				continue;
			}
			Renderer[] componentsInChildren = gameObject2.GetComponentsInChildren<Renderer>();
			if (componentsInChildren.Length == 0)
			{
				continue;
			}
			if (!friendlyNames.TryGetValue(text, out var value))
			{
				value = text;
			}
			PerformanceGroup performanceGroup = new PerformanceGroup();
			performanceGroup.name = value;
			performanceGroup.findByName = string.Empty;
			performanceGroup.gameObjects = new GameObject[1] { gameObject2 };
			performanceGroup.allRenderers = new List<Renderer>(componentsInChildren);
			performanceGroup.enabled = true;
			list.Add(performanceGroup);
		}
		if (Singleton<Foreman>.SP.currentEnvironmentStyle == LevelStyle.Winter)
		{
			Terrain terrain = Object.FindObjectOfType<Terrain>();
			if (terrain != null)
			{
				PerformanceGroup snowGroup = new PerformanceGroup();
				snowGroup.name = "Snow";
				snowGroup.findByName = string.Empty;
				snowGroup.gameObjects = new GameObject[0];
				snowGroup.allRenderers = new List<Renderer>();
				snowGroup.terrain = terrain;
				snowGroup.enabled = true;
				list.Add(snowGroup);
			}
		}
		performanceGroups.groups = list.ToArray();
	}

	public static bool CommandsEnabled()
	{
		return commandsEnabled;
	}

	private void Update()
	{
		if (!commandsEnabled)
		{
			CheckEnable();
			return;
		}
		CheckCommands();
		totalDT += Time.deltaTime;
		dtCounter++;
		if (dtCounter > 0 && Time.time > dtCountStartTime + smoothDTUpdateInterval)
		{
			smoothDT = totalDT / (float)dtCounter;
			dtCounter = 0;
			totalDT = 0f;
			dtCountStartTime = Time.time;
		}
		totalDT2 += Time.deltaTime;
		dtCounter2++;
		if (dtCounter2 > 0 && Time.time > dtCountStartTime2 + smoothDTUpdateInterval2)
		{
			smoothDT2 = totalDT2 / (float)dtCounter2;
			dtCounter2 = 0;
			totalDT2 = 0f;
			dtCountStartTime2 = Time.time;
		}
		string text = "[007FFF]FPS: " + (int)(1f / Time.smoothDeltaTime) + "\nms: " + (Time.smoothDeltaTime * 1000f).ToString("N1");
		string text2 = text;
		text = text2 + "\nFPS/sec: " + (int)(1f / smoothDT) + "\nms/sec: " + (smoothDT * 1000f).ToString("N1");
		text = ((smoothDT2 == 0f) ? (text + "\nWaiting for 10sec..") : (text + "\nms/10sec: " + (smoothDT2 * 1000f).ToString("N1")));
		fpsLabel.text = text;
	}

	private void UpdateText()
	{
		if (!performanceMode)
		{
			string text = "[FF8000]Action Henk developer:[-]\n";
			text += "[007FFF]0.[-] show/hide\n";
			text += "[007FFF]1.[-] toggle GUI\n";
			text += "[007FFF]2.[-] show Rendering Groups\n";
			text += "[007FFF]3.[-] Unlock all skins and characters\n";
			text += "[007FFF]4.[-] Lock all skins and characters\n";
			text += "[007FFF]5.[-] Toggle Console\n";
			text += "[007FFF]6.[-] Next skin\n";
			if (Application.isEditor && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)))
			{
				text += "[007FFF]B.[-] save this replay as [cd7f32]BRONZE[-]\n";
				text += "[007FFF]S.[-] save this replay as [c0c0c0]SILVER[-]\n";
				text += "[007FFF]H.[-] save this replay as [ffd700]GOLD[-]\n";
				text += "[007FFF]P.[-] save this replay as [ff0000]R[-][ff7f00]A[-][ffff00]I[-][00ff00]N[-][0000ff]B[-][4B0082]O[-][8f00ff]W[-]\n";
				text += "[007FFF]C.[-] save this replay as CHALLENGE\n";
			}
			if (Application.isEditor)
			{
				text += "[007FFF]7.[-] Force all to disconnect\n";
			}
			text += "[007FFF]8.[-] Disable all particles\n";
			text += "[007FFF]9.[-] Party Train!\n";
			string splitColor = (Singleton<BonusSplitManager>.SP.SplitFrequency == 0) ? "[FF0000]" : "[00FF00]";
			text += "[007FFF]Q.[-] Cycle bonus splits: " + splitColor + splitModeLabels[Singleton<BonusSplitManager>.SP.SplitFrequency] + "[-]\n";
			string fgmColor = IsFullGameMode() ? "[FF0000]" : "[00FF00]";
			text += "[007FFF]F.[-] Challenge and bonus ghosts: " + fgmColor + (IsFullGameMode() ? "Off" : "On") + "[-]\n";
			label.text = text;
			return;
		}
		string text2 = "[FF8000]Rendering groups:[-]\n";
		text2 += "[007FFF]0.[-] go back\n";
		string text3 = (skyboxEnabled ? "[00FF00]" : "[FF0000]");
		text2 = text2 + text3 + "`.[-] Skybox\n";
		if (performanceGroups != null)
		{
			for (int i = 0; i < performanceGroups.groups.Length; i++)
			{
				PerformanceGroup performanceGroup = performanceGroups.groups[i];
				string text4 = ((!performanceGroup.enabled) ? "[FF0000]" : "[00FF00]");
				string text5 = text2;
				text2 = text5 + text4 + (i + 1) + ".[-] " + performanceGroup.name + "\n";
			}
		}
		label.text = text2;
	}

	private void CheckCommands()
	{
		if (!performanceMode)
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				label.enabled = !label.enabled;
				fpsLabel.enabled = !fpsLabel.enabled;
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				if (GUICam == null || GUICam3D == null)
				{
					FindGUICam();
				}
				if ((bool)GUICam)
				{
					GUICam.enabled = !GUICam.enabled;
				}
				if ((bool)GUICam3D)
				{
					GUICam3D.enabled = GUICam.enabled;
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				performanceMode = true;
				if (performanceGroups == null)
				{
					performanceGroups = Object.FindObjectOfType<PerformanceGroups>();
				}
				if ((bool)performanceGroups)
				{
					performanceGroups.inEditMode = true;
				}
				else
				{
					GeneratePerformanceGroups();
					performanceGroups.inEditMode = true;
				}
				UpdateText();
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Singleton<ActionHenk>.SP.UnlockEverything();
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Singleton<ActionHenk>.SP.LockEverything();
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				Singleton<GetRoot>.SP.Get().GetComponent<OnScreenLog>().enabled = !Singleton<GetRoot>.SP.Get().GetComponent<OnScreenLog>().enabled;
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				GameObject p = Singleton<PlayerManager>.SP.GetPlayer();
				if (p != null) p.GetComponent<PlayerGraphics>().NextSkin();
			}
			if (Input.GetKeyDown(KeyCode.Q))
			{
				Singleton<BonusSplitManager>.SP.CycleSplitFrequency();
				UpdateText();
			}
			if (Input.GetKeyDown(KeyCode.F))
			{
				ToggleFullGameMode();
				UpdateText();
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				Singleton<MultiManager>.SP.ForceAllToDisconnect();
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				ParticleSystem[] array = Object.FindObjectsOfType<ParticleSystem>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(value: false);
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				if (Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard == null)
				{
					Singleton<PermaGUI>.SP.PopUpNotification("Go select ghost->friends first");
				}
				for (int j = 0; j < 250; j++)
				{
					int max = Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard.Length;
					int num = Random.Range(0, max);
					ulong steamID = Singleton<HenkSWLeaderboards>.SP.friendsLeaderboard[num].m_steamIDUser.m_SteamID;
					GameObject[] allGhosts = Singleton<PlayerManager>.SP.GetAllGhosts();
					for (int k = 0; k < allGhosts.Length; k++)
					{
						_ = allGhosts[k].GetComponent<ReplayController>().steamID;
					}
					if (steamID != Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID)
					{
						Singleton<IRCManager>.SP.TrySpawnGhost(steamID, Singleton<HenkSWUserStats>.SP.GetNameBySteamID(steamID));
						AudioController.Play("Toeter");
						break;
					}
				}
			}
			if (Application.isEditor && Singleton<GamestateManager>.SP.IsCurrentState(typeof(State_PostGame)))
			{
				if (Input.GetKeyDown(KeyCode.B))
				{
					Singleton<ReplayRecorder>.SP.SaveReplayLocal(SaveReplayType.Bronze);
				}
				if (Input.GetKeyDown(KeyCode.S))
				{
					Singleton<ReplayRecorder>.SP.SaveReplayLocal(SaveReplayType.Silver);
				}
				if (Input.GetKeyDown(KeyCode.H))
				{
					Singleton<ReplayRecorder>.SP.SaveReplayLocal(SaveReplayType.Gold);
				}
				if (Input.GetKeyDown(KeyCode.P))
				{
					Singleton<ReplayRecorder>.SP.SaveReplayLocal(SaveReplayType.Rainbow);
				}
				if (Input.GetKeyDown(KeyCode.C))
				{
					Singleton<ReplayRecorder>.SP.SaveReplayLocal(SaveReplayType.Challenge);
				}
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			performanceMode = false;
			if ((bool)performanceGroups)
			{
				performanceGroups.inEditMode = false;
			}
			UpdateText();
		}
		if (performanceMode && Input.GetKeyDown(KeyCode.BackQuote))
		{
			ToggleSkybox();
			UpdateText();
		}
		if (performanceMode && performanceGroups != null)
		{
			for (int i = 0; i < performanceGroups.groups.Length; i++)
			{
				if (Input.GetKeyDown((KeyCode)(49 + i)))
				{
					performanceGroups.ToggleGroup(i);
					UpdateText();
					break;
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (Time.timeScale == 1f)
			{
				Time.timeScale = 0.6f;
			}
			else if (Time.timeScale == 0.6f)
			{
				Time.timeScale = 0.3f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
	}

	private string GetColor(bool input)
	{
		if (input)
		{
			return "[11DD11]";
		}
		return "[DD1111]";
	}

	private void CheckEnable()
	{
		if (!SteamManager.Initialized || !HenkUtils.IsDev(Singleton<HenkSWUserStats>.SP.GetSteamID().m_SteamID))
		{
			return;
		}
		if (Input.GetKey(KeyCode.Space) && ((enableSequence == 0 && Input.GetKeyDown(KeyCode.H)) || (enableSequence == 1 && Input.GetKeyDown(KeyCode.E)) || (enableSequence == 2 && Input.GetKeyDown(KeyCode.N)) || (enableSequence == 3 && Input.GetKeyDown(KeyCode.K))))
		{
			enableSequence++;
			enableTimer = 1f;
			if (enableSequence == 4)
			{
				commandsEnabled = true;
				AudioController.Play("Toeter");
				label.enabled = true;
				fpsLabel.enabled = true;
			}
		}
		enableTimer -= Time.deltaTime;
		if (enableTimer < 0f)
		{
			enableSequence = 0;
		}
	}

	public void UpdateQuality()
	{
	}
}
