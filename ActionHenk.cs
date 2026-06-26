using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ActionHenk : Singleton<ActionHenk>
{
	public enum Database
	{
		None,
		RageSquid,
		Steamworks,
		Playstation
	}

	public const bool TOURNAMENT = false;

	public const bool TWITCHHEROES = false;

	public const bool CONFERENCEMODE = false;

	public const bool EVALUATEGHOSTS = false;

	public const bool DEBUGSTEAMWORKS = true;

	public static bool UNLOCKALL;

	private GameObject Root;

	private GameObject unlockMan;

	private GameObject levelEditor;

	public GameObject levels;

	private new GameObject audio;

	private GameObject achievements;

	public string motdString = string.Empty;

	public string CurrentVersion = string.Empty;

	private string LanguageSetting;

	public bool TestScene;

	[NonSerialized]
	public string randomString = "3wrapfd061clympfywryog4tq5yz29";

	private string myID = string.Empty;

	private string playerPrefsKey = "RageSquid_GUID";

	private string myName = string.Empty;

	public bool exitgame;

	public bool fetchedGameSettings;

	public Database database = Database.Steamworks;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		LanguageSetting = PlayerPrefs.GetString("M2H_lastLanguage", "EN");
	}

	private void Start()
	{
		if (audio == null)
		{
			audio = GameObject.FindGameObjectWithTag("AUDIO");
			if (audio == null)
			{
				audio = UnityEngine.Object.Instantiate(Resources.Load("_AUDIO")) as GameObject;
				audio.name = "_AUDIO (Instantiated)";
			}
			UnityEngine.Object.DontDestroyOnLoad(audio);
		}
		if (levels == null)
		{
			levels = GameObject.FindGameObjectWithTag("LEVELS");
			if (levels == null)
			{
				levels = UnityEngine.Object.Instantiate(Resources.Load("_LEVELS")) as GameObject;
				levels.name = "_LEVELS (Instantiated)";
			}
			UnityEngine.Object.DontDestroyOnLoad(levels);
		}
		if (unlockMan == null)
		{
			unlockMan = GameObject.FindGameObjectWithTag("UNLOCKMAN");
			if (unlockMan == null)
			{
				unlockMan = UnityEngine.Object.Instantiate(Resources.Load("_UNLOCKS")) as GameObject;
				unlockMan.name = "_UNLOCKMAN (Instantiated)";
			}
			UnityEngine.Object.DontDestroyOnLoad(unlockMan);
		}
		if (achievements == null)
		{
			achievements = GameObject.FindGameObjectWithTag("ACHIEVEMENTS");
			if (achievements == null)
			{
				achievements = UnityEngine.Object.Instantiate(Resources.Load("_ACHIEVEMENTS")) as GameObject;
				achievements.name = "_ACHIEVEMENTS (Instantiated)";
			}
			UnityEngine.Object.DontDestroyOnLoad(achievements);
		}
		if (levelEditor == null)
		{
			levelEditor = GameObject.FindGameObjectWithTag("EDITOR");
			if (levelEditor == null)
			{
				levelEditor = UnityEngine.Object.Instantiate(Resources.Load("_EDITOR")) as GameObject;
				levelEditor.name = "_EDITOR (Instantiated)";
			}
			UnityEngine.Object.DontDestroyOnLoad(levelEditor);
		}
		if (Root == null)
		{
			Root = GameObject.FindGameObjectWithTag("ROOT");
			if (Root == null)
			{
				Root = UnityEngine.Object.Instantiate(Resources.Load("_ROOT")) as GameObject;
				Root.name = "_ROOT (Instantiated)";
			}
			UnityEngine.Object.DontDestroyOnLoad(Root);
		}
		if (Singleton<GamestateManager>.SP.State == null)
		{
			if (Application.loadedLevelName == "MenuScene")
			{
				Singleton<GamestateManager>.SP.SetState(typeof(State_MainMenu));
				Singleton<InputManager>.SP.inputEnabled = false;
			}
			else
			{
				Singleton<LevelBatchManager>.SP.SetCurrentLevel();
				Singleton<HenkSWLeaderboards>.SP.RefreshCurrentLeaderboard();
				if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Singleplayer)
				{
					UnityEngine.Object.FindObjectOfType<State_PreGame>().OnLevelWasLoadedCampaign();
				}
				else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Multiplayer)
				{
					UnityEngine.Object.FindObjectOfType<State_PreGameMultiplayer>().OnLevelWasLoadedCampaign();
				}
				else if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.Replay)
				{
					UnityEngine.Object.FindObjectOfType<State_ReplayMode>().OnLevelWasLoadedCampaign();
				}
				if (Singleton<GamestateManager>.SP.GetCurrentGameMode() == GameMode.LocalMultiplayer)
				{
					UnityEngine.Object.FindObjectOfType<State_LMP_Pregame>().LevelFileLoadedLocalMultiplayer();
				}
				Singleton<GUIManager>.SP.GetCurrentScreen().GetComponent<GUI_Base>().acceptInput = true;
			}
		}
		CheckGameSettings();
		Singleton<PlayerPrefsManager>.SP.Initialize();
		Singleton<UnlockManager>.SP.Initialize();
		Singleton<InternetManager>.SP.CheckConnection();
		Singleton<HighscoreManager>.SP.InitHighscoreManager();
		Singleton<MutatorManager>.SP.FetchMutator();
		Language.SwitchLanguage(LanguageSetting);
		LocalizeUILabel[] componentsInChildren = Singleton<GetRoot>.SP.Get().GetComponentsInChildren<LocalizeUILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateLabelOnce();
		}
		myID = GenerateGUID();
		if (myID == string.Empty)
		{
			Debug.LogError(base.name + ": INVALID GUID!");
			PlayerPrefs.DeleteKey(playerPrefsKey);
			myID = GenerateGUID();
			if (myID == string.Empty)
			{
				Debug.LogError(base.name + ": INVALID GUID! You have no identity this session.");
			}
		}
		if (Singleton<PlayerPrefsManager>.SP.GetInt("UnlockAll", 0) == 1)
		{
			UNLOCKALL = true;
		}
		PlayerPrefs.SetInt("MOSTRECENTLYCOMPLETEDLEVEL", 0);
		Singleton<AudioManager>.SP.Initialize();
		GameObject gameObject = GameObject.Find("GUI_SplashScene");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (exitgame)
		{
			Application.Quit();
		}
	}

	public void HardResetGame(string fromWhere)
	{
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(array[i]);
		}
		Debug.LogError("HARD RESET: " + fromWhere);
		Application.LoadLevel(0);
	}

	public string GetComputerID()
	{
		return myID;
	}

	public string GetName()
	{
		if (myName != string.Empty)
		{
			return myName;
		}
		string empty = string.Empty;
		if (PlayerPrefs.HasKey("ActionHenkPlayerName"))
		{
			empty = PlayerPrefs.GetString("ActionHenkPlayerName");
		}
		else
		{
			empty = "Action Henk";
			PlayerPrefs.SetString("ActionHenkPlayerName", empty);
			PlayerPrefs.Save();
		}
		return empty;
	}

	public void SetName(string name)
	{
		myName = name;
		PlayerPrefs.SetString("ActionHenkPlayerName", name);
		PlayerPrefs.Save();
		StartCoroutine(SubmitName(name));
	}

	private IEnumerator SubmitName(string name)
	{
		string value = Singleton<PhpMyAdminMan>.SP.Md5Sum(GetComputerID() + GetName() + Singleton<PhpMyAdminMan>.SP.GetSecretKey());
		string submitNameURL = Singleton<PhpMyAdminMan>.SP.GetSubmitNameURL();
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("guid", GetComputerID());
		wWWForm.AddField("name", GetName());
		wWWForm.AddField("Hash", value);
		WWW hs_post = new WWW(submitNameURL, wWWForm);
		yield return hs_post;
		if (hs_post.error != null)
		{
			Debug.LogError("There was an error submitting the name: " + hs_post.error);
		}
	}

	public string GenerateGUID()
	{
		string empty = string.Empty;
		if (PlayerPrefs.HasKey(playerPrefsKey))
		{
			empty = PlayerPrefs.GetString(playerPrefsKey);
		}
		else
		{
			empty = Guid.NewGuid().ToString();
			PlayerPrefs.SetString(playerPrefsKey, empty);
			PlayerPrefs.Save();
		}
		if (ValidateGuid(empty))
		{
			return empty;
		}
		return string.Empty;
	}

	private bool ValidateGuid(string theGuid)
	{
		try
		{
			new Guid(theGuid);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public void ValidateVersion()
	{
		if (PlayerPrefs.HasKey("ActionHenkLastPlayedVersion"))
		{
			if (PlayerPrefs.GetString("ActionHenkLastPlayedVersion") != CurrentVersion)
			{
				Debug.LogError("Your last played version of Action Henk is different from current one, resetting game data.");
				ResetPlayerPrefs();
				TrashReplays();
			}
		}
		else
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.SetString("ActionHenkLastPlayedVersion", CurrentVersion);
			PlayerPrefs.SetString("RageSquid_GUID", myID);
			PlayerPrefs.Save();
		}
	}

	public void TrashReplays()
	{
		FileInfo[] files = new DirectoryInfo(Application.dataPath + "/Resources/../../PBreplays/").GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (fileInfo.Name.StartsWith("replay"))
			{
				MonoBehaviour.print("Deleting: " + fileInfo.Name);
				fileInfo.Delete();
			}
		}
	}

	public void ResetPlayerPrefs()
	{
		string text = GetName();
		int value = PlayerPrefs.GetInt("sfxVolume_i");
		int value2 = PlayerPrefs.GetInt("musicVolume_i");
		PlayerPrefs.DeleteAll();
		SetName(text);
		Language.SwitchLanguage(LanguageSetting);
		LocalizeUILabel[] componentsInChildren = Singleton<GetRoot>.SP.Get().GetComponentsInChildren<LocalizeUILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateLabelOnce();
		}
		PlayerPrefs.SetString("RageSquid_GUID", myID);
		PlayerPrefs.SetInt("musicVolume_i", value2);
		PlayerPrefs.SetInt("sfxVolume_i", value);
		PlayerPrefs.SetString("ActionHenkLastPlayedVersion", CurrentVersion);
		Singleton<UnlockManager>.SP.PlayerPrefsWipe();
		Singleton<LevelBatchManager>.SP.ResetUnlocks();
		PlayerPrefs.SetInt("MOSTRECENTLYCOMPLETEDLEVEL", 0);
		PlayerPrefs.Save();
		Singleton<UnlockManager>.SP.UnlockStandardCharacters();
	}

	public void UnlockEverything()
	{
		Singleton<UnlockManager>.SP.UnlockEverything();
		Debug.Log("Unlocked everything!");
		Singleton<PlayerPrefsManager>.SP.SetInt("UnlockAll", 1);
		UNLOCKALL = true;
	}

	public void LockEverything()
	{
		Singleton<UnlockManager>.SP.LockEverything();
		Debug.Log("Locked everything!");
		Singleton<UnlockManager>.SP.UnlockStandardCharacters();
		Singleton<PlayerPrefsManager>.SP.SetInt("UnlockAll", 0);
		UNLOCKALL = false;
	}

	private void CheckGameSettings()
	{
		if (!fetchedGameSettings)
		{
			StartCoroutine(CheckGameSettingsWebcall());
		}
	}

	private IEnumerator CheckGameSettingsWebcall()
	{
		motdString = "Challenge & Bonus Ghosts + Environment Derender mod by willowRG | Daily Fix by minirop and gyt";
		WWW webcall = new WWW("http://www.ragesquid.com/actionhenkdatabase/gamesettings.txt");
		yield return webcall;
		if (webcall.error != null)
		{
			Debug.LogError("Error retrieving game settings: " + webcall.error);
		}
		else
		{
			string[] array = webcall.text.Split('\n');
			if (array.Length != 0)
			{
				Singleton<HenkSWNotifications>.SP.serverLocation = array[0];
			}
			if (array.Length > 1)
			{
				int.TryParse(array[1], out Singleton<HenkSWNotifications>.SP.minUpdateInterval);
			}
			if (array.Length > 2)
			{
				if (array[2] == "EU")
				{
					Singleton<MultiManager>.SP.serverRegion = 1;
				}
				else if (array[2] == "US")
				{
					Singleton<MultiManager>.SP.serverRegion = 3;
				}
				else if (array[2] == "JP")
				{
					Singleton<MultiManager>.SP.serverRegion = 4;
				}
				else
				{
					Singleton<MultiManager>.SP.serverRegion = 5;
				}
			}
			if (array.Length > 3)
			{
				randomString = array[3];
			}
			if (array.Length > 4)
			{
				motdString = array[4] + "    ***    Challenge & Bonus Ghosts + Environment Derender mod by willowRG | Daily Fix by minirop and gyt";
			}
			else
			{
				motdString = "Challenge & Bonus Ghosts + Environment Derender mod by willowRG | Daily Fix by minirop and gyt";
			}
		}
		fetchedGameSettings = true;
	}
}
